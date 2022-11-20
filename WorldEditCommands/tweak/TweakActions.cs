using System;
using System.Linq;

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
public enum BossAffix
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
public static class TweakActions
{
  private const string DEFAULT = "default";
  private static string HashFirst(string value)
  {
    var split = value.Split(',');
    split[0] = Actions.GetId(split[0]).ToString();
    return string.Join(",", split);
  }

  private static string Print<T>(T? value) => value == null ? DEFAULT : value.ToString();

  public static string Render(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Render);
    return $"¤ render set to {Print(value)}.";
  }
  public static string Interact(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Interact);
    return $"¤ interact set to {Print(value)}.";
  }
  public static string Collision(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Collision);
    return $"¤ collision set to {Print(value)}.";
  }
  public static string Wear(ZNetView view, string? value)
  {
    Actions.SetInt(view, WearNumber(value), Hash.Wear);
    return $"¤ wear set to {Print(value)}.";
  }
  private static int WearNumber(string? wear)
  {
    if (wear == "broken") return 0;
    if (wear == "damaged") return 1;
    if (wear == "healthy") return 2;
    return -1;
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
  public static string Weather(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Weather);
    return $"¤ weather set to {Print(value)}.";
  }
  public static string Water(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Water);
    return $"¤ water set to {Print(value)}.";
  }

  public static string Fall(ZNetView view, string? value)
  {
    Actions.SetInt(view, FallNumber(value), Hash.Fall);
    return $"¤ fall set to {Print(value)}.";
  }
  private static int FallNumber(string? fall)
  {
    if (fall == null) return -1;
    if (fall == "off") return 0;
    if (fall == "terrain") return 1;
    if (fall == "solid") return 2;
    return -1;
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
    if (growth == "big") return 0;
    if (growth == "big_bad") return 1;
    if (growth == "small") return 2;
    if (growth == "small_bad") return 3;
    return -1;
  }

  public static string Component(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.Component);
    return $"¤ component set to {Print(value)}.";
  }

  public static string Restrict(ZNetView view, bool? value)
  {
    Actions.SetBool(view, value, Hash.Restrict);
    return $"¤ restrict set to {Print(value)}.";
  }
  public static string Unlock(ZNetView view, bool? value)
  {
    value = Actions.ToggleBool(view, value, Hash.Unlock);
    return $"¤ unlock set to {Print(value)}.";
  }
  public static string Creator(ZNetView view, long? value)
  {
    Actions.SetCreator(view, value ?? 0);
    return $"¤ unlock set to {Print(value)}.";
  }
  public static string Smoke(ZNetView view, string? value)
  {
    int val = -1;
    if (value == "off") val = 0;
    if (value == "ignore") val = 1;
    Actions.SetInt(view, val, Hash.Smoke);
    return $"¤ smoke set to {Print(value)}.";
  }
  public static string CLLC(ZNetView view, string[] values)
  {
    var cllc = BossAffix.None;
    foreach (var value in values)
    {
      if (Enum.TryParse<BossAffix>(value, true, out var affix)) cllc |= affix;
    }
    Actions.SetInt(view, (int)cllc, Hash.CLLC_Affix);
    return $"¤ affix set to {Print(cllc)}.";
  }
  public static string Spawn(ZNetView view, string? value)
  {
    Actions.SetPrefab(view, value, Hash.Spawn);
    return $"¤ spawn prefab set to {Print(value)}.";
  }

  public static string ItemOffset(ZNetView view, string? value)
  {
    Actions.SetString(view, value, Hash.ItemOffset);
    return $"¤ item offset set to {Print(value)}.";
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
    value = value == null ? value : value.Replace("_", " ");
    Actions.SetString(view, value, Hash.OverrideName);
    return $"¤ name set to {Print(value)}.";
  }
  public static string Text(ZNetView view, string? value)
  {
    value = value == null ? value : value.Replace("_", " ");
    Actions.SetString(view, value, Hash.OverrideText);
    return $"¤ text set to {Print(value)}.";
  }
  public static string Compendium(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value).Replace("_", " ");
    Actions.SetString(view, str, Hash.Compendium);
    return $"¤ compendium set to {Print(value)}.";
  }
  public static string Topic(ZNetView view, string? value)
  {
    value = value == null ? value : value.Replace("_", " ");
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
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.Spawn);
    return $"¤ spawn prefabs set to {Print(str)}.";
  }
  public static string Items(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.OverrideItems);
    return $"¤ items set to {Print(str)}.";
  }
  public static string SpawnEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.SpawnEffect);
    return $"¤ spawn effect set to {Print(str)}.";
  }
  public static string UseEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.UseEffect);
    return $"¤ use effect set to {Print(str)}.";
  }
  public static string StartEffect(ZNetView view, string[] value)
  {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
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
    return $"¤ max near set to {Print(value)}.";
  }
  public static string SpawnCondition(ZNetView view, int? value)
  {
    Actions.SetInt(view, value, Hash.SpawnCondition);
    return $"¤ spawn condition set to {Print(value)}.";
  }
  public static string Respawn(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.Respawn);
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
  public static string RespawnSeconds(ZNetView view, float? value)
  {
    Actions.SetFloat(view, value, Hash.Respawn);
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

}