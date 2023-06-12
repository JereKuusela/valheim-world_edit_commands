using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace Service;

public class DataHelper {
  public static void Init(GameObject obj, Vector3 pos, Quaternion rot, Vector3 scale, ZPackage data) {
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    var prefab = Utils.GetPrefabName(obj).GetStableHashCode();
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos, prefab);
    Load(data, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot.eulerAngles;
    ZNetView.m_initZDO.Type = view.m_type;
    ZNetView.m_initZDO.Distant = view.m_distant;
    ZNetView.m_initZDO.Persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = prefab;
    if (view.m_syncInitialScale)
      ZNetView.m_initZDO.Set(ZDOVars.s_scaleHash, scale);
    ZNetView.m_initZDO.DataRevision = 1;
  }
  public static void CleanUp() {
    ZNetView.m_initZDO = null;
  }
  public static ZPackage? Deserialize(string data) => data == "" ? null : new(data);

  public static void Serialize(ZDO zdo, ZPackage pkg, string filter) {
    var id = zdo.m_uid;
    var vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    var ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    var floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    var quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    var strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    var longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    var byteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new();
    if (filter == "") {
      vecs.Remove(Hash.Scale);
      vecs.Remove(Hash.SpawnPoint);
      ints.Remove(Hash.Seed);
      ints.Remove(Hash.Location);
      if (strings.ContainsKey(Hash.OverrideItems)) {
        ints.Remove(Hash.AddedDefaultItems);
        strings.Remove(Hash.Items);
      }
    } else if (filter != "all") {
      var filters = Parse.Split(filter).Select(s => s.GetStableHashCode()).ToHashSet();
      vecs = FilterZdo(vecs, filters);
      quats = FilterZdo(quats, filters);
      floats = FilterZdo(floats, filters);
      ints = FilterZdo(ints, filters);
      longs = FilterZdo(longs, filters);
      byteArrays = FilterZdo(byteArrays, filters);
    }
    var num = 0;
    if (floats.Count() > 0)
      num |= 1;
    if (vecs.Count() > 0)
      num |= 2;
    if (quats.Count() > 0)
      num |= 4;
    if (ints.Count() > 0)
      num |= 8;
    if (strings.Count() > 0)
      num |= 16;
    if (longs.Count() > 0)
      num |= 64;
    if (byteArrays.Count() > 0)
      num |= 128;

    pkg.Write(num);
    if (floats.Count() > 0) {
      pkg.Write((byte)floats.Count());
      foreach (var kvp in floats) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (vecs.Count() > 0) {
      pkg.Write((byte)vecs.Count());
      foreach (var kvp in vecs) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (quats.Count() > 0) {
      pkg.Write((byte)quats.Count());
      foreach (var kvp in quats) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (ints.Count() > 0) {
      pkg.Write((byte)ints.Count());
      foreach (var kvp in ints) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (longs.Count() > 0) {
      pkg.Write((byte)longs.Count());
      foreach (var kvp in longs) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (strings.Count() > 0) {
      pkg.Write((byte)strings.Count());
      foreach (var kvp in strings) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (byteArrays.Count() > 0) {
      pkg.Write((byte)byteArrays.Count());
      foreach (var kvp in byteArrays) {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
  }
  private static void Load(ZPackage pkg, ZDO zdo) {
    var id = zdo.m_uid;
    var num = pkg.ReadInt();
    if ((num & 1) != 0) {
      var count = pkg.ReadByte();
      var floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id] : new();
      for (var i = 0; i < count; ++i)
        floats[pkg.ReadInt()] = pkg.ReadSingle();
    }
    if ((num & 2) != 0) {
      var count = pkg.ReadByte();
      var vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id] : new();
      for (var i = 0; i < count; ++i)
        vecs[pkg.ReadInt()] = pkg.ReadVector3();
    }
    if ((num & 4) != 0) {
      var count = pkg.ReadByte();
      var quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id] : new();
      for (var i = 0; i < count; ++i)
        quats[pkg.ReadInt()] = pkg.ReadQuaternion();
    }
    if ((num & 8) != 0) {
      var count = pkg.ReadByte();
      var ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id] : new();
      for (var i = 0; i < count; ++i)
        ints[pkg.ReadInt()] = pkg.ReadInt();
    }
    if ((num & 16) != 0) {
      var count = pkg.ReadByte();
      var strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id] : new();
      for (var i = 0; i < count; ++i)
        strings[pkg.ReadInt()] = pkg.ReadString();
    }
    if ((num & 64) != 0) {
      var count = pkg.ReadByte();
      var longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id] : new();
      for (var i = 0; i < count; ++i)
        longs[pkg.ReadInt()] = pkg.ReadLong();
    }
    if ((num & 128) != 0) {
      var count = pkg.ReadByte();
      var byteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id] : new();
      for (var i = 0; i < count; ++i)
        byteArrays[pkg.ReadInt()] = pkg.ReadByteArray();
    }
  }
  private static Dictionary<int, T> FilterZdo<T>(Dictionary<int, T> dict, HashSet<int> filters) {
    return dict.Where(kvp => filters.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, pair => pair.Value);
  }
}