// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data;

public partial class DataEntry
{
  /// <summary>True if the ZDO matches all set values.</summary>
  public bool Match(Functions f, ZDO zdo) => Matches(f, zdo, false);

  /// <summary>True if the ZDO matches none of the set values.</summary>
  public bool MatchNone(Functions f, ZDO zdo) => Matches(f, zdo, true);

  // Match fails when a value gives the failOn result. Values that can't be evaluated never fail.
  private bool Matches(Functions f, ZDO zdo, bool failOn)
  {
    if (Fails(Strings, f, zdo, failOn, GetString, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Floats, f, zdo, failOn, GetFloat, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Ints, f, zdo, failOn, GetInt, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Longs, f, zdo, failOn, GetLong, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Bools, f, zdo, failOn, GetBool, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Hashes, f, zdo, failOn, GetInt, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Vecs, f, zdo, failOn, GetVec, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(Quats, f, zdo, failOn, GetQuaternion, static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Fails(ByteArrays, f, zdo, failOn, static (z, key) => z.GetByteArray(key), static (v, ctx, data) => v.Match(ctx, data))) return false;
    if (Persistent != null && Persistent.Match(f, zdo.Persistent) == failOn) return false;
    if (Distant != null && Distant.Match(f, zdo.Distant) == failOn) return false;
    if (Priority != null && (Priority.Value == zdo.Type) == failOn) return false;
    if (Item != null && Item.MatchSingle(f, zdo) == failOn) return false;
    if (Items != null) return ItemValue.Match(f, Items, zdo, ItemAmount) != failOn;
    if (ItemAmount != null) return ItemValue.Match(f, zdo, ItemAmount) != failOn;
    if (ConnectionType.HasValue)
    {
      if (ConnectionType.Value == ZDOExtraData.ConnectionType.None)
      {
        var conn = zdo.GetConnection();
        var matches = conn == null || conn.m_target == ZDOID.None;
        if (matches == failOn) return false;
      }
      else
      {
        var conn = zdo.GetConnectionZDOID(ConnectionType.Value);
        if (TargetConnectionId == null)
        {
          if ((conn != ZDOID.None) == failOn) return false;
        }
        else
        {
          var target = TargetConnectionId.Get(f);
          if (target != null && (conn == target) == failOn) return false;
        }
      }
    }
    return true;
  }

  private static bool Fails<TValue, TData>(Dictionary<int, TValue>? values, Functions f, ZDO zdo, bool failOn, Func<ZDO, int, TData> read, Func<TValue, Functions, TData, bool?> match)
  {
    if (values == null) return false;
    foreach (var pair in values)
    {
      if (match(pair.Value, f, read(zdo, pair.Key)) == failOn) return true;
    }
    return false;
  }

  private static string GetString(ZDO zdo, int key) => ZdoHelper.TryGetString(zdo, key) ?? "";
  private static float GetFloat(ZDO zdo, int key) => ZdoHelper.TryGetFloat(zdo, key) ?? 0f;
  private static int GetInt(ZDO zdo, int key) => ZdoHelper.TryGetInt(zdo, key) ?? 0;
  private static long GetLong(ZDO zdo, int key) => ZdoHelper.TryGetLong(zdo, key) ?? 0L;
  private static bool GetBool(ZDO zdo, int key) => ZdoHelper.TryGetBool(zdo, key) ?? false;
  private static Vector3 GetVec(ZDO zdo, int key) => ZdoHelper.TryGetVec(zdo, key) ?? Vector3.zero;
  private static Quaternion GetQuaternion(ZDO zdo, int key) => ZdoHelper.TryGetQuaternion(zdo, key) ?? Quaternion.identity;
}
