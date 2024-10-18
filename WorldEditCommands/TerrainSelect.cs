using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;
public abstract class TerrainNode
{
  public int Index;
  public Vector3 Position;
  public float DistanceWidth;
  public float DistanceDepth;
  public float Distance;

  public TerrainComp? Compiler;
}

public class HeightNode : TerrainNode { }
public class PaintNode : TerrainNode { }


public enum BlockCheck
{
  Off,
  On,
  Inverse
}

public partial class Terrain
{

  public static TerrainComp[] GetCompilers(Vector3 position, Range<float> radius)
  {
    List<Heightmap> heightMaps = [];
    Heightmap.FindHeightmap(position, radius.Max + 1, heightMaps);
    var pos = ZNet.instance.GetReferencePosition();
    var ns = ZNetScene.instance;
    return heightMaps.Where(hmap => ZNetScene.InActiveArea(ZoneSystem.GetZone(hmap.transform.position), pos)).Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToArray();
  }
  public static TerrainComp[] GetCompilers(Vector3 position, Range<float> width, Range<float> depth, float angle)
  {
    List<Heightmap> heightMaps = [];
    // Turn the rectable to a square for an upper bound.
    var maxDimension = Mathf.Max(width.Max, depth.Max);
    // Rotating increases the square dimensions.
    var dimensionMultiplier = Mathf.Abs(Mathf.Sin(angle)) + Mathf.Abs(Mathf.Cos(angle));
    var size = maxDimension * dimensionMultiplier;
    Heightmap.FindHeightmap(position, size + 1, heightMaps);
    var pos = ZNet.instance.GetReferencePosition();
    var ns = ZNetScene.instance;
    return heightMaps.Where(hmap => ZNetScene.InActiveArea(ZoneSystem.GetZone(hmap.transform.position), pos)).Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToArray();
  }

  public static Func<TerrainNode, bool> CreateBlockCheckFilter(BlockCheck blockCheck, string[] includedIds, string[] excludedIds)
  {
    var included = Selector.GetPrefabs(includedIds);
    var excluded = Selector.GetExcludedPrefabs(excludedIds);
    var zs = ZoneSystem.instance;
    return (TerrainNode index) =>
    {
      if (blockCheck == BlockCheck.Off) return true;
      var pos = index.Position;
      pos.y += 2000f;
      var hits = Physics.RaycastAll(pos, Vector3.down, 10000f, zs.m_blockRayMask);
      var blocked = hits.Select(Selector.GetPrefabFromHit).Any(prefab => included.Contains(prefab) && !excluded.Contains(prefab));
      if (blocked && blockCheck == BlockCheck.On) return false;
      if (!blocked && blockCheck == BlockCheck.Inverse) return false;
      return true;
    };
  }
  public static Func<TerrainNode, bool> CreateAltitudeFilter(float min, float max)
  {
    return (TerrainNode index) =>
    {
      var height = ZoneSystem.instance.GetGroundHeight(index.Position);
      return height >= min && height <= max;
    };
  }
  public static void GetHeightNodesWithCircle(List<HeightNode> nodes, TerrainComp compiler, Vector3 centerPos, Range<float> radius)
  {
    if (radius.Max == 0f) return;
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var dx = nodePos.x - centerPos.x;
        var dz = nodePos.z - centerPos.z;
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (!Helper.Within(radius, distance)) continue;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = dx / radius.Max,
          DistanceDepth = dz / radius.Max,
          Distance = distance / radius.Max,
          Compiler = compiler
        });
      }
    }
  }
  private static Vector3 VertexToWorld(Heightmap hmap, int x, int z)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2) * hmap.m_scale;
    vector.z += (z - hmap.m_width / 2) * hmap.m_scale;
    return vector;
  }

  private static float GetX(float x, float z, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * z;
  private static float GetZ(float x, float z, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * z;
  public static void GetHeightNodesWithRect(List<HeightNode> nodes, TerrainComp compiler, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    if (width.Max == 0f || depth.Max == 0f) return;
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var rawDx = nodePos.x - centerPos.x;
        var rawDz = nodePos.z - centerPos.z;
        var dx = GetX(rawDx, rawDz, angle);
        var dz = GetZ(rawDx, rawDz, angle);
        if (!Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
          continue;
        var distanceWidth = dx / width.Max;
        var distanceDepth = dz / depth.Max;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = distanceWidth,
          DistanceDepth = distanceDepth,
          Distance = Mathf.Max(Mathf.Abs(distanceWidth), Mathf.Abs(distanceDepth)),
          Compiler = compiler
        });
      }
    }
  }

  public static void GetPaintNodesWithRect(List<PaintNode> nodes, TerrainComp compiler, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var rawDx = nodePos.x - centerPos.x;
        var rawDz = nodePos.z - centerPos.z;
        var dx = GetX(rawDx, rawDz, angle);
        var dz = GetZ(rawDx, rawDz, angle);
        if (!Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
          continue;
        var distanceWidth = dx / width.Max;
        var distanceDepth = dz / depth.Max;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = distanceWidth,
          DistanceDepth = distanceDepth,
          Distance = Mathf.Max(Mathf.Abs(distanceWidth), Mathf.Abs(distanceDepth)),
          Compiler = compiler
        });
      }
    }
  }

  public static void GetPaintNodesWithCircle(List<PaintNode> nodes, TerrainComp compiler, Vector3 centerPos, Range<float> radius)
  {
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var dx = nodePos.x - centerPos.x;
        var dz = nodePos.z - centerPos.z;
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (!Helper.Within(radius, distance)) continue;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = dx / radius.Max,
          DistanceDepth = dz / radius.Max,
          Distance = distance / radius.Max,
          Compiler = compiler
        });
      }
    }
  }
}
