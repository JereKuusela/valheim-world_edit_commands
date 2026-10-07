// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using Common;
using Service;

namespace Data;

/// <summary>Named lists of values. Besides the user defined groups, every prefab component and some item/material types are available as groups.</summary>
public static class ValueGroups
{
  private static readonly Dictionary<int, List<string>> Groups = [];
  private static readonly Dictionary<int, List<string>> DefaultGroups = [];
  private static readonly Dictionary<string, List<string>> ComponentsByPrefab = [];

  public static int Count => Groups.Count;
  public static int Hash(string group) => group.ToLowerInvariant().GetStableHashCode();
  public static bool Contains(int hash) => Groups.ContainsKey(hash);
  public static bool TryGet(int hash, out List<string> values) => Groups.TryGetValue(hash, out values);
  public static bool TryGet(string group, out List<string> values) => Groups.TryGetValue(Hash(group), out values);
  public static bool IsComponentGroup(string group) => DefaultGroups.ContainsKey(Hash(group));
  public static List<string> GetComponents(string prefabName) => ComponentsByPrefab.TryGetValue(prefabName, out var components) ? components : [];

  public static bool TryGetRandom(string group, out string value)
  {
    if (!TryGet(group, out var values) || values.Count == 0)
    {
      value = group;
      return false;
    }
    value = values[UnityEngine.Random.Range(0, values.Count)];
    return true;
  }

  public static void Clear() => Groups.Clear();

  public static void Add(DataYaml data, string? file = null)
  {
    if (data.value != null)
    {
      var kvp = Parse.Kvp(data.value);
      Add(kvp.Key, [kvp.Value], file);
    }
    if (data.valueGroup != null && data.values != null)
      Add(data.valueGroup, data.values, file);
  }

  private static void Add(string group, IEnumerable<string> values, string? file)
  {
    var hash = Hash(group);
    if (Groups.TryGetValue(hash, out var existing))
      Log.Warning(file == null ? $"Duplicate value group entry: {group}" : $"Duplicate value group entry: {group} at {file}");
    else
      Groups[hash] = existing = [];
    existing.AddRange(values);
  }

  /// <summary>Must be called after all groups are added and before entries using them are loaded.</summary>
  public static void Resolve()
  {
    LoadDefaults();
    foreach (var values in Groups.Values)
      ResolveNested(values);
    foreach (var pair in DefaultGroups)
    {
      if (!Groups.ContainsKey(pair.Key))
        Groups[pair.Key] = pair.Value;
    }
  }

  private static void ResolveNested(List<string> values)
  {
    for (var i = 0; i < values.Count; ++i)
    {
      var value = values[i];
      if (!value.StartsWith("<", StringComparison.Ordinal) || !value.EndsWith(">", StringComparison.Ordinal))
        continue;
      var hash = Hash(value.Substring(1, value.Length - 2));
      if (Groups.TryGetValue(hash, out var group))
      {
        values.RemoveAt(i);
        values.InsertRange(i, group);
        // Recheck inserted values because value groups can be nested.
        i -= 1;
      }
      else if (DefaultGroups.TryGetValue(hash, out var defaultGroup))
      {
        values.RemoveAt(i);
        values.InsertRange(i, defaultGroup);
        // No need to recheck because default value groups are not nested.
        i += defaultGroup.Count - 1;
      }
    }
  }

  private static void LoadDefaults()
  {
    if (DefaultGroups.Count > 0) return;
    if (!ZNetScene.instance) return;
    ComponentsByPrefab.Clear();
    foreach (var prefab in ZNetScene.instance.m_namedPrefabs.Values)
    {
      if (!prefab) continue;
      var prefabName = prefab.name;
      if (!ComponentsByPrefab.TryGetValue(prefabName, out var names))
        ComponentsByPrefab[prefabName] = names = [];
      HashSet<int> added = [];
      prefab.GetComponentsInChildren(ZNetView.m_tempComponents);
      foreach (var component in ZNetView.m_tempComponents)
      {
        // Some mods leave destroyed or missing components behind.
        if (!component) continue;
        var name = component.GetType().Name;
        AddDefault(added, name, prefabName);
        var lower = name.ToLowerInvariant();
        AddName(names, lower);
        if (lower == "humanoid")
          AddName(names, "character");
        if (lower == "wearntear")
          AddName(names, "structure");
        if (component is WearNTear wearNTear)
          AddDefault(added, $"material_{wearNTear.m_materialType}", prefabName);
        if (component is ItemDrop item)
          AddDefault(added, $"itemtype_{item.m_itemData.m_shared.m_itemType}", prefabName);
      }
    }
    // Some key codes are hardcoded for legacy reasons.
    Alias("creature", "humanoid");
    Alias("structure", "wearntear");
  }

  private static void AddName(List<string> names, string name)
  {
    if (!names.Contains(name)) names.Add(name);
  }

  // Prefabs can have multiple components of the same type, but should be listed only once.
  private static void AddDefault(HashSet<int> added, string group, string prefabName)
  {
    var hash = Hash(group.Replace(" ", "_"));
    if (!added.Add(hash)) return;
    if (!DefaultGroups.TryGetValue(hash, out var values))
      DefaultGroups[hash] = values = [];
    values.Add(prefabName);
  }

  private static void Alias(string alias, string group)
  {
    if (DefaultGroups.TryGetValue(Hash(group), out var values))
      DefaultGroups[Hash(alias)] = values;
  }
}
