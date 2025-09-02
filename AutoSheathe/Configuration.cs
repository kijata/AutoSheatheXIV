using Dalamud.Configuration;
using System;

namespace AutoSheathe;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public float sheatheTime { get; set; } = 1.0f;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
