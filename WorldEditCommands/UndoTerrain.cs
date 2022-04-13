using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class HeightUndoData {
  public float Smooth = 0f;
  public float Level = 0f;
  public int Index = -1;
  public bool HeightModified = false;
}

public class PaintUndoData {
  public bool PaintModified = false;
  public Color Paint = Color.black;
  public int Index = -1;
}
public class TerrainUndoData {
  public HeightUndoData[] Heights = new HeightUndoData[0];
  public PaintUndoData[] Paints = new PaintUndoData[0];
}

public class UndoTerrain : UndoAction {

  private Dictionary<Vector3, TerrainUndoData> Before = new();
  private Dictionary<Vector3, TerrainUndoData> After = new();
  public Vector3 Position;
  public float Radius;
  public UndoTerrain(Dictionary<Vector3, TerrainUndoData> before, Dictionary<Vector3, TerrainUndoData> after, Vector3 position, float radius) {
    Before = before;
    After = after;
    Position = position;
    Radius = radius;
  }
  public void Undo() {
    Terrain.ApplyData(Before, Position, Radius);
  }
  public string UndoMessage() => "Undoing terrain changes";

  public void Redo() {
    Terrain.ApplyData(After, Position, Radius);
  }
  public string RedoMessage() => "Redoing terrain changes";
}
