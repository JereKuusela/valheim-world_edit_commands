using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using UnityEngine;
namespace Service;

public class DataHelper
{
  public static ZDO Regen(ZDO existing, ZDO zdo)
  {
    ZNetScene.instance.CreateObject(zdo);
    Destroy(existing);
    return zdo;
  }
  public static ZDO Regen(ZDO existing, ZDOData data)
  {
    var newZdo = CloneBase(existing);
    data.Write(newZdo);
    ZNetScene.instance.CreateObject(newZdo);
    Destroy(existing);
    return newZdo;
  }
  public static void Destroy(ZDO zdo)
  {
    zdo.SetOwner(ZDOMan.instance.m_sessionID);
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view) && view)
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);

  }
  public static ZDO CloneBase(ZDO zdo)
  {
    var clone = ZDOMan.instance.CreateNewZDO(zdo.m_position, zdo.m_prefab);
    clone.m_prefab = zdo.m_prefab;
    clone.m_rotation = zdo.m_rotation;
    clone.Type = zdo.Type;
    clone.Distant = zdo.Distant;
    clone.Persistent = zdo.Persistent;
    // Needed to trigger changes.
    clone.IncreaseDataRevision();
    return clone;
  }
  public static ZDO CloneWithKeys(ZDO zdo, string[] keys)
  {
    var hashed = keys.Select(s => s.GetStableHashCode()).ToHashSet();
    var clone = CloneBase(zdo);
    var id = zdo.m_uid;
    var cid = clone.m_uid;
    foreach (var key in hashed)
    {
      if (ZDOExtraData.GetFloat(id, key, out var value))
        ZDOExtraData.Set(cid, key, value);
      if (ZDOExtraData.GetVec3(id, key, out var vec))
        ZDOExtraData.Set(cid, key, vec);
      if (ZDOExtraData.GetQuaternion(id, key, out var quat))
        ZDOExtraData.Set(cid, key, quat);
      if (ZDOExtraData.GetInt(id, key, out var i))
        ZDOExtraData.Set(cid, key, i);
      if (ZDOExtraData.GetLong(id, key, out var l))
        ZDOExtraData.Set(cid, key, l);
      if (ZDOExtraData.GetString(id, key, out var s))
        ZDOExtraData.Set(cid, key, s);
      if (ZDOExtraData.GetByteArray(id, key, out var b))
        ZDOExtraData.Set(cid, key, b);
    }
    return clone;
  }
  public static ZDO CloneWithoutKeys(ZDO zdo, string[] keys)
  {
    var hashed = keys.Select(s => s.GetStableHashCode()).ToHashSet();
    var clone = CloneBase(zdo);
    var id = zdo.m_uid;
    var cid = clone.m_uid;
    var floats = ZDOExtraData.GetFloats(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var vecs = ZDOExtraData.GetVec3s(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var quats = ZDOExtraData.GetQuaternions(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var ints = ZDOExtraData.GetInts(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var longs = ZDOExtraData.GetLongs(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var strings = ZDOExtraData.GetStrings(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var byteArrays = ZDOExtraData.GetByteArrays(id).Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    foreach (var kvp in floats)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    foreach (var kvp in vecs)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    foreach (var kvp in quats)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    foreach (var kvp in ints)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    foreach (var kvp in longs)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    foreach (var kvp in strings)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    foreach (var kvp in byteArrays)
      ZDOExtraData.Set(cid, kvp.Key, kvp.Value);
    return clone;
  }
  public static List<string> Print(ZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
    var id = zdo.m_uid;
    List<string> lines = [
      $"Id: {id}",
      $"Prefab: {(prefab ? prefab.name : "Unknown")}",
      $"Owner: {zdo.GetOwner()}",
      $"Position: {Helper.PrintVectorXZY(zdo.m_position)} (vec x,z,y)",
      $"Rotation: {Helper.PrintVectorYXZ(zdo.m_rotation)} (quat y,x,z)",
      $"Revision: {zdo.DataRevision} + {zdo.OwnerRevision}"
    ];
    var vecs = ZDOExtraData.GetVec3s(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {Helper.PrintVectorXZY(kvp.Value)} (vec x,z,y)");
    var ints = ZDOExtraData.GetInts(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (int)");
    var floats = ZDOExtraData.GetFloats(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (float)");
    var quats = ZDOExtraData.GetQuaternions(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {Helper.PrintAngleYXZ(kvp.Value)} (quat y,x,z)");
    var strings = ZDOExtraData.GetStrings(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (string)");
    var longs = ZDOExtraData.GetLongs(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (long)");
    var byteArrays = ZDOExtraData.GetByteArrays(id).Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {Convert.ToBase64String(kvp.Value)} (byte array)");
    return lines.Concat(vecs).Concat(ints).Concat(floats).Concat(quats).Concat(strings).Concat(longs).Concat(byteArrays).ToList();
  }
  public static void Init(GameObject obj, Vector3 pos, Quaternion rot, Vector3 scale, ZDOData? data)
  {
    if (data == null && scale == Vector3.one) return;
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    var prefab = Utils.GetPrefabName(obj).GetStableHashCode();
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos, prefab);
    data?.Write(ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot.eulerAngles;
    ZNetView.m_initZDO.Type = view.m_type;
    ZNetView.m_initZDO.Distant = view.m_distant;
    ZNetView.m_initZDO.Persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = prefab;
    if (!view.m_syncInitialScale && WorldEditCommands.WorldEditCommands.IsTweaks)
    {
      ZNetView.m_initZDO.Set(Hash.HasFields, true);
      ZNetView.m_initZDO.Set("HasFieldsZNetView", true);
      ZNetView.m_initZDO.Set("ZNetView.m_syncInitialScale", true);
      view.m_syncInitialScale = true;
      Console.instance.AddString("Note: Scaling set to true.");
    }
    if (view.m_syncInitialScale)
      ZNetView.m_initZDO.Set(ZDOVars.s_scaleHash, scale);
    ZNetView.m_initZDO.DataRevision = 0;
    // This is needed to trigger the ZDO sync.
    ZNetView.m_initZDO.IncreaseDataRevision();
  }
  public static void CleanUp()
  {
    ZNetView.m_initZDO = null;
  }
}