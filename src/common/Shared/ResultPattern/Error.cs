namespace Shared.Results;

public record Error
{
    private const char SEPARATOR = '|';

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => 
        new(code, message, ErrorType.Conflict);

    public static Error Iternal(string code, string message) =>
        new(code, message, ErrorType.Internal);

    public static Error None() =>
        new("", "", ErrorType.None);

    public string Serialize()
    {
        return string.Join(SEPARATOR, Code, Message, Type);
    }

    public static Error Deserialize(string serialize)
    {
        var parts = serialize.Split(SEPARATOR);

        if(parts.Length < 3)
        {
            throw new ArgumentException("Invalid serialize format");
        }

        if (Enum.TryParse<ErrorType>(parts[2], out ErrorType errorType) == false) 
        {
            throw new ArgumentException("Invalid error type");
        }

        return new Error(parts[0], parts[1], errorType);
    }
}

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Internal
}
