using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Service;

// Replicates ZDO from Valheim for an abstract ZDO that isn't in the world.
public class FakeZDO {
  private readonly ZDOData Data;
  public readonly ZDO Source;
  public int Prefab => Source.m_prefab;
  public Vector3 Position => Source.m_position;
  public FakeZDO(ZDO zdo) {
    Data = new(zdo);
    Source = zdo.Clone();
  }
  public ZDO Create() {
    var zdo = ZDOMan.instance.CreateNewZDO(Position, Prefab);
    zdo.m_prefab = Source.m_prefab;
    zdo.m_position = Source.m_position;
    zdo.m_rotation = Source.m_rotation;
    zdo.Type = Source.Type;
    zdo.Distant = Source.Distant;
    Data.Copy(zdo);
    return zdo;
  }
  public void Destroy() {
    if (!Source.IsOwner())
      Source.SetOwner(ZDOMan.instance.m_sessionID);
    ZDOMan.instance.DestroyZDO(Source);
  }
}

// Replicates ZDO data from Valheim.
public class ZDOData {
  public ZDOData() { }
  public ZDOData(ZDO zdo) {
    var id = zdo.m_uid;
    Floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    Ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    Longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    Strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    Vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    Quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    ByteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
  }
  public ZDOData(ZPackage? pkg) {
    if (pkg == null) return;
    Load(pkg);
  }

  public void Copy(ZDO zdo) {
    var id = zdo.m_uid;
    if (Floats.Count > 0) {
      ZDOExtraData.s_floats[id] = new();
      foreach (var kvp in Floats) ZDOExtraData.s_floats[id][kvp.Key] = kvp.Value;
    }
    if (Ints.Count > 0) {
      ZDOExtraData.s_ints[id] = new();
      foreach (var kvp in Ints) ZDOExtraData.s_ints[id][kvp.Key] = kvp.Value;
    }
    if (Longs.Count > 0) {
      ZDOExtraData.s_longs[id] = new();
      foreach (var kvp in Longs) ZDOExtraData.s_longs[id][kvp.Key] = kvp.Value;
    }
    if (Strings.Count > 0) {
      ZDOExtraData.s_strings[id] = new();
      foreach (var kvp in Strings) ZDOExtraData.s_strings[id][kvp.Key] = kvp.Value;
    }
    if (Vecs.Count > 0) {
      ZDOExtraData.s_vec3[id] = new();
      foreach (var kvp in Vecs) ZDOExtraData.s_vec3[id][kvp.Key] = kvp.Value;
    }
    if (Quats.Count > 0) {
      ZDOExtraData.s_quats[id] = new();
      foreach (var kvp in Quats) ZDOExtraData.s_quats[id][kvp.Key] = kvp.Value;
    }
    if (ByteArrays.Count > 0) {
      ZDOExtraData.s_byteArrays[id] = new();
      foreach (var kvp in ByteArrays) ZDOExtraData.s_byteArrays[id][kvp.Key] = kvp.Value;
    }
  }

  public Dictionary<int, string> Strings = new();
  public string GetString(int hash, string defautlValue = "") => Strings.TryGetValue(hash, out var value) ? value : defautlValue;
  public string GetString(string key, string defautlValue = "") => GetString(key.GetStableHashCode(), defautlValue);
  public void Set(int hash, string value) => Strings[hash] = value;
  public void Set(string key, string value) => Set(key.GetStableHashCode(), value);
  public Dictionary<int, float> Floats = new();
  public float GetFloat(int hash, float defautlValue = 0) => Floats.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, float value) => Floats[hash] = value;
  public Dictionary<int, int> Ints = new();
  public int GetInt(int hash, int defautlValue = 0) => Ints.TryGetValue(hash, out var value) ? value : defautlValue;
  public int GetInt(string key, int defautlValue = 0) => GetInt(key.GetStableHashCode(), defautlValue);
  public void Set(int hash, int value) => Ints[hash] = value;
  public void Set(string key, int value) => Set(key.GetStableHashCode(), value);
  public void Set(int hash, bool value) => Ints[hash] = value ? 1 : 0;
  public Dictionary<int, long> Longs = new();
  public long GetLong(int hash, long defautlValue = 0) => Longs.TryGetValue(hash, out var value) ? value : defautlValue;
  public ZDOID GetZDOID(KeyValuePair<int, int> hashPair) {
    var value = GetLong(hashPair.Key, 0L);
    var num = (uint)GetLong(hashPair.Value, 0L);
    if (value == 0L || num == 0U) return ZDOID.None;
    return new ZDOID(value, num);
  }
  public void Set(int hash, long value) => Longs[hash] = value;
  public void Set(KeyValuePair<int, int> hashPair, ZDOID id) {
    Set(hashPair.Key, id.UserID);
    Set(hashPair.Value, (long)(ulong)id.ID);
  }
  public Dictionary<int, Vector3> Vecs = new();
  public Vector3 GetVector(int hash, Vector3 defautlValue) => Vecs.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, Vector3 value) => Vecs[hash] = value;
  public Dictionary<int, Quaternion> Quats = new();
  public Quaternion GetQuaternion(int hash, Quaternion defautlValue) => Quats.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, Quaternion value) => Quats[hash] = value;
  public Dictionary<int, byte[]> ByteArrays = new();
  public byte[] GetByteArray(int hash, byte[] defautlValue) => ByteArrays.TryGetValue(hash, out var value) ? value : defautlValue;
  public void Set(int hash, byte[] value) => ByteArrays[hash] = value;

  public ZPackage Save() {
    ZPackage pkg = new();
    Vecs.Remove(Hash.Scale);
    Vecs.Remove(Hash.SpawnPoint);
    if (Strings.ContainsKey(Hash.OverrideItems)) {
      Ints.Remove(Hash.AddedDefaultItems);
      Strings.Remove(Hash.Items);
    }
    var num = 0;
    if (Floats.Count > 0)
      num |= 1;
    if (Vecs.Count > 0)
      num |= 2;
    if (Quats.Count > 0)
      num |= 4;
    if (Ints.Count > 0)
      num |= 8;
    if (Strings.Count > 0)
      num |= 16;
    if (Longs.Count > 0)
      num |= 64;
    if (ByteArrays.Count > 0)
      num |= 128;

    pkg.Write(num);
    if (Floats.Count > 0) {
      pkg.Write((byte)Floats.Count);
      foreach (var kvp in Floats) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Vecs.Count > 0) {
      pkg.Write((byte)Vecs.Count);
      foreach (var kvp in Vecs) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Quats.Count > 0) {
      pkg.Write((byte)Quats.Count);
      foreach (var kvp in Quats) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Ints.Count > 0) {
      pkg.Write((byte)Ints.Count);
      foreach (var kvp in Ints) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    // Intended to come before strings (changing would break existing data).
    if (Longs.Count > 0) {
      pkg.Write((byte)Longs.Count);
      foreach (var kvp in Longs) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Strings.Count > 0) {
      pkg.Write((byte)Strings.Count);
      foreach (var kvp in Strings) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (ByteArrays.Count > 0) {
      pkg.Write((byte)ByteArrays.Count);
      foreach (var kvp in ByteArrays) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    return pkg;
  }
  public void Load(ZPackage pkg) {
    var num = pkg.ReadInt();
    if ((num & 1) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Floats[pkg.ReadInt()] = pkg.ReadSingle();
    }
    if ((num & 2) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Vecs[pkg.ReadInt()] = pkg.ReadVector3();
    }
    if ((num & 4) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Quats[pkg.ReadInt()] = pkg.ReadQuaternion();
    }
    if ((num & 8) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Ints[pkg.ReadInt()] = pkg.ReadInt();
    }
    // Intended to come before strings (changing would break existing data).
    if ((num & 64) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Longs[pkg.ReadInt()] = pkg.ReadLong();
    }
    if ((num & 16) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Strings[pkg.ReadInt()] = pkg.ReadString();
    }
    if ((num & 128) != 0) {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ByteArrays[pkg.ReadInt()] = pkg.ReadByteArray();
    }
  }

}