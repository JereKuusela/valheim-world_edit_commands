// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Service;
using UnityEngine;

namespace Data;

public partial class DataEntry
{
  public void Load(ZDO zdo)
  {
    var id = zdo.m_uid;
    Floats = ZDOExtraData.s_floats.TryGetValue(id, out var floatsData) ? floatsData.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    Ints = ZDOExtraData.s_ints.TryGetValue(id, out var intsData) ? intsData.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    Longs = ZDOExtraData.s_longs.TryGetValue(id, out var longsData) ? longsData.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    Strings = ZDOExtraData.s_strings.TryGetValue(id, out var stringsData) ? stringsData.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    Vecs = ZDOExtraData.s_vec3.TryGetValue(id, out var vec3Data) ? vec3Data.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    Quats = ZDOExtraData.s_quats.TryGetValue(id, out var quatsData) ? quatsData.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    ByteArrays = ZDOExtraData.s_byteArrays.TryGetValue(id, out var byteArraysData) ? byteArraysData.ToDictionary(kvp => kvp.Key, kvp => DataValue.Constant(kvp.Value)) : null;
    if (ZDOExtraData.s_connectionsHashData.TryGetValue(id, out var conn))
    {
      ConnectionType = conn.m_type;
      ConnectionHash = conn.m_hash;
    }
    OriginalId = new ConstantZdoIdValue(id);
    if (ZDOExtraData.s_connections.TryGetValue(id, out var zdoConn) && zdoConn.m_target != ZDOID.None)
    {
      TargetConnectionId = new ConstantZdoIdValue(zdoConn.m_target);
      ConnectionType = zdoConn.m_type;
    }
    // Usually these don't want to be copied automatically.
    Persistent = null;
    Distant = null;
    Priority = null;
    CanBeInjected = CheckCanBeInjected();
  }
  public void Load(DataYaml data)
  {
    HashSet<string> componentsToAdd = [];
    if (data.floats != null)
    {
      Floats ??= [];
      foreach (var value in data.floats)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse float {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Floats.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate float key {kvp.Key}.");
        Floats[hash] = DataValue.Float(kvp.Value);
      }
    }
    if (data.ints != null)
    {
      Ints ??= [];
      foreach (var value in data.ints)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse int {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Ints.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate int key {kvp.Key}.");
        Ints[hash] = DataValue.Int(kvp.Value);
      }
    }
    if (data.bools != null)
    {
      Bools ??= [];
      foreach (var value in data.bools)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse bool {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Bools.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate bool key {kvp.Key}.");
        Bools[hash] = DataValue.Bool(kvp.Value);
      }
    }
    if (data.hashes != null)
    {
      Hashes ??= [];
      foreach (var value in data.hashes)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse hash {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Hashes.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate hash key {kvp.Key}.");
        Hashes[hash] = DataValue.Hash(kvp.Value);
      }
    }
    if (data.longs != null)
    {
      Longs ??= [];
      foreach (var value in data.longs)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse long {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Longs.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate long key {kvp.Key}.");
        Longs[hash] = DataValue.Long(kvp.Value);
      }
    }
    if (data.strings != null)
    {
      Strings ??= [];
      foreach (var value in data.strings)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse string {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        // Legacy inventories must replace the current byte-array inventory instead.
        if (hash == ZDOVars.s_items)
        {
          ByteArrays ??= [];
          if (ByteArrays.ContainsKey(hash))
            Log.Warning($"Data {data.name}: Duplicate string key {kvp.Key}.");
          ByteArrays[hash] = DataValue.Bytes(kvp.Value);
          continue;
        }
        if (Strings.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate string key {kvp.Key}.");
        Strings[hash] = DataValue.String(kvp.Value);
      }
    }
    if (data.vecs != null)
    {
      Vecs ??= [];
      foreach (var value in data.vecs)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse vector {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Vecs.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate vector key {kvp.Key}.");
        Vecs[hash] = DataValue.Vector3(kvp.Value);
      }
    }
    if (data.quats != null)
    {
      Quats ??= [];
      foreach (var value in data.quats)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse quaternion {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (Quats.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate quaternion key {kvp.Key}.");
        Quats[hash] = DataValue.Quaternion(kvp.Value);
      }
    }
    if (data.bytes != null)
    {
      ByteArrays ??= [];
      foreach (var value in data.bytes)
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Failed to parse byte array {value}.");
        if (kvp.Key.Contains("."))
          componentsToAdd.Add(kvp.Key.Split('.')[0]);
        var hash = ZdoHelper.Hash(kvp.Key);
        if (ByteArrays.ContainsKey(hash))
          Log.Warning($"Data {data.name}: Duplicate byte array key {kvp.Key}.");
        ByteArrays[hash] = DataValue.Bytes(kvp.Value);
      }
    }
    if (data.items != null)
    {
      Items = [.. data.items.Select(item => new ItemValue(item))];
    }
    if (data.item != null)
      Item = new ItemValue(data.item);
    if (!string.IsNullOrWhiteSpace(data.containerSize))
      ContainerSize = Parse.Vector2Int(data.containerSize!);
    if (!string.IsNullOrWhiteSpace(data.itemAmount))
      ItemAmount = DataValue.Int(data.itemAmount!);
    CanBeInjected = componentsToAdd.Count == 0;
    if (componentsToAdd.Count > 0)
    {
      Components ??= [];
      Components[ZdoHelper.Hash("HasFields")] = DataValue.Constant(1);
      foreach (var component in componentsToAdd)
        Components[ZdoHelper.Hash($"HasFields{component}")] = DataValue.Constant(1);
    }
    if (!string.IsNullOrWhiteSpace(data.position))
      Position = DataValue.Vector3(data.position!);
    if (!string.IsNullOrWhiteSpace(data.rotation))
      Rotation = DataValue.Quaternion(data.rotation!);
    if (data.persistent != null)
      Persistent = DataValue.Bool(data.persistent);
    if (data.distant != null)
      Distant = DataValue.Bool(data.distant);
    if (data.priority != null)
      Priority = Enum.TryParse<ZDO.ObjectType>(data.priority, true, out var parsed) ? parsed : null;
    if (!string.IsNullOrWhiteSpace(data.connection))
    {
      var split = Parse.SplitWithEmpty(data.connection!);
      if (split.Length == 1)
      {
        ConnectionType = ToByteEnum<ZDOExtraData.ConnectionType>([.. split]);
      }
      else
      {
        var types = split.Take(split.Length - 1).ToList();
        var hash = split[split.Length - 1];
        ConnectionType = ToByteEnum<ZDOExtraData.ConnectionType>(types);
        // Hacky way, this should be entirely rethought but not much use for the connection system so far.
        if (hash.Contains(":") || hash.Contains("<"))
        {
          TargetConnectionId = DataValue.ZdoId(hash);
          // Must be set to run the connection code.
          OriginalId = TargetConnectionId;
        }
        else
        {
          ConnectionHash = Parse.Int(hash);
          if (ConnectionHash == 0) ConnectionHash = hash.GetStableHashCode();
        }
      }
    }
  }
  public static HashSet<string> SupportedTypes =
  [
    "float",
    "int",
    "bool",
    "hash",
    "long",
    "string",
    "vec",
    "vec3",
    "quat",
    "bytes",
  ];
  public void Load(string[] typeKeyValues)
  {
    if (typeKeyValues.Length != 3)
      throw new InvalidOperationException($"Failed to parse type, field, value.");
    var type = typeKeyValues[0].ToLowerInvariant();
    var key = typeKeyValues[1];
    var value = typeKeyValues[2];
    if (key.Contains("."))
    {
      CanBeInjected = false;
      var component = key.Split('.')[0];
      Ints ??= [];
      Ints[ZdoHelper.Hash("HasFields")] = DataValue.Constant(1);
      Ints[ZdoHelper.Hash($"HasFields{component}")] = DataValue.Constant(1);
    }
    switch (type)
    {
      case "float":
        Floats ??= [];
        Floats[ZdoHelper.Hash(key)] = DataValue.Float(value);
        break;
      case "int":
        Ints ??= [];
        Ints[ZdoHelper.Hash(key)] = DataValue.Int(value);
        break;
      case "bool":
        Bools ??= [];
        Bools[ZdoHelper.Hash(key)] = DataValue.Bool(value);
        break;
      case "hash":
        Hashes ??= [];
        Hashes[ZdoHelper.Hash(key)] = DataValue.Hash(value);
        break;
      case "long":
        Longs ??= [];
        Longs[ZdoHelper.Hash(key)] = DataValue.Long(value);
        break;
      case "string":
        Strings ??= [];
        Strings[ZdoHelper.Hash(key)] = DataValue.String(value);
        break;
      case "vec":
      case "vec3":
        Vecs ??= [];
        Vecs[ZdoHelper.Hash(key)] = DataValue.Vector3(value);
        break;
      case "quat":
        Quats ??= [];
        Quats[ZdoHelper.Hash(key)] = DataValue.Quaternion(value);
        break;
      case "bytes":
        ByteArrays ??= [];
        ByteArrays[ZdoHelper.Hash(key)] = DataValue.Bytes(value);
        break;
      default:
        throw new InvalidOperationException($"Unknown type {type}.");
    }
  }
  public void Load(ZPackage pkg)
  {
    pkg.SetPos(0);
    var num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      Floats ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Floats[pkg.ReadInt()] = new ConstantFloatValue(pkg.ReadSingle());
    }
    if ((num & 2) != 0)
    {
      Vecs ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Vecs[pkg.ReadInt()] = new ConstantVector3Value(pkg.ReadVector3());
    }
    if ((num & 4) != 0)
    {
      Quats ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Quats[pkg.ReadInt()] = new ConstantQuaternionValue(pkg.ReadQuaternion());
    }
    if ((num & 8) != 0)
    {
      Ints ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Ints[pkg.ReadInt()] = new ConstantIntValue(pkg.ReadInt());
    }
    // Intended to come before strings (changing would break existing data).
    if ((num & 64) != 0)
    {
      Longs ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Longs[pkg.ReadInt()] = new ConstantLongValue(pkg.ReadLong());
    }
    if ((num & 16) != 0)
    {
      Strings ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Strings[pkg.ReadInt()] = new ConstantStringValue(pkg.ReadString());
    }
    if ((num & 128) != 0)
    {
      ByteArrays ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ByteArrays[pkg.ReadInt()] = new ConstantBytesValue(pkg.ReadByteArray());
    }
    if ((num & 256) != 0)
    {
      ConnectionType = (ZDOExtraData.ConnectionType)pkg.ReadByte();
      ConnectionHash = pkg.ReadInt();
    }
    if ((num & 512) != 0)
      Persistent = new ConstantBoolValue(pkg.ReadBool());
    if ((num & 1024) != 0)
      Distant = new ConstantBoolValue(pkg.ReadBool());
    if ((num & 2048) != 0)
      Priority = (ZDO.ObjectType)pkg.ReadByte();
    CanBeInjected = CheckCanBeInjected();
  }

  private static T ToByteEnum<T>(List<string> list) where T : struct, Enum
  {

    byte value = 0;
    foreach (var item in list)
    {
      var trimmed = item.Trim();
      if (Enum.TryParse<T>(trimmed, true, out var parsed))
        value += (byte)(object)parsed;
      else
        Log.Warning($"Failed to parse value {trimmed} as {nameof(T)}.");
    }
    return (T)(object)value;
  }
}
