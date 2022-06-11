using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WorldEditCommands;
using CompilerIndices = Dictionary<TerrainComp, Indices>;

public partial class Terrain {
  ///<summary>Returns terrain data of given indices</summary>
  public static Dictionary<Vector3, TerrainUndoData> GetData(CompilerIndices compilerIndices) {
    return compilerIndices.ToDictionary(kvp => kvp.Key.transform.position, kvp => {
      return new TerrainUndoData {
        Heights = kvp.Value.HeightIndices.Select(heightIndex => new HeightUndoData {
          Index = heightIndex.Index,
          HeightModified = kvp.Key.m_modifiedHeight[heightIndex.Index],
          Level = kvp.Key.m_levelDelta[heightIndex.Index],
          Smooth = kvp.Key.m_smoothDelta[heightIndex.Index]
        }).ToArray(),
        Paints = kvp.Value.PaintIndices.Select(paintIndex => new PaintUndoData {
          Index = paintIndex.Index,
          PaintModified = kvp.Key.m_modifiedPaint[paintIndex.Index],
          Paint = kvp.Key.m_paintMask[paintIndex.Index],
        }).ToArray(),
      };
    });
  }

  public static void ApplyData(Dictionary<Vector3, TerrainUndoData> data, Vector3 pos, float radius) {
    foreach (var kvp in data) {
      var compiler = TerrainComp.FindTerrainCompiler(kvp.Key);
      if (!compiler) continue;
      foreach (var value in kvp.Value.Heights) {
        compiler.m_smoothDelta[value.Index] = value.Smooth;
        compiler.m_levelDelta[value.Index] = value.Level;
        compiler.m_modifiedHeight[value.Index] = value.HeightModified;
      }
      foreach (var value in kvp.Value.Paints) {
        compiler.m_modifiedPaint[value.Index] = value.PaintModified;
        compiler.m_paintMask[value.Index] = value.Paint;
      }
      Save(compiler);
    }
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
  public static void Save(TerrainComp compiler) {
    compiler.GetComponent<ZNetView>()?.ClaimOwnership();
    compiler.m_operations++;
    // These are only used to remove grass which isn't really needed.
    compiler.m_lastOpPoint = Vector3.zero;
    compiler.m_lastOpRadius = 0f;
    compiler.Save();
    compiler.m_hmap.Poke(false);
  }
}
