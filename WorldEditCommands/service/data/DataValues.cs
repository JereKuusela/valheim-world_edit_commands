
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;

namespace Data;

public class DataValue
{

  public static IZdoIdValue ZdoId(string values)
  {
    var split = SplitWithValues(values);
    var zdo = Parse.ZdoId(split[0]);
    if (split.Length == 1 && zdo != ZDOID.None)
      return new SimpleZdoIdValue(zdo);
    return new ZdoIdValue(split);
  }
  // Different function name because string would be ambiguous.
  public static IIntValue Simple(int value) => new SimpleIntValue(value);
  public static IStringValue Simple(string value) => new SimpleStringValue(value);
  public static IFloatValue Simple(float value) => new SimpleFloatValue(value);
  public static ILongValue Simple(long value) => new SimpleLongValue(value);
  public static IVector3Value Simple(Vector3 value) => new SimpleVector3Value(value);
  public static IQuaternionValue Simple(Quaternion value) => new SimpleQuaternionValue(value);
  public static IBytesValue Simple(byte[]? value) => new SimpleBytesValue(value);

  public static IIntValue Int(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && int.TryParse(split[0], out var result))
      return new SimpleIntValue(result);
    return new IntValue(split);
  }

  public static IRangeIntValue RangeInt(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
    {
      if (int.TryParse(split[0], out var result))
        return new SimpleRangeIntValue(new Range<int>(result, 0));
      var range = Parse.IntRange(split[0]);
      return new SimpleRangeIntValue(range);
    }
    return new RangeIntValue(split);
  }

  public static IFloatValue Float(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && Parse.TryFloat(split[0], out var result))
      return new SimpleFloatValue(result);
    return new FloatValue(split);
  }

  public static ILongValue Long(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && Parse.TryLong(split[0], out var result))
      return new SimpleLongValue(result);
    return new LongValue(split);
  }

  public static IStringValue String(string values)
  {
    // Quick hack for quoted strings.
    if (values.Length > 2 && values[0] == '"' && values[values.Length - 1] == '"')
      return new SimpleStringValue(values.Substring(1, values.Length - 2));
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
      return new SimpleStringValue(split[0]);
    return new StringValue(split);
  }
  public static IBoolValue Bool(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && bool.TryParse(split[0], out var result))
      return new SimpleBoolValue(result);
    return new BoolValue(split);
  }

  public static IBytesValue Bytes(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
    {
      if (string.IsNullOrEmpty(split[0]))
        return new SimpleBytesValue(null);
      try
      {
        var bytes = System.Convert.FromBase64String(split[0]);
        return new SimpleBytesValue(bytes);
      }
      catch (System.FormatException)
      {
        // If not valid base64, treat as a dynamic value
      }
    }
    return new BytesValue(split);
  }

  public static IHashValue Hash(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
      return new SimpleHashValue(split[0]);
    return new HashValue(split);
  }
  public static IPrefabValue Prefab(string values)
  {
    if (HasFunctions(values))
      return new PrefabValue(SplitWithValues(values));
    var prefabs = PrefabHelper.GetPrefabs(values, "");
    if (prefabs.Count == 0) return new SimplePrefabValue(null);
    if (prefabs.Count == 1) return new SimplePrefabValue(prefabs[0]);
    return new SimplePrefabsValue(prefabs);
  }

  public static IVector3Value Vector3(string values)
  {
    var split = SplitWithValues(values);
    if (HasFunctions(values) || split.Length > 3)
    {
      // Vectors are trickly to handle because the coordinate separator is same as value separator.
      List<string> combined = [];
      for (var i = 0; i < split.Length; i += 3)
      {
        // Last vector can be partial.
        var v = i + 3 < split.Length ? split.Skip(i).Take(3).ToArray() : [.. split.Skip(i)];
        combined.Add(string.Join(",", v));
      }
      return new Vector3Value([.. combined]);
    }
    if (Parse.TryDistanceAngle(split, out var polar))
      return new SimpleVector3Value(polar);
    var parsed = Parse.VectorXZYNull(split);
    return new SimpleVector3Value(parsed.HasValue ? parsed.Value : UnityEngine.Vector3.zero);
  }
  public static IQuaternionValue Quaternion(string values)
  {
    var split = SplitWithValues(values);
    if (HasFunctions(values) || split.Length > 3)
    {
      List<string> combined = [];
      for (var i = 0; i < split.Length; i += 3)
      {
        var v = i + 3 < split.Length ? split.Skip(i).Take(3).ToArray() : [.. split.Skip(i)];
        combined.Add(string.Join(",", v));
      }
      return new QuaternionValue([.. combined]);
    }
    var parsed = Parse.AngleYXZNull(split);
    return new SimpleQuaternionValue(parsed.HasValue ? parsed.Value : UnityEngine.Quaternion.identity);
  }

  private static bool HasFunctions(string value) => value.Contains("<") && value.Contains(">");

  private static string[] SplitWithValues(string str)
  {
    List<string> result = [];
    var split = Parse.SplitWithEmpty(str);
    foreach (var value in split)
    {
      if (!value.Contains("<") || !value.Contains(">"))
      {
        result.Add(value);
        continue;
      }
      var parSplit = value.Split('<', '>');
      List<string> parameters = [];
      List<int> hashes = [];
      for (var i = 1; i < parSplit.Length; i += 2)
      {
        var hash = parSplit[i].ToLowerInvariant().GetStableHashCode();
        // Value groups should work same as using the value directly.
        // So it makes sense to resolve them on load.
        // This way other code doesn't have to worry about resolving them and performance is better.
        // Originally they were resolved later because World Edit Commands allows overriding them.
        // But this is not needed for EW Prefabs.
        if (DataLoading.ValueGroups.ContainsKey(hash))
        {
          parameters.Add($"<{parSplit[i]}>");
          hashes.Add(hash);
        }
      }
      if (parameters.Count == 0)
      {
        result.Add(value);
        // Early exit because preloading too many values wastes memory.
        if (result.Count > 1000)
          break;
      }
      else
        SubstitueValues(result, value, parameters, hashes, 0);

    }
    if (result.Count > 1000)
      Log.Warning("Too many values loaded for " + str);
    return result.Count > 1000 ? [str] : [.. result];
  }

  // Recursion needed because there can be multiple value groups.
  private static void SubstitueValues(List<string> result, string format, List<string> parameters, List<int> hashes, int index)
  {
    var groups = DataLoading.ValueGroups[hashes[index]];
    foreach (var group in groups)
    {
      var newFormat = format.Replace(parameters[index], group);
      if (index == parameters.Count - 1)
      {
        result.Add(newFormat);
        // Early exit because preloading too many values wastes memory.
        if (result.Count > 1000)
          break;
      }
      else
        SubstitueValues(result, newFormat, parameters, hashes, index + 1);
    }
  }
}


public class AnyValue(string[] values)
{
  protected readonly string[] Values = values;
  public IReadOnlyList<string> RawValues => Values;

  private string? RollValue()
  {
    if (Values.Length == 1)
      return Values[0];
    return Values[Random.Range(0, Values.Length)];
  }
  protected string? GetValue(Functions f)
  {
    var value = RollValue();
    if (value == null || value == "<none>")
      return null;
    return f.Replace(value);
  }
  protected string? GetValue()
  {
    var value = RollValue();
    return value == null || value == "<none>" ? null : value;
  }
  protected List<string> GetAllValues(Functions f)
  {
    return [.. Values.Select(f.Replace).Where(v => v != null && v != "" && v != "<none>")];
  }
  protected string GetWholeValue(Functions f)
  {
    return string.Join(",", Values.Select(f.Replace));
  }
}
public partial class ItemValue(ItemData data)
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
