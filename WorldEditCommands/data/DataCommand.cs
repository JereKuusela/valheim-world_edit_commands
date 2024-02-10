using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using Service;
namespace WorldEditCommands;

public class DataCommand
{
  // Dictionary to only add undo for edited objects.
  private static readonly Dictionary<ZDOID, EditData> EditedInfo = [];
  private static void AddUndo(ZNetView view)
  {
    var zdo = view.GetZDO();
    if (EditedInfo.ContainsKey(zdo.m_uid)) return;
    EditedInfo[zdo.m_uid] = new EditData(zdo);
  }


  protected string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "save" && value != null)
    {
      DataLoading.Save(new ZDOData(view.GetZDO()), value, false);
      return $"¤ data saved.";
    }
    if (operation == "load" && value != null)
    {
      AddUndo(view);
      var zdo = DataHelper.CloneBase(view.GetZDO());
      DataLoading.Load(value, zdo);
      Regen(view, zdo);
      return $"¤ data loaded from {value}.";
    }
    throw new NotImplementedException();
  }
  protected string DoOperation(ZNetView view, string operation, float? value)
  {
    throw new NotImplementedException();
  }
  protected string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "merge" && value.Length > 0)
    {
      AddUndo(view);
      var values = value.SelectMany(str => str.Split(',')).ToArray();
      foreach (var str in values)
      {
        DataLoading.Load(str, view.GetZDO());
      }
      Actions.Refresh(view);
      return $"¤ data merged from {string.Join(", ", values)}.";
    }
    if (operation == "keep" && value.Length > 0)
    {
      AddUndo(view);
      var keys = value.SelectMany(str => str.Split(',')).ToArray();
      Regen(view, DataHelper.CloneWithKeys(view.GetZDO(), keys));
      return $"¤ data cleaned except {string.Join(", ", keys)}.";
    }
    if (operation == "remove" && value.Length > 0)
    {
      AddUndo(view);
      var keys = value.SelectMany(str => str.Split(',')).ToArray();
      Regen(view, DataHelper.CloneWithoutKeys(view.GetZDO(), keys));
      return $"¤ data keys removed {string.Join(", ", keys)}.";
    }
    if (operation == "set" && value.Length > 0)
    {
      AddUndo(view);
      var zdo = view.GetZDO();
      var id = zdo.m_uid;
      foreach (var str in value)
      {
        var split = str.Split(',');
        if (split.Length != 3) continue;
        var type = split[0].ToLowerInvariant();
        var key = int.TryParse(split[1], out var i) ? i : split[1].GetStableHashCode();
        var val = split[2];
        if (type == "float")
          ZDOExtraData.Set(id, key, Parse.Float(val));
        else if (type == "vec3")
          ZDOExtraData.Set(id, key, Parse.VectorXZY(val));
        else if (type == "quat")
          ZDOExtraData.Set(id, key, Parse.AngleYXZ(val));
        else if (type == "int")
          ZDOExtraData.Set(id, key, Parse.Int(val));
        else if (type == "hash")
          ZDOExtraData.Set(id, key, val.GetStableHashCode());
        else if (type == "bool")
          ZDOExtraData.Set(id, key, Parse.Boolean(val) == true ? 1 : 0);
        else if (type == "long")
          ZDOExtraData.Set(id, key, Parse.Long(val));
        else if (type == "string")
          ZDOExtraData.Set(id, key, val);
        else if (type == "array")
          ZDOExtraData.Set(id, key, Convert.FromBase64String(val));
        else throw new InvalidOperationException($"Unknown type {type}.");
      }
      zdo.IncreaseDataRevision();
      Actions.Refresh(view);
      return $"¤ data set.";
    }
    throw new NotImplementedException();
  }
  protected string DoOperation(ZNetView view, string operation, int? value)
  {
    throw new NotImplementedException();
  }
  protected string DoOperation(ZNetView view, string operation, bool? value)
  {
    if (value == false) return "";
    if (operation == "clear")
    {
      Regen(view, DataHelper.CloneBase(view.GetZDO()));
      return $"¤ data cleared.";
    }
    if (operation == "print")
    {
      return $"¤\n{string.Join("\n", DataHelper.Print(view.GetZDO()))}";
    }
    throw new NotImplementedException();
  }
  protected string DoOperation(ZNetView view, string operation, long? value)
  {
    throw new NotImplementedException();
  }
  // Removing properties is not supported, so have to recreate the object.
  // This means undo information has to be updated.
  private void Regen(ZNetView view, ZDO zdo)
  {
    var existing = view.GetZDO();
    var entry = EditedInfo[existing.m_uid];
    entry.Zdo = DataHelper.Regen(existing, zdo);
    EditedInfo[entry.Zdo.m_uid] = entry;
    EditedInfo.Remove(existing.m_uid);
  }
  static readonly int Player = "Player".GetStableHashCode();
  private void Execute(Terminal context, Dictionary<string, object?> operations, ZNetView[] views)
  {
    EditedInfo.Clear();

    var count = views.Count();
    foreach (var view in views)
    {
      var zdo = view.GetZDO();
      if (zdo.GetPrefab() != Player)
        view.ClaimOwnership();

      foreach (var operation in operations)
      {
        var type = DataParameters.SupportedOperations[operation.Key];
        var name = Utils.GetPrefabName(view.gameObject);
        string output;
        if (type == typeof(int))
          output = DoOperation(view, operation.Key, (int?)operation.Value);
        else if (type == typeof(long))
          output = DoOperation(view, operation.Key, (long?)operation.Value);
        else if (type == typeof(float))
          output = DoOperation(view, operation.Key, (float?)operation.Value);
        else if (type == typeof(string[]))
          output = DoOperation(view, operation.Key, (string[])operation.Value!);
        else if (type == typeof(bool))
          output = DoOperation(view, operation.Key, (bool?)operation.Value!);
        else
          output = DoOperation(view, operation.Key, (string?)operation.Value);
        // No operation.
        if (output == "") continue;
        var message = output.Replace("¤", name);
        if (count == 1)
          Helper.AddMessage(context, message);
        else
          context.AddString(message);
      }
    }
    if (EditedInfo.Count > 0)
    {
      foreach (var info in EditedInfo)
        info.Value.Update();
      UndoManager.Add(new UndoEdit(EditedInfo.Select(kvp => kvp.Value)));
    }
  }
  public const string Name = "data";
  public DataCommand()
  {
    AutoComplete.Register(Name, (int index) => DataAutoComplete.GetOptions(), DataAutoComplete.GetNamedOptions());
    Helper.Command(Name, "Modifies object data.", (args) =>
    {
      DataParameters pars = new(args);
      var views = pars.GetObjects();
      if (pars.Operations.ContainsKey("save") && views.Length > 1)
        throw new InvalidOperationException("Can't save multiple objects at once.");
      Execute(args.Context, pars.Operations, views);

    });
  }
}
