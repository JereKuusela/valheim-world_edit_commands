using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class TerrainParameters {
  public Vector3 Position = Vector3.zero;
  public Vector3 Offset = Vector3.zero;
  public Vector3 Step = Vector3.zero;
  public float Size = 0f;
  public float? Diameter = null;
  public float? Width = null;
  public float? Depth = null;
  public float Angle = 0f;
  public float? Set = null;
  public float? Delta = null;
  public float? Level = null;
  public float Smooth = 0;
  public float? Slope = null;
  public float SlopeAngle = 0f;
  public string Paint = "";
  public BlockCheck BlockCheck = BlockCheck.Off;

  private float ParseAngle(string value) {
    var angle = 0f;
    if (value == "n") angle = 0f;
    else if (value == "ne") angle = 45f;
    else if (value == "e") angle = 90f;
    else if (value == "se") angle = 135f;
    else if (value == "s") angle = 180f;
    else if (value == "sw") angle = 225;
    else if (value == "w") angle = 270f;
    else if (value == "nw") angle = 315;
    else angle = Parse.TryFloat(value, 0f);
    angle *= Mathf.PI / 180f;
    return angle;
  }

  public bool ParseArgs(Terminal.ConsoleEventArgs args, Terminal terminal) {
    var playerPosition = Position;
    var useGroundHeight = true;
    foreach (var arg in args.Args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      if (name == "refpos") {
        useGroundHeight = Parse.Split(value).Length < 3;
        Position = Parse.TryVectorXZY(Parse.Split(value));
      }
    }
    if (useGroundHeight) {
      if (ZoneSystem.instance.IsZoneLoaded(Position))
        Position.y = ZoneSystem.instance.GetGroundHeight(Position);
      else {
        Helper.AddMessage(terminal, "Error: Unable to find the ground height. Use <color=yellow>refPos</color> with the y coordinate.");
        return false;
      }
    }
    foreach (var arg in args.Args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == "reset")
        Set = 0f;
      if (name == "level")
        Level = Position.y;
      if (name == "blockcheck")
        BlockCheck = BlockCheck.On;
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "circle")
        Diameter = Parse.TryFloat(value, 0f);
      if (name == "rect") {
        var size = Parse.TryScale(values);
        Width = size.x;
        Depth = size.z;
      }
      if (name == "paint")
        Paint = value;
      if (name == "angle")
        Angle = ParseAngle(value);
      if (name == "raise")
        Delta = Parse.TryFloat(value, 0f);
      if (name == "lower")
        Delta = -Parse.TryFloat(value, 0f);
      if (name == "smooth")
        Smooth = Parse.TryFloat(value, 0f);
      if (name == "slope") {
        Slope = Parse.TryFloat(values, 0, 0f);
        if (values.Length > 1) SlopeAngle = ParseAngle(values[1]);
      }
      if (name == "offset")
        Offset = Parse.TryVectorZXY(values);
      if (name == "level")
        Level = Parse.TryFloat(value, Position.y);
      if (name == "step")
        Step = Parse.TryVectorZXY(values);
      if (name == "blockcheck") {
        if (value == "on") BlockCheck = BlockCheck.On;
        else if (value == "inverse") BlockCheck = BlockCheck.Inverse;
        else if (value == "off") BlockCheck = BlockCheck.Off;
        else {
          Helper.AddMessage(terminal, $"Error: Invalid value {value} for blockcheck.");
          return false;
        }
      }
    }
    if (Diameter.HasValue && Depth.HasValue) {
      Helper.AddMessage(terminal, $"Error: circle and rect parameters can't be used together.");
      return false;
    }
    if (!Diameter.HasValue && !Depth.HasValue) {
      Helper.AddMessage(terminal, $"Error: circle or rect parameter must be used.");
      return false;
    }
    if (Diameter.HasValue) Size = Diameter.Value / 2f;
    if (Depth.HasValue && Width.HasValue) Size = Mathf.Max(Depth.Value, Width.Value) / 2;
    if (Step != Vector3.zero) {
      var width = Size;
      var depth = Size;
      if (Width.HasValue) width = Width.Value / 2;
      if (Depth.HasValue) depth = Depth.Value / 2;
      Offset.x += Step.x * width * 2;
      Offset.z += Step.z * depth * 2;
      if (Slope.HasValue) {
        Offset.y = Slope.Value * (Step.z + Step.y);
        // Remove half to level at start of the slope (more intuitive for the users).
        if (Level.HasValue) Level += Offset.y - 0.5f * Slope.Value;
      }
    }
    if (Offset != Vector3.zero) {
      var original = Offset;
      Offset.x = Mathf.Cos(Angle) * original.x + Mathf.Sin(Angle) * original.z;
      Offset.z = Mathf.Cos(Angle) * original.z - Mathf.Sin(Angle) * original.x;
      Position += Offset;
    }
    // Circle doesn't use the angle so the slope needs both.
    if (Diameter.HasValue) SlopeAngle += Angle;
    return true;
  }
}
