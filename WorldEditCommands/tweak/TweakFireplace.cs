using System;
using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakFireplaceCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "smoke")
      return TweakActions.Smoke(view, value);
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

    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, long? value) {
    throw new NotImplementedException();
  }

  public static List<string> SmokeTypes = new() {
      "off",
      "on",
      "ignore"
  };
  public TweakFireplaceCommand() {
    Component = typeof(Fireplace);
    ComponentName = "fireplace";
    SupportedOperations.Add("smoke", typeof(string));

    AutoComplete.Add("smoke", (int index) => index == 0 ? SmokeTypes : ParameterInfo.None);
    Init("tweak_fireplace", "Modify fireplaces");
  }
}
