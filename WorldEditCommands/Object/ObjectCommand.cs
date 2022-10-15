using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;

///<summary>Needed to keep track of edited zdos.</summary>
public class EditInfo {
  public EditInfo(ZDO zdo, bool refresh = false) {
    From = zdo.Clone();
    To = zdo;
    Refresh = refresh;
  }
  public EditData ToData() => new EditData(From, To, Refresh);
  ZDO From;
  ZDO To;
  public bool Refresh;
}
public class ObjectCommand {
  public static System.Random Random = new();
  public static bool Roll(float value) {
    if (value >= 1f) return true;
    return Random.NextDouble() < value;
  }
  public const string Name = "object";
  private static Dictionary<ZDOID, EditInfo> EditedInfo = new();
  private static void AddData(ZNetView view, bool refresh = false) {
    var zdo = view.GetZDO();
    if (EditedInfo.TryGetValue(zdo.m_uid, out var info)) {
      if (refresh) info.Refresh = refresh;
      return;
    }
    EditedInfo[zdo.m_uid] = new EditInfo(zdo, refresh);
  }
  private static void Execute(Terminal context, ObjectParameters pars, IEnumerable<string> operations, ZNetView[] views) {
    var scene = ZNetScene.instance;
    Dictionary<ZDOID, long> oldOwner = new();
    views = views.Where(view => {
      if (!view || !view.GetZDO().IsValid()) {
        context.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      if (!Roll(pars.Chance)) {
        context.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      return true;
    }).ToArray();
    List<ZDO> removed = new();
    EditedInfo.Clear();
    foreach (var view in views) {
      oldOwner.Add(view.GetZDO().m_uid, view.GetZDO().m_owner);
      view.ClaimOwnership();
    }
    var count = views.Count();
    foreach (var operation in operations) {
      foreach (var view in views) {
        if (!view) continue;
        var output = "";
        var name = Utils.GetPrefabName(view.gameObject);
        if (operation == "durability" || operation == "health")
          output = ChangeHealth(view, Helper.RandomValue(pars.Health));
        if (operation == "stars" || operation == "level")
          output = SetStars(view, Helper.RandomValue(pars.Level) - 1);
        if (operation == "fuel" && pars.Fuel != null)
          output = SetFuel(view, Helper.RandomValue(pars.Fuel));
        if (operation == "creator")
          output = SetCreator(view, pars.Creator);
        if (operation == "fuel" && pars.Fuel == null)
          output = PrintFuel(view);
        if (operation == "tame")
          output = MakeTame(view);
        if (operation == "wild")
          output = MakeWild(view);
        if (operation == "baby")
          output = SetBaby(view);
        if (operation == "component")
          output = Component(view, pars.Component);
        if (operation == "effect")
          output = Effect(view, pars.Effect);
        if (operation == "weather")
          output = Weather(view, pars.Weather);
        if (operation == "event")
          output = Event(view, pars.Event);
        if (operation == "status")
          output = Status(view, pars.Status);
        if (operation == "respawn")
          output = Respawn(view);
        if (operation == "info")
          output = GetInfo(view, pars.Info);
        if (operation == "sleep")
          output = MakeSleep(view);
        if (operation == "visual")
          output = SetVisual(view, pars.Visual);
        if (operation == "model")
          output = SetModel(view, Helper.RandomValue(pars.Model));
        if (operation == "helmet")
          output = SetHelmet(view, pars.Helmet);
        if (operation == "left_hand")
          output = SetLeftHand(view, pars.LeftHand);
        if (operation == "right_hand")
          output = SetRightHand(view, pars.RightHand);
        if (operation == "chest")
          output = SetChest(view, pars.Chest);
        if (operation == "shoulders")
          output = SetShoulder(view, pars.Shoulders);
        if (operation == "legs")
          output = SetLegs(view, pars.Legs);
        if (operation == "utility")
          output = SetUtility(view, pars.Utility);
        if (operation == "prefab")
          output = SetPrefab(view, pars.Prefab);
        if (operation == "collision")
          output = SetCollision(view, pars.Collision);
        if (operation == "interact")
          output = SetInteract(view, pars.Interact);
        if (operation == "show")
          output = SetRender(view, pars.Show);
        if (operation == "wear")
          output = SetWear(view, pars.Wear);
        if (operation == "fall")
          output = SetFall(view, pars.Fall);
        if (operation == "growth")
          output = SetGrowth(view, pars.Growth);
        if (operation == "move")
          output = Move(view, Helper.RandomValue(pars.Offset), pars.Origin);
        if (operation == "mirror" && pars.Center.HasValue)
          output = Mirror(view, pars.Center.Value);
        if (operation == "rotate") {
          if (pars.ResetRotation)
            output = ResetRotation(view);
          else
            output = Rotate(view, Helper.RandomValue(pars.Rotation), pars.Origin, pars.Center);
        }
        if (operation == "scale")
          output = Scale(view, Helper.RandomValue(pars.Scale));
        if (operation == "remove") {
          removed.Add(view.GetZDO().Clone());
          Actions.RemoveZDO(view.GetZDO());
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
    if (removed.Count > 0) {
      UndoRemove undo = new(removed);
      UndoManager.Add(undo);
    }
    if (EditedInfo.Count > 0) {
      UndoEdit undo = new(EditedInfo.Select(info => info.Value.ToData()));
      UndoManager.Add(undo);
    }
  }
  public ObjectCommand() {
    ObjectAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Modifies the selected object(s).", null, autoComplete.NamedParameters);
    Helper.Command(Name, description, (args) => {
      ObjectParameters pars = new(args);
      if (pars.Operations.Contains("guide")) {
        Ruler.Create(pars.ToRuler());
        return;
      }
      ZNetView[] views;
      if (pars.Connect) {
        var view = Selector.GetHovered(50f, null);
        if (view == null) return;
        views = Selector.GetConnected(view);
      } else if (pars.Radius.HasValue) {
        views = Selector.GetNearby(pars.Id, pars.ObjectType, position => Selector.Within(position, pars.Center ?? pars.From, pars.Radius.Value, pars.Height));
      } else if (pars.Width.HasValue && pars.Depth.HasValue) {
        views = Selector.GetNearby(pars.Id, pars.ObjectType, position => Selector.Within(position, pars.Center ?? pars.From, pars.Angle, pars.Width.Value, pars.Depth.Value, pars.Height));
      } else {
        var view = Selector.GetHovered(50f, null);
        if (view == null) return;
        if (!Selector.GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab())) {
          Helper.AddMessage(args.Context, $"Skipped: {view.name} has invalid id.");
          return;
        }
        views = new[] { view };
      }
      Execute(args.Context, pars, pars.Operations, views);

    }, () => autoComplete.NamedParameters);
  }

  private static string ChangeHealth(ZNetView obj, float amount) {
    if (!obj.GetComponent<Character>() && !obj.GetComponent<WearNTear>() && !obj.GetComponent<TreeLog>() && !obj.GetComponent<Destructible>() && !obj.GetComponent<TreeBase>())
      return "Skipped: ¤ is not a creature or a destructible.";
    AddData(obj);
    var previous = Actions.SetHealth(obj.gameObject, amount);
    var amountStr = amount == 0f ? "default" : amount.ToString("F0");
    return $"¤ health changed from {previous.ToString("F0")} to {amountStr}.";
  }
  private static string SetStars(ZNetView view, int amount) {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    var previous = obj.GetLevel() - 1;
    Actions.SetLevel(obj, amount + 1);
    return $"¤ stars changed from {previous} to {amount}.";
  }
  private static string PrintFuel(ZNetView view) {
    var obj = view.GetComponent<Fireplace>();
    if (!obj) return "Skipped: ¤ is not a fireplace.";
    var amount = view.GetZDO().GetFloat("fuel", 0f);
    return $"¤ has {amount} fuel.";
  }
  private static string SetFuel(ZNetView view, float amount) {
    var smelter = view.GetComponent<Smelter>();
    if (!view.GetComponent<Fireplace>() && (!smelter || !smelter.m_fuelItem)) return "Skipped: ¤ is not a fireplace or smelter.";
    AddData(view);
    var previous = view.GetZDO().GetFloat("fuel", 0f);
    Actions.SetFuel(view, amount);
    return $"¤ fuel changed from {previous} to {amount}.";
  }
  private static string Move(ZNetView view, Vector3 offset, string origin) {
    AddData(view);
    Actions.Move(view, offset, origin);
    return $"¤ moved {offset.ToString("F1")} from the {origin}.";
  }
  private static string ResetRotation(ZNetView view) {
    AddData(view);
    Actions.ResetRotation(view);
    return $"¤ rotation reseted.";
  }
  private static string Mirror(ZNetView view, Vector3 center) {
    AddData(view);
    Actions.Mirror(view, center);
    return $"¤ mirrored.";
  }
  private static string Rotate(ZNetView view, Vector3 rotation, string origin, Vector3? center = null) {
    AddData(view);
    Actions.Rotate(view, rotation, origin, center);
    return $"¤ rotated {rotation.ToString("F1")} from the {origin}.";
  }
  private static string Scale(ZNetView view, Vector3 scale) {
    AddData(view);
    Actions.Scale(view, scale);
    return $"¤ scaled to {scale.ToString("F1")}.";
  }
  private static string SetBaby(ZNetView view) {
    var obj = view.GetComponent<Growup>();
    if (!obj) return "Skipped: ¤ is not an offspring.";
    AddData(view);
    Actions.SetBaby(obj);
    return "¤ growth disabled.";
  }
  private static string Respawn(ZNetView view) {
    if (!Actions.CanRespawn(view.gameObject))
      return "Skipped: ¤ is not a loot container, pickable or spawn point.";
    AddData(view);
    Actions.Respawn(view.gameObject);
    return "¤ respawned.";
  }
  private const string DEFAULT = "default";
  private static string Component(ZNetView view, string value) {
    AddData(view, true);
    Actions.SetComponent(view, value);
    return $"¤ component set to {(value == "" ? DEFAULT : value)}.";
  }
  private static string Effect(ZNetView view, string value) {
    AddData(view, true);
    Actions.SetEffect(view, value);
    return $"¤ effect set to {value}.";
  }
  private static string Weather(ZNetView view, string value) {
    AddData(view, true);
    Actions.SetWeather(view, value);
    return $"¤ weather set to {value}.";
  }
  private static string Event(ZNetView view, string value) {
    AddData(view, true);
    Actions.SetEvent(view, value);
    return $"¤ event set to {value}.";
  }
  private static string Status(ZNetView view, string value) {
    AddData(view, true);
    Actions.SetStatus(view, value);
    return $"¤ status set to {value}.";
  }
  private static string MakeTame(ZNetView view) {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetTame(obj, true);
    return "¤ made tame.";
  }
  private static string MakeWild(ZNetView view) {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetTame(obj, false);
    return "¤ made wild.";
  }
  private static string MakeSleep(ZNetView view) {
    var obj = view.GetComponent<MonsterAI>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetSleeping(obj, true);
    return "¤ made to sleep.";
  }
  private static string SetCreator(ZNetView view, long creator) {
    var obj = view.GetComponent<Piece>();
    if (!obj) return "Skipped: ¤ is not a piece.";
    AddData(view);
    var previous = Actions.SetCreator(obj, creator);
    return $"Creator of ¤ set from {previous} to {creator}.";
  }
  private static string SetPrefab(ZNetView view, string prefab) {
    AddData(view, true);
    if (Actions.SetPrefab(view, prefab))
      return $"Prefab of ¤ set to {prefab}.";
    return $"Error: Prefab of ¤ was not set to {prefab}. Probably invalid prefab name.";
  }
  private static string SetWear(ZNetView view, Wear wear) {
    var obj = view.GetComponent<WearNTear>();
    if (!obj) return "Skipped: ¤ is not a structure.";
    AddData(view);
    Actions.SetWear(obj, wear);
    return $"Wear of ¤ set to {wear}.";
  }
  private static string SetFall(ZNetView view, Fall fall) {
    var obj = view.GetComponent<StaticPhysics>();
    if (!obj) return "Skipped: ¤ is not a static object.";
    AddData(view, true);
    Actions.SetFall(obj, fall);
    return $"Fall of ¤ set to {fall}.";
  }
  private static string SetCollision(ZNetView view, bool? value) {
    Actions.SetCollision(view, value);
    AddData(view, true);
    return $"Collision of ¤ set to {(value.HasValue ? value.Value : DEFAULT)}.";
  }
  private static string SetInteract(ZNetView view, bool? value) {
    Actions.SetInteract(view, value);
    AddData(view);
    return $"Interact of ¤ set to {(value.HasValue ? value.Value : DEFAULT)}.";
  }
  private static string SetRender(ZNetView view, bool? value) {
    Actions.SetRender(view, value);
    AddData(view, true);
    return $"Render of ¤ set to {(value.HasValue ? value.Value : DEFAULT)}.";
  }
  private static string SetGrowth(ZNetView view, Growth growth) {
    var obj = view.GetComponent<Plant>();
    if (!obj) return "Skipped: ¤ is not a plant.";
    AddData(view);
    Actions.SetGrowth(obj, growth);
    return $"Growth of ¤ set to {growth}.";
  }
  private static string SetVisual(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<ItemStand>();
    if (!obj) return "Skipped: ¤ is not an item stand.";
    AddData(view);
    Actions.SetVisual(obj, item);
    return $"Visual of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetHelmet(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Helmet, item);
    return $"Helmet of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLeftHand(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.HandLeft, item);
    return $"Left hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetRightHand(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.HandRight, item);
    return $"Right hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetChest(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Chest, item);
    return $"Chest of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetShoulder(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Shoulder, item);
    return $"Shoulder of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLegs(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Legs, item);
    return $"Legs of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetModel(ZNetView view, int index) {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    Actions.SetModel(obj, index);
    return $"Model of ¤ set to {index}.";
  }
  private static string SetUtility(ZNetView view, Item? item) {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Utility, item);
    return $"Utility item of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string GetInfo(ZNetView obj) {
    List<string> info = new();
    info.Add("Id: ¤");
    info.Add("Pos: " + Helper.PrintVectorXZY(obj.transform.position));
    info.Add("Rot: " + Helper.PrintAngleYXZ(obj.transform.rotation));
    if (obj.m_syncInitialScale)
      info.Add("Scale: " + Helper.PrintVectorXZY(obj.transform.localScale));
    var character = obj.GetComponent<Character>();
    if (character) {
      var health = character.GetHealth();
      if (health > 1E17)
        info.Add("Health: Infinite");
      else
        info.Add("Health: " + health.ToString("F0") + " / " + character.GetMaxHealth());
      info.Add("Faction: " + character.m_faction);
      info.Add("Stars: " + (character.GetLevel() - 1));
      info.Add("Tamed: " + (character.IsTamed() ? "Yes" : "No"));
      var growUp = obj.GetComponent<Growup>();
      if (growUp)
        info.Add("Baby: " + (growUp.m_baseAI.GetTimeSinceSpawned().TotalSeconds < 0 ? "Yes" : "No"));
    } else {
      var health = Actions.GetHealth(obj);
      if (health > 1E17)
        info.Add("Health: Infinite");
      else if (health > -1f)
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

  private static string GetInfo(ZNetView obj, string data) {
    if (data == "") return GetInfo(obj);
    List<string> info = new();
    var hash = data.GetStableHashCode();
    var zdo = obj.GetZDO();
    info.Add("Id: ¤");
    info.Add($"Owner: {zdo.m_owner}");
    info.Add($"Revision: {zdo.m_dataRevision} + {zdo.m_ownerRevision}");
    if (zdo.m_vec3?.ContainsKey(hash) == true)
      info.Add($"{data}: {Helper.PrintVectorXZY(zdo.m_vec3[hash])}");
    if (zdo.m_quats?.ContainsKey(hash) == true)
      info.Add($"{data}: {Helper.PrintAngleYXZ(zdo.m_quats[hash])}");
    if (zdo.m_longs?.ContainsKey(hash) == true)
      info.Add($"{data}: {zdo.m_longs[hash]}");
    if (zdo.m_strings?.ContainsKey(hash) == true)
      info.Add($"{data}: {zdo.m_strings[hash]}");
    if (zdo.m_ints?.ContainsKey(hash) == true)
      info.Add($"{data}: {zdo.m_ints[hash]}");
    if (zdo.m_floats?.ContainsKey(hash) == true)
      info.Add($"{data}: {zdo.m_floats[hash].ToString("F1")}");
    if (info.Count < 4)
      info.Add($"{data}: No data!");
    return string.Join(", ", info);
  }
}
