using FMOD;
using FMODUnity;
using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoModLoader.utils;
/// <summary>
/// The Type used for which sound group this sound goes into, UI, Music, and SFX which control its volume
/// </summary>
public enum SoundType
{
    /// <summary>
    /// Apart of the Music Group
    /// </summary>
    Music,
    /// <summary>
    /// Apart of the Sound Group (SFX)
    /// </summary>
    Sound,
    /// <summary>
    /// Apart of the UI Group
    /// </summary>
    UI
}
/// <summary>
/// The Mode which controls if and how the volume changes depending on your distance to it
/// </summary>
public enum SoundMode
{
    /// <summary>
    /// 2D sound, volume doesnt change
    /// </summary>
    Basic,
    /// <summary>
    /// 3D sound (volume changes depending on distance) which uses 2 audio channels, is quiet
    /// </summary>
    Stereo3D,
    /// <summary>
    /// 3D sound which uses 1 channel, can be loud
    /// </summary>
    Mono3D
}
internal struct WavContainer
{
    [JsonIgnore]public string Path;
    public SoundMode Mode;
    public float Volume;
    public SoundType Type;
    public int LoopCount;
    public bool Ramp;
    public WavContainer(string Path, SoundMode Mode, float Volume, int LoopCount = 0, bool Ramp = false, SoundType Type = SoundType.Sound)
    {
        this.Ramp = Ramp;
        this.Path = Path;
        this.Mode = Mode;
        this.Volume = Volume;
        this.Type = Type;
        this.LoopCount = LoopCount;
    }
}
/// <summary>
/// A container to manage the Sound
/// </summary>
public struct ChannelContainer
{
    /// <summary>
    /// A FMOD Audio Channel which plays the sound
    /// </summary>
    public Channel Channel { get; internal set; }
    /// <summary>
    /// to be used for Mono3D
    /// </summary>
    /// <remarks>
    /// X and Y represent position, Z represents volume
    /// </remarks>
    public Vector3 PosAndVolume = default;
    /// <summary>
    /// The Transform or gameobject which this Sound is attached two. sounds whose mode is BASIC must not use this
    /// </summary>
    public Transform AttachedTo;
    internal ChannelContainer(Channel channel, Transform attachedTo = null, Vector3 PosAndVolume = default)
    {
        Channel = channel;
        this.PosAndVolume = PosAndVolume;
        AttachedTo = attachedTo;
    }
    /// <summary>
    /// returns true if the channel has stopped playing or something wrong happened
    /// </summary>
    public readonly bool Finushed
    {
        get { return Channel.isPlaying(out bool IsPlaying) != RESULT.OK || !IsPlaying; }
    }
}
/// <summary>
/// A Manager for managing your custom sounds!
/// </summary>
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
        for (int i =0; i < Channels.Count; i++)
        {
            ChannelContainer C = Channels[i];
            if (!UpdateChannel(C))
            {
                Channels.Remove(C);
                i--;
            }
        }
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MusicBox), nameof(MusicBox.playSound), typeof(string), typeof(float), typeof(float),
    typeof(bool), typeof(bool))]
    [HarmonyPriority(Priority.Last)]
    static bool PlaySoundPatch(string pSoundPath, float pX, float pY, bool pGameViewOnly)
    {
        if (!MusicBox.sounds_on)
        {
            return true;
        }
        if (pGameViewOnly && World.world.quality_changer.isLowRes())
        {
            return true;
        }
        pSoundPath = pSoundPath.ToLower();
        if (!AudioWavLibrary.ContainsKey(pSoundPath)) return true;
        LoadCustomSound(pSoundPath, pX, pY);
        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MusicBox), nameof(MusicBox.playDrawingSound))]
    [HarmonyPriority(Priority.Last)]
    static bool PlayDrawingSoundPatch(string pSoundPath, float pX, float pY)
    {
        if (!MusicBox.sounds_on)
        {
            return true;
        }
        pSoundPath = pSoundPath.ToLower();
        if (!AudioWavLibrary.ContainsKey(pSoundPath)) return true;
        LoadDrawingSound(pSoundPath, pX, pY);
        return false;
    }
    /// <summary>
    /// plays a sound at a location unless another sound with the same path is playing, then the other sound is set to that position 
    /// </summary>
    public static ChannelContainer LoadDrawingSound(string pSoundPath, float pX, float pY)
    {
        if(DrawingSounds.TryGetValue(pSoundPath, out ChannelContainer container) && !container.Finushed)
        {
            SetChannelPosition(container, pX, pY);
        }
        else
        {
            DrawingSounds.Remove(pSoundPath);
            container = LoadCustomSound(pSoundPath, pX, pY);
            DrawingSounds.Add(pSoundPath, container);
        }
        return container;
    }
    /// <summary>
    /// Loads a custom sound from the wav library
    /// </summary>
    /// <param name="pX">the X position</param>
    /// <param name="pY">the Y position</param>
    /// <param name="WAVName">the ID of the wav file, aka its file name</param>
    /// <param name="AttachedTo">The transform to attach the sound to</param>
    /// <returns>The ID of the channel that the sound is playing in, default if failed</returns>
    public static ChannelContainer LoadCustomSound(string WAVName, float pX, float pY, Transform AttachedTo = null)
    {
        WavContainer WAV = AudioWavLibrary[WAVName];
        if(WAV.Mode == SoundMode.Basic)
        {
            AttachedTo = null;
        }
        if (fmodSystem.createSound(WAV.Path, WAV.Mode == SoundMode.Stereo3D ? MODE.LOOP_NORMAL | MODE._3D : MODE.LOOP_NORMAL, out var sound) != RESULT.OK)
        {
            LogService.LogError($"Unable to play sound {WAVName}!");
            return default;
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
        AddChannel(channel, AttachedTo, WAV.Mode == SoundMode.Mono3D ? new Vector3(pX, pY, WAV.Volume) : default);
        if (WAV.Mode == SoundMode.Stereo3D) {
            SetChannelPosition(channel, pX, pY);
        }
        return Channels[Channels.Count-1];
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
    internal static void AddChannel(Channel channel, Transform AttachedTo = null, Vector3 PosAndVolume = default)
    {
        Channels.Add(new(channel, AttachedTo, PosAndVolume));
    }
    /// <summary>
    ///    Allows the Modder to modify the data of the wav file at runtime
    /// </summary>
    public static void ModifyWavData(string ID, float Volume, SoundMode Mode, int LoopCount = 0, bool Ramp = false, SoundType Type = SoundType.Sound)
    {
        ID = ID.ToLower();
        if (!AudioWavLibrary.ContainsKey(ID))
        {
            return;
        }
        AudioWavLibrary[ID] = new WavContainer(AudioWavLibrary[ID].Path, Mode, Volume, LoopCount, Ramp, Type);
    }
    static bool UpdateChannel(ChannelContainer channel)
    {
        if (channel.Finushed)
        {
            return false;
        }
        if (channel.AttachedTo != null)
        {
            SetChannelPosition(channel, channel.AttachedTo.position.x, channel.AttachedTo.position.y);
        }
        if(channel.PosAndVolume != default)
        {
            UpdateMonoVolume(channel);
        }
        return true;
    }
    static void UpdateMonoVolume(ChannelContainer Channel)
    {
        Vector3 CameraPos = Camera.main.transform.position;
        float Distance = Vector3.Distance(new Vector3(CameraPos.x, CameraPos.y, Camera.main.orthographicSize), new Vector2(Channel.PosAndVolume.x, Channel.PosAndVolume.y));
        Channel.Channel.setVolume(Mathf.Clamp01(Channel.PosAndVolume.z/Distance));
    }
    /// <summary>
    /// Clears All Custom Sounds
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapBox), nameof(MapBox.clearWorld))]
    public static void ClearAllCustomSounds()
    {
        foreach (var channel in Channels)
        {
            channel.Channel.stop();
        }
        Channels.Clear();
    }
    /// <summary>
    /// Sets the position of a channel, channel must be 3D
    /// </summary>
    public static void SetChannelPosition(ChannelContainer channel, float pX, float pY)
    {
        if (channel.PosAndVolume == default)
        {
            SetChannelPosition(channel.Channel, pX, pY);
        }
        else
        {
            channel.PosAndVolume.x = pX;
            channel.PosAndVolume.y = pY;
        }
    }
    /// <summary>
    /// Sets the position of a channel, channel must be Stereo-3D
    /// </summary>
    public static void SetChannelPosition(Channel channel, float pX, float pY)
    {
        channel.get3DAttributes(out VECTOR Pos, out VECTOR vel);
        if (Pos.x != pX || Pos.y != pY)
        {
            VECTOR pos = new() { x = pX, y = pY, z = 0 };
            channel.set3DAttributes(ref pos, ref vel);
        }
    }
    static float GetVolume(SoundType soundType)
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
    internal static readonly Dictionary<string, WavContainer> AudioWavLibrary = new();
    static readonly List<ChannelContainer> Channels = new();
    static readonly Dictionary<string, ChannelContainer> DrawingSounds = new();
}