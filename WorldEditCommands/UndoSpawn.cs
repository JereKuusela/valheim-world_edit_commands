using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ServerDevcommands;
namespace WorldEditCommands;
public class UndoSpawn : UndoAction {
  private IEnumerable<ZDO> ZDOs;
  private readonly string Command;
  public UndoSpawn(IEnumerable<ZDO> zdos, string command) {
    ZDOs = zdos;
    Command = command;
  }
  public void Undo() {
    foreach (var zdo in ZDOs) {
      if (zdo == null || !zdo.IsValid()) continue;
      zdo.SetOwner(ZDOMan.instance.GetMyID());
      ZDOMan.instance.DestroyZDO(zdo);
    }
  }

  public string UndoMessage() {
    var groups = ZDOs.GroupBy(zdo => zdo.GetPrefab());
    if (groups.Count() == 1) {
      var group = groups.First();
      var name = Helper.GetPrefabName(group.Key);
      if (group.Count() == 1)
        return $"Removing {name}.";
      return $"Removing {ZDOs.Count()} of {name}.";
    } else
      return "Removing " + ZDOs.Count() + " spawned objects.";
  }


  public void Redo() {
    AddedZDOs.StartTracking();
    Console.instance.TryRunCommand(Command);
    ZDOs = AddedZDOs.StopTracking();
  }
  public string RedoMessage() {
    var groups = ZDOs.GroupBy(zdo => zdo.GetPrefab());
    if (groups.Count() == 1) {
      var group = groups.First();
      var name = Helper.GetPrefabName(group.Key);
      if (group.Count() == 1)
        return $"Respawning {name}.";
      return $"Respawning {ZDOs.Count()} of {name}.";
    } else
      return "Respawning " + ZDOs.Count() + " spawned objects.";
  }
}


///<summary>Catch new zdos for undo.</summary>
[HarmonyPatch(typeof(ZNetView), "Awake")]
public class AddedZDOs {
  private static List<ZDO> zdos = new();
  private static bool Track = false;
  public static void StartTracking() {
    zdos = new();
    Track = true;
  }
  public static IEnumerable<ZDO> StopTracking() {
    Track = false;
    return zdos;
  }
  public static void Postfix(ZNetView __instance) {
    if (Track) zdos.Add(__instance.GetZDO());
  }
}
