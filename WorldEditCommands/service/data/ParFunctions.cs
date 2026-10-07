using System.Collections.Generic;
using UnityEngine;

namespace Data;

// WEC only: resolves <key> from command parameters and value groups first, then from the source ZDO.
public class ParFunctions(Dictionary<string, string> pars, ZDO? zdo) : Functions(GetPrefabName(zdo), [], zdo == null ? Vector3.zero : zdo.m_position)
{
  private readonly ObjectFunctions? objectFunctions = zdo == null ? null : new(GetPrefabName(zdo), [], zdo);

  private static string GetPrefabName(ZDO? zdo) => zdo == null ? "" : ZNetScene.instance.GetPrefab(zdo.m_prefab)?.name ?? "";

  protected override string? GetFunction(string key, string defaultValue)
  {
    if (pars.TryGetValue($"<{key}>", out var value)) return value;
    if (ValueGroups.TryGetRandom(key, out value)) return value;
    return objectFunctions != null ? objectFunctions.Resolve(key, defaultValue) : base.GetFunction(key, defaultValue);
  }
}
