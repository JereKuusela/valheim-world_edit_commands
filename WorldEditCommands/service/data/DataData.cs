using System.ComponentModel;

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
}

