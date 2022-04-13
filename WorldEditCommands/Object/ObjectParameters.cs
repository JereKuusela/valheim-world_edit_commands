using System;
using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class ObjectParameters : SharedObjectParameters {
  public Range<Vector3> Rotation = new(Vector3.zero);
  public Range<Vector3> Offset = new(Vector3.zero);
  public Range<float>? Fuel = null;
  public string Id = "";
  public string Prefab = "";
  public string Origin = "player";
  public HashSet<string> Operations = new();
  public bool ResetRotation = false;
  public Item? Visual = null;

  public static HashSet<string> SupportedOperations = new() {
    "health",
    "stars",
    "tame",
    "wild",
    "level",
    "baby",
    "info",
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
    "prefab"
  };
  public override bool ParseArgs(string[] args, Terminal terminal) {
    if (!base.ParseArgs(args, terminal)) return false;
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (SupportedOperations.Contains(name)) {
        if (Operations.Contains(name)) {
          Helper.AddMessage(terminal, $"Error: Operation {name} used multiple times.");
          return false;
        }
        Operations.Add(name);
      }
      if (split.Length < 2) continue;
      var value = split[1];
      if (name == "rotate") {
        if (value == "reset") ResetRotation = true;
        else Rotation = Parse.TryVectorYXZRange(value, Vector3.zero);
      }
      if (name == "move") Offset = Parse.TryVectorXZYRange(value, Vector3.zero);
      if (name == "id") Id = value;
      if (name == "prefab") Prefab = value;
      if (name == "origin") Origin = value.ToLower();
      if (name == "visual") Visual = new(value);
      if (name == "fuel") Fuel = Parse.TryFloatRange(value, 0f);
    }
    Radius = Math.Min(Radius, 100f);
    if (Operations.Contains("remove") && Operations.Count > 1) {
      Helper.AddMessage(terminal, "Error: Remove can't be used with other operations.");
      return false;
    }
    if (Operations.Contains("remove") && Id == "") {
      Helper.AddMessage(terminal, "Error: Remove can't be used without id.");
      return false;
    }
    if (Id == "") Id = "*";
    return true;
  }
}
