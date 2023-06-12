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
  public List<HeightUndoData> Heights = new();
  public List<PaintUndoData> Paints = new();
}

public class UndoTerrain : IUndoAction {

  private readonly Dictionary<Vector3, TerrainUndoData> Before = new();
  private readonly Dictionary<Vector3, TerrainUndoData> After = new();
  public Vector3 Position;
  public float Radius;
  public UndoTerrain(Dictionary<Vector3, TerrainUndoData> before, Dictionary<Vector3, TerrainUndoData> after, Vector3 position, float radius) {
    Before = before;
    After = after;
    Position = position;
    Radius = radius;
  }
  public string Undo() {
    Terrain.ApplyData(Before, Position, Radius);
    return "Undoing terrain changes";
  }

  public string Redo() {
    Terrain.ApplyData(After, Position, Radius);
    return "Redoing terrain changes";
  }
}
