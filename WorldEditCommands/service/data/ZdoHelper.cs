using System.Collections.Generic;
using Service;
using UnityEngine;

namespace Data;

// WEC counterpart of the EWP helper, only the parts needed by the shared data code.
public static class ZdoHelper
{
  public static int Hash(string key) => Parse.TryInt(key, out var result) ? result : ZDOKeys.Hash(key);
  public static string ReverseHash(int hash) => ZDOKeys.Convert(hash);

  public static string? TryGetString(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetString(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_strings.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : null;
  }
  public static float? TryGetFloat(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetFloat(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_floats.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : null;
  }
  public static int? TryGetInt(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetInt(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : null;
  }
  public static long? TryGetLong(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetLong(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_longs.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : null;
  }
  public static bool? TryGetBool(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetInt(zdo, hash, out var packed)) return packed > 0;
    return ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value > 0 : null;
  }
  public static Vector3? TryGetVec(ZDO zdo, int hash) =>
    ZDOExtraData.s_vec3.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : null;
  public static Quaternion? TryGetQuaternion(ZDO zdo, int hash) =>
    ZDOExtraData.s_quats.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : null;

  private static readonly int InventoryWidthHash = Hash("Container.m_width");
  private static readonly int InventoryHeightHash = Hash("Container.m_height");
  public static Vector2i GetInventorySize(DataEntry entry, Functions f, ZDO? zdo)
  {
    // Width and height can come from data entry, existing ZDO fields or directly from the prefab.
    int width = 0;
    int height = 0;

    if (entry.Ints?.TryGetValue(InventoryWidthHash, out var w) == true)
      width = w.Get(f) ?? 0;
    if (entry.Ints?.TryGetValue(InventoryHeightHash, out var h) == true)
      height = h.Get(f) ?? 0;
    if (width > 0 && height > 0)
      return new Vector2i(width, height);

    if (zdo == null)
      return new Vector2i(width > 0 ? width : 4, height > 0 ? height : 2);

    if (width <= 0)
      width = zdo.GetInt(InventoryWidthHash, 0);
    if (height <= 0)
      height = zdo.GetInt(InventoryHeightHash, 0);
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
