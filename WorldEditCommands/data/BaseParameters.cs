using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
namespace WorldEditCommands;

public abstract class BaseParameters(Dictionary<string, Type> supportedOperations) : ObjectSelection
{
  private readonly Dictionary<string, Type> SupportedOperations = supportedOperations;
  private Terminal terminal = null!;

  public void ParseCommand(Terminal.ConsoleEventArgs args)
  {
    terminal = args.Context;
    ParseArgs(args.Args);
  }
  public ZNetView[] GetObjects() => GetObjects(terminal);

  public Dictionary<string, object?> Operations = [];

  abstract protected void ParseArg(string arg);
  abstract protected void ParseArg(string arg, string value);
  protected virtual void ParseArgs(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split(['='], 2);
      var name = split[0].ToLower();
      ParseArgInternal(name);
      ParseSelectionArg(name);
      ParseArg(name);
      if (split.Length < 2) continue;
      var value = split[1].Trim();
      ParseArgInternal(name, value);
      ParseSelectionArg(name, value);
      ParseArg(name, value);
    }
    if (Operations.Count == 0)
      throw new InvalidOperationException("Missing the operation.");
    ValidateSelection();
  }
  private void ParseArgInternal(string name)
  {
    if (SupportedOperations.TryGetValue(name, out var type))
    {
      if (Operations.ContainsKey(name) && type != typeof(string[]))
        throw new InvalidOperationException($"Operation {name} used multiple times.");
      if (type == typeof(string[]))
      {
        if (!Operations.ContainsKey(name))
          Operations.Add(name, new string[0]);
      }
      else
        Operations.Add(name, null);
    }
  }
  private void ParseArgInternal(string name, string value)
  {
    if (SupportedOperations.TryGetValue(name, out var type))
    {
      if (type == typeof(int))
        Operations[name] = Parse.Int(value);
      else if (type == typeof(float))
        Operations[name] = Parse.Float(value);
      else if (type == typeof(bool))
        Operations[name] = Parse.Boolean(value);
      else if (type == typeof(string[]))
        Operations[name] = (Operations[name] as string[]).Append(value).ToArray();
      else
        Operations[name] = value;
    }
  }
}
