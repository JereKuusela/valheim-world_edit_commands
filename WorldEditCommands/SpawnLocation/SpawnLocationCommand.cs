using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {

  public class SpawnLocationCommand {
    public SpawnLocationCommand() {
      new Terminal.ConsoleCommand("spawn_location", "[name] (seed=n pos=x,z,y rot=y refPos=x,z,y refRot=y) - Spawns a given location.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          return;
        }
        var obj = ZoneSystem.instance;
        var name = args[1];
        var location = obj.GetLocation(name);
        if (location == null) {
          ZLog.Log("Missing location:" + name);
          args.Context.AddString("Missing location:" + name);
          return;
        }
        if (location.m_prefab == null) {
          ZLog.Log("Missing prefab in location:" + name);
          args.Context.AddString("Missing location:" + name);
          return;
        }

        var seed = UnityEngine.Random.Range(0, 99999);
        var relativeAngle = (float)UnityEngine.Random.Range(0, 16) * 22.5f;
        var baseAngle = 0f;
        var relativePosition = Vector3.zero;
        var basePosition = Vector3.zero;
        var player = Player.m_localPlayer.transform;
        if (player) {
          basePosition = player.position;
          relativePosition = 2.0f * player.transform.forward;
          baseAngle = player.transform.rotation.eulerAngles.y;
        }
        var snap = true;
        foreach (var arg in args.Args) {
          var split = arg.Split('=');
          if (split.Length < 2) continue;
          if (split[0] == "seed")
            seed = Parse.TryInt(split[1], 0);
          if (split[0] == "rot" || split[0] == "rotation")
            relativeAngle = Parse.TryFloat(split[1], 0);
          if (split[0] == "pos" || split[0] == "position") {
            relativePosition = Parse.TryVectorXZY(split[1].Split(','));
            snap = split[1].Split(',').Length < 3;
          }
          if (split[0] == "refRot" || split[0] == "refRotation") {
            baseAngle = Parse.TryFloat(split[1], baseAngle);
          }
          if (split[0] == "refPos" || split[0] == "refPosition") {
            basePosition = Parse.TryVectorXZY(split[1].Split(','), basePosition);
          }
        }
        var baseRotation = Quaternion.Euler(0f, baseAngle, 0f);
        var spawnPosition = basePosition;
        spawnPosition += baseRotation * Vector3.forward * relativePosition.x;
        spawnPosition += baseRotation * Vector3.right * relativePosition.z;
        spawnPosition += baseRotation * Vector3.up * relativePosition.y;
        var spawnRotation = baseRotation * Quaternion.Euler(0f, relativeAngle, 0f);
        if (snap && ZoneSystem.instance.FindFloor(spawnPosition, out var value))
          spawnPosition.y = value;

        AddedZDOs.StartTracking();
        ZoneSystem.instance.SpawnLocation(location, seed, spawnPosition, spawnRotation, ZoneSystem.SpawnMode.Full, new List<GameObject>());
        args.Context.AddString("Spawned: " + name + " at " + Helper.PrintVectorXZY(spawnPosition));
        var spawns = AddedZDOs.StopTracking();
        // Disable player based positioning.
        var undoCommand = "spawn_location " + name + " refRot=" + baseAngle + " refPos=" + Helper.PrintVectorXZY(basePosition) + " seed=" + seed + " rot=" + relativePosition + " " + string.Join(" ", args.Args.Skip(2));
        var undo = new UndoSpawn(spawns, undoCommand);

      }, true, true, optionsFetcher: () => ParameterInfo.LocationIds);
      new SpawnLocationAutoComplete();
    }
  }
}
