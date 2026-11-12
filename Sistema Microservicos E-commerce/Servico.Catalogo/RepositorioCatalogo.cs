using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArteSuave.Comum;

namespace ArteSuave.Catalogo
{
    /// <summary>
    /// Dono dos dados de produtos e estoque. Só este serviço lê e grava kimonos.txt e equipamentos.txt.
    /// Todas as operações passam por um lock, porque o servidor atende requisições em paralelo.
    /// </summary>
    public sealed class RepositorioCatalogo
    {
        private readonly object _trava = new object();
        private readonly ArquivoTexto _arqKimonos = new ArquivoTexto("catalogo", "kimonos.txt");
        private readonly ArquivoTexto _arqEquipamentos = new ArquivoTexto("catalogo", "equipamentos.txt");
        private readonly List<Kimono> _kimonos = new List<Kimono>();
        private readonly List<Equipamento> _equipamentos = new List<Equipamento>();

        public RepositorioCatalogo()
        {
            Carregar();
        }

        public Estoque ObterEstoque()
        {
            lock (_trava)
                return new Estoque { Kimonos = _kimonos.Select(Copiar).ToList(), Equipamentos = _equipamentos.Select(Copiar).ToList() };
        }

        public bool NomeEmUso(string nome)
        {
            lock (_trava)
                return BuscarKimono(nome) >= 0 || BuscarEquipamento(nome) >= 0;
        }

        public bool AdicionarKimono(Kimono kimono)
        {
            lock (_trava)
            {
                if (NomeEmUso(kimono.Nome))
                    return false;

                _kimonos.Add(kimono);
                SalvarKimonos();
                return true;
            }
        }

        public bool AdicionarEquipamento(Equipamento equipamento)
        {
            lock (_trava)
            {
                if (NomeEmUso(equipamento.Nome))
                    return false;

                _equipamentos.Add(equipamento);
                SalvarEquipamentos();
                return true;
            }
        }

        public ProdutoEncontrado Buscar(string nome)
        {
            lock (_trava)
            {
                int i = BuscarKimono(nome);
                if (i >= 0)
                    return new ProdutoEncontrado { Tipo = TiposProduto.Kimono, Kimono = Copiar(_kimonos[i]) };

                i = BuscarEquipamento(nome);
                if (i >= 0)
                    return new ProdutoEncontrado { Tipo = TiposProduto.Equipamento, Equipamento = Copiar(_equipamentos[i]) };

                return null;
            }
        }

        public bool RemoverKimono(string nome)
        {
            lock (_trava)
            {
                int i = BuscarKimono(nome);
                if (i < 0) return false;

                _kimonos.RemoveAt(i);
                SalvarKimonos();
                return true;
            }
        }

        public bool RemoverEquipamento(string nome)
        {
            lock (_trava)
            {
                int i = BuscarEquipamento(nome);
                if (i < 0) return false;

                _equipamentos.RemoveAt(i);
                SalvarEquipamentos();
                return true;
            }
        }

        /// <summary>
        /// Ajusta o estoque de um produto. delta negativo = baixa (venda), positivo = estorno.
        /// Retorna null se o produto não existe; lança InvalidOperationException se não houver estoque suficiente.
        /// </summary>
        public MovimentoEstoque Movimentar(string tipo, string nome, int delta)
        {
            lock (_trava)
            {
                if (tipo == TiposProduto.Kimono)
                {
                    int i = BuscarKimono(nome);
                    if (i < 0) return null;

                    Kimono k = _kimonos[i];
                    if (k.Quantidade + delta < 0)
                        throw new InvalidOperationException($"Estoque insuficiente: '{k.Nome}' tem {k.Quantidade} unidade(s).");

                    k.Quantidade += delta;
                    SalvarKimonos();
                    return new MovimentoEstoque { Nome = k.Nome, Categoria = "Kimono", PrecoUnitario = k.Preco, QuantidadeRestante = k.Quantidade };
                }

                if (tipo == TiposProduto.Equipamento)
                {
                    int i = BuscarEquipamento(nome);
                    if (i < 0) return null;

                    Equipamento eq = _equipamentos[i];
                    if (eq.Quantidade + delta < 0)
                        throw new InvalidOperationException($"Estoque insuficiente: '{eq.Nome}' tem {eq.Quantidade} unidade(s).");

                    eq.Quantidade += delta;
                    SalvarEquipamentos();
                    return new MovimentoEstoque { Nome = eq.Nome, Categoria = eq.Categoria, PrecoUnitario = eq.Preco, QuantidadeRestante = eq.Quantidade };
                }

                return null;
            }
        }

        private int BuscarKimono(string nome) =>
            _kimonos.FindIndex(k => k.Nome.Equals(nome ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        private int BuscarEquipamento(string nome) =>
            _equipamentos.FindIndex(e => e.Nome.Equals(nome ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        private static Kimono Copiar(Kimono k) =>
            new Kimono { Nome = k.Nome, Cor = k.Cor, Tamanho = k.Tamanho, Quantidade = k.Quantidade, Preco = k.Preco };

        private static Equipamento Copiar(Equipamento e) =>
            new Equipamento { Nome = e.Nome, Categoria = e.Categoria, Quantidade = e.Quantidade, Preco = e.Preco };

        private void Carregar()
        {
            var inv = CultureInfo.InvariantCulture;

            foreach (string[] c in _arqKimonos.LerRegistros(5))
            {
                if (int.TryParse(c[3], NumberStyles.Integer, inv, out int qtd) &&
                    double.TryParse(c[4], NumberStyles.Float, inv, out double preco))
                    _kimonos.Add(new Kimono { Nome = c[0], Cor = c[1], Tamanho = c[2], Quantidade = qtd, Preco = preco });
            }

            foreach (string[] c in _arqEquipamentos.LerRegistros(4))
            {
                if (int.TryParse(c[2], NumberStyles.Integer, inv, out int qtd) &&
                    double.TryParse(c[3], NumberStyles.Float, inv, out double preco))
                    _equipamentos.Add(new Equipamento { Nome = c[0], Categoria = c[1], Quantidade = qtd, Preco = preco });
            }
        }

        private void SalvarKimonos()
        {
            var inv = CultureInfo.InvariantCulture;
            _arqKimonos.GravarRegistros(_kimonos.Select(k => new[]
            {
                k.Nome, k.Cor, k.Tamanho, k.Quantidade.ToString(inv), k.Preco.ToString(inv)
            }));
        }

        private void SalvarEquipamentos()
        {
            var inv = CultureInfo.InvariantCulture;
            _arqEquipamentos.GravarRegistros(_equipamentos.Select(e => new[]
            {
                e.Nome, e.Categoria, e.Quantidade.ToString(inv), e.Preco.ToString(inv)
            }));
        }
    }
}
