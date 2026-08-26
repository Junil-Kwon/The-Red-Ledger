using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CutSceneManager : MonoBehaviour
{
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

    private string _fadePrefabAddress = "Assets/Prefabs/Fade Canvas.prefab";

    private AsyncOperationHandle<GameObject> _loadHandle;

    private ACutScene _currentCutScene;
    /*
    public bool IsFadeCutSceneActive {
        get
        {
            if (_fadeCutScene == null)
            {
                Debug.LogWarning("FadeCutScene instance is null. It may not have been loaded yet.");
                return false;
            }
            return _fadeCutScene.gameObject.activeSelf;
        }
    }
    */
    /*
    public async Task<FadeCutScene> GetFadeCutSceneAsync()
    {
        if (_fadeCutScene == null)
        {
            // Use the recommended API instead of the obsolete FindObjectOfType
            _fadeCutScene = FindAnyObjectByType<FadeCutScene>();
            if (_fadeCutScene == null)
            {
                _loadHandle = Addressables.LoadAssetAsync<GameObject>(_fadePrefabAddress);
                await _loadHandle.Task; // 로딩 완료까지 대기

                if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject prefab = _loadHandle.Result;
                    // 프리팹을 생성하고 컴포넌트를 가져옴
                    GameObject go = Instantiate(prefab);
                    _fadeCutScene = go.GetComponent<FadeCutScene>();
                    
                    Debug.Log("Prefab loaded and instantiated successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to load prefab at address: {_fadePrefabAddress}");
                }
            }
        }

        return _fadeCutScene;
    }
    */

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
}
