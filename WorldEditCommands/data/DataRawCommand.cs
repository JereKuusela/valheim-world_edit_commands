using System;
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
      var str = zdo.GetBase64();
      if (str == "AAAAAA==")
        throw new InvalidOperationException($"Data entry {args[1]} is empty.");
      GUIUtility.systemCopyBuffer = str;
      Helper.AddMessage(args.Context, $"Data entry {args[1]} copied to clipboard.");
    });
  }
}
