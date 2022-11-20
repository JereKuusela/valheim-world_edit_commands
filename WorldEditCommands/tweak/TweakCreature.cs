using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakCreatureCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "faction") return TweakActions.Faction(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "affix") return TweakActions.CLLC(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, long? value)
  {
    throw new NotImplementedException();
  }

  public TweakCreatureCommand()
  {
    Component = typeof(Character);
    ComponentName = "character";
    if (WorldEditCommands.IsCLLC)
      SupportedOperations.Add("affix", typeof(string[]));

    if (WorldEditCommands.IsCLLC)
      AutoComplete.Add("affix", (int index) => Enum.GetNames(typeof(BossAffix)).ToList());
    AutoComplete.Add("faction", (int index) => index == 0 ? Enum.GetNames(typeof(Character.Faction)).ToList() : ParameterInfo.None);
    Init("tweak_creature", "Modify creatures");
  }
}
