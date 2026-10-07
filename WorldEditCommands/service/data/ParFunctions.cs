using System;
using System.Collections.Generic;
using System.Globalization;
using ServerDevcommands;
using Service;
using UnityEngine;
using Parse = Service.Parse;

namespace Data;

// WEC only: resolves <key> from command parameters, value groups and the source ZDO.
public class ParFunctions(Dictionary<string, string> pars, ZDO? zdo) : Functions(zdo == null ? "" : ZNetScene.instance.GetPrefab(zdo.m_prefab)?.name ?? "", [], zdo == null ? Vector3.zero : zdo.m_position)
{
  protected override string? GetFunction(string key, string defaultValue)
  {
    if (pars.TryGetValue($"<{key}>", out var value)) return value;
    if (DataLoading.TryGetValueFromGroup(key, out value)) return value;
    if (zdo != null)
    {
      value = GetZdoParameter(zdo, key);
      if (value != null) return value;
    }
    return base.GetFunction(key, defaultValue);
  }

  private static string? GetZdoParameter(ZDO zdo, string key)
  {
    if (key == "rot") return Helper.PrintAngleYXZ(zdo.GetRotation());
    var kvp = Parse.Kvp(key, '_');
    if (kvp.Value == "") return null;
    var type = kvp.Key;
    var zdoKey = kvp.Value;
    return type switch
    {
      "string" => zdo.GetString(zdoKey),
      "float" => zdo.GetFloat(zdoKey).ToString(CultureInfo.InvariantCulture),
      "int" => zdo.GetInt(zdoKey).ToString(CultureInfo.InvariantCulture),
      "hash" => zdo.GetInt(zdoKey).ToString(CultureInfo.InvariantCulture),
      "long" => zdo.GetLong(zdoKey).ToString(CultureInfo.InvariantCulture),
      "bool" => zdo.GetBool(zdoKey).ToString(),
      "vec" => Helper.PrintVectorXZY(zdo.GetVec3(zdoKey, Vector3.zero)),
      "quat" => Helper.PrintAngleYXZ(zdo.GetQuaternion(zdoKey, Quaternion.identity)),
      "byte" => Convert.ToBase64String(zdo.GetByteArray(zdoKey)),
      _ => null,
    };
  }
}
