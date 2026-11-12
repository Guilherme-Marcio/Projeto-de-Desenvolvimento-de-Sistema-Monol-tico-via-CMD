using System.Collections.Generic;
using System.Linq;
using ArteSuave.Comum;

namespace ArteSuave.Relatorios
{
    /// <summary>
    /// Serviço de Relatórios: não guarda dados próprios. Consulta o serviço de Vendas
    /// e calcula o total por categoria e o total geral.
    /// </summary>
    internal static class Program
    {
        private static readonly ClienteServico Vendas = new ClienteServico("Vendas", Enderecos.Vendas);

        private static void Main()
        {
            var servidor = new ServidorHttp("Relatórios", Enderecos.Relatorios);
            servidor.Mapear("GET", "/relatorios/vendas", GerarRelatorioVendas);
            servidor.Executar();
        }

        private static Resposta GerarRelatorioVendas(Requisicao r)
        {
            RespostaServico<List<Venda>> resposta = Vendas.Get<List<Venda>>("vendas");
            if (!resposta.Sucesso)
                return Resposta.Falha(resposta.Status, resposta.Erro);

            List<Venda> vendas = resposta.Dados ?? new List<Venda>();

            // Mantém a ordem em que cada categoria apareceu pela primeira vez, como no monólito
            List<TotalCategoria> totais = vendas
                .GroupBy(v => v.Categoria)
                .Select(g => new TotalCategoria { Categoria = g.Key, Total = g.Sum(v => v.ValorTotal) })
                .ToList();

            return Resposta.Ok(new RelatorioVendas
            {
                Vendas = vendas,
                TotaisPorCategoria = totais,
                TotalGeral = totais.Sum(t => t.Total)
            });
        }
    }
}
