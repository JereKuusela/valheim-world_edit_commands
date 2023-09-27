using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace WorldEditCommands;

public class DataAutoComplete
{
  private static Dictionary<string, Dictionary<string, Type>>? fields;
  public static Dictionary<string, Dictionary<string, Type>> Fields => fields ??= LoadFields();
  private static List<string>? components;
  private static List<string> Components => components ??= [.. Fields.Keys];
  private static readonly HashSet<Type> ValidTypes = [
    typeof(string),
    typeof(int),
    typeof(float),
    typeof(bool),
    typeof(Vector3),
    typeof(GameObject)
  ];
  public static Dictionary<string, Dictionary<string, Type>> LoadFields()
  {
    Dictionary<string, Dictionary<string, Type>> fields = [];
    List<Assembly> assemblies = [Assembly.GetAssembly(typeof(ZNetView)), .. Chainloader.PluginInfos.Values.Where(p => p.Instance != null).Select(p => p.Instance.GetType().Assembly)];
    var assembly = Assembly.GetAssembly(typeof(ZNetView));
    var baseType = typeof(MonoBehaviour);
    var types = assemblies.SelectMany(s => s.GetTypes()).Where(baseType.IsAssignableFrom);

    foreach (var type in types)
    {
      if (!fields.ContainsKey(type.Name))
        fields[type.Name] = [];
      foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        if (!ValidTypes.Contains(fieldInfo.FieldType)) continue;
        fields[type.Name][fieldInfo.Name] = fieldInfo.FieldType;
      }
      if (fields[type.Name].Count == 0)
        fields.Remove(type.Name);
    }
    return fields;
  }
  public static List<string> GetComponents()
  {
    var prefab = PrefabFromCommand(GetInput());
    return GetComponents(prefab);
  }
  private static string GetInput()
  {
    Aliasing.RestoreAlias(Console.m_instance.m_input);
    var text = Aliasing.Plain(Console.m_instance.m_input.text);
    Aliasing.RemoveAlias(Console.m_instance.m_input);
    return text;
  }
  public static string PrefabFromCommand(string command)
  {
    var split = command.Split(' ');
    var arg = split.LastOrDefault(s => s.StartsWith("id=", StringComparison.Ordinal));
    var prefab = "";
    if (!string.IsNullOrEmpty(arg))
      prefab = arg.Split('=')[1];
    else if (split.Length > 1 && ZNetScene.instance.m_namedPrefabs.ContainsKey(split[1].GetStableHashCode()))
      prefab = split[1];
    else
    {
      var selection = Player.m_localPlayer?.GetHoverObject();
      if (selection)
        prefab = Utils.GetPrefabName(selection);
    }
    return prefab;
  }
  private static List<string> GetComponents(string prefab)
  {
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(prefab.GetStableHashCode(), out var gameObject))
      return gameObject.GetComponents<MonoBehaviour>().Select(c => c.GetType().Name).ToList();
    return Components;
  }
  public static List<string> GetFields()
  {
    var command = GetInput();
    var prefab = PrefabFromCommand(command);
    var component = ComponentFromCommand(prefab, command);
    return GetFields(component);
  }
  private static string ComponentFromCommand(string prefab, string command)
  {
    var split = command.Split(' ');
    var arg = split.LastOrDefault(s => s.StartsWith("field=", StringComparison.Ordinal) || s.StartsWith("f=", StringComparison.Ordinal));
    if (arg == null) return "";
    var component = arg.Split('=')[1].Split(',')[0];
    return RealComponent(prefab, component);
  }
  private static List<string> GetFields(string component)
  {
    return Fields.TryGetValue(component, out var fields) ? [.. fields.Keys.Select(s => s.Replace("m_", ""))] : [];
  }
  public static List<string> GetTypes(int index)
  {
    var command = GetInput();
    var prefab = PrefabFromCommand(command);
    var component = ComponentFromCommand(prefab, command);
    if (!Fields.TryGetValue(component, out var fields)) return [];
    var field = FieldFromCommand(component, command);
    if (!fields.TryGetValue(field, out var type)) return [];
    if (type == typeof(string)) return ["Text"];
    if (type == typeof(int)) return ["Number"];
    if (type == typeof(float)) return ["Decimal"];
    if (type == typeof(bool)) return ["true", "false"];
    if (type == typeof(Vector3)) return ServerDevcommands.ParameterInfo.XZY("Field", index);
    if (type == typeof(GameObject)) return ServerDevcommands.ParameterInfo.Ids;
    return [];
  }
  private static string FieldFromCommand(string component, string command)
  {
    var split = command.Split(' ');
    var arg = split.LastOrDefault(s => s.StartsWith("field=", StringComparison.Ordinal) || s.StartsWith("f=", StringComparison.Ordinal));
    if (arg == null) return "";
    split = arg.Split('=')[1].Split(',');
    return split.Length < 2 ? "" : RealField(component, split[1]);
  }
  public static Type GetType(string component, string field)
  {
    if (!Fields.TryGetValue(component, out var fields)) return typeof(void);
    if (!fields.TryGetValue(field, out var type)) return typeof(void);
    return type;
  }

  public static string RealComponent(string prefab, string component) => GetComponents(prefab).FirstOrDefault(s => s.StartsWith(component, StringComparison.OrdinalIgnoreCase)) ?? component;
  public static string RealField(string component, string field)
  {
    var f = $"m_{field}";
    var fields = Fields.TryGetValue(component, out var fs) ? fs.Keys.ToList() : [];
    var primaryField = fields.FirstOrDefault(s => s.Equals(f, StringComparison.OrdinalIgnoreCase));
    if (primaryField != null) return primaryField;
    var secondaryField = fields.FirstOrDefault(s => s.StartsWith(f, StringComparison.OrdinalIgnoreCase) || s.StartsWith(field, StringComparison.OrdinalIgnoreCase));
    if (secondaryField != null) return secondaryField;
    return field;
  }
}
