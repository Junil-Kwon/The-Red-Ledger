using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink")]
    [SerializeField] private TextAsset inkJSON;
    private Story story;
    
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private List<TextMeshProUGUI> speakerTexts;
    //[SerializeField] private GameObject choiceButtonPrefab;
    //[SerializeField] private Transform choiceContainer;
    private int speakerIndex = 0;
    private string processedText;
    private Coroutine typingCoroutine;

    [Header("Dialogue Settings")]
    [SerializeField] private float textSpeed = 0.05f;

    [SerializeField]private List<GameObject> currentChoices = new List<GameObject>();
    private bool waitingForAdvance = false;
    private bool advanceRequested = false;

    void OnEnable()
    {
        if (PlayerInputController.Instance != null)
        {
            PlayerInputController.Instance.Input.Player.Click.performed += ctx => HandleTap();
        }
    }

    void OnDisable()
    {
        if (PlayerInputController.Instance != null)
        {
            PlayerInputController.Instance.Input.Player.Click.performed -= ctx => HandleTap();
        }
    }

    private void HandleTap()
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
            // 대화가 진행 중이지만 텍스트가 완전히 출력되지 않은 경우, 즉시 전체 텍스트 표시
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                speakerTexts[speakerIndex].text = processedText;
                waitingForAdvance = true;
                advanceRequested = false;
            }
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

            // 타이핑이 끝날 때까지 OR 유저가 스킵할 때까지 대기
            // (TypeText 내부에서 완료 시 typingCoroutine = null 처리를 해준다고 가정)
            yield return new WaitUntil(() => typingCoroutine == null || advanceRequested);
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
