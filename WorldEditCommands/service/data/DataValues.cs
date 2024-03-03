
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace Data;

public class DataValue
{
  // Different function name because string would be ambiguous.
  public static IIntValue Simple(int value) => new SimpleIntValue(value);
  public static IStringValue Simple(string value) => new SimpleStringValue(value);

  public static IIntValue Int(ZPackage pkg) => new SimpleIntValue(pkg.ReadInt());
  public static IIntValue Int(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEmpty(values);
    if (!hasParameters && split.Length == 1 && int.TryParse(split[0], out var result))
      return new SimpleIntValue(result);
    return new IntValue(split);
  }

  public static IFloatValue Float(ZPackage pkg) => new SimpleFloatValue(pkg.ReadSingle());
  public static IFloatValue Float(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEmpty(values);
    if (!hasParameters && split.Length == 1 && Parse.TryFloat(split[0], out var result))
      return new SimpleFloatValue(result);
    return new FloatValue(split);
  }

  public static ILongValue Long(ZPackage pkg) => new SimpleLongValue(pkg.ReadLong());
  public static ILongValue Long(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEmpty(values);
    if (!hasParameters && split.Length == 1 && long.TryParse(split[0], out var result))
      return new SimpleLongValue(result);
    return new LongValue(split);
  }

  public static IStringValue String(ZPackage pkg) => new SimpleStringValue(pkg.ReadString());
  public static IStringValue String(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEscape(values);
    if (!hasParameters && split.Length == 1)
      return new SimpleStringValue(split[0]);
    return new StringValue(split);
  }
  public static IBoolValue Bool(bool value) => new SimpleBoolValue(value);
  public static IBoolValue Bool(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEmpty(values);
    if (!hasParameters && split.Length == 1 && bool.TryParse(split[0], out var result))
      return new SimpleBoolValue(result);
    return new BoolValue(split);
  }


  public static IHashValue Hash(string value) => new SimpleHashValue(value);
  public static IHashValue Hash(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEmpty(values);
    if (!hasParameters && split.Length == 1)
      return new SimpleHashValue(split[0]);
    return new HashValue(split);
  }
  public static IVector3Value Vector3(ZPackage pkg) => new SimpleVector3Value(pkg.ReadVector3());
  public static IVector3Value Vector3(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEscape(values);
    var parsed = Parse.VectorXZYNull(values);
    if (!hasParameters && parsed.HasValue)
      return new SimpleVector3Value(parsed.Value);
    return new Vector3Value(split);
  }
  public static IQuaternionValue Quaternion(ZPackage pkg) => new SimpleQuaternionValue(pkg.ReadQuaternion());
  public static IQuaternionValue Quaternion(string values, HashSet<string> requiredParameters)
  {
    var hasParameters = CheckParameters(values, requiredParameters);
    var split = Parse.SplitWithEscape(values);
    var parsed = Parse.AngleYXZNull(values);
    if (!hasParameters && parsed.HasValue)
      return new SimpleQuaternionValue(parsed.Value);
    return new QuaternionValue(split);
  }

  private static bool CheckParameters(string value, HashSet<string> requiredParameters)
  {
    // Parameter format is <par>.
    if (!value.Contains("<") || !value.Contains(">")) return false;
    var split = value.Split('<', '>');
    for (var i = 1; i < split.Length; i += 2)
      requiredParameters.Add(split[i]);
    return split.Length > 1;
  }

}


public class AnyValue(string[] values)
{
  protected readonly string[] Values = values;

  private string? RollValue()
  {
    if (Values.Length == 1)
      return Values[0];
    return Values[Random.Range(0, Values.Length)];
  }
  protected string? GetValue(Dictionary<string, string> pars)
  {
    var value = RollValue();
    if (value == null || value == "<none>")
      return null;
    return ReplaceParameters(value, pars);
  }
  protected string? GetValue()
  {
    var value = RollValue();
    return value == null || value == "<none>" ? null : value;
  }
  protected string[] GetAllValues(Dictionary<string, string> pars)
  {
    return Values.Select(v => ReplaceParameters(v, pars)).Where(v => v != null && v != "<none").ToArray();
  }

  protected string ReplaceParameters(string value, Dictionary<string, string> pars)
  {
    foreach (var kvp in pars)
      value = value.Replace(kvp.Key, kvp.Value);
    return value;
  }
}
public class ItemValue(ItemData data, HashSet<string> requiredParameters)
{
  public static string LoadItems(Dictionary<string, string> pars, ItemValue[] items, Vector2i? size, int amount)
  {
    ZPackage pkg = new();
    pkg.Write(106);
    items = Generate(pars, items, size ?? new(0, 0), amount);
    pkg.Write(items.Length);
    foreach (var item in items)
      item.Write(pars, pkg);
    return pkg.GetBase64();
  }
  public static ItemValue[] Generate(Dictionary<string, string> pars, ItemValue[] data, Vector2i size, int amount)
  {
    var fixedPos = data.Where(item => item.Position != "").ToList();
    var randomPos = data.Where(item => item.Position == "").ToList();
    Dictionary<Vector2i, ItemValue> inventory = [];
    foreach (var item in fixedPos)
    {
      if (!item.Roll(pars)) continue;
      inventory[item.RolledPosition] = item;
    }
    if (amount == 0)
      GenerateEach(pars, inventory, size, randomPos);
    else
      GenerateAmount(pars, inventory, size, randomPos, amount);
    return [.. inventory.Values];
  }
  private static void GenerateEach(Dictionary<string, string> pars, Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items)
  {
    foreach (var item in items)
    {
      if (!item.Roll(pars)) continue;
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      item.RolledPosition = slot.Value;
      inventory[slot.Value] = item;
    }
  }
  private static void GenerateAmount(Dictionary<string, string> pars, Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items, int amount)
  {
    var maxWeight = items.Sum(item => item.Chance);
    for (var i = 0; i < amount && items.Count > 0; ++i)
    {
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      var item = RollItem(items, maxWeight);
      item.RolledPosition = slot.Value;
      if (item.RollPrefab(pars))
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
  // Prefab is saved as string, so hash can't be used.
  public IStringValue Prefab = DataValue.String(data.prefab, requiredParameters);
  public float Chance = data.chance;
  public IIntValue Stack = DataValue.Int(data.stack, requiredParameters);
  public IFloatValue Durability = DataValue.Float(data.durability, requiredParameters);
  public string Position = data.pos;
  private Vector2i RolledPosition = Parse.Vector2Int(data.pos);
  public IBoolValue Equipped = DataValue.Bool(data.equipped, requiredParameters);
  public IIntValue Quality = DataValue.Int(data.quality, requiredParameters);
  public IIntValue Variant = DataValue.Int(data.variant, requiredParameters);
  public ILongValue CrafterID = DataValue.Long(data.crafterID, requiredParameters);
  public IStringValue CrafterName = DataValue.String(data.crafterName, requiredParameters);
  public Dictionary<string, IStringValue> CustomData = data.customData?.ToDictionary(kvp => kvp.Key, kvp => DataValue.String(kvp.Value, requiredParameters)) ?? [];
  public IIntValue WorldLevel = DataValue.Int(data.worldLevel, requiredParameters);
  public IBoolValue PickedUp = DataValue.Bool(data.pickedUp, requiredParameters);
  // Must know before writing is the prefab good, so it has to be rolled first.
  private string RolledPrefab = "";
  public bool RollPrefab(Dictionary<string, string> pars)
  {
    RolledPrefab = Prefab.Get(pars) ?? "";
    return RolledPrefab != "";
  }
  public bool RollChance() => Chance >= 1f || Random.value <= Chance;
  public bool Roll(Dictionary<string, string> pars) => RollChance() && RollPrefab(pars);
  public void Write(Dictionary<string, string> pars, ZPackage pkg)
  {
    pkg.Write(RolledPrefab);
    pkg.Write(Stack.Get(pars) ?? 1);
    pkg.Write(Durability.Get(pars) ?? 100f);
    pkg.Write(RolledPosition);
    pkg.Write(Equipped.Get(pars) ?? 0);
    pkg.Write(Quality.Get(pars) ?? 1);
    pkg.Write(Variant.Get(pars) ?? 1);
    pkg.Write(CrafterID.Get(pars) ?? 0);
    pkg.Write(CrafterName.Get(pars) ?? "");
    pkg.Write(CustomData?.Count ?? 0);
    foreach (var kvp in CustomData ?? [])
    {
      pkg.Write(kvp.Key);
      pkg.Write(kvp.Value.Get(pars));
    }
    pkg.Write(WorldLevel.Get(pars) ?? 1);
    pkg.Write(PickedUp.Get(pars) ?? 0);
  }
}
