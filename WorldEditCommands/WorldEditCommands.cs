using BepInEx;
using HarmonyLib;

namespace WorldEditCommands {
  [BepInPlugin("valheim.jerekuusela.world_edit_commands", "WorldEditCommands", "1.0.0.0")]
  [BepInDependency("valheim.jerekuusela.server_devcommands")]
  public class WorldEditCommands : BaseUnityPlugin {
    public void Awake() {
      Harmony harmony = new Harmony("valheim.jerekuusela.world_edit_commands");
      harmony.PatchAll();
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
}
