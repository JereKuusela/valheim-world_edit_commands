using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;

public class ObjectCommand
{
  public static System.Random Random = new();
  public static bool Roll(float value)
  {
    if (value >= 1f) return true;
    return Random.NextDouble() < value;
  }
  public const string Name = "object";
  // Dictionary to only add undo for edited objects.
  private static readonly Dictionary<ZDOID, EditData> EditedInfo = [];
  private static void AddUndo(ZNetView view)
  {
    var zdo = view.GetZDO();
    if (zdo.GetPrefab() == Hash.Player) return;
    if (EditedInfo.ContainsKey(zdo.m_uid)) return;
    EditedInfo[zdo.m_uid] = new EditData(zdo);
  }
  private static void Execute(Terminal context, ObjectParameters pars, IEnumerable<string> operations, ZNetView[] views)
  {
    var scene = ZNetScene.instance;
    DataEntry? matchData = pars.Match == "" ? null : DataHelper.Get(pars.Match);
    DataEntry? unmatchData = pars.Unmatch == "" ? null : DataHelper.Get(pars.Unmatch);
    views = views.Where(view =>
    {
      if (!view || !view.GetZDO().IsValid())
      {
        context.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      if (matchData != null && !matchData.Match(pars.DataParameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), view.GetZDO()))
      {
        context.AddString($"Skipped: {view.name} not matching filter.");
        return false;
      }
      if (unmatchData != null && !unmatchData.Unmatch(pars.DataParameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), view.GetZDO()))
      {
        context.AddString($"Skipped: {view.name} matching filter.");
        return false;
      }
      if (!Roll(pars.Chance))
      {
        context.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      return true;
    }).ToArray();
    List<FakeZDO> removed = [];
    EditedInfo.Clear();
    foreach (var view in views)
    {
      var zdo = view.GetZDO();
      if (zdo.GetPrefab() != Hash.Player)
        view.ClaimOwnership();
    }
    var count = views.Count();
    foreach (var operation in operations)
    {
      foreach (var view in views)
      {
        if (!view) continue;
        var output = "";
        var name = Utils.GetPrefabName(view.gameObject);
        if (operation == "durability" || operation == "health")
          output = ChangeHealth(view, Helper.RandomValue(pars.Health), pars.isHealthPercentage);
        if (operation == "damage")
          output = Actions.Damage(view, Helper.RandomValue(pars.Damage));
        if (operation == "ammo")
          output = Actions.Ammo(view, Helper.RandomValue(pars.Ammo));
        if (operation == "ammotype")
          output = Actions.AmmoType(view, pars.AmmoType);
        if (operation == "stars" || operation == "level")
          output = SetStars(view, Helper.RandomValue(pars.Level!) - 1);
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
        if (operation == "components")
          output = GetComponents(view);
        if (operation == "sleep")
          output = MakeSleep(view);
        if (operation == "visual")
          output = SetVisual(view, pars.Visual);
        if (operation == "model")
          output = SetModel(view, Helper.RandomValue(pars.Model));
        if (operation == "helmet")
          output = SetHelmet(view, pars.Helmet);
        if (operation == "field" || operation == "f")
          SetFields(view, pars.Fields);
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
        if (operation == "copy")
          output = CopyId(view);
        if (operation == "status" && pars.StatusName != null)
          output = SetStatus(view, pars.StatusName, Helper.RandomValue(pars.StatusDuration), Helper.RandomValue(pars.StatusIntensity));
        if (operation == "move")
          output = Move(view, Helper.RandomValue(pars.Offset), pars.Origin);
        if (operation == "mirror" && pars.Center.HasValue)
          output = Mirror(view, pars.Center.Value);
        if (operation == "rotate")
        {
          if (pars.ResetRotation)
            output = ResetRotation(view);
          else
            output = Rotate(view, Helper.RandomValue(pars.Rotation), pars.Origin, pars.Center);
        }
        if (operation == "scale")
          output = Scale(view, Helper.RandomValue(pars.Scale));
        if (operation == "remove")
        {
          removed.Add(new(view.GetZDO()));
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
    var moved = operations.Contains("move") || operations.Contains("scale") || operations.Contains("rotate") || operations.Contains("mirror");
    if (moved)
    {
      foreach (var view in views)
      {
        if (view && view.TryGetComponent<WearNTear>(out var wearNTear))
          wearNTear.m_colliders = null; // Forces the next support check to refresh collider positions.
      }
    }
    if (removed.Count() > 0)
    {
      UndoRemove undo = new(removed);
      UndoManager.Add(undo);
    }
    if (EditedInfo.Count() > 0)
    {
      foreach (var info in EditedInfo)
        info.Value.Update();
      UndoManager.Add(new UndoEdit(EditedInfo.Select(kvp => kvp.Value)));
    }
  }
  public ObjectCommand()
  {
    ObjectAutoComplete autoComplete = new();
    Helper.Command(Name, "Modifies objects.", (args) =>
    {
      ObjectParameters pars = new(args);
      ZNetView[] views;
      if (pars.Connect)
      {
        var view = Selector.GetHovered(50f, pars.IncludedIds, pars.Components, pars.ExcludedIds);
        if (view == null) return;
        views = Selector.GetConnected(view, pars.IncludedIds, pars.ExcludedIds);
      }
      else if (pars.Radius != null)
      {
        views = Selector.GetNearby(pars.IncludedIds, pars.Components, pars.ExcludedIds, pars.Center ?? pars.From, pars.Radius, pars.Height);
      }
      else if (pars.Width != null && pars.Depth != null)
      {
        views = Selector.GetNearby(pars.IncludedIds, pars.Components, pars.ExcludedIds, pars.Center ?? pars.From, pars.Angle, pars.Width, pars.Depth, pars.Height);
      }
      else
      {
        var view = Selector.GetHovered(50f, pars.IncludedIds, pars.ExcludedIds);
        if (view == null) return;
        if (!Selector.GetPrefabs(pars.IncludedIds).Contains(view.GetZDO().GetPrefab()))
        {
          Helper.AddMessage(args.Context, $"Skipped: {view.name} has invalid id.");
          return;
        }
        views = [view];
      }
      Execute(args.Context, pars, pars.Operations, views);

    });
  }

  private static string ChangeHealth(ZNetView obj, float amount, bool isPercentage)
  {
    if (!obj.GetComponent<Character>() && !obj.GetComponent<WearNTear>() && !obj.GetComponent<TreeLog>() && !obj.GetComponent<Destructible>() && !obj.GetComponent<TreeBase>())
      return "Skipped: ¤ is not a creature or a destructible.";
    AddUndo(obj);
    var previous = Actions.SetHealth(obj.gameObject, amount, isPercentage);
    var amountStr = amount == 0f ? "default" : isPercentage ? $"{100 * amount:0.##} %" : amount.ToString("F0");
    return $"¤ health changed from {previous:F0} to {amountStr}.";
  }
  private static string SetStars(ZNetView view, int amount)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    var previous = obj.GetLevel() - 1;
    Actions.SetLevel(obj, amount + 1);
    return $"¤ stars changed from {previous} to {amount}.";
  }
  private static string PrintFuel(ZNetView view)
  {
    var hasFuel = view.TryGetComponent(out Smelter smelter) && smelter.m_fuelItem != null;
    hasFuel |= view.TryGetComponent(out Fireplace fireplace) && fireplace.m_fuelItem != null;
    hasFuel |= view.TryGetComponent(out CookingStation cs) && cs.m_fuelItem != null;
    if (!hasFuel) return "Skipped: ¤ doesn't use fuel.";
    var amount = view.GetZDO().GetFloat("fuel", 0f);
    return $"¤ has {amount} fuel.";
  }
  private static string SetFields(ZNetView view, Dictionary<string, object> fields)
  {
    AddUndo(view);
    var added = Actions.SetFields(view, fields);
    if (added == 0) return "Skipped: ¤ doesn't have valid components.";
    if (added == fields.Count) return $"¤ {fields.Count} fields set.";
    return $"¤ {added} of {fields.Count} fields set.";
  }
  private static string SetFuel(ZNetView view, float amount)
  {
    var hasFuel = view.TryGetComponent(out Smelter smelter) && smelter.m_fuelItem != null;
    hasFuel |= view.TryGetComponent(out Fireplace fireplace) && fireplace.m_fuelItem != null;
    hasFuel |= view.TryGetComponent(out CookingStation cs) && cs.m_fuelItem != null;
    if (!hasFuel) return "Skipped: ¤ doesn't use fuel.";
    AddUndo(view);
    var previous = view.GetZDO().GetFloat("fuel", 0f);
    Actions.SetFuel(view, amount);
    return $"¤ fuel changed from {previous} to {amount}.";
  }
  private static string Move(ZNetView view, Vector3 offset, string origin)
  {
    AddUndo(view);
    Actions.Move(view, offset, origin);
    return $"¤ moved {offset:F1} from the {origin}.";
  }
  private static string ResetRotation(ZNetView view)
  {
    AddUndo(view);
    Actions.ResetRotation(view);
    return $"¤ rotation reseted.";
  }
  private static string Mirror(ZNetView view, Vector3 center)
  {
    AddUndo(view);
    Actions.Mirror(view, center);
    return $"¤ mirrored.";
  }
  private static string Rotate(ZNetView view, Vector3 rotation, string origin, Vector3? center = null)
  {
    AddUndo(view);
    Actions.Rotate(view, rotation, origin, center);
    return $"¤ rotated {rotation:F1} from the {origin}.";
  }
  private static string Scale(ZNetView view, Vector3 scale)
  {
    AddUndo(view);
    var tweaked = Actions.Scale(view, scale);
    var tweakStr = tweaked ? " (scaling enabled)" : "";
    return $"¤ scaled to {scale:F1}{tweakStr}.";
  }
  private static string SetBaby(ZNetView view)
  {
    var obj = view.GetComponent<Growup>();
    if (!obj) return "Skipped: ¤ is not an offspring.";
    AddUndo(view);
    Actions.SetBaby(obj);
    return "¤ growth disabled.";
  }
  private static string Respawn(ZNetView view)
  {
    if (!Actions.CanRespawn(view.gameObject))
      return "Skipped: ¤ is not a loot container, pickable or spawn point.";
    AddUndo(view);
    Actions.Respawn(view.gameObject);
    return "¤ respawned.";
  }
  private static string MakeTame(ZNetView view)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetTame(obj, true);
    return "¤ made tame.";
  }
  private static string MakeWild(ZNetView view)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetTame(obj, false);
    return "¤ made wild.";
  }
  private static string MakeSleep(ZNetView view)
  {
    var obj = view.GetComponent<MonsterAI>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetSleeping(obj, true);
    return "¤ made to sleep.";
  }
  private static string SetCreator(ZNetView view, long creator)
  {
    var obj = view.GetComponent<Piece>();
    if (!obj) return "Skipped: ¤ is not a piece.";
    AddUndo(view);
    var previous = Actions.SetCreator(obj, creator);
    return $"Creator of ¤ set from {previous} to {creator}.";
  }
  private static string SetPrefab(ZNetView view, string prefab)
  {
    AddUndo(view);
    if (Actions.SetPrefab(view, prefab))
      return $"Prefab of ¤ set to {prefab}.";
    return $"Error: Prefab of ¤ was not set to {prefab}. Probably invalid prefab name.";
  }
  private static string CopyId(ZNetView view)
  {
    var basePrefab = ZNetScene.instance.GetPrefab(view.GetZDO().GetPrefab());
    GUIUtility.systemCopyBuffer = basePrefab.name;
    return $"Copied id of ¤.";
  }
  private static string SetVisual(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<ItemStand>();
    if (!obj) return "Skipped: ¤ is not an item stand.";
    AddUndo(view);
    Actions.SetVisual(obj, item);
    return $"Visual of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetHelmet(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.Helmet, item);
    return $"Helmet of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLeftHand(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.HandLeft, item);
    return $"Left hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetRightHand(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.HandRight, item);
    return $"Right hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetChest(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.Chest, item);
    return $"Chest of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetShoulder(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.Shoulder, item);
    return $"Shoulder of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLegs(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.Legs, item);
    return $"Legs of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetModel(ZNetView view, int index)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    Actions.SetModel(obj, index);
    return $"Model of ¤ set to {index}.";
  }
  private static string SetUtility(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddUndo(view);
    Actions.SetVisual(obj, VisSlot.Utility, item);
    return $"Utility item of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetStatus(ZNetView obj, string name, float duration, float intensity)
  {
    if (!obj.TryGetComponent<Character>(out var creature)) return "Skipped: ¤ is not a creature.";
    obj.ClaimOwnership();
    var hash = name.GetHashCode();
    creature.GetSEMan()?.AddStatusEffect(hash, true);
    var effect = creature.GetSEMan()?.GetStatusEffect(hash);
    if (effect == null) return $"Failed to set status of ¤ to {name}";
    effect.m_ttl = duration;
    if (effect is SE_Shield shield)
      shield.m_absorbDamage = intensity;
    if (effect is SE_Burning burning)
    {
      if (name == "Burning")
      {
        burning.m_fireDamageLeft = 0;
        burning.AddFireDamage(intensity);
      }
      else
      {
        burning.m_spiritDamageLeft = 0;
        burning.AddSpiritDamage(intensity);
      }
    }
    if (effect is SE_Poison poison)
    {
      poison.m_damageLeft = intensity;
      poison.m_damagePerHit = intensity / effect.m_ttl * poison.m_damageInterval;
    }
    return $"Status of ¤ set to {name}";
  }
  private static string GetInfo(ZNetView obj)
  {
    List<string> info = [
      "Id: ¤",
      "Pos: " + Helper.PrintVectorXZY(obj.transform.position),
      "Rot: " + Helper.PrintAngleYXZ(obj.transform.rotation)
    ];
    if (obj.m_syncInitialScale)
      info.Add("Scale: " + Helper.PrintVectorXZY(obj.transform.localScale));
    var character = obj.GetComponent<Character>();
    if (character)
    {
      var health = character.GetHealth();
      if (health < 0f || health > 1E17f)
        info.Add("Health: Infinite");
      else
        info.Add("Health: " + health.ToString("F0") + " / " + character.GetMaxHealth());
      info.Add("Faction: " + character.m_faction);
      info.Add("Stars: " + (character.GetLevel() - 1));
      info.Add("Tamed: " + (character.IsTamed() ? "Yes" : "No"));
      var growUp = obj.GetComponent<Growup>();
      if (growUp)
        info.Add("Baby: " + (growUp.m_baseAI.GetTimeSinceSpawned().TotalSeconds < 0 ? "Yes" : "No"));
    }
    else
    {
      var health = Actions.GetHealth(obj);
      if (health < 0f || health > 1E17f)
        info.Add("Health: Infinite");
      else
        info.Add("Health: " + health.ToString("F0"));
    }
    var equipment = obj.GetComponent<VisEquipment>();
    if (equipment)
    {
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
    if (piece)
    {
      info.Add("Creator ID: " + piece.GetCreator());
    }
    return string.Join(", ", info);
  }
  private static string GetComponents(ZNetView obj) => string.Join(", ", ComponentInfo.Get(obj));
}
