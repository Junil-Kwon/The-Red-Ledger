using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameDialogueManager : MonoBehaviour
{
    [Header("Ink")]
    [SerializeField] private TextAsset inkJSON;
    private Story story;
    
    [Header("Speaker UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private List<TextMeshProUGUI> speakerTexts;
    [SerializeField] private List<GameObject> currentChoices = new List<GameObject>();
    //[SerializeField] private GameObject choiceButtonPrefab;
    //[SerializeField] private Transform choiceContainer;

    [Header("Dialogue Settings")]
    [SerializeField] private float textSpeed = 0.05f;
    [Range(0f, 1f)]
    [SerializeField] private float suspicion = 0f; // 의심 수치 (0~1)

    private int speakerIndex = 0;
    private string processedText;
    private Coroutine typingCoroutine;
    private bool waitingForAdvance = false;
    private bool advanceRequested = false;
    private bool skipFlag = false; // 타이핑 스킵 플래그

    void OnEnable()
    {
        if (PlayerInputController.Instance != null)
        {
            PlayerInputController.Instance.Input.Player.Click.performed += HandleTap;
        }
    }

    void OnDisable()
    {
        if (PlayerInputController.Instance != null)
        {
            PlayerInputController.Instance.Input.Player.Click.performed -= HandleTap;
        }
    }

    private void HandleTap(InputAction.CallbackContext ctx)
    {
        Debug.Log("HandleTap called");

        if (PlayerInputController.Instance.IsPointerOverUI())
            return; // UI 위에서 클릭한 경우 대화 진행 방지

        if (waitingForAdvance)
        {
            advanceRequested = true;
        }
        else
        {
            if (typingCoroutine != null) skipFlag = true; // 타이핑 중이면 스킵 플래그 설정
        }
    }
    
    void Start()
    {
        InitializeStory();
        StartDialogue();
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
        story.BindExternalFunction("PlaySound", (string soundName) => {
            // 유니티 내부에서의 사운드 재생 로직
        });
        
        story.BindExternalFunction("ChangeScene", (string sceneName) => {
            // 유니티 내부에서의 씬 전환 로직
        });
    }
    
    public void StartDialogue()
    {
        waitingForAdvance = false;
        advanceRequested = false;
        dialoguePanel.SetActive(true);
        StartCoroutine(ContinueStory());
    }
    
    IEnumerator ContinueStory()
    {
        // 텍스트 출력
        while (story.canContinue)
        {
            string text = story.Continue();
            processedText = text.Trim();
            ProcessTags(story.currentTags);

            // 타이핑 시작
            typingCoroutine = StartCoroutine(TypeText(processedText));

            // 타이핑이 끝날 때까지  대기
            yield return typingCoroutine;
            typingCoroutine = null;

            waitingForAdvance = true;
            advanceRequested = false;
            yield return new WaitUntil(() => advanceRequested);
            waitingForAdvance = false;
            advanceRequested = false;
        }
        
        // 선택지 표시
        DisplayChoices();
    }

    IEnumerator TypeText(string text)
    {
        speakerTexts[speakerIndex].text = "";

        for (int i = 0; i < text.Length; i++)
        {
            speakerTexts[speakerIndex].text += text[i];
            yield return new WaitForSeconds(textSpeed);

            if (skipFlag) 
            {
                skipFlag = false; // 스킵 플래그 초기화
                speakerTexts[speakerIndex].text = text; // 전체 텍스트 즉시 표시
                yield break; // 스킵 플래그가 설정되면 타이핑 중단
            }
        }
    }
    
    void ProcessTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            string[] parts = tag.Split(':');
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                
                // 태그 처리 로직
                HandleTag(key, value, ref speakerIndex);
            }
        }
    }
    
    void HandleTag(string key, string value, ref int speakerIndex)
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
            case "audio":
                // 오디오 재생
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
            currentChoices[choiceIndex].GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            
            currentChoices[choiceIndex].GetComponent<Button>().onClick.AddListener(() => {
                OnChoiceSelected(choiceIndex);
            });
            
            //currentChoices.Add(choiceButton);
            currentChoices[choiceIndex].SetActive(true);
        }
        
        if (story.currentChoices.Count == 0)
        {
            // 대화 종료
            EndDialogue();
        }
    }
    
    void OnChoiceSelected(int choiceIndex)
    {
        waitingForAdvance = false;
        advanceRequested = false;
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
        dialoguePanel.SetActive(false);
    }
    
    public void JumpToKnot(string knotName)
    {
        waitingForAdvance = false;
        advanceRequested = false;
        story.ChoosePathString(knotName);
        StartCoroutine(ContinueStory());
    }
}
