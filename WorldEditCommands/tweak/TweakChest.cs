using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakChestCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "name")
      return TweakActions.Name(view, value);
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
    if (operation == "unlock")
      return TweakActions.Unlock(view, value);
    throw new NotImplementedException();
  }

  public TweakChestCommand() {
    Component = typeof(Container);
    ComponentName = "chest";
    SupportedOperations.Add("unlock", typeof(bool));
    SupportedOperations.Add("name", typeof(string));

    AutoComplete.Add("unlock", (int index) => index == 0 ? ParameterInfo.Create("unlock=<color=yellow>true/false</color>", "Ignores wards. No value to toggle.") : ParameterInfo.None);
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    Init("tweak_chest", "Modify chests");
  }
}
