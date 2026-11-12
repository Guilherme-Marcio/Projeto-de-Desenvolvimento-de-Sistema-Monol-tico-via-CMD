using System.Collections.Generic;

namespace ArteSuave.Comum
{
    // Contratos trocados entre os serviços (JSON). Mesmos campos das structs do monólito.

    public class Kimono
    {
        public string Nome { get; set; }
        public string Cor { get; set; }
        public string Tamanho { get; set; }
        public int Quantidade { get; set; }
        public double Preco { get; set; }

        public override string ToString()
        {
            return $"[KIMONO] Nome: {Nome} | Cor: {Cor} | Tamanho: {Tamanho} | Qtd: {Quantidade} | Preço: R$ {Preco:F2}";
        }
    }

    public class Equipamento
    {
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public int Quantidade { get; set; }
        public double Preco { get; set; }

        public override string ToString()
        {
            return $"[EQUIPAMENTO] Nome: {Nome} | Cat: {Categoria} | Qtd: {Quantidade} | Preço: R$ {Preco:F2}";
        }
    }

    public class Venda
    {
        public string NomeProduto { get; set; }
        public string Categoria { get; set; }
        public int QuantidadeVendida { get; set; }
        public double PrecoUnitario { get; set; }
        public double ValorTotal { get; set; }

        public override string ToString()
        {
            return $"[VENDA] Produto: {NomeProduto} ({Categoria}) | Qtd: {QuantidadeVendida} | Unit.: R$ {PrecoUnitario:F2} | Total: R$ {ValorTotal:F2}";
        }
    }

    public static class TiposProduto
    {
        public const string Kimono = "kimono";
        public const string Equipamento = "equipamento";
    }

    /// <summary>Pedido de venda enviado ao serviço de Vendas e repassado ao Catálogo para baixar o estoque.</summary>
    public class PedidoVenda
    {
        public string Tipo { get; set; }
        public string Nome { get; set; }
        public int Quantidade { get; set; }
    }

    /// <summary>Resposta do Catálogo após baixar (ou estornar) o estoque.</summary>
    public class MovimentoEstoque
    {
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public double PrecoUnitario { get; set; }
        public int QuantidadeRestante { get; set; }
    }

    public class Estoque
    {
        public List<Kimono> Kimonos { get; set; } = new List<Kimono>();
        public List<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
    }

    public class ProdutoEncontrado
    {
        public string Tipo { get; set; }
        public Kimono Kimono { get; set; }
        public Equipamento Equipamento { get; set; }
    }

    public class TotalCategoria
    {
        public string Categoria { get; set; }
        public double Total { get; set; }
    }

    public class RelatorioVendas
    {
        public List<Venda> Vendas { get; set; } = new List<Venda>();
        public List<TotalCategoria> TotaisPorCategoria { get; set; } = new List<TotalCategoria>();
        public double TotalGeral { get; set; }
    }

    public class Erro
    {
        public string Mensagem { get; set; }
    }

    public class Saude
    {
        public string Servico { get; set; }
        public string Status { get; set; }
    }
}
