using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Service;
using UnityEngine;

namespace Data;

// WEC only: snapshot of a ZDO that can be saved to file, copied as base64 and used for undo/redo.
public class PlainDataEntry : ResolvedDataEntry
{
  public PlainDataEntry() { }
  public PlainDataEntry(ZDO zdo)
  {
    Load(zdo);
  }

  private bool HasConnection => ConnectionType.HasValue && ConnectionType != ZDOExtraData.ConnectionType.None && ConnectionHash != 0;

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
    Distant = zdo.Distant;
    Persistent = zdo.Persistent;
    Priority = zdo.Type;
  }
  private static readonly HashSet<int> HashKeys = [
    ZDOVars.s_helmetItem,
    ZDOVars.s_chestItem,
    ZDOVars.s_legItem,
    ZDOVars.s_shoulderItem,
    ZDOVars.s_utilityItem,
    ZDOVars.s_leftItem,
    ZDOVars.s_rightItem,
    ZDOVars.s_content,
    ZDOVars.s_item,
    .. Enumerable.Range(0, 11).Select(i => ZDOKeys.Hash($"{i}_item"))
  ];
  public void Write(DataYaml data, bool all)
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
    // Unreadable or invalid items stay in their raw byte field.
    var bytes = ByteArrays == null ? null : new Dictionary<int, byte[]>(ByteArrays);
    if (bytes != null && bytes.TryGetValue(ZDOVars.s_items, out var packedItems))
    {
      var records = ItemDataHelper.Load(new ZPackage(packedItems));
      if (records.Count > 0 && ItemDataHelper.CountInvalid(records) == 0)
      {
        data.items = [.. records.Select(r => ToData(r, true, all))];
        bytes.Remove(ZDOVars.s_items);
      }
    }
    if (bytes != null && bytes.TryGetValue(ZDOVars.s_itemData, out var packedItem) && ItemDataHelper.LoadItem(packedItem) is { } record)
    {
      data.item = ToData(record, all, all);
      bytes.Remove(ZDOVars.s_itemData);
    }
    data.bytes = bytes?.Count > 0 ? bytes.Select(pair => $"{ZDOKeys.Convert(pair.Key)}, {Convert.ToBase64String(pair.Value)}").ToArray() : null;

    if (HasConnection)
      data.connection = $"{ConnectionType}, {ConnectionHash}";
    var persistent = Persistent ?? true;
    var distant = Distant ?? false;
    var priority = Priority ?? ZDO.ObjectType.Default;
    if (all)
    {
      data.persistent = persistent ? "true" : "false";
      data.distant = distant ? "true" : "false";
      data.priority = priority.ToString();
    }
    else
    {
      // Usually people don't need distant, persistent or priority.
      // So only write when they are different or unusual.
      var defaultDistant = false;
      var defaultPersistent = true;
      var defaultType = ZDO.ObjectType.Default;
      if (OriginalId.HasValue && OriginalId != ZDOID.None)
      {
        var zdo = ZDOMan.instance.GetZDO(OriginalId.Value);
        var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
        var view = prefab?.GetComponent<ZNetView>();
        if (view != null)
        {
          defaultDistant = view.m_distant;
          defaultPersistent = view.m_persistent;
          defaultType = view.m_type;
        }
      }
      if (persistent != defaultPersistent)
        data.persistent = persistent ? "true" : "false";
      if (distant != defaultDistant)
        data.distant = distant ? "true" : "false";
      if (priority != defaultType)
        data.priority = priority.ToString();
    }
  }
  private static ItemYaml ToData(ItemRecord record, bool includePos, bool all) => new()
  {
    pos = includePos ? $"{record.GridPos.x}, {record.GridPos.y}" : "",
    prefab = record.PrefabName,
    stack = record.Stack.ToString(),
    // Missing quality loads as 1.
    quality = all || record.Quality != 1 ? record.Quality.ToString() : null,
    variant = all || record.Variant != 0 ? record.Variant.ToString() : null,
    customData = record.CustomData.Count > 0 ? record.CustomData : null,
    equipped = all ? (record.Equipped ? "true" : "false") : record.Equipped ? "true" : null,
    durability = Serialize(record.Durability),
    crafterID = all || record.CrafterID != 0 ? record.CrafterID.ToString() : null,
    crafterName = all || record.CrafterName != "" ? record.CrafterName : null,
    pickedUp = all ? (record.PickedUp ? "true" : "false") : record.PickedUp ? "true" : null,
    cheated = all ? (record.Cheated ? "true" : "false") : record.Cheated ? "true" : null,
    worldLevel = all || record.WorldLevel != 0 ? record.WorldLevel.ToString() : null
  };

  private static string Serialize(string? str) => str == null || str == "" ? "\"\"" : str.Contains(",") ? $"\"{str}\"" : str;
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
  public string GetBase64()
  {
    var pkg = new ZPackage();
    Write(pkg);
    return pkg.GetBase64();
  }
  public void Write(ZPackage pkg)
  {
    var num = 0;
    if (Floats?.Count > 0)
      num |= 1;
    if (Vecs?.Count > 0)
      num |= 2;
    if (Quats?.Count > 0)
      num |= 4;
    if (Ints?.Count > 0)
      num |= 8;
    if (Strings?.Count > 0)
      num |= 16;
    if (Longs?.Count > 0)
      num |= 64;
    if (ByteArrays?.Count > 0)
      num |= 128;
    if (HasConnection)
      num |= 256;
    if (Persistent == false)
      num |= 512;
    if (Distant == true)
      num |= 1024;
    if (Priority.HasValue && Priority != ZDO.ObjectType.Default)
      num |= 2048;

    pkg.Write(num);
    if (Floats?.Count > 0)
    {
      pkg.Write((byte)Floats.Count);
      foreach (var kvp in Floats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Vecs?.Count > 0)
    {
      pkg.Write((byte)Vecs.Count);
      foreach (var kvp in Vecs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Quats?.Count > 0)
    {
      pkg.Write((byte)Quats.Count);
      foreach (var kvp in Quats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Ints?.Count > 0)
    {
      pkg.Write((byte)Ints.Count);
      foreach (var kvp in Ints)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Longs?.Count > 0)
    {
      pkg.Write((byte)Longs.Count);
      foreach (var kvp in Longs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Strings?.Count > 0)
    {
      pkg.Write((byte)Strings.Count);
      foreach (var kvp in Strings)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (ByteArrays?.Count > 0)
    {
      pkg.Write((byte)ByteArrays.Count);
      foreach (var kvp in ByteArrays)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (HasConnection)
    {
      pkg.Write((byte)ConnectionType!.Value);
      pkg.Write(ConnectionHash);
    }
    if ((num & 512) != 0)
      pkg.Write(Persistent!.Value);
    if ((num & 1024) != 0)
      pkg.Write(Distant!.Value);
    if ((num & 2048) != 0)
      pkg.Write((byte)Priority!.Value);
  }
}
