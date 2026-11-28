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
  public const string Name = "object";
  private static void Execute(Terminal context, ObjectParameters pars, IEnumerable<string> operations, ZNetView[] views)
  {
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
        var name = Utils.GetPrefabName(view.gameObject);
        var output = ApplyOperation(operation, pars, view);
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
    // Component fields are only read when the object is created.
    if (operations.Contains("field") || operations.Contains("f"))
    {
      foreach (var view in views)
      {
        if (view) Actions.Refresh(view);
      }
    }
  }
  private static string ApplyOperation(string operation, ObjectParameters pars, ZNetView view)
  {
    switch (operation)
    {
      case "durability":
      case "health":
        return ChangeHealth(view, Helper.RandomValue(pars.Health), pars.isHealthPercentage);
      case "damage":
        return Actions.Damage(view, Helper.RandomValue(pars.Damage));
      case "ammo":
        return Actions.Ammo(view, Helper.RandomValue(pars.Ammo));
      case "ammotype":
        return Actions.AmmoType(view, pars.AmmoType);
      case "stars":
      case "level":
        return SetStars(view, Helper.RandomValue(pars.Level!) - 1);
      case "fuel":
        return pars.Fuel != null ? SetFuel(view, Helper.RandomValue(pars.Fuel)) : PrintFuel(view);
      case "creator":
        return SetCreator(view, pars.Creator);
      case "tame":
        return MakeTame(view);
      case "wild":
        return MakeWild(view);
      case "baby":
        return SetBaby(view);
      case "respawn":
        return Respawn(view);
      case "info":
        return GetInfo(view);
      case "components":
        return GetComponents(view);
      case "sleep":
        return MakeSleep(view);
      case "visual":
        return SetVisual(view, pars.Visual);
      case "model":
        return SetModel(view, Helper.RandomValue(pars.Model));
      case "helmet":
        return SetHelmet(view, pars.Helmet);
      case "field":
      case "f":
        return SetFields(view, pars.Fields);
      case "left_hand":
        return SetLeftHand(view, pars.LeftHand);
      case "right_hand":
        return SetRightHand(view, pars.RightHand);
      case "chest":
        return SetChest(view, pars.Chest);
      case "shoulders":
        return SetShoulder(view, pars.Shoulders);
      case "legs":
        return SetLegs(view, pars.Legs);
      case "utility":
        return SetUtility(view, pars.Utility);
      case "prefab":
        return SetPrefab(view, pars.Prefab);
      case "copy":
        return CopyId(view);
      case "status" when pars.StatusName != null:
        return SetStatus(view, pars.StatusName, Helper.RandomValue(pars.StatusDuration), Helper.RandomValue(pars.StatusIntensity));
      case "move":
        return Move(view, Helper.RandomValue(pars.Offset), pars.Origin);
      case "mirror" when pars.Selection.Center.HasValue:
        return Mirror(view, pars.Selection.Center.Value);
      case "distant" when pars.Distant != null:
        return SetDistant(view, pars.Distant.Value);
      case "persist" when pars.Persist != null:
        return SetPersistent(view, pars.Persist.Value);
      case "rotate":
        return pars.ResetRotation ? ResetRotation(view) : Rotate(view, Helper.RandomValue(pars.Rotation), pars.Origin, pars.Selection.Center);
      case "scale":
        return Scale(view, Helper.RandomValue(pars.Scale));
      case "remove":
        UndoHelper.AddRemoveAction(view.GetZDO());
        Actions.RemoveZDO(view.GetZDO());
        return "Entity ¤ destroyed.";
      default:
        return "";
    }
  }
  public ObjectCommand()
  {
    ObjectAutoComplete autoComplete = new();
    Helper.Command(Name, "Modifies objects.", (args) =>
    {
      ObjectParameters pars = new(args);
      var views = pars.Selection.GetObjects(args.Context);
      UndoHelper.Run(() => Execute(args.Context, pars, pars.Operations, views));

    });
  }

  private static string ChangeHealth(ZNetView obj, float amount, bool isPercentage)
  {
    if (!obj.GetComponent<Character>() && !obj.GetComponent<WearNTear>() && !obj.GetComponent<TreeLog>() && !obj.GetComponent<Destructible>() && !obj.GetComponent<TreeBase>())
      return "Skipped: ¤ is not a creature or a destructible.";
    UndoHelper.AddEditAction(obj);
    var previous = Actions.SetHealth(obj.gameObject, amount, isPercentage);
    var amountStr = amount == 0f ? "default" : isPercentage ? $"{100 * amount:0.##} %" : amount.ToString("F0");
    return $"¤ health changed from {previous:F0} to {amountStr}.";
  }
  private static string SetStars(ZNetView view, int amount)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
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
    UndoHelper.AddEditAction(view);
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
    UndoHelper.AddEditAction(view);
    var previous = view.GetZDO().GetFloat("fuel", 0f);
    Actions.SetFuel(view, amount);
    return $"¤ fuel changed from {previous} to {amount}.";
  }
  private static string Move(ZNetView view, Vector3 offset, string origin)
  {
    UndoHelper.AddEditAction(view);
    Actions.Move(view, offset, origin);
    return $"¤ moved {offset:F1} from the {origin}.";
  }
  private static string ResetRotation(ZNetView view)
  {
    UndoHelper.AddEditAction(view);
    Actions.ResetRotation(view);
    return $"¤ rotation reseted.";
  }
  private static string Mirror(ZNetView view, Vector3 center)
  {
    UndoHelper.AddEditAction(view);
    Actions.Mirror(view, center);
    return $"¤ mirrored.";
  }
  private static string Rotate(ZNetView view, Vector3 rotation, string origin, Vector3? center = null)
  {
    UndoHelper.AddEditAction(view);
    Actions.Rotate(view, rotation, origin, center);
    return $"¤ rotated {rotation:F1} from the {origin}.";
  }
  private static string Scale(ZNetView view, Vector3 scale)
  {
    UndoHelper.AddEditAction(view);
    var tweaked = Actions.Scale(view, scale);
    var tweakStr = tweaked ? " (scaling enabled)" : "";
    return $"¤ scaled to {scale:F1}{tweakStr}.";
  }
  private static string SetDistant(ZNetView view, bool distant)
  {
    UndoHelper.AddEditAction(view);
    Actions.SetDistant(view.gameObject, distant);
    return distant ? "¤ made distant loaded." : "¤ made normally loaded.";
  }
  private static string SetPersistent(ZNetView view, bool persist)
  {
    UndoHelper.AddEditAction(view);
    Actions.SetPersistent(view.gameObject, persist);
    return persist ? "¤ made persistent." : "¤ made non-persistent.";
  }
  private static string SetBaby(ZNetView view)
  {
    var obj = view.GetComponent<Growup>();
    if (!obj) return "Skipped: ¤ is not an offspring.";
    UndoHelper.AddEditAction(view);
    Actions.SetBaby(obj);
    return "¤ growth disabled.";
  }
  private static string Respawn(ZNetView view)
  {
    if (!Actions.CanRespawn(view.gameObject))
      return "Skipped: ¤ is not a loot container, pickable or spawn point.";
    UndoHelper.AddEditAction(view);
    Actions.Respawn(view.gameObject);
    return "¤ respawned.";
  }
  private static string MakeTame(ZNetView view)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetTame(obj, true);
    return "¤ made tame.";
  }
  private static string MakeWild(ZNetView view)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetTame(obj, false);
    return "¤ made wild.";
  }
  private static string MakeSleep(ZNetView view)
  {
    var obj = view.GetComponent<MonsterAI>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetSleeping(obj, true);
    return "¤ made to sleep.";
  }
  private static string SetCreator(ZNetView view, long creator)
  {
    var obj = view.GetComponent<Piece>();
    if (!obj) return "Skipped: ¤ is not a piece.";
    UndoHelper.AddEditAction(view);
    var previous = Actions.SetCreator(obj, creator);
    return $"Creator of ¤ set from {previous} to {creator}.";
  }
  private static string SetPrefab(ZNetView view, string prefab)
  {
    UndoHelper.AddEditAction(view);
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
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, item);
    return $"Visual of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetHelmet(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, VisSlot.Helmet, item);
    return $"Helmet of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLeftHand(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, VisSlot.HandLeft, item);
    return $"Left hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetRightHand(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, VisSlot.HandRight, item);
    return $"Right hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetChest(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, VisSlot.Chest, item);
    return $"Chest of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetShoulder(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, VisSlot.Shoulder, item);
    return $"Shoulder of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLegs(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetVisual(obj, VisSlot.Legs, item);
    return $"Legs of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetModel(ZNetView view, int index)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
    Actions.SetModel(obj, index);
    return $"Model of ¤ set to {index}.";
  }
  private static string SetUtility(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    UndoHelper.AddEditAction(view);
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
      if (equipment.m_rightItem != 0)
        info.Add("Right hand: " + DataHelper.Name(equipment.m_rightItem));
      if (equipment.m_leftItem != 0)
        info.Add("Left hand: " + DataHelper.Name(equipment.m_leftItem));
      if (equipment.m_helmetItem != 0)
        info.Add("Helmet: " + DataHelper.Name(equipment.m_helmetItem));
      if (equipment.m_shoulderItem != 0)
        info.Add("Shoulders: " + DataHelper.Name(equipment.m_shoulderItem));
      if (equipment.m_chestItem != 0)
        info.Add("Chest: " + DataHelper.Name(equipment.m_chestItem));
      if (equipment.m_legItem != 0)
        info.Add("Legs: " + DataHelper.Name(equipment.m_legItem));
      if (equipment.m_utilityItem != 0)
        info.Add("Utility: " + DataHelper.Name(equipment.m_utilityItem));
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
