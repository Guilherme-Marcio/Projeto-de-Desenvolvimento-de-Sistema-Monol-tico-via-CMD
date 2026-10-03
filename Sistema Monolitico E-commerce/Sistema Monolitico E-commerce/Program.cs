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
        static void Main(string[] args)
{
    CarregarTodosOsDados();

    bool executando = true;
    while (executando)
    {
        Console.Clear();
        ExibirMenuPrincipal();

        Console.Write(" Escolha uma opção: ");
        string opcao = Console.ReadLine();

        switch (opcao)
        {
            case "1":
                MenuCadastrarProduto();
                break;
            case "2":
                MenuRealizarVenda();
                break;
            case "3":
                MenuConsultarExcluir();
                break;
            case "4":
                ImprimirEstoque();
                break;
            case "5":
                ImprimirRelatorioVendas();
                break;
            case "0":
                executando = false;
                Console.WriteLine("\nSaindo do sistema... Oss!");
                break;
            default:
                ExibirMensagemErro("Opção inválida! Pressione ENTER para tentar novamente.");
                break;
        }
    }
}
static void ExibirMenuPrincipal()
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
    Console.WriteLine();
}
static void ExibirOpcaoMenu(string codigo, string descricao, ConsoleColor cor, int largura)
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
static string CentralizarTexto(string texto, int largura)
{
    if (texto.Length >= largura) return texto.Substring(0, largura);

    int espacosTotais = largura - texto.Length;
    int esquerda = espacosTotais / 2;
    int direita = espacosTotais - esquerda;
    return new string(' ', esquerda) + texto + new string(' ', direita);
}

static void MenuCadastrarProduto()
{
    Console.Clear();
    ExibirCabecalho("--- CADASTRO DE PRODUTOS ---", ConsoleColor.Green);
    Console.WriteLine("1 - Cadastrar Kimono");
    Console.WriteLine("2 - Cadastrar Equipamento/Acessório");
    Console.WriteLine("0 - Voltar");
    Console.Write("\nOpção: ");
    string op = Console.ReadLine();

    if (op == "1") CadastrarKimono();
    else if (op == "2") CadastrarEquipamento();
}

static void CadastrarKimono()
{
    Console.Clear();
    ExibirCabecalho("--- CADASTRO DE KIMONO ---", ConsoleColor.Green);

    Kimono k;
    Console.Write("Nome / Modelo (ex: Kimono Atama Mundial): ");
    k.Nome = Console.ReadLine().Trim();

    Console.Write("Cor (ex: Azul, Branco, Preto): ");
    k.Cor = Console.ReadLine().Trim();

    Console.Write("Tamanho (ex: A1, A2, A3): ");
    k.Tamanho = Console.ReadLine().Trim();

    k.Quantidade = LerInteiroPositivo("Quantidade em Estoque: ");
    k.Preco = LerDoublePositivo("Preço Unitário (R$): ");

    listaKimonos.Add(k);
    SalvarKimonos(); 

    ExibirMensagemSucesso("Kimono cadastrado e salvo com sucesso!");
}

static void CadastrarEquipamento()
{
    Console.Clear();
    ExibirCabecalho("--- CADASTRO DE EQUIPAMENTO / ACESSÓRIO ---", ConsoleColor.Green);

    Equipamento eq;
    Console.Write("Nome do Produto (ex: Faixa Preta Especial): ");
    eq.Nome = Console.ReadLine().Trim();

    Console.Write("Categoria (ex: Faixa, Rashguard, Protetor Bucal): ");
    eq.Categoria = Console.ReadLine().Trim();

    eq.Quantidade = LerInteiroPositivo("Quantidade em Estoque: ");
    eq.Preco = LerDoublePositivo("Preço Unitário (R$): ");

    listaEquipamentos.Add(eq);
    SalvarEquipamentos(); 

    ExibirMensagemSucesso("Equipamento cadastrado e salvo com sucesso!");
   }

static void MenuRealizarVenda()
{
    Console.Clear();
    ExibirCabecalho("--- REALIZAR VENDA ---", ConsoleColor.Magenta);
    Console.WriteLine("1 - Vender Kimono");
    Console.WriteLine("2 - Vender Equipamento/Acessório");
    Console.WriteLine("0 - Voltar");
    Console.Write("\nOpção: ");
    string op = Console.ReadLine();

    if (op == "1") RealizarVendaKimono();
    else if (op == "2") RealizarVendaEquipamento();
}

static void RealizarVendaKimono()
{
    Console.Clear();
    ExibirCabecalho("--- VENDA DE KIMONO ---", ConsoleColor.Magenta);

    Console.Write("Digite o Nome/Modelo do Kimono: ");
    string nome = Console.ReadLine().Trim();

    int index = listaKimonos.FindIndex(k => k.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));

    if (index == -1)
    {
        ExibirMensagemErro("Kimono não encontrado no estoque!");
        return;
    }

    Kimono k = listaKimonos[index];
    Console.WriteLine($"Estoque atual de '{k.Nome}': {k.Quantidade} unidade(s).");

    int qtdVenda = LerInteiroPositivo("Quantidade a vender: ");

    if (qtdVenda > k.Quantidade)
    {
        ExibirMensagemErro("Estoque insuficiente para realizar esta venda!");
        return;
    }

    k.Quantidade -= qtdVenda;
    listaKimonos[index] = k;
    SalvarKimonos();

    Venda v;
    v.NomeProduto = k.Nome;
    v.Categoria = "Kimono";
    v.QuantidadeVendida = qtdVenda;
    v.PrecoUnitario = k.Preco;
    v.ValorTotal = k.Preco * qtdVenda;

    listaVendas.Add(v);
    SalvarVendas();

    ExibirMensagemSucesso($"Venda realizada! Total: R$ {v.ValorTotal:F2}");
}

static void RealizarVendaEquipamento()
{
    Console.Clear();
    ExibirCabecalho("--- VENDA DE EQUIPAMENTO ---", ConsoleColor.Magenta);

    Console.Write("Digite o Nome do Equipamento: ");
    string nome = Console.ReadLine().Trim();

    int index = listaEquipamentos.FindIndex(eq => eq.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));

    if (index == -1)
    {
        ExibirMensagemErro("Equipamento não encontrado no estoque!");
        return;
    }

    Equipamento eq = listaEquipamentos[index];
    Console.WriteLine($"Estoque atual de '{eq.Nome}': {eq.Quantidade} unidade(s).");

    int qtdVenda = LerInteiroPositivo("Quantidade a vender: ");

    if (qtdVenda > eq.Quantidade)
    {
        ExibirMensagemErro("Estoque insuficiente para realizar esta venda!");
        return;
    }

    eq.Quantidade -= qtdVenda;
    listaEquipamentos[index] = eq;
    SalvarEquipamentos();

    Venda v;
    v.NomeProduto = eq.Nome;
    v.Categoria = eq.Categoria;
    v.QuantidadeVendida = qtdVenda;
    v.PrecoUnitario = eq.Preco;
    v.ValorTotal = eq.Preco * qtdVenda;

    listaVendas.Add(v);
    SalvarVendas();

    ExibirMensagemSucesso($"Venda realizada! Total: R$ {v.ValorTotal:F2}");
}

static void MenuConsultarExcluir()
{
    Console.Clear();
    ExibirCabecalho("--- CONSULTAR / EXCLUIR PRODUTO ---", ConsoleColor.Yellow);
    Console.Write("Digite o nome do produto que deseja pesquisar: ");
    string busca = Console.ReadLine().Trim();

    for (int i = 0; i < listaKimonos.Count; i++)
    {
        if (listaKimonos[i].Nome.Equals(busca, StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[PRODUTO ENCONTRADO]");
            Console.WriteLine(listaKimonos[i].ToString());
            Console.ResetColor();

            Console.Write("\nDeseja EXCLUIR este Kimono? (S/N): ");
            if (Console.ReadLine().Trim().ToUpper() == "S")
            {
                listaKimonos.RemoveAt(i);
                SalvarKimonos();
                ExibirMensagemSucesso("Kimono excluído do estoque com sucesso!");
            }
            return;
        }
    }

    for (int i = 0; i < listaEquipamentos.Count; i++)
    {
        if (listaEquipamentos[i].Nome.Equals(busca, StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[PRODUTO ENCONTRADO]");
            Console.WriteLine(listaEquipamentos[i].ToString());
            Console.ResetColor();

            Console.Write("\nDeseja EXCLUIR este Equipamento? (S/N): ");
            if (Console.ReadLine().Trim().ToUpper() == "S")
            {
                listaEquipamentos.RemoveAt(i);
                SalvarEquipamentos();
                ExibirMensagemSucesso("Equipamento excluído do estoque com sucesso!");
            }
            return;
        }
    }

    ExibirMensagemErro("Nenhum produto cadastrado com esse nome.");
}

static void ImprimirEstoque()
{
    Console.Clear();
    ExibirCabecalho("--- RELATÓRIO DE ESTOQUE COMPLETO ---", ConsoleColor.Blue);

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("=== KIMONOS ===");
    Console.ResetColor();
    if (listaKimonos.Count == 0) Console.WriteLine("Nenhum kimono cadastrado.");
    else
    {
        foreach (var k in listaKimonos)
            Console.WriteLine(k.ToString());
    }

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("\n=== EQUIPAMENTOS / ACESSÓRIOS ===");
    Console.ResetColor();
    if (listaEquipamentos.Count == 0) Console.WriteLine("Nenhum equipamento cadastrado.");
    else
    {
        foreach (var eq in listaEquipamentos)
            Console.WriteLine(eq.ToString());
    }

    Console.WriteLine("\nPressione ENTER para voltar...");
    Console.ReadLine();
 }

static void ImprimirRelatorioVendas()
{
    Console.Clear();
    ExibirCabecalho("--- RELATÓRIO DE VENDAS ---", ConsoleColor.Blue);

    if (listaVendas.Count == 0)
    {
        Console.WriteLine("Nenhuma venda registrada.");
        Console.WriteLine("\nPressione ENTER para voltar...");
        Console.ReadLine();
        return;
    }

    foreach (var v in listaVendas)
        Console.WriteLine(v.ToString());

    // Soma o total vendido de cada categoria, sem repetir categoria na lista
    List<string> categorias = new List<string>();
    List<double> totaisPorCategoria = new List<double>();

    foreach (var v in listaVendas)
    {
        int indice = categorias.IndexOf(v.Categoria);
        if (indice == -1)
        {
            categorias.Add(v.Categoria);
            totaisPorCategoria.Add(v.ValorTotal);
        }
        else
        {
            totaisPorCategoria[indice] += v.ValorTotal;
        }
    }

    Console.WriteLine("\n--- TOTAL POR CATEGORIA ---");
    double totalGeral = 0;
    for (int i = 0; i < categorias.Count; i++)
    {
        Console.WriteLine($"{categorias[i]}: R$ {totaisPorCategoria[i]:F2}");
        totalGeral += totaisPorCategoria[i];
    }

    Console.WriteLine($"\nTOTAL GERAL: R$ {totalGeral:F2}");

    Console.WriteLine("\nPressione ENTER para voltar...");
    Console.ReadLine();
}
 }
}
