using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.AddressableAssets;
using PrimeTween;

public class LoadingSceneController : MonoBehaviour
{
    #region Singleton

    private static LoadingSceneController instance;
    public static LoadingSceneController Instance
    {
        get
        {
            if (instance == null)
            {
                LoadingSceneController sceneController = FindAnyObjectByType<LoadingSceneController>();
                if (sceneController != null)
                {
                    instance = sceneController;
                }
                else
                {
                    // 인스턴스가 없다면 생성
                    instance = Create();
                }
            }

            return instance;
        }
    }

    #endregion

    private static LoadingSceneController Create()
    {
        // 리소스에서 로드
        GameObject loadingUI = Addressables.InstantiateAsync(_prefabPath).WaitForCompletion() as GameObject;
        return loadingUI.GetComponent<LoadingSceneController>();
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] private float minimumLoadingTime = 2.0f;
    [SerializeField] private CanvasGroup mCanvasGroup;
    [SerializeField] private GameObject mControlTipPanel;
    [SerializeField] private TextMeshProUGUI mGameTipLabel;
    [SerializeField][TextArea] string[] mGameTips;

    private const string _prefabPath = "Assets/Prefabs/LoadingUI.prefab";

    private bool initialLoading = true; // 초기 로딩 여부를 나타내는 플래그

    private string mLoadSceneName;
    Action mOnSceneLoadAction;

    public void LoadScene(int sceneIndex, Action OnLoadaction = null)
    {
        gameObject.SetActive(true);
        SceneManager.sceneLoaded += OnSceneLoaded;
        mOnSceneLoadAction = OnLoadaction;

        string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        mLoadSceneName = sceneName;

        if (!initialLoading) mGameTipLabel.text = mGameTips[UnityEngine.Random.Range(0, mGameTips.Length - 1)];

        StartCoroutine(CoLoadSceneProcess());
    }
    
    public void LoadScene(string sceneName, Action OnLoadaction = null)
    {
        gameObject.SetActive(true);
        SceneManager.sceneLoaded += OnSceneLoaded;
        mOnSceneLoadAction = OnLoadaction;

        mLoadSceneName = sceneName;

        if (!initialLoading) mGameTipLabel.text = mGameTips[UnityEngine.Random.Range(0, mGameTips.Length - 1)];

        StartCoroutine(CoLoadSceneProcess());
    }

    private IEnumerator CoLoadSceneProcess()
    {
        float timer = 0.0f;
        //코루틴 안에서 yield return으로 코루틴을 실행하면.. 해당 코루틴이 끝날때까지 대기한다
        //if (!initialLoading) 
        //{
            //yield return StartCoroutine(Fade(true));
            FadeIn();
        //}

        if (InputManager.Instance != null)
        {
            InputManager.Instance.Input.Disable(); // 로딩 씬에서는 플레이어 입력 비활성화
        }

        //로컬 로딩
        AsyncOperation op = SceneManager.LoadSceneAsync(mLoadSceneName);

        op.allowSceneActivation = false;

        float process = 0.0f;

        //씬 로드가 끝나지 않은 상태라면?
        while (!op.isDone)
        {
            timer += Time.deltaTime;
            yield return null;

            if (op.progress < 0.9f)
            {
                //mProgressBar.fillAmount = op.progress;
            }
            else
            {
                process += Time.deltaTime * 5.0f;

                if (process > 1.0f && timer > minimumLoadingTime) op.allowSceneActivation = true;
            }
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == mLoadSceneName)
        {
            //StartCoroutine(Fade(false));
            FadeOut();

            if (InputManager.Instance != null)
            {
                InputManager.Instance.Input.Enable(); // 씬이 완전히 로드되면 플레이어 입력 활성화
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private IEnumerator CoLateStart()
    {
        yield return new WaitForEndOfFrame();

        // 예약된 함수 실행
        mOnSceneLoadAction?.Invoke();
    }
    /*
    private IEnumerator Fade(bool isFadeIn)
    {
        float process = 0f;

        if (!isFadeIn)
            StartCoroutine(CoLateStart());

        while (process < 1.0f)
        {
            process += Time.unscaledDeltaTime;
            mCanvasGroup.alpha = isFadeIn ? Mathf.Lerp(0.0f, 1.0f, process) : Mathf.Lerp(1.0f, 0.0f, process);

            yield return null;
        }

        if (!isFadeIn)
        {
            gameObject.SetActive(false);

            if (initialLoading) 
            {
                mControlTipPanel.SetActive(true); // 첫 로딩이 끝난 후에는 컨트롤 힌트 패널 활성화
                mGameTipLabel.gameObject.SetActive(true); // 게임 팁 텍스트 활성화
                initialLoading = false; // 첫 로딩이 끝났으므로 플래그 업데이트
            }
        }
    }
    */

    private void FadeIn()
    {
        StartCoroutine(CoLateStart());
        
        Tween.Alpha(mCanvasGroup, 1f, 0.5f, Ease.Linear).OnComplete(() =>
        {
            // 페이드인 완료 후 필요한 작업 수행
        });
    }

    private void FadeOut()
    {
        Tween.Alpha(mCanvasGroup, 0f, 0.5f, Ease.Linear).OnComplete(() =>
        {
            // 페이드아웃 완료 후 필요한 작업 수행
            gameObject.SetActive(false);

            if (initialLoading) 
            {
                mControlTipPanel.SetActive(true); // 첫 로딩이 끝난 후에는 컨트롤 힌트 패널 활성화
                mGameTipLabel.gameObject.SetActive(true); // 게임 팁 텍스트 활성화
                initialLoading = false; // 첫 로딩이 끝났으므로 플래그 업데이트
            }
        });
    }
}