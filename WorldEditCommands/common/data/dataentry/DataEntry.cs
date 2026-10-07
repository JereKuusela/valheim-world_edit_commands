// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;

namespace Data;

public partial class DataEntry
{
  public DataEntry()
  {
  }
  public DataEntry(string[] typeKeyValues)
  {
    Load(typeKeyValues);
  }
  public DataEntry(DataYaml data)
  {
    Load(data);
  }
  public DataEntry(ZDO zdo)
  {
    Load(zdo);
  }
  public DataEntry(ZPackage pkg)
  {
    Load(pkg);
  }
  private static readonly int HasFieldsHash = ZdoHelper.Hash("HasFields");

  public bool CanBeInjected = true;
  // Nulls add more code but should be more performant.
  public Dictionary<int, IStringValue>? Strings;
  public Dictionary<int, IFloatValue>? Floats;
  public Dictionary<int, IIntValue>? Ints;
  // Separate from ints so that these don't get matched.
  public Dictionary<int, IIntValue>? Components;
  public Dictionary<int, IBoolValue>? Bools;
  public Dictionary<int, IHashValue>? Hashes;
  public Dictionary<int, ILongValue>? Longs;
  public Dictionary<int, IVector3Value>? Vecs;
  public Dictionary<int, IQuaternionValue>? Quats;
  public Dictionary<int, IBytesValue>? ByteArrays;
  public List<ItemValue>? Items;
  public ItemValue? Item;
  public Vector2i? ContainerSize;
  public IIntValue? ItemAmount;
  public ZDOExtraData.ConnectionType? ConnectionType;
  public int ConnectionHash = 0;
  public IZdoIdValue? OriginalId;
  public IZdoIdValue? TargetConnectionId;
  public IBoolValue? Persistent;
  public IBoolValue? Distant;
  public ZDO.ObjectType? Priority;
  public IVector3Value? Position;
  public IQuaternionValue? Rotation;

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
    if (data.Components != null)
    {
      Components ??= [];
      foreach (var pair in data.Components)
        Components[pair.Key] = pair.Value;
    }
    if (data.Items != null)
    {
      Items ??= [];
      foreach (var item in data.Items)
        Items.Add(item);
    }
    if (data.Item != null)
      Item = data.Item;
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
    if (data.Position != null)
      Position = data.Position;
    if (data.Rotation != null)
      Rotation = data.Rotation;
    CanBeInjected = data.CanBeInjected;
  }
  // Reusing the same object keeps references working.
  public DataEntry Reset(DataYaml data)
  {
    CanBeInjected = true;
    Floats = null;
    Vecs = null;
    Quats = null;
    Ints = null;
    Strings = null;
    ByteArrays = null;
    Longs = null;
    Bools = null;
    Hashes = null;
    Items = null;
    Item = null;
    Components = null;
    ContainerSize = null;
    ItemAmount = null;
    ConnectionType = null;
    ConnectionHash = 0;
    OriginalId = null;
    TargetConnectionId = null;
    Position = null;
    Rotation = null;
    Distant = null;
    Persistent = null;
    Priority = null;
    Load(data);
    return this;
  }

  private bool CheckCanBeInjected() =>
  // Level requires regeneration to refresh health.
    (Ints == null || (!Ints.ContainsKey(HasFieldsHash) && !Ints.ContainsKey(ZDOVars.s_level)))
    && Components == null
    && Position == null
    && Rotation == null;
}
