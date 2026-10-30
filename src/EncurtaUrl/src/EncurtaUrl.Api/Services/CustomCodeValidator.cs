namespace EncurtaUrl.Api.Services;

public interface ICustomCodeValidator
{
    string[] Validate(string code);
}

public class CustomCodeValidator : ICustomCodeValidator
{
    private const int MinLength = 3;
    private const int MaxLength = 32;

    // Primeiros segmentos de rotas da própria API, que não podem virar código
    private static readonly HashSet<string> ReservedCodes =
        new(["urls", "openapi"], StringComparer.OrdinalIgnoreCase);

    public string[] Validate(string code)
    {
        var errors = new List<string>();

        if (code.Length is < MinLength or > MaxLength)
            errors.Add($"O código deve ter entre {MinLength} e {MaxLength} caracteres.");

        if (!code.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            errors.Add("O código deve conter apenas letras, números, hífen (-) e underscore (_).");

        if (ReservedCodes.Contains(code))
            errors.Add($"O código '{code}' é reservado e não pode ser usado.");

        return [.. errors];
    }
}
