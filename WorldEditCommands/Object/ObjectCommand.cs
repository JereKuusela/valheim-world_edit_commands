using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public enum ObjectType {
  All,
  Character,
  Structure
}

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
  public static Dictionary<ZDOID, EditInfo> EditedInfo = new();
  private static bool IsIncluded(string id, string name) {
    if (id.StartsWith("*", StringComparison.Ordinal) && id.EndsWith("*", StringComparison.Ordinal))
      return name.Contains(id.Substring(1, id.Length - 3));
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
  private static float GetX(float x, float y, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * y;
  private static float GetY(float x, float y, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
  private static bool Within(Vector3 position, Vector3 center, float angle, float width, float depth, float height) {
    var dx = position.x - center.x;
    var dz = position.z - center.z;
    var distanceX = GetX(dx, dz, angle);
    var distanceZ = GetY(dx, dz, angle);
    if (center.y - position.y > 1000f) return false;
    if (position.y - center.y > (height == 0f ? 1000f : height)) return false;
    if (Mathf.Abs(distanceX) > width) return false;
    if (Mathf.Abs(distanceZ) > depth) return false;
    return true;
  }
  private static bool Within(Vector3 position, Vector3 center, float radius, float height) {
    return Utils.DistanceXZ(position, center) <= radius && center.y - position.y < 1000f && position.y - center.y <= (height == 0f ? 1000f : height);
  }
  public static IEnumerable<ZDO> GetZDOs(string id, ObjectType type, Func<Vector3, bool> checker) {
    var codes = GetPrefabs(id);
    IEnumerable<ZDO> zdos = ZDOMan.instance.m_objectsByID.Values;
    var scene = ZNetScene.instance;
    zdos = zdos.Where(zdo => codes.Contains(zdo.GetPrefab()));
    if (type == ObjectType.Structure)
      zdos = zdos.Where(zdo => scene.GetPrefab(zdo.GetPrefab()).GetComponent<Piece>());
    if (type == ObjectType.Character)
      zdos = zdos.Where(zdo => scene.GetPrefab(zdo.GetPrefab()).GetComponent<Character>());
    return zdos.Where(zdo => checker(zdo.GetPosition()));
  }
  private static void AddData(ZNetView view, bool refresh = false) {
    var zdo = view.GetZDO();
    if (EditedInfo.TryGetValue(zdo.m_uid, out var info)) {
      if (refresh) info.Refresh = refresh;
      return;
    }
    EditedInfo[zdo.m_uid] = new EditInfo(zdo, refresh);
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
      if (!Roll(pars.Chance)) {
        context.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      return true;
    }).Select(zdo => scene.FindInstance(zdo)).ToArray();
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
        if (operation == "stars")
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
        if (operation == "respawn")
          output = Respawn(view);
        if (operation == "info")
          output = GetInfo(view);
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
      IEnumerable<ZDO> zdos;
      if (pars.Radius.HasValue) {
        zdos = GetZDOs(pars.Id, pars.ObjectType, position => Within(position, pars.Center ?? pars.From, pars.Radius.Value, pars.Height));
      } else if (pars.Width.HasValue && pars.Depth.HasValue) {
        zdos = GetZDOs(pars.Id, pars.ObjectType, position => Within(position, pars.Center ?? pars.From, pars.Angle, pars.Width.Value, pars.Depth.Value, pars.Height));
      } else {
        var view = Helper.GetHovered(args);
        if (view == null) return;
        if (!GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab())) {
          Helper.AddMessage(args.Context, $"Skipped: {view.name} has invalid id.");
          return;
        }
        zdos = new[] { view.GetZDO() };
      }
      Execute(args.Context, pars, pars.Operations, zdos);

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
    var obj = view.GetComponent<Fireplace>();
    if (!obj) return "Skipped: ¤ is not a fireplace.";
    AddData(view);
    var previous = view.GetZDO().GetFloat("fuel", 0f);
    Actions.SetFuel(obj, amount);
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
    return $"Prefab of ¤ set from {previous} to {creator}.";
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
  private static string SetCollision(ZNetView view, bool? value) {
    var result = Actions.SetCollision(view, value);
    AddData(view, true);
    return $"Collision of ¤ set to {result}.";
  }
  private static string SetInteract(ZNetView view, bool? value) {
    var result = Actions.SetInteract(view, value);
    AddData(view);
    return $"Interact of ¤ set to {result}.";
  }
  private static string SetRender(ZNetView view, bool? value) {
    var result = Actions.SetRender(view, value);
    AddData(view, true);
    return $"Render of ¤ set to {result}.";
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
