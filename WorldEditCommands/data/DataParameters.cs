using System;
using System.Collections.Generic;
namespace WorldEditCommands;
public class DataParameters() : BaseParameters(SupportedOperations)
{
  public static Dictionary<string, Type> SupportedOperations = new()
  {
    ["save"] = typeof(string),
    ["clear"] = typeof(bool),
    ["load"] = typeof(string[]),
    ["merge"] = typeof(string[]),
    ["set"] = typeof(string[]),
    ["remove"] = typeof(string[]),
    ["keep"] = typeof(string[]),
    ["print"] = typeof(bool),
    ["copy_raw"] = typeof(string),

  };
  protected override void ParseArg(string arg)
  {
  }
  protected override void ParseArg(string arg, string value)
  {
  }
}
