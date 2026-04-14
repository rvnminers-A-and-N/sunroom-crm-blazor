using MudBlazor;

namespace SunroomCrm.Blazor.Components.Layout;

public static class SunroomTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#02795F",       // Sunroom Emerald
            Secondary = "#F76C6C",     // Sunroom Coral
            Tertiary = "#F4C95D",      // Sunroom Gold
            Info = "#3B82F6",
            Success = "#10B981",
            Warning = "#F59E0B",
            Error = "#EF4444",
            Background = "#F9FAFB",
            Surface = "#FFFFFF",
            DrawerBackground = "#111827",
            DrawerText = "#D1D5DB",
            DrawerIcon = "#9CA3AF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#111827",
            TextPrimary = "#111827",
            TextSecondary = "#6B7280",
            ActionDefault = "#6B7280",
            ActionDisabled = "#D1D5DB",
            ActionDisabledBackground = "#F3F4F6",
            Divider = "#E5E7EB",
            LinesDefault = "#E5E7EB"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#10B981",
            Secondary = "#F76C6C",
            Tertiary = "#F4C95D",
            Background = "#111827",
            Surface = "#1F2937",
            DrawerBackground = "#0F172A",
            DrawerText = "#D1D5DB",
            DrawerIcon = "#9CA3AF",
            AppbarBackground = "#1F2937",
            AppbarText = "#F9FAFB",
            TextPrimary = "#F9FAFB",
            TextSecondary = "#9CA3AF"
        },
        Typography = new Typography
        {
            Default = new Default { FontFamily = new[] { "DM Sans", "Segoe UI", "Helvetica", "Arial", "sans-serif" } },
            H1 = new H1 { FontSize = "2.25rem", FontWeight = 700 },
            H2 = new H2 { FontSize = "1.875rem", FontWeight = 700 },
            H3 = new H3 { FontSize = "1.5rem", FontWeight = 600 },
            H4 = new H4 { FontSize = "1.25rem", FontWeight = 600 },
            H5 = new H5 { FontSize = "1.125rem", FontWeight = 600 },
            H6 = new H6 { FontSize = "1rem", FontWeight = 600 },
            Body1 = new Body1 { FontSize = "0.9375rem" },
            Body2 = new Body2 { FontSize = "0.875rem" },
            Button = new Button { FontWeight = 600, TextTransform = "none" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "260px"
        }
    };
}
