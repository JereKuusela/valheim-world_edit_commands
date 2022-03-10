using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {
  public class SpawnObjectCommand {
    private static List<GameObject> SpawnObject(GameObject prefab, Vector3 position, int count, float radius, bool snap) {
      var spawned = new List<GameObject>();
      for (int i = 0; i < count; i++) {
        var spawnPosition = position;
        if (i > 0)
          spawnPosition += UnityEngine.Random.insideUnitSphere * radius;
        if (snap && ZoneSystem.instance.FindFloor(spawnPosition, out var height))
          spawnPosition.y = height;
        var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, spawnPosition, Quaternion.identity);
        spawned.Add(obj);
      }
      return spawned;
    }

    private static SpawnObjectParameters ParseArgs(Terminal.ConsoleEventArgs args) {
      var pars = new SpawnObjectParameters();
      if (Player.m_localPlayer) {
        pars.BasePosition = Player.m_localPlayer.transform.position;
        pars.BaseRotation = Player.m_localPlayer.transform.rotation;
      }
      if (!pars.ParseArgs(args.Args, args.Context)) return null;
      return pars;
    }
    private static Vector3 GetPosition(Vector3 basePosition, Vector3 relativePosition, Quaternion rotation) {
      var position = basePosition;
      position += rotation * Vector3.forward * relativePosition.x;
      position += rotation * Vector3.right * relativePosition.z;
      position += rotation * Vector3.up * relativePosition.y;
      return position;
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
        if (pars.Helmet != null || pars.Chest != null || pars.Shoulders != null || pars.Legs != null || pars.Utility != null || pars.LeftHand != null || pars.RightHand != null) {
          var zdo = obj.GetComponent<ZNetView>()?.GetZDO();
          // Temporarily losing the ownership prevents default items replacing the set items.
          if (zdo != null) zdo.m_owner = 0;
        }
      }
    }
    public SpawnObjectCommand() {
      var autoComplete = new SpawnObjectAutoComplete();
      var description = CommandInfo.Create("Spawns an object.", new string[] { "name" }, autoComplete.NamedParameters);
      new Terminal.ConsoleCommand("spawn_object", description, delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var prefabName = args[1];
        var prefab = Helper.GetPrefab(prefabName);
        if (!prefab) return;

        var pars = ParseArgs(args);
        if (pars == null) return;
        var itemDrop = prefab.GetComponent<ItemDrop>();
        var amount = Helper.RandomValue(pars.Amount);
        var count = amount;
        if (itemDrop)
          count = (int)Math.Ceiling((double)count / itemDrop.m_itemData.m_shared.m_maxStackSize);

        // For usability, spawn in front of the player if nothing is specified (similar to the base game command).
        if (pars.UseDefaultRelativePosition)
          pars.RelativePosition = new Vector3(2.0f, 0, 0);
        var position = GetPosition(pars.BasePosition, pars.RelativePosition, pars.BaseRotation);
        var spawned = SpawnObject(prefab, position, count, pars.Radius, pars.Snap);
        Manipulate(spawned, pars, amount);
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName, spawned.Count, null);
        args.Context.AddString("Spawned: " + prefabName + " at " + Helper.PrintVectorXZY(position));
        var spawns = spawned.Select(obj => obj.GetComponent<ZNetView>()?.GetZDO()).Where(obj => obj != null).ToList();
        // Undo uses refPos which would disable the default relative position. So apply it to the refPos to keep the same position.
        if (pars.UseDefaultRelativePosition)
          pars.BasePosition = position;
        // refPos and refRot override the player based positioning (fixes undo position).
        var undoCommand = "spawn_object " + prefabName + " refRot=" + Helper.PrintAngleYXZ(pars.BaseRotation) + " refPos=" + Helper.PrintVectorXZY(pars.BasePosition) + " " + string.Join(" ", args.Args.Skip(2));
        var undo = new UndoSpawn(spawns, undoCommand);
        UndoManager.Add(undo);
      }, true, true, optionsFetcher: () => ParameterInfo.ObjectIds);
    }
  }
}