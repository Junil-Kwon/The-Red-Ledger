using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 배경음악(BGM) 타입
public enum EBgm
{
    TITLE,
    GAME,
    RESULT,
}

// 효과음(SFX) 타입
public enum ESfx
{
    BUTTON_CLICK,
    ITEM_PICKUP,
    DOOR_OPEN,
    MISSION_CLEAR,
}

public class SoundManager : Singleton<SoundManager>
{
    public static SoundManager instance;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips; // BGM 오디오 클립 배열
    [SerializeField] private AudioClip[] sfxClips; // SFX 오디오 클립 배열

    [Header("Object Pool Settings")]
    [SerializeField] private int poolSize = 10; // 풀 크기

    private Dictionary<EBgm, AudioClip> bgmDict; // BGM을 저장할 Dictionary
    private Dictionary<ESfx, AudioClip> sfxDict; // SFX를 저장할 Dictionary
    private Queue<AudioSource> audioSourcePool; // 오브젝트 풀

    private AudioSource bgmPlayer; // BGM 재생용 AudioSource

    protected override void Awake()
    {
        base.Awake(); // Singleton의 Awake 호출

        //if (Instance != this) return; // 싱글톤 인스턴스가 이미 존재하면 초기화하지 않음
        
        Init(); // 초기화 메서드 호출
    }

    private void Init()
    {
        // BGM Dictionary 초기화
        bgmDict = new Dictionary<EBgm, AudioClip>();
        for (int i = 0; i < bgmClips.Length; i++)
        {
            bgmDict[(EBgm)i] = bgmClips[i]; // enum과 인덱스를 매핑
        }

        // SFX Dictionary 초기화
        sfxDict = new Dictionary<ESfx, AudioClip>();
        for (int i = 0; i < sfxClips.Length; i++)
        {
            sfxDict[(ESfx)i] = sfxClips[i]; // enum과 인덱스를 매핑
        }

        // BGM 플레이어 초기화
        bgmPlayer = gameObject.AddComponent<AudioSource>();
        bgmPlayer.loop = true;

        // 오브젝트 풀 초기화
        InitPool();
    }

    private void InitPool()
    {
        audioSourcePool = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.enabled = false;
            audioSourcePool.Enqueue(source);
        }
    }

    // BGM 재생
    public void PlayBGM(EBgm bgmType, float volume = 1.0f, float fadeDuration = 1.0f)
    {
        if (bgmDict.TryGetValue(bgmType, out var clip))
        {
            if (bgmPlayer.clip != clip)
            {
                StartCoroutine(FadeBGM(clip, volume, fadeDuration));
            }
        }
    }

    private IEnumerator FadeBGM(AudioClip newClip, float targetVolume, float duration)
    {
        // 1. 기존 음악 페이드 아웃
        float startVolume = bgmPlayer.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmPlayer.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        // 2. 음악 교체 및 페이드 인
        bgmPlayer.clip = newClip;
        bgmPlayer.Play();
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmPlayer.volume = Mathf.Lerp(0, targetVolume, t / duration);
            yield return null;
        }
        bgmPlayer.volume = targetVolume;
    }

    // SFX 재생 (Object Pooling 사용)
    public void PlaySFX(ESfx sfxType, float volume = 1.0f)
    {
        if (sfxDict.TryGetValue(sfxType, out var clip))
        {
            if (audioSourcePool.Count > 0)
            {
                AudioSource source = audioSourcePool.Dequeue();
                source.clip = clip;
                source.volume = volume;
                source.enabled = true;
                source.Play();

                StartCoroutine(ReturnToPool(source, clip.length));
            }
            else
            {
                // 풀에 오디오 소스가 없을 경우, 새로 생성하여 사용
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                newSource.clip = clip;
                newSource.volume = volume;
                newSource.playOnAwake = false;
                newSource.enabled = true;
                newSource.Play();

                // 새로 생성한 소스는 재사용 후 풀에 다시 넣을 수 있도록 코루틴을 사용
                StartCoroutine(ReturnToPool(newSource, clip.length));
            }
        }
        else
        {
            Debug.LogWarning("SFX not found");
        }
    }

    public void PlaySoundOnObject(ESfx sfxType, GameObject obj, float volume = 1.0f)
    {
        if (sfxDict.TryGetValue(sfxType, out var clip))
        {
            AudioSource source = obj.GetComponent<AudioSource>();
            if (source == null) source = obj.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = volume;
            source.Play();
        }
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.enabled = false;
        source.volume = 1.0f;
        audioSourcePool.Enqueue(source);
    }
}