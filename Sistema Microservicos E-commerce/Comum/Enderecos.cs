using System;

namespace ArteSuave.Comum
{
    /// <summary>
    /// Endereço de cada serviço. Os valores padrão rodam tudo em localhost;
    /// cada um pode ser trocado por variável de ambiente (ex: ARTE_SUAVE_CATALOGO=http://outra-maquina:5001/).
    /// </summary>
    public static class Enderecos
    {
        public static string Gateway => Ler("ARTE_SUAVE_GATEWAY", "http://localhost:5000/");
        public static string Catalogo => Ler("ARTE_SUAVE_CATALOGO", "http://localhost:5001/");
        public static string Vendas => Ler("ARTE_SUAVE_VENDAS", "http://localhost:5002/");
        public static string Relatorios => Ler("ARTE_SUAVE_RELATORIOS", "http://localhost:5003/");

        private static string Ler(string variavel, string padrao)
        {
            string valor = Environment.GetEnvironmentVariable(variavel);
            if (string.IsNullOrWhiteSpace(valor))
                valor = padrao;

            return valor.EndsWith("/") ? valor : valor + "/";
        }
    }
}
