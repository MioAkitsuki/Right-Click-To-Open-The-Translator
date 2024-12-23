using System;
using System.Collections;
using System.Collections.Generic;
using DataSystem;
using Puzzle;
using QFramework;
using SceneControl;
using Settings;
using Translator;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMng : MonoSingleton<AudioMng>
{
    private readonly char[] randTab = new char[] { '0', '2', '1', '1', '0', '1', '3', '2', '1', '1', '3', '0', '3', '0', '2' };
    private int pRandTab = 0;
    bool skipFirstEnter = true;

    public static string isPlayingName = null;

    AudioSource ambientChannel;
    AudioSource current;
    AudioSource BGM1;
    AudioSource BGM2;
    [SerializeField] float fadeTime = 60;
    private Dictionary<string, AudioClip> backGroundMusics = new();

    //!!This DONOT work for WebGL.
    public AudioMixer audioMixer;
    private AudioMixerSnapshot[] audioMixerSnapshots = new AudioMixerSnapshot[2];

    private float m_backgroundVolume;
    public float ambientVolume { get; private set; } = 0.8f;
    public float effectVolume { get; private set; }
    private float currentVolScale = 1f;

    ResLoader res;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        ResKit.Init();

        ambientChannel = transform.Find("Ambient").GetComponent<AudioSource>();
        BGM1 = transform.Find("Music1").GetComponent<AudioSource>();
        BGM2 = transform.Find("Music2").GetComponent<AudioSource>();
        current = BGM1;

        UpdateVolume();

        audioMixerSnapshots[0] = audioMixer.FindSnapshot("SnapshotOrigin");
        audioMixerSnapshots[1] = audioMixer.FindSnapshot("SnapshotPuzzle");

        TypeEventSystem.Global.Register<OnVolumeSettingsChanged>(e => UpdateVolume()).UnRegisterWhenGameObjectDestroyed(gameObject);
        TypeEventSystem.Global.Register<OnPuzzleInitializedEvent>(e => OnPuzzleInitialize()).UnRegisterWhenGameObjectDestroyed(gameObject);
        TypeEventSystem.Global.Register<OnPuzzleExitEvent>(e => OnPuzzleExit()).UnRegisterWhenGameObjectDestroyed(gameObject);
        TypeEventSystem.Global.Register<OnSceneControlDeactivatedEvent>(e => LoadSceneAudioAssets()).UnRegisterWhenGameObjectDestroyed(gameObject);
        TypeEventSystem.Global.Register<OnSceneControlActivatedEvent>(e => StopAll()).UnRegisterWhenGameObjectDestroyed(gameObject);
        TypeEventSystem.Global.Register<OnInventoryIncreasedEvent>(e => AudioKit.PlaySound("023PickupItem")).UnRegisterWhenGameObjectDestroyed(gameObject);

        res = ResLoader.Allocate();
        if (PlayerPrefs.HasKey("Played") && PlayerPrefs.GetInt("Played") == 1)
        {
            AudioClip audioClip = res.LoadSync<AudioClip>("AmbientResearcher");
            ambientChannel.clip = audioClip;
        }
        else
        {
            AudioClip audioClip = res.LoadSync<AudioClip>("AmbientRain");
            ambientChannel.clip = audioClip;
        }
        BGM2.volume = 0;
    }

    private void UpdateVolume()
    {
        m_backgroundVolume = PlayerPrefs.HasKey("Background Volume") ? PlayerPrefs.GetFloat("Background Volume") : 0.8f;
        effectVolume = PlayerPrefs.HasKey("Effect Volume") ? PlayerPrefs.GetFloat("Effect Volume") : 0.8f;
        ambientVolume = PlayerPrefs.HasKey("Ambient Volume") ? PlayerPrefs.GetFloat("Ambient Volume") : 1.0f;
    }
    private void LoadSceneAudioAssets()
    {
        StopAll();
        try
        {
            GameObject audioAssets = GameObject.Find("AudioContainer");
            if (audioAssets == null) return;

            AudioContainer audioContainer = audioAssets.GetComponent<AudioContainer>();
            Instance.ambientChannel.clip = audioContainer.ambient;
            Instance.backGroundMusics = audioContainer.keyValuePairs;
            PlayAmbient();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e + "Audio Container Missing");
        }

    }
    public void PlayAmbient()
    {
        if (ambientChannel.clip.IsNull()) PlayAmbient("AmbientResearcher");
        ambientChannel.loop = true;
        ambientChannel.volume = ambientVolume;
        ambientChannel.Play();
    }
    public void PlayAmbient(string name)
    {
        if (ambientChannel.clip.name.IsNull() || name != ambientChannel.clip.name)
        {
            AudioClip audioClip = res.LoadSync<AudioClip>(name);
            ambientChannel.clip = audioClip;
        }
        PlayAmbient();
    }

    public void PlayBGM(string name)
    {
        Debug.Log("Try Play");
        TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
        ChangeBGM(name);
        TimeSpan end = new TimeSpan(DateTime.Now.Ticks);    //获取当前时间的刻度数
        TimeSpan abs = end.Subtract(start).Duration();      //时间差的绝对值
        Debug.Log(string.Format("程序执行时间：{0}", abs.TotalMilliseconds));
    }
    public void PlayFootsteps()
    {
        AudioKit.PlaySound("ftstp-1-" + randTab[pRandTab++], volume: 0.6f);
        if (pRandTab == 15) pRandTab = 0;
    }
    /// <summary>
    /// Play sfx when a UI button is pressed.
    /// 0: Click; 1: Apply; 2:Cancel
    /// </summary>
    /// <param name="type">the type of button that change the sfx it plays. 0: click; 1: Apply; 2:Cancel</param>
    public static void PlayBtnPressed(int type, float volumeScale = 0.8f)
    {
        if (type == 0)
        {
            AudioKit.PlaySound("click", volume: Instance.effectVolume * volumeScale);
        }
        else if (type == 1)
        {
            AudioKit.PlaySound("apply", volume: Instance.effectVolume * volumeScale);
        }
        else if (type == 2)
        {
            AudioKit.PlaySound("cancel", volume: Instance.effectVolume * volumeScale);
        }
    }

    public void PlayTranslatorSFX(bool isEnter)
    {
        if (skipFirstEnter)
        {
            skipFirstEnter = false;
            return;
        }
        if (isEnter)
        {
            AudioKit.PlaySound("TranslatorOn", volume: AudioMng.Instance.effectVolume * 0.8f);
        }
        else
        {
            AudioKit.PlaySound("TranslatorOff", volume: AudioMng.Instance.effectVolume * 0.8f);
        }
    }

    public void D_PlayBGM(string name)
    {
        if (backGroundMusics.TryGetValue(name, out var audioClip))
        {
            current.clip = audioClip;
            current.Play();
        }
        else
        {
            LoadBGM(name);
            D_PlayBGM(name);
        }
    }

    public static void StopBGM()
    {
        Instance.FadeMusic(Instance.current, 0f);
    }
    public static void StopAmbient()
    {
        Instance.FadeMusic(Instance.ambientChannel, 0f);
        Instance.ambientChannel.Stop();
    }

    public static void StopAll()
    {
        Instance.FadeMusic(Instance.current, 0f);
        Instance.FadeMusic(Instance.ambientChannel, 0f);
        isPlayingName = null;
    }

    public void FadeMusic(AudioSource audioSource, float target)
    {
        if (audioSource == null) return;

        float delV = (target - audioSource.volume) / fadeTime;
        StartCoroutine(FadeMusicTo(audioSource, target, delV));
    }

    IEnumerator FadeMusicTo(AudioSource audioSource, float target, float delV)
    {
        for (float i = audioSource.volume; i < target - 0.01 || i > target + 0.01; i += delV)
        {
            audioSource.volume = i;
            yield return new WaitForFixedUpdate();
        }
        audioSource.volume = target;
        if (target == 0f) audioSource.Stop();
    }

    public void LoadBGM(string name)
    {
        Debug.Log("Try Loading"+name);
        if (backGroundMusics.ContainsKey(name))
        {
            Debug.Log("Duplicated loading music source.");
        }
        else
        {

            AudioClip audioClip = res.LoadSync<AudioClip>(name);
            // AudioClip audioClip = Resources.Load<AudioClip>("Audios/BGM/" + name);
            backGroundMusics.Add(name, audioClip);
        }
    }
    public void RemoveBGM(string name)
    {
        try
        {
            backGroundMusics.Remove(name);
        }
        catch (Exception e)
        {
            throw new Exception("BGM key error", e);
        }
    }

    public void ChangeAmbient(string name)
    {
        StartCoroutine(IEChangeAudio(name, ambientChannel));
    }
    public void ChangeBGM(string name)
    {
        StartCoroutine(IEChangeAudio(name, current));
    }


    IEnumerator IEChangeAudio(string name, AudioSource changeAS)
    {
        AudioSource temp = GetAuxChannel();
        temp.volume = 0;
        temp.clip = changeAS.clip;
        temp.Play();
        FadeMusic(temp, changeAS == this.ambientChannel ? ambientVolume : m_backgroundVolume);
        FadeMusic(changeAS, 0);
        AudioClip newAudio;
        yield return null;
        bool i = backGroundMusics.TryGetValue(name,out newAudio);
        if(!i) newAudio = res.LoadSync<AudioClip>(name);

        yield return new WaitUntil(() => changeAS.volume == 0);

        changeAS.clip = newAudio;
        changeAS.Play();
        FadeMusic(temp, 0);
        FadeMusic(changeAS, changeAS == this.ambientChannel ? ambientVolume : m_backgroundVolume);
    }

    private AudioSource GetAuxChannel()
    {
        return current == BGM1 ? BGM2 : BGM1;
    }

    public void OnPuzzleInitialize()
    {
        AudioKit.PlaySound("InteractShow", volume: effectVolume);
        audioMixer.TransitionToSnapshots(audioMixerSnapshots, new float[] { 0, 1 }, 0.2f);
    }
    public void OnPuzzleExit()
    {
        audioMixer.TransitionToSnapshots(audioMixerSnapshots, new float[] { 1, 0 }, 0.5f);
    }
}
