using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.AI;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiAiServiceTests
{
    private static (ApiAiService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiAiService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task SummarizeAsync_ReturnsResponse()
    {
        var (service, handler) = CreateService();
        var expected = new SummarizeResponse { Summary = "Brief summary of the text." };
        handler.SetupResponse("/api/ai/summarize", expected);

        var result = await service.SummarizeAsync(new SummarizeRequest { Text = "Long text here" });

        result.Summary.Should().Be("Brief summary of the text.");
    }

    [Fact]
    public async Task SmartSearchAsync_ReturnsResponse()
    {
        var (service, handler) = CreateService();
        var expected = new SmartSearchResponse
        {
            Interpretation = "Searching for 'test'",
            Contacts = new(),
            Activities = new()
        };
        handler.SetupResponse("/api/ai/search", expected);

        var result = await service.SmartSearchAsync(new SmartSearchRequest { Query = "test" });

        result.Interpretation.Should().Contain("test");
    }

    [Fact]
    public async Task GetDealInsightsAsync_ReturnsInsights()
    {
        var (service, handler) = CreateService();
        var expected = new List<DealInsightDto>
        {
            new() { Id = 1, Insight = "Deal is progressing well." }
        };
        handler.SetupResponse("/api/deals/1/insights", expected);

        var result = await service.GetDealInsightsAsync(1);

        result.Should().ContainSingle().Which.Insight.Should().Contain("progressing");
    }

    [Fact]
    public async Task GenerateDealInsightAsync_ReturnsNewInsight()
    {
        var (service, handler) = CreateService();
        var expected = new DealInsightDto { Id = 1, Insight = "Generated insight." };
        handler.SetupResponse("/api/deals/1/insights", expected);

        var result = await service.GenerateDealInsightAsync(1);

        result.Insight.Should().Be("Generated insight.");
    }

    [Fact]
    public async Task SummarizeStreamAsync_YieldsTokens()
    {
        var (service, handler) = CreateService();
        handler.SetupSseResponse("/api/ai/summarize/stream", ["Hello", " world"]);

        var tokens = new List<string>();
        await foreach (var token in service.SummarizeStreamAsync(new SummarizeRequest { Text = "test" }))
        {
            tokens.Add(token);
        }

        tokens.Should().Equal("Hello", " world");
    }

    [Fact]
    public async Task SmartSearchStreamAsync_YieldsTokens()
    {
        var (service, handler) = CreateService();
        handler.SetupSseResponse("/api/ai/search/stream", ["Found", " results"]);

        var tokens = new List<string>();
        await foreach (var token in service.SmartSearchStreamAsync("test query"))
        {
            tokens.Add(token);
        }

        tokens.Should().Equal("Found", " results");
    }

    [Fact]
    public async Task DealInsightsStreamAsync_YieldsTokens()
    {
        var (service, handler) = CreateService();
        handler.SetupSseResponse("/api/ai/deal-insights/1/stream", ["Deal", " insight"]);

        var tokens = new List<string>();
        await foreach (var token in service.DealInsightsStreamAsync(1))
        {
            tokens.Add(token);
        }

        tokens.Should().Equal("Deal", " insight");
    }

    [Fact]
    public async Task SummarizeStreamAsync_SkipsInvalidJsonAndEmptyTokens()
    {
        var (service, handler) = CreateService();
        // Include: invalid JSON, empty token, valid token, then DONE
        var raw = "data: not-json\n\n" +
                  "data: {\"token\":\"\"}\n\n" +
                  "data: {\"token\":\"valid\"}\n\n" +
                  "data: [DONE]\n\n";
        handler.SetupRawSseResponse("/api/ai/summarize/stream", raw);

        var tokens = new List<string>();
        await foreach (var token in service.SummarizeStreamAsync(new SummarizeRequest { Text = "x" }))
        {
            tokens.Add(token);
        }

        tokens.Should().Equal("valid");
    }
}
