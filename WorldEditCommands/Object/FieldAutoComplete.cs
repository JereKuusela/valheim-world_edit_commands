using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands;

class ObjectHash { }
class LocationHash { }
class RoomHash { }

public class FieldAutoComplete
{
  public static Dictionary<string, Dictionary<string, Type>> Fields = [];
  public static Dictionary<string, List<string>> Keys = [];
  private static List<string> Components = [];

  public static void Init()
  {
    // Easier to always load everything at start up, doesn't take long anyways.
    Fields = LoadFields();
    InitZdoFields();
    foreach (var kvp in Fields)
    {
      if (!Keys.ContainsKey(kvp.Key))
        Keys[kvp.Key] = [];
      foreach (var kvp2 in kvp.Value)
      {
        var key = kvp2.Key.Replace("m_", "");
        if (Keys[kvp.Key].Contains(key)) continue;
        Keys[kvp.Key].Add(key);
      }
    }
    foreach (var kvp in ZdoFields)
    {
      if (!Keys.ContainsKey(kvp.Key))
        Keys[kvp.Key] = [];
      foreach (var kvp2 in kvp.Value)
      {
        if (Keys[kvp.Key].Contains(kvp2.Key)) continue;
        Keys[kvp.Key].Add(kvp2.Key);
      }
    }
    Components = [.. Keys.Keys];
    Components = Components.Distinct().ToList();
  }
  private static readonly HashSet<Type> ValidTypes = [
    typeof(string),
    typeof(int),
    typeof(float),
    typeof(bool),
    typeof(Vector3),
    typeof(GameObject)
  ];
  private static readonly HashSet<Type> ValidTweakTypes = [
    ..ValidTypes,
    typeof(ItemDrop),
    typeof(EffectList)
  ];
  private static Dictionary<string, Dictionary<string, Type>> LoadFields()
  {
    Dictionary<string, Dictionary<string, Type>> fields = [];
    var types = ComponentInfo.Types;
    var valid = WorldEditCommands.IsTweaks ? ValidTweakTypes : ValidTypes;
    foreach (var type in types)
    {
      if (!fields.ContainsKey(type.Name))
        fields[type.Name] = [];
      foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        if (!valid.Contains(fieldInfo.FieldType)) continue;
        fields[type.Name][fieldInfo.Name] = fieldInfo.FieldType;
        PreventStripping.AddKey(type, fieldInfo);
      }
      if (fields[type.Name].Count == 0)
        fields.Remove(type.Name);
    }
    return fields;
  }
  private static Dictionary<string, Dictionary<string, Type>> LoadTweakFields()
  {
    Dictionary<string, Dictionary<string, Type>> fields = [];
    var types = ComponentInfo.Types;
    var valid = ValidTweakTypes;
    foreach (var type in types)
    {
      if (!fields.ContainsKey(type.Name))
        fields[type.Name] = [];
      foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        if (!fieldInfo.FieldType.IsEnum && !valid.Contains(fieldInfo.FieldType)) continue;
        fields[type.Name][fieldInfo.Name] = fieldInfo.FieldType;
        PreventStripping.AddKey(type, fieldInfo);
      }
      if (fields[type.Name].Count == 0)
        fields.Remove(type.Name);
    }
    return fields;
  }
  public static List<string> GetComponents()
  {
    var prefab = PrefabFromCommand(GetInput());
    return GetComponents(prefab);
  }
  private static string GetInput()
  {
    Aliasing.RestoreAlias(Console.m_instance.m_input);
    var text = Aliasing.Plain(Console.m_instance.m_input.text);
    Aliasing.RemoveAlias(Console.m_instance.m_input);
    return text;
  }
  public static string PrefabFromCommand(string command)
  {
    var split = command.Split(' ');
    var arg = split.LastOrDefault(s => s.StartsWith("id=", StringComparison.Ordinal));
    var prefab = "";
    if (!string.IsNullOrEmpty(arg))
      prefab = arg.Split('=')[1];
    else if (split.Length > 1 && ZNetScene.instance.m_namedPrefabs.ContainsKey(split[1].GetStableHashCode()))
      prefab = split[1];
    else
    {
      var selection = Player.m_localPlayer?.GetHoverObject();
      if (selection)
        prefab = Utils.GetPrefabName(selection);
    }
    return prefab;
  }
  public static List<string> GetComponents(string prefab) => GetComponents(prefab.GetStableHashCode());
  public static List<string> GetComponents(int prefab)
  {
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(prefab, out var gameObject))
      return [.. gameObject.GetComponentsInChildren<MonoBehaviour>().Select(c => c.GetType().Name), "zdo"];
    return Components;
  }
  public static List<string> GetFields()
  {
    var command = GetInput();
    var prefab = PrefabFromCommand(command);
    var component = ComponentFromCommand(prefab, command);
    return GetFields(component);
  }
  private static string ComponentFromCommand(string prefab, string command)
  {
    var split = command.Split(' ');
    var arg = split.LastOrDefault(s => s.StartsWith("field=", StringComparison.Ordinal) || s.StartsWith("f=", StringComparison.Ordinal));
    if (arg == null) return "";
    var component = arg.Split('=')[1].Split(',')[0];
    return RealComponent(prefab, component);
  }
  private static List<string> GetFields(string component)
  {
    return Keys.TryGetValue(component, out var keys) ? keys : [];
  }
  public static List<string> GetTypes(int index)
  {
    var tweaks = WorldEditCommands.IsTweaks;
    var command = GetInput();
    var prefab = PrefabFromCommand(command);
    var component = ComponentFromCommand(prefab, command);
    var field = FieldFromCommand(component, command);
    var realField = RealField(component, field, out var zdoField);
    var type = GetType(component, realField, zdoField);
    if (type == typeof(string)) return ["Text"];
    if (type == typeof(int)) return ["Number"];
    if (type == typeof(float)) return ["Decimal"];
    if (type == typeof(long)) return ["Timestamp"];
    if (type == typeof(bool)) return ["true", "false"];
    if (type == typeof(Vector3)) return ServerDevcommands.ParameterInfo.XZY("Field", index);
    if (type == typeof(Quaternion)) return ServerDevcommands.ParameterInfo.YXZ("Field", index);
    if (type == typeof(GameObject)) return tweaks ? GetIdsOrTransforms(prefab, component, field) : ServerDevcommands.ParameterInfo.Ids;
    if (type == typeof(ObjectHash)) return ServerDevcommands.ParameterInfo.ObjectIds;
    if (type == typeof(LocationHash)) return ServerDevcommands.ParameterInfo.LocationIds;
    if (type == typeof(RoomHash)) return ServerDevcommands.ParameterInfo.RoomIds;
    if (tweaks && type == typeof(ItemDrop)) return ServerDevcommands.ParameterInfo.ItemIds;
    if (tweaks && type.IsEnum) return [.. Enum.GetNames(type)];
    if (tweaks && type == typeof(EffectList)) return ServerDevcommands.ParameterInfo.Ids;
    return [];
  }
  private static List<string> GetIdsOrTransforms(string prefab, string component, string field)
  {
    var obj = ZNetScene.instance.GetPrefab(prefab);
    if (!obj) return ServerDevcommands.ParameterInfo.Ids;
    var c = obj.GetComponents<MonoBehaviour>().FirstOrDefault(c => c.GetType().Name == component);
    if (c == null) return ServerDevcommands.ParameterInfo.Ids;
    var f = c.GetType().GetField(RealField(component, field, out _));
    if (f == null || f.FieldType != typeof(GameObject)) return ServerDevcommands.ParameterInfo.Ids;
    var go = (GameObject)f.GetValue(c);
    if (go && ZNetScene.instance.GetPrefab(go.name) == go) return ServerDevcommands.ParameterInfo.Ids;
    List<string> trs = [];
    GetChild("/", obj.transform, trs);
    return trs;
  }
  private static void GetChild(string prefix, Transform tr, List<string> trs)
  {
    foreach (Transform child in tr)
    {
      trs.Add($"{prefix}{child.name}");
      GetChild($"{prefix}{child.name}/", child, trs);
    }
  }
  private static string FieldFromCommand(string component, string command)
  {
    var split = command.Split(' ');
    var arg = split.LastOrDefault(s => s.StartsWith("field=", StringComparison.Ordinal) || s.StartsWith("f=", StringComparison.Ordinal));
    if (arg == null) return "";
    split = arg.Split('=')[1].Split(',');
    return split.Length < 2 ? "" : RealField(component, split[1], out _);
  }
  public static Type GetType(string component, string field, bool zdoField)
  {
    if (zdoField)
    {
      if (!ZdoFields.TryGetValue("zdo", out var zdo)) return typeof(void);
      if (!zdo.TryGetValue(field, out var type)) return typeof(void);
      return type;
    }
    else
    {
      if (!Fields.TryGetValue(component, out var fields)) return typeof(void);
      if (!fields.TryGetValue(field, out var type)) return typeof(void);
      return type;
    }
  }

  public static string RealComponent(string prefab, string component)
  {
    var components = GetComponents(prefab);
    return components.FirstOrDefault(s => s.Equals(component, StringComparison.OrdinalIgnoreCase))
    ?? components.FirstOrDefault(s => s.StartsWith(component, StringComparison.OrdinalIgnoreCase))
    ?? component;
  }
  public static string RealField(string component, string field, out bool zdoField)
  {
    zdoField = false;
    var f = $"m_{field}";
    var fields = Fields.TryGetValue(component, out var fs) ? fs.Keys.ToList() : [];
    var primaryField = fields.FirstOrDefault(s => s.Equals(f, StringComparison.OrdinalIgnoreCase));
    if (primaryField != null) return primaryField;
    zdoField = true;
    var zdoFields = ZdoFields.TryGetValue(component, out var zdo) ? zdo.Keys.ToList() : [];
    var secondaryField = zdoFields.FirstOrDefault(s => s.Equals(field, StringComparison.OrdinalIgnoreCase));
    if (secondaryField != null) return secondaryField;
    zdoField = false;
    var tertiaryField = fields.FirstOrDefault(s => s.StartsWith(f, StringComparison.OrdinalIgnoreCase) || s.StartsWith(field, StringComparison.OrdinalIgnoreCase));
    if (tertiaryField != null) return tertiaryField;
    zdoField = true;
    var quaternaryField = zdoFields.FirstOrDefault(s => s.StartsWith(field, StringComparison.OrdinalIgnoreCase));
    if (quaternaryField != null) return quaternaryField;
    return field;
  }

  private static void InitZdoFields()
  {
    if (ZdoFields.ContainsKey("zdo")) return;
    if (WorldEditCommands.IsCLLC)
    {
      var values = ZdoFields["MonsterAI"];
      values["CL&LC effect"] = typeof(Enum_CLLC_Effect);
      values["CL&LC infusion"] = typeof(Enum_CLLC_Infusion);
    }
    Dictionary<string, Type> zdoValues = [];
    foreach (var kvp in ZdoFields)
    {
      foreach (var kvp2 in kvp.Value)
        zdoValues[kvp2.Key] = kvp2.Value;
    }
    ZdoFields["zdo"] = zdoValues;
  }
  private static readonly Dictionary<string, Dictionary<string, Type>> ZdoFields = new(){
    {
      nameof(ArmorStand), new Dictionary<string, Type>
      {
        { "pose", typeof(int) },
        { "0_item", typeof(GameObject) },
        { "1_item", typeof(GameObject) },
        { "2_item", typeof(GameObject) },
        { "0_variant", typeof(int) },
        { "1_variant", typeof(int) },
        { "2_variant", typeof(int) }
      }
    },{
      nameof(AnimalAI), new Dictionary<string, Type>
      {
        { "aggravated", typeof(bool) },
        { "alert", typeof(bool) },
        { "bosscount", typeof(bool) },
        { "spawntime", typeof(long) },
        { "ShownAlertMessage", typeof(bool) },
        { "huntplayer", typeof(bool) },
        { "lastWorldTime", typeof(long) },
        { "spawnpoint", typeof(Vector3) },
        { "patrol", typeof(bool) },
        { "patrolPoint", typeof(Vector3) },
      }
    },{
      nameof(Bed), new Dictionary<string, Type>
      {
        { "owner", typeof(long) },
        { "ownerName", typeof(string) },
      }
    },{
      nameof(Beehive), new Dictionary<string, Type>
      {
        { "lastTime", typeof(long) },
        { "level", typeof(int) },
        { "product", typeof(float) },
      }
    },{
      nameof(Catapult), new Dictionary<string, Type>
      {
        { "Locked", typeof(bool) },
        { "visual", typeof(GameObject) },
      }
    },{
      nameof(CharacterAnimEvent), new Dictionary<string, Type>
      {
        { "LookTarget", typeof(Vector3) },
      }
    },{
      nameof(Cinder), new Dictionary<string, Type>
      {
        { "spread", typeof(bool) },
      }
    },{
      nameof(CinderSpawner), new Dictionary<string, Type>
      {
        { "spread", typeof(bool) },
      }
    },{
      nameof(Container), new Dictionary<string, Type>
      {
        { "addedDefaultItems", typeof(bool) },
        { "InUse", typeof(bool) },
        { "items", typeof(string) },
      }
    },{
      nameof(CookingStation), new Dictionary<string, Type>
      {
        { "fuel", typeof(float) },
        { "StartTime", typeof(long) },
        { "slot0", typeof(GameObject) },
        { "slot1", typeof(GameObject) },
        { "slot2", typeof(GameObject) },
        { "slotstatus0", typeof(int) },
        { "slotstatus1", typeof(int) },
        { "slotstatus2", typeof(int) },
      }
    },{
      nameof(Corpse), new Dictionary<string, Type>
      {
        { "timeOfDeath", typeof(long) },
      }
    },{
      nameof(CreatureSpawner), new Dictionary<string, Type>
      {
        { "alive_time", typeof(long) },
      }
    },{
      nameof(DungeonGenerator), new Dictionary<string, Type>
      {
        { "rooms", typeof(int) },
        { "room0", typeof(RoomHash) },
        { "room1", typeof(RoomHash) },
        { "room2", typeof(RoomHash) },
        { "room0_pos", typeof(Vector3) },
        { "room1_pos", typeof(Vector3) },
        { "room2_pos", typeof(Vector3) },
        { "room0_rot", typeof(Quaternion) },
        { "room1_rot", typeof(Quaternion) },
        { "room2_rot", typeof(Quaternion) },
      }
    },{
      nameof(Destructible), new Dictionary<string, Type>
      {
        { "health", typeof(float) },
      }
    },{
      nameof(Door), new Dictionary<string, Type>
      {
        { "state", typeof(int) },
      }
    },{
      nameof(EggGrow), new Dictionary<string, Type>
      {
        { "GrowStart", typeof(float) },
      }
    },{
      nameof(Fermenter), new Dictionary<string, Type>
      {
        { "Content", typeof(GameObject) },
        { "StartTime", typeof(long) },
      }
    },{
      nameof(Fireplace), new Dictionary<string, Type>
      {
        { "fuel", typeof(float) },
        { "lastTime", typeof(long) },
      }
    },{
      nameof(Fish), new Dictionary<string, Type>
      {
        { "escape", typeof(float) },
        { "hooked", typeof(bool) },
        { "spawnpoint", typeof(Vector3) },
      }
    },{
      nameof(FishingFloat), new Dictionary<string, Type>
      {
        { "Bait", typeof(GameObject) },
        { "rodOwner", typeof(long) },
      }
    },{
      nameof(Gibber), new Dictionary<string, Type>
      {
        { "HitPoint", typeof(Vector3) },
        { "HitDir", typeof(Vector3) },
      }
    },{
      nameof(Humanoid), new Dictionary<string, Type>
      {
        { "BodyVelocity", typeof(Vector3) },
        { "bosscount", typeof(bool) },
        { "health", typeof(float) },
        { "IsBlocking", typeof(bool) },
        { "level", typeof(int) },
        { "max_health", typeof(float) },
        { "noise", typeof(float) },
        { "RandomSkillFactor", typeof(float) },
        { "seed", typeof(int) },
        { "tamed", typeof(bool) },
      }
    },{
      nameof(ItemDrop), new Dictionary<string, Type>
      {
        { "crafterID", typeof(long) },
        { "crafterName", typeof(string) },
        { "data_0", typeof(string) },
        { "data_1", typeof(string) },
        { "data_2", typeof(string) },
        { "data__0", typeof(string) },
        { "data__1", typeof(string) },
        { "data__2", typeof(string) },
        { "durability", typeof(float) },
        { "pickedUp", typeof(bool) },
        { "quality", typeof(int) },
        { "spawntime", typeof(long) },
        { "stack", typeof(int) },
        { "variant", typeof(int) },
        { "worldLevel", typeof(int) },
      }
    },{
      nameof(ItemStand), new Dictionary<string, Type>
      {
        { "quality", typeof(int) },
        { "item", typeof(GameObject) },
        { "variant", typeof(int) },
      }
    },{
      nameof(Leviathan), new Dictionary<string, Type>
      {
        { "dead", typeof(bool) },
      }
    },{
      nameof(LineConnect), new Dictionary<string, Type>
      {
        { "line_slack", typeof(float) },
      }
    },{
      nameof(LocationProxy), new Dictionary<string, Type>
      {
        { "location", typeof(LocationHash) },
        { "seed", typeof(int) },
      }
    },{
      nameof(LootSpawner), new Dictionary<string, Type>
      {
        { "spawntime", typeof(long) },
      }
    },{
      nameof(MineRock), new Dictionary<string, Type>
      {
        { "Health0", typeof(float) },
        { "Health1", typeof(float) },
        { "Health2", typeof(float) },
      }
    },{
      nameof(MineRock5), new Dictionary<string, Type>
      {
        { "health", typeof(float) },
      }
    },{
      nameof(MonsterAI), new Dictionary<string, Type>
      {
        { "aggravated", typeof(bool) },
        { "alert", typeof(bool) },
        { "bosscount", typeof(bool) },
        { "DespawnInDay", typeof(bool) },
        { "EventCreature", typeof(bool) },
        { "huntplayer", typeof(bool) },
        { "lastWorldTime", typeof(long) },
        { "patrol", typeof(bool) },
        { "patrolPoint", typeof(Vector3) },
        { "ShownAlertMessage", typeof(bool) },
        { "sleeping", typeof(bool) },
        { "spawnpoint", typeof(Vector3) },
        { "spawntime", typeof(long) },
      }
    },{
      nameof(MusicLocation), new Dictionary<string, Type>
      {
        { "played", typeof(bool) },
      }
    },{
       nameof(Pickable), new Dictionary<string, Type>
      {
        { "picked", typeof(bool) },
        { "picked_time", typeof(long) },
      }
    },{
       nameof(PickableItem), new Dictionary<string, Type>
      {
        { "itemPrefab", typeof(ObjectHash) },
        { "itemStack", typeof(int) },
      }
    },{
      nameof(Piece), new Dictionary<string, Type>
      {
        { "creator", typeof(long) },
      }
    },{
      nameof(Plant), new Dictionary<string, Type>
      {
        { "seed", typeof(int) },
        { "plantTime", typeof(long) },
      }
    },{
      nameof(Player), new Dictionary<string, Type>
      {
        { "baseValue", typeof(int) },
        { "BodyVelocity", typeof(Vector3) },
        { "DebugFly", typeof(bool) },
        { "dodgeinv", typeof(bool) },
        { "dead", typeof(bool) },
        { "health", typeof(float) },
        { "IsBlocking", typeof(bool) },
        { "eitr", typeof(float) },
        { "emote", typeof(string) },
        { "emoteID", typeof(int) },
        { "emote_oneshot", typeof(bool) },
        { "inBed", typeof(bool) },
        { "noise", typeof(float) },
        { "playerID", typeof(long) },
        { "playerName", typeof(string) },
        { "pvp", typeof(bool) },
        { "stamina", typeof(float) },
        { "Stealth", typeof(float) },
        { "wakeup", typeof(bool) },
        { "WeaponLoaded", typeof(bool) },
      }
    },{
      nameof(PrivateArea), new Dictionary<string, Type>
      {
        { "creatorName", typeof(string) },
        { "enabled", typeof(bool) },
        { "permitted", typeof(int) },
        { "pu_id0", typeof(long) },
        { "pu_id1", typeof(long) },
        { "pu_id2", typeof(long) },
        { "pu_name0", typeof(string) },
        { "pu_name1", typeof(string) },
        { "pu_name2", typeof(string) },
      }
    },{
      nameof(Procreation), new Dictionary<string, Type>
      {
        { "lovePoints", typeof(int) },
        { "pregnant", typeof(long) },
      }
    },{
      nameof(Projectile), new Dictionary<string, Type>
      {
        { "visual", typeof(string) },
      }
    },{
      nameof(Ragdoll), new Dictionary<string, Type>
      {
        { "drops", typeof(int) },
        { "drop_hash0", typeof(ObjectHash) },
        { "drop_hash1", typeof(ObjectHash) },
        { "drop_hash2", typeof(ObjectHash) },
        { "drop_amount0", typeof(float) },
        { "drop_amount1", typeof(float) },
        { "drop_amount2", typeof(float) },
        { "Hue", typeof(float) },
        { "InitVel", typeof(Vector3) },
        { "Saturation", typeof(float) },
        { "Value", typeof(float) },
      }
    },{
      nameof(RandomFlyingBird), new Dictionary<string, Type>
      {
        { "landed", typeof(bool) },
        { "spawnpoint", typeof(Vector3) },
      }
    },{
      nameof(ResourceRoot), new Dictionary<string, Type>
      {
        { "lastTime", typeof(long) },
        { "level", typeof(float) },
      }
    },{
      nameof(Sadle), new Dictionary<string, Type>
      {
        { "user", typeof(long) },
      }
    },{
      nameof(SapCollector), new Dictionary<string, Type>
      {
        { "lastTime", typeof(long) },
        { "level", typeof(int) },
        { "product", typeof(float) },
      }
    },{
      nameof(SEMan), new Dictionary<string, Type>
      {
        { "seAttrib", typeof(int) },
      }
    },{
      nameof(ShieldGenerator), new Dictionary<string, Type>
      {
        { "fuel", typeof(float) },
        { "StartTime", typeof(long) },
      }
    },{
      nameof(Ship), new Dictionary<string, Type>
      {
        { "forward", typeof(int) },
        { "rudder", typeof(float) },
      }
    },{
      nameof(ShipConstructor), new Dictionary<string, Type>
      {
        { "user", typeof(long) },
      }
    },{
      nameof(Sign), new Dictionary<string, Type>
      {
        { "author", typeof(string) },
        { "text", typeof(string) },
      }
    },{
      nameof(Smelter), new Dictionary<string, Type>
      {
        { "accTime", typeof(float) },
        { "bakeTimer", typeof(float) },
        { "fuel", typeof(float) },
        { "item0", typeof(GameObject) },
        { "item1", typeof(GameObject) },
        { "item2", typeof(GameObject) },
        { "queued", typeof(int) },
        { "SpawnOre", typeof(GameObject) },
        { "SpawnAmount", typeof(int) },
        { "StartTime", typeof(long) },
      }
    },{
      nameof(Tameable), new Dictionary<string, Type>
      {
        { "HaveSaddle", typeof(bool) },
        { "TamedName", typeof(string) },
        { "TamedNameAuthor", typeof(string) },
      }
    },{
      nameof(TeleportWorld), new Dictionary<string, Type>
      {
        { "tag", typeof(string) },
        { "tagauthor", typeof(string) },
      }
    },{
      nameof(Trap), new Dictionary<string, Type>
      {
        { "state", typeof(int) },
        { "triggered", typeof(float) },
      }
    },{
      nameof(TreeBase), new Dictionary<string, Type>
      {
        { "health", typeof(float) },
      }
    },{
      nameof(TreeLog), new Dictionary<string, Type>
      {
        { "health", typeof(float) },
      }
    },{
      nameof(TriggerSpawner), new Dictionary<string, Type>
      {
        { "spawntime", typeof(long) },
      }
    },{
      nameof(Turret), new Dictionary<string, Type>
      {
        { "ammo", typeof(int) },
        { "ammoType", typeof(GameObject) },
        { "lastAttack", typeof(float) },
        { "targets", typeof(int) },
        { "target0", typeof(string) },
        { "target1", typeof(string) },
        { "target2", typeof(string) },
      }
    },{
      nameof(Vagon), new Dictionary<string, Type>
      {
        { "attachJoint", typeof(bool) },
      }
    },{
      nameof(WearNTear), new Dictionary<string, Type>
      {
        { "health", typeof(float) },
        { "support", typeof(float) },
      }
    },{
      nameof(WispSpawner), new Dictionary<string, Type>
      {
        { "LastSpawn", typeof(long) },
      }
    },
  };
}

[HarmonyPatch(typeof(ZDO))]
public class PreventStripping
{
  private static readonly HashSet<int> FieldZdoKeys = [];

  public static void AddKey(Type type, FieldInfo fieldInfo)
  {
    if (fieldInfo.FieldType == typeof(string) || fieldInfo.FieldType == typeof(int) || fieldInfo.FieldType == typeof(bool))
    {
      var key = $"{type.Name}.{fieldInfo.Name}";
      FieldZdoKeys.Add(key.GetStableHashCode());
    }
  }

  // Transpiling is more tricky but should be more performant.
  [HarmonyPatch(nameof(ZDO.Strip), typeof(int), typeof(string)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> String(IEnumerable<CodeInstruction> instructions, ILGenerator generator) => Transpile(instructions, generator);
  [HarmonyPatch(nameof(ZDO.Strip), typeof(int), typeof(int)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> Int(IEnumerable<CodeInstruction> instructions, ILGenerator generator) => Transpile(instructions, generator);

  private static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
  {
    return new CodeMatcher(instructions, generator)
      .CreateLabelAt(0, out var label)
      .Start()
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PreventStripping), nameof(FieldZdoKeys))))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HashSet<int>), nameof(HashSet<int>.Contains))))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label)) // Jump to the original code, otherwise return false.
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ret))
    .InstructionEnumeration();
  }

}