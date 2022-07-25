using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class UndoHelper {
  public static void Refresh(ZDO zdo) {
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var obj)) {
      var newObj = ZNetScene.instance.CreateObject(zdo);
      UnityEngine.Object.Destroy(obj.gameObject);
      ZNetScene.instance.m_instances[zdo] = newObj.GetComponent<ZNetView>();
    }
  }
  public static void CopyData(ZDO from, ZDO to) {
    from = from.Clone();
    var refresh = to.m_prefab != from.m_prefab;
    to.m_floats = from.m_floats;
    to.m_vec3 = from.m_vec3;
    to.m_quats = from.m_quats;
    to.m_ints = from.m_ints;
    to.m_longs = from.m_longs;
    to.m_strings = from.m_strings;
    to.m_byteArrays = from.m_byteArrays;
    to.m_prefab = from.m_prefab;
    to.m_position = from.m_position;
    to.m_rotation = from.m_rotation;
    var zs = ZNetScene.instance;
    if (zs.m_instances.TryGetValue(to, out var view)) {
      view.transform.position = from.m_position;
      view.transform.rotation = from.m_rotation;
      view.transform.localScale = from.GetVec3("scale", Vector3.one);
      if (refresh) {
        var newObj = ZNetScene.instance.CreateObject(to);
        if (newObj) {
          UnityEngine.Object.Destroy(view.gameObject);
          ZNetScene.instance.m_instances[to] = newObj.GetComponent<ZNetView>();
        }
      }
    }
    to.IncreseDataRevision();
  }
  public static ZDO Place(ZDO zdo) {
    var prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
    if (!prefab) throw new InvalidOperationException("Error: Prefab not found.");
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, zdo.GetPosition(), zdo.GetRotation());
    var netView = obj.GetComponent<ZNetView>();
    if (!netView) throw new InvalidOperationException("Error: View not found.");
    var added = netView.GetZDO();
    netView.SetLocalScale(zdo.GetVec3("scale", obj.transform.localScale));
    CopyData(zdo, added);
    //Hammer.FixData(netView);
    return added;
  }
  public static ZDO[] Place(ZDO[] data) => data.Select(Place).ToArray();


  public static string Name(ZDO zdo) => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(zdo.GetPrefab()));
  public static string Print(IEnumerable<ZDO> data) {
    if (data.Count() == 1) return Name(data.First());
    var names = data.GroupBy(Name);
    if (names.Count() == 1) return $"{names.First().Key} {names.First().Count()}x";
    return $" objects {data.Count()}x";
  }
  public static ZDO[] Remove(ZDO[] toRemove) {
    var data = UndoHelper.Clone(toRemove);
    foreach (var zdo in toRemove) Actions.RemoveZDO(zdo);
    return data;
  }

  public static ZDO[] Clone(IEnumerable<ZDO> data) => data.Select(zdo => zdo.Clone()).ToArray();
}
public class UndoRemove : MonoBehaviour, UndoAction {
  private ZDO[] Data;
  public UndoRemove(IEnumerable<ZDO> data) {
    Data = data.ToArray();
  }
  public void Undo() {
    Data = UndoHelper.Place(Data);
  }
  public void Redo() {
    Data = UndoHelper.Remove(Data);
  }
  public string UndoMessage() => $"Undo: Restored {UndoHelper.Print(Data)}";
  public string RedoMessage() => $"Redo: Removed {UndoHelper.Print(Data)}";
}


public class EditData {
  public EditData(ZDO previous, ZDO current, bool refresh) {
    Zdo = current;
    Previous = previous.Clone();
    Current = current.Clone();
    Refresh = refresh;
  }
  public ZDO Zdo;
  public ZDO Previous;
  public ZDO Current;
  public bool Refresh;
}
public class UndoEdit : MonoBehaviour, UndoAction {
  private EditData[] Data;
  public UndoEdit(IEnumerable<EditData> data) {
    Data = data.ToArray();
  }
  public void Undo() {
    foreach (var data in Data) {
      UndoHelper.CopyData(data.Previous, data.Zdo);
      if (data.Refresh) UndoHelper.Refresh(data.Zdo);
    }
  }

  public string UndoMessage() => $"Undo: Changed {UndoHelper.Print(Data.Select(data => data.Zdo))}";

  public void Redo() {
    foreach (var data in Data) {
      UndoHelper.CopyData(data.Current, data.Zdo);
      if (data.Refresh) UndoHelper.Refresh(data.Zdo);
    }
  }
  public string RedoMessage() => $"Redo: Changed {UndoHelper.Print(Data.Select(data => data.Zdo))}";
}
