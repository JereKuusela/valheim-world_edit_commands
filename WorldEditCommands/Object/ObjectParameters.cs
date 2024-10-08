using System;
using System.Collections.Generic;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;
public class ObjectParameters : SharedObjectParameters
{
  public Range<Vector3> Rotation = new(Vector3.zero);
  public Range<Vector3> Offset = new(Vector3.zero);
  public Vector3 From;
  public Vector3? Center = null;
  public Range<float>? Fuel = null;
  public string[] IncludedIds = [];
  public string[] ExcludedIds = [];
  public string Prefab = "";
  public string Origin = "player";
  public bool? Remove;
  public HashSet<string> Operations = [];
  public bool ResetRotation = false;
  public bool Respawn = false;
  public Item? Visual = null;
  public float Angle = 0f;
  public long Creator = 0;
  public Range<float>? Width;
  public Range<float>? Depth;
  public float Height = 0f;
  public float Chance = 1f;
  public string Match = "";
  public string Unmatch = "";
  public bool Connect;
  public HashSet<string> Components = [];
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
    "copy"
  ];

  public ObjectParameters(Terminal.ConsoleEventArgs args)
  {
    if (Player.m_localPlayer)
    {
      From = Player.m_localPlayer.transform.position;
    }
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
      if (name == "center" || name == "mirror") Center = From;
      if (name == "connect") Connect = true;
      if (split.Length < 2) continue;
      var value = split[1];
      var values = Parse.Split(value);
      if (name == "rotate")
      {
        if (value == "reset") ResetRotation = true;
        else Rotation = Parse.VectorYXZRange(value, Vector3.zero);
      }
      if (name == "center" || name == "from") Center = Parse.VectorXZY(values);
      if (name == "move") Offset = Parse.VectorZXYRange(value, Vector3.zero);
      if (name == "id") IncludedIds = values;
      if (name == "ignore") ExcludedIds = values;
      if (name == "prefab") Prefab = value;
      if (name == "origin") Origin = value.ToLower();
      if (name == "visual") Visual = new(value);
      if (name == "fuel") Fuel = Parse.FloatRange(value, 0f);
      if (name == "chance") Chance = Parse.Float(value, 1f);
      if (name == "type") AddComponents(values);
      if (name == "match") Match = value;
      if (name == "unmatch") Unmatch = value;
      if (name == "rect")
      {
        var size = Parse.ScaleRange(value);
        Width = new(size.Min.x, size.Max.x);
        Depth = new(size.Min.z, size.Max.z);
      }
      if (name == "height")
        Height = Parse.Float(value, 0f);
      if (name == "creator")
        Creator = Parse.Long(value, 0L);
      if (name == "angle")
        Angle = Parse.Float(value, 0f) * Mathf.PI / 180f;
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
    if (Radius != null && Depth != null)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
    if (Radius != null && Connect)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>connect</color> parameters can't be used together.");
    if (Depth != null && Connect)
      throw new InvalidOperationException($"<color=yellow>connect</color> and <color=yellow>rect</color> parameters can't be used together.");
  }

  private void AddComponents(string[] values)
  {
    foreach (var value in values)
    {
      var lower = value.ToLowerInvariant();
      if (lower == "structure")
        Components.Add("WearNTear");
      else if (lower == "creature")
        Components.Add("Humanoid");
      else
        Components.Add(value);
    }
  }
}
