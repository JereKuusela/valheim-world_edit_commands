using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Data;

// Split from DataEntry to keep it simpler. When loading data from actual objects, only simple values are needed.
// This can be used for saving ZDO data to file and for undo/redo.
public class PlainDataEntry
{
  public PlainDataEntry(ZDO zdo)
  {
    Load(zdo);
  }

  // Nulls add more code but should be more performant.
  public Dictionary<int, string>? Strings;
  public Dictionary<int, float>? Floats;
  public Dictionary<int, int>? Ints;
  public Dictionary<int, long>? Longs;
  public Dictionary<int, Vector3>? Vecs;
  public Dictionary<int, Quaternion>? Quats;
  public Dictionary<int, byte[]>? ByteArrays;
  public ZDOExtraData.ConnectionType ConnectionType = ZDOExtraData.ConnectionType.None;
  public int ConnectionHash = 0;
  public ZDOID OriginalId = ZDOID.None;
  public ZDOID TargetConnectionId = ZDOID.None;


  public void Load(ZDO zdo)
  {
    var id = zdo.m_uid;
    Floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    Ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    Longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    Strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    Vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    Quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    ByteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    if (ZDOExtraData.s_connectionsHashData.TryGetValue(id, out var conn))
    {
      ConnectionType = conn.m_type;
      ConnectionHash = conn.m_hash;
    }
    OriginalId = id;
    if (ZDOExtraData.s_connections.TryGetValue(id, out var zdoConn) && zdoConn.m_target != ZDOID.None)
    {
      TargetConnectionId = zdoConn.m_target;
      ConnectionType = zdoConn.m_type;
    }
  }

  private static readonly HashSet<int> HashKeys = [
    ZDOVars.s_helmetItem,
    ZDOVars.s_chestItem,
    ZDOVars.s_legItem,
    ZDOVars.s_shoulderItem,
    ZDOVars.s_utilityItem,
    ZDOVars.s_leftItem,
    ZDOVars.s_rightItem
  ];
  public void Write(DataData data)
  {
    // No need to roll here because tbis always come from ZDO that doesn't have item values.
    data.floats = Floats?.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {pair.Value}").ToArray();
    data.ints = Ints?.Where(kvp => !HashKeys.Contains(kvp.Key)).Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {pair.Value}").ToArray();
    if (data.ints?.Length == 0) data.ints = null;
    data.hashes = Ints?.Where(kvp => HashKeys.Contains(kvp.Key)).Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {ZNetScene.instance.GetPrefab(pair.Value)?.name ?? pair.Value.ToString()}").ToArray();
    if (data.hashes?.Length == 0) data.hashes = null;

    data.longs = Longs?.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {pair.Value}").ToArray();
    data.strings = Strings?.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {Serialize(pair.Value)}").ToArray();
    data.vecs = Vecs?.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {Serialize(pair.Value)}").ToArray();
    data.quats = Quats?.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {Serialize(pair.Value)}").ToArray();
    data.bytes = ByteArrays?.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {Convert.ToBase64String(pair.Value)}").ToArray();
    var items = Strings?.FirstOrDefault(kvp => kvp.Key == ZDOVars.s_items).Value;
    if (items != null)
      data.items = GetItems(items);
    if (ConnectionType != ZDOExtraData.ConnectionType.None && ConnectionHash != 0)
      data.connection = $"{ConnectionType}, {ConnectionHash}";
  }
  private static ItemData[] GetItems(string encoded)
  {
    ZPackage pkg = new(encoded);
    var version = pkg.ReadInt();
    if (version != 106) return [];
    var amount = pkg.ReadInt();
    var list = new ItemData[amount];
    for (var i = 0; i < amount; ++i)
    {
      var text = pkg.ReadString();
      var stack = pkg.ReadInt();
      var durability = pkg.ReadSingle();
      var pos = pkg.ReadVector2i();
      var equipped = pkg.ReadBool();
      var quality = pkg.ReadInt();
      var variant = pkg.ReadInt();
      var crafterID = pkg.ReadLong();
      var crafterName = pkg.ReadString();
      var dictionary = new Dictionary<string, string>();
      var num3 = pkg.ReadInt();
      for (var j = 0; j < num3; ++j)
      {
        dictionary[pkg.ReadString()] = pkg.ReadString();
      }
      var worldLevel = pkg.ReadInt();
      var pickedUp = pkg.ReadBool();
      var extra = num3 > 0 ? $", {string.Join(", ", dictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}" : "";
      list[i] = new()
      {
        pos = $"{pos.x}, {pos.y}",
        prefab = text,
        stack = stack.ToString(),
        quality = quality.ToString(),
        variant = variant.ToString(),
        customData = dictionary.Count > 0 ? dictionary : null,
        equipped = equipped ? "true" : "false",
        durability = Serialize(durability),
        crafterID = crafterID.ToString(),
        crafterName = crafterName,
        pickedUp = pickedUp ? "true" : "false",
        worldLevel = worldLevel.ToString()
      };
    }
    return list;
  }

  private static string Serialize(string? str) => str == null ? "<none>" : str.Contains(",") ? $"\"{str}\"" : str;
  private static string Serialize(Quaternion quat)
  {
    var euler = quat.eulerAngles;
    if (euler.x == 0f && euler.z == 0f)
      return Serialize(euler.y);
    else
      return $"{Serialize(euler.y)},{Serialize(euler.x)},{Serialize(euler.z)}";
  }
  private static string Serialize(float value)
  {
    return value.ToString("0.#####", NumberFormatInfo.InvariantInfo);
  }
  private static string Serialize(Vector3 vec)
  {
    return $"{Serialize(vec.x)},{Serialize(vec.z)},{Serialize(vec.y)}";
  }
  public void Write(ZDO zdo)
  {
    var id = zdo.m_uid;
    if (Floats?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_floats, id);
      foreach (var pair in Floats)
        ZDOExtraData.s_floats[id].SetValue(pair.Key, pair.Value);
    }
    if (Vecs?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_vec3, id);
      foreach (var pair in Vecs)
        ZDOExtraData.s_vec3[id].SetValue(pair.Key, pair.Value);
    }
    if (Quats?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_quats, id);
      foreach (var pair in Quats)
        ZDOExtraData.s_quats[id].SetValue(pair.Key, pair.Value);
    }
    if (Ints?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_ints, id);
      foreach (var pair in Ints)
        ZDOExtraData.s_ints[id].SetValue(pair.Key, pair.Value);
    }
    if (Longs?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_longs, id);
      foreach (var pair in Longs)
        ZDOExtraData.s_longs[id].SetValue(pair.Key, pair.Value);
    }
    if (Strings?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_strings, id);
      foreach (var pair in Strings)
        ZDOExtraData.s_strings[id].SetValue(pair.Key, pair.Value);
    }
    if (ByteArrays?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_byteArrays, id);
      foreach (var pair in ByteArrays)
        ZDOExtraData.s_byteArrays[id].SetValue(pair.Key, pair.Value);
    }
    HandleConnection(zdo);
    HandleHashConnection(zdo);
  }
  private void HandleConnection(ZDO ownZdo)
  {
    if (OriginalId == ZDOID.None) return;
    var ownId = ownZdo.m_uid;
    if (TargetConnectionId != ZDOID.None)
    {
      // If target is known, the setup is easy.
      var otherZdo = ZDOMan.instance.GetZDO(TargetConnectionId);
      if (otherZdo == null) return;

      ownZdo.SetConnection(ConnectionType, TargetConnectionId);
      // Portal is two way.
      if (ConnectionType == ZDOExtraData.ConnectionType.Portal)
        otherZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ownId);

    }
    else
    {
      // Otherwise all zdos must be scanned.
      var other = ZDOExtraData.s_connections.FirstOrDefault(kvp => kvp.Value.m_target == OriginalId);
      if (other.Value == null) return;
      var otherZdo = ZDOMan.instance.GetZDO(other.Key);
      if (otherZdo == null) return;
      // Connection is always one way here, otherwise TargetConnectionId would be set.
      otherZdo.SetConnection(other.Value.m_type, ownId);
    }
  }
  private void HandleHashConnection(ZDO ownZdo)
  {
    if (ConnectionHash == 0) return;
    if (ConnectionType == ZDOExtraData.ConnectionType.None) return;
    var ownId = ownZdo.m_uid;

    // Hash data is regenerated on world save.
    // But in this case, it's manually set, so might be needed later.
    ZDOExtraData.SetConnectionData(ownId, ConnectionType, ConnectionHash);

    // While actual connection can be one way, hash is always two way.
    // One of the hashes always has the target type.
    var otherType = ConnectionType ^ ZDOExtraData.ConnectionType.Target;
    var isOtherTarget = (ConnectionType & ZDOExtraData.ConnectionType.Target) == 0;
    var zdos = ZDOExtraData.GetAllConnectionZDOIDs(otherType);
    var otherId = zdos.FirstOrDefault(z => ZDOExtraData.GetConnectionHashData(z, ConnectionType)?.m_hash == ConnectionHash);
    if (otherId == ZDOID.None) return;
    var otherZdo = ZDOMan.instance.GetZDO(otherId);
    if (otherZdo == null) return;
    if ((ConnectionType & ZDOExtraData.ConnectionType.Spawned) > 0)
    {
      // Spawn is one way.
      var connZDO = isOtherTarget ? ownZdo : otherZdo;
      var targetId = isOtherTarget ? otherId : ownId;
      connZDO.SetConnection(ZDOExtraData.ConnectionType.Spawned, targetId);
    }
    if ((ConnectionType & ZDOExtraData.ConnectionType.SyncTransform) > 0)
    {
      // Sync is one way.
      var connZDO = isOtherTarget ? ownZdo : otherZdo;
      var targetId = isOtherTarget ? otherId : ownId;
      connZDO.SetConnection(ZDOExtraData.ConnectionType.SyncTransform, targetId);
    }
    if ((ConnectionType & ZDOExtraData.ConnectionType.Portal) > 0)
    {
      // Portal is two way.
      otherZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ownId);
      ownZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, otherId);
    }
  }
}
