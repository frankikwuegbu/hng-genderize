namespace Application.Common;

public class Result
{
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int Count { get; set; }
    public object? Data { get; set; }

    public static Result Success(object data, int count = 0)
    {
        return new Result
        {
            Status = "success",
            Count = count,
            Data = data
        };
    }

    public static Result Failure(string message, object? data = null)
    {
        return new Result
        {
            Status = "failed",
            Message = message,
            Data = data
        };
    }

    public static Result Error(string message)
    {
        return new Result
        {
            Status = "error",
            Message = message
        };
    }
}
