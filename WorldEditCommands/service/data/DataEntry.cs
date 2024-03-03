using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace Data;

// Replicates ZDO data from Valheim.
public class DataEntry
{
  public DataEntry(string base64)
  {
    Load(new ZPackage(base64));
  }
  public DataEntry(DataData data)
  {
    Load(data);
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
  public ItemValue[]? Items;
  public Vector2i? ContainerSize;
  public IIntValue? ItemAmount;
  public ZDOExtraData.ConnectionType ConnectionType = ZDOExtraData.ConnectionType.None;
  public int ConnectionHash = 0;
  public ZDOID OriginalId = ZDOID.None;
  public ZDOID TargetConnectionId = ZDOID.None;

  public HashSet<string> RequiredParameters = [];

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
        Floats.Add(Helper.Hash(kvp.Key), DataValue.Float(kvp.Value, RequiredParameters));
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
        Ints.Add(Helper.Hash(kvp.Key), DataValue.Int(kvp.Value, RequiredParameters));
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
        Bools.Add(Helper.Hash(kvp.Key), DataValue.Bool(kvp.Value, RequiredParameters));
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
        Hashes.Add(Helper.Hash(kvp.Key), DataValue.Hash(kvp.Value, RequiredParameters));
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
        Longs.Add(Helper.Hash(kvp.Key), DataValue.Long(kvp.Value, RequiredParameters));
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
        Strings.Add(Helper.Hash(kvp.Key), DataValue.String(kvp.Value, RequiredParameters));
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
        Vecs.Add(Helper.Hash(kvp.Key), DataValue.Vector3(kvp.Value, RequiredParameters));
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
        Quats.Add(Helper.Hash(kvp.Key), DataValue.Quaternion(kvp.Value, RequiredParameters));
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
        ByteArrays.Add(Helper.Hash(kvp.Key), Convert.FromBase64String(kvp.Value));
      }
    }
    if (data.items != null)
    {
      Items = data.items.Select(item => new ItemValue(item, RequiredParameters)).ToArray();
    }
    if (!string.IsNullOrWhiteSpace(data.containerSize))
      ContainerSize = Parse.Vector2Int(data.containerSize);
    if (!string.IsNullOrWhiteSpace(data.itemAmount))
      ItemAmount = DataValue.Int(data.itemAmount!, RequiredParameters);
    if (componentsToAdd.Count > 0)
    {
      Ints ??= [];
      Ints[$"HasFields".GetStableHashCode()] = DataValue.Simple(1);
      foreach (var component in componentsToAdd)
        Ints[$"HasFields{component}".GetStableHashCode()] = DataValue.Simple(1);
    }
    if (!string.IsNullOrWhiteSpace(data.connection))
    {
      var split = Parse.SplitWithEmpty(data.connection);
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
        ServerDevcommands.ServerDevcommands.Log.LogWarning($"Failed to parse value {trimmed} as {nameof(T)}.");
    }
    return (T)(object)value;
  }

  public void Write(Dictionary<string, string> pars, ZDO zdo)
  {
    AddParameters(pars, zdo);
    RollItems(pars);
    var id = zdo.m_uid;
    if (Floats?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_floats, id);
      foreach (var pair in Floats)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_floats[id].SetValue(pair.Key, value.Value);
      }
    }
    if (Vecs?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_vec3, id);
      foreach (var pair in Vecs)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_vec3[id].SetValue(pair.Key, value.Value);

      }
    }
    if (Quats?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_quats, id);
      foreach (var pair in Quats)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_quats[id].SetValue(pair.Key, value.Value);
      }
    }
    if (Ints?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_ints, id);
      foreach (var pair in Ints)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_ints[id].SetValue(pair.Key, value.Value);
      }
    }
    if (Hashes?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_ints, id);
      foreach (var pair in Hashes)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_ints[id].SetValue(pair.Key, value.Value);
      }
    }
    if (Bools?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_ints, id);
      foreach (var pair in Bools)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_ints[id].SetValue(pair.Key, value.Value);
      }
    }
    if (Longs?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_longs, id);
      foreach (var pair in Longs)
      {
        var value = pair.Value.Get(pars);
        if (value.HasValue)
          ZDOExtraData.s_longs[id].SetValue(pair.Key, value.Value);

      }
    }
    if (Strings?.Count > 0)
    {
      ZDOHelper.Init(ZDOExtraData.s_strings, id);
      foreach (var pair in Strings)
      {
        var value = pair.Value.Get(pars);
        if (value != null)
          ZDOExtraData.s_strings[id].SetValue(pair.Key, value);

      }
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
  public string GetBase64(Dictionary<string, string> pars)
  {
    var pkg = new ZPackage();
    Write(pars, pkg);
    return pkg.GetBase64();
  }
  public void Write(Dictionary<string, string> pars, ZPackage pkg)
  {
    AddParameters(pars, null);
    RollItems(pars);
    var num = 0;
    if (Floats != null)
      num |= 1;
    if (Vecs != null)
      num |= 2;
    if (Quats != null)
      num |= 4;
    if (Ints != null || Hashes != null || Bools != null)
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
      var kvps = Floats.Select(kvp => new KeyValuePair<int, float?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray();
      pkg.Write((byte)kvps.Count());
      foreach (var kvp in kvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
    }
    if (Vecs != null)
    {
      var kvps = Vecs.Select(kvp => new KeyValuePair<int, Vector3?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray();
      pkg.Write((byte)kvps.Count());
      foreach (var kvp in kvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
    }
    if (Quats != null)
    {
      var kvps = Quats.Select(kvp => new KeyValuePair<int, Quaternion?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray();
      pkg.Write((byte)kvps.Count());
      foreach (var kvp in kvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
    }
    if (Ints != null || Hashes != null || Bools != null)
    {
      var intKvps = Ints?.Select(kvp => new KeyValuePair<int, int?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray() ?? [];
      var hashKvps = Hashes?.Select(kvp => new KeyValuePair<int, int?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray() ?? [];
      var boolKvps = Bools?.Select(kvp => new KeyValuePair<int, int?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray() ?? [];
      var count = intKvps.Length + hashKvps.Length + boolKvps.Length;
      pkg.Write((byte)count);
      foreach (var kvp in intKvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
      foreach (var kvp in hashKvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
      foreach (var kvp in boolKvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
    }
    if (Longs != null)
    {
      var kvps = Longs.Select(kvp => new KeyValuePair<int, long?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value.HasValue).ToArray();
      pkg.Write((byte)kvps.Count());
      foreach (var kvp in kvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!.Value);
      }
    }
    if (Strings != null)
    {
      var kvps = Strings.Select(kvp => new KeyValuePair<int, string?>(kvp.Key, kvp.Value.Get(pars))).Where(kvp => kvp.Value != null).ToArray();
      pkg.Write((byte)kvps.Count());
      foreach (var kvp in kvps)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value!);
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

  private void RollItems(Dictionary<string, string> pars)
  {
    if (Items?.Length > 0)
    {
      var encoded = ItemValue.LoadItems(pars, Items, ContainerSize, ItemAmount?.Get(pars) ?? 0);
      Strings ??= [];
      Strings[ZDOVars.s_items] = DataValue.Simple(encoded);
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
