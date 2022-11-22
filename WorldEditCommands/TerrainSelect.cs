using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
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
  public static Func<BaseIndex, bool> CreateBlockCheckFilter(BlockCheck blockCheck)
  {
    return (BaseIndex index) =>
    {
      if (blockCheck == BlockCheck.Off) return true;
      var blocked = ZoneSystem.instance.IsBlocked(index.Position);
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
    compiler.m_hmap.WorldToVertex(centerPos, out var cx, out var cy);
    Range<float> distanceLimit = new(radius.Min / compiler.m_hmap.m_scale, radius.Max / compiler.m_hmap.m_scale);
    var max = compiler.m_width + 1;
    Vector2 center = new((float)cx, (float)cy);
    for (int i = 0; i < max; i++)
    {
      for (int j = 0; j < max; j++)
      {
        var distance = Vector2.Distance(center, new((float)j, (float)i));
        if (!Helper.Within(distanceLimit, distance)) continue;
        var distanceX = j - cx;
        var distanceY = i - cy;
        indices.Add(new()
        {
          Index = i * max + j,
          Position = VertexToWorld(compiler.m_hmap, j, i),
          DistanceWidth = distanceX / distanceLimit.Max,
          DistanceDepth = distanceY / distanceLimit.Max,
          Distance = distance / distanceLimit.Max
        });
      }
    }
    return indices;
  }
  private static Vector3 VertexToWorld(Heightmap hmap, int x, int y)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    vector.z += (y - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    return vector;
  }

  private static float GetX(int x, int y, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * y;
  private static float GetY(int x, int y, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
  private static IEnumerable<HeightIndex> GetHeightIndicesWithRect(TerrainComp compiler, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    List<HeightIndex> indices = new();
    compiler.m_hmap.WorldToVertex(centerPos, out var cx, out var cy);
    Range<float> widthLimit = new(width.Min / compiler.m_hmap.m_scale, width.Max / compiler.m_hmap.m_scale);
    Range<float> depthLimit = new(depth.Min / compiler.m_hmap.m_scale, depth.Max / compiler.m_hmap.m_scale);
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int y = 0; y < max; y++)
      {
        var dx = x - cx;
        var dy = y - cy;
        var distanceX = GetX(dx, dy, angle);
        var distanceY = GetY(dx, dy, angle);
        if (!Helper.Within(widthLimit, depthLimit, Mathf.Abs(distanceX), Mathf.Abs(distanceY)))
          continue;
        var distanceWidth = distanceX / widthLimit.Max;
        var distanceDepth = distanceY / depthLimit.Max;
        indices.Add(new()
        {
          Index = y * max + x,
          Position = VertexToWorld(compiler.m_hmap, x, y),
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
    centerPos = new(centerPos.x - 0.5f, centerPos.y, centerPos.z - 0.5f);
    List<PaintIndex> indices = new();
    compiler.m_hmap.WorldToVertex(centerPos, out var cx, out var cy);
    Range<float> widthLimit = new(width.Min / compiler.m_hmap.m_scale, width.Max / compiler.m_hmap.m_scale);
    Range<float> depthLimit = new(depth.Min / compiler.m_hmap.m_scale, depth.Max / compiler.m_hmap.m_scale);
    var max = compiler.m_width;
    for (int x = 0; x < max; x++)
    {
      for (int y = 0; y < max; y++)
      {
        var dx = x - cx;
        var dy = y - cy;
        var distanceX = GetX(dx, dy, angle);
        var distanceY = GetY(dx, dy, angle);
        if (!Helper.Within(widthLimit, depthLimit, Mathf.Abs(distanceX), Mathf.Abs(distanceY)))
          continue;
        indices.Add(new()
        {
          Index = y * max + x,
          Position = VertexToWorld(compiler.m_hmap, x, y)
        });
      }
    }
    return indices;
  }

  private static IEnumerable<PaintIndex> GetPaintIndicesWithCircle(TerrainComp compiler, Vector3 centerPos, Range<float> radius)
  {
    centerPos = new(centerPos.x - 0.5f, centerPos.y, centerPos.z - 0.5f);
    List<PaintIndex> indices = new();
    compiler.m_hmap.WorldToVertex(centerPos, out var cx, out var cy);
    Range<float> distanceLimit = new(radius.Min / compiler.m_hmap.m_scale, radius.Max / compiler.m_hmap.m_scale);
    var max = compiler.m_width;
    Vector2 center = new(cx, cy);
    for (int i = 0; i < max; i++)
    {
      for (int j = 0; j < max; j++)
      {
        var distance = Vector2.Distance(center, new(j, i));
        if (!Helper.Within(distanceLimit, distance)) continue;
        indices.Add(new()
        {
          Index = i * max + j,
          Position = VertexToWorld(compiler.m_hmap, j, i)
        });
      }
    }
    return indices;
  }
}
