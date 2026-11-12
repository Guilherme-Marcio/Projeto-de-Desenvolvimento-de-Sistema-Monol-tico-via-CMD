using System;
using System.Collections.Generic;
using System.Text;
using ArteSuave.Comum;

namespace ArteSuave.Cliente
{
    /// <summary>
    /// Cliente CMD: o mesmo menu do monólito, mas sem lógica de negócio nem arquivos.
    /// Toda operação vira uma chamada HTTP ao API Gateway, que encaminha ao serviço certo.
    /// </summary>
    internal static class Program
    {
        private static readonly ClienteServico Api = new ClienteServico("Gateway", Enderecos.Gateway + "api/");

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Arte Suave | Cliente";

            bool executando = true;
            while (executando)
            {
                LimparTela();
                ExibirMenuPrincipal();
                ExibirStatusServicos();

                Console.Write(" Escolha uma opção: ");
                string opcao = Console.ReadLine();

                try
                {
                    switch (opcao)
                    {
                        case "1": MenuCadastrarProduto(); break;
                        case "2": MenuRealizarVenda(); break;
                        case "3": MenuConsultarExcluir(); break;
                        case "4": ImprimirEstoque(); break;
                        case "5": ImprimirRelatorioVendas(); break;
                        case "0":
                            executando = false;
                            Console.WriteLine("\nSaindo do sistema... Oss!");
                            break;
                        default:
                            ExibirMensagemErro("Opção inválida! Pressione ENTER para tentar novamente.");
                            break;
                    }
                }
                catch (ServicoIndisponivelException ex)
                {
                    ExibirMensagemErro(ex.Message + " Rode o iniciar.bat para subir os serviços.");
                }
            }
        }

        // ---------------------------------------------------------------- menus

        private static void ExibirMenuPrincipal()
        {
            const int largura = 46;
            const string titulo = "ARTE SUAVE STORE | JIU-JITSU GEAR";

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("╔" + new string('═', largura) + "╗");
            Console.WriteLine("║" + CentralizarTexto(titulo, largura) + "║");
            Console.WriteLine("╠" + new string('═', largura) + "╣");
            Console.ResetColor();

            ExibirOpcaoMenu("1", "Cadastrar Produto", ConsoleColor.Cyan, largura);
            ExibirOpcaoMenu("2", "Realizar Venda", ConsoleColor.Cyan, largura);
            ExibirOpcaoMenu("3", "Consultar / Excluir Produto", ConsoleColor.Cyan, largura);
            ExibirOpcaoMenu("4", "Imprimir Estoque", ConsoleColor.Cyan, largura);
            ExibirOpcaoMenu("5", "Imprimir Relatório de Vendas", ConsoleColor.Cyan, largura);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("╠" + new string('═', largura) + "╣");
            Console.ResetColor();

            ExibirOpcaoMenu("0", "Sair", ConsoleColor.Red, largura);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("╚" + new string('═', largura) + "╝");
            Console.ResetColor();
        }

        private static void ExibirStatusServicos()
        {
            Console.Write(" Serviços: ");
            try
            {
                RespostaServico<List<Saude>> saude = Api.Get<List<Saude>>("saude");
                foreach (Saude s in saude.Dados ?? new List<Saude>())
                {
                    Console.ForegroundColor = s.Status == "ok" ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write("● ");
                    Console.ResetColor();
                    Console.Write(s.Servico + "  ");
                }
            }
            catch (ServicoIndisponivelException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("● Gateway fora do ar");
                Console.ResetColor();
            }
            Console.WriteLine("\n");
        }

        private static void MenuCadastrarProduto()
        {
            LimparTela();
            ExibirCabecalho("--- CADASTRO DE PRODUTOS ---", ConsoleColor.Green);
            Console.WriteLine("1 - Cadastrar Kimono");
            Console.WriteLine("2 - Cadastrar Equipamento/Acessório");
            Console.WriteLine("0 - Voltar");
            Console.Write("\nOpção: ");
            string op = Console.ReadLine();

            if (op == "1") CadastrarKimono();
            else if (op == "2") CadastrarEquipamento();
        }

        private static void CadastrarKimono()
        {
            LimparTela();
            ExibirCabecalho("--- CADASTRO DE KIMONO ---", ConsoleColor.Green);

            var k = new Kimono();
            k.Nome = LerTexto("Nome / Modelo (ex: Kimono Atama Mundial): ");
            k.Cor = LerTexto("Cor (ex: Azul, Branco, Preto): ");
            k.Tamanho = LerTexto("Tamanho (ex: A1, A2, A3): ");
            k.Quantidade = LerInteiroPositivo("Quantidade em Estoque: ");
            k.Preco = LerDoublePositivo("Preço Unitário (R$): ");

            var r = Api.Post<Kimono>("kimonos", k);
            if (r.Sucesso) ExibirMensagemSucesso("Kimono cadastrado e salvo com sucesso!");
            else ExibirMensagemErro(r.Erro);
        }

        private static void CadastrarEquipamento()
        {
            LimparTela();
            ExibirCabecalho("--- CADASTRO DE EQUIPAMENTO / ACESSÓRIO ---", ConsoleColor.Green);

            var eq = new Equipamento();
            eq.Nome = LerTexto("Nome do Produto (ex: Faixa Preta Especial): ");
            eq.Categoria = LerTexto("Categoria (ex: Faixa, Rashguard, Protetor Bucal): ");
            eq.Quantidade = LerInteiroPositivo("Quantidade em Estoque: ");
            eq.Preco = LerDoublePositivo("Preço Unitário (R$): ");

            var r = Api.Post<Equipamento>("equipamentos", eq);
            if (r.Sucesso) ExibirMensagemSucesso("Equipamento cadastrado e salvo com sucesso!");
            else ExibirMensagemErro(r.Erro);
        }

        private static void MenuRealizarVenda()
        {
            LimparTela();
            ExibirCabecalho("--- REALIZAR VENDA ---", ConsoleColor.Magenta);
            Console.WriteLine("1 - Vender Kimono");
            Console.WriteLine("2 - Vender Equipamento/Acessório");
            Console.WriteLine("0 - Voltar");
            Console.Write("\nOpção: ");
            string op = Console.ReadLine();

            if (op == "1") RealizarVenda(TiposProduto.Kimono, "--- VENDA DE KIMONO ---", "Digite o Nome/Modelo do Kimono: ");
            else if (op == "2") RealizarVenda(TiposProduto.Equipamento, "--- VENDA DE EQUIPAMENTO ---", "Digite o Nome do Equipamento: ");
        }

        private static void RealizarVenda(string tipo, string titulo, string pergunta)
        {
            LimparTela();
            ExibirCabecalho(titulo, ConsoleColor.Magenta);

            string nome = LerTexto(pergunta);

            // Mostra o estoque atual antes de pedir a quantidade, como no monólito
            var busca = Api.Get<ProdutoEncontrado>("produtos/" + Uri.EscapeDataString(nome));
            if (!busca.Sucesso || busca.Dados.Tipo != tipo)
            {
                ExibirMensagemErro(tipo == TiposProduto.Kimono ? "Kimono não encontrado no estoque!" : "Equipamento não encontrado no estoque!");
                return;
            }

            ProdutoEncontrado p = busca.Dados;
            string nomeReal = p.Kimono?.Nome ?? p.Equipamento.Nome;
            int estoque = p.Kimono?.Quantidade ?? p.Equipamento.Quantidade;
            Console.WriteLine($"Estoque atual de '{nomeReal}': {estoque} unidade(s).");

            int qtd = LerInteiroPositivo("Quantidade a vender: ");

            // A validação final de estoque é do serviço de Catálogo, chamado pelo serviço de Vendas
            var r = Api.Post<Venda>("vendas", new PedidoVenda { Tipo = tipo, Nome = nomeReal, Quantidade = qtd });
            if (r.Sucesso) ExibirMensagemSucesso($"Venda realizada! Total: R$ {r.Dados.ValorTotal:F2}");
            else ExibirMensagemErro(r.Erro);
        }

        private static void MenuConsultarExcluir()
        {
            LimparTela();
            ExibirCabecalho("--- CONSULTAR / EXCLUIR PRODUTO ---", ConsoleColor.Yellow);
            string busca = LerTexto("Digite o nome do produto que deseja pesquisar: ");

            var r = Api.Get<ProdutoEncontrado>("produtos/" + Uri.EscapeDataString(busca));
            if (!r.Sucesso)
            {
                ExibirMensagemErro(r.Erro);
                return;
            }

            bool ehKimono = r.Dados.Tipo == TiposProduto.Kimono;
            string nome = ehKimono ? r.Dados.Kimono.Nome : r.Dados.Equipamento.Nome;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[PRODUTO ENCONTRADO]");
            Console.WriteLine(ehKimono ? r.Dados.Kimono.ToString() : r.Dados.Equipamento.ToString());
            Console.ResetColor();

            Console.Write($"\nDeseja EXCLUIR este {(ehKimono ? "Kimono" : "Equipamento")}? (S/N): ");
            if ((Console.ReadLine() ?? "").Trim().ToUpper() != "S")
                return;

            string rota = (ehKimono ? "kimonos/" : "equipamentos/") + Uri.EscapeDataString(nome);
            var exclusao = Api.Delete<object>(rota);
            if (exclusao.Sucesso) ExibirMensagemSucesso((ehKimono ? "Kimono" : "Equipamento") + " excluído do estoque com sucesso!");
            else ExibirMensagemErro(exclusao.Erro);
        }

        private static void ImprimirEstoque()
        {
            LimparTela();
            ExibirCabecalho("--- RELATÓRIO DE ESTOQUE COMPLETO ---", ConsoleColor.Blue);

            var r = Api.Get<Estoque>("estoque");
            if (!r.Sucesso)
            {
                ExibirMensagemErro(r.Erro);
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("=== KIMONOS ===");
            Console.ResetColor();
            if (r.Dados.Kimonos.Count == 0) Console.WriteLine("Nenhum kimono cadastrado.");
            else foreach (var k in r.Dados.Kimonos) Console.WriteLine(k);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n=== EQUIPAMENTOS / ACESSÓRIOS ===");
            Console.ResetColor();
            if (r.Dados.Equipamentos.Count == 0) Console.WriteLine("Nenhum equipamento cadastrado.");
            else foreach (var eq in r.Dados.Equipamentos) Console.WriteLine(eq);

            Console.WriteLine("\nPressione ENTER para voltar...");
            Console.ReadLine();
        }

        private static void ImprimirRelatorioVendas()
        {
            LimparTela();
            ExibirCabecalho("--- RELATÓRIO DE VENDAS ---", ConsoleColor.Blue);

            var r = Api.Get<RelatorioVendas>("relatorios/vendas");
            if (!r.Sucesso)
            {
                ExibirMensagemErro(r.Erro);
                return;
            }

            if (r.Dados.Vendas.Count == 0)
            {
                Console.WriteLine("Nenhuma venda registrada.");
            }
            else
            {
                foreach (var v in r.Dados.Vendas)
                    Console.WriteLine(v);

                Console.WriteLine("\n--- TOTAL POR CATEGORIA ---");
                foreach (var t in r.Dados.TotaisPorCategoria)
                    Console.WriteLine($"{t.Categoria}: R$ {t.Total:F2}");

                Console.WriteLine($"\nTOTAL GERAL: R$ {r.Dados.TotalGeral:F2}");
            }

            Console.WriteLine("\nPressione ENTER para voltar...");
            Console.ReadLine();
        }

        // ---------------------------------------------------------------- utilitários de console

        // Console.Clear falha quando a saída está redirecionada (ex: testes automatizados)
        private static void LimparTela()
        {
            if (!Console.IsOutputRedirected)
                Console.Clear();
        }

        private static void ExibirOpcaoMenu(string codigo, string descricao, ConsoleColor cor, int largura)
        {
            string linha = $" [{codigo}] {descricao}";

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("║");
            Console.ForegroundColor = cor;
            Console.Write(linha.PadRight(largura));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("║");
            Console.ResetColor();
        }

        private static string CentralizarTexto(string texto, int largura)
        {
            if (texto.Length >= largura) return texto.Substring(0, largura);

            int espacosTotais = largura - texto.Length;
            int esquerda = espacosTotais / 2;
            int direita = espacosTotais - esquerda;
            return new string(' ', esquerda) + texto + new string(' ', direita);
        }

        private static void ExibirCabecalho(string titulo, ConsoleColor cor)
        {
            Console.ForegroundColor = cor;
            Console.WriteLine(titulo);
            Console.WriteLine(new string('-', titulo.Length));
            Console.ResetColor();
            Console.WriteLine();
        }

        // Repete a pergunta até o usuário digitar um texto não vazio
        private static string LerTexto(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string valor = (Console.ReadLine() ?? "").Trim();
                if (valor.Length > 0)
                    return valor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Campo obrigatório! Digite um valor.");
                Console.ResetColor();
            }
        }

        // Repete a pergunta até o usuário digitar um número inteiro maior que zero
        private static int LerInteiroPositivo(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                if (int.TryParse(Console.ReadLine(), out int valor) && valor > 0)
                    return valor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Valor inválido! Digite um número inteiro maior que zero.");
                Console.ResetColor();
            }
        }

        // Repete a pergunta até o usuário digitar um número maior que zero (aceita vírgula)
        private static double LerDoublePositivo(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                if (double.TryParse(Console.ReadLine(), out double valor) && valor > 0)
                    return valor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Valor inválido! Digite um número maior que zero (ex: 199,90).");
                Console.ResetColor();
            }
        }

        private static void ExibirMensagemErro(string mensagem)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[ERRO] " + mensagem);
            Console.ResetColor();
            Console.WriteLine("Pressione ENTER para continuar...");
            Console.ReadLine();
        }

        private static void ExibirMensagemSucesso(string mensagem)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[OK] " + mensagem);
            Console.ResetColor();
            Console.WriteLine("Pressione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}
