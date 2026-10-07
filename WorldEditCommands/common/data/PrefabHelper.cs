// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using System.Linq;
using Common;
using Service;

namespace Data;

/// <summary>
/// Resolves prefab names with wildcards, multiple values and value groups.
/// Results are cached for the same input, so the cache must be cleared when value groups change.
/// </summary>
public static partial class PrefabHelper
{
  private static readonly Dictionary<string, List<int>> ResultCache = [];
  private static Dictionary<string, int> PrefabCache = [];

  // Hosts can add prefabs that are not in the scene.
  static partial void AddCustomPrefabs(Dictionary<string, int> prefabs);
  static partial void OnClearCache();

  public static void ClearCache()
  {
    ResultCache.Clear();
    PrefabCache.Clear();
    OnClearCache();
  }

  public static List<int> GetPrefabs(string include, string exclude)
  {
    var key = $"{include}|{exclude}";
    if (ResultCache.TryGetValue(key, out var cached)) return cached;
    var includes = Parse.ToList(include);
    var excludes = exclude == "" ? null : Parse.ToList(exclude);
    var prefabs = GetPrefabs(includes, excludes);
    // No point to cache error results from users.
    if (prefabs == null) return [];
    ResultCache[key] = prefabs;
    return prefabs;
  }

  // Called by PrefabValue that handles the caching.
  public static List<int>? GetPrefabs(List<string> includes, List<string>? excludes)
  {
    if (includes.Count == 0) return null;
    HashSet<int>? excludedPrefabs = null;
    if (excludes != null && excludes.Count > 0)
      excludedPrefabs = [.. excludes.Select(ParsePrefabs).Where(s => s != null).SelectMany(s => s!)];

    if (includes.Count == 1)
    {
      var prefabs = ParsePrefabs(includes[0]);
      if (prefabs == null) return null;
      if (excludedPrefabs != null)
        prefabs = [.. prefabs.Where(i => !excludedPrefabs.Contains(i))];
      return prefabs;
    }
    HashSet<int> value = [];
    foreach (var prefabs in includes.Select(ParsePrefabs))
    {
      if (prefabs == null) continue;
      foreach (var prefab in prefabs)
      {
        if (excludedPrefabs != null && excludedPrefabs.Contains(prefab))
          continue;
        value.Add(prefab);
      }
    }
    return value.Count == 0 ? null : [.. value];
  }

  private static void EnsurePrefabCache()
  {
    if (PrefabCache.Count != 0) return;
    PrefabCache = ZNetScene.instance.m_namedPrefabs.ToDictionary(pair => pair.Value.name, pair => pair.Key);
    AddCustomPrefabs(PrefabCache);
  }

  private static List<int>? ParsePrefabs(string prefab)
  {
    EnsurePrefabCache();
    if (Wildcard.IsPattern(prefab))
      return [.. PrefabCache.Where(pair => Wildcard.Match(pair.Key, prefab, true)).Select(pair => pair.Value)];
    if (PrefabCache.TryGetValue(prefab, out var hash))
      return [hash];
    if (ValueGroups.TryGet(prefab, out var group))
      return [.. group.Select(s => s.GetStableHashCode())];
    Log.Warning($"Failed to resolve prefab: {prefab}");
    return null;
  }
}
