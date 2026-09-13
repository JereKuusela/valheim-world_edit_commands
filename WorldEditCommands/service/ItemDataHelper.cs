using System;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace Service;

/// <summary>Plain data snapshot of an item, decoupled from ItemDrop.ItemData/prefab lifetime.</summary>
public class ItemRecord
{
  public int PrefabHash;
  public string PrefabName = "";
  public int Stack;
  public float Durability;
  public Vector2i GridPos;
  public bool Equipped;
  public int Quality = 1;
  public int Variant;
  public long CrafterID;
  public string CrafterName = "";
  public Dictionary<string, string> CustomData = [];
  public int WorldLevel;
  public bool PickedUp;
  public bool Cheated;
}

/// <summary>Reads/writes ZDOVars.s_items without instantiating item GameObjects (unlike Inventory.Load/AddItem).</summary>
public static class ItemDataHelper
{
  public static ZPackage? GetPackage(ZDO zdo)
  {
    var bytes = zdo.GetByteArray(ZDOVars.s_items);
    if (bytes != null && bytes.Length > 0) return new ZPackage(bytes);
    var str = zdo.GetString(ZDOVars.s_items, "");
    return str != "" ? new ZPackage(str) : null;
  }

  public static List<ItemRecord> Load(ZDO zdo)
  {
    var pkg = GetPackage(zdo);
    return pkg == null ? [] : Load(pkg);
  }

  public static List<ItemRecord> Load(ZPackage pkg)
  {
    List<ItemRecord> records = [];
    try
    {
      // Inventory uses int for version, itemDrops uses byte.
      var version = (Version.Item)pkg.ReadInt();
      // Item Drawers mod uses the same ZDO key but writes 0 as version, so it's detected and skipped here.
      if (version == 0) return records;
      if (version >= Version.Item.Smaller)
        LoadNew(pkg, version, records);
      else
        LoadOld(pkg, version, records);
    }
    catch { }
    return records;
  }

  private static void LoadNew(ZPackage pkg, Version.Item version, List<ItemRecord> records)
  {
    var count = pkg.ReadUShort();
    for (var i = 0; i < count; i++)
    {
      var (hash, item) = ItemDrop.ItemData.Load(pkg, version);
      // Empty PrefabName means the prefab no longer exists (removed mod/item).
      var prefabName = hash != 0 ? ObjectDB.instance.GetItemPrefab(hash)?.name ?? "" : "";
      records.Add(new ItemRecord
      {
        PrefabHash = hash,
        PrefabName = prefabName,
        Stack = item.m_stack,
        Durability = item.m_durability,
        GridPos = item.m_gridPos,
        Equipped = item.m_equipped,
        Quality = item.m_quality,
        Variant = item.m_variant,
        CrafterID = item.m_crafterID,
        CrafterName = item.m_crafterName,
        CustomData = item.m_customData,
        WorldLevel = item.m_worldLevel,
        PickedUp = item.m_pickedUp,
        Cheated = item.m_cheated,
      });
    }
  }

  private static void LoadOld(ZPackage pkg, Version.Item version, List<ItemRecord> records)
  {
    var count = pkg.ReadInt();
    for (var i = 0; i < count; i++)
    {
      var name = pkg.ReadString();
      var stack = pkg.ReadInt();
      var durability = pkg.ReadSingle();
      var pos = pkg.ReadVector2i();
      var equipped = pkg.ReadBool();
      var quality = version >= Version.Item.Quality ? pkg.ReadInt() : 1;
      var variant = version >= Version.Item.Variant ? pkg.ReadInt() : 0;
      var crafterID = 0L;
      var crafterName = "";
      if (version >= Version.Item.CrafterID)
      {
        crafterID = pkg.ReadLong();
        crafterName = pkg.ReadString();
      }
      Dictionary<string, string> customData = [];
      if (version >= Version.Item.CustomData)
      {
        var dataCount = pkg.ReadInt();
        for (var j = 0; j < dataCount; j++)
          customData[pkg.ReadString()] = pkg.ReadString();
      }
      var worldLevel = version >= Version.Item.WorldLevel ? pkg.ReadInt() : 0;
      var pickedUp = version >= Version.Item.PickedUp && pkg.ReadBool();
      var cheated = version == Version.Item.AbandonedDN && pkg.ReadBool();

      // Empty PrefabName means the prefab no longer exists (removed mod/item).
      var hash = name != "" ? name.GetStableHashCode() : 0;
      var prefabName = hash != 0 ? ObjectDB.instance.GetItemPrefab(hash)?.name ?? "" : "";
      records.Add(new ItemRecord
      {
        PrefabHash = hash,
        PrefabName = prefabName,
        Stack = stack,
        Durability = durability,
        GridPos = pos,
        Equipped = equipped,
        Quality = quality,
        Variant = variant,
        CrafterID = crafterID,
        CrafterName = crafterName,
        CustomData = customData,
        WorldLevel = worldLevel,
        PickedUp = pickedUp,
        Cheated = cheated,
      });
    }
  }

  public static int CountInvalid(List<ItemRecord> records) => records.Count(r => r.PrefabName == "");

  public static List<ItemRecord> RemoveInvalid(List<ItemRecord> records) => [.. records.Where(r => r.PrefabName != "")];

  public static byte[] Save(List<ItemRecord> records)
  {
    ZPackage pkg = new();
    Save(records, pkg);
    return pkg.GetArray();
  }

  public static void Save(List<ItemRecord> records, ZPackage pkg)
  {
    pkg.Write((byte)Version.Item.ChunksNCheats);
    pkg.Write((ushort)records.Count);
    foreach (var record in records)
    {
      ItemDrop.ItemData item = new()
      {
        m_dropPrefab = ObjectDB.instance.GetItemPrefab(record.PrefabHash),
        m_stack = record.Stack,
        m_durability = record.Durability,
        m_gridPos = record.GridPos,
        m_equipped = record.Equipped,
        m_quality = record.Quality,
        m_variant = record.Variant,
        m_crafterID = record.CrafterID,
        m_crafterName = record.CrafterName,
        m_customData = record.CustomData,
        m_worldLevel = record.WorldLevel,
        m_pickedUp = record.PickedUp,
        m_cheated = record.Cheated,
      };
      item.Save(pkg);
    }
  }

  // Prefab name fields used to be stored as strings, now they are hashed.
  private static readonly int[] HashKeys = [
    ZDOVars.s_content,
    ZDOVars.s_item,
    .. Enumerable.Range(0, 15).Select(i => $"{i}_item".GetStableHashCode())
  ];

  // Items used to be stored as a base64 string, now they belong in ByteArrays.
  public static void ConvertInventory(PlainDataEntry data)
  {
    if (data.Strings == null || !data.Strings.TryGetValue(ZDOVars.s_items, out var value)) return;
    if (!string.IsNullOrEmpty(value))
    {
      data.ByteArrays ??= [];
      data.ByteArrays[ZDOVars.s_items] = Convert.FromBase64String(value);
    }
    data.Strings.Remove(ZDOVars.s_items);
    if (data.Strings.Count == 0) data.Strings = null;
  }

  // Inventory prefab fields used to be stored as strings, now they are hashed.
  public static void ConvertItemNames(PlainDataEntry data)
  {
    if (data.Strings == null) return;
    foreach (var key in HashKeys)
    {
      if (!data.Strings.TryGetValue(key, out var value)) continue;
      if (!string.IsNullOrEmpty(value))
      {
        data.Ints ??= [];
        data.Ints[key] = value.GetStableHashCode();
      }
      data.Strings.Remove(key);
    }
    if (data.Strings.Count == 0) data.Strings = null;
  }

  // Item fields used to be stored directly on the ZDO, now they belong in a single s_itemData byte array.
  // Bools (pickedUp/cheated) live in Ints here, matching how ZDOExtraData actually stores them.
  private static bool HasLegacyItemData(PlainDataEntry data) =>
    data.Floats?.ContainsKey(ZDOVars.s_durability) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_stack) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_quality) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_variant) == true ||
    data.Longs?.ContainsKey(ZDOVars.s_crafterID) == true ||
    data.Strings?.ContainsKey(ZDOVars.s_crafterName) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_dataCount) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_worldLevel) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_pickedUp) == true ||
    data.Ints?.ContainsKey(ZDOVars.s_cheated) == true;

  public static void ConvertItemData(PlainDataEntry data)
  {
    if (!HasLegacyItemData(data)) return;

    byte[]? existing = null;
    data.ByteArrays?.TryGetValue(ZDOVars.s_itemData, out existing);
    var itemData = ParseLegacyItemData(data, existing);

    var pkg = new ZPackage();
    pkg.Write((byte)Version.Item.ChunksNCheats);
    itemData.Save(pkg);
    data.ByteArrays ??= [];
    data.ByteArrays[ZDOVars.s_itemData] = pkg.GetArray();

    RemoveLegacyItemData(data, itemData);
  }

  // Builds an ItemData from legacy loose fields, merged onto the existing s_itemData bytes if present.
  private static ItemDrop.ItemData ParseLegacyItemData(PlainDataEntry data, byte[]? existing)
  {
    ItemDrop.ItemData itemData = new();
    if (existing != null && existing.Length > 0)
    {
      var existingPkg = new ZPackage(existing);
      // Inventory uses int for version, itemDrops uses byte.
      var existingVersion = (Version.Item)existingPkg.ReadByte();
      (_, itemData) = ItemDrop.ItemData.Load(existingPkg, existingVersion);
    }

    if (data.Floats != null && data.Floats.TryGetValue(ZDOVars.s_durability, out var durability))
      itemData.m_durability = durability;
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_stack, out var stack))
      itemData.m_stack = stack;
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_quality, out var quality))
      itemData.m_quality = quality;
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_variant, out var variant))
      itemData.m_variant = variant;
    if (data.Longs != null && data.Longs.TryGetValue(ZDOVars.s_crafterID, out var crafterId))
      itemData.m_crafterID = crafterId;
    if (data.Strings != null && data.Strings.TryGetValue(ZDOVars.s_crafterName, out var crafterName))
      itemData.m_crafterName = crafterName;
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_dataCount, out var dataCount))
    {
      for (var i = 0; i < dataCount; i++)
      {
        var keyHash = $"data_{i}".GetStableHashCode();
        var valueHash = $"data__{i}".GetStableHashCode();
        var key = data.Strings != null && data.Strings.TryGetValue(keyHash, out var k) ? k : null;
        var value = data.Strings != null && data.Strings.TryGetValue(valueHash, out var v) ? v : null;
        if (!string.IsNullOrEmpty(key))
          itemData.m_customData[key!] = value ?? "";
        data.Strings?.Remove(keyHash);
        data.Strings?.Remove(valueHash);
      }
    }
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_worldLevel, out var worldLevel))
      itemData.m_worldLevel = (byte)worldLevel;
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_pickedUp, out var pickedUp))
      itemData.m_pickedUp = pickedUp != 0;
    if (data.Ints != null && data.Ints.TryGetValue(ZDOVars.s_cheated, out var cheated))
      itemData.m_cheated = cheated != 0;

    return itemData;
  }

  // Removes legacy loose item fields after they've been folded into s_itemData.
  // Quality and variant are kept only if they differ from their defaults, matching vanilla ConvertInventories.
  private static void RemoveLegacyItemData(PlainDataEntry data, ItemDrop.ItemData itemData)
  {
    data.Floats?.Remove(ZDOVars.s_durability);
    data.Ints?.Remove(ZDOVars.s_stack);
    if (itemData.m_quality == 1)
      data.Ints?.Remove(ZDOVars.s_quality);
    if (itemData.m_variant == 0)
      data.Ints?.Remove(ZDOVars.s_variant);
    data.Longs?.Remove(ZDOVars.s_crafterID);
    data.Strings?.Remove(ZDOVars.s_crafterName);
    data.Ints?.Remove(ZDOVars.s_dataCount);
    data.Ints?.Remove(ZDOVars.s_worldLevel);
    data.Ints?.Remove(ZDOVars.s_pickedUp);
    data.Ints?.Remove(ZDOVars.s_cheated);
    if (data.Floats?.Count == 0) data.Floats = null;
    if (data.Ints?.Count == 0) data.Ints = null;
    if (data.Longs?.Count == 0) data.Longs = null;
    if (data.Strings?.Count == 0) data.Strings = null;
  }
}
