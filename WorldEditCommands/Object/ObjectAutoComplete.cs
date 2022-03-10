using System;
using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands {
  using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
  public class ObjectAutoComplete : SharedObjectAutoComplete {
    public List<string> NamedParameters;
    public ObjectAutoComplete() {
      NamedParameters = MergeDefault(new List<string>() {
        "baby",
        "wild",
        "info",
        "sleep",
        "id",
        "move",
        "rotate",
        "remove",
        "origin",
        "visual"
      });
      AutoComplete.Register("object", (int index) => NamedParameters, MergeDefault(new NamedOptionsFetchers() {
        {
          "baby", (int index) => ParameterInfo.Flag("Baby")
        },
        {
          "wild", (int index) => ParameterInfo.Flag("Wild")
        },
        {
          "remove", (int index) => ParameterInfo.Flag("Remove")
        },
        {
          "sleep", (int index) => ParameterInfo.Flag("Sleep")
        },
        {
          "info", (int index) => ParameterInfo.Flag("Info")
        },
        {
          "id", (int index) => index == 0 ? ParameterInfo.Ids : null
        },
        {
          "move", (int index) => ParameterInfo.XZY("move", "Movement offset based on the player rotation (unless origin is given)", index)
        },
        {
          "rotate", (int index) => {
            var desc = $"Rotation based on the player rotation (unless origin is given)";
            if (index == 0) return ParameterInfo.Create("rotate=<color=yellow>reset</> or " + ParameterInfo.YXZ("rotate", desc, index)[0]);
            return ParameterInfo.YXZ("rotate", desc, index);
          }
        },
        {
          "visual", (int index) => VisualAutoComplete("visual", index)
        },
        {
          "origin", (int index) => index == 0? ParameterInfo.Origin : null
        },
      }));
    }
  }
}
