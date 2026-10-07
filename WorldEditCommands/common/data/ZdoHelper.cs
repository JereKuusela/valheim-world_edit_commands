// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using System.Globalization;
using Common;
using UnityEngine;

namespace Data;

/// <summary>
/// Helpers for reading ZDO values by key. Hosts implement the TryGet* methods, because some have additional data sources.
/// Values missing from the ZDO fall back to the field of the prefab component, for example "Container.m_width".
/// </summary>
public static partial class ZdoHelper
{
  // Hooks for hosts that track or know more keys.
  static partial void OnHashed(string key, int hash);
  static partial void FindKnownKey(int hash, ref string? key);

  private static readonly Dictionary<string, int> HashCache = [];
  private static readonly Dictionary<int, string> ReverseHashCache = [];

  public static int Hash(string key)
  {
    if (HashCache.TryGetValue(key, out var hash)) return hash;
    hash = HashKey(key);
    HashCache[key] = hash;
    ReverseHashCache[hash] = key;
    OnHashed(key, hash);
    return hash;
  }

  private static int HashKey(string key)
  {
    if (Parse.TryInt(key, out var result)) return result;
    if (key.StartsWith("$", StringComparison.InvariantCultureIgnoreCase))
    {
      var hash = ZSyncAnimation.GetHash(key.Substring(1));
      // Animation keys are offset by 438569, except for $anim_speed.
      if (key == "$anim_speed") return hash;
      return 438569 + hash;
    }
    return key.GetStableHashCode();
  }

  public static string ReverseHash(int hash)
  {
    if (ReverseHashCache.TryGetValue(hash, out var key)) return key;
    string? known = null;
    FindKnownKey(hash, ref known);
    return known ?? hash.ToString(CultureInfo.InvariantCulture);
  }

  public static string GetString(ZDO zdo, string key, string defaultValue)
  {
    var hash = Hash(key);
    // Valheim update moved items from string to bytes, keeps the legacy string working for convenience.
    if (hash == ZDOVars.s_items) return GetBytes(zdo, key, defaultValue);
    return TryGetString(zdo, hash) ?? defaultValue;
  }
  public static string GetBytes(ZDO zdo, string key, string defaultValue)
  {
    var bytes = TryGetBytes(zdo, Hash(key));
    return bytes == null ? defaultValue : Convert.ToBase64String(bytes);
  }
  public static float GetFloat(ZDO zdo, string key, string defaultValue) => TryGetFloat(zdo, Hash(key)) ?? Parse.Float(defaultValue);
  public static int GetInt(ZDO zdo, string key, string defaultValue) => TryGetInt(zdo, Hash(key)) ?? Parse.Int(defaultValue);
  public static long GetLong(ZDO zdo, string key, string defaultValue) => TryGetLong(zdo, Hash(key)) ?? Parse.Long(defaultValue);
  public static bool GetBool(ZDO zdo, string key, string defaultValue) => TryGetBool(zdo, Hash(key)) ?? Parse.Boolean(defaultValue);
  public static Vector3 GetVec(ZDO zdo, string key, string defaultValue) => TryGetVec(zdo, Hash(key)) ?? Parse.VectorXZY(defaultValue);
  public static Quaternion GetQuaternion(ZDO zdo, string key, string defaultValue) => TryGetQuaternion(zdo, Hash(key)) ?? Parse.AngleYXZ(defaultValue);

  public static string? TryGetStringField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is string s ? s : null;
  public static float? TryGetFloatField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is float f ? f : null;
  public static int? TryGetIntField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is int i ? i : null;
  public static bool? TryGetBoolField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is bool b ? b : null;
  public static long? TryGetLongField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is long l ? l : null;
  public static Vector3? TryGetVecField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is Vector3 v ? v : null;
  public static Quaternion? TryGetQuatField(int prefabHash, int hash) => GetField(prefabHash, ReverseHash(hash)) is Quaternion q ? q : null;

  // Value is "Component.field" or "Component.field.subfield".
  private static object? GetField(int prefabHash, string value)
  {
    var kvp = Parse.Kvp(value, '.');
    if (kvp.Value == "") return null;
    var prefab = ZNetScene.instance.GetPrefab(prefabHash);
    if (prefab == null) return null;
    var component = FindComponent(prefab, kvp.Key);
    if (component == null) return null;
    var fields = kvp.Value.Split('.');
    object result = component;
    foreach (var field in fields)
    {
      var fieldInfo = result.GetType().GetField(field);
      if (fieldInfo == null) return null;
      result = fieldInfo.GetValue(result);
      if (result == null) return null;
    }
    if (result is GameObject go) return go.name;
    if (result is ItemDrop itemDrop) return itemDrop.gameObject.name;
    return result;
  }

  private static Component? FindComponent(GameObject obj, string name)
  {
    obj.GetComponentsInChildren(ZNetView.m_tempComponents);
    foreach (var monoBehaviour in ZNetView.m_tempComponents)
    {
      if (monoBehaviour.GetType().Name == name)
        return monoBehaviour;
    }
    foreach (Transform child in obj.transform)
    {
      var component = FindComponent(child.gameObject, name);
      if (component != null) return component;
    }
    return null;
  }

  public static Vector2i GetInventorySize(DataEntry entry, Functions f, ZDO? zdo)
  {
    var widthHash = Hash("Container.m_width");
    var heightHash = Hash("Container.m_height");
    // Width and height can come from data entry, existing ZDO fields or directly from the prefab.
    int width = 0;
    int height = 0;

    if (entry.Ints?.TryGetValue(widthHash, out var w) == true)
      width = w.Get(f) ?? 0;
    if (entry.Ints?.TryGetValue(heightHash, out var h) == true)
      height = h.Get(f) ?? 0;
    if (width > 0 && height > 0)
      return new Vector2i(width, height);

    if (zdo == null)
      return new Vector2i(width > 0 ? width : 4, height > 0 ? height : 2);

    if (width <= 0)
      width = zdo.GetInt(widthHash, 0);
    if (height <= 0)
      height = zdo.GetInt(heightHash, 0);
    if (width > 0 && height > 0)
      return new Vector2i(width, height);

    // Field might only change one dimension, which complicates the logic.
    var obj = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
    var container = obj.GetComponentInChildren<Container>();
    if (container)
    {
      if (width <= 0)
        width = container.m_width;
      if (height <= 0)
        height = container.m_height;
    }
    if (width <= 0) width = 4;
    if (height <= 0) height = 2;
    return new Vector2i(width, height);
  }
}
