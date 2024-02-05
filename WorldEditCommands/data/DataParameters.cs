using System;
using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class DataParameters(Terminal.ConsoleEventArgs args) : BaseParameters(SupportedOperations, args)
{
  public static Dictionary<string, Type> SupportedOperations = new()
  {
    ["save"] = typeof(string),
    ["clear"] = typeof(bool),
    ["load"] = typeof(string),
    ["merge"] = typeof(string[]),
    ["set"] = typeof(string[]),
    ["remove"] = typeof(string[]),
    ["keep"] = typeof(string),
    ["print"] = typeof(bool),

  };
  protected override void ParseArg(string arg)
  {
  }
  protected override void ParseArg(string arg, string value)
  {
  }
}
