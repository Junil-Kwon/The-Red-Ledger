using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using StoryFlags; // StoryFlag enum이 정의된 네임스페이스를 임포트

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("Ink")]
    //[SerializeField] private TextAsset _inkJSON;
    private DialogueData _dialogueData;
    private Story _story;
    
    [Header("Speaker UI Elements")]
    [SerializeField] private List<GameObject> _currentChoices = new List<GameObject>();
    //[SerializeField] private GameObject choiceButtonPrefab;
    //[SerializeField] private Transform choiceContainer;

    [Header("Dialogue Settings")]
    [SerializeField] private float _textSpeed = 0.05f;

    private int _textBoxIndex = 0;
    private Color _defaultTextColor = Color.black;
    private Color _nextTextColor = Color.clear;
    private string _processedText;
    private bool _isTyping = false;
    private bool _isWaitingForAdvance = false;
    private ETutorialPopupType? _pendingPopupAfterAdvance;

    private bool _isEventPlaying = false; // 이벤트 진행 중인지 여부

    void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Input.Player.Click.performed += HandleClick;
            InputManager.Instance.Input.Player.Interact.performed += HandleInteract;
        }
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Input.Player.Click.performed -= HandleClick;
            InputManager.Instance.Input.Player.Interact.performed -= HandleInteract;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        //inkStatusManager = FindAnyObjectByType<InkStatusManager>();
    }
    
    void Start()
    {
        /*
        InitializeStory();

        for (int i = 0; i < _textBoxes.Count; i++)
        {
            Debug.Log($"{i}번째 말풍선 등록: {_textBoxes[i].name}");
            TextBoxRouter.RegisterTextBox(i, _textBoxes[i].GetComponentInChildren<ITextBoxTarget>(true));
        }

        StartDialogue();
        */
    }

    // =================== 입력 methods ===================
    private void HandleClick(InputAction.CallbackContext ctx)
    {
        //Debug.Log("HandleClick called");

        if (InputManager.Instance.IsPointerOverUIWhenClick() || _isEventPlaying)
            return; // UI 위에서 클릭한 경우 또는 이벤트 진행 중일 때 대화 진행 방지

        SkipLine();
    }

    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        //Debug.Log("HandleInteraction called");

        if (_isEventPlaying)
            return; // 이벤트 진행 중일 때는 상호작용 무시

        switch(ctx.control.displayName)
        {
            case "Space":
            case "Enter":
                SkipLine();
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

    public void SetDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
        {
            Debug.LogError("DialogueData가 null입니다. 대화를 시작할 수 없습니다.");
            return;
        }

        _dialogueData = dialogueData;
        InitializeStory(_dialogueData.inkJSON);

        /*
        ITextBoxTarget[] textBoxTargets = FindObjectsByType<MonoBehaviour>().OfType<ITextBoxTarget>().ToArray();
        Debug.Log($"발견된 ITextBoxTarget 개수: {textBoxTargets.Length}");
        //말풍선 구독
        for (int i = 0; i < textBoxTargets.Length; i++)
        {
            //Debug.Log($"{i}번째 말풍선 등록: {_textBoxes[i].name}");
            TextBoxRouter.RegisterTextBox(i, textBoxTargets[i]);
        }
        */
        
        TextMeshProUGUI textBox = GameObject.FindWithTag("DialogueTextBox")?.GetComponent<TextMeshProUGUI>();
        TextBoxRouter.Instance.BeginLayer("baseLayer"); // 기본 레이어 시작

        if (textBox != null)
        {
            TextBoxRouter.Instance.RegisterTextBox(0, textBox);
        }
        else
        {
            Debug.LogWarning("TextBox 태그가 지정된 GameObject를 찾을 수 없습니다.");
            return;
        }

        InitializeSceneElements(_dialogueData);
    }

    private void InitializeSceneElements(DialogueData dialogueData)
    {
        if (dialogueData.backgroundImage != null)
        {
            var backgroundImage = GameObject.FindWithTag("BackgroundImg").GetComponent<SpriteRenderer>();
            if (backgroundImage != null)
            {
                backgroundImage.sprite = dialogueData.backgroundImage;
            }
            else
            {
                Debug.LogError("Background GameObject not found!");
            }
        }
        else Debug.Log("DialogueData에 배경 이미지가 할당되지 않았습니다.");

        // 캐릭터 이미지 및 위치 설정
        for (int i = 0; i < dialogueData.characterInfos.Count; i++)
        {
            var characterPair = dialogueData.characterInfos[i];
            var characterGO = GameObject.Find($"Character{i + 1}"); // 추후에는 캐릭터 오브젝트를 생성할 수 있도록 수정
            if (characterGO != null)
            {
                var characterSpriteRenderer = characterGO.GetComponent<SpriteRenderer>();
                if (characterSpriteRenderer != null)
                {
                    characterSpriteRenderer.sprite = characterPair.Key;
                    characterGO.transform.position = characterPair.Value;
                }
                else
                {
                    Debug.LogError($"Character{i + 1} GameObject에 SpriteRenderer가 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"Character{i + 1} GameObject를 찾을 수 없습니다.");
            }
        }

        // BGM 재생
        if (dialogueData.bgm != null)
        {
            //SoundManager.Instance.PlayBGM(dialogueData.bgm);
        }
    }

    public void SetEventPlaying(bool isPlaying)
    {
        _isEventPlaying = isPlaying;
    }

    public void SetDefaultTextColor(Color color)
    {
        _defaultTextColor = color;
    }

    private void InitFields()
    {
        _textBoxIndex = 0;
        _nextTextColor = Color.black;
    }

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
    
    void InitializeStory(TextAsset inkJSON)
    {
        if (inkJSON == null)
        {
            Debug.LogWarning("Ink JSON 파일이 할당되지 않았습니다.");
            _story = null;
            return;
        }
        _story = new Story(inkJSON.text);
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

        _story.BindExternalFunction("PlayCutScene", (string cutSceneType) => {
            Debug.Log($"PlayCutScene called with: {cutSceneType}");
            if (Enum.TryParse(cutSceneType, true, out CutSceneManager.CutSceneType type))
            {
                CutSceneManager.Instance?.PlayCutScene(type);
            }
            else
            {
                Debug.LogError($"Ink에서 잘못된 컷씬 타입을 보냈습니다: {cutSceneType}");
            }
        });
        
        _story.BindExternalFunction("UpdateStatusRecoveryCount", (bool value) => {
            Debug.Log($"UpdateStatusRecoveryCount called with: {value}");
            InkStatusManager.Instance?.UpdateStatusRecoveryCount(value);
        });

        _story.BindExternalFunction("AddIntel", (int points) => {
            Debug.Log($"AddIntel called with: {points}");
            InkStatusManager.Instance?.AddIntel(points);
        });

        _story.BindExternalFunction("AddCreativeFlagCount", () => {
            Debug.Log("AddCreativeFlagCount called");
            InkStatusManager.Instance?.AddCreativeFlagCount();
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
            return InkStatusManager.Instance?.GetObjectiveState();
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
    
    public void PlayDialogue()
    {
        if (_story == null)
        {
            Debug.LogWarning("Ink 스토리가 초기화되지 않았습니다. 대화를 시작할 수 없습니다.");
            return;
        }
        //_playerSpeechPanel.SetActive(true);
        StartCoroutine(ContinueStory());
    }
    
    IEnumerator ContinueStory()
    {
        // 텍스트 출력
        while (_story.canContinue)
        {
            string text = _story.Continue();
            _processedText = text.Trim();

            ProcessTags(_story.currentTags);
            
            yield return null;
            yield return new WaitUntil(() => _isEventPlaying == false); // 대기 시간 적용 여부 확인

            // 빈 문자열(엔터 등)일 경우 클릭 대기를 건너뛰고 바로 다음으로 진행
            if (string.IsNullOrEmpty(_processedText))
            {
                continue; 
            }

            // 타이핑 시작
            // 타이핑이 끝날 때까지  대기
            yield return StartCoroutine(TypeText(_processedText));

            _isWaitingForAdvance = true;
            yield return new WaitUntil(() => !_isWaitingForAdvance);
        }
        
        // 선택지 표시
        DisplayChoices(out bool choicesAvailable);

        if (!choicesAvailable)
        {
            EndDialogue();
        }
    }

    void SkipLine()
    {
        if (_isTyping)
        {
            CompleteTypingText();
        }
        else if (_isWaitingForAdvance)
        {
            _isWaitingForAdvance = false;

            if (_pendingPopupAfterAdvance.HasValue)
            {
                ETutorialPopupType popupType = _pendingPopupAfterAdvance.Value;
                _pendingPopupAfterAdvance = null;
                _isEventPlaying = true;
                TutorialPopupManager.Instance.ShowPopup(popupType, OnPopupClosed);
            }
        }
    }

    private void CompleteTypingText()
    {
        TextMeshProUGUI target = TextBoxRouter.Instance.GetTextBox(_textBoxIndex);
        if (target != null)
        {
            target.text = _processedText;
        }

        _isTyping = false;
        _textBoxIndex = 0;
        _nextTextColor = Color.clear;
    }

    IEnumerator TypeText(string text)
    {
        _isTyping = true;

        //ITextBoxTarget target = TextBoxRouter.GetTextBox(_speakerIndex);
        TextMeshProUGUI target = TextBoxRouter.Instance.GetTextBox(_textBoxIndex);
        if (target == null)
        {
            Debug.LogError($"textBox {_textBoxIndex}에 등록된 텍스트박스가 없습니다.");
            _isTyping = false;
            yield break;
        }

        //target.BubbleInit();
        target.text = ""; // 말풍선 초기화
        string tmpText = "";
        if (_nextTextColor == Color.clear) 
        {
            //target.SetTextColor(_defaultTextColor); // 기본 색상 설정
            target.color = _defaultTextColor; // 기본 색상 설정
        }
        else 
        {
            //target.SetTextColor(_nextTextColor);
            target.color = _nextTextColor;
            _nextTextColor = Color.clear; // 텍스트 색상 초기화
        }

        for (int i = 0; i < text.Length; i++)
        {
            tmpText += text[i];
            //target.UpdateBubble(tmpText);
            target.text = tmpText;
            yield return new WaitForSeconds(_textSpeed);

            if (!_isTyping)
            {
                yield break;
            }
        }
        _isTyping = false;
        _textBoxIndex = 0; // 스피커 인덱스 초기화
        _nextTextColor = Color.clear; // 텍스트 색상 초기화
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
                //_speakerIndex = int.Parse(value);
                _textBoxIndex = 0; // 현재는 단일 말풍선만 사용하므로 항상 0으로 설정
                break;
            case "name":
                // 화자 이름 변경
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
                        _nextTextColor = Color.clear;
                        break;
                }
                break;
            case "popupAfter":
                // 현재 대사를 출력한 뒤 다음 입력에서 팝업 표시
                if (Enum.TryParse(value, true, out ETutorialPopupType popupAfterType))
                {
                    _pendingPopupAfterAdvance = popupAfterType;
                }
                else
                {
                    Debug.LogWarning($"알 수 없는 팝업 타입: {value}");
                }
                break;
            default:
                Debug.LogWarning($"알 수 없는 태그: {key}:{value}");
                break;
        }
    }
    
    void DisplayChoices(out bool choicesAvailable)
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
            
            _nextTextColor = Color.clear; // 선택지 색상 초기화
        }

        choicesAvailable = _story.currentChoices.Count > 0;
    }

    private void OnPopupClosed()
    {
        _isEventPlaying = false; // 다시 클릭으로 대사 진행 가능
        
        SkipLine();
    }
    
    void OnChoiceSelected(int choiceIndex)
    {
        _isWaitingForAdvance = false;
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
        Debug.Log("대화 종료");
    }
    
    public void JumpToKnot(string knotName)
    {
        _isWaitingForAdvance = false;
        _story.ChoosePathString(knotName);
        StartCoroutine(ContinueStory());
    }
}
