using System;
using System.Collections.Generic;
using System.IO;

namespace ArteSuave.Comum
{
    /// <summary>
    /// Persistência em arquivo texto separado por "|", no mesmo formato do monólito.
    /// Cada serviço grava na sua própria pasta (dados/&lt;servico&gt;), nenhum lê o arquivo do outro.
    /// </summary>
    public sealed class ArquivoTexto
    {
        private readonly string _caminho;

        public ArquivoTexto(string servico, string nomeArquivo)
        {
            string pasta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados", servico);
            Directory.CreateDirectory(pasta);
            _caminho = Path.Combine(pasta, nomeArquivo);
        }

        public string Caminho => _caminho;

        public IEnumerable<string[]> LerRegistros(int quantidadeCampos)
        {
            if (!File.Exists(_caminho))
                yield break;

            foreach (string linha in File.ReadAllLines(_caminho))
            {
                string[] campos = linha.Split('|');
                if (campos.Length == quantidadeCampos)
                    yield return campos;
            }
        }

        public void GravarRegistros(IEnumerable<string[]> registros)
        {
            var linhas = new List<string>();
            foreach (string[] campos in registros)
            {
                for (int i = 0; i < campos.Length; i++)
                    campos[i] = LimparCampo(campos[i]);

                linhas.Add(string.Join("|", campos));
            }

            // Grava num arquivo temporário e troca, para não corromper os dados se o processo cair no meio
            string temporario = _caminho + ".tmp";
            File.WriteAllLines(temporario, linhas);
            if (File.Exists(_caminho))
                File.Replace(temporario, _caminho, null);
            else
                File.Move(temporario, _caminho);
        }

        public static string LimparCampo(string valor)
        {
            return (valor ?? string.Empty)
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
}
