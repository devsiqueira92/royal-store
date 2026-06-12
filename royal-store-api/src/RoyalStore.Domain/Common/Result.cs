namespace RoyalStore.Domain.Common;

public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public T Match<T>(Func<T> onSuccess, Func<Error, T> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);
}
