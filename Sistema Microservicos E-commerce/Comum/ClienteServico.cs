using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArteSuave.Comum
{
    public sealed class ServicoIndisponivelException : Exception
    {
        public ServicoIndisponivelException(string servico, Exception interna)
            : base($"Serviço {servico} indisponível. Ele está rodando?", interna)
        {
        }
    }

    public sealed class RespostaServico<T>
    {
        public int Status { get; set; }
        public T Dados { get; set; }
        public string Erro { get; set; }
        public bool Sucesso => Status >= 200 && Status < 300;
    }

    /// <summary>Cliente HTTP síncrono usado na comunicação entre serviços e pelo cliente CMD.</summary>
    public sealed class ClienteServico
    {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private readonly string _nomeServico;
        private readonly Uri _base;

        public ClienteServico(string nomeServico, string urlBase)
        {
            _nomeServico = nomeServico;
            _base = new Uri(urlBase);
        }

        public RespostaServico<T> Get<T>(string caminho) => Enviar<T>(HttpMethod.Get, caminho, null);
        public RespostaServico<T> Post<T>(string caminho, object corpo) => Enviar<T>(HttpMethod.Post, caminho, corpo);
        public RespostaServico<T> Delete<T>(string caminho) => Enviar<T>(HttpMethod.Delete, caminho, null);

        public RespostaServico<T> Enviar<T>(HttpMethod metodo, string caminho, object corpo)
        {
            var mensagem = new HttpRequestMessage(metodo, new Uri(_base, caminho.TrimStart('/')));
            if (corpo != null)
                mensagem.Content = new StringContent(Json.Serializar(corpo), Encoding.UTF8, "application/json");

            HttpResponseMessage resposta;
            string texto;
            try
            {
                resposta = Http.SendAsync(mensagem).GetAwaiter().GetResult();
                texto = resposta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                throw new ServicoIndisponivelException(_nomeServico, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ServicoIndisponivelException(_nomeServico, ex);
            }

            var resultado = new RespostaServico<T> { Status = (int)resposta.StatusCode };

            if (resultado.Sucesso)
            {
                resultado.Dados = Json.Desserializar<T>(texto);
            }
            else
            {
                Erro erro = null;
                try { erro = Json.Desserializar<Erro>(texto); } catch (Exception) { }
                resultado.Erro = erro?.Mensagem ?? $"O serviço {_nomeServico} respondeu {(int)resposta.StatusCode}.";
            }

            return resultado;
        }

        public bool EstaNoAr()
        {
            try
            {
                return Get<Saude>("saude").Sucesso;
            }
            catch (ServicoIndisponivelException)
            {
                return false;
            }
        }
    }
}
