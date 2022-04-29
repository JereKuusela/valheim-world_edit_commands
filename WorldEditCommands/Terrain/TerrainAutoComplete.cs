using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class TerrainAutoComplete {
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
    "blockcheck",
    "rect",
    "circle",
    "smooth",
    "slope",
    "angle",
    "offset",
    "step",
    "refPos",
    "target"
  };
  public TerrainAutoComplete() {
    NamedParameters.Sort();
    List<string> paints = new() {
      "dirt",
      "paved",
      "cultivated",
      "grass"
    };
    paints.Sort();
    AutoComplete.Register(TerrainCommand.Name, (int index) => NamedParameters, new() {
      {
        "reset",
        (int index) => ParameterInfo.Flag("Reset")
      },
      {
        "blockcheck",
        (int index) => index == 0 ? ParameterInfo.Create("blockcheck=<color=yellow>inverse</color>/<color=yellow>off</color>/<color=yellow>on</color>", "When <color=yellow>on</color>, excludes terrain under structures. When <color=yellow>inverse</color>, only includes terrain under structures.") : ParameterInfo.None
      },
      {
        "rect",
        (int index) => {
          if (index == 0) return ParameterInfo.Create("rect=<color=yellow>size</color> or rect=<color=yellow>width</color>,depth (from 0 to 128)", "Size of the included terrain (rectangle).");
          if (index == 1) return ParameterInfo.Create("rect=width,<color=yellow>depth</color> (from 0 to 128)", "Size of the included terrain (rectangle).");
          return ParameterInfo.None;
        }
      },
      {
        "angle",
        (int index) => index == 0 ? ParameterInfo.Create("angle=<color=yellow>number</color>/<color=yellow>n</color>/<color=yellow>ne</color>/<color=yellow>e</color>/<color=yellow>se</color>/<color=yellow>s</color>/<color=yellow>sw</color>/<color=yellow>w</color>/<color=yellow>nw</color> (from 0 to 360)", "Direction of the shape.") : ParameterInfo.None
      },
      {
        "circle",
        (int index) => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color> (from 0 to 128)", "Diameter of the included terrain (circle).") : ParameterInfo.None
      },
      {
        "offset",
        (int index) => ParameterInfo.FRU("offset", "Position of the center", index)
      },
      {
        "refPos",
        (int index) => ParameterInfo.XZY("refPos", "Overrides the player position", index)
      },
      {
        "target",
        (int index) => ParameterInfo.XZY("target", "Determines the angle and circle/rect distance.", index)
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
          if (index == 0) return ParameterInfo.Create("slope=<color=yellow>height</color>,angle=0", "Creates a slope with altitude difference of a given amount.");
          if (index == 1) return ParameterInfo.Create("slope=height,<color=yellow>angle</color>", "Changes the slope direction (added to the general angle).");
          return ParameterInfo.None;
        }
      },
      {
        "raise",
        (int index) => index == 0 ? ParameterInfo.Create("raise=<color=yellow>number</color>", "Raises the terrain by a given amount.") : ParameterInfo.None
      },
      {
        "lower",
        (int index) => index == 0 ? ParameterInfo.Create("lower=<color=yellow>number</color>", "Lowers the terrain by a given amount.") : ParameterInfo.None
      },
      {
        "level",
        (int index) => index == 0 ? ParameterInfo.Create("level or level=<color=yellow>number</color>", "Levels the terrain to a given altitude. Without parameters, levels to the terrain altitude below the player.") : ParameterInfo.None
      },
      {
        "paint",
        (int index) => index == 0 ? paints : ParameterInfo.None
      },
    });
  }
}
