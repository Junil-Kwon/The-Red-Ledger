using PrimeTween;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct FadeConfig
{
    public bool isFade;
    public bool isWhite;
}

public class FadeCutScene : ACutScene
{
    [SerializeField] private List<GameObject> _textBoxes = new();
    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private Image _backgroundImage;

    [SerializeField] private FadeConfig[] _modeConfigs = new FadeConfig[]
    {
        new FadeConfig { isWhite = false, isFade = true, }, // Mode 0
        new FadeConfig { isWhite = false, isFade = false,  }, // Mode 1
        new FadeConfig { isWhite = true,  isFade = true, }, // Mode 2
        new FadeConfig { isWhite = true,  isFade = false,  }, // Mode 3
    };

    private TextBoxLayer _layer;
    private Sequence _fadeSequence;

    public override void Show(int mode)
    {
        FadeConfig config = _modeConfigs[mode];

        if (_layer != null)
        {
            Debug.LogWarning("FadeCutScene is already active. Show() called again without Hide().");
            return;
        }
        /*
        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager not found. Cut scene processing flag not set.");
            return;
        }
        */

        DialogueManager.Instance.SetDefaultTextColor(config.isWhite ? Color.black : Color.white); // 텍스트 색상 설정
        _backgroundImage.color = config.isWhite ? Color.white : Color.black;
        gameObject.SetActive(true);
        DialogueManager.Instance.SetCutSceneProcessing(true); // 컷씬 진행 중임을 알림

        _layer = TextBoxRouter.Instance.BeginLayer("FadeCutScene");

        _fadeSequence.Stop();

        int boxIndex = 0;
        foreach (var box in _textBoxes)
        {
            //_layer.Set(speakerIndex, box.GetComponent<ITextBoxTarget>());
            //_layer.Set(boxIndex, box.GetComponent<TextMeshProUGUI>());
            TextBoxRouter.Instance.RegisterTextBox(boxIndex, box.GetComponent<TextMeshProUGUI>());
            boxIndex++;
        }

        if (config.isFade)
        {
            // 페이드인이 필요할 때만 시퀀스를 깔끔하게 딱 1번 생성해서 등록
            var seq = Sequence.Create();
            seq.Group(Tween.Alpha(GetComponent<CanvasGroup>(), 0f, 0f, Ease.Linear)); // 초기 알파값 설정
            seq.Group(Tween.Alpha(GetComponent<CanvasGroup>(), 1f, _fadeDuration, Ease.Linear));
            seq.OnComplete(() =>
            {
                DialogueManager.Instance.SetCutSceneProcessing(false); // 컷씬 진행 완료 알림
            });
            
            _fadeSequence = seq;
        }
        else
        {
            // 즉시 표시일 때는 트윈 시스템을 전혀 쓰지 않음
            GetComponent<CanvasGroup>().alpha = 1f; 
            DialogueManager.Instance.SetCutSceneProcessing(false);
            _fadeSequence = default; // 또는 null 처리 (구조에 따라 지정)
        }
    }

    public override void Hide(int mode)
    {
        FadeConfig config = _modeConfigs[mode];

        if (_layer == null)
        {
            Debug.LogWarning("FadeCutScene is not active. Hide() called without Show().");
            return;
        }
        /*
        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager not found. Cut scene processing flag not set.");
            return;
        }
        */

        DialogueManager.Instance.SetCutSceneProcessing(true); // 컷씬 진행 중임을 알림

        _fadeSequence.Stop();

        if (config.isFade)
        {
            // 페이드인이 필요할 때만 시퀀스를 깔끔하게 딱 1번 생성해서 등록
            var seq = Sequence.Create();
            seq.Group(Tween.Alpha(GetComponent<CanvasGroup>(), 1f, 0f, Ease.Linear)); // 초기 알파값 설정
            seq.Group(Tween.Alpha(GetComponent<CanvasGroup>(), 0f, _fadeDuration, Ease.Linear));
            seq.OnComplete(() => {
                gameObject.SetActive(false);
                DialogueManager.Instance.SetCutSceneProcessing(false);
                DialogueManager.Instance.SetDefaultTextColor(Color.black); // 텍스트 색상 설정
            }); // 페이드 아웃 완료 후 비활성화
            
            _fadeSequence = seq;
        }
        else
        {
            // 즉시 표시일 때는 트윈 시스템을 전혀 쓰지 않음
            GetComponent<CanvasGroup>().alpha = 0f;
            gameObject.SetActive(false);
            DialogueManager.Instance.SetCutSceneProcessing(false);
            _fadeSequence = default; // 또는 null 처리 (구조에 따라 지정)
        }

        TextBoxRouter.Instance.PopLayer(); // 모든 인덱스가 한 번에 원상복귀
        _layer = null;
    }

    void OnDisable()
    {
        _fadeSequence.Stop();
        
        foreach (var box in _textBoxes)
        {
            if (box != null)
            {
                /*
                var target = box.GetComponent<ITextBoxTarget>();
                if (target != null)
                {
                    target.BubbleInit(); // 말풍선 초기화
                }
                */
                var tmp = box.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = ""; // 말풍선 초기화
                }
            }
        }

        if (_layer != null)
        {
            TextBoxRouter.Instance?.PopLayer();
            _layer = null;
        }
    }
}