using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class SpawnObjectCommand {
  public const string Name = "spawn_object";
  private static List<GameObject> SpawnObject(SpawnObjectParameters pars, GameObject prefab, int count) {
    List<GameObject> spawned = new();
    for (int i = 0; i < count; i++) {
      Vector3 spawnPosition;
      if (pars.To.HasValue)
        spawnPosition = GetPosition(pars.From, pars.To.Value, i, count);
      else {
        spawnPosition = GetPosition(pars.From, pars.RelativePosition, pars.BaseRotation);
        if (i > 0)
          spawnPosition += UnityEngine.Random.insideUnitSphere * (pars.Radius ?? 0.5f);
      }
      if (pars.Snap && ZoneSystem.instance.FindFloor(spawnPosition, out var height))
        spawnPosition.y = height;
      var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, spawnPosition, Quaternion.identity);
      spawned.Add(obj);
    }
    return spawned;
  }

  private static Vector3 GetPosition(Vector3 basePosition, Range<Vector3> relativePosition, Quaternion rotation) {
    var relative = Helper.RandomValue(relativePosition);
    var position = basePosition;
    position += rotation * Vector3.forward * relative.x;
    position += rotation * Vector3.right * relative.z;
    position += rotation * Vector3.up * relative.y;
    return position;
  }
  private static Vector3 GetPosition(Vector3 from, Vector3 to, int index, int max) {
    return from + (to - from) * index / (max - 1);
  }
  private static void Manipulate(IEnumerable<GameObject> spawned, SpawnObjectParameters pars, int total) {
    foreach (var obj in spawned) {
      var rotation = pars.BaseRotation * Quaternion.Euler(Helper.RandomValue(pars.Rotation));
      Actions.SetLevel(obj, Helper.RandomValue(pars.Level));
      Actions.SetHealth(obj, Helper.RandomValue(pars.Health));
      Actions.SetVariant(obj, Helper.RandomValue(pars.Variant));
      Actions.SetName(obj, pars.Name);
      Actions.SetHunt(obj, pars.Hunt);
      Actions.SetTame(obj, pars.Tamed);
      total -= Actions.SetStack(obj, total);
      Actions.SetRotation(obj, rotation);
      Actions.SetScale(obj, Helper.RandomValue(pars.Scale));
      Actions.SetVisual(obj, VisSlot.Helmet, pars.Helmet);
      Actions.SetVisual(obj, VisSlot.Chest, pars.Chest);
      Actions.SetVisual(obj, VisSlot.Shoulder, pars.Shoulders);
      Actions.SetVisual(obj, VisSlot.Legs, pars.Legs);
      Actions.SetVisual(obj, VisSlot.Utility, pars.Utility);
      Actions.SetVisual(obj, VisSlot.HandLeft, pars.LeftHand);
      Actions.SetVisual(obj, VisSlot.HandRight, pars.RightHand);
      Actions.SetModel(obj, Helper.RandomValue(pars.Model));
      if (pars.Helmet != null || pars.Chest != null || pars.Shoulders != null || pars.Legs != null || pars.Utility != null || pars.LeftHand != null || pars.RightHand != null) {
        var zdo = obj.GetComponent<ZNetView>()?.GetZDO();
        // Temporarily losing the ownership prevents default items replacing the set items.
        if (zdo != null) zdo.m_owner = 0;
      }
    }
  }
  public SpawnObjectCommand() {
    SpawnObjectAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Spawns an object.", new[] { "name" }, autoComplete.NamedParameters);
    Helper.Command(Name, description, (args) => {
      Helper.ArgsCheck(args, 2, "Missing object id.");
      var prefabName = args[1];
      var prefab = Helper.GetPrefab(prefabName);
      if (!prefab) throw new InvalidOperationException("Unable to find the object.");

      SpawnObjectParameters pars = new(args);
      var itemDrop = prefab.GetComponent<ItemDrop>();
      var amount = Helper.RandomValue(pars.Amount);
      var count = amount;
      if (itemDrop)
        count = (int)Math.Ceiling((double)count / itemDrop.m_itemData.m_shared.m_maxStackSize);

      // For usability, spawn in front of the player if nothing is specified (similar to the base game command).
      if (pars.UseDefaultRelativePosition)
        pars.RelativePosition = new(new(2.0f, 0, 0));
      var position = GetPosition(pars.From, pars.RelativePosition, pars.BaseRotation);
      var spawned = SpawnObject(pars, prefab, count);
      Manipulate(spawned, pars, amount);
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName, spawned.Count, null);
      args.Context.AddString("Spawned: " + prefabName + " at " + Helper.PrintVectorXZY(position));
      var spawns = spawned.Select(obj => obj.GetComponent<ZNetView>()).Where(obj => obj != null).Select(obj => obj.GetZDO()).ToList();
      // Undo uses refPos which would disable the default relative position. So apply it to the refPos to keep the same position.
      if (pars.UseDefaultRelativePosition)
        pars.From = position;
      // refPos and refRot override the player based positioning (fixes undo position).
      var undoCommand = "spawn_object " + prefabName + " refRot=" + Helper.PrintAngleYXZ(pars.BaseRotation) + " refPos=" + Helper.PrintVectorXZY(pars.From) + " " + string.Join(" ", args.Args.Skip(2));
      UndoSpawn undo = new(spawns, undoCommand);
      UndoManager.Add(undo);
    }, () => ParameterInfo.ObjectIds);
  }
}
