using System;
using System.Collections.Generic;
using System.Linq;
using DEV;
using Service;
using UnityEngine;

namespace WorldEditCommands {

  class SpawnObjectParameters {
    public Quaternion RelativeRotation = Quaternion.identity;
    public Quaternion BaseRotation = Quaternion.identity;
    public Vector3 Scale = Vector3.one;
    public Vector3 RelativePosition = Vector3.zero;
    public Vector3 BasePosition = Vector3.zero;
    public int Level = 1;
    public int Amount = 1;
    public float Health = 0f;
    public string Name = "";
    public int Variant = 0;
    public bool Tamed = false;
    public bool Hunt = false;
    public bool Snap = true;
  }
  public class SpawnObjectCommand : BaseCommand {
    private static List<GameObject> SpawnObject(GameObject prefab, Vector3 position, int count, bool snap) {
      var spawned = new List<GameObject>();
      for (int i = 0; i < count; i++) {
        var spawnPosition = position;
        if (i > 0)
          spawnPosition += UnityEngine.Random.insideUnitSphere * 0.5f;
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
      var useDefaultRelativePosition = true;
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        if (split[0] == "tame" || split[0] == "tamed")
          pars.Tamed = true;
        if (split[0] == "hunt")
          pars.Hunt = true;
        if (split.Length < 2) continue;
        if (split[0] == "health" || split[0] == "durability")
          pars.Health = TryFloat(split[1], 0);
        if (split[0] == "name" || split[0] == "crafter")
          pars.Name = split[1];
        if (split[0] == "variant")
          pars.Variant = TryInt(split[1], 0);
        if (split[0] == "star" || split[0] == "stars")
          pars.Level = TryInt(split[1], 0) + 1;
        if (split[0] == "level" || split[0] == "levels")
          pars.Level = TryInt(split[1], 1);
        if (split[0] == "amount")
          pars.Amount = TryInt(split[1], 1);
        if (split[0] == "rot" || split[0] == "rotation") {
          pars.RelativeRotation = ParseAngleYXZ(split[1]);
        }
        if (split[0] == "refRot" || split[0] == "refRotation") {
          pars.BaseRotation = ParseAngleYXZ(split[1], pars.BaseRotation);
        }
        if (split[0] == "sc" || split[0] == "scale") {
          pars.Scale = TryScale(split[1].Split(','));
        }
        if (split[0] == "pos" || split[0] == "position") {
          useDefaultRelativePosition = false;
          pars.RelativePosition = TryVectorXZY(split[1].Split(','));
          pars.Snap = split[1].Split(',').Length < 3;
        }
        if (split[0] == "refPos" || split[0] == "refPosition") {
          useDefaultRelativePosition = false;
          pars.BasePosition = TryVectorXZY(split[1].Split(','), pars.BasePosition);
        }
        if (split[0] == "refPlayer") {
          useDefaultRelativePosition = false;
          var player = FindPlayer(split[1]);
          if (player.m_characterID.IsNone()) {
            args.Context.AddString("Error: Unable to find the player.");
            return null;
          } else if (!player.m_publicPosition) {
            args.Context.AddString("Error: Player doesn't have a public position.");
            return null;
          } else {
            pars.BasePosition = player.m_position;
          }
        }
      }
      // For usability, spawn in front of the player if nothing is specified (similar to the base game command).
      if (useDefaultRelativePosition)
        pars.RelativePosition = new Vector3(2.0f, 0, 0);
      return pars;
    }
    private static Vector3 GetPosition(Vector3 basePosition, Vector3 relativePosition, Quaternion rotation) {
      var position = basePosition;
      position += rotation * Vector3.forward * relativePosition.x;
      position += rotation * Vector3.right * relativePosition.z;
      position += rotation * Vector3.up * relativePosition.y;
      return position;
    }
    private static void Manipulate(IEnumerable<GameObject> spawned, SpawnObjectParameters pars) {
      var rotation = pars.BaseRotation * pars.RelativeRotation;
      var total = pars.Amount;
      foreach (var obj in spawned) {
        Actions.SetLevel(obj, pars.Level);
        Actions.SetHealth(obj, pars.Health);
        Actions.SetVariant(obj, pars.Variant);
        Actions.SetName(obj, pars.Name);
        Actions.SetHunt(obj, pars.Hunt);
        Actions.SetTame(obj, pars.Tamed);
        total -= Actions.SetStack(obj, total);
        Actions.SetRotation(obj, rotation);
        Actions.SetScale(obj, pars.Scale);
      }
    }
    public SpawnObjectCommand() {
      new Terminal.ConsoleCommand("spawn_object", "[name] ...parameters - Spawns an object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var prefabName = args[1];
        var prefab = Helper.GetPrefab(prefabName);
        if (!prefab) return;

        var pars = ParseArgs(args);
        if (pars == null) return;
        var itemDrop = prefab.GetComponent<ItemDrop>();
        var count = pars.Amount;
        if (itemDrop)
          count = (int)Math.Ceiling((double)count / itemDrop.m_itemData.m_shared.m_maxStackSize);
        var position = GetPosition(pars.BasePosition, pars.RelativePosition, pars.BaseRotation);
        var spawned = SpawnObject(prefab, position, count, pars.Snap);
        Manipulate(spawned, pars);
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName, spawned.Count, null);
        args.Context.AddString("Spawned: " + prefabName + " at " + PrintVectorXZY(position));
        var spawns = spawned.Select(obj => obj.GetComponent<ZNetView>()?.GetZDO()).Where(obj => obj != null).ToList();
        // Disable player based positioning.
        var undoCommand = "spawn_object " + prefabName + " refRot=" + PrintAngleYXZ(pars.BaseRotation) + " refPos=" + PrintVectorXZY(pars.BasePosition) + " " + string.Join(" ", args.Args.Skip(2));
        var undo = new UndoSpawn(spawns, undoCommand);
        UndoManager.Add(undo);
      }, true, true, optionsFetcher: () => ParameterInfo.Ids);
      new SpawnObjectAutoComplete();
    }
  }
}