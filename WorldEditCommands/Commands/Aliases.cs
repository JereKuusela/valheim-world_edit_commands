using DEV;

namespace WorldEditCommands {
  public class AliasesCommand : BaseCommand {
    public void AlisesCommand() {
      new Terminal.ConsoleCommand("world_edit_aliases", "[set/clear] - Sets some useful aliases.", delegate (Terminal.ConsoleEventArgs args) {
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
        args.Context.TryRunCommand("alias change_utility bject utility=$ radius=$ id=$");
        args.Context.TryRunCommand("alias essential object tame health=1E30 radius=$ id=$");
      });
    }
  }
}