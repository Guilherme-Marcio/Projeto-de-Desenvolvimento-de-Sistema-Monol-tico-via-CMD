using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArteSuave.Comum;

namespace ArteSuave.Vendas
{
    /// <summary>
    /// Serviço de Vendas: registra vendas e é dono do vendas.txt.
    /// Para vender, pede ao Catálogo a baixa do estoque; se não conseguir gravar a venda,
    /// devolve o estoque (compensação), para os dois serviços não ficarem inconsistentes.
    /// </summary>
    internal static class Program
    {
        private static readonly object Trava = new object();
        private static readonly ArquivoTexto Arquivo = new ArquivoTexto("vendas", "vendas.txt");
        private static readonly List<Venda> Vendas = new List<Venda>();
        private static readonly ClienteServico Catalogo = new ClienteServico("Catálogo", Enderecos.Catalogo);

        private static void Main()
        {
            Carregar();

            var servidor = new ServidorHttp("Vendas", Enderecos.Vendas);
            servidor.Mapear("GET", "/vendas", r => { lock (Trava) return Resposta.Ok(Vendas.ToList()); });
            servidor.Mapear("POST", "/vendas", RealizarVenda);
            servidor.Executar();
        }

        private static Resposta RealizarVenda(Requisicao r)
        {
            var pedido = r.LerCorpo<PedidoVenda>();
            if (pedido == null || string.IsNullOrWhiteSpace(pedido.Nome))
                return Resposta.Invalida("Informe o produto a vender.");
            if (pedido.Tipo != TiposProduto.Kimono && pedido.Tipo != TiposProduto.Equipamento)
                return Resposta.Invalida("Tipo deve ser 'kimono' ou 'equipamento'.");
            if (pedido.Quantidade <= 0)
                return Resposta.Invalida("A quantidade deve ser um inteiro maior que zero.");

            // 1. Baixa o estoque no serviço dono dele
            RespostaServico<MovimentoEstoque> baixa = Catalogo.Post<MovimentoEstoque>("estoque/baixa", pedido);
            if (!baixa.Sucesso)
                return Resposta.Falha(baixa.Status, baixa.Erro);

            MovimentoEstoque mov = baixa.Dados;
            var venda = new Venda
            {
                NomeProduto = mov.Nome,
                Categoria = mov.Categoria,
                QuantidadeVendida = pedido.Quantidade,
                PrecoUnitario = mov.PrecoUnitario,
                ValorTotal = mov.PrecoUnitario * pedido.Quantidade
            };

            // 2. Registra a venda; se falhar, estorna o estoque
            try
            {
                lock (Trava)
                {
                    Vendas.Add(venda);
                    try
                    {
                        Salvar();
                    }
                    catch (Exception)
                    {
                        Vendas.RemoveAt(Vendas.Count - 1);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Vendas] falha ao gravar venda, estornando estoque: {ex.Message}");
                Catalogo.Post<MovimentoEstoque>("estoque/estorno", pedido);
                return Resposta.Falha(500, "Não foi possível registrar a venda. O estoque foi devolvido.");
            }

            return Resposta.Criado(venda);
        }

        private static void Carregar()
        {
            var inv = CultureInfo.InvariantCulture;
            foreach (string[] c in Arquivo.LerRegistros(5))
            {
                if (int.TryParse(c[2], NumberStyles.Integer, inv, out int qtd) &&
                    double.TryParse(c[3], NumberStyles.Float, inv, out double unit) &&
                    double.TryParse(c[4], NumberStyles.Float, inv, out double total))
                {
                    Vendas.Add(new Venda { NomeProduto = c[0], Categoria = c[1], QuantidadeVendida = qtd, PrecoUnitario = unit, ValorTotal = total });
                }
            }
        }

        private static void Salvar()
        {
            var inv = CultureInfo.InvariantCulture;
            Arquivo.GravarRegistros(Vendas.Select(v => new[]
            {
                v.NomeProduto, v.Categoria, v.QuantidadeVendida.ToString(inv), v.PrecoUnitario.ToString(inv), v.ValorTotal.ToString(inv)
            }));
        }
    }
}
