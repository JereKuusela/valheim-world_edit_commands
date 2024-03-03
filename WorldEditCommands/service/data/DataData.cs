using System.Collections.Generic;
using System.ComponentModel;

namespace Data;

public class DataData
{
  [DefaultValue(null)]
  public string? name;
  [DefaultValue(null)]
  public string? connection;
  [DefaultValue(null)]
  public string[]? bools;
  [DefaultValue(null)]
  public string[]? ints;
  [DefaultValue(null)]
  public string[]? hashes;
  [DefaultValue(null)]
  public string[]? floats;
  [DefaultValue(null)]
  public string[]? strings;
  [DefaultValue(null)]
  public string[]? longs;
  [DefaultValue(null)]
  public string[]? vecs;
  [DefaultValue(null)]
  public string[]? quats;
  [DefaultValue(null)]
  public string[]? bytes;
  [DefaultValue(null)]
  public ItemData[]? items;
  [DefaultValue(null)]
  public string? containerSize;
  [DefaultValue(null)]
  public string? itemAmount;

  [DefaultValue(null)]
  public string? valueGroup;
  [DefaultValue(null)]
  public string? value;
  [DefaultValue(null)]
  public string[]? values;
}

public class ItemData
{
  public string pos = "";
  [DefaultValue(1f)]
  public float chance = 1f;
  [DefaultValue("")]
  public string prefab = "";
  [DefaultValue("1")]
  public string stack = "1";
  [DefaultValue("1")]
  public string quality = "1";
  [DefaultValue("0")]
  public string variant = "0";
  [DefaultValue("0")]
  public string durability = "0";
  [DefaultValue("0")]
  public string crafterID = "0";
  [DefaultValue("")]
  public string crafterName = "";
  [DefaultValue("0")]
  public string worldLevel = "0";
  [DefaultValue("false")]
  public string equipped = "false";
  [DefaultValue("false")]
  public string pickedUp = "false";
  [DefaultValue(null)]
  public Dictionary<string, string>? customData;
}