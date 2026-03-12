namespace MundialCorporativo.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    public static Result Success() => new(true);

    public static Result Failure(string errorMessage, string? errorCode = null)
        => new(false, errorMessage, errorCode);
}

public class Result<T> : Result
{
    private Result(T value) : base(true)
    {
        Value = value;
    }

    private Result(string errorMessage, string? errorCode = null)
        : base(false, errorMessage, errorCode)
    {
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(string errorMessage, string? errorCode = null)
        => new(errorMessage, errorCode);
}
