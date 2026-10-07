using UnityEngine;

namespace Service;

public static class FloatCompare
{
  public static bool Approx(float a, float b) => Mathf.Abs(a - b) < 0.001f;
  public static bool ApproxBetween(float a, float min, float max) => min - 0.001f <= a && a <= max + 0.001f;
}
