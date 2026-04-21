using System.Text.Json.Serialization;

namespace Application.Common;

public class Result
{
    public string Status { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Page { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Limit { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Total { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Count { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    [JsonIgnore]
    public int StatusCode { get; set; } = 200;

    public static Result Success(
        object data,
        string? message = null,
        int statusCode = 200,
        int count = 0,
        int page = 0,
        int limit = 0,
        int total = 0)
    {
        return new Result
        {
            Status = "success",
            Message = message,
            Page = page,
            Limit = limit,
            Total = total,
            Count = count,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static Result Error(string message, int statusCode)
    {
        return new Result
        {
            Status = "error",
            Message = message,
            StatusCode = statusCode
        };
    }

    public static Result NoContent()
    {
        return new Result
        {
            StatusCode = 204
        };
    }
}
