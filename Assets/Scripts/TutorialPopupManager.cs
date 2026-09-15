using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PrimeTween;

public enum ETutorialPopupType
{
    ImmedDetected,
    FlagCreatTrans,
}

public class TutorialPopupManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _outerBackground; // 팝업 배경 이미지
    [SerializeField] private GameObject _popupUI;
    private TextMeshProUGUI _popupText; // 팝업 텍스트 컴포넌트
    
    [Header("Settings")]
    private float _typingSpeed = 0.05f; // 글자 출력 속도
    private float _outerBGAlpha = 220 / 255f; // 배경 투명도 (0~1)
    private float _textBoxLocationY = -260f; // 텍스트 박스 위치 Y 좌표

    private Action _onPopupCloseCallback;
    private string[] _sentences;
    private int _currentSentenceIndex = 0;
    
    private bool _isTyping = false; // 현재 텍스트 연출 중인지 체크
    private Coroutine _typingCoroutine;

    private static TutorialPopupManager _instance;

    public static TutorialPopupManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<TutorialPopupManager>();
                if (_instance == null)
                {
                    Debug.Log("TutorialPopupManager가 씬에 없습니다. 새로 생성합니다.");
                    _instance = new GameObject("TutorialPopupManager").AddComponent<TutorialPopupManager>();
                }
            }
            return _instance;
        }
    }

    private Dictionary<ETutorialPopupType, string[]> _tutorialSentences = new Dictionary<ETutorialPopupType, string[]>
    {
        { ETutorialPopupType.ImmedDetected, new string[] { "통역 내용이 의심받고 있습니다. 지금 당신에게는 한 번의 기회가 있습니다.",
                                                           "사실대로 말하거나, 둘러대거나.",
                                                           "단 — 이번 심문에서 다시 발각되면 그 순간이 마지막입니다" } },
        { ETutorialPopupType.FlagCreatTrans, new string[] { "탐색에서 얻은 단서가 새로운 통역을 가능하게 합니다.",
                                                            "이 통역은 포로에게 직접 전달되며, 심문관은 내용을 알 수 없습니다.",
                                                            "단, 포로의 반응을 심문관이 눈치챌 수 있습니다. 그 때는 상황을 수습하십시오.", 
                                                            "이 통역으로 인한 발각은 누적되지 않습니다." } },
    };

    public void ShowPopup(ETutorialPopupType type, Action onCloseCallback = null)
    {
        _sentences = _tutorialSentences.GetValueOrDefault(type);
        if (_sentences == null || _sentences.Length == 0)
        {
            Debug.LogWarning("No sentences provided for the tutorial popup.");
            return;
        }
        
        _outerBackground.gameObject.SetActive(true);
        _popupUI.SetActive(true);

        var showSequence = Sequence.Create();
        showSequence.Group(Tween.Alpha(_outerBackground, 0f, _outerBGAlpha, 0.5f, Ease.Linear));
        showSequence.Group(Tween.LocalPositionY(_popupUI.transform, -1000f, _textBoxLocationY, 0.5f, Ease.OutBack));
        showSequence.OnComplete(() =>
        {
            _currentSentenceIndex = 0;
            _onPopupCloseCallback = onCloseCallback;

            InputManager.Instance.Input.Player.Click.performed += HandleClick; // 입력 이벤트 등록
            InputManager.Instance.Input.Player.Interact.performed += HandleInteract; // 입력 이벤트 등록

            ShowNextSentence();
        });
    }

    private void HandleClick(InputAction.CallbackContext ctx)
    {
        if (InputManager.Instance.IsPointerOverUIWhenClick()) return; // UI 위에서 클릭한 경우 대화 진행 방지

        if (_isTyping)
        {
            StopCoroutine(_typingCoroutine);
            _popupText.text = _sentences[_currentSentenceIndex];
            _isTyping = false;
        }
        else
        {
            _currentSentenceIndex++;
            ShowNextSentence();
        }
    }

    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        if(ctx.control.displayName is "Space" or "Enter")
        {
            if (_isTyping)
            {
                StopCoroutine(_typingCoroutine);
                _popupText.text = _sentences[_currentSentenceIndex];
                _isTyping = false;
            }
            else
            {
                _currentSentenceIndex++;
                ShowNextSentence();
            }
        }
    }

    private void ShowNextSentence()
    {
        // 출력할 문장이 남아있는 경우
        if (_currentSentenceIndex < _sentences.Length)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeSentence(_sentences[_currentSentenceIndex]));
        }
        // 모든 문장을 다 출력한 경우
        else
        {
            ClosePopup();

            InputManager.Instance.Input.Player.Click.performed -= HandleClick; // 입력 이벤트 해제
            InputManager.Instance.Input.Player.Interact.performed -= HandleInteract; // 입력 이벤트 해제
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        _popupText.text = "";
        
        // 한 글자씩 더해가며 출력
        foreach (char letter in sentence.ToCharArray())
        {
            _popupText.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }
        
        _isTyping = false;
    }

    private void ClosePopup()
    {
        var closeSequence = Sequence.Create();
        closeSequence.Group(Tween.Alpha(_outerBackground, _outerBGAlpha, 0f, 0.5f, Ease.Linear));
        closeSequence.Group(Tween.LocalPositionY(_popupUI.transform, _textBoxLocationY, -1000f, 0.5f, Ease.InBack));
        closeSequence.OnComplete(() =>
        {
            _outerBackground.gameObject.SetActive(false);
            _popupUI.gameObject.SetActive(false);
            _popupText.text = "";

            _sentences = null; // 데이터 초기화

            // DialogueManager의 잠금을 해제하는 콜백 실행
            _onPopupCloseCallback?.Invoke(); 
            _onPopupCloseCallback = null; 
        });
    }

    // ============== Lifecycle Methods ==============
    void Awake()
    {
        _popupText = _popupUI.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
