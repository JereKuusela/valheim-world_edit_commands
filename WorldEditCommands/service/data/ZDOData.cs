using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data;

// Replicates ZDO data from Valheim.
public class ZDOData
{
  public ZDOData(ZDO zdo)
  {
    Load(zdo);
  }
  public ZDOData(string base64)
  {
    Load(new ZPackage(base64));
  }
  public ZDOData(DataData data)
  {
    Load(data);
  }

  // Nulls add more code but should be more performant.
  public Dictionary<int, StringValue>? Strings;
  public Dictionary<int, FloatValue>? Floats;
  public Dictionary<int, IntValue>? Ints;
  public Dictionary<int, LongValue>? Longs;
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
    Floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => new FloatValue(kvp.Value)) : null;
    Ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => new IntValue(kvp.Value)) : null;
    Longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => new LongValue(kvp.Value)) : null;
    Strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => new StringValue(kvp.Value)) : null;
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
  public void Load(DataData data)
  {
    if (data.floats != null)
    {
      Floats ??= [];
      foreach (var value in data.floats)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Floats.Add(hash, new(split));
      }
    }
    if (data.ints != null)
    {
      Ints ??= [];
      foreach (var value in data.ints)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Ints.Add(hash, new(split));
      }
    }
    if (data.bools != null)
    {
      Ints ??= [];
      foreach (var value in data.bools)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Ints.Add(hash, new BoolValue(split));
      }
    }
    if (data.hashes != null)
    {
      Ints ??= [];
      foreach (var value in data.hashes)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Ints.Add(hash, new HashValue(split));
      }
    }
    if (data.longs != null)
    {
      Longs ??= [];
      foreach (var value in data.longs)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Longs.Add(hash, new(split));
      }
    }
    if (data.strings != null)
    {
      Strings ??= [];
      foreach (var value in data.strings)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Strings.Add(hash, new(split));
      }
    }
    if (data.vecs != null)
    {
      Vecs ??= [];
      foreach (var value in data.vecs)
      {
        var split = Parse.Split(value);
        if (split.Length != 4) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Vecs.Add(hash, Parse.VectorXZY(split, 1));
      }
    }
    if (data.quats != null)
    {
      Quats ??= [];
      foreach (var value in data.quats)
      {
        var split = Parse.Split(value);
        if (split.Length != 4) continue;
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        Quats.Add(hash, Parse.AngleYXZ(split, 1));
      }
    }
    if (data.bytes != null)
    {
      ByteArrays ??= [];
      foreach (var value in data.bytes)
      {
        var split = Parse.Split(value);
        if (split.Length < 2) continue;
        var str = string.Join(",", split.Skip(1));
        var hash = int.TryParse(split[0], out var h) ? h : Helper.Hash(split[0]);
        ByteArrays.Add(hash, Convert.FromBase64String(str));
      }
    }
    if (!string.IsNullOrWhiteSpace(data.connection))
    {
      var split = Parse.Split(data.connection);
      if (split.Length > 1)
      {
        var types = split.Take(split.Length - 1).ToList();
        var hash = split[split.Length - 1];
        ConnectionType = ToByteEnum<ZDOExtraData.ConnectionType>(types);
        ConnectionHash = Parse.Int(hash);
        if (ConnectionHash == 0) ConnectionHash = hash.GetStableHashCode();
      }
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
        Floats[pkg.ReadInt()] = new(pkg.ReadSingle());
    }
    if ((num & 2) != 0)
    {
      Vecs ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Vecs[pkg.ReadInt()] = pkg.ReadVector3();
    }
    if ((num & 4) != 0)
    {
      Quats ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Quats[pkg.ReadInt()] = pkg.ReadQuaternion();
    }
    if ((num & 8) != 0)
    {
      Ints ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Ints[pkg.ReadInt()] = new(pkg.ReadInt());
    }
    // Intended to come before strings (changing would break existing data).
    if ((num & 64) != 0)
    {
      Longs ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Longs[pkg.ReadInt()] = new(pkg.ReadLong());
    }
    if ((num & 16) != 0)
    {
      Strings ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Strings[pkg.ReadInt()] = new(pkg.ReadString());
    }
    if ((num & 128) != 0)
    {
      ByteArrays ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ByteArrays[pkg.ReadInt()] = pkg.ReadByteArray();
    }
    if ((num & 256) != 0)
    {
      ConnectionType = (ZDOExtraData.ConnectionType)pkg.ReadByte();
      ConnectionHash = pkg.ReadInt();
    }
  }
  public bool Match(ZDO zdo)
  {
    if (Strings != null && !Strings.All(pair => pair.Value.Match(zdo.GetString(pair.Key)))) return false;
    if (Floats != null && !Floats.All(pair => pair.Value.Match(zdo.GetFloat(pair.Key)))) return false;
    if (Ints != null && !Ints.All(pair => pair.Value.Match(zdo.GetInt(pair.Key)))) return false;
    if (Longs != null && !Longs.All(pair => pair.Value.Match(zdo.GetLong(pair.Key)))) return false;
    if (Vecs != null && !Vecs.All(pair => pair.Value == zdo.GetVec3(pair.Key, Vector3.zero))) return false;
    if (Quats != null && !Quats.All(pair => pair.Value == zdo.GetQuaternion(pair.Key, Quaternion.identity))) return false;
    if (ByteArrays != null && !ByteArrays.All(pair => pair.Value.SequenceEqual(zdo.GetByteArray(pair.Key)))) return false;
    return true;
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
        ServerDevcommands.ServerDevcommands.Log.LogWarning($"Failed to parse value {trimmed} as {nameof(T)}.");
    }
    return (T)(object)value;
  }

  public void Write(ZDO zdo)
  {
    // Hack to allow resetting object health to default.
    // Proper way to remove keys could be supported but so far this is the only use case.
    if (Floats != null && Floats.TryGetValue(ZDOVars.s_health, out var h) && h.Get() == 0f)
      Floats.Remove(ZDOVars.s_health);
    var id = zdo.m_uid;
    if (Floats?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_floats, id);
      foreach (var pair in Floats)
        ZDOExtraData.s_floats[id].SetValue(pair.Key, pair.Value.Get());
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
        ZDOExtraData.s_ints[id].SetValue(pair.Key, pair.Value.Get());
    }
    if (Longs?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_longs, id);
      foreach (var pair in Longs)
        ZDOExtraData.s_longs[id].SetValue(pair.Key, pair.Value.Get());
    }
    if (Strings?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_strings, id);
      foreach (var pair in Strings)
        ZDOExtraData.s_strings[id].SetValue(pair.Key, pair.Value.Get());
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
  public string GetBase64()
  {
    var pkg = new ZPackage();
    Write(pkg);
    return pkg.GetBase64();
  }
  public void Write(ZPackage pkg)
  {
    var num = 0;
    if (Floats != null)
      num |= 1;
    if (Vecs != null)
      num |= 2;
    if (Quats != null)
      num |= 4;
    if (Ints != null)
      num |= 8;
    if (Strings != null)
      num |= 16;
    if (Longs != null)
      num |= 64;
    if (ByteArrays != null)
      num |= 128;
    if (ConnectionType != ZDOExtraData.ConnectionType.None && ConnectionHash != 0)
      num |= 256;

    pkg.Write(num);
    if (Floats != null)
    {
      pkg.Write((byte)Floats.Count());
      foreach (var kvp in Floats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value.Get());
      }
    }
    if (Vecs != null)
    {
      pkg.Write((byte)Vecs.Count());
      foreach (var kvp in Vecs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Quats != null)
    {
      pkg.Write((byte)Quats.Count());
      foreach (var kvp in Quats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (Ints != null)
    {
      pkg.Write((byte)Ints.Count());
      foreach (var kvp in Ints)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value.Get());
      }
    }
    if (Longs != null)
    {
      pkg.Write((byte)Longs.Count());
      foreach (var kvp in Longs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value.Get());
      }
    }
    if (Strings != null)
    {
      pkg.Write((byte)Strings.Count());
      foreach (var kvp in Strings)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value.Get());
      }
    }
    if (ByteArrays != null)
    {
      pkg.Write((byte)ByteArrays.Count());
      foreach (var kvp in ByteArrays)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (ConnectionType != ZDOExtraData.ConnectionType.None && ConnectionHash != 0)
    {
      pkg.Write((byte)ConnectionType);
      pkg.Write(ConnectionHash);
    }
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


public class StringValue
{
  private readonly string? Value;
  private readonly string[]? Values;
  public StringValue(string value)
  {
    Value = value;
  }
  public StringValue(string[] values)
  {
    if (values.Length == 2)
      Value = values[1];
    else
    {
      Values = values.Skip(1).ToArray();
    }
  }
  public string Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    return Value ?? "";
  }
  public bool Match(string value)
  {
    if (Values != null) return Values.Contains(value);
    return Value == value;
  }
}
public class IntValue
{
  protected int? Value;
  protected Range<int>? Range;
  protected int[]? Values;
  public IntValue(int value)
  {
    Value = value;
  }
  public IntValue()
  {

  }
  public IntValue(string[] values)
  {
    if (values.Length == 2)
    {
      var range = Parse.IntRange(values[1]);
      if (range.Min == range.Max)
        Value = range.Min;
      else
        Range = range;
    }
    else
      Values = values.Skip(1).Select(s => Parse.Int(s)).ToArray();
  }
  public int Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    if (Range != null) return Random.Range(Range.Min, Range.Max + 1);
    return Value ?? 0;
  }
  public bool Match(int value)
  {
    if (Values != null) return Values.Contains(value);
    if (Range != null) return value >= Range.Min && value <= Range.Max;
    return Value == value;
  }
}
public class HashValue : IntValue
{
  public HashValue(string[] values)
  {
    if (values.Length == 2)
      Value = values[1].GetStableHashCode();
    else
      Values = values.Skip(1).Select(s => s.GetStableHashCode()).ToArray();
  }
}
public class BoolValue : IntValue
{
  public BoolValue(string[] values)
  {
    if (values.Length == 2)
      Value = values[1] == "true" ? 1 : 0;
    else
      Values = values.Skip(1).Select(s => s == "true" ? 1 : 0).ToArray();
  }
}
public class FloatValue
{
  private readonly float? Value;
  private readonly Range<float>? Range;
  private readonly float[]? Values;
  public FloatValue(float value)
  {
    Value = value;
  }
  public FloatValue(string[] values)
  {
    if (values.Length == 2)
    {
      var range = Parse.FloatRange(values[1]);
      if (range.Min == range.Max)
        Value = range.Min;
      else
        Range = range;
    }
    else
      Values = values.Skip(1).Select(s => Parse.Float(s)).ToArray();
  }
  public float Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    if (Range != null) return Random.Range(Range.Min, Range.Max);
    return Value ?? 0f;
  }
  public bool Match(float value)
  {
    if (Values != null) return Values.Contains(value);
    if (Range != null) return value >= Range.Min && value <= Range.Max;
    return Value == value;
  }
}
public class LongValue
{
  private readonly long? Value;
  private readonly long[]? Values;
  public LongValue(long value)
  {
    Value = value;
  }
  public LongValue(string[] values)
  {
    if (values.Length == 2)
      Value = Parse.Long(values[1]);
    else
      Values = values.Skip(1).Select(s => Parse.Long(s)).ToArray();
  }
  public long Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    return Value ?? 0L;
  }
  public bool Match(long value)
  {
    if (Values != null) return Values.Contains(value);
    return Value == value;
  }
}
