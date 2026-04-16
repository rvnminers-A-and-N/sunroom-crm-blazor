using Bunit;
using FluentAssertions;
using SunroomCrm.Blazor.Components.Shared;
using SunroomCrm.Shared.DTOs.Tags;

namespace SunroomCrm.Blazor.Tests.Unit.Components;

public class TagChipTests : TestContext
{
    [Fact]
    public void TagChip_RendersTagName()
    {
        var tag = new TagDto { Id = 1, Name = "VIP", Color = "#02795F" };

        var cut = RenderComponent<TagChip>(p => p.Add(x => x.Tag, tag));

        cut.Markup.Should().Contain("VIP");
    }

    [Fact]
    public void TagChip_AppliesTagColor()
    {
        var tag = new TagDto { Id = 1, Name = "Hot", Color = "#FF5733" };

        var cut = RenderComponent<TagChip>(p => p.Add(x => x.Tag, tag));

        cut.Markup.Should().Contain("#FF5733");
    }
}
