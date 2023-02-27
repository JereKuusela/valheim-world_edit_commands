using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;
using CompilerIndices = Dictionary<TerrainComp, Indices>;
public class BaseIndex
{
  public int Index;
  public Vector3 Position;
}
public class HeightIndex : BaseIndex
{
  public float DistanceWidth;
  public float DistanceDepth;
  public float Distance;
}

public class PaintIndex : BaseIndex
{
}
public class Indices
{
  public HeightIndex[] HeightIndices = new HeightIndex[0];
  public PaintIndex[] PaintIndices = new PaintIndex[0];
}

public enum BlockCheck
{
  Off,
  On,
  Inverse
}

public partial class Terrain
{
  public static Func<TerrainComp, Indices> CreateIndexer(Vector3 centerPos, Range<float> radius)
  {
    return (TerrainComp comp) =>
    {
      return new()
      {
        HeightIndices = GetHeightIndicesWithCircle(comp, centerPos, radius).ToArray(),
        PaintIndices = GetPaintIndicesWithCircle(comp, centerPos, radius).ToArray()
      };
    };
  }
  public static Func<TerrainComp, Indices> CreateIndexer(Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    return (TerrainComp comp) =>
    {
      return new()
      {
        HeightIndices = GetHeightIndicesWithRect(comp, centerPos, width, depth, angle).ToArray(),
        PaintIndices = GetPaintIndicesWithRect(comp, centerPos, width, depth, angle).ToArray()
      };
    };
  }

  public static TerrainComp[] GetCompilers(Vector3 position, Range<float> radius)
  {
    List<Heightmap> heightMaps = new();
    Heightmap.FindHeightmap(position, radius.Max + 1, heightMaps);
    var pos = ZNet.instance.GetReferencePosition();
    var zs = ZoneSystem.instance;
    var ns = ZNetScene.instance;
    return heightMaps.Where(hmap => ns.InActiveArea(zs.GetZone(hmap.transform.position), pos)).Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToArray();
  }
  public static TerrainComp[] GetCompilers(Vector3 position, Range<float> width, Range<float> depth, float angle)
  {
    List<Heightmap> heightMaps = new();
    // Turn the rectable to a square for an upper bound.
    var maxDimension = Mathf.Max(width.Max, depth.Max);
    // Rotating increases the square dimensions.
    var dimensionMultiplier = Mathf.Abs(Mathf.Sin(angle)) + Mathf.Abs(Mathf.Cos(angle));
    var size = maxDimension * dimensionMultiplier;
    Heightmap.FindHeightmap(position, size + 1, heightMaps);
    var pos = ZNet.instance.GetReferencePosition();
    var zs = ZoneSystem.instance;
    var ns = ZNetScene.instance;
    return heightMaps.Where(hmap => ns.InActiveArea(zs.GetZone(hmap.transform.position), pos)).Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToArray();
  }
  private static CompilerIndices FilterEmpty(CompilerIndices indices)
  {
    return indices.Where(kvp => kvp.Value.HeightIndices.Count() + kvp.Value.PaintIndices.Count() > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
  }
  public static CompilerIndices GetIndices(IEnumerable<TerrainComp> compilers, Func<TerrainComp, Indices> indexer, IEnumerable<Func<BaseIndex, bool>> filterers)
  {
    var filterer = (BaseIndex index) => filterers.All(filterer => filterer(index));
    return FilterEmpty(compilers.ToDictionary(compiler => compiler, compiler =>
    {
      var indices = indexer(compiler);
      indices.HeightIndices = indices.HeightIndices.Where(index => filterer(index)).ToArray();
      indices.PaintIndices = indices.PaintIndices.Where(index => filterer(index)).ToArray();
      return indices;
    }));
  }
  public static Func<BaseIndex, bool> CreateBlockCheckFilter(BlockCheck blockCheck, string[] includedIds, string[] excludedIds)
  {
    var included = Selector.GetPrefabs(includedIds);
    var excluded = Selector.GetExcludedPrefabs(excludedIds);
    var zs = ZoneSystem.instance;
    return (BaseIndex index) =>
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
  public static Func<BaseIndex, bool> CreateAltitudeFilter(float min, float max)
  {
    return (BaseIndex index) =>
    {
      var height = ZoneSystem.instance.GetGroundHeight(index.Position);
      return height >= min && height <= max;
    };
  }
  private static IEnumerable<HeightIndex> GetHeightIndicesWithCircle(TerrainComp compiler, Vector3 centerPos, Range<float> radius)
  {
    List<HeightIndex> indices = new();
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
        indices.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = dx / radius.Max,
          DistanceDepth = dz / radius.Max,
          Distance = distance / radius.Max
        });
      }
    }
    return indices;
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
  private static IEnumerable<HeightIndex> GetHeightIndicesWithRect(TerrainComp compiler, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    List<HeightIndex> indices = new();
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
        indices.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = distanceWidth,
          DistanceDepth = distanceDepth,
          Distance = Mathf.Max(Mathf.Abs(distanceWidth), Mathf.Abs(distanceDepth))
        });
      }
    }
    return indices;
  }

  private static IEnumerable<PaintIndex> GetPaintIndicesWithRect(TerrainComp compiler, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    List<PaintIndex> indices = new();
    var max = compiler.m_width;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        // Painting is applied from the corner of the node, not the center.
        nodePos.x += 0.5f;
        nodePos.z += 0.5f;
        var dx = nodePos.x - centerPos.x;
        var dz = nodePos.z - centerPos.z;
        var distanceX = GetX(dx, dz, angle);
        var distanceZ = GetZ(dx, dz, angle);
        if (!Helper.Within(width, depth, Mathf.Abs(distanceX), Mathf.Abs(distanceZ)))
          continue;
        indices.Add(new()
        {
          Index = z * max + x,
          Position = nodePos
        });
      }
    }
    return indices;
  }

  private static IEnumerable<PaintIndex> GetPaintIndicesWithCircle(TerrainComp compiler, Vector3 centerPos, Range<float> radius)
  {
    List<PaintIndex> indices = new();
    var max = compiler.m_width;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        // Painting is applied from the corner of the node, not the center.
        nodePos.x += 0.5f;
        nodePos.z += 0.5f;
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (!Helper.Within(radius, distance)) continue;
        indices.Add(new()
        {
          Index = z * max + x,
          Position = nodePos
        });
      }
    }
    return indices;
  }
}
