using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;

namespace Data;

public class DataData
{
  public string name = "";
  [DefaultValue("")]
  public string connection = "";
  [DefaultValue(null)]
  public string[]? bools = null;
  [DefaultValue(null)]
  public string[]? ints = null;
  [DefaultValue(null)]
  public string[]? hashes = null;
  [DefaultValue(null)]
  public string[]? floats = null;
  [DefaultValue(null)]
  public string[]? strings = null;
  [DefaultValue(null)]
  public string[]? longs = null;
  [DefaultValue(null)]
  public string[]? vecs = null;
  [DefaultValue(null)]
  public string[]? quats = null;
  [DefaultValue(null)]
  public string[]? bytes = null;
  [DefaultValue(null)]
  public ItemData[]? items = null;
}

public class ItemData
{
  public string pos = "0, 0";
  [DefaultValue("")]
  public string prefab = "";
  [DefaultValue(1)]
  public int stack = 1;
  [DefaultValue(1)]
  public int quality = 1;
  [DefaultValue(0)]
  public int variant = 0;
  [DefaultValue(0f)]
  public float durability = 0;
  [DefaultValue(0L)]
  public long crafterID = 0;
  [DefaultValue("")]
  public string crafterName = "";
  [DefaultValue(0)]
  public int worldLevel = 0;
  [DefaultValue(false)]
  public bool equipped = false;
  [DefaultValue(false)]
  public bool pickedUp = false;
  [DefaultValue(null)]
  public Dictionary<string, string>? customData = null;
}