using BepInEx;
using HarmonyLib;
namespace WorldEditCommands;
[BepInPlugin("valheim.jerekuusela.world_edit_commands", "WorldEditCommands", "1.4.0.0")]
[BepInDependency("valheim.jerekuusela.server_devcommands", "1.20.0.0")]
public class WorldEditCommands : BaseUnityPlugin {
  public void Awake() {
    Harmony harmony = new("valheim.jerekuusela.world_edit_commands");
    harmony.PatchAll();
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
