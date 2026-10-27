using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum CutSceneType
{
    FadeInSTD,
    FadeInIMD,
    FadeInWhiteSTD,
    FadeInWhiteIMD,
    FadeOutSTD,
    FadeOutIMD,
    // 다른 컷씬 타입 추가 가능
}

public class CutSceneManager : MonoBehaviour
{
    private string _fadePrefabAddress = "Assets/Prefabs/Fade Canvas.prefab";

    private readonly struct CutSceneEntry
    {
        public readonly string PrefabAddress;
        public readonly int Mode;
        public readonly bool IsShow;

        public CutSceneEntry(string address, int mode, bool isShow)
        {
            PrefabAddress = address;
            Mode = mode;
            IsShow = isShow;
        }
    }

    private Dictionary<CutSceneType, CutSceneEntry> _entries;

    private void BuildEntries()
    {
        _entries = new Dictionary<CutSceneType, CutSceneEntry>
        {
            { CutSceneType.FadeInSTD,       new(_fadePrefabAddress, 0, true)  },
            { CutSceneType.FadeInIMD,       new(_fadePrefabAddress, 1, true)  },
            { CutSceneType.FadeInWhiteSTD,  new(_fadePrefabAddress, 2, true)  },
            { CutSceneType.FadeInWhiteIMD,  new(_fadePrefabAddress, 3, true)  },
            { CutSceneType.FadeOutSTD,      new(_fadePrefabAddress, 0, false) },
            { CutSceneType.FadeOutIMD,      new(_fadePrefabAddress, 1, false) },
            // 새 컷씬 추후 추가
            // { CutSceneType.VideoIntro,   new(_videoPrefabAddress, 0, true) },
        };
    }

    //private AsyncOperationHandle<GameObject> _loadHandle;

    private ACutScene _currentCutScene;
    private string _currentCutSceneAddress;

    private static CutSceneManager _instance;

    public static CutSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<CutSceneManager>();
                if (_instance == null)
                {
                    Debug.Log("CutSceneManager가 씬에 없습니다. 새로 생성합니다.");
                    _instance = new GameObject("CutSceneManager").AddComponent<CutSceneManager>();
                }
            }
            return _instance;
        }
    }

    // 비동기적 실행
    /*
    public async void PlayCutScene(CutSceneType type, bool mode = true)
    {
        switch (type)
        {
            case CutSceneType.FadeIn:
                // FadeIn 컷씬 효과 실행
                _fadeCutScene = await GetFadeCutSceneAsync();
                _fadeCutScene.Show(isFade : mode);
                break;
            case CutSceneType.FadeOut:
                // FadeOut 컷씬 효과 실행
                _fadeCutScene = await GetFadeCutSceneAsync();
                _fadeCutScene.Hide(isFade : mode);
                break;
            case CutSceneType.FadeInWhite:
                // FadeInWhite 컷씬 효과 실행
                _fadeCutScene = await GetFadeCutSceneAsync();
                _fadeCutScene.Show(isFade : mode, isWhite : true);
                break;
            default:
                Debug.LogWarning("Unknown CutSceneType: " + type);
                break;
        }
    }
    */

    // OBSOLETE: 동기적 컷씬 실행 방식
    /*
    // 동기적 실행
    public void PlayCutScene(CutSceneType type)
    {
        switch (type)
        {
            case CutSceneType.FadeInSTD:
                // FadeIn 컷씬 효과 실행
                _currentCutScene = GetCutScene<FadeCutScene>();
                _currentCutScene?.Show(0); // Mode 3: FadeInSTD
                break;
            case CutSceneType.FadeInIMD:
                // FadeIn 컷씬 효과 실행
                _currentCutScene = GetCutScene<FadeCutScene>();
                _currentCutScene?.Show(1); // Mode 1: FadeInIMD
                break;
            case CutSceneType.FadeInWhiteSTD:
                // FadeInWhite 컷씬 효과 실행
                _currentCutScene = GetCutScene<FadeCutScene>();
                _currentCutScene?.Show(2); // Mode 2: FadeInWhiteSTD
                break;
            case CutSceneType.FadeInWhiteIMD:
                // FadeInWhite 컷씬 효과 실행
                _currentCutScene = GetCutScene<FadeCutScene>();
                _currentCutScene?.Show(3); // Mode 3: FadeInWhiteIMD
                break;
            case CutSceneType.FadeOutSTD:
                // FadeOut 컷씬 효과 실행
                _currentCutScene = GetCutScene<FadeCutScene>();
                _currentCutScene?.Hide(0); // Mode 0: FadeOutSTD
                break;
            case CutSceneType.FadeOutIMD:
                // FadeOut 컷씬 효과 실행
                _currentCutScene = GetCutScene<FadeCutScene>();
                _currentCutScene?.Hide(1); // Mode 1: FadeOutIMD
                break;
            default:
                Debug.LogWarning("Unknown CutSceneType: " + type);
                break;
        }
    }
    */

    public void PlayCutScene(CutSceneType type, Action<ACutScene> onReady = null)
    {
        StartCoroutine(PlayCutSceneRoutine(type, onReady));
    }

    private IEnumerator PlayCutSceneRoutine(CutSceneType type, Action<ACutScene> onReady)
    {
        if (!_entries.TryGetValue(type, out var entry))
        {
            Debug.LogError($"등록되지 않은 컷씬 타입: {type}");
            onReady?.Invoke(null); // 안 부르면 대화가 영원히 멈춤-> cutscene = null로 전달
            yield break;
        }

        ACutScene cutScene = null;
        yield return StartCoroutine(GetCutSceneRoutine(entry.PrefabAddress, (result) => cutScene = result));

        if (cutScene == null) { onReady?.Invoke(null); yield break; }

        if (entry.IsShow) cutScene.Show(entry.Mode, onReady);
        else              cutScene.Hide(entry.Mode, onReady);
    }

    //OBSOLETE: 동기적 컷씬 로딩 및 실행 방식
    /*
    public ACutScene GetCutScene<T>() where T : ACutScene
    {
        string prefabAddress = typeof(T).Name switch
        {
            nameof(FadeCutScene) => _fadePrefabAddress,
            // 다른 컷씬 타입에 대한 주소를 여기에 추가
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(prefabAddress))
        {
            Debug.LogError($"No prefab address defined for cutscene type: {typeof(T).Name}");
            return null;
        }

        if (_currentCutScene is not T)
        {
            if (_currentCutScene != null)
            {
                Destroy(_currentCutScene.gameObject);
            }

            _currentCutScene = FindAnyObjectByType<T>();
            if (_currentCutScene == null)
            {
                GameObject prefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();
                GameObject go = Instantiate(prefab);
                _currentCutScene = go.GetComponent<T>();
            }
        }
        return _currentCutScene;
    }
    */

    private readonly Dictionary<string, ACutScene> _cache = new();

    private IEnumerator GetCutSceneRoutine(string address, Action<ACutScene> onResult)
    {
        // Unity의 == 오버로드 덕분에 파괴된 오브젝트는 null로 취급됨
        if (_cache.TryGetValue(address, out var cached) && cached != null)
        {
            onResult(cached);
            yield break;
        }

        // 이미 캐시된 오브젝트가 없거나 파괴된 경우, Addressables를 통해 컷씬 프리팹을 비동기적으로 로드
        // Addressables를 통해 컷씬 프리팹을 비동기적으로 로드
        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"컷씬 프리팹 로드 실패: {address}");
            onResult(null);
            yield break;
        }

        var cutScene = Instantiate(handle.Result).GetComponent<ACutScene>();
        Addressables.Release(handle); // 메모리 누수 방지

        _cache[address] = cutScene;
        onResult(cutScene);
    }

    void Awake()
    {
        BuildEntries();
    }
}
