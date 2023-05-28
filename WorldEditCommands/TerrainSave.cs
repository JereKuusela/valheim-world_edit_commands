using System.Collections.Generic;
using UnityEngine;
namespace WorldEditCommands;
public partial class Terrain {
  ///<summary>Returns terrain data of given indices</summary>
  public static Dictionary<Vector3, TerrainUndoData> GetData(List<HeightNode> heightNodes, List<PaintNode> paintNodes) {
    Dictionary<Vector3, TerrainUndoData> data = new();
    foreach (var node in heightNodes) {
      var compiler = node.Compiler;
      if (compiler == null) continue;
      var pos = compiler.transform.position;
      if (!data.ContainsKey(pos))
        data.Add(pos, new TerrainUndoData());

      data[pos].Heights.Add(new HeightUndoData {
        Index = node.Index,
        HeightModified = compiler.m_modifiedHeight[node.Index],
        Level = compiler.m_levelDelta[node.Index],
        Smooth = compiler.m_smoothDelta[node.Index]
      });
    }
    foreach (var node in paintNodes) {
      var compiler = node.Compiler;
      if (compiler == null) continue;
      var pos = compiler.transform.position;
      if (!data.ContainsKey(pos))
        data.Add(pos, new TerrainUndoData());

      data[pos].Paints.Add(new PaintUndoData {
        Index = node.Index,
        PaintModified = compiler.m_modifiedPaint[node.Index],
        Paint = compiler.m_paintMask[node.Index],
      });
    }
    return data;
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
