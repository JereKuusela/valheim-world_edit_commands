
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace Data;
public class StringValue
{
  private readonly string? Value;
  private readonly string?[]? Values;
  public StringValue(KeyValuePair<int, string> value)
  {
    Value = value.Value;
  }
  public StringValue(ZPackage pkg)
  {
    Value = pkg.ReadString();
  }
  public StringValue(string value) : this(Parse.SplitWithEscape(value))
  {
  }
  public StringValue(IEnumerable<string> values)
  {
    if (values.Count() == 1)
      Value = Read(values.First());
    else
    {
      Values = values.Select(Read).ToArray();
    }
  }
  static string? Read(string value) => value == "<none>" ? null : value;
  public string? Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    return Value;
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
  protected int?[]? Values;
  public IntValue(KeyValuePair<int, int> value)
  {
    Value = value.Value;
  }
  public IntValue(ZPackage pkg)
  {
    Value = pkg.ReadInt();
  }
  public IntValue(string value) : this(Parse.SplitWithEmpty(value))
  {
  }
  public IntValue(int value)
  {
    Value = value;
  }
  public IntValue(IEnumerable<string> values)
  {
    if (values.Count() == 1)
    {
      var range = Parse.IntRange(values.First());
      if (range.Min == range.Max)
        Value = Read(values.First());
      else
        Range = range;
    }
    else
      Values = values.Select(Read).ToArray();
  }
  protected virtual int? Read(string value) => Parse.IntNull(value);
  public int? Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    if (Range != null) return Random.Range(Range.Min, Range.Max + 1);
    return Value;
  }
  public bool Match(int value)
  {
    if (Values != null) return Values.Contains(value);
    if (Range != null) return value >= Range.Min && value <= Range.Max;
    return (Value ?? 0) == value;
  }
}
public class HashValue : IntValue
{
  protected override int? Read(string value) => value == "<none>" ? null : value == "" ? null : value.GetStableHashCode();
  public HashValue(string value) : base(value)
  {
  }
  public HashValue(IEnumerable<string> values) : base(values)
  {
  }
}
public class BoolValue : IntValue
{
  protected override int? Read(string value) => value == "true" ? 1 : value == "false" ? 0 : null;
  public BoolValue(string value) : base(value)
  {
  }
  public BoolValue(IEnumerable<string> values) : base(values)
  {
  }
}
public class FloatValue
{
  private readonly float? Value;
  private readonly Range<float>? Range;
  private readonly float?[]? Values;
  public FloatValue(KeyValuePair<int, float> value)
  {
    Value = value.Value;
  }
  public FloatValue(ZPackage pkg)
  {
    Value = pkg.ReadSingle();
  }
  public FloatValue(string value) : this(Parse.SplitWithEmpty(value))
  {
  }
  public FloatValue(IEnumerable<string> values)
  {
    if (values.Count() == 1)
    {
      var range = Parse.FloatRange(values.First());
      if (range.Min == range.Max)
        Value = Parse.FloatNull(values.First());
      else
        Range = range;
    }
    else
      Values = values.Select(Parse.FloatNull).ToArray();
  }
  public float? Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    if (Range != null) return Random.Range(Range.Min, Range.Max);
    return Value;
  }
  public bool Match(float value)
  {
    if (Values != null) return Values.Any(v => Helper.Approx(v ?? 0, value));
    if (Range != null) return value >= Range.Min - 0.001f && value <= Range.Max + 0.001f;
    return Helper.Approx(Value ?? 0f, value);
  }
}
public class LongValue
{
  private readonly long? Value;
  private readonly long?[]? Values;
  public LongValue(KeyValuePair<int, long> value)
  {
    Value = value.Value;
  }
  public LongValue(ZPackage pkg)
  {
    Value = pkg.ReadLong();
  }
  public LongValue(string value) : this(Parse.SplitWithEmpty(value))
  {
  }
  public LongValue(IEnumerable<string> values)
  {
    if (values.Count() == 1)
      Value = Parse.LongNull(values.First());
    else
      Values = values.Select(Parse.LongNull).ToArray();
  }
  public long? Get()
  {
    if (Values != null) return Values[Random.Range(0, Values.Length)];
    return Value;
  }
  public bool Match(long value)
  {
    if (Values != null) return Values.Contains(value);
    return Value == value;
  }
}

public class ItemValue(ItemData data)
{
  public static string LoadItems(ItemValue[] items, Vector2i? size, Range<int>? amount)
  {
    ZPackage pkg = new();
    pkg.Write(106);
    items = Generate(items, size ?? new(0, 0), amount ?? new(0));
    pkg.Write(items.Length);
    foreach (var item in items)
      item.Write(pkg);
    return pkg.GetBase64();
  }
  public static ItemValue[] Generate(ItemValue[] data, Vector2i size, Range<int> amount)
  {
    var fixedPos = data.Where(item => item.Position != "").ToList();
    var randomPos = data.Where(item => item.Position == "").ToList();
    Dictionary<Vector2i, ItemValue> inventory = [];
    foreach (var item in fixedPos)
    {
      if (!item.Roll()) continue;
      inventory[item.RolledPosition] = item;
    }
    var amountToGenerate = amount.Min == amount.Max ? amount.Min : Random.Range(amount.Min, amount.Max + 1);
    if (amountToGenerate == 0)
      GenerateEach(inventory, size, randomPos);
    else
      GenerateAmount(inventory, size, randomPos, amountToGenerate);
    return [.. inventory.Values];
  }
  private static void GenerateEach(Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items)
  {
    foreach (var item in items)
    {
      if (!item.Roll()) continue;
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      item.RolledPosition = slot.Value;
      inventory[slot.Value] = item;
    }
  }
  private static void GenerateAmount(Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items, int amount)
  {
    var maxWeight = items.Sum(item => item.Chance);
    for (var i = 0; i < amount && items.Count > 0; ++i)
    {
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      var item = RollItem(items, maxWeight);
      item.RolledPosition = slot.Value;
      if (item.RollPrefab())
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
    var maxW = size.x == 0 ? 4 : size.x;
    var maxH = size.y == 0 ? 2 : size.y;
    for (var y = 0; y < maxH; ++y)
      for (var x = 0; x < maxW; ++x)
      {
        var pos = new Vector2i(x, y);
        if (!inventory.ContainsKey(pos))
          return pos;
      }
    return null;
  }
  public StringValue Prefab = new(data.prefab);
  public float Chance = data.chance;
  public IntValue Stack = new(data.stack);
  public FloatValue Durability = new(data.durability);
  public string Position = data.pos;
  private Vector2i RolledPosition = Parse.Vector2Int(data.pos);
  public BoolValue Equipped = new(data.equipped);
  public IntValue Quality = new(data.quality);
  public IntValue Variant = new(data.variant);
  public LongValue CrafterID = new(data.crafterID);
  public StringValue CrafterName = new(data.crafterName);
  public Dictionary<string, StringValue> CustomData = data.customData?.ToDictionary(kvp => kvp.Key, kvp => new StringValue(kvp.Value)) ?? [];
  public IntValue WorldLevel = new(data.worldLevel);
  public BoolValue PickedUp = new(data.pickedUp);
  // Must know before writing is the prefab good, so it has to be rolled first.
  private string RolledPrefab = "";
  public bool RollPrefab()
  {
    RolledPrefab = Prefab.Get() ?? "";
    return RolledPrefab != "";
  }
  public bool RollChance() => Chance >= 1f || Random.value <= Chance;
  public bool Roll() => RollChance() && RollPrefab();
  public void Write(ZPackage pkg)
  {
    pkg.Write(RolledPrefab);
    pkg.Write(Stack.Get() ?? 1);
    pkg.Write(Durability.Get() ?? 100f);
    pkg.Write(RolledPosition);
    pkg.Write(Equipped.Get() > 0);
    pkg.Write(Quality.Get() ?? 1);
    pkg.Write(Variant.Get() ?? 1);
    pkg.Write(CrafterID.Get() ?? 0);
    pkg.Write(CrafterName.Get() ?? "");
    pkg.Write(CustomData?.Count ?? 0);
    foreach (var kvp in CustomData ?? [])
    {
      pkg.Write(kvp.Key);
      pkg.Write(kvp.Value.Get());
    }
    pkg.Write(WorldLevel.Get() ?? 1);
    pkg.Write(PickedUp.Get() > 0);
  }
}