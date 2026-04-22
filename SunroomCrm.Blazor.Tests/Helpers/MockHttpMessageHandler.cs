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

    public void SetupSseResponse(string url, string[] tokens, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var lines = string.Join("", tokens.Select(t => $"data: {{\"token\":\"{t}\"}}\n\n"));
        lines += "data: [DONE]\n\n";
        SetupRawSseResponse(url, lines, statusCode);
    }

    public void SetupRawSseResponse(string url, string rawBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rawBody));
        _responses[url] = new HttpResponseMessage(statusCode)
        {
            Content = new StreamContent(stream)
        };
        _responses[url].Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");
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
