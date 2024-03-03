using System;
using System.Collections.Generic;
using Data;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class DataRawCommand
{

  public DataRawCommand()
  {

    AutoComplete.Register("data_raw", (int index) => index == 0 ? DataLoading.DataKeys : ParameterInfo.None);
    Helper.Command("data_raw", "[name] - Copies data entry to clipboard as base64 encoded string.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing data entry name.");
      if (!DataLoading.Data.TryGetValue(args[1].GetStableHashCode(), out var zdo))
        throw new InvalidOperationException($"Data entry {args[1]} not found.");
      var pars = ParseCommand(args);
      var str = zdo.GetBase64(pars);
      if (str == "AAAAAA==")
        throw new InvalidOperationException($"Data entry {args[1]} is empty.");
      GUIUtility.systemCopyBuffer = str;
      Helper.AddMessage(args.Context, $"Data entry {args[1]} copied to clipboard.");
    });
  }

  private static Dictionary<string, string> ParseCommand(Terminal.ConsoleEventArgs args)
  {
    Dictionary<string, string> parameters = [];
    foreach (var arg in args.Args)
    {
      var split = arg.Split(['='], 2);
      if (split.Length < 2) continue;
      var name = split[0].ToLower();
      if (name == "par")
      {
        var kvp = Parse.Kvp(split[1]);
        if (kvp.Key == "") throw new InvalidOperationException($"Invalid data parameter {split[1]}.");
        parameters[kvp.Key] = kvp.Value;
      }
    }
    return parameters;
  }

}
