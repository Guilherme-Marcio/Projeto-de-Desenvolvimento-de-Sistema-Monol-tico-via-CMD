using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ArteSuave.Comum
{
    public sealed class Requisicao
    {
        public string Metodo { get; internal set; }
        public string Caminho { get; internal set; }
        public string CaminhoComQuery { get; internal set; }
        public string Corpo { get; internal set; }
        public string TipoConteudo { get; internal set; }
        public Dictionary<string, string> Parametros { get; internal set; } = new Dictionary<string, string>();

        public T LerCorpo<T>() where T : class
        {
            try
            {
                return Json.Desserializar<T>(Corpo);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public sealed class Resposta
    {
        public int Status { get; set; }
        public object Corpo { get; set; }

        /// <summary>Corpo já serializado (usado pelo Gateway para repassar a resposta sem reprocessar).</summary>
        public string CorpoBruto { get; set; }

        public static Resposta Ok(object corpo) => new Resposta { Status = 200, Corpo = corpo };
        public static Resposta Criado(object corpo) => new Resposta { Status = 201, Corpo = corpo };
        public static Resposta SemConteudo() => new Resposta { Status = 204 };
        public static Resposta Invalida(string msg) => Falha(400, msg);
        public static Resposta NaoEncontrado(string msg) => Falha(404, msg);
        public static Resposta Conflito(string msg) => Falha(409, msg);
        public static Resposta Indisponivel(string msg) => Falha(503, msg);
        public static Resposta Falha(int status, string msg) => new Resposta { Status = status, Corpo = new Erro { Mensagem = msg } };
    }

    /// <summary>
    /// Servidor HTTP mínimo sobre HttpListener, com roteamento por método + caminho.
    /// Padrões aceitam parâmetros: "/kimonos/{nome}".
    /// Todo serviço ganha automaticamente GET /saude.
    /// </summary>
    public sealed class ServidorHttp
    {
        private readonly string _nomeServico;
        private readonly string _urlBase;
        private readonly HttpListener _listener = new HttpListener();
        private readonly List<Rota> _rotas = new List<Rota>();
        private Func<Requisicao, Resposta> _rotaPadrao;

        public ServidorHttp(string nomeServico, string urlBase)
        {
            _nomeServico = nomeServico;
            _urlBase = urlBase;
            _listener.Prefixes.Add(urlBase);

            Mapear("GET", "/saude", r => Resposta.Ok(new Saude { Servico = nomeServico, Status = "ok" }));
        }

        public void Mapear(string metodo, string padrao, Func<Requisicao, Resposta> acao)
        {
            _rotas.Add(new Rota(metodo, padrao, acao));
        }

        /// <summary>Atende qualquer requisição que não casou com uma rota (usado pelo Gateway).</summary>
        public void MapearPadrao(Func<Requisicao, Resposta> acao)
        {
            _rotaPadrao = acao;
        }

        public void Executar()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Arte Suave | " + _nomeServico;

            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{_nomeServico}] Não foi possível escutar em {_urlBase}: {ex.Message}");
                Console.WriteLine("Verifique se a porta já está em uso por outro processo.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{_nomeServico}] escutando em {_urlBase}");
            Console.ResetColor();
            foreach (var rota in _rotas)
                Console.WriteLine($"  {rota.Metodo,-6} {rota.Padrao}");
            Console.WriteLine();

            while (_listener.IsListening)
            {
                HttpListenerContext contexto = _listener.GetContext();
                ThreadPool.QueueUserWorkItem(_ => Atender(contexto));
            }
        }

        private void Atender(HttpListenerContext contexto)
        {
            Resposta resposta;
            Requisicao requisicao = null;

            try
            {
                requisicao = LerRequisicao(contexto.Request);
                resposta = Rotear(requisicao);
            }
            catch (ServicoIndisponivelException ex)
            {
                resposta = Resposta.Indisponivel(ex.Message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{_nomeServico}] erro inesperado: {ex}");
                Console.ResetColor();
                resposta = Resposta.Falha(500, "Erro interno no serviço " + _nomeServico + ".");
            }

            Escrever(contexto.Response, resposta);
            Registrar(requisicao, resposta.Status);
        }

        private Resposta Rotear(Requisicao requisicao)
        {
            bool caminhoExiste = false;

            foreach (var rota in _rotas)
            {
                if (!rota.Casa(requisicao.Caminho, out var parametros))
                    continue;

                caminhoExiste = true;
                if (!rota.Metodo.Equals(requisicao.Metodo, StringComparison.OrdinalIgnoreCase))
                    continue;

                requisicao.Parametros = parametros;
                return rota.Acao(requisicao);
            }

            if (_rotaPadrao != null)
                return _rotaPadrao(requisicao);

            return caminhoExiste
                ? Resposta.Falha(405, "Método não permitido para " + requisicao.Caminho)
                : Resposta.NaoEncontrado("Rota não encontrada: " + requisicao.Caminho);
        }

        private static Requisicao LerRequisicao(HttpListenerRequest request)
        {
            string bruto = request.RawUrl ?? "/";
            int interrogacao = bruto.IndexOf('?');
            string caminho = interrogacao >= 0 ? bruto.Substring(0, interrogacao) : bruto;

            string corpo = string.Empty;
            if (request.HasEntityBody)
            {
                using (var leitor = new StreamReader(request.InputStream, Encoding.UTF8))
                    corpo = leitor.ReadToEnd();
            }

            return new Requisicao
            {
                Metodo = request.HttpMethod.ToUpperInvariant(),
                Caminho = caminho.Length > 1 ? caminho.TrimEnd('/') : caminho,
                CaminhoComQuery = bruto,
                Corpo = corpo,
                TipoConteudo = request.ContentType
            };
        }

        private static void Escrever(HttpListenerResponse response, Resposta resposta)
        {
            try
            {
                response.StatusCode = resposta.Status;
                string texto = resposta.CorpoBruto ?? (resposta.Corpo == null ? null : Json.Serializar(resposta.Corpo));

                if (texto != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(texto);
                    response.ContentType = "application/json; charset=utf-8";
                    response.ContentLength64 = bytes.Length;
                    response.OutputStream.Write(bytes, 0, bytes.Length);
                }
            }
            finally
            {
                response.Close();
            }
        }

        private void Registrar(Requisicao requisicao, int status)
        {
            if (requisicao == null || requisicao.Caminho == "/saude")
                return;

            ConsoleColor cor = status >= 500 ? ConsoleColor.Red : status >= 400 ? ConsoleColor.Yellow : ConsoleColor.Gray;
            lock (Console.Out)
            {
                Console.ForegroundColor = cor;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} {requisicao.Metodo,-6} {requisicao.CaminhoComQuery} -> {status}");
                Console.ResetColor();
            }
        }

        private sealed class Rota
        {
            private readonly string[] _segmentos;

            public Rota(string metodo, string padrao, Func<Requisicao, Resposta> acao)
            {
                Metodo = metodo;
                Padrao = padrao;
                Acao = acao;
                _segmentos = Dividir(padrao);
            }

            public string Metodo { get; }
            public string Padrao { get; }
            public Func<Requisicao, Resposta> Acao { get; }

            public bool Casa(string caminho, out Dictionary<string, string> parametros)
            {
                parametros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string[] partes = Dividir(caminho);

                if (partes.Length != _segmentos.Length)
                    return false;

                for (int i = 0; i < partes.Length; i++)
                {
                    string esperado = _segmentos[i];
                    string recebido = Uri.UnescapeDataString(partes[i]);

                    if (esperado.StartsWith("{") && esperado.EndsWith("}"))
                        parametros[esperado.Substring(1, esperado.Length - 2)] = recebido;
                    else if (!esperado.Equals(recebido, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            }

            private static string[] Dividir(string caminho)
            {
                return caminho.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            }
        }
    }
}
