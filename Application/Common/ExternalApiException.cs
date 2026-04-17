namespace Application.Common;

public class ExternalApiException : Exception
{
    public ExternalApiException(string externalApi)
        : base($"{externalApi} returned an invalid response")
    {
        ExternalApi = externalApi;
    }

    public string ExternalApi { get; }
}
