namespace Siaed.Application.Common;

public class Result
{
    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList().AsReadOnly();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; }

    public static Result Success() => new(true, []);
    public static Result Failure(string error) => new(false, [error]);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, []) => _value = value;
    private Result(IEnumerable<string> errors) : base(false, errors) => _value = default;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Não é possível acessar o valor de um resultado falho.");

    public new static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error) => new([error]);
    public new static Result<T> Failure(IEnumerable<string> errors) => new(errors);
}
