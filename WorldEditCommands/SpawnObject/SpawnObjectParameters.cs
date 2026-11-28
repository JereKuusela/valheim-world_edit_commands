using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Data;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

class SpawnObjectParameters : SharedObjectParameters
{
  public Quaternion BaseRotation = Quaternion.identity;
  public Range<Vector3> Rotation = new(Vector3.zero);
  public Range<Vector3> RelativePosition = new(Vector3.zero);
  public Vector3 From = Vector3.zero;
  public Vector3? To = null;
  public Range<int>? Amount;
  public string? Name;
  public long? CrafterId;
  public Range<int>? Variant;
  public bool Snap = true;
  public bool? Tamed;
  public bool? Hunt;
  private bool UseDefaultRelativePosition = false;
  public DataEntry? Data;

  public SpawnObjectParameters(Terminal.ConsoleEventArgs args)
  {
    if (Player.m_localPlayer)
    {
      From = Player.m_localPlayer.transform.position;
      BaseRotation = Player.m_localPlayer.transform.rotation;
      UseDefaultRelativePosition = true;
    }
    ParseArgs(args.Args);
  }

  protected override void ParseArgs(string[] args)
  {
    base.ParseArgs(args);
    foreach (var arg in args)
    {
      var split = arg.Split(['='], 2);
      var name = split[0].ToLower();
      if (name == "tame" || name == "tamed")
        Tamed = true;
      if (name == "hunt")
        Hunt = true;
      if (split.Length < 2) continue;
      var value = split[1];
      if (name == "name" || name == "crafter")
        Name = value.Replace("_", " ");
      if (name == "crafterid")
        CrafterId = Parse.Long(value);
      if (name == "variant")
        Variant = Parse.IntRange(value, 0);
      if (name == "amount")
        Amount = Parse.IntRange(value);
      if (name == "hunt")
        Hunt = Parse.Boolean(value);
      if (name == "tame" || name == "tamed")
        Tamed = Parse.Boolean(value);
      if (name == "refrot" || name == "refrotation")
      {
        BaseRotation = Parse.AngleYXZ(value, BaseRotation);
      }
      if (name == "pos" || name == "position")
      {
        UseDefaultRelativePosition = false;
        var values = value.Split(',');
        RelativePosition = Parse.VectorZXYRange(value, Vector3.zero);
        if (values.Length >= 3)
          Snap = false;
      }
      if (name == "rot" || name == "rotation")
      {
        Rotation = Parse.VectorYXZRange(value, Vector3.zero);
      }
      if (name == "from" || name == "refpos")
      {
        UseDefaultRelativePosition = false;
        var values = value.Split(',');
        From = Parse.VectorXZY(values, From);
        if (values.Length >= 3)
          Snap = false;
      }
      if (name == "to")
      {
        UseDefaultRelativePosition = false;
        To = Parse.VectorXZY(value.Split(','), From);
      }
      if (name == "data")
      {
        Data = DataHelper.Merge([.. Parse.Split(value).Select(DataHelper.Get)]);
      }
      if (name == "refplayer")
      {
        UseDefaultRelativePosition = false;
        var player = Helper.FindPlayer(value);
        if (player.m_characterID.IsNone())
        {
          throw new InvalidOperationException("Unable to find the player.");
        }
        else if (!player.m_publicPosition)
        {
          throw new InvalidOperationException("Player doesn't have a public position.");
        }
        else
        {
          From = player.m_position;
        }
      }
    }
    // For usability, spawn in front of the player if nothing is specified (similar to the base game command).
    // Must be applied to From so that the undo command works correctly.
    if (UseDefaultRelativePosition)
      From += BaseRotation * Vector3.forward * 2.0f;
    if (To.HasValue && Radius != null)
      throw new InvalidOperationException("<color=yellow>radius</color> can't be used with <color=yellow>to</color>.");
  }

  public (DataEntry? Entry, Dictionary<string, string> Parameters, Func<ZDO, int, int>? WriteOverrides) BuildSpawnData(GameObject prefab)
  {
    var item = prefab.GetComponent<ItemDrop>();
    var view = prefab.GetComponent<ZNetView>();
    var character = prefab.GetComponent<Character>();
    var ai = prefab.GetComponent<BaseAI>();
    var equipment = character ? prefab.GetComponent<VisEquipment>() : null;
    var setBaby = Baby == true && prefab.GetComponent<Growup>();
    var setTamed = Tamed.HasValue && character;
    var hasItemOverrides = item && (Amount != null || Level != null || Health != null || Variant != null || Name != null || CrafterId.HasValue);
    var setName = Name != null && prefab.GetComponent<Tameable>();
    var setHunt = Hunt.HasValue && ai;
    var hasItemVisuals = Helmet != null || Chest != null || Shoulders != null || Legs != null || Utility != null || LeftHand != null || RightHand != null;
    var hasEquipmentOverrides = equipment && (Model != null || hasItemVisuals);
    var objectHealth = Health == null || item || character ? null : prefab.GetComponent<WearNTear>()?.m_health ??
      prefab.GetComponent<TreeLog>()?.m_health ?? prefab.GetComponent<TreeBase>()?.m_health ?? prefab.GetComponent<Destructible>()?.m_health;
    var hasCharacterStats = !item && character && (Level != null || Health != null);
    var hasOverrides = hasItemOverrides || hasCharacterStats || objectHealth.HasValue || hasEquipmentOverrides || setBaby || setTamed ||
      setName || setHunt || Damage != null || Ammo != null || AmmoType != null || Persist.HasValue || Distant.HasValue || Fields.Count > 0;
    return (Data, DataParameters, hasOverrides ? WriteOverrides : null);

    int WriteOverrides(ZDO zdo, int remaining)
    {
      if (Damage != null)
        zdo.Set(Hash.Damage, Helper.RandomValue(Damage));
      if (Ammo != null)
        zdo.Set(Hash.Ammo, Helper.RandomValue(Ammo));
      if (AmmoType != null)
        zdo.Set(Hash.AmmoType, AmmoType);
      if (Persist.HasValue)
        zdo.Persistent = Persist.Value;
      if (Distant.HasValue)
        zdo.Distant = Distant.Value;
      if (setBaby)
        zdo.Set(Hash.SpawnTime, DateTime.MaxValue.Ticks);
      if (setName)
        zdo.Set(Hash.TamedName, Name!);
      if (setHunt)
        zdo.Set(Hash.HuntPlayer, Hunt!.Value);
      if (setTamed)
        WriteTamed(zdo, ai, Tamed!.Value);
      if (hasCharacterStats && character != null)
        WriteCharacterStats(zdo, character);
      if (objectHealth.HasValue && Health != null)
      {
        var health = Helper.RandomValue(Health);
        if (isHealthPercentage) health *= objectHealth.Value;
        zdo.Set(Hash.Health, health == 0 ? objectHealth.Value : health);
      }
      if (hasEquipmentOverrides && equipment != null)
        WriteEquipment(zdo, equipment);
      var stack = hasItemOverrides && item != null ? WriteItem(zdo, item, remaining) : 0;
      if (Fields.Count > 0 && view)
        Actions.WriteFields(view, zdo, Fields);
      if (hasEquipmentOverrides && hasItemVisuals && character is Humanoid)
        PreserveSpawnVisuals.Remember(zdo);
      return stack;
    }
  }

  private static void WriteTamed(ZDO zdo, BaseAI? ai, bool tamed)
  {
    zdo.Set(ZDOVars.s_tamed, tamed);
    if (!ai) return;
    zdo.Set(ZDOVars.s_alert, false);
    zdo.Set(ZDOVars.s_haveTargetHash, false);
    if (!tamed) return;
    zdo.Set(Hash.HuntPlayer, false);
    zdo.Set(ZDOVars.s_patrol, true);
    zdo.Set(ZDOVars.s_patrolPoint, zdo.GetPosition());
    if (ai is MonsterAI)
    {
      zdo.Set(ZDOVars.s_despawnInDay, false);
      zdo.Set(ZDOVars.s_eventCreature, false);
    }
  }

  private void WriteCharacterStats(ZDO zdo, Character character)
  {
    var previousLevel = zdo.GetInt(ZDOVars.s_level, 1);
    var level = Level == null ? previousLevel : Helper.RandomValue(Level);
    var levelChanged = level >= 1 && level != previousLevel;
    if (levelChanged)
      zdo.Set(ZDOVars.s_level, level);
    var baseHealth = character.GetMaxHealthBase() * (levelChanged ? level : previousLevel);
    var maxHealth = zdo.GetFloat(ZDOVars.s_maxHealth, character.m_health);
    var currentHealth = zdo.GetFloat(ZDOVars.s_health, maxHealth);
    var wasFullHealth = currentHealth == maxHealth;
    if (levelChanged)
    {
      zdo.Set(ZDOVars.s_maxHealth, baseHealth);
      zdo.Set(ZDOVars.s_health, wasFullHealth ? baseHealth : Math.Min(currentHealth, baseHealth));
    }
    if (Health == null) return;
    var health = Helper.RandomValue(Health);
    if (isHealthPercentage) health *= levelChanged || wasFullHealth ? baseHealth : maxHealth;
    zdo.Set(ZDOVars.s_maxHealth, health == 0 ? baseHealth : health);
    zdo.Set(ZDOVars.s_health, health == 0 ? (wasFullHealth ? baseHealth : Math.Min(currentHealth, baseHealth)) : health * 1.000001f);
  }

  private void WriteEquipment(ZDO zdo, VisEquipment equipment)
  {
    SetItem(Helmet, ZDOVars.s_helmetItem);
    SetItem(Chest, ZDOVars.s_chestItem);
    SetItem(Shoulders, ZDOVars.s_shoulderItem, ZDOVars.s_shoulderItemVariant);
    SetItem(Legs, ZDOVars.s_legItem);
    SetItem(Utility, ZDOVars.s_utilityItem);
    SetItem(LeftHand, ZDOVars.s_leftItem, ZDOVars.s_leftItemVariant);
    SetItem(RightHand, ZDOVars.s_rightItem);
    if (Model != null)
    {
      var model = Helper.RandomValue(Model);
      if (model >= 0 && model < equipment.m_models.Length)
        zdo.Set(ZDOVars.s_modelIndex, model);
    }

    void SetItem(Item? item, int key, int? variantKey = null)
    {
      if (item == null) return;
      zdo.Set(key, ZdoHelper.Hash(item.Name));
      if (variantKey.HasValue)
        zdo.Set(variantKey.Value, item.Variant);
    }
  }

  [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GiveDefaultItems))]
  private class PreserveSpawnVisuals
  {
    private static readonly HashSet<ZDOID> Pending = [];

    internal static void Remember(ZDO zdo)
    {
      Pending.Add(zdo.m_uid);
    }

    static bool Prefix(Humanoid __instance)
    {
      var view = __instance.m_nview;
      if (!view) return true;
      var zdo = view.GetZDO();
      if (zdo == null || !Pending.Contains(zdo.m_uid)) return true;
      Pending.Remove(zdo.m_uid);
      return false;
    }
  }

  private int WriteItem(ZDO zdo, ItemDrop prefab, int remaining)
  {
    var item = prefab.m_itemData.Clone();
    ItemDrop.LoadFromZDO(item, zdo);
    var level = Level == null ? 0 : Helper.RandomValue(Level);
    if (level >= 1)
      zdo.Set(Hash.Quality, item.m_quality = level);
    if (Health != null)
    {
      var durability = Helper.RandomValue(Health);
      if (isHealthPercentage) durability *= item.GetMaxDurability();
      zdo.Set(Hash.Durability, item.m_durability = durability == 0 ? item.GetMaxDurability() : durability);
    }
    if (Variant != null)
      zdo.Set(Hash.Variant, item.m_variant = Helper.RandomValue(Variant));
    if (Name != null)
    {
      item.m_crafterID = Name == "" ? 0 : -1;
      zdo.Set(Hash.CrafterName, item.m_crafterName = Name);
    }
    if (CrafterId.HasValue)
      item.m_crafterID = CrafterId.Value;
    if (Name != null || CrafterId.HasValue)
      zdo.Set(Hash.CrafterID, item.m_crafterID);
    var stack = Amount != null && remaining > 0 ? Math.Min(remaining, item.m_shared.m_maxStackSize) : 0;
    if (stack > 0)
      zdo.Set(Hash.Stack, item.m_stack = stack);
    ItemDrop.SaveToZDO(item, zdo);
    return stack;
  }

  public Vector3 GetPosition() => From + BaseRotation * Helper.RandomValue(RelativePosition);
  public Vector3 GetSpawnPosition(int index, int count)
  {
    if (To.HasValue)
      return count > 1 ? Vector3.Lerp(From, To.Value, (float)index / (count - 1)) : From;
    var position = GetPosition();
    var random = UnityEngine.Random.insideUnitCircle * (Radius?.Max ?? (count - 1) * 0.5f);
    if (Radius != null && Radius.Min != Radius.Max)
    {
      var angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      random = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized * Helper.RandomValue(Radius);
    }
    return position + new Vector3(random.x, 0f, random.y);
  }
}
