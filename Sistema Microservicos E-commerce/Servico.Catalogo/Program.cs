using System;
using ArteSuave.Comum;

namespace ArteSuave.Catalogo
{
    /// <summary>
    /// Serviço de Catálogo/Estoque: cadastro, consulta, exclusão e movimentação de estoque
    /// de kimonos e equipamentos.
    /// </summary>
    internal static class Program
    {
        private static readonly RepositorioCatalogo Repositorio = new RepositorioCatalogo();

        private static void Main()
        {
            var servidor = new ServidorHttp("Catálogo", Enderecos.Catalogo);

            servidor.Mapear("GET", "/estoque", r => Resposta.Ok(Repositorio.ObterEstoque()));
            servidor.Mapear("GET", "/kimonos", r => Resposta.Ok(Repositorio.ObterEstoque().Kimonos));
            servidor.Mapear("GET", "/equipamentos", r => Resposta.Ok(Repositorio.ObterEstoque().Equipamentos));
            servidor.Mapear("POST", "/kimonos", CadastrarKimono);
            servidor.Mapear("POST", "/equipamentos", CadastrarEquipamento);
            servidor.Mapear("GET", "/produtos/{nome}", BuscarProduto);
            servidor.Mapear("DELETE", "/kimonos/{nome}", r => Remover(Repositorio.RemoverKimono(r.Parametros["nome"]), "Kimono"));
            servidor.Mapear("DELETE", "/equipamentos/{nome}", r => Remover(Repositorio.RemoverEquipamento(r.Parametros["nome"]), "Equipamento"));
            servidor.Mapear("POST", "/estoque/baixa", r => Movimentar(r, baixa: true));
            servidor.Mapear("POST", "/estoque/estorno", r => Movimentar(r, baixa: false));

            servidor.Executar();
        }

        private static Resposta CadastrarKimono(Requisicao r)
        {
            var k = r.LerCorpo<Kimono>();
            if (k == null)
                return Resposta.Invalida("Corpo da requisição inválido.");

            k.Nome = k.Nome?.Trim();
            k.Cor = k.Cor?.Trim();
            k.Tamanho = k.Tamanho?.Trim();

            string erro = Validar(k.Nome, k.Quantidade, k.Preco);
            if (erro != null)
                return Resposta.Invalida(erro);

            if (!Repositorio.AdicionarKimono(k))
                return Resposta.Conflito($"Já existe um produto chamado '{k.Nome}'.");

            return Resposta.Criado(k);
        }

        private static Resposta CadastrarEquipamento(Requisicao r)
        {
            var eq = r.LerCorpo<Equipamento>();
            if (eq == null)
                return Resposta.Invalida("Corpo da requisição inválido.");

            eq.Nome = eq.Nome?.Trim();
            eq.Categoria = eq.Categoria?.Trim();

            string erro = Validar(eq.Nome, eq.Quantidade, eq.Preco);
            if (erro != null)
                return Resposta.Invalida(erro);

            if (!Repositorio.AdicionarEquipamento(eq))
                return Resposta.Conflito($"Já existe um produto chamado '{eq.Nome}'.");

            return Resposta.Criado(eq);
        }

        private static Resposta BuscarProduto(Requisicao r)
        {
            ProdutoEncontrado p = Repositorio.Buscar(r.Parametros["nome"]);
            return p == null
                ? Resposta.NaoEncontrado("Nenhum produto cadastrado com esse nome.")
                : Resposta.Ok(p);
        }

        private static Resposta Remover(bool removido, string tipo)
        {
            return removido
                ? Resposta.SemConteudo()
                : Resposta.NaoEncontrado(tipo + " não encontrado no estoque.");
        }

        private static Resposta Movimentar(Requisicao r, bool baixa)
        {
            var pedido = r.LerCorpo<PedidoVenda>();
            if (pedido == null || string.IsNullOrWhiteSpace(pedido.Nome) || pedido.Quantidade <= 0)
                return Resposta.Invalida("Informe Tipo, Nome e uma Quantidade maior que zero.");

            try
            {
                MovimentoEstoque mov = Repositorio.Movimentar(pedido.Tipo, pedido.Nome.Trim(), baixa ? -pedido.Quantidade : pedido.Quantidade);
                return mov == null
                    ? Resposta.NaoEncontrado(pedido.Tipo == TiposProduto.Kimono ? "Kimono não encontrado no estoque!" : "Equipamento não encontrado no estoque!")
                    : Resposta.Ok(mov);
            }
            catch (InvalidOperationException ex)
            {
                return Resposta.Conflito(ex.Message);
            }
        }

        private static string Validar(string nome, int quantidade, double preco)
        {
            if (string.IsNullOrWhiteSpace(nome)) return "O nome do produto é obrigatório.";
            if (quantidade <= 0) return "A quantidade deve ser um inteiro maior que zero.";
            if (preco <= 0) return "O preço deve ser maior que zero.";
            return null;
        }
    }
}
