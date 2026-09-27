using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Monolitico_E_commerce
{
    struct Kimono
    {
        public string Nome;
        public string Cor;
        public string Tamanho; 
        public int Quantidade;
        public double Preco;

        public override string ToString()
        {
            return $"[KIMONO] Nome: {Nome} | Cor: {Cor} | Tamanho: {Tamanho} | Qtd: {Quantidade} | Preço: R$ {Preco:F2}";
        }
    }
    struct Equipamento
    {
        public string Nome;
        public string Categoria; 
        public int Quantidade;
        public double Preco;

        public override string ToString()
        {
            return $"[EQUIPAMENTO] Nome: {Nome} | Cat: {Categoria} | Qtd: {Quantidade} | Preço: R$ {Preco:F2}";
        }
    }
    struct Venda
    {
        public string NomeProduto;
        public string Categoria; 
        public int QuantidadeVendida;
        public double PrecoUnitario;
        public double ValorTotal;

        public override string ToString()
        {
            return $"[VENDA] Produto: {NomeProduto} ({Categoria}) | Qtd: {QuantidadeVendida} | Unit.: R$ {PrecoUnitario:F2} | Total: R$ {ValorTotal:F2}";
        }
    }
    internal class Program
    {
        static List<Kimono> listaKimonos = new List<Kimono>();
        static List<Equipamento> listaEquipamentos = new List<Equipamento>();
        static List<Venda> listaVendas = new List<Venda>();

        string ARQ_KIMONOS = "kimonos.txt";
        string ARQ_EQUIPAMENTOS = "equipamentos.txt";
        string ARQ_VENDAS = "vendas.txt";
    }
}