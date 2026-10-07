// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using System.ComponentModel;

namespace Data;

public class DataYaml
{
  [DefaultValue(null)]
  public string? name;
  [DefaultValue(null)]
  public string? position;
  [DefaultValue(null)]
  public string? rotation;
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
  public ItemYaml[]? items;
  [DefaultValue(null)]
  public ItemYaml? item;
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
  [DefaultValue(null)]
  public string? persistent;
  [DefaultValue(null)]
  public string? distant;
  [DefaultValue(null)]
  public string? priority;
}

public class ItemYaml
{
  public string pos = "";
  [DefaultValue(1f)]
  public float chance = 1f;
  [DefaultValue("")]
  public string prefab = "";
  public string? stack;
  public string? quality;
  public string? variant;
  public string? durability;
  public string? crafterID;
  public string? crafterName;
  public string? worldLevel;
  public string? equipped;
  public string? pickedUp;
  public string? cheated;
  public Dictionary<string, string>? customData;
}