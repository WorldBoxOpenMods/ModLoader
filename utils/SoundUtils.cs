using FMOD;
using FMODUnity;
using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using UnityEngine;

namespace NeoModLoader.utils;
public enum SoundType
{
    Music,
    Sound,
    UI
}
internal struct WavContainer
{
    [JsonIgnore]public string Path;
    [JsonProperty("3D")] public bool _3D;
    public float Volume;
    public SoundType Type;
    public int LoopCount;
    public bool Ramp;
    public WavContainer(string Path, bool _3D, float Volume, int LoopCount = 0, bool Ramp = false, SoundType Type = SoundType.Sound)
    {
        this.Ramp = Ramp;
        this.Path = Path;
        this._3D = _3D;
        this.Volume = Volume;
        this.Type = Type;
        this.LoopCount = LoopCount;
    }
}
public struct ChannelContainer
{
    public Channel Channel { get; internal set; }
    public Transform AttachedTo;
    internal ChannelContainer(Channel channel, Transform attachedTo = null)
    {
        Channel = channel;
        AttachedTo = attachedTo;
    }
}
public class CustomAudioManager
{
    static FMOD.System fmodSystem;
    static ChannelGroup SFXGroup;
    static ChannelGroup MusicGroup;
    static ChannelGroup UIGroup;
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RuntimeManager), "Update")]
    static void Update()
    {
        SFXGroup.setVolume(GetVolume(SoundType.Sound));
        MusicGroup.setVolume(GetVolume(SoundType.Music));
        UIGroup.setVolume(GetVolume(SoundType.UI));
        for (int i =0; i < channels.Count; i++)
        {
            ChannelContainer C = channels[i];
            if (!UpdateChannel(C))
            {
                channels.Remove(C);
                i--;
            }
        }
    }
    /// <summary>
    /// Loads a custom sound from the wav library
    /// </summary>
    /// <param name="pSoundPath">the ID of the wav file, aka its file name</param>
    /// <param name="AttachedTo">The transform to attach the sound to</param>
    /// <returns>The ID of the channel that the sound is playing in, -1 if failed</returns>
    public static int LoadCustomSound(float pX, float pY, string pSoundPath, Transform AttachedTo = null)
    {
        WavContainer WAV = AudioWavLibrary[pSoundPath];
        if (fmodSystem.createSound(WAV.Path, WAV._3D ? MODE.LOOP_NORMAL | MODE._3D : MODE.LOOP_NORMAL, out var sound) != RESULT.OK)
        {
            UnityEngine.Debug.Log($"Unable to play sound {pSoundPath}!");
            return -1;
        }
        sound.setLoopCount(WAV.LoopCount);
        Channel channel = default;
        switch (WAV.Type) {
            case SoundType.Music: fmodSystem.playSound(sound, MusicGroup, false, out channel); break;
            case SoundType.Sound: fmodSystem.playSound(sound, SFXGroup, false, out channel); break;
            case SoundType.UI: fmodSystem.playSound(sound, UIGroup, false, out channel); break;
        }
        channel.setVolumeRamp(WAV.Ramp);
        channel.setVolume(WAV.Volume/100);
        AddChannel(channel, AttachedTo);
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
        if(fmodSystem.createChannelGroup("SFXGroup", out SFXGroup) != RESULT.OK)
        {
            LogService.LogError("Failed to create SFXGroup!");
        }
        if(fmodSystem.createChannelGroup("MusicGroup", out MusicGroup) != RESULT.OK)
        {
            LogService.LogError("Failed to create MusicGroup!");
        }
        if(fmodSystem.createChannelGroup("UIGroup", out UIGroup) != RESULT.OK)
        {
            LogService.LogError("Failed to create UIGroup!");
        }
    }
    internal static void AddChannel(Channel channel, Transform AttachedTo = null)
    {
        ChannelContainer Container = new ChannelContainer(channel, AttachedTo);
        channels.Add(Container);
    }
    /// <summary>
    ///    Allows the Modder to modify the data of the wav file at runtime
    /// </summary>
    public static void ModifyWavData(string ID, float Volume, bool _3D, int LoopCount = 0, bool Ramp = false, SoundType Type = SoundType.Sound)
    {
        if (!AudioWavLibrary.ContainsKey(ID))
        {
            return;
        }
        AudioWavLibrary[ID] = new WavContainer(AudioWavLibrary[ID].Path, _3D, Volume, LoopCount, Ramp, Type);
    }
    static bool UpdateChannel(ChannelContainer channel)
    {
        channel.Channel.isPlaying(out bool isPlaying);
        if (!isPlaying)
        {
            return false;
        }
        if (channel.AttachedTo != null)
        {
            SetChannelPosition(channel.Channel, channel.AttachedTo.position.x, channel.AttachedTo.position.y);
        }
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
        channel.get3DAttributes(out VECTOR Pos, out VECTOR vel);
        if (Pos.x != pX || Pos.y != pY)
        {
            VECTOR pos = new VECTOR() { x = pX, y = pY, z = 0 };
            channel.set3DAttributes(ref pos, ref vel);
        }
    }
    public static float GetVolume(SoundType soundType)
    {
        float Volume = 1;
        if (soundType == SoundType.Music)
        {
            Volume *= PlayerConfig.getIntValue("volume_music") / 100f;
        }
        else if(soundType == SoundType.Sound)
        {
            Volume *= PlayerConfig.getIntValue("volume_sound_effects") / 100f;
        }
        else
        {
            Volume *= PlayerConfig.getIntValue("volume_ui") / 100f;
        }
        Volume *= PlayerConfig.getIntValue("volume_master_sound") / 100f;
        return Volume;
    }
    internal static readonly Dictionary<string, WavContainer> AudioWavLibrary = new Dictionary<string, WavContainer>();
    static readonly List<ChannelContainer> channels = new List<ChannelContainer>();
    public static ReadOnlyCollection<ChannelContainer> ChannelList { get { return channels.AsReadOnly(); } }
}
