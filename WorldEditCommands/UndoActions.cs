using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;

public class UndoHelper {
  public static FakeZDO Place(FakeZDO zdo) {
    var prefab = ZNetScene.instance.GetPrefab(zdo.Prefab);
    if (!prefab) throw new InvalidOperationException("Error: Prefab not found.");
    ZNetView.m_initZDO = zdo.Create();
    FakeZDO newZdo = new(ZNetView.m_initZDO);
    UnityEngine.Object.Instantiate(prefab, ZNetView.m_initZDO.GetPosition(), ZNetView.m_initZDO.GetRotation());
    return newZdo;
  }
  public static void Remove(FakeZDO[] toRemove) {
    foreach (var zdo in toRemove)
      zdo.Destroy();
  }

  public static string Name(int hash) => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(hash));
  public static string Print(IEnumerable<ZDO> zdos) => Print(zdos.Where(zdo => zdo.IsValid()).Select(zdo => zdo.m_prefab));
  public static string Print(IEnumerable<FakeZDO> zdos) => Print(zdos.Select(zdo => zdo.Prefab));
  public static string Print(IEnumerable<int> data) {
    if (data.Count() == 1) return Name(data.First());
    var names = data.GroupBy(Name);
    if (names.Count() == 1) return $"{names.First().Key} {names.First().Count()}x";
    return $" objects {data.Count()}x";
  }
}


public class UndoRemove : MonoBehaviour, IUndoAction {
  private FakeZDO[] Zdos;
  public UndoRemove(IEnumerable<FakeZDO> zdos) {
    Zdos = zdos.ToArray();
  }
  public string Undo() {
    Zdos = Zdos.Select(UndoHelper.Place).ToArray();
    return $"Restored {UndoHelper.Print(Zdos)}";
  }
  public string Redo() {
    UndoHelper.Remove(Zdos);
    return $"Removed {UndoHelper.Print(Zdos)}"; ;
  }
}



public class EditData {
  public EditData(ZDO zdo) {
    Zdo = zdo;
    Previous = new(zdo);
    Current = Previous;
  }
  public void Update() {
    Current = new(Zdo);
  }
  public ZDO Zdo;
  public FakeZDO Previous;
  public FakeZDO Current;
}
public class UndoEdit : MonoBehaviour, IUndoAction {
  private readonly EditData[] Data;
  public UndoEdit(IEnumerable<EditData> data) {
    Data = data.ToArray();
  }
  public string Undo() {
    var message = $"Changed {UndoHelper.Print(Data.Select(data => data.Zdo))}";
    foreach (var data in Data) {
      // Refreshed objects get a new zdo instance.
      var zdo = ZDOMan.instance.GetZDO(data.Zdo.m_uid);
      // Could possibly edit a deleted ZDO.
      if (zdo == null || !zdo.IsValid()) continue;
      data.Previous.Copy(zdo);
      Actions.Refresh(zdo);
    }
    return message;
  }

  public string Redo() {
    var message = $"Changed {UndoHelper.Print(Data.Select(data => data.Zdo))}";
    foreach (var data in Data) {
      // Refreshed objects get a new zdo instance.
      var zdo = ZDOMan.instance.GetZDO(data.Zdo.m_uid);
      // Could possibly edit a deleted ZDO.
      if (zdo == null || !zdo.IsValid()) continue;
      data.Current.Copy(zdo);
      Actions.Refresh(zdo);
    }
    return message;
  }
}
