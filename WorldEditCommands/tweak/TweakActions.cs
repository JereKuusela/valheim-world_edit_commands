using System.Linq;

namespace WorldEditCommands;
public static class TweakActions {
  private const string DEFAULT = "default";
  private static string HashFirst(string value) {
    var split = value.Split(',');
    split[0] = Actions.GetId(split[0]).ToString();
    return string.Join(",", split);
  }

  private static string Print<T>(T? value) => value == null ? DEFAULT : value.ToString();

  public static string Restrict(ZNetView view, bool? value) {
    Actions.SetBool(view, value, Hash.Restrict);
    return $"¤ restrict set to {Print(value)}.";
  }
  public static string Unlock(ZNetView view, bool? value) {
    value = Actions.ToggleBool(view, value, Hash.Unlock);
    return $"¤ unlock set to {Print(value)}.";
  }
  public static string Smoke(ZNetView view, string? value) {
    bool? val = null;
    if (value == "off") val = false;
    if (value == "ignore") val = true;
    Actions.SetBool(view, val, Hash.Smoke);
    return $"¤ restrict set to {Print(value)}.";
  }
  public static string Spawn(ZNetView view, string? value) {
    Actions.SetPrefab(view, value, Hash.Spawn);
    return $"¤ spawn prefab set to {Print(value)}.";
  }

  public static string ItemOffset(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.ItemOffset);
    return $"¤ item offset set to {Print(value)}.";
  }
  public static string ItemStandPrefix(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.ItemStandPrefix);
    return $"¤ item stand prefix set to {Print(value)}.";
  }
  public static string GlobalKey(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.GlobalKey);
    return $"¤ global key set to {Print(value)}.";
  }
  public static string Name(ZNetView view, string? value) {
    value = value == null ? value : value.Replace("_", " ");
    Actions.SetString(view, value, Hash.OverrideName);
    return $"¤ name set to {Print(value)}.";
  }
  public static string Text(ZNetView view, string? value) {
    value = value == null ? value : value.Replace("_", " ");
    Actions.SetString(view, value, Hash.OverrideText);
    return $"¤ text set to {Print(value)}.";
  }
  public static string Compendium(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value).Replace("_", " ");
    Actions.SetString(view, str, Hash.Compendium);
    return $"¤ compendium set to {Print(value)}.";
  }
  public static string Topic(ZNetView view, string? value) {
    value = value == null ? value : value.Replace("_", " ");
    Actions.SetString(view, value, Hash.Topic);
    return $"¤ topic set to {Print(value)}.";
  }
  public static string Discover(ZNetView view, string? value) {
    Actions.SetString(view, value, Hash.Discover);
    return $"¤ discover set to {Print(value)}.";
  }
  public static string Spawns(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.Spawn);
    return $"¤ spawn prefabs set to {Print(str)}.";
  }
  public static string SpawnEffect(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.SpawnEffect);
    return $"¤ spawn effect set to {Print(str)}.";
  }
  public static string UseEffect(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.UseEffect);
    return $"¤ use effect set to {Print(str)}.";
  }
  public static string StartEffect(ZNetView view, string[] value) {
    var str = value.Length == 0 ? null : string.Join("|", value.Select(HashFirst));
    Actions.SetString(view, str, Hash.StartEffect);
    return $"¤ start effect set to {Print(str)}.";
  }
  public static string SpawnItem(ZNetView view, string? value) {
    Actions.SetPrefab(view, value, Hash.SpawnItem);
    return $"¤ spawn item prefab set to {Print(value)}.";
  }
  public static string MinLevel(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MinLevel);
    return $"¤ minimum level set to {Print(value)}.";
  }
  public static string MaxLevel(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MaxLevel);
    return $"¤ maximum level set to {Print(value)}.";
  }
  public static string Amount(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.Amount);
    return $"¤ amount set to {Print(value)}.";
  }
  public static string MaxNear(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MaxNear);
    return $"¤ max near set to {Print(value)}.";
  }
  public static string MaxTotal(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.MaxTotal);
    return $"¤ max near set to {Print(value)}.";
  }
  public static string SpawnCondition(ZNetView view, int? value) {
    Actions.SetInt(view, value, Hash.SpawnCondition);
    return $"¤ spawn condition set to {Print(value)}.";
  }
  public static string Respawn(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.Respawn);
    return $"¤ respawn time set to {Print(value)} minutes.";
  }
  public static string Delay(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.Delay);
    return $"¤ delay set to {Print(value)} seconds.";
  }
  public static string ItemStandRange(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.ItemStandRange);
    return $"¤ item stand range set to {Print(value)} meters.";
  }
  public static string SpawnMaxY(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnMaxY);
    return $"¤ spawn max y set to {Print(value)} meters.";
  }
  public static string RespawnSeconds(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.Respawn);
    return $"¤ respawn time set to {Print(value)} seconds.";
  }
  public static string SpawnHealth(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnHealth);
    return $"¤ spawn health set to {Print(value)}.";
  }
  public static string TriggerDistance(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.TriggerDistance);
    return $"¤ trigger distance set to {Print(value)}.";
  }
  public static string TriggerNoise(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.TriggerNoise);
    return $"¤ trigger noise set to {Print(value)} meters.";
  }
  public static string LevelChance(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.LevelChance);
    return $"¤ level up chance set to {Print(value)} %.";
  }
  public static string SpawnRadius(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnRadius);
    return $"¤ spawn radius set to {Print(value)} meters.";
  }
  public static string NearRadius(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.NearRadius);
    return $"¤ near radius set to {Print(value)} meters.";
  }
  public static string FarRadius(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.FarRadius);
    return $"¤ far radius set to {Print(value)} meters.";
  }
  public static string SpawnOffset(ZNetView view, float? value) {
    Actions.SetFloat(view, value, Hash.SpawnOffset);
    return $"¤ spawn offset to {Print(value)} meters.";
  }

}