using System.Collections.Generic;
using DEV;

namespace WorldEditCommands {
  public class AliasesCommand : BaseCommand {
    public AliasesCommand() {
      new Terminal.ConsoleCommand("world_edit_aliases", "[set/clear] - Sets some useful aliases.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length > 1 && args[1] == "clear") {
          args.Context.TryRunCommand("alias move");
          args.Context.TryRunCommand("alias rotate");
          args.Context.TryRunCommand("alias scal");
          args.Context.TryRunCommand("alias stars");
          args.Context.TryRunCommand("alias health");
          args.Context.TryRunCommand("alias remove");
          args.Context.TryRunCommand("alias change_helmet");
          args.Context.TryRunCommand("alias change_left");
          args.Context.TryRunCommand("alias change_right");
          args.Context.TryRunCommand("alias change_legs");
          args.Context.TryRunCommand("alias change_chest");
          args.Context.TryRunCommand("alias change_shoulders");
          args.Context.TryRunCommand("alias change_utility");
          args.Context.TryRunCommand("alias essential");
        } else {
          args.Context.TryRunCommand("alias move object move=$,$ radius=$ id=$");
          args.Context.TryRunCommand("alias rotate object rotate=$,$ radius=$ id=$");
          args.Context.TryRunCommand("alias scale object scale=$ radius=$ id=$");
          args.Context.TryRunCommand("alias stars object stars=$ radius=$ id=$");
          args.Context.TryRunCommand("alias health object stars=$ radius=$ id=$");
          args.Context.TryRunCommand("alias remove object remove=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_helmet object helmet=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_left object left_hand=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_right object right_hand=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_legs object legs=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_chest object chest=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_shoulders object shoulders=$ radius=$ id=$");
          args.Context.TryRunCommand("alias change_utility object utility=$ radius=$ id=$");
          args.Context.TryRunCommand("alias essential object tame health=1E30 radius=$ id=$");

        }
      });
      AutoComplete.Register("world_edit_aliases", (int index) => {
        if (index == 0) return new List<string>() { "set", "clear" };
        return null;
      });
    }
  }
}