using FMOD;
using FMODUnity;
using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [JsonIgnore]public string Path;
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
public struct ChannelContainer
{
    public float Volume = -1;
    public SoundType SoundType;
    public Channel Channel { get; internal set; }
    internal ChannelContainer(float volume, SoundType soundType, Channel channel)
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
        using ListPool<int> toremove = new ListPool<int>();
        foreach (KeyValuePair<int, ChannelContainer> c in channels)
        {
            ChannelContainer C = c.Value;
            if (!UpdateChannel(C))
            {
                toremove.Add(c.Key);
            }
        }
        foreach (int i in toremove)
        {
            channels.Remove(i);
        }
    }
    /// <summary>
    /// Loads a custom sound from the wav library
    /// </summary>
    /// <param name="pSoundPath">the ID of the wav file, aka its file name</param>
    /// <returns>The ID of the channel that the sound is playing in, -1 if failed</returns>
    /// It can recognize jpg, png, jpeg by postfix now
    public static int LoadCustomSound(float pX, float pY, string pSoundPath)
    {
        WavContainer WAV = AudioWavLibrary[pSoundPath];
        float Volume = GetVolume(WAV.Volume, WAV.Type);
        if (Volume == 0)
        {
            return -1;
        }
        if (fmodSystem.createSound(WAV.Path, WAV._3D ? MODE.LOOP_NORMAL | MODE._3D : MODE.LOOP_NORMAL, out var sound) != RESULT.OK)
        {
            UnityEngine.Debug.Log($"Unable to play sound {pSoundPath}!");
            return -1;
        }
        sound.setLoopCount(WAV.LoopCount);
        fmodSystem.playSound(sound, masterChannelGroup, false, out Channel channel);
        channel.setVolume(Volume);
        AddChannel(channel, WAV.Volume, WAV.Type);
        SetChannelPosition(channel, pX, pY);
        return channels.Count-1;
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
    public static ChannelContainer GetChannel(int ID)
    {
        if (!channels.ContainsKey(ID))
        {
            //structs cant be null
            return new ChannelContainer();
        }
        return channels[ID];
    }
    internal static void AddChannel(Channel channel, float volume, SoundType soundType)
    {
        ChannelContainer Container = new ChannelContainer(volume, soundType, channel);
        channels.Add(NextIndex, Container);
        NextIndex++;
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
    static bool UpdateChannel(ChannelContainer channel)
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
            channel.Value.Channel.stop();
        }
        channels.Clear();
        NextIndex = 0;
    }
    public static void SetChannelPosition(Channel channel, float pX, float pY)
    {
        channel.get3DAttributes(out VECTOR Pos, out VECTOR vel);
        if (Pos.x != pX || Pos.y != pY)
        {
            VECTOR pos = new VECTOR() { x = pX, y = pY, z = 0 };
            channel.set3DAttributes(ref pos, ref vel);
        }
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
    private static int NextIndex = 0;
    internal static readonly Dictionary<string, WavContainer> AudioWavLibrary = new Dictionary<string, WavContainer>();
    static readonly Dictionary<int, ChannelContainer> channels = new Dictionary<int, ChannelContainer>();
}
