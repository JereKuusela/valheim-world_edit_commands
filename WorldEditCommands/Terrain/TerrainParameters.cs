using System;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class TerrainParameters
{
  public Vector3 Position = Vector3.zero;
  public Vector3 Offset = Vector3.zero;
  public Vector3 Step = Vector3.zero;
  public float Size = 0f;
  public Range<float>? Radius;
  public Range<float>? Width;
  public Range<float>? Depth;
  public string[] IncludedIds = new string[0];
  public string[] ExcludedIds = new string[0];
  public float Angle = 0f;
  public float? Set;
  public bool Reset = false;
  public float? Delta;
  public float? Level;
  public float? Min;
  public float? Max;
  public float Smooth = 0;
  public float? Slope;
  public float SlopeAngle = 0f;
  public string Paint = "";
  public bool FixedPosition = false;
  public bool FixedAngle = false;
  public BlockCheck BlockCheck = BlockCheck.Off;
  public Range<float>? Within;

  public TerrainParameters(Terminal.ConsoleEventArgs args)
  {
    if (Player.m_localPlayer)
    {
      var precision = Mathf.PI / 4f;
      Position = Player.m_localPlayer.transform.position;
      Angle = precision * Mathf.Round(Player.m_localPlayer.transform.rotation.eulerAngles.y / 45f);
    }
    ParseArgs(args.Args);
  }

  private float ParseAngle(string value)
  {
    var angle = 0f;
    if (value == "n") angle = 0f;
    else if (value == "ne") angle = 45f;
    else if (value == "e") angle = 90f;
    else if (value == "se") angle = 135f;
    else if (value == "s") angle = 180f;
    else if (value == "sw") angle = 225;
    else if (value == "w") angle = 270f;
    else if (value == "nw") angle = 315;
    else angle = Parse.Float(value, 0f);
    angle *= Mathf.PI / 180f;
    return angle;
  }

  protected void ParseArgs(string[] args)
  {
    var playerPosition = Position;
    var useGroundHeight = true;
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      if (name == "from")
      {
        FixedPosition = true;
        useGroundHeight = Parse.Split(value).Length < 3;
        Position = Parse.VectorXZY(Parse.Split(value));
      }
    }
    if (useGroundHeight)
    {
      if (ZoneSystem.instance.IsZoneLoaded(Position))
        Position.y = ZoneSystem.instance.GetGroundHeight(Position);
      else
      {
        throw new InvalidOperationException("Unable to find the ground height. Use <color=yellow>from</color> with the y coordinate.");
      }
    }
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == "reset")
        Reset = true;
      if (name == "delta")
        Set = 0f;
      if (name == "level")
        Level = Position.y;
      if (name == "blockcheck")
        BlockCheck = BlockCheck.On;
      if (name == "circle")
        Radius = new(0f);
      if (name == "rect")
      {
        Width = new(0f);
        Depth = new(0f);
      }
      if (name == "slope")
      {
        Slope = 0f;
      }
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "id") IncludedIds = values;
      if (name == "ignore") ExcludedIds = values;
      if (name == "circle")
        Radius = Parse.FloatRange(value, 0f);
      if (name == "rect")
      {
        var size = Parse.ScaleRange(value);
        Width = new(size.Min.x, size.Max.x);
        Depth = new(size.Min.z, size.Max.z);
      }
      if (name == "paint")
        Paint = value;
      if (name == "angle")
      {
        FixedAngle = true;
        Angle = ParseAngle(value);
      }
      if (name == "delta")
        Set = Parse.Float(value, 0f);
      if (name == "min")
        Min = Parse.Float(value, float.MinValue);
      if (name == "max")
        Max = Parse.Float(value, float.MaxValue);
      if (name == "raise")
        Delta = Parse.Float(value, 0f);
      if (name == "lower")
        Delta = -Parse.Float(value, 0f);
      if (name == "smooth")
        Smooth = Parse.Float(value, 0f);
      if (name == "slope")
      {
        Slope = Parse.Float(values, 0, 0f);
        if (values.Length > 1) SlopeAngle = ParseAngle(values[1]);
      }
      if (name == "offset")
        Offset = Parse.VectorZXY(values);
      if (name == "within")
        Within = Parse.FloatRange(value);
      if (name == "level")
        Level = Parse.Float(value, Position.y);
      if (name == "step")
        Step = Parse.VectorZXY(values);
      if (name == "blockcheck")
      {
        if (value == "on") BlockCheck = BlockCheck.On;
        else if (value == "inverse") BlockCheck = BlockCheck.Inverse;
        else if (value == "off") BlockCheck = BlockCheck.Off;
        else throw new InvalidOperationException($"Invalid value {value} for blockcheck.");
      }
    }
    HandleTo(args);
    if (Radius != null && Depth != null)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");

    if (Radius == null && Depth == null)
      throw new InvalidOperationException($"<color=yellow>circle</color> or <color=yellow>rect</color> parameter must be used.");
    if (Radius != null) Size = Radius.Max;
    if (Depth != null && Width != null) Size = Mathf.Max(Depth.Max, Width.Max);
    if (Step != Vector3.zero)
    {
      var width = Size;
      var depth = Size;
      if (Width != null) width = Width.Max;
      if (Depth != null) depth = Depth.Max;
      Offset.x += Step.x * width * 2;
      Offset.z += Step.z * depth * 2;
      if (Slope.HasValue)
      {
        Offset.y = Slope.Value * (Step.z + Step.y);
        // Remove half to level at start of the slope (more intuitive for the users).
        if (Level.HasValue) Level += Offset.y - 0.5f * Slope.Value;
      }
    }
    if (Offset != Vector3.zero)
    {
      var original = Offset;
      Offset.x = Mathf.Cos(Angle) * original.x + Mathf.Sin(Angle) * original.z;
      Offset.z = Mathf.Cos(Angle) * original.z - Mathf.Sin(Angle) * original.x;
      Position += Offset;
    }
    // Circle doesn't use the angle so the slope needs both.
    if (Radius != null) SlopeAngle += Angle;
  }

  private void HandleTo(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      if (name == "to")
      {
        var to = Parse.VectorXZY(Parse.Split(value));

        if (Slope == 0 && Parse.Split(value).Length < 3)
        {
          if (ZoneSystem.instance.IsZoneLoaded(to))
            to.y = ZoneSystem.instance.GetGroundHeight(to);
          else
            throw new InvalidOperationException("Unable to find the ground height. Use <color=yellow>to</color> with the y coordinate.");
        }
        var distance = Utils.DistanceXZ(Position, to);
        if (Radius != null) Radius = new(distance / 2f);
        if (Width?.Max == 0f) Width = new(distance / 2f);
        if (Depth != null) Depth = new(distance / 2f);
        FixedAngle = true;
        Angle = Vector3.SignedAngle(Vector3.forward, Utils.DirectionXZ(to - Position), Vector3.up) * Mathf.PI / 180f;
        Position.x = (Position.x + to.x) / 2f;
        Position.z = (Position.z + to.z) / 2f;
        if (Slope.HasValue)
        {
          if (Slope == 0)
            Slope = to.y - Position.y;
          Position.y += Slope.Value / 2f;
        }
      }
    }
  }
}
