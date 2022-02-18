using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {

  public class TerrainParameters {
    public float Radius = 1f;
    public float? Set = null;
    public float? Delta = null;
    public float? Level = null;
    public float Smooth = 0;
    public string Paint = "";
    public bool Square = false;
    public BlockCheck BlockCheck = BlockCheck.Off;

    public bool ParseArgs(Terminal.ConsoleEventArgs args, Terminal terminal, float height) {
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        var name = split[0].ToLower();
        if (name == "reset")
          Set = 0f;
        if (name == "square")
          Square = true;
        if (name == "level")
          Level = height;
        if (name == "blockcheck")
          BlockCheck = BlockCheck.On;
        if (split.Length < 2) continue;
        var value = split[1].ToLower();
        if (name == "radius")
          Radius = Mathf.Min(64f, Parse.TryFloat(value, 0f));
        if (name == "paint")
          Paint = value;
        if (name == "raise")
          Delta = Parse.TryFloat(value, 0f);
        if (name == "lower")
          Delta = -Parse.TryFloat(value, 0f);
        if (name == "smooth")
          Smooth = Parse.TryFloat(value, 0f);
        if (name == "level")
          Level = Parse.TryFloat(value, height);
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
      return true;
    }
  }

}
