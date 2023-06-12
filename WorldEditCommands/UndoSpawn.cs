using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ServerDevcommands;
using Service;
namespace WorldEditCommands;
public class UndoSpawn : IUndoAction {
  private FakeZDO[] ZDOs;
  private readonly string Command;
  public UndoSpawn(IEnumerable<ZDO> zdos, string command) {
    ZDOs = zdos.Select(zdo => new FakeZDO(zdo)).ToArray();
    Command = command;
  }
  public string Undo() {
    var message = $"Removed {UndoHelper.Print(ZDOs)}";
    UndoHelper.Remove(ZDOs);
    return message;
  }


  public string Redo() {
    AddedZDOs.StartTracking();
    Console.instance.TryRunCommand(Command);
    ZDOs = AddedZDOs.StopTracking().Select(zdo => new FakeZDO(zdo)).ToArray();
    return $"Ran command {Command}";
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
