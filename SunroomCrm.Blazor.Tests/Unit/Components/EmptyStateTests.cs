using Bunit;
using FluentAssertions;
using MudBlazor;
using MudBlazor.Services;
using SunroomCrm.Blazor.Components.Shared;

namespace SunroomCrm.Blazor.Tests.Unit.Components;

public class EmptyStateTests : TestContext
{
    public EmptyStateTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void EmptyState_RendersTitle()
    {
        var cut = RenderComponent<EmptyState>(p => p
            .Add(x => x.Title, "No contacts yet"));

        cut.Markup.Should().Contain("No contacts yet");
    }

    [Fact]
    public void EmptyState_RendersDescription()
    {
        var cut = RenderComponent<EmptyState>(p => p
            .Add(x => x.Title, "Empty")
            .Add(x => x.Description, "Create your first contact."));

        cut.Markup.Should().Contain("Create your first contact.");
    }

    [Fact]
    public void EmptyState_HasEmptyStateCssClass()
    {
        var cut = RenderComponent<EmptyState>();

        cut.Find(".empty-state").Should().NotBeNull();
    }
}
