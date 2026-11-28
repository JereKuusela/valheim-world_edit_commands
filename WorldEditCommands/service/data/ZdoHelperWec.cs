using UnityEngine;

namespace Data;

public static partial class ZdoHelper
{
  public static byte[]? TryGetBytes(ZDO zdo, int hash) => zdo.GetByteArray(hash);
  public static string? TryGetString(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetString(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_strings.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : TryGetStringField(zdo.m_prefab, hash);
  }
  public static float? TryGetFloat(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetFloat(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_floats.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : TryGetFloatField(zdo.m_prefab, hash);
  }
  public static int? TryGetInt(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetInt(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : TryGetIntField(zdo.m_prefab, hash);
  }
  public static long? TryGetLong(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetLong(zdo, hash, out var packed)) return packed;
    return ZDOExtraData.s_longs.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : TryGetLongField(zdo.m_prefab, hash);
  }
  public static bool? TryGetBool(ZDO zdo, int hash)
  {
    if (ItemDataHelper.TryGetInt(zdo, hash, out var packed)) return packed > 0;
    return ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value > 0 : TryGetBoolField(zdo.m_prefab, hash);
  }
  public static Vector3? TryGetVec(ZDO zdo, int hash) =>
    ZDOExtraData.s_vec3.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : TryGetVecField(zdo.m_prefab, hash);
  public static Quaternion? TryGetQuaternion(ZDO zdo, int hash) =>
    ZDOExtraData.s_quats.TryGetValue(zdo.m_uid, out var data) && data.TryGetValue(hash, out var value) ? value : TryGetQuatField(zdo.m_prefab, hash);
}
