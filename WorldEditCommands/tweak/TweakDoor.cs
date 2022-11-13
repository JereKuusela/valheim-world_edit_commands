using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakDoorCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
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
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    if (operation == "unlock")
      return TweakActions.Unlock(view, value);
    throw new NotImplementedException();
  }

  public TweakDoorCommand()
  {
    Component = typeof(Door);
    ComponentName = "door";
    SupportedOperations.Add("unlock", typeof(bool));

    AutoComplete.Add("unlock", (int index) => index == 0 ? ParameterInfo.Create("unlock=<color=yellow>true/false</color>", "Ignores wards. No value to toggle.") : ParameterInfo.None);
    Init("tweak_door", "Modify doors");
  }
}
