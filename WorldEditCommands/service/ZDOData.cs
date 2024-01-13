using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Service;

// Replicates ZDO from Valheim for an abstract ZDO that isn't in the world.
public class FakeZDO(ZDO zdo)
{
  private readonly ZDOData Data = new(zdo);
  public readonly ZDO Source = zdo.Clone();
  public int Prefab => Source.m_prefab;
  public Vector3 Position => Source.m_position;

  public ZDO Create()
  {
    var zdo = ZDOMan.instance.CreateNewZDO(Position, Prefab);
    Copy(zdo);
    zdo.DataRevision = 0;
    // This is needed to trigger the ZDO sync.
    zdo.IncreaseDataRevision();
    return zdo;
  }
  public void Copy(ZDO zdo)
  {
    zdo.m_prefab = Source.m_prefab;
    zdo.m_position = Source.m_position;
    zdo.m_rotation = Source.m_rotation;
    zdo.Type = Source.Type;
    zdo.Distant = Source.Distant;
    zdo.Persistent = Source.Persistent;
    Data.Copy(zdo);
  }
  public void Destroy()
  {
    if (!Source.IsOwner())
      Source.SetOwner(ZDOMan.instance.m_sessionID);
    ZDOMan.instance.DestroyZDO(Source);
  }
}

// Replicates ZDO data from Valheim.
public class ZDOData
{
  public ZDOData(ZDO zdo)
  {
    var id = zdo.m_uid;
    Floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    ByteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var conn = ZDOExtraData.s_connectionsHashData.TryGetValue(id, out var c) ? c : null;
    ConnectionType = conn?.m_type ?? ZDOExtraData.ConnectionType.None;
    ConnectionHash = conn?.m_hash ?? 0;
  }

  public void Copy(ZDO zdo)
  {
    var id = zdo.m_uid;

    if (Floats.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_floats, id);
      foreach (var kvp in Floats) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (Ints.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_ints, id);
      foreach (var kvp in Ints) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (Longs.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_longs, id);
      foreach (var kvp in Longs) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (Strings.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_strings, id);
      foreach (var kvp in Strings) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (Vecs.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_vec3, id);
      foreach (var kvp in Vecs) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (Quats.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_quats, id);
      foreach (var kvp in Quats) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (ByteArrays.Count > 0)
    {
      ZDOHelper.Release(ZDOExtraData.s_byteArrays, id);
      foreach (var kvp in ByteArrays) ZDOExtraData.Set(id, kvp.Key, kvp.Value);
    }
    if (ConnectionType != ZDOExtraData.ConnectionType.None && ConnectionHash != 0)
    {
      ZDOExtraData.SetConnectionData(id, ConnectionType, ConnectionHash);
    }
    zdo.IncreaseDataRevision();
  }

  public Dictionary<int, string> Strings = [];
  public string GetString(int hash, string defautlValue = "") => Strings.TryGetValue(hash, out var value) ? value : defautlValue;
  public string GetString(string key, string defautlValue = "") => GetString(key.GetStableHashCode(), defautlValue);
  public void Set(int hash, string value) => Strings[hash] = value;
  public void Set(string key, string value) => Set(key.GetStableHashCode(), value);
  public Dictionary<int, float> Floats = [];
  public float GetFloat(int hash, float defautlValue = 0) => Floats.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, float value) => Floats[hash] = value;
  public Dictionary<int, int> Ints = [];
  public int GetInt(int hash, int defautlValue = 0) => Ints.TryGetValue(hash, out var value) ? value : defautlValue;
  public int GetInt(string key, int defautlValue = 0) => GetInt(key.GetStableHashCode(), defautlValue);
  public void Set(int hash, int value) => Ints[hash] = value;
  public void Set(string key, int value) => Set(key.GetStableHashCode(), value);
  public void Set(int hash, bool value) => Ints[hash] = value ? 1 : 0;
  public Dictionary<int, long> Longs = [];
  public long GetLong(int hash, long defautlValue = 0) => Longs.TryGetValue(hash, out var value) ? value : defautlValue;
  public ZDOID GetZDOID(KeyValuePair<int, int> hashPair)
  {
    var value = GetLong(hashPair.Key, 0L);
    var num = (uint)GetLong(hashPair.Value, 0L);
    if (value == 0L || num == 0U) return ZDOID.None;
    return new ZDOID(value, num);
  }
  public void Set(int hash, long value) => Longs[hash] = value;
  public void Set(KeyValuePair<int, int> hashPair, ZDOID id)
  {
    Set(hashPair.Key, id.UserID);
    Set(hashPair.Value, (long)(ulong)id.ID);
  }
  public Dictionary<int, Vector3> Vecs = [];
  public Vector3 GetVector(int hash, Vector3 defautlValue) => Vecs.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, Vector3 value) => Vecs[hash] = value;
  public Dictionary<int, Quaternion> Quats = [];
  public Quaternion GetQuaternion(int hash, Quaternion defautlValue) => Quats.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, Quaternion value) => Quats[hash] = value;
  public Dictionary<int, byte[]> ByteArrays = [];
  public byte[] GetByteArray(int hash, byte[] defautlValue) => ByteArrays.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, byte[] value) => ByteArrays[hash] = value;
  public ZDOExtraData.ConnectionType ConnectionType = ZDOExtraData.ConnectionType.None;
  public int ConnectionHash = 0;
}