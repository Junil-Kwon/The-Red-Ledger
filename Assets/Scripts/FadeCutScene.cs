using PrimeTween;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class FadeCutScene : MonoBehaviour
{
    [SerializeField] private List<GameObject> _boxes = new();
    [SerializeField] private float _fadeDuration = 1f;

    private TextBoxLayer _layer;
    private Sequence _fadeSequence;

    public void Show(bool isFade = true)
    {
        if (_layer != null)
        {
            Debug.LogWarning("FadeCutScene is already active. Show() called again without Hide().");
            return;
        }

        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager not found. Cut scene processing flag not set.");
            return;
        }

        gameObject.SetActive(true);
        dialogueManager.SetCutSceneProcessing(true); // 컷씬 진행 중임을 알림

        _layer = TextBoxRouter.Instance.BeginLayer("FadeCutScene");

        _fadeSequence.Stop();

        int speakerIndex = 0;
        foreach (var box in _boxes)
        {
            _layer.Set(speakerIndex, box.GetComponent<ITextBoxTarget>());
            speakerIndex++;
        }

        if (isFade)
        {
            // 페이드인이 필요할 때만 시퀀스를 깔끔하게 딱 1번 생성해서 등록
            var seq = Sequence.Create();
            seq.Group(Tween.Alpha(GetComponent<CanvasGroup>(), 1f, _fadeDuration, Ease.Linear));
            seq.OnComplete(() =>
            {
                dialogueManager.SetCutSceneProcessing(false); // 컷씬 진행 완료 알림
            });
            
            _fadeSequence = seq;
        }
        else
        {
            // 즉시 표시일 때는 트윈 시스템을 전혀 쓰지 않음
            GetComponent<CanvasGroup>().alpha = 1f; 
            dialogueManager.SetCutSceneProcessing(false);
            _fadeSequence = default; // 또는 null 처리 (구조에 따라 지정)
        }
    }

    public void Hide(bool isFade = true)
    {
        if (_layer == null)
        {
            Debug.LogWarning("FadeCutScene is not active. Hide() called without Show().");
            return;
        }

        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager not found. Cut scene processing flag not set.");
            return;
        }

        dialogueManager.SetCutSceneProcessing(true); // 컷씬 진행 중임을 알림

        _fadeSequence.Stop();

        if (isFade)
        {
            // 페이드인이 필요할 때만 시퀀스를 깔끔하게 딱 1번 생성해서 등록
            var seq = Sequence.Create();
            seq.Group(Tween.Alpha(GetComponent<CanvasGroup>(), 0f, _fadeDuration, Ease.Linear));
            seq.OnComplete(() => {
                gameObject.SetActive(false);
                dialogueManager.SetCutSceneProcessing(false);
            }); // 페이드 아웃 완료 후 비활성화
            
            _fadeSequence = seq;
        }
        else
        {
            // 즉시 표시일 때는 트윈 시스템을 전혀 쓰지 않음
            GetComponent<CanvasGroup>().alpha = 0f;
            gameObject.SetActive(false);
            dialogueManager.SetCutSceneProcessing(false);
            _fadeSequence = default; // 또는 null 처리 (구조에 따라 지정)
        }

        TextBoxRouter.Instance.PopLayer(_layer); // 모든 인덱스가 한 번에 원상복귀
        _layer = null;
    }

    void OnDisable()
    {
        _fadeSequence.Stop();
        
        foreach (var box in _boxes)
        {
            if (box != null)
            {
                var target = box.GetComponent<ITextBoxTarget>();
                if (target != null)
                {
                    target.BubbleInit(); // 말풍선 초기화
                }
            }
        }

        if (_layer != null)
        {
            TextBoxRouter.Instance?.PopLayer(_layer);
            _layer = null;
        }
    }
}