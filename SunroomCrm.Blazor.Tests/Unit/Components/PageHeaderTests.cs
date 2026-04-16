using Bunit;
using FluentAssertions;
using MudBlazor.Services;
using SunroomCrm.Blazor.Components.Shared;

namespace SunroomCrm.Blazor.Tests.Unit.Components;

public class PageHeaderTests : TestContext
{
    public PageHeaderTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void PageHeader_RendersTitle()
    {
        var cut = RenderComponent<PageHeader>(p => p
            .Add(x => x.Title, "Test Title"));

        cut.Markup.Should().Contain("Test Title");
    }

    [Fact]
    public void PageHeader_RendersSubtitle()
    {
        var cut = RenderComponent<PageHeader>(p => p
            .Add(x => x.Title, "Title")
            .Add(x => x.Subtitle, "A subtitle here"));

        cut.Markup.Should().Contain("A subtitle here");
    }
}
