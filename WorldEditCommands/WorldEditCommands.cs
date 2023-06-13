using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
namespace WorldEditCommands;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("server_devcommands", "1.52")]
public class WorldEditCommands : BaseUnityPlugin {
  public const string GUID = "world_edit_commands";
  public const string NAME = "World Edit Commands";
  public const string VERSION = "1.36";
  public void Awake() {
    new Harmony(GUID).PatchAll();
  }
  public static bool IsSpawnerTweaks = false;
  public static bool IsStructureTweaks = false;
  public static bool IsCLLC = false;
  public void Start() {
    IsSpawnerTweaks = Chainloader.PluginInfos.ContainsKey("spawner_tweaks") || Chainloader.PluginInfos.ContainsKey("logic_tweaks");
    IsStructureTweaks = Chainloader.PluginInfos.ContainsKey("structure_tweaks") || Chainloader.PluginInfos.ContainsKey("logic_tweaks");
    IsCLLC = Chainloader.PluginInfos.ContainsKey("rg.bepinex.plugins.creaturelevelcontrol");
  }
}
[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands {
  public static void Postfix() {
    new SpawnLocationCommand();
    new SpawnObjectCommand();
    new ObjectCommand();
    new TerrainCommand();
    new AliasesCommand();
    new DungeonCommand();
    if (WorldEditCommands.IsSpawnerTweaks || WorldEditCommands.IsStructureTweaks)
      new TweakObjectCommand();
    if (WorldEditCommands.IsSpawnerTweaks) {
      new TweakAltarCommand();
      new TweakBeehiveCommand();
      new TweakPickableCommand();
      new TweakSpawnerCommand();
      new TweakSpawnPointCommand();
      new TweakItemStandCommand();
      new TweakChestCommand();
      new TweakCreatureCommand();
      new TweakSmelterCommand();
      new TweakFermenterCommand();
    }
    if (WorldEditCommands.IsStructureTweaks) {
      new TweakDungeonCommand();
      new TweakRunestoneCommand();
      new TweakPortalCommand();
      new TweakFireplaceCommand();
      new TweakChestCommand();
      new TweakDoorCommand();
    }
  }
}
