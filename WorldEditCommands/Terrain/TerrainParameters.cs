using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {

  public class TerrainParameters {
    public Vector3 Position = Vector3.zero;
    public Vector3 Offset = Vector3.zero;
    public Vector3 Step = Vector3.zero;
    public float Radius = 1f;
    public float Angle = 0f;
    public float? Set = null;
    public float? Delta = null;
    public float? Level = null;
    public float Smooth = 0;
    public float? Slope = null;
    public string Paint = "";
    public bool Square = false;
    public BlockCheck BlockCheck = BlockCheck.Off;

    public bool ParseArgs(Terminal.ConsoleEventArgs args, Terminal terminal) {
      var playerPosition = Position;
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        var name = split[0].ToLower();
        if (split.Length < 2) continue;
        var value = split[1].ToLower();
        if (name == "refpos")
          Position = Parse.TryVectorXZY(Parse.Split(value));
      }
      Position.y = ZoneSystem.instance.GetGroundHeight(Position);
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        var name = split[0].ToLower();
        if (name == "reset")
          Set = 0f;
        if (name == "square")
          Square = true;
        if (name == "level")
          Level = Offset.y;
        if (name == "blockcheck")
          BlockCheck = BlockCheck.On;
        if (split.Length < 2) continue;
        var value = split[1].ToLower();
        if (name == "radius")
          Radius = Parse.TryFloat(value, 0f);
        if (name == "paint")
          Paint = value;
        if (name == "angle") {
          if (value == "n") Angle = 0f;
          else if (value == "ne") Angle = 45f;
          else if (value == "e") Angle = 90f;
          else if (value == "se") Angle = 135f;
          else if (value == "s") Angle = 180f;
          else if (value == "sw") Angle = 225;
          else if (value == "w") Angle = 270f;
          else if (value == "nw") Angle = 315;
          else Angle = Parse.TryFloat(value, 0f);
        }
        if (name == "raise")
          Delta = Parse.TryFloat(value, 0f);
        if (name == "lower")
          Delta = -Parse.TryFloat(value, 0f);
        if (name == "smooth")
          Smooth = Parse.TryFloat(value, 0f);
        if (name == "slope")
          Slope = Parse.TryFloat(value, 0f);
        if (name == "offset")
          Offset = Parse.TryVectorXZY(Parse.Split(value));
        if (name == "level")
          Level = Parse.TryFloat(value, Offset.y);
        if (name == "step")
          Step = Parse.TryVectorXZY(Parse.Split(value));
        if (name == "refPos")
          Position = Parse.TryVectorXZY(Parse.Split(value), Position);
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
      if (Step != Vector3.zero) {
        Offset = Step * Radius * 2;
        if (Slope.HasValue)
          Offset.y = Slope.Value * (Step.x + Step.y);
      }
      if (Offset != Vector3.zero) {
        var original = Offset;
        Offset.x = Mathf.Sin(Angle * Mathf.PI / 180f) * original.x + Mathf.Cos(Angle * Mathf.PI / 180f) * original.z;
        Offset.z = Mathf.Cos(Angle * Mathf.PI / 180f) * original.x + Mathf.Sin(Angle * Mathf.PI / 180f) * original.z;
        Position += Offset;
      }
      var maxRadius = 64f - Utils.LengthXZ(Position - playerPosition);
      if (maxRadius < 0) {
        Helper.AddMessage(terminal, $"Error: The edited terrain is too far.");
        return false;
      }
      if (maxRadius < Radius) {
        Helper.AddMessage(terminal, $"Note: Radius lowered to {maxRadius.ToString("F1")} to stay within the editing limits.");
      }
      Radius = Mathf.Min(maxRadius, Radius);
      return true;
    }
  }

}
