namespace TuneTrail.Api.Schemas.Results;

public class ResultSchema
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public string Code { get; }

    protected ResultSchema(bool isSuccess, string message, string code)
    {
        IsSuccess = isSuccess;
        Message = message;
        Code = code;
    }

    public static ResultSchema Success() => new(true, string.Empty, string.Empty);

    public static ResultSchema Fail(ResultError error) => new(false, error.Message, error.Code);
}

public class ResultSchema<T> : ResultSchema
    where T : class
{
    private readonly T? _value;

    private ResultSchema(T? value, bool isSuccess, string message, string code)
        : base(isSuccess, message, code)
    {
        _value = value;
    }

    public T Value => _value!;

    public static ResultSchema<T> Success(T value) => new(value, true, string.Empty, string.Empty);

    public static new ResultSchema<T> Fail(ResultError error) =>
        new(default, false, error.Message, error.Code);
}
