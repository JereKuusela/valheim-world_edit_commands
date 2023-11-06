using ServerDevcommands;
namespace WorldEditCommands;
public class AliasesCommand
{
  public AliasesCommand()
  {
    new Terminal.ConsoleCommand("world_edit_aliases", "[set/clear] - Sets some useful aliases.", (args) =>
    {
      var sub = ServerDevcommands.Settings.Substitution;
      if (args.Length > 1 && args[1] == "clear")
      {
        args.Context.TryRunCommand($"alias move");
        args.Context.TryRunCommand($"alias rotate");
        args.Context.TryRunCommand($"alias scal");
        args.Context.TryRunCommand($"alias stars");
        args.Context.TryRunCommand($"alias health");
        args.Context.TryRunCommand($"alias remove");
        args.Context.TryRunCommand($"alias change_helmet");
        args.Context.TryRunCommand($"alias change_left");
        args.Context.TryRunCommand($"alias change_right");
        args.Context.TryRunCommand($"alias change_legs");
        args.Context.TryRunCommand($"alias change_chest");
        args.Context.TryRunCommand($"alias change_shoulders");
        args.Context.TryRunCommand($"alias change_utility");
        args.Context.TryRunCommand($"alias essential");
        args.Context.TryRunCommand($"alias spawn");
      }
      else
      {
        args.Context.TryRunCommand($"alias move object move={sub},{sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias rotate object rotate={sub},{sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias scale object scale={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias stars object stars={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias health object stars={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias remove object remove={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_helmet object helmet={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_left object left_hand={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_right object right_hand={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_legs object legs={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_chest object chest={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_shoulders object shoulders={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias change_utility object utility={sub} radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias essential object tame health=1E30 radius={sub} id={sub}");
        args.Context.TryRunCommand($"alias spawn spawn_object {sub} amount={sub} level={sub}");
      }
    });
    AutoComplete.Register("world_edit_aliases", (int index) =>
    {
      if (index == 0) return ["set", "clear"];
      return ParameterInfo.None;
    });
  }
}
