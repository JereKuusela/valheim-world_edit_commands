using System;
using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class ObjectParameters : SharedObjectParameters
{
  public ObjectSelection Selection = new();
  public Range<Vector3> Rotation = new(Vector3.zero);
  public Range<Vector3> Offset = new(Vector3.zero);
  public Range<float>? Fuel = null;
  public string Prefab = "";
  public string Origin = "player";
  public bool? Remove;
  public HashSet<string> Operations = [];
  public bool ResetRotation = false;
  public bool Respawn = false;
  public Item? Visual = null;
  public long Creator = 0;
  public string? StatusName;
  public Range<float>? StatusDuration;
  public Range<float>? StatusIntensity;

  public static HashSet<string> SupportedOperations = [
    "status",
    "health",
    "damage",
    "ammo",
    "ammotype",
    "durability",
    "stars",
    "tame",
    "wild",
    "level",
    "baby",
    "info",
    "data",
    "sleep",
    "remove",
    "visual",
    "model",
    "helmet",
    "left_hand",
    "right_hand",
    "shoulders",
    "legs",
    "utility",
    "move",
    "rotate",
    "scale",
    "chest",
    "fuel",
    "prefab",
    "respawn",
    "mirror",
    "creator",
    "copy",
    "field",
    "f",
    "components",
    "distant",
    "persist"
  ];

  public ObjectParameters(Terminal.ConsoleEventArgs args)
  {
    ParseArgs(args.Args);
  }

  protected override void ParseArgs(string[] args)
  {
    base.ParseArgs(args);
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (SupportedOperations.Contains(name))
      {
        if (name != "field" && name != "f" && Operations.Contains(name))
          throw new InvalidOperationException($"Operation {name} used multiple times.");
        Operations.Add(name);
      }
      if (name == "mirror") Selection.Center ??= Selection.From;
      Selection.ParseSelectionArg(name);
      if (split.Length < 2) continue;
      var value = split[1];
      var values = Parse.Split(value);
      Selection.ParseSelectionArg(name, value);
      if (name == "rotate")
      {
        if (value == "reset") ResetRotation = true;
        else Rotation = Parse.VectorYXZRange(value, Vector3.zero);
      }
      if (name == "move") Offset = Parse.VectorZXYRange(value, Vector3.zero);
      if (name == "prefab") Prefab = value;
      if (name == "origin") Origin = value.ToLower();
      if (name == "visual") Visual = new(value);
      if (name == "fuel") Fuel = Parse.FloatRange(value, 0f);
      if (name == "creator")
        Creator = Parse.Long(value, 0L);
      if (name == "status")
      {
        StatusName = values[0];
        StatusDuration = Parse.FloatRange(values, 1, 60);
        StatusIntensity = Parse.FloatRange(values, 2, 100);
      }
    }
    if (Operations.Contains("remove") && Operations.Count > 1)
      throw new InvalidOperationException("Remove can't be used with other operations.");
    if (Operations.Count == 0)
      throw new InvalidOperationException("Missing the operation.");
    Selection.ValidateSelection();
  }
}
