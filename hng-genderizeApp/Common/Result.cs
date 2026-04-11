namespace hng_genderizeApp.Common;

public class Result
{
    public string Status { get; set; }
    public object Data { get; set; }
    public string Message { get; set; }

    public static Result Success(object data, string status = "success")
    {
        return new Result
        {
            Status = status,
            Data = data
        };
    }

    public static Result Failure(string message, string status = "failed")
    {
        return new Result
        {
            Status = status,
            Message = message
        };
    }

    public static Result Error(string message, string status = "error")
    {
        return new Result
        {
            Status = status,
            Message = message
        };
    }
}
