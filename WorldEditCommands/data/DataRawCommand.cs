using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class DataRawCommand
{

  public DataRawCommand()
  {

    AutoComplete.Register("data_raw", (int index) => index == 0 ? DataLoading.DataKeys : ParameterInfo.None);
    Helper.Command("data_raw", "[name] - Copies data entry to clipboard as base64 encoded string.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing data entry name.");
      if (!DataLoading.Data.TryGetValue(args[1].GetStableHashCode(), out var zdo))
        throw new InvalidOperationException($"Data entry {args[1]} not found.");
      var pars = ParseCommand(args);
      var str = zdo.GetBase64(pars);
      if (str == "AAAAAA==")
        throw new InvalidOperationException($"Data entry {args[1]} is empty.");
      GUIUtility.systemCopyBuffer = str;
      Helper.AddMessage(args.Context, $"Data entry {args[1]} copied to clipboard.");
    });
  }

  private static Dictionary<string, string> ParseCommand(Terminal.ConsoleEventArgs args)
  {
    Dictionary<string, string> parameters = [];
    foreach (var arg in args.Args)
    {
      var split = arg.Split(['='], 2);
      if (split.Length < 2) continue;
      var name = split[0].ToLower();
      if (name == "par")
      {
        var kvp = Parse.Kvp(split[1]);
        if (kvp.Key == "") throw new InvalidOperationException($"Invalid data parameter {split[1]}.");
        parameters[kvp.Key] = kvp.Value;
      }
    }
    return parameters;
  }
  public static void LegacySerialize(ZDO zdo, ZPackage pkg, string filter)
  {
    var id = zdo.m_uid;
    var vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var byteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    if (filter == "")
    {
      vecs.Remove(Hash.Scale);
      vecs.Remove(Hash.SpawnPoint);
      ints.Remove(Hash.Seed);
      ints.Remove(Hash.Location);
      if (strings.ContainsKey(Hash.OverrideItems))
      {
        ints.Remove(Hash.AddedDefaultItems);
        strings.Remove(Hash.Items);
      }
    }
    else if (filter != "all")
    {
      var filters = Parse.Split(filter).Select(s => s.GetStableHashCode()).ToHashSet();
      vecs = FilterZdo(vecs, filters);
      quats = FilterZdo(quats, filters);
      floats = FilterZdo(floats, filters);
      ints = FilterZdo(ints, filters);
      longs = FilterZdo(longs, filters);
      byteArrays = FilterZdo(byteArrays, filters);
    }
    var num = 0;
    if (floats.Count() > 0)
      num |= 1;
    if (vecs.Count() > 0)
      num |= 2;
    if (quats.Count() > 0)
      num |= 4;
    if (ints.Count() > 0)
      num |= 8;
    if (strings.Count() > 0)
      num |= 16;
    if (longs.Count() > 0)
      num |= 64;
    if (byteArrays.Count() > 0)
      num |= 128;
    var conn = ZDOExtraData.s_connectionsHashData.TryGetValue(id, out var c) ? c : null;
    if (conn != null && filter == "all" && conn.m_type != ZDOExtraData.ConnectionType.None && conn.m_hash != 0)
      num |= 256;

    pkg.Write(num);
    if (floats.Count() > 0)
    {
      pkg.Write((byte)floats.Count());
      foreach (var kvp in floats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (vecs.Count() > 0)
    {
      pkg.Write((byte)vecs.Count());
      foreach (var kvp in vecs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (quats.Count() > 0)
    {
      pkg.Write((byte)quats.Count());
      foreach (var kvp in quats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (ints.Count() > 0)
    {
      pkg.Write((byte)ints.Count());
      foreach (var kvp in ints)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (longs.Count() > 0)
    {
      pkg.Write((byte)longs.Count());
      foreach (var kvp in longs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (strings.Count() > 0)
    {
      pkg.Write((byte)strings.Count());
      foreach (var kvp in strings)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (byteArrays.Count() > 0)
    {
      pkg.Write((byte)byteArrays.Count());
      foreach (var kvp in byteArrays)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (conn != null && (num & 256) != 0)
    {
      pkg.Write((byte)conn.m_type);
      pkg.Write(conn.m_hash);
    }
  }
  private static Dictionary<int, T> FilterZdo<T>(Dictionary<int, T> dict, HashSet<int> filters)
  {
    return dict.Where(kvp => filters.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, pair => pair.Value);
  }
}
