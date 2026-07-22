using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CutSceneEffectManager : MonoBehaviour
{
    public enum CutSceneType
    {
        FadeIn,
        FadeInWhite,
        FadeOut,
        // 다른 컷씬 타입 추가 가능
    }

    private string _fadePrefabAddress = "Assets/Prefabs/Fade Canvas.prefab";

    private AsyncOperationHandle<GameObject> _loadHandle;

    private FadeCutScene _fadeCutScene;
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
}
