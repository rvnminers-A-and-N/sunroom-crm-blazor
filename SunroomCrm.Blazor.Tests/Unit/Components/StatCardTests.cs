using Bunit;
using FluentAssertions;
using SunroomCrm.Blazor.Components.Shared;

namespace SunroomCrm.Blazor.Tests.Unit.Components;

public class StatCardTests : TestContext
{
    [Fact]
    public void StatCard_RendersLabelAndValue()
    {
        var cut = RenderComponent<StatCard>(p => p
            .Add(x => x.Label, "Total Contacts")
            .Add(x => x.Value, "42"));

        cut.Markup.Should().Contain("Total Contacts");
        cut.Markup.Should().Contain("42");
    }

    [Fact]
    public void StatCard_HasStatCardCssClass()
    {
        var cut = RenderComponent<StatCard>(p => p
            .Add(x => x.Label, "Deals")
            .Add(x => x.Value, "10"));

        cut.Find(".stat-card").Should().NotBeNull();
    }
}
