using System.Linq;

namespace WorldEditCommands;
public static class TweakActions {
  private const string DEFAULT = "default";
  private static string HashFirst(string value) {
    var split = value.Split(',');
    split[0] = split[0].GetStableHashCode().ToString();
    return string.Join(",", split);
  }

  private static string Print<T>(T? value) => value == null ? DEFAULT : value.ToString();

  public static string Spawn(ZNetView view, string? value) {
    Actions.SetPrefab(view, value, Hash.Spawn, true);
    return $"¤ spawn prefab set to {Print(value)}.";
  }

  public static string ItemOffset(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.ItemOffset, true);
    return $"¤ item offset set to {Print(value)}.";
  }
  public static string ItemStandPrefix(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.ItemStandPrefix, true);
    return $"¤ item stand prefix set to {Print(value)}.";
  }
  public static string GlobalKey(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.GlobalKey, true);
    return $"¤ global key set to {Print(value)}.";
  }
  public static string Name(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.OverrideName, true);
    return $"¤ name set to {Print(value)}.";
  }
  public static string Text(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.OverrideText, true);
    return $"¤ text set to {Print(value)}.";
  }
  public static string Spawns(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.Spawn, true);
    return $"¤ spawn prefabs set to {Print(str)}.";
  }
  public static string SpawnEffect(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.SpawnEffect, true);
    return $"¤ spawn effect set to {Print(str)}.";
  }
  public static string UseEffect(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.UseEffect, true);
    return $"¤ use effect set to {Print(str)}.";
  }
  public static string StartEffect(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.StartEffect, true);
    return $"¤ start effect set to {Print(str)}.";
  }
  public static string SpawnItem(ZNetView view, string? value) {
    Actions.SetPrefab(view, value, Hash.SpawnItem, true);
    return $"¤ spawn item prefab set to {Print(value)}.";
  }
  public static string MinLevel(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MinLevel, true);
    return $"¤ minimum level set to {Print(value)}.";
  }
  public static string MaxLevel(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MaxLevel, true);
    return $"¤ maximum level set to {Print(value)}.";
  }
  public static string Amount(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.Amount, true);
    return $"¤ amount set to {Print(value)}.";
  }
  public static string MaxNear(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MaxNear, true);
    return $"¤ max near set to {Print(value)}.";
  }
  public static string MaxTotal(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MaxTotal, true);
    return $"¤ max near set to {Print(value)}.";
  }
  public static string SpawnCondition(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.SpawnCondition, true);
    return $"¤ spawn condition set to {Print(value)}.";
  }
  public static string Respawn(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.Respawn, true);
    return $"¤ respawn time set to {Print(value)} minutes.";
  }
  public static string Delay(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.Delay, true);
    return $"¤ delay set to {Print(value)} seconds.";
  }
  public static string ItemStandRange(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.ItemStandRange, true);
    return $"¤ item stand range set to {Print(value)} meters.";
  }
  public static string SpawnMaxY(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnMaxY, true);
    return $"¤ spawn max y set to {Print(value)} meters.";
  }
  public static string RespawnSeconds(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.Respawn, true);
    return $"¤ respawn time set to {Print(value)} seconds.";
  }
  public static string SpawnHealth(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnHealth, true);
    return $"¤ spawn health set to {Print(value)}.";
  }
  public static string TriggerDistance(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnHealth, true);
    return $"¤ trigger distance set to {Print(value)}.";
  }
  public static string TriggerNoise(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.TriggerNoise, true);
    return $"¤ trigger noise set to {Print(value)} meters.";
  }
  public static string LevelChance(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.TriggerDistance, true);
    return $"¤ level up chance set to {Print(value)} meters.";
  }
  public static string SpawnRadius(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnRadius, true);
    return $"¤ spawn radius set to {Print(value)} meters.";
  }
  public static string NearRadius(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.NearRadius, true);
    return $"¤ near radius set to {Print(value)} meters.";
  }
  public static string FarRadius(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.FarRadius, true);
    return $"¤ far radius set to {Print(value)} meters.";
  }
  public static string SpawnOffset(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnOffset, true);
    return $"¤ spawn offset to {Print(value)} meters.";
  }

}