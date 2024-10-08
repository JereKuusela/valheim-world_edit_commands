using System;
using System.Linq;
using Data;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands;

public enum Growth
{
  Default,
  HealthyGrown,
  UnhealthyGrown,
  Healthy,
  Unhealthy
}
public enum Wear
{
  Default,
  Broken,
  Damaged,
  Healthy
}
public enum Fall
{
  Default,
  Off,
  Terrain,
  Solid
}
public enum Enum_CLLC_Boss
{
  None,
  Reflective,
  Shielded,
  Mending,
  Summoner,
  Elementalist,
  Enraged,
  Twin
}
public enum Enum_CLLC_Infusion
{
  None,
  Lightning,
  Fire,
  Frost,
  Poison,
  Chaos,
  Spirit
}
public enum Enum_CLLC_Effect
{
  None,
  Aggressive,
  Quick,
  Regenerating,
  Curious,
  Splitting,
  Armored
}

public static class TweakActions
{
  private const string DEFAULT = "default";
  private static int BiomeToInt(string value)
  {
    var split = value.Split(',');
    Heightmap.Biome biomes = Heightmap.Biome.None;
    foreach (var s in split)
    {
      if (Enum.TryParse<Heightmap.Biome>(s, true, out var biome))
        biomes |= biome;
    }
    return (int)biomes;
  }

  private static string Print<T>(T? value) => value == null ? DEFAULT : value.ToString();
  private static bool? Reverse(bool? value) => value == null ? null : !value;
  public static string Render(ZNetView view, bool? value)
  {
    value = !Actions.ToggleBool(view, Reverse(value), Hash.NoRender);
    return $"¤ render set to {Print(value)}.";
  }
  public static string Interact(ZNetView view, bool? value)
  {
    value = !Actions.ToggleBool(view, Reverse(value), Hash.NoInteract);
    return $"¤ interact set to {Print(value)}.";
  }
  public static string Collision(ZNetView view, bool? value)
  {
    value = !Actions.ToggleBool(view, Reverse(value), Hash.NoCollision);
    return $"¤ collision set to {Print(value)}.";
  }
  public static string Wear(ZNetView view, string? value)
  {
    Actions.SetInt(view, WearNumber(value), Hash.Wear);
    return $"¤ wear set to {Print(value)}.";
  }
  private static int WearNumber(string? wear)
  {
    if (wear == "broken") return 3;
    if (wear == "damaged") return 1;
    if (wear == "healthy") return 2;
    return 0;
  }

  public static string Status(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Status);
    return $"¤ status set to {Print(value)}.";
  }
  public static string Event(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Event);
    return $"¤ event set to {Print(value)}.";
  }
  public static string Effect(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Effect);
    return $"¤ effect set to {Print(value)}.";
  }
  public static string Weather(ZNetView view, int hash, string? value)
  {
    Actions.SetString(view, value, hash);
    return $"¤ weather set to {Print(value)}.";
  }
  public static string Water(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Water);
    return $"¤ water set to {Print(value)}.";
  }
  public static string EnterText(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.DungeonEnterText);
    return $"¤ enter text set to {Print(value)}.";
  }
  public static string ExitText(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.DungeonExitText);
    return $"¤ exit text set to {Print(value)}.";
  }
  public static string EnterHover(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.DungeonEnterHover);
    return $"¤ enter hover set to {Print(value)}.";
  }
  public static string ExitHover(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.DungeonExitHover);
    return $"¤ exit hover set to {Print(value)}.";
  }
  public static string Fall(ZNetView view, string? value)
  {
    Actions.SetInt(view, FallNumber(value), Hash.Fall);
    return $"¤ fall set to {Print(value)}.";
  }
  private static int FallNumber(string? fall)
  {
    if (fall == null) return 0;
    if (fall == "off") return 3;
    if (fall == "terrain") return 1;
    if (fall == "solid") return 2;
    return 0;
  }
  public static string Growth(ZNetView view, string? value)
  {
    var number = GrowthNumber(value);
    Actions.SetInt(view, number, Hash.Growth);
    var time = number < 0 ? ZNet.instance.GetTime().Ticks : DateTime.MaxValue.Ticks / 2L;
    view.GetZDO().Set(Hash.PlantTime, time);
    return $"¤ growth set to {Print(value)}.";
  }
  private static int GrowthNumber(string? growth)
  {
    if (growth == "big") return 4;
    if (growth == "big_bad") return 1;
    if (growth == "small") return 2;
    if (growth == "small_bad") return 3;
    return 0;
  }

  public static string Component(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Component);
    return $"¤ component set to {Print(value)}.";
  }
  public static string AddComponent(ZNetView view, string value)
  {
    var components = Parse.Split(view.GetZDO().GetString(Hash.Component, "")).ToHashSet();
    components.Add(value);
    Actions.SetString(view, string.Join(",", components), Hash.Component);
    return $"¤ component set to {Print(value)}.";
  }

  public static string Restrict(ZNetView view, bool? value)
  {
    value = !Actions.ToggleBool(view, Reverse(value), Hash.NoRestrict);
    return $"¤ restrict set to {Print(value)}.";
  }
  public static string Boss(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Boss);
    return $"¤ boss set to {Print(value)}.";
  }
  public static string Hunt(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.HuntPlayer);
    return $"¤ hunt set to {Print(value)}.";
  }
  public static string Tame(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Tamed);
    return $"¤ tamed set to {Print(value)}.";
  }
  public static string Unlock(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Unlock);
    return $"¤ unlock set to {Print(value)}.";
  }
  public static string Consume(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.DoorConsume);
    return $"¤ consume set to {Print(value)}.";
  }
  public static string NoClose(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.DoorNoClose);
    return $"¤ no close set to {Print(value)}.";
  }
  public static string DoorKey(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.DoorKey);
    return $"¤ door key set to {Print(value)}.";
  }
  public static string Creator(ZNetView view, long? value)
  {
    Actions.SetLong(view, value ?? 0L, Hash.Creator);
    return $"¤ creator set to {Print(value)}.";
  }
  public static string Smoke(ZNetView view, string? value)
  {
    int val = 0;
    if (value == "off") val = 1;
    if (value == "ignore") val = 2;
    Actions.SetInt(view, val, Hash.Smoke);
    return $"¤ smoke set to {Print(value)}.";
  }
  public static string CLLC_BossEffect(ZNetView view, string[] values)
  {
    var cllc = Enum_CLLC_Boss.None;
    foreach (var value in values)
    {
      if (Enum.TryParse<Enum_CLLC_Boss>(value, true, out var v)) cllc |= v;
    }
    Actions.SetInt(view, (int)cllc, Hash.CLLC_BossEffect);
    return $"¤ CLLC affix set to {Print(cllc)}.";
  }
  public static string CLLC_Effect(ZNetView view, string[] values)
  {
    var cllc = Enum_CLLC_Effect.None;
    foreach (var value in values)
    {
      if (Enum.TryParse<Enum_CLLC_Effect>(value, true, out var v)) cllc |= v;
    }
    Actions.SetInt(view, (int)cllc, Hash.CLLC_Effect);
    return $"¤ CLLC effect set to {Print(cllc)}.";
  }
  public static string CLLC_Infusion(ZNetView view, string[] values)
  {
    var cllc = Enum_CLLC_Infusion.None;
    foreach (var value in values)
    {
      if (Enum.TryParse<Enum_CLLC_Infusion>(value, true, out var v)) cllc |= v;
    }
    Actions.SetInt(view, (int)cllc, Hash.CLLC_Infusion);
    return $"¤ CLLC infusion set to {Print(cllc)}.";
  }
  public static string SpawnData(ZNetView view, string? value)
  {
    Actions.SetString(view, value == null ? null : DataHelper.Base64([], value), Hash.Data);
    return $"¤ spawn data set to {Print(value)}.";
  }
  public static string Spawn(ZNetView view, int hash, string? value)
  {
    Actions.SetPrefab(view, value, hash);
    return $"¤ spawn prefab set to {Print(value)}.";
  }
  public static string Biome(ZNetView view, string? value)
  {
    Actions.SetInt(view, BiomeToInt(value ?? ""), Hash.Biome);
    return $"¤ biome set to {Print(value)}.";
  }
  public static string Fuel(ZNetView view, string? value)
  {
    Actions.SetPrefab(view, value, Hash.OverrideFuel);
    return $"¤ fuel prefab set to {Print(value)}.";
  }

  public static string ItemOffset(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.ItemOffset);
    return $"¤ item offset set to {Print(value)}.";
  }
  public static string CoverOffset(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.CoverOffset);
    return $"¤ cover offset set to {Print(value)}.";
  }
  public static string SpawnOffset(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.SpawnOffset);
    return $"¤ spawn offset set to {Print(value)}.";
  }
  public static string ItemStandPrefix(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.ItemStandPrefix);
    return $"¤ item stand prefix set to {Print(value)}.";
  }
  public static string GlobalKey(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.GlobalKey);
    return $"¤ global key set to {Print(value)}.";
  }
  public static string Name(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.OverrideName);
    return $"¤ name set to {Print(value)}.";
  }
  public static string Text(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.OverrideText);
    return $"¤ text set to {Print(value)}.";
  }
  public static string TextBiome(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.TextBiome);
    return $"¤ biome text set to {Print(value)}.";
  }
  public static string TextSpace(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.TextSpace);
    return $"¤ space text set to {Print(value)}.";
  }
  public static string TextSleep(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.TextSleep);
    return $"¤ sleep text set to {Print(value)}.";
  }
  public static string TextHappy(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.TextHappy);
    return $"¤ happy text set to {Print(value)}.";
  }
  public static string TextCheck(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.TextCheck);
    return $"¤ check text set to {Print(value)}.";
  }
  public static string TextExtract(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.TextExtract);
    return $"¤ extract text set to {Print(value)}.";
  }
  public static string Compendium(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.Compendium);
    return $"¤ compendium set to {Print(value)}.";
  }
  public static string Topic(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Topic);
    return $"¤ topic set to {Print(value)}.";
  }
  public static string Discover(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Discover);
    return $"¤ discover set to {Print(value)}.";
  }
  public static string Spawns(ZNetView view, string[] value)
  {
    if (value.Length > 4 && !Parse.TryFloat(value[4], out _))
      value[4] = DataHelper.Base64([], value[4]);
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.SpawnSpawnArea);
    return $"¤ spawn prefabs set to {Print(str)}.";
  }
  public static string Conversions(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.Conversion);
    return $"¤ conversions set to {Print(str)}.";
  }
  public static string Items(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.OverrideItems);
    return $"¤ items set to {Print(str)}.";
  }
  public static string SpawnEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.SpawnEffect);
    return $"¤ spawn effect set to {Print(str)}.";
  }
  public static string DestroyEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.DestroyEffect);
    return $"¤ destroy effect set to {Print(str)}.";
  }
  public static string UseEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.UseEffect);
    return $"¤ use effect set to {Print(str)}.";
  }
  public static string InputEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.InputEffect);
    return $"¤ input effect set to {Print(str)}.";
  }
  public static string OpenEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.OpenEffect);
    return $"¤ open effect set to {Print(str)}.";
  }
  public static string CloseEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.CloseEffect);
    return $"¤ close effect set to {Print(str)}.";
  }
  public static string LockedEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.LockedEffect);
    return $"¤ locked effect set to {Print(str)}.";
  }
  public static string OutputEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.OutputEffect);
    return $"¤ output effect set to {Print(str)}.";
  }
  public static string FuelEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.FuelEffect);
    return $"¤ fuel effect set to {Print(str)}.";
  }
  public static string StartEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.StartEffect);
    return $"¤ start effect set to {Print(str)}.";
  }
  public static string SpawnItem(ZNetView view, string? value)
  {
    Actions.SetPrefab(view, value, Hash.SpawnItem);
    return $"¤ spawn item prefab set to {Print(value)}.";
  }
  public static string Item(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.OverrideItem);
    return $"¤ item set to {Print(value)}.";
  }
  public static string Faction(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Faction);
    return $"¤ faction set to {Print(value)}.";
  }
  public static string Command(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Command);
    return $"¤ command set to {Print(value)}.";
  }
  private static string Resistance(string value)
  {
    var split = value.Split(',');
    if (split.Length < 2) return "";
    if (!Enum.TryParse<HitData.DamageType>(split[0], true, out var type)) return "";
    if (!Enum.TryParse<HitData.DamageModifier>(split[1], true, out var modifier)) return "";
    return (int)type + "," + (int)modifier;
  }
  public static string Resistances(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(Resistance).Where(s => s != ""));
    Actions.SetString(view, str, Hash.Resistances);
    return $"¤ resistances set to {Print(string.Join("|", value))}.";
  }
  public static string Attacks(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value);
    Actions.SetString(view, str, Hash.Attacks);
    return $"¤ attacks set to {Print(string.Join("|", value))}.";
  }
  public static string MinLevel(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MinLevel);
    return $"¤ minimum level set to {Print(value)}.";
  }
  public static string MaxLevel(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MaxLevel);
    return $"¤ maximum level set to {Print(value)}.";
  }
  public static string MinAmount(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MinAmount);
    return $"¤ minimum amount set to {Print(value)}.";
  }

  public static string Level(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.Level);
    return $"¤ level set to {Print(value)}.";
  }
  public static string Health(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.Health);
    return $"¤ health set to {Print(value)}.";
  }
  public static string MaxAmount(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MaxAmount);
    return $"¤ maximum amount set to {Print(value)}.";
  }
  public static string Amount(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.Amount);
    return $"¤ amount set to {Print(value)}.";
  }
  public static string MaxNear(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MaxNear);
    return $"¤ max near set to {Print(value)}.";
  }
  public static string MaxTotal(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MaxTotal);
    return $"¤ max total set to {Print(value)}.";
  }
  public static string SpawnCondition(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.SpawnCondition);
    return $"¤ spawn condition set to {Print(value)}.";
  }
  public static string MaxFuel(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.MaxFuel);
    return $"¤ maximum fuel set to {Print(value)}.";
  }
  public static string FuelUsage(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.FuelUsage);
    return $"¤ fuel usage set to {Print(value)}.";
  }
  public static string Speed(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.Speed);
    return $"¤ speed set to {Print(value)} seconds.";
  }
  public static string MaxCover(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.MaxCover);
    return $"¤ max cover set to {Print(value)} seconds.";
  }
  public static string Respawn(ZNetView view, int hash, float? value)
  {
    Actions.SetFloat(view, value, hash);
    return $"¤ respawn time set to {Print(value)} minutes.";
  }
  public static string Delay(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.Delay);
    return $"¤ delay set to {Print(value)} seconds.";
  }
  public static string ItemStandRange(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.ItemStandRange);
    return $"¤ item stand range set to {Print(value)} meters.";
  }
  public static string SpawnMaxY(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.SpawnMaxY);
    return $"¤ spawn max y set to {Print(value)} meters.";
  }
  public static string RespawnSeconds(ZNetView view, int hash, float? value)
  {
    Actions.SetFloat(view, value, hash);
    return $"¤ respawn time set to {Print(value)} seconds.";
  }

  public static string SpawnHealth(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.SpawnHealth);
    return $"¤ spawn health set to {Print(value)}.";
  }
  public static string TriggerDistance(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.TriggerDistance);
    return $"¤ trigger distance set to {Print(value)}.";
  }
  public static string TriggerNoise(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.TriggerNoise);
    return $"¤ trigger noise set to {Print(value)} meters.";
  }
  public static string LevelChance(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.LevelChance);
    return $"¤ level up chance set to {Print(value)} %.";
  }
  public static string SpawnRadius(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.SpawnRadius);
    return $"¤ spawn radius set to {Print(value)} meters.";
  }
  public static string NearRadius(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.NearRadius);
    return $"¤ near radius set to {Print(value)} meters.";
  }
  public static string FarRadius(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.FarRadius);
    return $"¤ far radius set to {Print(value)} meters.";
  }
  public static string SpawnOffset(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.SpawnOffset);
    return $"¤ spawn offset to {Print(value)} meters.";
  }
  public static string Destroy(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.Destroy);
    return $"¤ timed destroy set to {Print(value)} seconds.";
  }
}