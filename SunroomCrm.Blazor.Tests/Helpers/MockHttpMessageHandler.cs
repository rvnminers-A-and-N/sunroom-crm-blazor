using System.Net;
using System.Text.Json;

namespace SunroomCrm.Blazor.Tests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses = new();

    public void SetupResponse(string url, object responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _responses[url] = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    public void SetupResponse(string url, HttpStatusCode statusCode)
    {
        _responses[url] = new HttpResponseMessage(statusCode);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.PathAndQuery;

        foreach (var kvp in _responses)
        {
            if (url.StartsWith(kvp.Key))
                return Task.FromResult(kvp.Value);
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
