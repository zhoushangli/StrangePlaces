using System;
using System.Text.Json;
using Godot;
using Protogame2D.Core;
using Protogame2D.Data;

namespace Protogame2D.Services;

/// <summary>
/// 存档服务，JSON 保存到 user://save/。
/// </summary>
public class SaveService : IService
{
    private const string SaveDir = "user://save/";
    private const string SettingsPath = "user://save/settings.json";
    private const string ProfilePath = "user://save/profile.json";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true
    };

    public SettingsData Settings { get; private set; } = new();
    public ProfileData Profile { get; private set; } = new();

    public void Init()
    {
        LoadAll();
    }

    public void LoadAll()
    {
        EnsureSaveDir();
        LoadSettings();
        LoadProfile();
    }

    private static void EnsureSaveDir()
    {
        var dir = DirAccess.Open("user://");
        if (dir != null && !dir.DirExists("save"))
        {
            dir.MakeDir("save");
        }
    }

    private void LoadSettings()
    {
        try
        {
            if (!FileAccess.FileExists(SettingsPath))
            {
                Settings = new SettingsData();
                SaveSettings();
                return;
            }
            var json = FileAccess.GetFileAsString(SettingsPath);
            Settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[SaveService] LoadSettings failed: {ex}");
            Settings = new SettingsData();
            SaveSettings();
        }
    }

    private void LoadProfile()
    {
        try
        {
            if (!FileAccess.FileExists(ProfilePath))
            {
                Profile = new ProfileData();
                SaveProfile();
                return;
            }
            var json = FileAccess.GetFileAsString(ProfilePath);
            Profile = JsonSerializer.Deserialize<ProfileData>(json) ?? new ProfileData();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[SaveService] LoadProfile failed: {ex}");
            Profile = new ProfileData();
            SaveProfile();
        }
    }

    public void SaveSettings()
    {
        EnsureSaveDir();
        try
        {
            var json = JsonSerializer.Serialize(Settings, JsonOpts);
            var fs = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Write);
            if (fs != null)
            {
                fs.StoreString(json);
                fs.Close();
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"[SaveService] SaveSettings failed: {ex}");
        }
    }

    public void SaveProfile()
    {
        EnsureSaveDir();
        try
        {
            var json = JsonSerializer.Serialize(Profile, JsonOpts);
            var fs = FileAccess.Open(ProfilePath, FileAccess.ModeFlags.Write);
            if (fs != null)
            {
                fs.StoreString(json);
                fs.Close();
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"[SaveService] SaveProfile failed: {ex}");
        }
    }

    public void Shutdown() { }
}
