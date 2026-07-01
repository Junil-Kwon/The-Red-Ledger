using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using StoryFlags; // StoryFlag enum이 정의된 네임스페이스를 임포트

public class DialogueManager : MonoBehaviour
{
    [Header("Ink")]
    [SerializeField] private TextAsset _inkJSON;
    private Story _story;
    
    [Header("Speaker UI Elements")]
    [SerializeField] private GameObject _playerSpeechPanel;
    [SerializeField] private List<GameObject> _speechBubbles = new List<GameObject>();
    [SerializeField] private List<GameObject> _currentChoices = new List<GameObject>();
    //[SerializeField] private GameObject choiceButtonPrefab;
    //[SerializeField] private Transform choiceContainer;

    [Header("Dialogue Settings")]
    [SerializeField] private float _textSpeed = 0.05f;

    private int _speakerIndex = 0;
    private Color _nextTextColor = Color.black;
    private string _processedText;
    private Coroutine _typingCoroutine;
    private bool _diagWaitingForAdvance = false;
    private bool _diagAdvanceRequested = false;
    private bool _diagLineSkipFlag = false; // 타이핑 스킵 플래그

    private InkStatusManager _inkStatusManager;

    public InkStatusManager InkStatusManager
    {
        get
        {
            if (_inkStatusManager == null)
            {
                _inkStatusManager = FindAnyObjectByType<InkStatusManager>();
                if (_inkStatusManager == null)
                {
                    Debug.Log("InkStatusManager가 씬에 없습니다. 새로 생성합니다.");
                    _inkStatusManager = new GameObject("InkStatusManager").AddComponent<InkStatusManager>();
                }
            }
            return _inkStatusManager;
        }
    }

    void OnEnable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.Input.Player.Click.performed += HandleClick;
            PlayerInputManager.Instance.Input.Player.Interact.performed += HandleInteract;
        }
    }

    void OnDisable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.Input.Player.Click.performed -= HandleClick;
            PlayerInputManager.Instance.Input.Player.Interact.performed -= HandleInteract;
        }
    }

    void Awake()
    {
        //inkStatusManager = FindAnyObjectByType<InkStatusManager>();
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

        if (PlayerInputManager.Instance.IsPointerOverUIWhenClick())
            return; // UI 위에서 클릭한 경우 대화 진행 방지

        skipLine(_diagWaitingForAdvance);
    }

    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        //Debug.Log("HandleInteraction called");

        switch(ctx.control.displayName)
        {
            case "Space":
            case "Enter":
                skipLine(_diagWaitingForAdvance);
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

        if (!_currentChoices[choiceIndex].activeSelf) // 쿨타임이 있다면 수정 필요
            return; // 대화 진행 중이 아니면 선택지 입력 무시

        _currentChoices[choiceIndex].GetComponent<Button>().onClick.Invoke(); // 해당 선택지 버튼의 클릭 이벤트 강제 호출
    }
    
    void InitializeStory()
    {
        _story = new Story(_inkJSON.text);
        _story.onError += OnStoryError;
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
        
        _story.BindExternalFunction("PlayBGM", (string BGMName) => {
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

        _story.BindExternalFunction("PlaySFX", (string SFXName) => {
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
        
        _story.BindExternalFunction("UpdateStatusRecoveryCount", (bool value) => {
            Debug.Log($"UpdateStatusRecoveryCount called with: {value}");
            InkStatusManager?.UpdateStatusRecoveryCount(value);
        });

        _story.BindExternalFunction("AddIntel", (int points) => {
            Debug.Log($"AddIntel called with: {points}");
            InkStatusManager?.AddIntel(points);
        });

        _story.BindExternalFunction("AddCreativeFlagCount", () => {
            Debug.Log("AddCreativeFlagCount called");
            InkStatusManager?.AddCreativeFlagCount();
        });

        _story.BindExternalFunction("TriggerStoryFlag", (string flag) => {
            Debug.Log($"TriggerStoryFlag called with: {flag}");
            
            if (Enum.TryParse(flag, true, out EStoryFlag storyFlag))
            {
                DataManager.Instance.TriggerStoryFlag(storyFlag);
            }
            else
            {
                Debug.LogError($"Ink에서 잘못된 스토리 플래그를 보냈습니다: {storyFlag}");
            }
            
        });

        _story.BindExternalFunction("GetObjectiveState", () => {
            return InkStatusManager.GetObjectiveState();
        });

        _story.BindExternalFunction("SystemNotify", (string action) => {
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
        _diagWaitingForAdvance = false;
        _diagAdvanceRequested = false;
        _playerSpeechPanel.SetActive(true);
        StartCoroutine(ContinueStory());
    }
    
    IEnumerator ContinueStory()
    {
        // 텍스트 출력
        while (_story.canContinue)
        {
            string text = _story.Continue();
            _processedText = text.Trim();

            // ⭐ [수정된 부분] 빈 문자열(엔터 등)일 경우 클릭 대기를 건너뛰고 바로 다음으로 진행
            if (string.IsNullOrEmpty(_processedText))
            {
                continue; 
            }

            ProcessTags(_story.currentTags);

            // 타이핑 시작
            _typingCoroutine = StartCoroutine(TypeText(_processedText));

            // 타이핑이 끝날 때까지  대기
            yield return _typingCoroutine;
            _typingCoroutine = null;

            _diagWaitingForAdvance = true;
            _diagAdvanceRequested = false;
            yield return new WaitUntil(() => _diagAdvanceRequested);
            _diagWaitingForAdvance = false;
            _diagAdvanceRequested = false;
        }
        
        // 선택지 표시
        DisplayChoices();
    }

    void skipLine(bool diagWaitingForAdvance)
    {
        if (diagWaitingForAdvance)
        {
            _diagAdvanceRequested = true;
        }
        else
        {
            if (_typingCoroutine != null) _diagLineSkipFlag = true; // 타이핑 중이면 스킵 플래그 설정
        }
    }

    IEnumerator TypeText(string text)
    {
        SpeechBubble bubble = _speechBubbles[_speakerIndex].GetComponent<SpeechBubble>();
        bubble.BubbleInit(); // 버블 초기화
        TextMeshProUGUI speakerText = _speechBubbles[_speakerIndex].GetComponentInChildren<TextMeshProUGUI>();
        string tmpText = ""; // 임시 텍스트 변수 초기화
        speakerText.color = _nextTextColor; // 텍스트 색상 적용

        for (int i = 0; i < text.Length; i++)
        {
            tmpText += text[i];
            bubble.UpdateBubble(tmpText); // 줄 바꿈 체크

            yield return new WaitForSeconds(_textSpeed);

            if (_diagLineSkipFlag) 
            {
                _diagLineSkipFlag = false; // 스킵 플래그 초기화
                tmpText = text; // 대사 출력 스킵
                bubble.UpdateBubble(tmpText); // 줄 바꿈 체크
                _nextTextColor = Color.black; // 텍스트 색상 초기화
                yield break; // 스킵 플래그가 설정되면 타이핑 중단
            }
        }

        _nextTextColor = Color.black; // 텍스트 색상 초기화
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
                _speakerIndex = int.Parse(value);
                break;
            case "emotion":
                // 표정 변경
                break;
            case "language":
                // 대사 색상 변경
                switch(value)
                {
                    case "Osten":
                        _nextTextColor = Color.red;
                        break;
                    case "Valeska":
                        _nextTextColor = Color.blue;
                        break;
                    default:
                        _nextTextColor = Color.black; // 기본 색상
                        break;
                }
                break;
            default:
                Debug.LogWarning($"알 수 없는 태그: {key}:{value}");
                break;
        }
    }
    
    void DisplayChoices()
    {
        //ClearChoices();
        
        foreach (Choice choice in _story.currentChoices)
        {
            int choiceIndex = choice.index;
            //GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            // NOTE: 버튼이 3개를 넘지 않는다는 가정하의 코드
            ProcessTags(choice.tags);

            ChoiceButton choicebtn = _currentChoices[choiceIndex].GetComponent<ChoiceButton>();
            choicebtn.gameObject.SetActive(true);

            choicebtn.SetText(choice.text);
            choicebtn.SetTextColor(_nextTextColor);

            choicebtn.GetComponent<Button>().onClick.AddListener(() => {
                OnChoiceSelected(choiceIndex);
            });
            
            //currentChoices.Add(choiceButton);
            
            _nextTextColor = Color.black; // 텍스트 색상 초기화
        }
        
        if (_story.currentChoices.Count == 0)
        {
            // 대화 종료
            EndDialogue();
        }
    }
    
    void OnChoiceSelected(int choiceIndex)
    {
        _diagWaitingForAdvance = false;
        _diagAdvanceRequested = false;
        _story.ChooseChoiceIndex(choiceIndex);
        StartCoroutine(ContinueStory());

        ClearChoices();
    }
    
    void ClearChoices()
    {
        foreach (GameObject choice in _currentChoices)
        {
            //Destroy(choice);
            choice.SetActive(false);
            choice.GetComponent<Button>().onClick.RemoveAllListeners();

        }
        //currentChoices.Clear();
        
    }
    
    void EndDialogue()
    {
        _playerSpeechPanel.SetActive(false);
    }
    
    public void JumpToKnot(string knotName)
    {
        _diagWaitingForAdvance = false;
        _diagAdvanceRequested = false;
        _story.ChoosePathString(knotName);
        StartCoroutine(ContinueStory());
    }
}
