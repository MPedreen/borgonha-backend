using System.Text;

namespace Borgonha.Api.Administracao;

/// <summary>
/// Comando <c>dotnet run --project src/Borgonha.Api -- gerar-hash-senha</c>: pede a senha sem mostrá-la
/// na tela e imprime o hash que vai na configuração (Administrador:SenhaHash). A senha não é gravada.
/// </summary>
internal static class GeradorDeHashDeSenha
{
    public const string Comando = "gerar-hash-senha";

    public static int Executar()
    {
        string senha;

        if (Console.IsInputRedirected)
        {
            senha = Console.ReadLine() ?? string.Empty;
        }
        else
        {
            senha = LerSemMostrar("Senha: ");
            if (LerSemMostrar("Repita a senha: ") != senha)
            {
                Console.Error.WriteLine("As senhas não conferem.");
                return 1;
            }
        }

        if (senha.Length == 0)
        {
            Console.Error.WriteLine("A senha não pode ser vazia.");
            return 1;
        }

        Console.WriteLine(HashDeSenha.Gerar(senha));
        return 0;
    }

    private static string LerSemMostrar(string pergunta)
    {
        Console.Write(pergunta);
        var senha = new StringBuilder();

        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return senha.ToString();
            }

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (senha.Length > 0)
                {
                    senha.Length--;
                }
            }
            else if (!char.IsControl(tecla.KeyChar))
            {
                senha.Append(tecla.KeyChar);
            }
        }
    }
}
