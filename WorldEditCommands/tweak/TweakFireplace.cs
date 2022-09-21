using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakFireplaceCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value) {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value) {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value) {
    if (operation == "restrict")
      return TweakActions.Restrict(view, value);
    throw new NotImplementedException();
  }

  public TweakFireplaceCommand() {
    Component = typeof(Fireplace);
    ComponentName = "fireplace";
    SupportedOperations.Add("restrict", typeof(bool));

    AutoComplete.Add("restrict", (int index) => index == 0 ? ParameterInfo.Create("restrict=<color=yellow>false</color>", "Prevents the fire going out when blocked by smoke.. No value to reset.") : ParameterInfo.None);
    Init("tweak_fireplace", "Modify fireplaces");
  }
}
