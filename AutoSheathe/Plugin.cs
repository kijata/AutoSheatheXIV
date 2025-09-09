using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using AutoSheathe.Windows;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;

using FFXIVClientStructs.FFXIV.Client.UI;
namespace AutoSheathe;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;


    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("AutoSheathe");
    private ConfigWindow ConfigWindow { get; init; }
    public DateTime timestamp { get; set; }
    public int setInterval { get; set; }
    private bool timerStarted;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);


        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Framework.Update += CombatTimer;
        this.timestamp = DateTime.Now;
        this.timerStarted = false;
        Log.Information($"===Init of AutoSheathe done===");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        Framework.Update -= CombatTimer;
    }

    private unsafe void CombatTimer(IFramework framework)
    {
        if (!IsInCombatButNoTarget())
        {
            // if not in combat, not weapon sheathed, has a target or is casting, stop
            this.timerStarted = false;
            return;
        }
        else
        { if (this.timerStarted == false)
            {
                this.timerStarted = true;
                var rand = new Random();
                this.timestamp = DateTime.Now.AddMilliseconds(Configuration.sheatheTime*1000+rand.Next(2000));
            }
        }
        if ((this.timestamp.CompareTo(DateTime.Now) < 0) &&
            IsInCombatButNoTarget() && 
            (this.timerStarted == true) )
        {
            UIModule.Instance()->ExecuteMainCommand(1);
            Log.Information($"===AutoSheathe triggered===");
            this.timerStarted = false;
        }
    }
    

    public unsafe bool IsInCombatButNoTarget()
    {
        var localPlayer = Plugin.ClientState.LocalPlayer;
        var ret = false;
        if (localPlayer == null)
        {
            return false;
        }
        if (localPlayer.CurrentHp == 0)
        {
            //if dead
            return false;
        }
        // if in combat, not weapon sheathed, has no target AND is not casting, return true
        ret = ((localPlayer.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) != 0) &&
            (UIState.Instance()->WeaponState.IsUnsheathed) &&
            ((localPlayer.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.IsCasting) == 0) &&
            (localPlayer.TargetObject == null);
        return ret;
    }
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
