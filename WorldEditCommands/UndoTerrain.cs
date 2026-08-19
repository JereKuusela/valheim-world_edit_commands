using System.Collections.Generic;
using UnityEngine;
namespace WorldEditCommands;

public class HeightUndoData
{
  public int Index = 0;
  public float Smooth = 0f;
  public float Level = 0f;
  public bool HeightModified = false;
}

public class PaintUndoData
{
  public int Index = 0;
  public bool PaintModified = false;
  public Color Paint = Color.black;
}
public class TerrainUndoData
{
  public List<HeightUndoData> Heights = [];
  public List<PaintUndoData> Paints = [];
}

public class UndoTerrain(Dictionary<Vector3, TerrainUndoData> before, Dictionary<Vector3, TerrainUndoData> after, Vector3 position, float radius)
{
  // Key is compiler position to not rely on the reference.
  private readonly Dictionary<Vector3, TerrainUndoData> Before = before;
  // Key is compiler position to not rely on the reference.
  private readonly Dictionary<Vector3, TerrainUndoData> After = after;
  public Vector3 Position = position;
  public float Radius = radius;

  public void Undo()
  {
    Terrain.ApplyData(Before, Position, Radius);
  }

  public void Redo()
  {
    Terrain.ApplyData(After, Position, Radius);
  }
}
