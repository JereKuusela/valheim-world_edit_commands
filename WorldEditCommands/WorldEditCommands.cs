using BepInEx;
using HarmonyLib;
namespace WorldEditCommands;
[BepInPlugin("valheim.jerekuusela.world_edit_commands", "WorldEditCommands", "1.3.0.0")]
[BepInDependency("valheim.jerekuusela.server_devcommands", "1.17.0.0")]
public class WorldEditCommands : BaseUnityPlugin {
  public void Awake() {
    Harmony harmony = new("valheim.jerekuusela.world_edit_commands");
    harmony.PatchAll();
  }

  public void LateUpdate() {
    TerrainRuler.Update();
  }
}
[HarmonyPatch(typeof(Terminal), "InitTerminal")]
public class SetCommands {
  public static void Postfix() {
    new SpawnLocationCommand();
    new SpawnObjectCommand();
    new ObjectCommand();
    new TerrainCommand();
    new AliasesCommand();
  }
}
