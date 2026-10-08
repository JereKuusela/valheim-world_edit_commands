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
  [DefaultValue("")]
  public string pos = "";
  [DefaultValue(1f)]
  public float chance = 1f;
  [DefaultValue("")]
  public string prefab = "";
  [DefaultValue(null)]
  public string? stack;
  [DefaultValue(null)]
  public string? quality;
  [DefaultValue(null)]
  public string? variant;
  [DefaultValue(null)]
  public string? durability;
  [DefaultValue(null)]
  public string? crafterID;
  [DefaultValue(null)]
  public string? crafterName;
  [DefaultValue(null)]
  public string? worldLevel;
  [DefaultValue(null)]
  public string? equipped;
  [DefaultValue(null)]
  public string? pickedUp;
  [DefaultValue(null)]
  public string? cheated;
  [DefaultValue(null)]
  public Dictionary<string, string>? customData;
}