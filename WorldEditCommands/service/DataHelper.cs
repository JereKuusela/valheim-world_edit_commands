using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace Data;

public class DataHelper
{
  public static ZDO Regen(ZDO existing, ZDO zdo)
  {
    ZNetScene.instance.CreateObject(zdo);
    Destroy(existing);
    return zdo;
  }
  public static ZDO Regen(ZDO existing, PlainDataEntry data)
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
  public static bool HasKey(ZDO zdo, string[] keys)
  {
    var hashed = keys.Select(s => s.GetStableHashCode()).ToHashSet();
    var id = zdo.m_uid;
    var floats = ZDOExtraData.s_floats.TryGetValue(id, out var floatVals) ? floatVals.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    var vecs = ZDOExtraData.s_vec3.TryGetValue(id, out var vec3s) ? vec3s.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    var quats = ZDOExtraData.s_quats.TryGetValue(id, out var quatVals) ? quatVals.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    var ints = ZDOExtraData.s_ints.TryGetValue(id, out var intVals) ? intVals.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    var longs = ZDOExtraData.s_longs.TryGetValue(id, out var longVals) ? longVals.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    var strings = ZDOExtraData.s_strings.TryGetValue(id, out var stringVals) ? stringVals.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    var byteArrays = ZDOExtraData.s_byteArrays.TryGetValue(id, out var byteArrayVals) ? byteArrayVals.Select(kvp => kvp.Key) : Enumerable.Empty<int>();
    return floats.Concat(vecs).Concat(quats).Concat(ints).Concat(longs).Concat(strings).Concat(byteArrays).Any(hashed.Contains);
  }
  public static ZDO CloneWithoutKeys(ZDO zdo, string[] keys)
  {
    var hashed = keys.Select(s => s.GetStableHashCode()).ToHashSet();
    var clone = CloneBase(zdo);
    var id = zdo.m_uid;
    var cid = clone.m_uid;
    var floats = ZDOExtraData.s_floats.TryGetValue(id, out var floatVals) ? floatVals.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var vecs = ZDOExtraData.s_vec3.TryGetValue(id, out var vec3s) ? vec3s.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var quats = ZDOExtraData.s_quats.TryGetValue(id, out var quatVals) ? quatVals.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var ints = ZDOExtraData.s_ints.TryGetValue(id, out var intVals) ? intVals.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var longs = ZDOExtraData.s_longs.TryGetValue(id, out var longVals) ? longVals.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var strings = ZDOExtraData.s_strings.TryGetValue(id, out var stringVals) ? stringVals.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var byteArrays = ZDOExtraData.s_byteArrays.TryGetValue(id, out var byteArrayVals) ? byteArrayVals.Where(kvp => !hashed.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];

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
    var vecs = ZDOExtraData.s_vec3.TryGetValue(id, out var vec3s) ? vec3s.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {Helper.PrintVectorXZY(kvp.Value)} (vec x,z,y)") : Enumerable.Empty<string>();
    var ints = ZDOExtraData.s_ints.TryGetValue(id, out var intVals) ? intVals.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (int)") : Enumerable.Empty<string>();
    var floats = ZDOExtraData.s_floats.TryGetValue(id, out var floatVals) ? floatVals.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (float)") : Enumerable.Empty<string>();
    var quats = ZDOExtraData.s_quats.TryGetValue(id, out var quatVals) ? quatVals.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {Helper.PrintAngleYXZ(kvp.Value)} (quat y,x,z)") : Enumerable.Empty<string>();
    var strings = ZDOExtraData.s_strings.TryGetValue(id, out var stringVals) ? stringVals.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (string)") : Enumerable.Empty<string>();
    var longs = ZDOExtraData.s_longs.TryGetValue(id, out var longVals) ? longVals.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {kvp.Value} (long)") : Enumerable.Empty<string>();
    var byteArrays = ZDOExtraData.s_byteArrays.TryGetValue(id, out var byteArrayVals) ? byteArrayVals.Select(kvp => $"{ZDOKeys.Convert(kvp.Key)}: {Convert.ToBase64String(kvp.Value)} (byte array)") : Enumerable.Empty<string>();
    return [.. lines, .. vecs, .. ints, .. floats, .. quats, .. strings, .. longs, .. byteArrays];
  }
  public static ZDO? Init(int prefab, Transform tr, DataEntry? data)
  {
    CleanUp();
    var obj = ZNetScene.instance.GetPrefab(prefab);
    if (!obj) return null;
    return Init(obj, tr.position, tr.rotation, tr.lossyScale, data, []);
  }
  public static ZDO? Init(GameObject obj, Vector3 pos, Quaternion rot, Vector3? scale, DataEntry? data, Dictionary<string, string> pars)
  {
    CleanUp();
    if (data == null && scale == null) return null;
    if (!obj.TryGetComponent<ZNetView>(out var view)) return null;
    var prefab = Utils.GetPrefabName(obj).GetStableHashCode();
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos, prefab);
    data?.Write(pars, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot.eulerAngles;
    ZNetView.m_initZDO.Type = data?.Priority ?? view.m_type;
    ZNetView.m_initZDO.Distant = data?.Distant?.GetBool(pars) ?? view.m_distant;
    ZNetView.m_initZDO.Persistent = data?.Persistent?.GetBool(pars) ?? view.m_persistent;
    ZNetView.m_initZDO.m_prefab = prefab;
    if (!view.m_syncInitialScale && scale != null && WorldEditCommands.WorldEditCommands.IsTweaks)
    {
      ZNetView.m_initZDO.Set(Hash.HasFields, true);
      ZNetView.m_initZDO.Set("HasFieldsZNetView", true);
      ZNetView.m_initZDO.Set("ZNetView.m_syncInitialScale", true);
      view.m_syncInitialScale = true;
      Console.instance.AddString("Note: Scaling set to true.");
    }
    if (view.m_syncInitialScale && scale != null)
      ZNetView.m_initZDO.Set(ZDOVars.s_scaleHash, scale.Value);
    ZNetView.m_initZDO.DataRevision = 0;
    // This is needed to trigger the ZDO sync.
    ZNetView.m_initZDO.IncreaseDataRevision();
    return ZNetView.m_initZDO;
  }
  public static void CleanUp()
  {
    ZNetView.m_initZDO = null;
  }


  public static DataEntry? Merge(params DataEntry?[] datas)
  {
    var nonNull = datas.Where(d => d != null).ToArray();
    if (nonNull.Length == 0) return null;
    if (nonNull.Length == 1) return nonNull[0];
    DataEntry result = new();
    foreach (var data in nonNull)
      result.Load(data!);
    return result;
  }

  public static DataEntry Get(string name)
  {
    var hash = name.GetStableHashCode();
    if (!DataLoading.Data.ContainsKey(hash))
    {
      try
      {
        DataLoading.Data[hash] = new DataEntry(name);
      }
      catch (Exception e)
      {
        if (name.Contains("=") || name.Length > 32)
          throw new InvalidOperationException($"Can't load data value: {name}", e);
        else
          throw new InvalidOperationException($"Can't find data entry: {name}", e);
      }
    }
    return DataLoading.Data[hash];
  }
  public static string Base64(Dictionary<string, string> pars, string data)
  {
    if (!DataLoading.Data.TryGetValue(data.GetStableHashCode(), out var zdo))
      return data;
    return zdo.GetBase64(pars);
  }

}