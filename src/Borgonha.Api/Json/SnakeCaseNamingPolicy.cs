using System.Text;
using System.Text.Json;

namespace Borgonha.Api.Json;

public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder();

        for (var i = 0; i < name.Length; i++)
        {
            var caractere = name[i];

            if (char.IsUpper(caractere))
            {
                if (i > 0)
                    builder.Append('_');

                builder.Append(char.ToLowerInvariant(caractere));
            }
            else
            {
                builder.Append(caractere);
            }
        }

        return builder.ToString();
    }
}
