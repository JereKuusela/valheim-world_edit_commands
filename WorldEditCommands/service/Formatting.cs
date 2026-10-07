using System.Globalization;
using UnityEngine;

namespace Service;

public static class Formatting
{
  public static string Format(float value) => value.ToString("0.#####", NumberFormatInfo.InvariantInfo);
  public static string Format(double value) => value.ToString("0.#####", NumberFormatInfo.InvariantInfo);
  // Valheim position order is x,z,y in user facing text.
  public static string FormatPos(Vector3 value) => $"{Format(value.x)},{Format(value.z)},{Format(value.y)}";
}
