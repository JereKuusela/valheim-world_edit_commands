// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data;

// Fully resolved ZDO data, built from a DataEntry (which is never mutated).
public class ResolvedDataEntry
{
  // Nulls add more code but should be more performant.
  public Dictionary<int, string>? Strings;
  public Dictionary<int, float>? Floats;
  public Dictionary<int, int>? Ints;
  public Dictionary<int, long>? Longs;
  public Dictionary<int, Vector3>? Vecs;
  public Dictionary<int, Quaternion>? Quats;
  public Dictionary<int, byte[]>? ByteArrays;
  public ZDOExtraData.ConnectionType? ConnectionType;
  public int ConnectionHash = 0;
  public ZDOID? OriginalId;
  public ZDOID? TargetConnectionId;
  // Null means unspecified, so the ZDO is left untouched.
  public Vector3? Position;
  // Euler angles.
  public Vector3? Rotation;
  public bool? Persistent;
  public bool? Distant;
  public ZDO.ObjectType? Priority;

  // Source is only used to look up item and inventory data, data itself is not modified.
  public void Load(DataEntry data, Functions f, ZDO? source)
  {
    if (data.Floats?.Count > 0)
    {
      foreach (var pair in data.Floats)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddFloat(pair.Key, value.Value);
      }
    }
    if (data.Ints?.Count > 0)
    {
      foreach (var pair in data.Ints)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddInt(pair.Key, value.Value);
      }
    }
    if (data.Longs?.Count > 0)
    {
      foreach (var pair in data.Longs)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddLong(pair.Key, value.Value);
      }
    }
    if (data.Strings?.Count > 0)
    {
      foreach (var pair in data.Strings)
      {
        var value = pair.Value.Get(f);
        if (value != null)
          AddString(pair.Key, value);
      }
    }
    if (data.Vecs?.Count > 0)
    {
      foreach (var pair in data.Vecs)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddVec(pair.Key, value.Value);
      }
    }
    if (data.Quats?.Count > 0)
    {
      foreach (var pair in data.Quats)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddQuat(pair.Key, value.Value);
      }
    }
    if (data.ByteArrays?.Count > 0)
    {
      foreach (var pair in data.ByteArrays)
      {
        var value = pair.Value.Get(f);
        if (value != null)
          AddByteArray(pair.Key, value);
      }
    }
    var itemData = data.CreateItemData(f, source);
    if (itemData != null)
      AddByteArray(ZDOVars.s_itemData, itemData);
    var inventory = data.CreateInventory(f, source);
    if (inventory != null)
      AddByteArray(ZDOVars.s_items, inventory);
    if (data.Bools?.Count > 0)
    {
      foreach (var pair in data.Bools)
      {
        var value = pair.Value.GetInt(f);
        if (value.HasValue)
          AddInt(pair.Key, value.Value);
      }
    }
    if (data.Hashes?.Count > 0)
    {
      foreach (var pair in data.Hashes)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddInt(pair.Key, value.Value);
      }
    }
    if (data.Components != null)
    {
      foreach (var pair in data.Components)
      {
        var value = pair.Value.Get(f);
        if (value.HasValue)
          AddInt(pair.Key, value.Value);
      }
    }
    ConnectionHash = data.ConnectionHash;
    ConnectionType = data.ConnectionType;
    if (data.OriginalId != null)
      OriginalId = data.OriginalId.Get(f);
    if (data.TargetConnectionId != null)
      TargetConnectionId = data.TargetConnectionId.Get(f);
    Distant = data.Distant?.GetBool(f);
    Persistent = data.Persistent?.GetBool(f);
    Priority = data.Priority;
    Position = data.Position?.Get(f) ?? Position;
    Rotation = data.Rotation?.Get(f)?.eulerAngles ?? Rotation;
    // Legacy loose item fields must be folded into a packed s_itemData.
    ItemDataHelper.ConvertAll(this);
  }

  public virtual void Write(ZDO zdo)
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
    if (Distant.HasValue) zdo.Distant = Distant.Value;
    if (Persistent.HasValue) zdo.Persistent = Persistent.Value;
    if (Priority.HasValue) zdo.Type = Priority.Value;
    if (Position.HasValue) zdo.SetPosition(Position.Value);
    if (Rotation.HasValue) zdo.m_rotation = Rotation.Value;
  }

  public virtual bool HasSyncedChanges()
  {
    if (Floats?.Count > 0) return true;
    if (Ints?.Count > 0) return true;
    if (Longs?.Count > 0) return true;
    if (Strings?.Count > 0) return true;
    if (Vecs?.Count > 0) return true;
    if (Quats?.Count > 0) return true;
    if (ByteArrays?.Count > 0) return true;
    if (ConnectionType.HasValue) return true;
    if (ConnectionHash != 0) return true;
    if (OriginalId.HasValue) return true;
    if (TargetConnectionId.HasValue) return true;
    if (Persistent.HasValue) return true;
    if (Distant.HasValue) return true;
    if (Priority.HasValue) return true;
    return false;
  }

  private void HandleConnection(ZDO ownZdo)
  {
    var originalId = OriginalId ?? ZDOID.None;
    if (originalId == ZDOID.None) return;
    var type = ConnectionType ?? ZDOExtraData.ConnectionType.None;
    var targetId = TargetConnectionId ?? ZDOID.None;
    var ownId = ownZdo.m_uid;
    if (targetId != ZDOID.None)
    {
      // If target is known, the setup is easy.
      var otherZdo = ZDOMan.instance.GetZDO(targetId);
      if (otherZdo == null) return;

      ownZdo.SetConnection(type, targetId);
      // Portal is two way.
      if (type == ZDOExtraData.ConnectionType.Portal)
        otherZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ownId);
    }
    else
    {
      // Otherwise all zdos must be scanned.
      var other = ZDOExtraData.s_connections.FirstOrDefault(kvp => kvp.Value.m_target == originalId);
      if (other.Value == null) return;
      var otherZdo = ZDOMan.instance.GetZDO(other.Key);
      if (otherZdo == null) return;
      // Connection is always one way here, otherwise TargetConnectionId would be set.
      otherZdo.SetConnection(other.Value.m_type, ownId);
    }
  }
  private void HandleHashConnection(ZDO ownZdo)
  {
    var type = ConnectionType ?? ZDOExtraData.ConnectionType.None;
    if (ConnectionHash == 0) return;
    if (type == ZDOExtraData.ConnectionType.None) return;
    var ownId = ownZdo.m_uid;

    // Hash data is regenerated on world save.
    // But in this case, it's manually set, so might be needed later.
    ZDOExtraData.SetConnectionData(ownId, type, ConnectionHash);

    // While actual connection can be one way, hash is always two way.
    // One of the hashes always has the target type.
    var otherType = type ^ ZDOExtraData.ConnectionType.Target;
    var isOtherTarget = (type & ZDOExtraData.ConnectionType.Target) == 0;
    var zdos = ZDOExtraData.GetAllConnectionZDOIDs(otherType);
    var otherId = zdos.FirstOrDefault(z => ZDOExtraData.GetConnectionHashData(z, type)?.m_hash == ConnectionHash);
    if (otherId == ZDOID.None) return;
    var otherZdo = ZDOMan.instance.GetZDO(otherId);
    if (otherZdo == null) return;
    if ((type & ZDOExtraData.ConnectionType.Spawned) > 0)
    {
      // Spawn is one way.
      var connZDO = isOtherTarget ? ownZdo : otherZdo;
      var targetId = isOtherTarget ? otherId : ownId;
      connZDO.SetConnection(ZDOExtraData.ConnectionType.Spawned, targetId);
    }
    if ((type & ZDOExtraData.ConnectionType.SyncTransform) > 0)
    {
      // Sync is one way.
      var connZDO = isOtherTarget ? ownZdo : otherZdo;
      var targetId = isOtherTarget ? otherId : ownId;
      connZDO.SetConnection(ZDOExtraData.ConnectionType.SyncTransform, targetId);
    }
    if ((type & ZDOExtraData.ConnectionType.Portal) > 0)
    {
      // Portal is two way.
      otherZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ownId);
      ownZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, otherId);
    }
  }

  protected virtual void AddString(int key, string value)
  {
    Strings ??= [];
    Strings[key] = value;
  }
  protected virtual void AddFloat(int key, float value)
  {
    Floats ??= [];
    Floats[key] = value;
  }
  protected virtual void AddInt(int key, int value)
  {
    Ints ??= [];
    Ints[key] = value;
  }
  protected virtual void AddLong(int key, long value)
  {
    Longs ??= [];
    Longs[key] = value;
  }
  protected virtual void AddVec(int key, Vector3 value)
  {
    Vecs ??= [];
    Vecs[key] = value;
  }
  protected virtual void AddQuat(int key, Quaternion value)
  {
    Quats ??= [];
    Quats[key] = value;
  }
  protected virtual void AddByteArray(int key, byte[] value)
  {
    ByteArrays ??= [];
    ByteArrays[key] = value;
  }
}
