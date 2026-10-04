using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public enum PathMode
{
  Curve,
  Line,
  Arc
}

public static class TerrainPath
{
  // Terrain granularity is 0.5 meters so finer sampling has no benefit.
  private const float Resolution = 0.5f;

  public static List<Vector3> Sample(List<Vector3> points, PathMode mode)
  {
    if (mode == PathMode.Line) return [.. points];
    if (mode == PathMode.Arc) return SampleArcs(points);
    return SampleCurve(points);
  }

  // Centripetal Catmull-Rom: passes through all points without cusps or self-intersections.
  private static List<Vector3> SampleCurve(List<Vector3> points)
  {
    List<Vector3> result = [];
    var n = points.Count;
    for (var i = 0; i < n - 1; i++)
    {
      var p1 = points[i];
      var p2 = points[i + 1];
      var p0 = i > 0 ? points[i - 1] : 2f * p1 - p2;
      var p3 = i < n - 2 ? points[i + 2] : 2f * p2 - p1;
      var steps = Mathf.Max(1, Mathf.CeilToInt(Utils.DistanceXZ(p1, p2) / Resolution));
      for (var k = 0; k < steps; k++)
        result.Add(CatmullRom(p0, p1, p2, p3, (float)k / steps));
    }
    result.Add(points[n - 1]);
    return result;
  }

  private static float Knot(Vector3 a, Vector3 b) => Mathf.Max(Mathf.Sqrt(Vector3.Distance(a, b)), 0.01f);
  private static Vector3 Interp(Vector3 a, Vector3 b, float ta, float tb, float u) => Vector3.LerpUnclamped(a, b, (u - ta) / (tb - ta));

  private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
  {
    var t0 = 0f;
    var t1 = t0 + Knot(p0, p1);
    var t2 = t1 + Knot(p1, p2);
    var t3 = t2 + Knot(p2, p3);
    var u = Mathf.Lerp(t1, t2, t);
    var a1 = Interp(p0, p1, t0, t1, u);
    var a2 = Interp(p1, p2, t1, t2, u);
    var a3 = Interp(p2, p3, t2, t3, u);
    var b1 = Interp(a1, a2, t0, t2, u);
    var b2 = Interp(a2, a3, t1, t3, u);
    return Interp(b1, b2, t1, t2, u);
  }

  // Each segment is a biarc (two arcs) matching the tangents at both points, so S-shapes stay smooth and the path has no kinks.
  private static List<Vector3> SampleArcs(List<Vector3> points)
  {
    var n = points.Count;
    var tangents = new Vector2[n];
    for (var i = 1; i < n - 1; i++)
    {
      var sum = Direction(points[i - 1], points[i]) + Direction(points[i], points[i + 1]);
      tangents[i] = sum.sqrMagnitude > 1e-6f ? sum.normalized : Direction(points[i], points[i + 1]);
    }
    // Ends mirror the neighbouring tangent over the chord, so a single arc is used when there are two points.
    var first = Direction(points[0], points[1]);
    var last = Direction(points[n - 2], points[n - 1]);
    tangents[0] = n > 2 ? Reflect(tangents[1], first) : first;
    tangents[n - 1] = n > 2 ? Reflect(tangents[n - 2], last) : last;
    List<Vector3> result = [points[0]];
    for (var i = 0; i < n - 1; i++)
      AddBiarc(result, points[i], tangents[i], points[i + 1], tangents[i + 1]);
    return result;
  }

  private static Vector2 Reflect(Vector2 t, Vector2 chord) => 2f * Vector2.Dot(chord, t) * chord - t;

  private static void AddBiarc(List<Vector3> result, Vector3 a, Vector2 ta, Vector3 b, Vector2 tb)
  {
    var v = new Vector2(b.x - a.x, b.z - a.z);
    var vv = v.sqrMagnitude;
    if (vv == 0f) return;
    var t = ta + tb;
    var vt = Vector2.Dot(v, t);
    var k = 2f * (1f - Vector2.Dot(ta, tb));
    float d;
    if (k < 1e-6f)
    {
      var vb = Vector2.Dot(v, tb);
      d = vb > 1e-6f ? vv / (4f * vb) : Mathf.Sqrt(vv) / 2f;
    }
    else
      d = (-vt + Mathf.Sqrt(vt * vt + k * vv)) / k;
    var joint = (new Vector2(a.x + b.x, a.z + b.z) + d * (ta - tb)) / 2f;
    var mid = new Vector3(joint.x, (a.y + b.y) / 2f, joint.y);
    var tangent = AddArc(result, a, mid, ta);
    AddArc(result, mid, b, tangent);
  }

  private static Vector2 Direction(Vector3 from, Vector3 to)
  {
    var d = new Vector2(to.x - from.x, to.z - from.z);
    return d.sqrMagnitude > 0f ? d.normalized : Vector2.up;
  }

  // Adds the arc from a to b that starts in the given tangent direction. Returns the tangent at b.
  private static Vector2 AddArc(List<Vector3> result, Vector3 a, Vector3 b, Vector2 tangent)
  {
    var dx = b.x - a.x;
    var dz = b.z - a.z;
    var chordSq = dx * dx + dz * dz;
    // Left normal of the tangent. Center is at a + normal * r (negative r means right side).
    var normal = new Vector2(-tangent.y, tangent.x);
    var side = dx * normal.x + dz * normal.y;
    if (chordSq == 0f) return tangent;
    if (Mathf.Abs(side) <= 1e-4f * Mathf.Sqrt(chordSq) || dx * tangent.x + dz * tangent.y < 0f)
    {
      var steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(chordSq) / Resolution));
      for (var k = 1; k <= steps; k++)
        result.Add(Vector3.Lerp(a, b, (float)k / steps));
      return Direction(a, b);
    }
    var r = chordSq / (2f * side);
    var radius = Mathf.Abs(r);
    var sign = Mathf.Sign(r);
    var cx = a.x + normal.x * r;
    var cz = a.z + normal.y * r;
    var angleA = Mathf.Atan2(a.z - cz, a.x - cx);
    var angleB = Mathf.Atan2(b.z - cz, b.x - cx);
    var twoPi = 2f * Mathf.PI;
    var sweep = sign > 0f ? Mathf.Repeat(angleB - angleA, twoPi) : -Mathf.Repeat(angleA - angleB, twoPi);
    var count = Mathf.Max(1, Mathf.CeilToInt(radius * Mathf.Abs(sweep) / Resolution));
    for (var k = 1; k <= count; k++)
    {
      var angle = angleA + sweep * k / count;
      result.Add(new(cx + radius * Mathf.Cos(angle), Mathf.Lerp(a.y, b.y, (float)k / count), cz + radius * Mathf.Sin(angle)));
    }
    return sign * new Vector2(-Mathf.Sin(angleB), Mathf.Cos(angleB));
  }

  public static float ClosestSegment(List<Vector3> path, IEnumerable<int> segments, Vector3 pos, out int segment, out float t)
  {
    var best = float.MaxValue;
    segment = -1;
    t = 0f;
    foreach (var i in segments)
    {
      var a = path[i];
      var b = path[i + 1];
      var abx = b.x - a.x;
      var abz = b.z - a.z;
      var lenSq = abx * abx + abz * abz;
      var rawT = lenSq > 0f ? ((pos.x - a.x) * abx + (pos.z - a.z) * abz) / lenSq : 0f;
      var ct = Mathf.Clamp01(rawT);
      var dx = a.x + abx * ct - pos.x;
      var dz = a.z + abz * ct - pos.z;
      var dSq = dx * dx + dz * dz;
      if (dSq >= best) continue;
      best = dSq;
      segment = i;
      t = rawT;
    }
    return Mathf.Sqrt(best);
  }
  public static float Distance(List<List<Vector3>> paths, Vector3 pos) => paths.Min(path => ClosestSegment(path, Enumerable.Range(0, path.Count - 1), pos, out _, out _));
}

public partial class Terrain
{
  public static TerrainComp[] GetCompilers(List<List<Vector3>> paths, float radius)
  {
    HashSet<Heightmap> heightMaps = [];
    List<Heightmap> found = [];
    // Heightmaps are 64 meters so checking every 16 meters is enough to find all of them.
    foreach (var path in paths)
    {
      for (var i = 0; i < path.Count; i++)
      {
        var steps = i < path.Count - 1 ? Mathf.Max(1, Mathf.CeilToInt(Utils.DistanceXZ(path[i], path[i + 1]) / 16f)) : 1;
        for (var k = 0; k < steps; k++)
        {
          var point = i < path.Count - 1 ? Vector3.Lerp(path[i], path[i + 1], (float)k / steps) : path[i];
          found.Clear();
          Heightmap.FindHeightmap(point, radius + 1, found);
          heightMaps.UnionWith(found);
        }
      }
    }
    var pos = ZNet.instance.GetReferencePosition();
    return heightMaps.Where(hmap => ZNetScene.InActiveArea(hmap.transform.position, pos)).Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToArray();
  }

  // Stacked paths are merged so that each node is only affected once (by the closest path).
  public static void GetNodesWithPath<T>(List<T> nodes, TerrainComp compiler, List<List<Vector3>> paths, Range<float> radius, bool flatEnds) where T : TerrainNode, new()
  {
    if (radius.Max == 0f) return;
    var hmap = compiler.m_hmap;
    var center = hmap.transform.position;
    var half = hmap.m_width * hmap.m_scale / 2f + radius.Max;
    var segmentsPerPath = paths.Select(path =>
    {
      List<int> segments = [];
      for (var i = 0; i < path.Count - 1; i++)
      {
        var a = path[i];
        var b = path[i + 1];
        if (Mathf.Max(a.x, b.x) < center.x - half || Mathf.Min(a.x, b.x) > center.x + half) continue;
        if (Mathf.Max(a.z, b.z) < center.z - half || Mathf.Min(a.z, b.z) > center.z + half) continue;
        segments.Add(i);
      }
      return segments;
    }).ToList();
    if (segmentsPerPath.All(s => s.Count == 0)) return;
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(hmap, x, z);
        var bestDistance = float.MaxValue;
        var bestHeight = 0f;
        for (var p = 0; p < paths.Count; p++)
        {
          if (segmentsPerPath[p].Count == 0) continue;
          var path = paths[p];
          var distance = TerrainPath.ClosestSegment(path, segmentsPerPath[p], nodePos, out var segment, out var t);
          if (distance >= bestDistance) continue;
          if (flatEnds && ((segment == 0 && t < 0f) || (segment == path.Count - 2 && t > 1f))) continue;
          bestDistance = distance;
          bestHeight = Mathf.Lerp(path[segment].y, path[segment + 1].y, Mathf.Clamp01(t));
        }
        if (bestDistance > radius.Max) continue;
        if (!Helper.Within(radius, bestDistance)) continue;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          Distance = bestDistance / radius.Max,
          Height = bestHeight,
          Compiler = compiler
        });
      }
    }
  }
}
