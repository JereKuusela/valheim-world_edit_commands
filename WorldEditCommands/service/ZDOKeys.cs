using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace Data;

// Provides reverse hash lookup for ZDO keys.

public class ZDOKeys
{
  private static string[]? knownKeys;
  private static string[] KnownKeys => knownKeys ??= GenerateKeys();
  private static string[] GenerateKeys()
  {
    List<string> keys = [.. StaticKeys];
    List<Assembly> assemblies = [Assembly.GetAssembly(typeof(ZNetView)), .. Chainloader.PluginInfos.Values.Where(p => p.Instance != null).Select(p => p.Instance.GetType().Assembly)];
    var baseType = typeof(MonoBehaviour);
    var types = assemblies.SelectMany(s =>
    {
      try
      {
        return s.GetTypes();
      }
      catch (ReflectionTypeLoadException e)
      {
        return e.Types.Where(t => t != null);
      }
    }).Where(t =>
    {
      try
      {
        return baseType.IsAssignableFrom(t);
      }
      catch
      {
        return false;
      }
    }).ToArray();
    keys.AddRange(types.Select(t => $"HasFields{t.Name}"));
    keys.AddRange(types.SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.Public).Select(f => $"{t.Name}.{f.Name}")));
    keys.AddRange(typeof(ZDOVars).GetFields(BindingFlags.Static | BindingFlags.Public).Select(f => f.Name.Replace("s_", "")));
    keys.AddRange(typeof(ZDOVars).GetFields(BindingFlags.Static | BindingFlags.Public).Select(f => FirstLetterUpper(f.Name.Replace("s_", ""))));
    for (var i = 0; i < 20; i++)
    {
      keys.Add($"MatVar{i}");
      keys.Add($"item{i}");
      keys.Add($"quality{i}");
      keys.Add($"variant{i}");
      keys.Add($"data_{i}");
      keys.Add($"data__{i}");
      keys.Add($"drop_hash{i}");
      keys.Add($"drop_amount{i}");
      keys.Add($"slot{i}");
      keys.Add($"slotstatus{i}");

      keys.Add($"{i}_item");
      keys.Add($"{i}_durability");
      keys.Add($"{i}_stack");
      keys.Add($"{i}_quality");
      keys.Add($"{i}_variant");
      keys.Add($"{i}_crafterID");
      keys.Add($"{i}_crafterName");
      keys.Add($"{i}_dataCount");
      for (var j = 0; j < 20; j++)
      {
        keys.Add($"{i}_data_{j}");
        keys.Add($"{i}_data__{j}");
      }
      keys.Add($"{i}_worldLevel");
      keys.Add($"{i}_pickedUp");

      keys.Add($"pu_id{i}");
      keys.Add($"pu_name{i}");
      keys.Add($"Health{i}");
      keys.Add($"room{i}");
      keys.Add($"room{i}_pos");
      keys.Add($"room{i}_rot");
      keys.Add($"target{i}");
    }
    keys.AddRange(AnimationKeys.Select(k => $"${k}"));
    return [.. keys.Distinct()];
  }
  public static bool IsRoomPos(string key) => key.EndsWith("_pos") && key.StartsWith("room");
  public static bool IsRoomRot(string key) => key.EndsWith("_rot") && key.StartsWith("room");
  private static string FirstLetterUpper(string s) => char.ToUpper(s[0]) + s.Substring(1);
  private static readonly string[] StaticKeys = [
    "alive_time",
    "attachJoint",
    "body_avel",
    "body_vel",
    "bosscount",
    "emote_oneshot",
    "HaveSaddle",
    "haveTarget",
    "IsBlocking",
    "max_health",
    "picked_time",
    "relPos",
    "relRot",
    "scale",
    "scaleScalar",
    "vel",
    "lastWorldTime",
    "spawntime",
    "spawnpoint",
    "HasFields",
    "user_u",
    "user_i",
    "RodOwner_u",
    "RodOwner_i",
    "CatchID_u",
    "CatchID_i",
    "target_u",
    "target_i",
    "rooms",
    "spawn_id_u",
    "spawn_id_i",
    "parent_id_u",
    "parent_id_i",
    // CLLC mod
    "CL&LC effect",
    "CL&LC infusion",
    // Structure / Spawner Tweaks
    "override_amount",
    "override_attacks",
    "override_biome",
    "override_boss",
    "override_collision",
    "override_compendium",
    "override_component",
    "override_conversion",
    "override_cover_offset",
    "override_data",
    "override_delay",
    "override_destroy",
    "override_discover",
    "override_dungeon_enter_hover",
    "override_dungeon_enter_text",
    "override_dungeon_exit_hover",
    "override_dungeon_exit_text",
    "override_dungeon_weather",
    "override_effect",
    "override_event",
    "override_faction",
    "override_fall",
    "override_far_radius",
    "override_fuel",
    "override_fuel_effect",
    "override_globalkey",
    "override_growth",
    "override_health",
    "override_input_effect",
    "override_interact",
    "override_item",
    "override_item_offset",
    "override_item_stand_prefix",
    "override_item_stand_range",
    "override_items",
    "override_level_chance",
    "override_maximum_amount",
    "override_maximum_cover",
    "override_maximum_fuel",
    "override_maximum_level",
    "override_max_near",
    "override_max_total",
    "override_minimum_amount",
    "override_minimum_level",
    "override_name",
    "override_near_radius",
    "override_output_effect",
    "override_pickable_spawn",
    "override_pickable_respawn",
    "override_render",
    "override_resistances",
    "override_respawn",
    "override_restrict",
    "override_smoke",
    "override_spawn",
    "override_spawn_condition",
    "override_spawn_effect",
    "override_spawn_max_y",
    "override_spawn_offset",
    "override_spawn_radius",
    "override_spawnarea_spawn",
    "override_spawnarea_respawn",
    "override_spawn_item",
    "override_start_effect",
    "override_text",
    "override_text_biome",
    "override_text_check",
    "override_text_extract",
    "override_text_happy",
    "override_text_sleep",
    "override_text_space",
    "override_topic",
    "override_trigger_distance",
    "override_trigger_noise",
    "override_unlock",
    "override_use_effect",
    "override_water",
    "override_wear",
    "override_weather",
    // Marketplace
    "KGmarketNPC",
    "KGnpcProfile",
    "KGnpcModelOverride",
    "KGnpcNameOverride",
    "KGnpcDialogue",
    "KGleftItem",
    "KGrightItem",
    "KGhelmetItem",
    "KGchestItem",
    "KGlegsItem",
    "KGcapeItem",
    "KGhairItem",
    "KGhairItemColor",
    "KGLeftItemBack",
    "KGRightItemBack",
    "KGinteractAnimation",
    "KGgreetingAnimation",
    "KGbyeAnimation",
    "KGgreetingText",
    "KGbyeText",
    "KGskinColor",
    "KGcraftingAnimation",
    "KGbeardItem",
    "KGbeardColor",
    "KGinteractSound",
    "KGtextSize",
    "KGtextHeight",
    "KGperiodicAnimation",
    "KGperiodicAnimationTime",
    "KGperiodicSound",
    "KGperiodicSoundTime",
    "KGnpcScale",
    // Item Stand All Items
    "itemstand_hide",
    "itemstand_offset",
    "itemstand_rotation",
    "itemstand_scale",
    // X Ray vision
    "steamName",
    "steamID",
    "xray_created",
    ];

  private static readonly string[] AnimationKeys = [
    "alert",
    "footstep",
    "forward_speed",
    "sideway_speed",
    "anim_speed",
    "statef",
    "statei",
    "blocking",
    "attack",
    "flapping",
    "falling",
    "onGround",
    "intro",
    "crouching",
    "encumbered",
    "equipping",
    "attach_bed",
    "attach_chair",
    "attach_throne",
    "attach_sitship",
    "attach_mast",
    "attach_dragon",
    "attach_lox",
    "bow_aim",
    "reload_crossbow",
    "crafting",
    "visible",
    "turn_speed",
    "idle",
    "flying",
    "body_forward_speed",
    "inWater",
    "onGround",
    "minoraction",
    "minoraction_fast",
    "emote",
  ];

  private static Dictionary<string, int> keyToHash = [];
  private static Dictionary<int, string>? hashToKey;
  private static Dictionary<int, string> HashToKey => hashToKey ??= KnownKeys.ToDictionary(CalculateHash, x => x);
  public static string Convert(int hash) => HashToKey.TryGetValue(hash, out var key) ? key : hash.ToString();
  public static int Hash(string key)
  {
    if (keyToHash.TryGetValue(key, out var hash)) return hash;
    hash = CalculateHash(key);
    keyToHash[key] = hash;
    HashToKey[hash] = key;
    return hash;
  }
  private static int CalculateHash(string key)
  {
    if (key.StartsWith("$", StringComparison.InvariantCultureIgnoreCase))
    {
      var hash = ZSyncAnimation.GetHash(key.Substring(1));
      // Animation keys are offset by 438569, except for $anim_speed.
      if (key == "$anim_speed") return hash;
      return hash + 438569;
    }
    return key.GetStableHashCode();
  }
  public static bool Exists(int hash) => hashToKey == null || HashToKey.ContainsKey(hash);

}
/* Code for collecting new keys.
[HarmonyPatch(typeof(ZDO))]
public class KeyCollector
{

  [HarmonyPatch(nameof(ZDO.Set), typeof(int), typeof(string)), HarmonyPrefix]
  static void String(int hash)
  {
    if (ZDOKeys.Exists(hash)) return;
    var stack = new System.Diagnostics.StackTrace();
    Debug.LogWarning($"Found new string key: {hash}\n{stack}");
  }
  [HarmonyPatch(nameof(ZDO.Set), typeof(int), typeof(int)), HarmonyPrefix]
  static void Int(int hash)
  {
    if (ZDOKeys.Exists(hash)) return;
    var stack = new System.Diagnostics.StackTrace();
    Debug.LogWarning($"Found new int key: {hash}\n{stack}");
  }
  [HarmonyPatch(nameof(ZDO.Set), typeof(int), typeof(long)), HarmonyPrefix]
  static void Long(int hash)
  {
    if (ZDOKeys.Exists(hash)) return;
    var stack = new System.Diagnostics.StackTrace();
    Debug.LogWarning($"Found new long key: {hash}\n{stack}");
  }
  [HarmonyPatch(nameof(ZDO.Set), typeof(int), typeof(float)), HarmonyPrefix]
  static void Float(int hash)
  {
    if (ZDOKeys.Exists(hash)) return;
    var stack = new System.Diagnostics.StackTrace();
    Debug.LogWarning($"Found new float key: {hash}\n{stack}");
  }
  [HarmonyPatch(nameof(ZDO.Set), typeof(int), typeof(Vector3)), HarmonyPrefix]
  static void Vector3(int hash)
  {
    if (ZDOKeys.Exists(hash)) return;
    var stack = new System.Diagnostics.StackTrace();
    Debug.LogWarning($"Found new Vector3 key: {hash}\n{stack}");
  }
}
[HarmonyPatch(typeof(ZSyncAnimation), nameof(ZSyncAnimation.GetHash))]
public class ZSyncAnimation_GetHash_Patch
{
  static void Postfix(string name, int __result)
  {
    if (ZDOKeys.Exists(__result + 438569)) return;
    Debug.LogWarning($"Found new animation key: {name}");
  }
}
*/