using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
namespace WorldEditCommands;
public class TerrainAutoComplete
{
  private static List<string> BlockCheck = new() {
    "inverse",
    "off",
    "on"
  };
  public List<string> NamedParameters = new() {
    "lower",
    "level",
    "raise",
    "reset",
    "paint",
    "delta",
    "blockcheck",
    "rect",
    "circle",
    "smooth",
    "slope",
    "angle",
    "offset",
    "step",
    "from",
    "to",
    "min",
    "max",
    "within"
  };
  public TerrainAutoComplete()
  {
    NamedParameters.Sort();
    var paints = TerrainCommand.Paints.Keys.ToList();
    paints.Sort();
    AutoComplete.Register(TerrainCommand.Name, (int index) => NamedParameters, new() {
      {
        "reset",
        (int index) => ParameterInfo.Flag("Reset")
      },
      {
        "void",
        (int index) => ParameterInfo.Flag("Void", "Removes all terrain.")
      },
      {
        "blockcheck",
        (int index) => index == 0 ? ParameterInfo.Create("blockcheck=<color=yellow>inverse</color>/<color=yellow>off</color>/<color=yellow>on</color>", "When <color=yellow>on</color>, excludes terrain under structures. When <color=yellow>inverse</color>, only includes terrain under structures.") : ParameterInfo.None
      },
      {
        "rect",
        (int index) => {
          if (index == 0) return ParameterInfo.Create("rect=<color=yellow>size</color> or rect=<color=yellow>width</color>,depth", "Size of the included terrain (rectangle).");
          if (index == 1) return ParameterInfo.Create("rect=width,<color=yellow>depth</color>", "Size of the included terrain (rectangle).");
          return ParameterInfo.None;
        }
      },
      {
        "min",
        (int index) => index == 0 ? ParameterInfo.Create("min", "altitude", "Raises terrain below the given altitude to the altitude.") : ParameterInfo.None
      },
      {
        "max",
        (int index) => index == 0 ? ParameterInfo.Create("max", "altitude", "Lowers terrain above the given altitude to the altitude.") : ParameterInfo.None
      },
      {
        "angle",
        (int index) => index == 0 ? ParameterInfo.Create("angle=<color=yellow>number</color>/<color=yellow>n</color>/<color=yellow>ne</color>/<color=yellow>e</color>/<color=yellow>se</color>/<color=yellow>s</color>/<color=yellow>sw</color>/<color=yellow>w</color>/<color=yellow>nw</color> (from 0 to 360)", "Direction of the shape.") : ParameterInfo.None
      },
      {
        "circle",
        (int index) => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color>", "Radius of the included terrain.") : ParameterInfo.None
      },
      {
        "offset",
        (int index) => ParameterInfo.FRU("offset", "Position of the center", index)
      },
      {
        "from",
        (int index) => ParameterInfo.XZY("from", "Overrides the player position", index)
      },
      {
        "to",
        (int index) => ParameterInfo.XZY("to", "Determines the angle and circle/rect distance.", index)
      },
      {
        "step",
        (int index) => ParameterInfo.FRU("step", "Offset based on radius", index)
      },
      {
        "smooth",
        (int index) => index == 0 ? ParameterInfo.Create("smooth=<color=yellow>number</color> (from 0.0 to 1.0)", "Higher values smoothen the effect near edges.") : ParameterInfo.None
      },
      {
        "slope",
        (int index) => {
          if (index == 0) return ParameterInfo.Create("slope=<color=yellow>meters</color>,angle=0", "Creates a slope with altitude difference of a given amount.");
          if (index == 1) return ParameterInfo.Create("slope=meters,<color=yellow>angle</color>", "Changes the slope direction (added to the general angle).");
          return ParameterInfo.None;
        }
      },
      {
        "delta",
        (int index) => index == 0 ? ParameterInfo.Create("delta", "meters", "Sets the terrain elevation difference from the original.") : ParameterInfo.None
      },
      {
        "raise",
        (int index) => index == 0 ? ParameterInfo.Create("raise", "meters", "Raises the terrain by a given amount.") : ParameterInfo.None
      },
      {
        "lower",
        (int index) => index == 0 ? ParameterInfo.Create("lower", "meters", "Lowers the terrain by a given amount.") : ParameterInfo.None
      },
      {
        "level",
        (int index) => index == 0 ? ParameterInfo.Create("level or level=<color=yellow>altitude</color>", "Levels the terrain to a given altitude. Without parameters, levels to the terrain altitude below the player.") : ParameterInfo.None
      },
      {
        "within",
        (int index) => index == 0 ? ParameterInfo.Create("within", "min-max", "Only includes terrain within the given altitude range.") : ParameterInfo.None
      },
      {
        "paint",
        (int index) => {
          if (index == 0) return paints;
          if (index == 1) return ParameterInfo.Create("paint=dirt,<color=yellow>cultivated,vegetation</color>,paved", "Custom color (values from 0.0 to 1.0).");
          if (index == 2) return ParameterInfo.Create("paint=dirt,cultivated,<color=yellow>paved</color>,vegetation", "Custom color (values from 0.0 to 1.0).");
          if (index == 3) return ParameterInfo.Create("paint=dirt,cultivated,paved,<color=yellow>vegetation</color>", "Custom color (values from 0.0 to 1.0).");
          return ParameterInfo.None;
        }
      },
      {
        "id", (int index) => ParameterInfo.Ids
      },
      {
        "ignore", (int index) => ParameterInfo.Ids
      },
    });
  }
}
