
using System.Collections.Generic;
using UnityEngine;

namespace Data;

public class PrefabValue(string[] values) : AnyValue(values), IPrefabValue
{
  // Caching makes sense because parameters and wildcards makes it slow.
  // Also prefab is often checked many times.
  private List<int>? Cache;
  private Functions? LastFunctions;
  public int? Get(Functions f)
  {
    if (f != LastFunctions)
    {
      var values = GetAllValues(f);
      Cache = PrefabHelper.GetPrefabs(values, null);
      LastFunctions = f;
    }
    if (Cache == null || Cache.Count == 0) return null;
    if (Cache.Count == 1) return Cache[0];
    return Cache[Random.Range(0, Cache.Count)];
  }

  public bool? Match(Functions f, int value)
  {
    if (f != LastFunctions)
    {
      var values = GetAllValues(f);
      Cache = PrefabHelper.GetPrefabs(values, null);
      LastFunctions = f;
    }
    if (Cache == null || Cache.Count == 0) return null;
    return Cache.Contains(value);
  }
}
public class SimplePrefabsValue(List<int> value) : IPrefabValue
{
  private readonly List<int> Values = value;

  public int? Get(Functions f) => RollValue();
  public bool? Match(Functions f, int value) => Values.Contains(value);
  private int RollValue() => Values[Random.Range(0, Values.Count)];
}
public class SimplePrefabValue(int? value) : IPrefabValue
{
  private readonly int? Value = value;

  public int? Get(Functions f) => Value;
  public bool? Match(Functions f, int value) => Value == value;
}
public interface IPrefabValue
{
  int? Get(Functions f);
  bool? Match(Functions f, int value);
}