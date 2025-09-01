using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using ECommons;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/as";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    public bool called { get; set; }
    public DateTime timestamp { get; set; }
    public int diff;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ECommonsMain.Init(pluginInterface, this);
        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        Framework.Update += CombatTimer;
        ActionEffect.ActionEffectEvent += OnAction;
        this.timestamp = DateTime.Now;
        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        Framework.Update -= CombatTimer;
        ActionEffect.ActionEffectEvent -= OnAction;
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        ToggleMainUI();
    }
    private unsafe void CombatTimer(IFramework framework)
    {

        var localPlayer = Plugin.ClientState.LocalPlayer;

        if (localPlayer == null)
        {
            return;
        }

        if (((localPlayer.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) != 0) &&
            UIState.Instance()->WeaponState.IsUnsheathed &&
            ((localPlayer.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.IsCasting) == 0)
            )
        {
            if (this.timestamp.AddMilliseconds(3000).CompareTo(DateTime.Now) < 0)
            {
                if (localPlayer.TargetObject == null)
                {
                    UIModule.Instance()->ExecuteMainCommand(1);
                    //ActionManager.Instance()->UseAction(ActionType.MainCommand, 1);
                   // ActionManager.Instance()->UseActionLocation(ActionType.MainCommand, 1);
                }
            }
        }
    }

    private unsafe void OnAction(ActionEffectSet set) {
        var localPlayer = Plugin.ClientState.LocalPlayer;

        if (localPlayer == null)
        {
            return;
        }
        if (((localPlayer.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) != 0) &&
            UIState.Instance()->WeaponState.IsUnsheathed)
        {
            this.timestamp = DateTime.Now;
        }
    }
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
