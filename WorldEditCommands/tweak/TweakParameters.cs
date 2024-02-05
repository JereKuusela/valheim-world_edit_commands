using System;
using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class TweakParameters(Dictionary<string, Type> supportedOperations, Terminal.ConsoleEventArgs args) : BaseParameters(supportedOperations, args)
{
  public long Creator = 0;
  public bool Force;

  protected override void ParseArg(string arg)
  {
    if (arg == "force") Force = true;
  }
  protected override void ParseArg(string arg, string value)
  {
    if (arg == "creator") Creator = Parse.Long(value, 0L);
  }
}
