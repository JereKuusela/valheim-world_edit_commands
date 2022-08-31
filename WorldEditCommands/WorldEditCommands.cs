using BepInEx;
using HarmonyLib;
namespace WorldEditCommands;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("server_devcommands", "1.27")]
public class WorldEditCommands : BaseUnityPlugin {
  public const string GUID = "world_edit_commands";
  public const string NAME = "World Edit Commands";
  public const string VERSION = "1.9";
  public void Awake() {
    new Harmony(GUID).PatchAll();
  }

  public void LateUpdate() {
    Ruler.Update();
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
  }
}
