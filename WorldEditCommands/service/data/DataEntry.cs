using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace Data;

// Replicates ZDO data from Valheim.
public class DataEntry
{
  public DataEntry()
  {
  }
  public DataEntry(string base64)
  {
    Load(new ZPackage(base64));
  }
  public DataEntry(DataData data)
  {
    Load(data);
  }
  public DataEntry(ZDO zdo)
  {
    Load(zdo);
  }

  // Nulls add more code but should be more performant.
  public Dictionary<int, IStringValue>? Strings;
  public Dictionary<int, IFloatValue>? Floats;
  public Dictionary<int, IIntValue>? Ints;
  public Dictionary<int, IBoolValue>? Bools;
  public Dictionary<int, IHashValue>? Hashes;
  public Dictionary<int, ILongValue>? Longs;
  public Dictionary<int, IVector3Value>? Vecs;
  public Dictionary<int, IQuaternionValue>? Quats;
  public Dictionary<int, byte[]>? ByteArrays;
  public List<ItemValue>? Items;
  public Vector2i? ContainerSize;
  public IIntValue? ItemAmount;
  public ZDOExtraData.ConnectionType ConnectionType = ZDOExtraData.ConnectionType.None;
  public int ConnectionHash = 0;
  public ZDOID OriginalId = ZDOID.None;
  public ZDOID TargetConnectionId = ZDOID.None;
  public IBoolValue? Persistent;
  public IBoolValue? Distant;
  public ZDO.ObjectType? Priority;

  public void Set(int key, string value)
  {
    Strings ??= [];
    Strings[key] = new SimpleStringValue(value);
  }
  public void Set(int key, float value)
  {
    Floats ??= [];
    Floats[key] = new SimpleFloatValue(value);
  }
  public void Set(int key, int value)
  {
    Ints ??= [];
    Ints[key] = new SimpleIntValue(value);
  }
  public void Set(int key, bool value)
  {
    Bools ??= [];
    Bools[key] = new SimpleBoolValue(value);
  }
  public void Set(int key, long value)
  {
    Longs ??= [];
    Longs[key] = new SimpleLongValue(value);
  }
  public void Set(int key, Vector3 value)
  {
    Vecs ??= [];
    Vecs[key] = new SimpleVector3Value(value);
  }
  public void Set(int key, Quaternion value)
  {
    Quats ??= [];
    Quats[key] = new SimpleQuaternionValue(value);
  }
  public void Set(int key, byte[] value)
  {
    ByteArrays ??= [];
    ByteArrays[key] = value;
  }
  public bool TryGetString(Dictionary<string, string> pars, int key, out string value)
  {
    value = "";
    if (Strings == null || !Strings.TryGetValue(key, out var val)) return false;
    var v = val.Get(pars);
    if (v == null) return false;
    value = v;
    return true;
  }
  public bool TryGetFloat(Dictionary<string, string> pars, int key, out float value)
  {
    value = 0;
    if (Floats == null || !Floats.TryGetValue(key, out var val)) return false;
    var v = val.Get(pars);
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }
  public bool TryGetInt(Dictionary<string, string> pars, int key, out int value)
  {
    value = 0;
    if (Ints == null || !Ints.TryGetValue(key, out var val)) return false;
    var v = val.Get(pars);
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }
  public bool TryGetBool(Dictionary<string, string> pars, int key, out bool value)
  {
    value = false;
    if (Bools == null || !Bools.TryGetValue(key, out var val)) return false;
    var v = val.GetInt(pars);
    if (!v.HasValue) return false;
    value = v.Value != 0;
    return true;
  }
  public bool TryGetHash(Dictionary<string, string> pars, int key, out int value)
  {
    value = 0;
    if (Hashes == null || !Hashes.TryGetValue(key, out var val)) return false;
    var v = val.Get(pars);
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }
  public bool TryGetLong(Dictionary<string, string> pars, int key, out long value)
  {
    value = 0;
    if (Longs == null || !Longs.TryGetValue(key, out var val)) return false;
    var v = val.Get(pars);
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }

  public HashSet<string> RequiredParameters = [];
  public void Load(DataEntry data)
  {
    if (data.Floats != null)
    {
      Floats ??= [];
      foreach (var pair in data.Floats)
        Floats[pair.Key] = pair.Value;
    }
    if (data.Vecs != null)
    {
      Vecs ??= [];
      foreach (var pair in data.Vecs)
        Vecs[pair.Key] = pair.Value;
    }
    if (data.Quats != null)
    {
      Quats ??= [];
      foreach (var pair in data.Quats)
        Quats[pair.Key] = pair.Value;
    }
    if (data.Ints != null)
    {
      Ints ??= [];
      foreach (var pair in data.Ints)
        Ints[pair.Key] = pair.Value;
    }
    if (data.Strings != null)
    {
      Strings ??= [];
      foreach (var pair in data.Strings)
        Strings[pair.Key] = pair.Value;
    }
    if (data.ByteArrays != null)
    {
      ByteArrays ??= [];
      foreach (var pair in data.ByteArrays)
        ByteArrays[pair.Key] = pair.Value;
    }
    if (data.Longs != null)
    {
      Longs ??= [];
      foreach (var pair in data.Longs)
        Longs[pair.Key] = pair.Value;
    }
    if (data.Bools != null)
    {
      Bools ??= [];
      foreach (var pair in data.Bools)
        Bools[pair.Key] = pair.Value;
    }
    if (data.Hashes != null)
    {
      Hashes ??= [];
      foreach (var pair in data.Hashes)
        Hashes[pair.Key] = pair.Value;
    }
    if (data.Items != null)
    {
      Items ??= [];
      foreach (var item in data.Items)
        Items.Add(item);
    }
    if (data.ContainerSize != null)
      ContainerSize = data.ContainerSize;
    if (data.ItemAmount != null)
      ItemAmount = data.ItemAmount;

    ConnectionType = data.ConnectionType;
    ConnectionHash = data.ConnectionHash;
    OriginalId = data.OriginalId;
    TargetConnectionId = data.TargetConnectionId;
    if (data.Persistent != null)
      Persistent = data.Persistent;
    if (data.Distant != null)
      Distant = data.Distant;
    if (data.Priority != null)
      Priority = data.Priority;
    foreach (var par in data.RequiredParameters)
      RequiredParameters.Add(par);
  }
  public void Load(ZDO zdo)
  {
    var id = zdo.m_uid;
    Floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => new SimpleFloatValue(kvp.Value) as IFloatValue) : null;
    Vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => new SimpleVector3Value(kvp.Value) as IVector3Value) : null;
    Quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => new SimpleQuaternionValue(kvp.Value) as IQuaternionValue) : null;
    Ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => new SimpleIntValue(kvp.Value) as IIntValue) : null;
    Strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => new SimpleStringValue(kvp.Value) as IStringValue) : null;
    Longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => new SimpleLongValue(kvp.Value) as ILongValue) : null;
    ByteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => (byte[])kvp.Value.Clone()) : null;
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
    Persistent = zdo.Persistent ? new SimpleBoolValue(true) : null;
    Distant = zdo.Distant ? new SimpleBoolValue(true) : null;
    Priority = zdo.Type;
  }
  public void Load(DataData data)
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
        Floats.Add(ZDOKeys.Hash(kvp.Key), DataValue.Float(kvp.Value, RequiredParameters));
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
        Ints.Add(ZDOKeys.Hash(kvp.Key), DataValue.Int(kvp.Value, RequiredParameters));
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
        Bools.Add(ZDOKeys.Hash(kvp.Key), DataValue.Bool(kvp.Value, RequiredParameters));
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
        Hashes.Add(ZDOKeys.Hash(kvp.Key), DataValue.Hash(kvp.Value, RequiredParameters));
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
        Longs.Add(ZDOKeys.Hash(kvp.Key), DataValue.Long(kvp.Value, RequiredParameters));
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
        Strings.Add(ZDOKeys.Hash(kvp.Key), DataValue.String(kvp.Value, RequiredParameters));
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
        Vecs.Add(ZDOKeys.Hash(kvp.Key), DataValue.Vector3(kvp.Value, RequiredParameters));
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
        Quats.Add(ZDOKeys.Hash(kvp.Key), DataValue.Quaternion(kvp.Value, RequiredParameters));
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
        ByteArrays.Add(ZDOKeys.Hash(kvp.Key), Convert.FromBase64String(kvp.Value));
      }
    }
    if (data.items != null)
    {
      Items = [.. data.items.Select(item => new ItemValue(item, RequiredParameters))];
    }
    if (!string.IsNullOrWhiteSpace(data.containerSize))
      ContainerSize = Parse.Vector2Int(data.containerSize!);
    if (!string.IsNullOrWhiteSpace(data.itemAmount))
      ItemAmount = DataValue.Int(data.itemAmount!, RequiredParameters);
    if (componentsToAdd.Count > 0)
    {
      Ints ??= [];
      Ints[$"HasFields".GetStableHashCode()] = DataValue.Simple(1);
      foreach (var component in componentsToAdd)
        Ints[$"HasFields{component}".GetStableHashCode()] = DataValue.Simple(1);
    }
    if (data.persistent != null)
      Persistent = DataValue.Bool(data.persistent, RequiredParameters);
    if (data.distant != null)
      Distant = DataValue.Bool(data.distant, RequiredParameters);
    if (data.priority != null)
      Priority = Enum.TryParse<ZDO.ObjectType>(data.priority, true, out var parsed) ? parsed : null;
    if (!string.IsNullOrWhiteSpace(data.connection))
    {
      var split = Parse.SplitWithEmpty(data.connection!);
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
        Floats[pkg.ReadInt()] = DataValue.Float(pkg);
    }
    if ((num & 2) != 0)
    {
      Vecs ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Vecs[pkg.ReadInt()] = DataValue.Vector3(pkg);
    }
    if ((num & 4) != 0)
    {
      Quats ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Quats[pkg.ReadInt()] = DataValue.Quaternion(pkg);
    }
    if ((num & 8) != 0)
    {
      Ints ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Ints[pkg.ReadInt()] = DataValue.Int(pkg);
    }
    // Intended to come before strings (changing would break existing data).
    if ((num & 64) != 0)
    {
      Longs ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Longs[pkg.ReadInt()] = DataValue.Long(pkg);
    }
    if ((num & 16) != 0)
    {
      Strings ??= [];
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        Strings[pkg.ReadInt()] = DataValue.String(pkg);
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
    if ((num & 512) != 0)
      Persistent = DataValue.Bool(pkg);
    if ((num & 1024) != 0)
      Distant = DataValue.Bool(pkg);
    if ((num & 2048) != 0)
      Priority = (ZDO.ObjectType)pkg.ReadByte();
  }
  public bool Match(Dictionary<string, string> pars, ZDO zdo)
  {
    AddParameters(pars, zdo);
    if (Strings != null && Strings.Any(pair => pair.Value.Match(pars, zdo.GetString(pair.Key)) == false)) return false;
    if (Floats != null && Floats.Any(pair => pair.Value.Match(pars, zdo.GetFloat(pair.Key)) == false)) return false;
    if (Ints != null && Ints.Any(pair => pair.Value.Match(pars, zdo.GetInt(pair.Key)) == false)) return false;
    if (Longs != null && Longs.Any(pair => pair.Value.Match(pars, zdo.GetLong(pair.Key)) == false)) return false;
    if (Bools != null && Bools.Any(pair => pair.Value.Match(pars, zdo.GetBool(pair.Key)) == false)) return false;
    if (Hashes != null && Hashes.Any(pair => pair.Value.Match(pars, zdo.GetInt(pair.Key)) == false)) return false;
    if (Vecs != null && Vecs.Any(pair => pair.Value.Match(pars, zdo.GetVec3(pair.Key, Vector3.zero)) == false)) return false;
    if (Quats != null && Quats.Any(pair => pair.Value.Match(pars, zdo.GetQuaternion(pair.Key, Quaternion.identity)) == false)) return false;
    if (ByteArrays != null && ByteArrays.Any(pair => pair.Value.SequenceEqual(zdo.GetByteArray(pair.Key)) == false)) return false;
    if (Persistent != null && Persistent.Match(pars, zdo.Persistent) == false) return false;
    if (Distant != null && Distant.Match(pars, zdo.Distant) == false) return false;
    if (Priority != null && Priority.Value != zdo.Type) return false;
    return true;
  }
  public bool Unmatch(Dictionary<string, string> pars, ZDO zdo)
  {
    AddParameters(pars, zdo);
    if (Strings != null && Strings.Any(pair => pair.Value.Match(pars, zdo.GetString(pair.Key)) == true)) return false;
    if (Floats != null && Floats.Any(pair => pair.Value.Match(pars, zdo.GetFloat(pair.Key)) == true)) return false;
    if (Ints != null && Ints.Any(pair => pair.Value.Match(pars, zdo.GetInt(pair.Key)) == true)) return false;
    if (Longs != null && Longs.Any(pair => pair.Value.Match(pars, zdo.GetLong(pair.Key)) == true)) return false;
    if (Bools != null && Bools.Any(pair => pair.Value.Match(pars, zdo.GetBool(pair.Key)) == true)) return false;
    if (Hashes != null && Hashes.Any(pair => pair.Value.Match(pars, zdo.GetInt(pair.Key)) == true)) return false;
    if (Vecs != null && Vecs.Any(pair => pair.Value.Match(pars, zdo.GetVec3(pair.Key, Vector3.zero)) == true)) return false;
    if (Quats != null && Quats.Any(pair => pair.Value.Match(pars, zdo.GetQuaternion(pair.Key, Quaternion.identity)) == true)) return false;
    if (ByteArrays != null && ByteArrays.Any(pair => pair.Value.SequenceEqual(zdo.GetByteArray(pair.Key)) == true)) return false;
    if (Persistent != null && Persistent.Match(pars, zdo.Persistent) == true) return false;
    if (Distant != null && Distant.Match(pars, zdo.Distant) == true) return false;
    if (Priority != null && Priority.Value == zdo.Type) return false;
    return true;
  }
  private void AddParameters(Dictionary<string, string> pars, ZDO? zdo)
  {
    // Custom parameters might include parameters.
    foreach (var value in pars.Values.ToArray())
    {
      AddNestedParameters(value, pars, zdo);
    }
    foreach (var par in RequiredParameters)
    {
      var key = $"<{par}>";
      if (pars.ContainsKey(key)) continue;
      // Don't use empty failsafes because sometimes tags must be passed (like <br> in strings).
      // If people miss parameters that's their fault.
      AddParameter(par, pars, zdo);
    }
  }
  private void AddNestedParameters(string value, Dictionary<string, string> pars, ZDO? zdo)
  {
    if (!value.Contains("<")) return;
    var split = value.Split('<', '>');
    for (var i = 1; i < split.Length; i += 2)
    {
      var key = $"<{split[i]}>";
      if (pars.ContainsKey(key)) continue;
      AddParameter(split[i], pars, zdo);
    }
  }
  private void AddParameter(string par, Dictionary<string, string> pars, ZDO? zdo)
  {
    var key = $"<{par}>";
    if (DataLoading.TryGetValueFromGroup(par, out var value))
    {
      pars[key] = value;
      // Value groups might include parameters.
      AddNestedParameters(value, pars, zdo);
      return;
    }
    if (key.Contains("_"))
    {
      if (zdo == null) return;
      var split = par.Split('_');
      if (split.Length < 2) return;
      var type = split[0];
      var zdoKey = split[1];
      key = $"<{type}_{zdoKey}>";
      if (type == "string")
        pars[key] = zdo.GetString(zdoKey);
      else if (type == "float")
        pars[key] = zdo.GetFloat(zdoKey).ToString(CultureInfo.InvariantCulture);
      else if (type == "int")
        pars[key] = zdo.GetInt(zdoKey).ToString(CultureInfo.InvariantCulture);
      else if (type == "long")
        pars[key] = zdo.GetLong(zdoKey).ToString(CultureInfo.InvariantCulture);
      else if (type == "bool")
        pars[key] = zdo.GetBool(zdoKey).ToString();
      else if (type == "hash")
        pars[key] = zdo.GetInt(zdoKey).ToString(CultureInfo.InvariantCulture);
      else if (type == "vec")
        pars[key] = Helper.PrintVectorXZY(zdo.GetVec3(zdoKey, Vector3.zero));
      else if (type == "quat")
        pars[key] = Helper.PrintAngleYXZ(zdo.GetQuaternion(zdoKey, Quaternion.identity));
      else if (type == "byte")
        pars[key] = Convert.ToBase64String(zdo.GetByteArray(zdoKey));
    }
    else
    {
      if (key == "<x>" && zdo != null)
        pars[key] = zdo.m_position.x.ToString(CultureInfo.InvariantCulture);
      else if (key == "<y>" && zdo != null)
        pars[key] = zdo.m_position.y.ToString(CultureInfo.InvariantCulture);
      else if (key == "<z>" && zdo != null)
        pars[key] = zdo.m_position.z.ToString(CultureInfo.InvariantCulture);
      else if (key == "<rot>" && zdo != null)
        pars[key] = Helper.PrintAngleYXZ(zdo.GetRotation());
    }
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

  // Evaluates parameters and legacy-format migration once, producing a snapshot ready to write to a ZDO.
  public PlainDataEntry Resolve(Dictionary<string, string> pars, ZDO? zdo = null)
  {
    AddParameters(pars, zdo);
    PlainDataEntry plain = new()
    {
      Strings = Strings?.Select(kvp => new KeyValuePair<int, string?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!),
      Floats = Floats?.Select(kvp => new KeyValuePair<int, float?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Value),
      Longs = Longs?.Select(kvp => new KeyValuePair<int, long?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Value),
      Vecs = Vecs?.Select(kvp => new KeyValuePair<int, Vector3?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Value),
      Quats = Quats?.Select(kvp => new KeyValuePair<int, Quaternion?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Value),
      ByteArrays = ByteArrays != null ? new Dictionary<int, byte[]>(ByteArrays) : null,
      ConnectionType = ConnectionType,
      ConnectionHash = ConnectionHash,
      OriginalId = OriginalId,
      TargetConnectionId = TargetConnectionId,
      Persistent = Persistent?.GetBool(pars) ?? zdo?.Persistent ?? true,
      Distant = Distant?.GetBool(pars) ?? zdo?.Distant ?? false,
      Priority = Priority ?? zdo?.Type ?? ZDO.ObjectType.Default,
    };
    Dictionary<int, int> ints = [];
    if (Ints != null)
      foreach (var pair in Ints)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue) ints[pair.Key] = value.Value;
      }
    if (Hashes != null)
      foreach (var pair in Hashes)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue) ints[pair.Key] = value.Value;
      }
    if (Bools != null)
      foreach (var pair in Bools)
      {
        var value = pair.Value.GetInt(pars);
        if (value.HasValue) ints[pair.Key] = value.Value;
      }
    if (ints.Count > 0) plain.Ints = ints;
    RollItems(pars, plain.ByteArrays ??= []);
    ItemDataHelper.ConvertInventory(plain);
    ItemDataHelper.ConvertItemNames(plain);
    ItemDataHelper.ConvertItemData(plain);
    return plain;
  }
  public void Write(Dictionary<string, string> pars, ZDO zdo)
  {
    Resolve(pars, zdo).Write(zdo);
  }
  public string GetBase64(Dictionary<string, string> pars) => Resolve(pars).GetBase64();
  public void Write(Dictionary<string, string> pars, ZPackage pkg) => Resolve(pars).Write(pkg);

  private void RollItems(Dictionary<string, string> pars, Dictionary<int, byte[]> byteArrays)
  {
    if (Items?.Count > 0)
    {
      var pkg = ItemValue.LoadItems(pars, Items, ContainerSize, ItemAmount?.Get(pars) ?? 0);
      byteArrays[ZDOVars.s_items] = pkg.GetArray();
    }
  }
}

