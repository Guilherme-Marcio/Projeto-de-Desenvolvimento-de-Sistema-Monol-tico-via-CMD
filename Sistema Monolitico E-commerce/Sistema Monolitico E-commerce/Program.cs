using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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

        private static readonly string ARQ_KIMONOS = "kimonos.txt";
        private static readonly string ARQ_EQUIPAMENTOS = "equipamentos.txt";
        private static readonly string ARQ_VENDAS = "vendas.txt";

        static void SalvarKimonos()
        {
            var linhas = new List<string>();

            foreach (Kimono kimono in listaKimonos)
            {
                linhas.Add(string.Join("|",
                    LimparCampo(kimono.Nome),
                    LimparCampo(kimono.Cor),
                    LimparCampo(kimono.Tamanho),
                    kimono.Quantidade.ToString(CultureInfo.InvariantCulture),
                    kimono.Preco.ToString(CultureInfo.InvariantCulture)));
            }

            File.WriteAllLines(ARQ_KIMONOS, linhas);
        }

        static void CarregarKimonos()
        {
            listaKimonos.Clear();

            if (!File.Exists(ARQ_KIMONOS))
                return;

            foreach (string linha in File.ReadAllLines(ARQ_KIMONOS))
            {
                string[] campos = linha.Split('|');

                if (campos.Length != 5)
                    continue;

                if (!int.TryParse(campos[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantidade))
                    continue;

                if (!double.TryParse(campos[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double preco))
                    continue;

                listaKimonos.Add(new Kimono
                {
                    Nome = campos[0],
                    Cor = campos[1],
                    Tamanho = campos[2],
                    Quantidade = quantidade,
                    Preco = preco
                });
            }
        }

        static void SalvarEquipamentos()
        {
            var linhas = new List<string>();

            foreach (Equipamento equipamento in listaEquipamentos)
            {
                linhas.Add(string.Join("|",
                    LimparCampo(equipamento.Nome),
                    LimparCampo(equipamento.Categoria),
                    equipamento.Quantidade.ToString(CultureInfo.InvariantCulture),
                    equipamento.Preco.ToString(CultureInfo.InvariantCulture)));
            }

            File.WriteAllLines(ARQ_EQUIPAMENTOS, linhas);
        }

        static void CarregarEquipamentos()
        {
            listaEquipamentos.Clear();

            if (!File.Exists(ARQ_EQUIPAMENTOS))
                return;

            foreach (string linha in File.ReadAllLines(ARQ_EQUIPAMENTOS))
            {
                string[] campos = linha.Split('|');

                if (campos.Length != 4)
                    continue;

                if (!int.TryParse(campos[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantidade))
                    continue;

                if (!double.TryParse(campos[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double preco))
                    continue;

                listaEquipamentos.Add(new Equipamento
                {
                    Nome = campos[0],
                    Categoria = campos[1],
                    Quantidade = quantidade,
                    Preco = preco
                });
            }
        }

        static void SalvarVendas()
        {
            var linhas = new List<string>();

            foreach (Venda venda in listaVendas)
            {
                linhas.Add(string.Join("|",
                    LimparCampo(venda.NomeProduto),
                    LimparCampo(venda.Categoria),
                    venda.QuantidadeVendida.ToString(CultureInfo.InvariantCulture),
                    venda.PrecoUnitario.ToString(CultureInfo.InvariantCulture),
                    venda.ValorTotal.ToString(CultureInfo.InvariantCulture)));
            }

            File.WriteAllLines(ARQ_VENDAS, linhas);
        }

        static void CarregarVendas()
        {
            listaVendas.Clear();

            if (!File.Exists(ARQ_VENDAS))
                return;

            foreach (string linha in File.ReadAllLines(ARQ_VENDAS))
            {
                string[] campos = linha.Split('|');

                if (campos.Length != 5)
                    continue;

                if (!int.TryParse(campos[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantidadeVendida))
                    continue;

                if (!double.TryParse(campos[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double precoUnitario))
                    continue;

                if (!double.TryParse(campos[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double valorTotal))
                    continue;

                listaVendas.Add(new Venda
                {
                    NomeProduto = campos[0],
                    Categoria = campos[1],
                    QuantidadeVendida = quantidadeVendida,
                    PrecoUnitario = precoUnitario,
                    ValorTotal = valorTotal
                });
            }
        }
        
        static string LimparCampo(string valor)
        {
            return (valor ?? string.Empty)
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        static void CarregarTodosOsDados()
        {
            CarregarKimonos();
            CarregarEquipamentos();
            CarregarVendas();
        }
    }
}
