using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;

///<summary>Needed to keep track of edited zdos.</summary>
public class EditInfo
{
  public EditInfo(ZDO zdo, bool refresh = false)
  {
    From = zdo.Clone();
    To = zdo;
    Refresh = refresh;
  }
  public EditData ToData() => new EditData(From, To, Refresh);
  ZDO From;
  ZDO To;
  public bool Refresh;
}
public class ObjectCommand
{
  public static System.Random Random = new();
  public static bool Roll(float value)
  {
    if (value >= 1f) return true;
    return Random.NextDouble() < value;
  }
  public const string Name = "object";
  private static Dictionary<ZDOID, EditInfo> EditedInfo = new();
  private static void AddData(ZNetView view, bool refresh = false)
  {
    var zdo = view.GetZDO();
    if (EditedInfo.TryGetValue(zdo.m_uid, out var info))
    {
      if (refresh) info.Refresh = refresh;
      return;
    }
    EditedInfo[zdo.m_uid] = new EditInfo(zdo, refresh);
  }
  private static void Execute(Terminal context, ObjectParameters pars, IEnumerable<string> operations, ZNetView[] views)
  {
    var scene = ZNetScene.instance;
    Dictionary<ZDOID, long> oldOwner = new();
    views = views.Where(view =>
    {
      if (!view || !view.GetZDO().IsValid())
      {
        context.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      if (!Roll(pars.Chance))
      {
        context.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      return true;
    }).ToArray();
    List<ZDO> removed = new();
    EditedInfo.Clear();
    foreach (var view in views)
    {
      oldOwner.Add(view.GetZDO().m_uid, view.GetZDO().m_owner);
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
          output = ChangeHealth(view, Helper.RandomValue(pars.Health));
        if (operation == "damage")
          output = Actions.Damage(view, Helper.RandomValue(pars.Damage));
        if (operation == "ammo")
          output = Actions.Ammo(view, Helper.RandomValue(pars.Ammo));
        if (operation == "ammotype")
          output = Actions.AmmoType(view, pars.AmmoType);
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
        if (operation == "respawn")
          output = Respawn(view);
        if (operation == "info")
          output = GetInfo(view);
        if (operation == "data")
          output = PrintData(view, pars.Data);
        if (operation == "copy")
          output = CopyData(view, pars.Copy);
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
    foreach (var view in views)
    {
      if (!view || view.GetZDO() == null || !view.GetZDO().IsValid() || !oldOwner.ContainsKey(view.GetZDO().m_uid)) continue;
      view.GetZDO().SetOwner(oldOwner[view.GetZDO().m_uid]);
    }
    if (removed.Count > 0)
    {
      UndoRemove undo = new(removed);
      UndoManager.Add(undo);
    }
    if (EditedInfo.Count > 0)
    {
      UndoEdit undo = new(EditedInfo.Select(info => info.Value.ToData()));
      UndoManager.Add(undo);
    }
  }
  public ObjectCommand()
  {
    ObjectAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Modifies the selected object(s).", null, autoComplete.NamedParameters);
    Helper.Command(Name, description, (args) =>
    {
      ObjectParameters pars = new(args);
      ZNetView[] views;
      var ignoredIds = Parse.Split(pars.Ignore).ToList();
      if (pars.Connect)
      {
        var view = Selector.GetHovered(50f, ignoredIds);
        if (view == null) return;
        views = Selector.GetConnected(view, ignoredIds);
      }
      else if (pars.Radius != null)
      {
        views = Selector.GetNearby(pars.Id, pars.ObjectType, ignoredIds, pars.Center ?? pars.From, pars.Radius, pars.Height);
      }
      else if (pars.Width != null && pars.Depth != null)
      {
        views = Selector.GetNearby(pars.Id, pars.ObjectType, ignoredIds, pars.Center ?? pars.From, pars.Angle, pars.Width, pars.Depth, pars.Height);
      }
      else
      {
        var view = Selector.GetHovered(50f, ignoredIds);
        if (view == null) return;
        if (!Selector.GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab()))
        {
          Helper.AddMessage(args.Context, $"Skipped: {view.name} has invalid id.");
          return;
        }
        views = new[] { view };
      }
      Execute(args.Context, pars, pars.Operations, views);

    }, () => autoComplete.NamedParameters);
  }

  private static string ChangeHealth(ZNetView obj, float amount)
  {
    if (!obj.GetComponent<Character>() && !obj.GetComponent<WearNTear>() && !obj.GetComponent<TreeLog>() && !obj.GetComponent<Destructible>() && !obj.GetComponent<TreeBase>())
      return "Skipped: ¤ is not a creature or a destructible.";
    AddData(obj);
    var previous = Actions.SetHealth(obj.gameObject, amount);
    var amountStr = amount == 0f ? "default" : amount.ToString("F0");
    return $"¤ health changed from {previous.ToString("F0")} to {amountStr}.";
  }
  private static string SetStars(ZNetView view, int amount)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    var previous = obj.GetLevel() - 1;
    Actions.SetLevel(obj, amount + 1);
    return $"¤ stars changed from {previous} to {amount}.";
  }
  private static string PrintFuel(ZNetView view)
  {
    var obj = view.GetComponent<Fireplace>();
    if (!obj) return "Skipped: ¤ is not a fireplace.";
    var amount = view.GetZDO().GetFloat("fuel", 0f);
    return $"¤ has {amount} fuel.";
  }
  private static string SetFuel(ZNetView view, float amount)
  {
    var smelter = view.GetComponent<Smelter>();
    if (!view.GetComponent<Fireplace>() && (!smelter || !smelter.m_fuelItem)) return "Skipped: ¤ is not a fireplace or smelter.";
    AddData(view);
    var previous = view.GetZDO().GetFloat("fuel", 0f);
    Actions.SetFuel(view, amount);
    return $"¤ fuel changed from {previous} to {amount}.";
  }
  private static string Move(ZNetView view, Vector3 offset, string origin)
  {
    AddData(view);
    Actions.Move(view, offset, origin);
    return $"¤ moved {offset.ToString("F1")} from the {origin}.";
  }
  private static string ResetRotation(ZNetView view)
  {
    AddData(view);
    Actions.ResetRotation(view);
    return $"¤ rotation reseted.";
  }
  private static string Mirror(ZNetView view, Vector3 center)
  {
    AddData(view);
    Actions.Mirror(view, center);
    return $"¤ mirrored.";
  }
  private static string Rotate(ZNetView view, Vector3 rotation, string origin, Vector3? center = null)
  {
    AddData(view);
    Actions.Rotate(view, rotation, origin, center);
    return $"¤ rotated {rotation.ToString("F1")} from the {origin}.";
  }
  private static string Scale(ZNetView view, Vector3 scale)
  {
    AddData(view);
    Actions.Scale(view, scale);
    return $"¤ scaled to {scale.ToString("F1")}.";
  }
  private static string SetBaby(ZNetView view)
  {
    var obj = view.GetComponent<Growup>();
    if (!obj) return "Skipped: ¤ is not an offspring.";
    AddData(view);
    Actions.SetBaby(obj);
    return "¤ growth disabled.";
  }
  private static string Respawn(ZNetView view)
  {
    if (!Actions.CanRespawn(view.gameObject))
      return "Skipped: ¤ is not a loot container, pickable or spawn point.";
    AddData(view);
    Actions.Respawn(view.gameObject);
    return "¤ respawned.";
  }
  private const string DEFAULT = "default";
  private static string MakeTame(ZNetView view)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetTame(obj, true);
    return "¤ made tame.";
  }
  private static string MakeWild(ZNetView view)
  {
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetTame(obj, false);
    return "¤ made wild.";
  }
  private static string MakeSleep(ZNetView view)
  {
    var obj = view.GetComponent<MonsterAI>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetSleeping(obj, true);
    return "¤ made to sleep.";
  }
  private static string SetCreator(ZNetView view, long creator)
  {
    var obj = view.GetComponent<Piece>();
    if (!obj) return "Skipped: ¤ is not a piece.";
    AddData(view);
    var previous = Actions.SetCreator(obj, creator);
    return $"Creator of ¤ set from {previous} to {creator}.";
  }
  private static string SetPrefab(ZNetView view, string prefab)
  {
    AddData(view, true);
    if (Actions.SetPrefab(view, prefab))
      return $"Prefab of ¤ set to {prefab}.";
    return $"Error: Prefab of ¤ was not set to {prefab}. Probably invalid prefab name.";
  }
  private static string SetVisual(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<ItemStand>();
    if (!obj) return "Skipped: ¤ is not an item stand.";
    AddData(view);
    Actions.SetVisual(obj, item);
    return $"Visual of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetHelmet(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Helmet, item);
    return $"Helmet of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLeftHand(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.HandLeft, item);
    return $"Left hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetRightHand(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.HandRight, item);
    return $"Right hand of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetChest(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Chest, item);
    return $"Chest of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetShoulder(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Shoulder, item);
    return $"Shoulder of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetLegs(ZNetView view, Item? item)
  {
    if (item == null) return "Skipped: Invalid item.";
    var obj = view.GetComponent<Character>();
    if (!obj) return "Skipped: ¤ is not a creature.";
    AddData(view);
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
    AddData(view);
    Actions.SetVisual(obj, VisSlot.Utility, item);
    return $"Utility item of ¤ set to {item.Name} with variant {item.Variant}.";
  }
  private static string SetStatus(ZNetView obj, string name, float duration, float intensity)
  {
    if (!obj.TryGetComponent<Character>(out var creature)) return "Skipped: ¤ is not a creature.";
    obj.ClaimOwnership();
    creature.GetSEMan()?.AddStatusEffect(name, true);
    var effect = creature.GetSEMan()?.GetStatusEffect(name);
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
    List<string> info = new();
    info.Add("Id: ¤");
    info.Add("Pos: " + Helper.PrintVectorXZY(obj.transform.position));
    info.Add("Rot: " + Helper.PrintAngleYXZ(obj.transform.rotation));
    if (obj.m_syncInitialScale)
      info.Add("Scale: " + Helper.PrintVectorXZY(obj.transform.localScale));
    var character = obj.GetComponent<Character>();
    if (character)
    {
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
    }
    else
    {
      var health = Actions.GetHealth(obj);
      if (health > 1E17)
        info.Add("Health: Infinite");
      else if (health > -1f)
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

  private static void Serialize(ZDO zdo, ZPackage pkg, bool smart)
  {
    zdo = zdo.Clone();
    if (smart)
    {
      zdo.m_vec3?.Remove("scale".GetStableHashCode());
      zdo.m_ints?.Remove(Hash.Seed);
      zdo.m_ints?.Remove(Hash.Location);
      if (zdo.m_strings != null && zdo.m_strings.ContainsKey(Hash.OverrideItems))
      {
        zdo.m_ints?.Remove(Hash.AddedDefaultItems);
        zdo.m_strings?.Remove(Hash.Items);
      }
    }
    var num = 0;
    if (zdo.m_floats != null && zdo.m_floats.Count > 0)
      num |= 1;
    if (zdo.m_vec3 != null && zdo.m_vec3.Count > 0)
      num |= 2;
    if (zdo.m_quats != null && zdo.m_quats.Count > 0)
      num |= 4;
    if (zdo.m_ints != null && zdo.m_ints.Count > 0)
      num |= 8;
    if (zdo.m_strings != null && zdo.m_strings.Count > 0)
      num |= 16;
    if (zdo.m_longs != null && zdo.m_longs.Count > 0)
      num |= 64;
    if (zdo.m_byteArrays != null && zdo.m_byteArrays.Count > 0)
      num |= 128;

    pkg.Write(num);
    if (zdo.m_floats != null && zdo.m_floats.Count > 0)
    {
      pkg.Write((byte)zdo.m_floats.Count);
      foreach (var kvp in zdo.m_floats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (zdo.m_vec3 != null && zdo.m_vec3.Count > 0)
    {
      pkg.Write((byte)zdo.m_vec3.Count);
      foreach (var kvp in zdo.m_vec3)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (zdo.m_quats != null && zdo.m_quats.Count > 0)
    {
      pkg.Write((byte)zdo.m_quats.Count);
      foreach (var kvp in zdo.m_quats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (zdo.m_ints != null && zdo.m_ints.Count > 0)
    {
      pkg.Write((byte)zdo.m_ints.Count);
      foreach (var kvp in zdo.m_ints)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (zdo.m_longs != null && zdo.m_longs.Count > 0)
    {
      pkg.Write((byte)zdo.m_longs.Count);
      foreach (var kvp in zdo.m_longs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (zdo.m_strings != null && zdo.m_strings.Count > 0)
    {
      pkg.Write((byte)zdo.m_strings.Count);
      foreach (var kvp in zdo.m_strings)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (zdo.m_byteArrays != null && zdo.m_byteArrays.Count > 0)
    {
      pkg.Write((byte)zdo.m_byteArrays.Count);
      foreach (var kvp in zdo.m_byteArrays)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
  }
  private static string CopyData(ZNetView obj, string value)
  {
    var zdo = obj.GetZDO();
    ZPackage pkg = new();
    Serialize(zdo, pkg, value != "all");
    var str = pkg.GetBase64();
    if (str == "AAAAAA==") str = "";
    GUIUtility.systemCopyBuffer = str;
    return str;
  }
  private static string PrintData(ZNetView obj, string data)
  {
    List<string> info = new();
    var zdo = obj.GetZDO();
    info.Add("Id: ¤");
    info.Add($"Owner: {zdo.m_owner}");
    info.Add($"Revision: {zdo.m_dataRevision} + {zdo.m_ownerRevision}");
    if (data != "")
    {
      var split = data.Split(',');
      var hash = split[0].GetStableHashCode();
      if (zdo.m_vec3?.ContainsKey(hash) == true)
      {
        if (split.Length > 1)
          zdo.Set(hash, Parse.VectorXZY(split, 1));
        info.Add($"{split[0]}: {Helper.PrintVectorXZY(zdo.m_vec3[hash])}");
      }
      if (zdo.m_quats?.ContainsKey(hash) == true)
      {
        info.Add($"{split[0]}: {Helper.PrintAngleYXZ(zdo.m_quats[hash])}");
      }
      if (zdo.m_longs?.ContainsKey(hash) == true)
      {
        if (split.Length > 1)
          zdo.Set(hash, Parse.Long(split[1]));
        info.Add($"{data}: {zdo.m_longs[hash]}");
      }
      if (zdo.m_strings?.ContainsKey(hash) == true)
      {
        if (split.Length > 1)
          zdo.Set(hash, split[1]);
        info.Add($"{split[0]}: {zdo.m_strings[hash]}");
      }
      if (zdo.m_ints?.ContainsKey(hash) == true)
      {
        if (split.Length > 1)
          zdo.Set(hash, Parse.Int(split[1]));
        info.Add($"{split[0]}: {zdo.m_ints[hash]}");
      }
      if (zdo.m_floats?.ContainsKey(hash) == true)
      {
        if (split.Length > 1)
          zdo.Set(hash, Parse.Float(split[1]));
        info.Add($"{data}: {zdo.m_floats[hash].ToString("F1")}");
      }
      if (info.Count < 4)
        info.Add($"{split[0]}: No data!");
    }
    return string.Join(", ", info);
  }
}
