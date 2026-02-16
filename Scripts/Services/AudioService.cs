using System.Collections.Generic;
using Godot;
using Protogame2D.Core;

namespace Protogame2D.Services;

/// <summary>
/// 音频服务。需要 Audio Bus: Master, BGM, SFX。
/// </summary>
public class AudioService : IService
{
    private const int SfxPoolSize = 8;
    private const string BgmBusName = "BGM";
    private const string SfxBusName = "SFX";

    private Node _audioRoot;
    private AudioStreamPlayer _bgmPlayer;
    private readonly List<AudioStreamPlayer> _sfxPool = new();
    private int _sfxIndex;

    private float _bgmVolume01 = 1f;
    private float _sfxVolume01 = 1f;

    public void Init()
    {
        EnsureAudioBuses();

        _audioRoot = new Node { Name = "AudioRoot" };

        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree?.Root != null)
        {
            tree.Root.AddChild(_audioRoot);
        }

        _bgmPlayer = new AudioStreamPlayer { Bus = BgmBusName };
        _audioRoot.AddChild(_bgmPlayer);

        for (var i = 0; i < SfxPoolSize; i++)
        {
            var p = new AudioStreamPlayer { Bus = SfxBusName };
            _audioRoot.AddChild(p);
            _sfxPool.Add(p);
        }

        if (App.Instance.TryGet<SaveService>(out var save))
        {
            _bgmVolume01 = save.Settings.BgmVolume01;
            _sfxVolume01 = save.Settings.SfxVolume01;
        }
    }

    private static void EnsureAudioBuses()
    {
        if (AudioServer.GetBusIndex(BgmBusName) < 0)
        {
            AudioServer.AddBus(-1);
            AudioServer.SetBusName(AudioServer.BusCount - 1, BgmBusName);
        }
        if (AudioServer.GetBusIndex(SfxBusName) < 0)
        {
            AudioServer.AddBus(-1);
            AudioServer.SetBusName(AudioServer.BusCount - 1, SfxBusName);
        }
    }

    private static float LinearToDb(float linear)
    {
        if (linear <= 0f) return -80f;
        return Mathf.LinearToDb(linear);
    }

    public void PlayBgm(string path, float fadeIn = 0.5f)
    {
        var stream = GD.Load<AudioStream>(path);
        if (stream == null)
        {
            GD.PushWarning($"[AudioService] BGM not found: {path}");
            return;
        }

        _bgmPlayer.Stream = stream;
        _bgmPlayer.VolumeDb = LinearToDb(0f);
        _bgmPlayer.Play();

        var tween = _bgmPlayer.CreateTween();
        tween.TweenProperty(_bgmPlayer, "volume_db", LinearToDb(_bgmVolume01), fadeIn)
            .From(LinearToDb(0f));
    }

    public void StopBgm(float fadeOut = 0.5f)
    {
        var tween = _bgmPlayer.CreateTween();
        tween.TweenProperty(_bgmPlayer, "volume_db", LinearToDb(0f), fadeOut);
        tween.TweenCallback(Callable.From(() => _bgmPlayer.Stop()));
    }

    public void PlaySfx(string path, float volumeDb = 0f)
    {
        var stream = GD.Load<AudioStream>(path);
        if (stream == null)
        {
            GD.PushWarning($"[AudioService] SFX not found: {path}");
            return;
        }

        var p = _sfxPool[_sfxIndex % SfxPoolSize];
        _sfxIndex++;

        p.Stream = stream;
        p.VolumeDb = LinearToDb(_sfxVolume01) + volumeDb;
        p.Play();
    }

    public void SetBgmVolume01(float v01)
    {
        _bgmVolume01 = Mathf.Clamp(v01, 0f, 1f);
        if (_bgmPlayer?.Playing == true)
        {
            _bgmPlayer.VolumeDb = LinearToDb(_bgmVolume01);
        }
    }

    public void SetSfxVolume01(float v01)
    {
        _sfxVolume01 = Mathf.Clamp(v01, 0f, 1f);
    }

    public void Shutdown()
    {
        _bgmPlayer?.Stop();
        _audioRoot?.QueueFree();
    }
}
