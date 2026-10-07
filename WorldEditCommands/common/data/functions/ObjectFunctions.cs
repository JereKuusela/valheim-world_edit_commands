// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using UnityEngine;

namespace Data;

/// <summary>Functions that read the data of an object (ZDO), for example &lt;string_key&gt; or &lt;pos&gt;.</summary>
public partial class ObjectFunctions(string prefab, string[] args, ZDO zdo) : Functions(prefab, args, zdo.m_position)
{
  // Hooks for functions that only some hosts have, hosts without them don't implement these.
  partial void GetHostFunction(string key, ref string? result);
  partial void GetHostValueFunction(string key, string value, string defaultValue, ref string? result);

  private List<ItemRecord>? inventory;

  // Lets other function sets delegate to this one.
  public string? Resolve(string key, string defaultValue) => GetFunction(key, defaultValue);

  protected override string? GetFunction(string key, string defaultValue)
  {
    var value = base.GetFunction(key, defaultValue);
    if (value != null) return value;
    value = GetObjectFunction(key);
    if (value != null) return value;
    GetHostFunction(key, ref value);
    return value;
  }

  protected override string? GetValueFunction(string key, string value, string defaultValue)
  {
    var result = GetObjectValueFunction(key, value, defaultValue);
    if (result != null) return result;
    GetHostValueFunction(key, value, defaultValue, ref result);
    return result ?? base.GetValueFunction(key, value, defaultValue);
  }

  private string? GetObjectFunction(string key) =>
    key switch
    {
      "zdo" => zdo.m_uid.ToString(),
      "pos" => Formatting.FormatPos(zdo.m_position),
      "i" => ZoneSystem.GetZone(zdo.m_position).x.ToString(),
      "j" => ZoneSystem.GetZone(zdo.m_position).y.ToString(),
      "a" => Formatting.Format(zdo.m_rotation.y),
      "rad" => Formatting.Format(zdo.m_rotation.y * Mathf.Deg2Rad),
      "deg" => Formatting.Format(zdo.m_rotation.y),
      "rot" => Formatting.FormatRot(zdo.m_rotation),
      "owner" => zdo.GetOwner().ToString(),
      "connected" => (zdo.GetConnection()?.m_target ?? ZDOID.None).ToString(),
      "biome" => WorldGenerator.instance.GetBiome(zdo.m_position).ToString(),
      "altbiome" => GetAltBiome(),
      "joints" => GetJoints(),
      _ => null,
    };

  private string? GetObjectValueFunction(string key, string value, string defaultValue) =>
    key switch
    {
      "string" => ZdoHelper.GetString(zdo, value, defaultValue),
      "float" => ZdoHelper.GetFloat(zdo, value, defaultValue).ToString(CultureInfo.InvariantCulture),
      "int" => ZdoHelper.GetInt(zdo, value, defaultValue).ToString(CultureInfo.InvariantCulture),
      "long" => ZdoHelper.GetLong(zdo, value, defaultValue).ToString(CultureInfo.InvariantCulture),
      "bool" => ZdoHelper.GetBool(zdo, value, defaultValue) ? "true" : "false",
      "hash" => GetHash(value, defaultValue),
      "vec" => Formatting.FormatPos(ZdoHelper.GetVec(zdo, value, defaultValue)),
      "quat" => Formatting.FormatRot(ZdoHelper.GetQuaternion(zdo, value, defaultValue).eulerAngles),
      "byte" => ZdoHelper.GetBytes(zdo, value, defaultValue),
      "zdo" => zdo.GetZDOID(value).ToString(),
      "amount" => GetAmount(value, defaultValue),
      "quality" => GetQuality(value, defaultValue),
      "durability" => GetDurability(value, defaultValue),
      "item" => GetItem(value, defaultValue),
      "pos" => Formatting.FormatPos(GetPos(value)),
      _ => null,
    };

  private string GetAltBiome()
  {
    var generator = WorldGenerator.instance;
    return GetBiomeName(generator.GetBiome(zdo.m_position), generator.GetBiomeSector(zdo.m_position).AltBiomes);
  }

  private static string GetBiomeName(Heightmap.Biome biome, List<AltBiome> altBiomes)
  {
    if (altBiomes.Count == 0) return biome.ToString();
    var names = altBiomes.Select(alt => alt.m_name).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().OrderBy(name => name, StringComparer.Ordinal).ToArray();
    return names.Length > 0 ? string.Join(", ", names) : biome.ToString();
  }

  private string GetJoints()
  {
    var obj = ZNetScene.instance.GetPrefab(zdo.m_prefab);
    if (obj == null) return "";
    Queue<Transform> queue = new();
    queue.Enqueue(obj.transform);
    List<string> jointNames = [];
    while (queue.Count > 0)
    {
      var current = queue.Dequeue();
      foreach (Transform child in current)
      {
        queue.Enqueue(child);
        jointNames.Add(child.name);
      }
    }
    return string.Join(", ", jointNames);
  }

  private string GetHash(string value, string defaultValue)
  {
    if (value == "") return defaultValue;
    var zdoValue = zdo.GetInt(value);
    return ZNetScene.instance.GetPrefab(zdoValue)?.name ?? ZoneSystem.instance.GetLocation(zdoValue)?.m_prefabName ?? defaultValue;
  }

  private Vector3 GetPos(string value)
  {
    var offset = Parse.VectorXZY(value);
    return zdo.GetPosition() + zdo.GetRotation() * offset;
  }

  // Item functions take either an item name or inventory coordinates.
  private string GetItem(string value, string defaultValue)
  {
    if (value == "") return defaultValue;
    if (!TryGetCoordinates(value, out var x, out var y)) return GetAmountOfItems(value).ToString();
    return GetItemAt(x, y)?.PrefabName ?? defaultValue;
  }
  private string GetAmount(string value, string defaultValue)
  {
    if (value == "") return defaultValue;
    if (!TryGetCoordinates(value, out var x, out var y)) return GetAmountOfItems(value).ToString();
    return GetItemAt(x, y)?.Stack.ToString() ?? defaultValue;
  }
  private string GetDurability(string value, string defaultValue)
  {
    if (!TryGetCoordinates(value, out var x, out var y)) return defaultValue;
    return GetItemAt(x, y)?.Durability.ToString() ?? defaultValue;
  }
  private string GetQuality(string value, string defaultValue)
  {
    if (!TryGetCoordinates(value, out var x, out var y)) return defaultValue;
    return GetItemAt(x, y)?.Quality.ToString() ?? defaultValue;
  }

  private static bool TryGetCoordinates(string value, out int x, out int y)
  {
    x = 0;
    y = 0;
    if (value == "") return false;
    var kvp = Parse.Kvp(value, Separator);
    return Parse.TryInt(kvp.Key, out x) && Parse.TryInt(kvp.Value, out y);
  }

  private int GetAmountOfItems(string name)
  {
    LoadInventory();
    if (inventory == null) return 0;
    if (name == "" || name == "*") return inventory.Sum(i => i.Stack);
    if (!Wildcard.IsPattern(name)) return inventory.Where(i => i.PrefabName == name).Sum(i => i.Stack);
    return inventory.Where(i => Wildcard.Match(i.PrefabName, name, true)).Sum(i => i.Stack);
  }

  private ItemRecord? GetItemAt(int x, int y)
  {
    LoadInventory();
    if (inventory == null || x < 0 || y < 0) return null;
    return inventory.FirstOrDefault(i => i.GridPos.x == x && i.GridPos.y == y);
  }

  private void LoadInventory()
  {
    inventory ??= ItemDataHelper.Load(zdo);
  }
}
