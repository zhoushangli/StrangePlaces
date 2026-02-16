using System;
using System.Text.Json;
using Godot;
using Godot.Collections;
using Protogame2D.Core;

namespace Protogame2D.Services;

/// <summary>
/// 配置服务，读取 default_config.json 并提供简单键值访问。
/// </summary>
public class ConfigService : IService
{
    private Dictionary<string, Variant> _data = new();

    public void Init()
    {
        const string path = "res://Data/default_config.json";
        if (!FileAccess.FileExists(path))
        {
            _data["version"] = 1;
            return;
        }

        try
        {
            var content = FileAccess.GetFileAsString(path);
            var doc = JsonDocument.Parse(content);
            LoadFromJsonElement(doc.RootElement, "");
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[ConfigService] Failed to load {path}: {ex}");
        }
    }

    private void LoadFromJsonElement(JsonElement el, string prefix)
    {
        if (el.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in el.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                LoadFromJsonElement(prop.Value, key);
            }
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            var arr = new Godot.Collections.Array<Variant>();
            foreach (var item in el.EnumerateArray())
            {
                arr.Add(JsonElementToVariant(item));
            }
            _data[prefix] = arr;
        }
        else
        {
            _data[prefix] = JsonElementToVariant(el);
        }
    }

    private static Variant JsonElementToVariant(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt32(out var i) ? i : (Variant)el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => default,
            _ => default
        };
    }

    public T Get<[MustBeVariant] T>(string key, T defaultValue)
    {
        if (!_data.TryGetValue(key, out var v))
        {
            return defaultValue;
        }

        try
        {
            if (v.VariantType == Variant.Type.Nil || v.VariantType == Variant.Type.Object)
            {
                return defaultValue;
            }
            return v.As<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    public void Shutdown() { }
}
