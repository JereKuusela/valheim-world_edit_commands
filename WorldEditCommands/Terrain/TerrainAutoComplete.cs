using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands {
  public class TerrainAutoComplete {
    public TerrainAutoComplete() {
      var paints = new List<string>() {
        "dirt", "paved", "cultivated", "grass"
      };
      paints.Sort();
      AutoComplete.Register("terrain", (int index) => TerrainCommand.Operations, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "reset", (int index) => ParameterInfo.Flag("Reset")
        },
        {
          "blockcheck", (int index) => ParameterInfo.Flag("Block check")
        },
        {
          "square", (int index) => ParameterInfo.Flag("Square")
        },
        {
          "radius", (int index) => index == 0 ? ParameterInfo.Create("Circle radius or half of the square side", "number (from 0.0 to 64.0)") : null
        },
        {
          "smooth", (int index) => index == 0 ? ParameterInfo.Create("Smoothness", "number (from 0.0 to 1.0)") : null
        },
        {
          "raise", (int index) => index == 0 ? ParameterInfo.Create("Amount", "number") : null
        },
        {
          "lower", (int index) => index == 0 ? ParameterInfo.Create("Amount", "number") : null
        },
        {
          "level", (int index) => index == 0 ? ParameterInfo.Create("Amount", "number") : null
        },
        {
          "paint", (int index) => index == 0 ? paints : null
        },
      });
    }
  }
}
