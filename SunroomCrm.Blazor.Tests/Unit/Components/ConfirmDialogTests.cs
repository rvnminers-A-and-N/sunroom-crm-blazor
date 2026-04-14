using Bunit;
using FluentAssertions;
using MudBlazor;
using MudBlazor.Services;
using SunroomCrm.Blazor.Components.Shared;

namespace SunroomCrm.Blazor.Tests.Unit.Components;

public class ConfirmDialogTests : TestContext
{
    public ConfirmDialogTests()
    {
        Services.AddMudServices();
        JSInterop.SetupVoid("mudPopover.initialize", _ => true);
        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ConfirmDialog_DefaultMessage_IsAreYouSure()
    {
        var dialog = new ConfirmDialog();

        dialog.Message.Should().Be("Are you sure?");
    }

    [Fact]
    public void ConfirmDialog_DefaultConfirmText_IsConfirm()
    {
        var dialog = new ConfirmDialog();

        dialog.ConfirmText.Should().Be("Confirm");
    }

    [Fact]
    public void ConfirmDialog_DefaultColor_IsError()
    {
        var dialog = new ConfirmDialog();

        dialog.ConfirmColor.Should().Be(Color.Error);
    }
}
