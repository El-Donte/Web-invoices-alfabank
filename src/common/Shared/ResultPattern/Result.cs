namespace Shared.Results;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public List<Error> Errors { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    protected Result(bool isSuccess, List<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() 
        => new(true, Error.None());

    public static Result Failure(Error error)
        => new(false, error);
}

public class Result<T> : Result
{
    private T? Value { get; }

    private Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    private Result(T? value, bool isSuccess, List<Error> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) 
        => new(value, true, Error.None());

    public static new Result<T> Failure(Error error)
        => new(default, false, error);
    
    public static new Result<T> Failure(List<Error> errors)
        => new(default, false, errors);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(List<Error> errors) => Failure(errors);

    public TResult Match<TResult>(
        Func<T, TResult> success,
        Func<Error, TResult> failre)
    {
        return IsSuccess ? success(Value!) : failre(Error);
    }
}
