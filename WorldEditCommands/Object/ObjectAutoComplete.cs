using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class ObjectAutoComplete : SharedObjectAutoComplete {
  public List<string> NamedParameters;
  public ObjectAutoComplete() {
    NamedParameters = WithSharedParameters(new() {
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
    AutoComplete.Register(ObjectCommand.Name, (int index) => NamedParameters, WithSharedFetchers(new() {
      {
        "baby",
        (int index) => ParameterInfo.Flag("Baby")
      },
      {
        "wild",
        (int index) => ParameterInfo.Flag("Wild")
      },
      {
        "remove",
        (int index) => ParameterInfo.Flag("Remove")
      },
      {
        "sleep",
        (int index) => ParameterInfo.Flag("Sleep")
      },
      {
        "info",
        (int index) => ParameterInfo.Flag("Info")
      },
      {
        "id",
        (int index) => index == 0 ? ParameterInfo.Ids : null
      },
      {
        "move",
        (int index) => ParameterInfo.XZY("move", "Movement offset based on the player rotation (unless origin is given)", index)
      },
      {
        "rotate",
        (int index) => {
          var desc = $"Rotation based on the player rotation (unless origin is given)";
          if (index == 0) return ParameterInfo.Create("rotate=<color=yellow>reset</color> or " + ParameterInfo.YXZ("rotate", desc, index)[0]);
          return ParameterInfo.YXZ("rotate", desc, index);
        }
      },
      {
        "visual",
        (int index) => VisualAutoComplete("visual", index)
      },
      {
        "origin",
        (int index) => index == 0 ? ParameterInfo.Origin : null
      },
    }));
  }
}
