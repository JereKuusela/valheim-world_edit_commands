using System;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands;
public class SpawnLocationCommand
{
  public const string Name = "spawn_location";
  public SpawnLocationCommand()
  {
    SpawnLocationAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Spawns a given location.", new[] { "name" }, autoComplete.NamedParameters);
    Helper.Command(Name, description, (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing location id.");
      var obj = ZoneSystem.instance;
      var name = args[1];
      var location = obj.GetLocation(name.GetStableHashCode()) ?? throw new InvalidOperationException($"Can't find location {name}.");
      if (location.m_prefab == null)
        throw new InvalidOperationException($"Can't find prefab for location {name}.");
      var seed = UnityEngine.Random.Range(0, 99999);
      var dungeonSeed = int.MinValue;
      var relativeAngle = (float)UnityEngine.Random.Range(0, 16) * 22.5f;
      var baseAngle = 0f;
      var relativePosition = Vector3.zero;
      var basePosition = Vector3.zero;
      var player = Player.m_localPlayer.transform;
      if (player)
      {
        basePosition = player.position;
        relativePosition = 2.0f * player.transform.forward;
        baseAngle = player.transform.rotation.eulerAngles.y;
      }
      var snap = true;
      foreach (var arg in args.Args)
      {
        var split = arg.Split('=');
        var argName = split[0].ToLower();
        if (split.Length < 2) continue;
        if (argName == "seed")
          seed = Parse.Int(split[1], 0);
        if (argName == "dungeonseed")
          dungeonSeed = Parse.Int(split[1], 0);
        if (argName == "rot" || argName == "rotation")
          relativeAngle = Parse.Float(split[1], 0);
        if (argName == "pos" || argName == "position")
        {
          relativePosition = Parse.VectorXZY(split[1].Split(','));
          snap = split[1].Split(',').Length < 3;
        }
        if (argName == "refrot" || argName == "refrotation")
        {
          baseAngle = Parse.Float(split[1], baseAngle);
        }
        if (argName == "from" || argName == "refpos")
        {
          basePosition = Parse.VectorXZY(split[1].Split(','), basePosition);
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
      DungeonGenerator.m_forceSeed = dungeonSeed;
      ZoneSystem.instance.SpawnLocation(location, seed, spawnPosition, spawnRotation, ZoneSystem.SpawnMode.Full, []);
      args.Context.AddString("Spawned: " + name + " at " + Helper.PrintVectorXZY(spawnPosition));
      var spawns = AddedZDOs.StopTracking();
      // Disable player based positioning.
      var undoCommand = "spawn_location " + name + " refRot=" + baseAngle + " from=" + Helper.PrintVectorXZY(basePosition) + " seed=" + seed + " rot=" + relativePosition + " " + string.Join(" ", args.Args.Skip(2));
      UndoSpawn undo = new(spawns, undoCommand);
      UndoManager.Add(undo);
    });
  }
}
