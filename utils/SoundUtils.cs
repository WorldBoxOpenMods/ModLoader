using FMOD;
using FMODUnity;
using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoModLoader.utils;
public enum SoundType
{
    Music,
    Sound,
}
internal struct WavContainer
{
    public string Path;
    [JsonProperty("3D")] public bool _3D;
    public float Volume;
    public SoundType Type;
    public int LoopCount;
    public WavContainer(string Path, bool _3D, float Volume, int LoopCount = 0, SoundType Type = SoundType.Sound)
    {
        this.Path = Path;
        this._3D = _3D;
        this.Volume = Volume;
        this.Type = Type;
        this.LoopCount = LoopCount;
    }
}
internal struct ChannelContainer
{
    public float Volume;
    public SoundType SoundType;
    public Channel Channel;
    public ChannelContainer(float volume, SoundType soundType, Channel channel)
    {
        Volume = volume;
        SoundType = soundType;
        Channel = channel;
    }
}
public class CustomAudioManager
{
    static FMOD.System fmodSystem;
    static ChannelGroup masterChannelGroup;
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RuntimeManager), "Update")]
    static void Update()
    {
        for (int i = 0; i < channels.Count; i++)
        {
            ChannelContainer c = channels[i];
            if (!UpdateChannel(c))
            {
                channels.Remove(c);
                i--;
            }
        }
    }
    public static void LoadCustomSound(float pX, float pY, string pSoundPath)
    {
        WavContainer WAV = AudioWavLibrary[pSoundPath];
        float Volume = GetVolume(WAV.Volume, WAV.Type);
        if (Volume == 0)
        {
            return;
        }
        if (fmodSystem.createSound(WAV.Path, WAV._3D ? MODE.LOOP_NORMAL | MODE._3D : MODE.LOOP_NORMAL, out var sound) != RESULT.OK)
        {
            UnityEngine.Debug.Log($"Unable to play sound {pSoundPath}!");
            return;
        }
        sound.setLoopCount(WAV.LoopCount);
        fmodSystem.playSound(sound, masterChannelGroup, false, out Channel channel);
        channel.setVolume(Volume);
        AddChannel(channel, WAV.Volume, WAV.Type);
        SetChannelPosition(channel, pX, pY);
    }
    internal static void Initialize()
    {
        if (RuntimeManager.StudioSystem.getCoreSystem(out fmodSystem) != RESULT.OK)
        {
            LogService.LogError("Failed to initialize FMOD Core System!");
            return;
        }

        if (fmodSystem.getMasterChannelGroup(out masterChannelGroup) != RESULT.OK)
        {
            LogService.LogError("Failed to retrieve master channel group!");
        }
    }
    internal static void AddChannel(Channel channel, float volume, SoundType soundType)
    {
        ChannelContainer Container = new ChannelContainer(volume, soundType, channel);
        channels.Add(Container);
    }
    /// <summary>
    ///    Allows the Modder to modify the data of the wav file at runtime
    /// </summary>
    public static void ModifyWavData(string ID, float Volume, bool _3D, int LoopCount = 0, SoundType Type = SoundType.Sound)
    {
        if (!AudioWavLibrary.ContainsKey(ID))
        {
            return;
        }
        AudioWavLibrary[ID] = new WavContainer(AudioWavLibrary[ID].Path, _3D, Volume, LoopCount, Type);
    }
    internal static bool UpdateChannel(ChannelContainer channel)
    {
        channel.Channel.isPlaying(out bool isPlaying);
        if (!isPlaying)
        {
            return false;
        }
        channel.Channel.setVolume(GetVolume(channel.Volume, channel.SoundType));
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapBox), nameof(MapBox.clearWorld))]
    public static void ClearAllCustomSounds()
    {
        foreach (var channel in channels)
        {
            channel.Channel.stop();
        }
        channels.Clear();
    }
    public static void SetChannelPosition(Channel channel, float pX, float pY)
    {
        VECTOR vel = new VECTOR() { x = 0, y = 0, z = 0 };
        VECTOR pos = new VECTOR() { x = pX, y = pY, z = 0 };
        channel.set3DAttributes(ref pos, ref vel);
    }
    public static float GetVolume(float Volume, SoundType soundType)
    {
        if (soundType == SoundType.Music)
        {
            Volume *= PlayerConfig.getIntValue("volume_music") / 100f;
        }
        else
        {
            Volume *= PlayerConfig.getIntValue("volume_sound_effects") / 100f;
        }
        Volume *= PlayerConfig.getIntValue("volume_master_sound") / 100f;
        return Mathf.Clamp01(Volume/100);
    }
    internal static readonly Dictionary<string, WavContainer> AudioWavLibrary = new Dictionary<string, WavContainer>();
    static List<ChannelContainer> channels = new List<ChannelContainer>();
}
