using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ArteSuave.Comum;

namespace ArteSuave.Gateway
{
    /// <summary>
    /// API Gateway: ponto único de entrada do cliente. Recebe /api/... e repassa
    /// para o serviço dono do recurso, pelo primeiro segmento do caminho.
    ///   /api/kimonos, /api/equipamentos, /api/produtos, /api/estoque -> Catálogo
    ///   /api/vendas                                                  -> Vendas
    ///   /api/relatorios                                              -> Relatórios
    /// </summary>
    internal static class Program
    {
        private const string Prefixo = "/api";

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        private static readonly Dictionary<string, Destino> Rotas = new Dictionary<string, Destino>(StringComparer.OrdinalIgnoreCase);

        private static void Main()
        {
            var catalogo = new Destino("Catálogo", Enderecos.Catalogo);
            var vendas = new Destino("Vendas", Enderecos.Vendas);
            var relatorios = new Destino("Relatórios", Enderecos.Relatorios);

            Rotas["kimonos"] = catalogo;
            Rotas["equipamentos"] = catalogo;
            Rotas["produtos"] = catalogo;
            Rotas["estoque"] = catalogo;
            Rotas["vendas"] = vendas;
            Rotas["relatorios"] = relatorios;

            var servidor = new ServidorHttp("Gateway", Enderecos.Gateway);
            servidor.Mapear("GET", "/api/saude", r => Resposta.Ok(VerificarServicos(catalogo, vendas, relatorios)));
            servidor.MapearPadrao(Encaminhar);

            Console.WriteLine("Encaminhamentos:");
            foreach (var par in Rotas)
                Console.WriteLine($"  {Prefixo}/{par.Key,-13} -> {par.Value.Nome} ({par.Value.Url})");
            Console.WriteLine();

            servidor.Executar();
        }

        private static Resposta Encaminhar(Requisicao r)
        {
            if (!r.Caminho.StartsWith(Prefixo + "/", StringComparison.OrdinalIgnoreCase))
                return Resposta.NaoEncontrado("Use o prefixo /api. Ex: GET /api/estoque");

            string semPrefixo = r.CaminhoComQuery.Substring(Prefixo.Length);
            string recurso = semPrefixo.TrimStart('/').Split('/', '?')[0];

            if (!Rotas.TryGetValue(recurso, out Destino destino))
                return Resposta.NaoEncontrado($"Nenhum serviço atende '/{recurso}'.");

            var mensagem = new HttpRequestMessage(new HttpMethod(r.Metodo), new Uri(new Uri(destino.Url), semPrefixo.TrimStart('/')));
            if (!string.IsNullOrEmpty(r.Corpo))
                mensagem.Content = new StringContent(r.Corpo, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage resposta = Http.SendAsync(mensagem).GetAwaiter().GetResult();
                string corpo = resposta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return new Resposta { Status = (int)resposta.StatusCode, CorpoBruto = string.IsNullOrEmpty(corpo) ? null : corpo };
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                return Resposta.Falha(502, $"Serviço {destino.Nome} indisponível. Ele está rodando?");
            }
        }

        private static List<Saude> VerificarServicos(params Destino[] destinos)
        {
            return destinos
                .Select(d => new Saude { Servico = d.Nome, Status = d.Cliente.EstaNoAr() ? "ok" : "fora do ar" })
                .ToList();
        }

        private sealed class Destino
        {
            public Destino(string nome, string url)
            {
                Nome = nome;
                Url = url;
                Cliente = new ClienteServico(nome, url);
            }

            public string Nome { get; }
            public string Url { get; }
            public ClienteServico Cliente { get; }
        }
    }
}
