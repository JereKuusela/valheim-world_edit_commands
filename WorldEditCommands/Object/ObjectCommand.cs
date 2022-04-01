using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class ObjectCommand {
  public const string Name = "object";
  private static bool IsIncluded(string id, string name) {
    if (id.StartsWith("*", StringComparison.Ordinal) && id.EndsWith("*", StringComparison.Ordinal)) {
      return name.Contains(id.Substring(1, id.Length - 3));
    }
    if (id.StartsWith("*", StringComparison.Ordinal)) return name.EndsWith(id.Substring(1), StringComparison.Ordinal);
    if (id.EndsWith("*", StringComparison.Ordinal)) return name.StartsWith(id.Substring(0, id.Length - 2), StringComparison.Ordinal);
    return id == name;
  }
  public static IEnumerable<int> GetPrefabs(string id) {
    id = id.ToLower();
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    values = values.Where(prefab => prefab.name != "Player");
    if (id == "*")
      values = values.Where(prefab => !prefab.name.StartsWith("_", StringComparison.Ordinal));
    else if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name.ToLower()));
    else
      values = values.Where(prefab => prefab.name.ToLower() == id);
    return values.Select(prefab => prefab.name.GetStableHashCode()).ToHashSet();
  }
  public static IEnumerable<ZDO> GetZDOs(string id, float distance) {
    var codes = GetPrefabs(id);
    IEnumerable<ZDO> zdos = ZDOMan.instance.m_objectsByID.Values;
    zdos = zdos.Where(zdo => codes.Contains(zdo.GetPrefab()));
    var position = Player.m_localPlayer ? Player.m_localPlayer.transform.position : Vector3.zero;
    if (distance > 0)
      return zdos.Where(zdo => Vector3.Distance(zdo.GetPosition(), position) <= distance);
    return zdos;
  }
  private static void Execute(Terminal context, ObjectParameters pars, IEnumerable<string> operations, IEnumerable<ZDO> zdos) {
    var scene = ZNetScene.instance;
    Dictionary<ZDOID, long> oldOwner = new();
    var views = zdos.Where(zdo => {
      var view = scene.FindInstance(zdo);
      if (!view || !view.GetZDO().IsValid()) {
        context.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      return true;
    }).Select(zdo => scene.FindInstance(zdo)).ToArray();
    foreach (var view in views) {
      oldOwner.Add(view.GetZDO().m_uid, view.GetZDO().m_owner);
      view.ClaimOwnership();
    }
    var count = views.Count();
    foreach (var operation in operations) {
      foreach (var view in views) {
        if (!view) continue;
        var character = view.GetComponent<Character>();
        var output = "";
        var name = Utils.GetPrefabName(view.gameObject);
        if (operation == "durability" || operation == "health")
          output = ChangeHealth(view, Helper.RandomValue(pars.Health));
        if (operation == "stars")
          output = SetStars(character, Helper.RandomValue(pars.Level) - 1);
        if (operation == "tame")
          output = MakeTame(character);
        if (operation == "wild")
          output = MakeWild(character);
        if (operation == "baby")
          output = SetBaby(view.GetComponent<Growup>());
        if (operation == "info")
          output = GetInfo(view);
        if (operation == "sleep")
          output = MakeSleep(view.GetComponent<MonsterAI>());
        if (operation == "visual")
          output = SetVisual(view.GetComponent<ItemStand>(), pars.Visual);
        if (operation == "model")
          output = SetModel(character, Helper.RandomValue(pars.Model));
        if (operation == "helmet")
          output = SetHelmet(character, pars.Helmet);
        if (operation == "left_hand")
          output = SetLeftHand(character, pars.LeftHand);
        if (operation == "right_hand")
          output = SetRightHand(character, pars.RightHand);
        if (operation == "chest")
          output = SetChest(character, pars.Chest);
        if (operation == "shoulders")
          output = SetShoulder(character, pars.Shoulders);
        if (operation == "legs")
          output = SetLegs(character, pars.Legs);
        if (operation == "utility")
          output = SetUtility(character, pars.Utility);
        if (operation == "move")
          output = Move(view, Helper.RandomValue(pars.Offset), pars.Origin);
        if (operation == "rotate") {
          if (pars.ResetRotation)
            output = ResetRotation(view);
          else
            output = Rotate(view, Helper.RandomValue(pars.Rotation), pars.Origin);
        }
        if (operation == "scale")
          output = Scale(view, Helper.RandomValue(pars.Scale));
        if (operation == "remove") {
          ZNetScene.instance.Destroy(view.gameObject);
          output = "Entity ¤ destroyed.";
        }
        // No operation.
        if (output == "") continue;
        var message = output.Replace("¤", name);
        if (count == 1)
          Helper.AddMessage(context, message);
        else
          context.AddString(message);
      }
    }
    foreach (var view in views) {
      if (!view || view.GetZDO() == null || !view.GetZDO().IsValid() || !oldOwner.ContainsKey(view.GetZDO().m_uid)) continue;
      view.GetZDO().SetOwner(oldOwner[view.GetZDO().m_uid]);
    }
  }
  public ObjectCommand() {
    ObjectAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Modifies the selected object(s).", null, autoComplete.NamedParameters);
    new Terminal.ConsoleCommand(Name, description, (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) return;
      ObjectParameters pars = new();
      if (!pars.ParseArgs(args.Args, args.Context)) return;
      IEnumerable<ZDO> zdos;
      if (pars.Radius > 0f) {
        zdos = GetZDOs(pars.Id, pars.Radius);
      } else {
        var view = Helper.GetHovered(args);
        if (!view) return;
        if (!GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab())) {
          Helper.AddMessage(args.Context, $"Skipped: {view.name} has invalid id.");
          return;
        }
        zdos = new[] { view.GetZDO() };
      }
      Execute(args.Context, pars, pars.Operations, zdos);

    }, true, true, optionsFetcher: () => autoComplete.NamedParameters);
  }

  private static string ChangeHealth(ZNetView obj, float amount) {
    var character = obj.GetComponent<Character>();
    if (character == null && obj.GetComponent<WearNTear>() == null && obj.GetComponent<TreeLog>() == null && obj.GetComponent<Destructible>() == null && obj.GetComponent<TreeBase>() == null)
      return "Skipped: ¤ is not a creature or a destructible.";
    var previous = Actions.SetHealth(obj.gameObject, amount);
    var amountStr = amount == 0f ? "default" : amount.ToString("F0");
    return $"¤ health changed from {previous.ToString("F0")} to {amountStr}.";
  }
  private static string SetStars(Character obj, int amount) {
    if (obj == null) return "Skipped: ¤ is not a creature.";
    var previous = obj.GetLevel() - 1;
    Actions.SetLevel(obj.gameObject, amount + 1);
    return $"¤ stars changed from {previous} to {amount}.";
  }
  private static string Move(ZNetView obj, Vector3 offset, string origin) {
    Actions.Move(obj, offset, origin);
    return $"¤ moved {offset.ToString("F1")} from {origin}.";
  }
  private static string ResetRotation(ZNetView obj) {
    Actions.ResetRotation(obj);
    return $"¤ rotation reseted.";
  }
  private static string Rotate(ZNetView obj, Vector3 rotation, string origin) {
    Actions.Rotate(obj, rotation, origin);
    return $"¤ rotated {rotation.ToString("F1")} from {origin}.";
  }
  private static string Scale(ZNetView obj, Vector3 scale) {
    Actions.Scale(obj, scale);
    return $"¤ scaled to {scale.ToString("F1")}.";
  }
  private static string SetBaby(Growup obj) {
    if (obj == null) return "Skipped: ¤ is not an offspring.";
    Actions.SetBaby(obj);
    return "¤ growth disabled.";
  }
  private static string MakeTame(Character obj) {
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetTame(obj, true);
    return "¤ made tame.";
  }
  private static string MakeWild(Character obj) {
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetTame(obj, false);
    return "¤ made wild.";
  }
  private static string MakeSleep(MonsterAI obj) {
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetSleeping(obj, true);
    return "¤ made to sleep.";
  }
  private static string SetVisual(ItemStand obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not an item stand.";
    Actions.SetVisual(obj, item);
    return $"Visual of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetHelmet(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.Helmet, item);
    return $"Helmet of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLeftHand(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.HandLeft, item);
    return $"Left hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetRightHand(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.HandRight, item);
    return $"Right hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetChest(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.Chest, item);
    return $"Chest of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetShoulder(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.Shoulder, item);
    return $"Shoulder of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLegs(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.Legs, item);
    return $"Legs of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetModel(Character obj, int index) {
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetModel(obj, index);
    return $"Model of ¤ set to {index}.";
  }
  private static string SetUtility(Character obj, Item item) {
    if (item == null) return "Skipped: Invalid item.";
    if (obj == null) return "Skipped: ¤ is not a creature.";
    Actions.SetVisual(obj, VisSlot.Utility, item);
    return $"Utility item of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string GetInfo(ZNetView obj) {
    List<string> info = new();
    info.Add("Id: ¤");
    info.Add("Pos: " + obj.transform.position.ToString("F1"));
    info.Add("Rot: " + obj.transform.rotation.eulerAngles.ToString("F1"));
    if (obj.m_syncInitialScale)
      info.Add("Scale: " + obj.transform.localScale.ToString("F1"));
    var character = obj.GetComponent<Character>();
    if (character) {
      info.Add("Health: " + character.GetHealth().ToString("F0") + " / " + character.GetMaxHealth());
      info.Add("Stars: " + (character.GetLevel() - 1));
      info.Add("Tamed: " + (character.IsTamed() ? "Yes" : "No"));
      var growUp = obj.GetComponent<Growup>();
      if (growUp)
        info.Add("Baby: " + (growUp.m_baseAI.GetTimeSinceSpawned().TotalSeconds < 0 ? "Yes" : "No"));
    } else {
      var health = Actions.GetHealth(obj);
      if (health > -1f)
        info.Add("Health: " + health.ToString("F0"));
    }
    var equipment = obj.GetComponent<VisEquipment>();
    if (equipment) {
      if (equipment.m_rightItem != "")
        info.Add("Right hand: " + equipment.m_rightItem);
      if (equipment.m_leftItem != "")
        info.Add("Left hand: " + equipment.m_leftItem);
      if (equipment.m_helmetItem != "")
        info.Add("Helmet: " + equipment.m_helmetItem);
      if (equipment.m_shoulderItem != "")
        info.Add("Shoulders: " + equipment.m_shoulderItem);
      if (equipment.m_chestItem != "")
        info.Add("Chest: " + equipment.m_chestItem);
      if (equipment.m_legItem != "")
        info.Add("Legs: " + equipment.m_legItem);
      if (equipment.m_utilityItem != "")
        info.Add("Utility: " + equipment.m_utilityItem);
    }
    var piece = obj.GetComponent<Piece>();
    if (piece) {
      info.Add("Creator ID: " + piece.GetCreator());
    }
    return string.Join(", ", info);
  }
}
