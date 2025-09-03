using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
namespace AutoSheathe.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration config;


    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public ConfigWindow(Plugin plugin)
        : base("AutoSheathe##ASheathe")
    {
        config = plugin.Configuration;
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;
        Size = new Vector2(200, 90);

    }

    public void Dispose() { }

    public override void Draw()
    {
        var tmp = config.sheatheTime;
        ImGui.TextUnformatted("Time for weapon to be sheathed:");
        if (ImGui.SliderFloat("s", ref tmp, 0, 10.0f, "%.1f"u8))
        {
            config.sheatheTime = tmp;
            config.Save();
        }
    }
}
