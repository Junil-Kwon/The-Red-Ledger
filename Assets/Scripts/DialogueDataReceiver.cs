using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueDataReceiver : Singleton<DialogueDataReceiver>
{
    private DialogueData dialogueData;

    public void InitializeData(DialogueData data, bool applyOnSceneLoaded = false)
    {
        dialogueData = data;

        if (applyOnSceneLoaded)
        {
            // SceneLoaded 이벤트에 ApplyData 메서드를 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyData();

        // SceneLoaded 이벤트에서 ApplyData 메서드를 제거하여 중복 호출 방지
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ApplyData()
    {
        ApplyDataToScene();

        if (dialogueData.inkJSON != null)
        {
            var dialogueManager = FindAnyObjectByType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("DialogueManager instance is null. Creating a new instance.");
                GameObject dialogueManagerGO = new GameObject("DialogueManager");
                dialogueManager = dialogueManagerGO.AddComponent<DialogueManager>();
            }

            dialogueManager.SetInkJSON(dialogueData.inkJSON);
        }
        else
        {
            Debug.LogWarning("Ink JSON is null. Cannot start dialogue.");
        }
    }

    private void ApplyDataToScene()
    {
        if (dialogueData == null)
        {
            Debug.LogError("DialogueData is null. Cannot apply data to scene.");
            return;
        }

        if (dialogueData.backgroundImage != null)
        {
            // 배경 이미지 설정
            if (dialogueData.backgroundImage != null)
            {
                var backgroundImage = GameObject.Find("Background").GetComponent<SpriteRenderer>(); // 배경 이미지 GameObject를 찾아서 Image 컴포넌트 가져오기
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = dialogueData.backgroundImage;
                }
                else
                {
                    Debug.LogError("Background GameObject not found!");
                }
            }
        }

        /*
        // 캐릭터 이미지 및 위치 설정
        for (int i = 0; i < dialogueData.characters.Count; i++)
        {
            var characterPair = dialogueData.characters[i];
            var characterGO = GameObject.Find($"Character{i + 1}");
            if (characterGO != null)
            {
                var characterImage = characterGO.GetComponent<Image>();
                if (characterImage != null)
                {
                    characterImage.sprite = characterPair.Key;
                    characterGO.transform.localPosition = characterPair.Value;
                }
                else
                {
                    Debug.LogError($"Character{i + 1} does not have an Image component!");
                }
            }
            else
            {
                Debug.LogError($"Character{i + 1} GameObject not found!");
            }
        }
        */

        // BGM 재생
        if (dialogueData.bgm != null)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = dialogueData.bgm;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("AudioSource component not found on DialogueDataReceiver!");
            }
        }
    }
    // ================= Lifecycle Methods =================

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
