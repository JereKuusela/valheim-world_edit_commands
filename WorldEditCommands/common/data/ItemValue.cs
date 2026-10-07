// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace Data;

public partial class ItemValue(ItemYaml data)
{

  public static bool Match(Functions f, List<ItemValue> data, ZDO zdo, IIntValue? amount)
  {
    var records = ItemDataHelper.Load(zdo);
    var matches = data.Count(item => item.Match(f, records));
    // If no amount is set, then must match exactly.
    if (amount == null)
      return matches == data.Count && records.Count == 0;
    return amount.Match(f, matches) == true;
  }
  public static bool Match(Functions f, ZDO zdo, IIntValue amount)
  {
    var records = ItemDataHelper.Load(zdo);
    return amount.Match(f, records.Count) == true;
  }

  // Item drop entity: prefab and position come from the ZDO itself.
  public bool MatchSingle(Functions f, ZDO zdo)
  {
    var record = ItemDataHelper.LoadSingle(zdo);
    if (record == null) return false;
    if (Stack?.Match(f, record.Stack) == false) return false;
    return MatchProperties(f, record);
  }
  public byte[]? Create(Functions f, ZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
    if (prefab == null || !prefab.TryGetComponent(out ItemDrop _)) return null;
    RolledStack = Stack?.Get(f) ?? 1;
    return ItemDataHelper.Serialize(CreateItemData(f, prefab));
  }

  public static string LoadItems(Functions f, List<ItemValue> items, Vector2i size, int amount) =>
    System.Convert.ToBase64String(LoadItemBytes(f, items, size, amount));

  internal static byte[] LoadItemBytes(Functions f, List<ItemValue> items, Vector2i size, int amount)
  {
    ZPackage pkg = new();
    // Inventory uses int for version, itemDrops uses byte.
    pkg.Write((int)Version.Item.ChunksNCheats);
    items = Generate(f, items, size, amount);
    items = [.. items.Where(item => item.CanWrite())];
    pkg.Write((ushort)items.Count);
    foreach (var item in items)
      item.Write(f, pkg);
    return pkg.GetArray();
  }
  public static List<ItemValue> Generate(Functions f, List<ItemValue> data, Vector2i size, int amount)
  {
    var fixedPos = data.Where(item => item.Position != "").ToList();
    var randomPos = data.Where(item => item.Position == "").ToList();
    Dictionary<Vector2i, ItemValue> inventory = [];
    foreach (var item in fixedPos)
    {
      if (!item.Roll(f)) continue;
      inventory[item.RolledPosition] = item;
    }
    if (amount == 0)
      GenerateEach(f, inventory, size, randomPos);
    else
      GenerateAmount(f, inventory, size, randomPos, amount);
    return [.. inventory.Values];
  }
  private static void GenerateEach(Functions f, Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items)
  {
    foreach (var item in items)
    {
      if (!item.Roll(f)) continue;
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      item.RolledPosition = slot.Value;
      inventory[slot.Value] = item;
    }
  }
  private static void GenerateAmount(Functions f, Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items, int amount)
  {
    var maxWeight = items.Sum(item => item.Chance);
    for (var i = 0; i < amount && items.Count > 0; ++i)
    {
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      var item = RollItem(items, maxWeight);
      item.RolledPosition = slot.Value;
      if (item.RollPrefab(f))
        inventory[slot.Value] = item;
      maxWeight -= item.Chance;
      items.Remove(item);
    }
  }
  private static ItemValue RollItem(List<ItemValue> items, float maxWeight)
  {
    var roll = Random.Range(0f, maxWeight);
    foreach (var item in items)
    {
      if (roll < item.Chance)
        return item;
      roll -= item.Chance;
    }
    return items.Last();
  }
  private static Vector2i? FindNextFreeSlot(Dictionary<Vector2i, ItemValue> inventory, Vector2i size)
  {
    var maxW = size.x;
    var maxH = size.y;
    for (var y = 0; y < maxH; ++y)
      for (var x = 0; x < maxW; ++x)
      {
        var pos = new Vector2i(x, y);
        if (!inventory.ContainsKey(pos))
          return pos;
      }
    return null;
  }
  // Prefab is saved as string, so hash can't be used.
  public IPrefabValue Prefab = DataValue.Prefab(data.prefab);
  public float Chance = data.chance;
  public IIntValue? Stack = data.stack == null ? null : DataValue.Int(data.stack);
  public IFloatValue? Durability = data.durability == null ? null : DataValue.Float(data.durability);
  public string Position = data.pos;
  private Vector2i RolledPosition = Parse.Vector2Int(data.pos);
  public IBoolValue? Equipped = data.equipped == null ? null : DataValue.Bool(data.equipped);
  public IIntValue? Quality = data.quality == null ? null : DataValue.Int(data.quality);
  public IIntValue? Variant = data.variant == null ? null : DataValue.Int(data.variant);
  public ILongValue? CrafterID = data.crafterID == null ? null : DataValue.Long(data.crafterID);
  public IStringValue? CrafterName = data.crafterName == null ? null : DataValue.String(data.crafterName);
  public Dictionary<string, IStringValue>? CustomData = data.customData?.ToDictionary(kvp => kvp.Key, kvp => DataValue.String(kvp.Value));
  public IIntValue? WorldLevel = data.worldLevel == null ? null : DataValue.Int(data.worldLevel);
  public IBoolValue? PickedUp = data.pickedUp == null ? null : DataValue.Bool(data.pickedUp);
  public IBoolValue? Cheated = data.cheated == null ? null : DataValue.Bool(data.cheated);
  // Must know before writing is the prefab good, so it has to be rolled first.
  private int RolledPrefab = 0;
  private int RolledStack = 0;
  public bool RollPrefab(Functions f)
  {
    RolledPrefab = Prefab.Get(f) ?? 0;
    RolledStack = Stack?.Get(f) ?? 1;
    return RolledPrefab != 0 && RolledStack != 0;
  }
  public bool RollChance() => Chance >= 1f || Random.value <= Chance;
  public bool Roll(Functions f) => RollChance() && RollPrefab(f);
  private bool CanWrite()
  {
    var prefab = ObjectDB.instance.GetItemPrefab(RolledPrefab);
    return prefab != null && prefab.TryGetComponent(out ItemDrop _);
  }
  public void Write(Functions f, ZPackage pkg)
  {
    var prefab = ObjectDB.instance.GetItemPrefab(RolledPrefab);
    if (prefab == null || !prefab.TryGetComponent(out ItemDrop _)) return;
    var itemData = CreateItemData(f, prefab);
    itemData.m_gridPos = RolledPosition;
    itemData.Save(pkg);
  }

  private ItemDrop.ItemData CreateItemData(Functions f, GameObject prefab)
  {
    var customData = CustomData?.ToDictionary(x => x.Key, x => x.Value.Get(f) ?? "");
    return ItemDataHelper.Create(
      prefab,
      RolledStack,
      Durability?.Get(f),
      Quality?.Get(f) ?? 1,
      Variant?.Get(f) ?? 0,
      CrafterID?.Get(f) ?? 0L,
      CrafterName?.Get(f) ?? "",
      WorldLevel?.Get(f) ?? 0,
      PickedUp?.GetBool(f) ?? false,
      Cheated?.GetBool(f) ?? false,
      Equipped?.GetBool(f) ?? false,
      customData);
  }

  public bool Match(Functions f, List<ItemRecord> records)
  {
    var item = FindMatch(f, records);
    if (item == null) return false;
    records.Remove(item);
    return true;
  }
  private ItemRecord? FindMatch(Functions f, List<ItemRecord> records)
  {
    if (Position != "")
    {
      var item = records.FirstOrDefault(r => r.GridPos == RolledPosition);
      if (item == null) return null;
      if (Stack?.Match(f, item.Stack) == false) return null;
      if (MatchItem(f, item)) return item;
      return null;
    }
    foreach (var item in records)
    {
      if (Stack?.Match(f, item.Stack) == false) continue;
      if (MatchItem(f, item)) return item;
    }
    return null;
  }
  private bool MatchItem(Functions f, ItemRecord item)
  {
    if (Prefab.Match(f, item.PrefabHash) == false) return false;
    return MatchProperties(f, item);
  }
  private bool MatchProperties(Functions f, ItemRecord item)
  {
    if (Durability?.Match(f, item.Durability) == false) return false;
    if (Equipped?.Match(f, item.Equipped) == false) return false;
    if (Quality?.Match(f, item.Quality) == false) return false;
    if (Variant?.Match(f, item.Variant) == false) return false;
    if (CrafterID?.Match(f, item.CrafterID) == false) return false;
    if (CrafterName?.Match(f, item.CrafterName) == false) return false;
    if (WorldLevel?.Match(f, item.WorldLevel) == false) return false;
    if (PickedUp?.Match(f, item.PickedUp) == false) return false;
    if (Cheated?.Match(f, item.Cheated) == false) return false;
    if (CustomData == null) return true;
    foreach (var kvp in CustomData)
    {
      if (!item.CustomData.TryGetValue(kvp.Key, out var value)) return false;
      if (kvp.Value.Match(f, value) == false) return false;
    }
    return true;
  }
}
