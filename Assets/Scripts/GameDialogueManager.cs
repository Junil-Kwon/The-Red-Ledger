using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using StoryFlags; // StoryFlag enum이 정의된 네임스페이스를 임포트

public class GameDialogueManager : MonoBehaviour
{
    [Header("Ink")]
    [SerializeField] private TextAsset inkJSON;
    private Story story;
    
    [Header("Speaker UI Elements")]
    [SerializeField] private GameObject playerSpeechPanel;
    [SerializeField] private List<GameObject> speechBubbles = new List<GameObject>();
    [SerializeField] private List<GameObject> currentChoices = new List<GameObject>();
    //[SerializeField] private GameObject choiceButtonPrefab;
    //[SerializeField] private Transform choiceContainer;

    [Header("Dialogue Settings")]
    [SerializeField] private float textSpeed = 0.05f;

    private int speakerIndex = 0;
    private Color nextTextColor = Color.black;
    private string processedText;
    private Coroutine typingCoroutine;
    private bool diagWaitingForAdvance = false;
    private bool diagAdvanceRequested = false;
    private bool diagLineSkipFlag = false; // 타이핑 스킵 플래그

    //private GameStatusManager gameStatusManager;

    void OnEnable()
    {
        if (PlayerInputController.Instance != null)
        {
            PlayerInputController.Instance.Input.Player.Click.performed += HandleClick;
            PlayerInputController.Instance.Input.Player.Interact.performed += HandleInteract;
        }
    }

    void OnDisable()
    {
        if (PlayerInputController.Instance != null)
        {
            PlayerInputController.Instance.Input.Player.Click.performed -= HandleClick;
            PlayerInputController.Instance.Input.Player.Interact.performed -= HandleInteract;
        }
    }

    void Awake()
    {
        //gameStatusManager = FindAnyObjectByType<GameStatusManager>();
    }
    
    void Start()
    {
        InitializeStory();
        StartDialogue();
    }

    // =================== 입력 methods ===================
    private void HandleClick(InputAction.CallbackContext ctx)
    {
        //Debug.Log("HandleClick called");

        if (PlayerInputController.Instance.IsPointerOverUIWhenClick())
            return; // UI 위에서 클릭한 경우 대화 진행 방지

        skipLine(diagWaitingForAdvance);
    }

    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        //Debug.Log("HandleInteraction called");

        switch(ctx.control.displayName)
        {
            case "Space":
            case "Enter":
                skipLine(diagWaitingForAdvance);
                break;
            case "1":
            case "2":
            case "3":
                ChooseChoiceByKey(ctx.control.displayName);
                break;
            default:
                // 다른 상호작용 키에 대한 처리 (예: E키로 대화 시작 등)
                break;
        }
        
    }

    // =================== 대화 시스템 methods ===================

    void ChooseChoiceByKey(string key)
    {
        int choiceIndex = key switch
        {
            "1" => 0,
            "2" => 1,
            "3" => 2,
            _ => -1
        };

        if (!currentChoices[choiceIndex].activeSelf) // 쿨타임이 있다면 수정 필요
            return; // 대화 진행 중이 아니면 선택지 입력 무시

        currentChoices[choiceIndex].GetComponent<Button>().onClick.Invoke(); // 해당 선택지 버튼의 클릭 이벤트 강제 호출
    }
    
    void InitializeStory()
    {
        story = new Story(inkJSON.text);
        story.onError += OnStoryError;
        BindExternalFunctions();
    }
    
    void OnStoryError(string message, Ink.ErrorType type)
    {
        if (type == Ink.ErrorType.Warning)
            Debug.LogWarning($"Ink Warning: {message}");
        else
            Debug.LogError($"Ink Error: {message}");
    }
    
    void BindExternalFunctions()
    {
        
        story.BindExternalFunction("PlayBGM", (string BGMName) => {
            Debug.Log($"PlayBGM called with: {BGMName}");
            /*
            if (Enum.TryParse(BGMName, true, out EBgm bgmType))
            {
                SoundManager.Instance.PlayBGM(bgmType);
            }
            else
            {
                Debug.LogError($"Ink에서 잘못된 BGM 이름을 보냈습니다: {BGMName}");
            }
            */
        });

        story.BindExternalFunction("PlaySFX", (string SFXName) => {
            Debug.Log($"PlaySFX called with: {SFXName}");
            /*
            if (Enum.TryParse(SFXName, true, out ESfx sfxType))
            {
                SoundManager.Instance.PlaySFX(sfxType);
            }
            else
            {
                Debug.LogError($"Ink에서 잘못된 SFX 이름을 보냈습니다: {SFXName}");
            }
            */
        });
        
        story.BindExternalFunction("UpdateStatus", (int value) => {
            Debug.Log($"UpdateStatus called with: {value}");
            //gameStatusManager?.UpdateStatus(value);
        });

        story.BindExternalFunction("AddIntel", (int points) => {
            Debug.Log($"AddIntel called with: {points}");
            // gameStatusManager?.AddIntel(points);
        });

        story.BindExternalFunction("AddCreativeFlagPoint", () => {
            Debug.Log("AddCreativeFlagPoint called");
            // gameStatusManager?.AddCreativeFlagPoint();
        });

        story.BindExternalFunction("TriggerStoryFlag", (string flag) => {
            Debug.Log($"TriggerStoryFlag called with: {flag}");
            /*
            if (Enum.TryParse(flag, true, out EStoryFlag storyFlag))
            {
                DataManager.Instance.TriggerStoryFlag(storyFlag);
            }
            else
            {
                Debug.LogError($"Ink에서 잘못된 스토리 플래그를 보냈습니다: {storyFlag}");
            }
            */
        });

        story.BindExternalFunction("GetObjectiveState", () => {
            return "YELLOW"; // 임시 반환값, 실제로는 gameStatusManager에서 상태를 가져와야 함
            //return gameStatusManager? gameStatusManager.GetObjectiveState() : 0;
        });

        story.BindExternalFunction("SystemNotify", (string action) => {
            Debug.Log($"SystemNotify called with: {action}");
            /*
            if (Enum.TryParse(action, true, out  ENotifyType notifyType))
            {
                //SystemNotifyManager.Instance.ShowNotification(notifyType);
            }
            else
            {
                Debug.LogError($"Ink에서 잘못된 시스템 알림 액션을 보냈습니다: {action}");
            }
            */
        });
    }
    
    public void StartDialogue()
    {
        diagWaitingForAdvance = false;
        diagAdvanceRequested = false;
        playerSpeechPanel.SetActive(true);
        StartCoroutine(ContinueStory());
    }
    
    IEnumerator ContinueStory()
    {
        // 텍스트 출력
        while (story.canContinue)
        {
            string text = story.Continue();
            processedText = text.Trim();

            // ⭐ [수정된 부분] 빈 문자열(엔터 등)일 경우 클릭 대기를 건너뛰고 바로 다음으로 진행
            if (string.IsNullOrEmpty(processedText))
            {
                continue; 
            }

            ProcessTags(story.currentTags);

            // 타이핑 시작
            typingCoroutine = StartCoroutine(TypeText(processedText));

            // 타이핑이 끝날 때까지  대기
            yield return typingCoroutine;
            typingCoroutine = null;

            diagWaitingForAdvance = true;
            diagAdvanceRequested = false;
            yield return new WaitUntil(() => diagAdvanceRequested);
            diagWaitingForAdvance = false;
            diagAdvanceRequested = false;
        }
        
        // 선택지 표시
        DisplayChoices();
    }

    void skipLine(bool diagWaitingForAdvance)
    {
        if (diagWaitingForAdvance)
        {
            diagAdvanceRequested = true;
        }
        else
        {
            if (typingCoroutine != null) diagLineSkipFlag = true; // 타이핑 중이면 스킵 플래그 설정
        }
    }

    IEnumerator TypeText(string text)
    {
        SpeechBubble bubble = speechBubbles[speakerIndex].GetComponent<SpeechBubble>();
        bubble.BubbleInit(); // 버블 초기화
        TextMeshProUGUI speakerText = speechBubbles[speakerIndex].GetComponentInChildren<TextMeshProUGUI>();
        string tmpText = ""; // 임시 텍스트 변수 초기화
        speakerText.color = nextTextColor; // 텍스트 색상 적용

        for (int i = 0; i < text.Length; i++)
        {
            tmpText += text[i];
            bubble.UpdateBubble(tmpText); // 줄 바꿈 체크

            yield return new WaitForSeconds(textSpeed);

            if (diagLineSkipFlag) 
            {
                diagLineSkipFlag = false; // 스킵 플래그 초기화
                tmpText = text; // 대사 출력 스킵
                bubble.UpdateBubble(tmpText); // 줄 바꿈 체크
                nextTextColor = Color.black; // 텍스트 색상 초기화
                yield break; // 스킵 플래그가 설정되면 타이핑 중단
            }
        }

        nextTextColor = Color.black; // 텍스트 색상 초기화
    }
    
    void ProcessTags(List<string> tags)
    {
        if (tags == null) return;

        foreach (string tag in tags)
        {
            string[] parts = tag.Split(':');
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                
                // 태그 처리 로직
                HandleTag(key, value);
            }
        }
    }
    
    void HandleTag(string key, string value)
    {
        switch (key)
        {
            case "speaker":
                // 화자 변경
                speakerIndex = int.Parse(value);
                break;
            case "emotion":
                // 표정 변경
                break;
            case "language":
                // 대사 색상 변경
                switch(value)
                {
                    case "Osten":
                        nextTextColor = Color.red;
                        break;
                    case "Valeska":
                        nextTextColor = Color.blue;
                        break;
                    default:
                        nextTextColor = Color.black; // 기본 색상
                        break;
                }
                break;
        }
    }
    
    void DisplayChoices()
    {
        //ClearChoices();
        
        foreach (Choice choice in story.currentChoices)
        {
            int choiceIndex = choice.index;
            //GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            // NOTE: 버튼이 3개를 넘지 않는다는 가정하의 코드
            ProcessTags(choice.tags);

            ChoiceButton choicebtn = currentChoices[choiceIndex].GetComponent<ChoiceButton>();
            choicebtn.gameObject.SetActive(true);

            choicebtn.SetText(choice.text);
            choicebtn.SetTextColor(nextTextColor);

            choicebtn.GetComponent<Button>().onClick.AddListener(() => {
                OnChoiceSelected(choiceIndex);
            });
            
            //currentChoices.Add(choiceButton);
            
            nextTextColor = Color.black; // 텍스트 색상 초기화
        }
        
        if (story.currentChoices.Count == 0)
        {
            // 대화 종료
            EndDialogue();
        }
    }
    
    void OnChoiceSelected(int choiceIndex)
    {
        diagWaitingForAdvance = false;
        diagAdvanceRequested = false;
        story.ChooseChoiceIndex(choiceIndex);
        StartCoroutine(ContinueStory());

        ClearChoices();
    }
    
    void ClearChoices()
    {
        foreach (GameObject choice in currentChoices)
        {
            //Destroy(choice);
            choice.SetActive(false);
            choice.GetComponent<Button>().onClick.RemoveAllListeners();

        }
        //currentChoices.Clear();
        
    }
    
    void EndDialogue()
    {
        playerSpeechPanel.SetActive(false);
    }
    
    public void JumpToKnot(string knotName)
    {
        diagWaitingForAdvance = false;
        diagAdvanceRequested = false;
        story.ChoosePathString(knotName);
        StartCoroutine(ContinueStory());
    }
}
