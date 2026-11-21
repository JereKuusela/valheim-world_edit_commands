// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Text.RegularExpressions;
using Common;
using UnityEngine;

namespace Data;

/// <summary>
/// Converts the info column of old PlanBuild blueprints to data.
/// The meaning of the text depends on the prefab, for example sign text or item stand contents.
/// </summary>
public static class BlueprintInfo
{
  public static DataEntry? ToData(string prefabName, string info)
  {
    if (info == "" || !ZNetScene.instance) return null;
    var prefab = ZNetScene.instance.GetPrefab(prefabName);
    return prefab ? ToData(prefab, info) : null;
  }

  public static DataEntry? ToData(GameObject prefab, string info)
  {
    if (info == "") return null;
    // New PlanBuild format can have an escaped string.
    if (info.Length > 1 && info.StartsWith("\"") && info.EndsWith("\""))
      info = Regex.Unescape(info.Substring(1, info.Length - 2));

    DataEntry data = new();
    var any = false;
    if (prefab.GetComponent<Sign>())
      any |= Set(data, ZDOVars.s_text, info);
    if (prefab.GetComponent<TeleportWorld>())
      any |= Set(data, ZDOVars.s_tag, info);
    if (prefab.GetComponent<Tameable>())
      any |= Set(data, ZDOVars.s_tamedName, info);
    // Fish don't work with piece.
    if (prefab.GetComponent<ItemDrop>() && !prefab.GetComponent<Fish>())
      any |= Set(data, ZDOVars.s_piece, 1);
    if (prefab.GetComponent<ItemStand>())
    {
      var split = info.Split(':');
      Set(data, ZDOVars.s_item, split[0].GetStableHashCode());
      Set(data, ZDOVars.s_variant, Parse.Int(split, 1, 0));
      Set(data, ZDOVars.s_quality, Parse.Int(split, 2, 1));
      if (split.Length > 3)
        Set(data, ZDOVars.s_type, Parse.Int(split, 3, 0));
      any = true;
    }
    if (prefab.TryGetComponent<ArmorStand>(out var armorStand))
    {
      var split = info.Split(':');
      Set(data, ZDOVars.s_pose, Parse.Int(split, 0, 0));
      for (var i = 0; i < armorStand.m_slots.Count; i++)
      {
        var name = Parse.String(split, i * 2 + 2);
        if (name == "") continue;
        Set(data, $"{i}_item".GetStableHashCode(), name.GetStableHashCode());
        Set(data, $"{i}_variant".GetStableHashCode(), Parse.Int(split, i * 2 + 3, 0));
      }
      any = true;
    }
    return any ? data : null;
  }

  private static bool Set(DataEntry data, int key, string value)
  {
    data.Strings ??= [];
    data.Strings[key] = DataValue.Constant(value);
    return true;
  }

  private static bool Set(DataEntry data, int key, int value)
  {
    data.Ints ??= [];
    data.Ints[key] = DataValue.Constant(value);
    return true;
  }
}
