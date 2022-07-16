using System;
using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class ObjectParameters : SharedObjectParameters {
  public Range<Vector3> Rotation = new(Vector3.zero);
  public Range<Vector3> Offset = new(Vector3.zero);
  public Vector3 From;
  public Vector3? Center = null;
  public Range<float>? Fuel = null;
  public string Id = "";
  public string Prefab = "";
  public string Origin = "player";
  public HashSet<string> Operations = new();
  public bool ResetRotation = false;
  public bool Respawn = false;
  public Item? Visual = null;
  public float Angle = 0f;
  public float? Width;
  public float? Depth;
  public float Height = 0f;

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
    "prefab",
    "respawn",
    "guide",
    "mirror"
  };

  public ObjectParameters(Terminal.ConsoleEventArgs args) {
    if (Player.m_localPlayer) {
      From = Player.m_localPlayer.transform.position;
    }
    ParseArgs(args.Args);
  }

  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Position = From,
    FixedPosition = Center != null
  };

  protected override void ParseArgs(string[] args) {
    base.ParseArgs(args);
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (SupportedOperations.Contains(name)) {
        if (Operations.Contains(name))
          throw new InvalidOperationException($"Operation {name} used multiple times.");
        Operations.Add(name);
      }
      if (name == "center" || name == "mirror") Center = From;
      if (name == "respawn") Respawn = true;
      if (split.Length < 2) continue;
      var value = split[1];
      var values = Parse.Split(value);
      if (name == "rotate") {
        if (value == "reset") ResetRotation = true;
        else Rotation = Parse.TryVectorYXZRange(value, Vector3.zero);
      }
      if (name == "center" || name == "from") Center = Parse.TryVectorXZY(values);
      if (name == "move") Offset = Parse.TryVectorZXYRange(value, Vector3.zero);
      if (name == "id") Id = value;
      if (name == "prefab") Prefab = value;
      if (name == "origin") Origin = value.ToLower();
      if (name == "visual") Visual = new(value);
      if (name == "fuel") Fuel = Parse.TryFloatRange(value, 0f);
      if (name == "rect") {
        var size = Parse.TryScale(values);
        Width = size.x;
        Depth = size.z;
      }
      if (name == "height")
        Height = Parse.TryFloat(value, 0f);
      if (name == "angle")
        Angle = Parse.TryFloat(value, 0f) * Mathf.PI / 180f;
    }
    if (Operations.Contains("remove") && Operations.Count > 1)
      throw new InvalidOperationException("Remove can't be used with other operations.");
    if (Operations.Count == 0)
      throw new InvalidOperationException("Missing the operation.");
    if (Operations.Contains("remove") && Id == "")
      throw new InvalidOperationException("Remove can't be used without id.");
    if (Id == "") Id = "*";
    if (Radius.HasValue && Depth.HasValue)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
  }
}
