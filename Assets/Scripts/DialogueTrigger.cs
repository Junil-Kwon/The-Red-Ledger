using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    // 트리거에 연결된 DialogueData -> 추후에 데이터를 읽고 씬에 적용하는 로직을 구현해야 함
    [SerializeField] private DialogueData _dialogueData; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DialogueManager.Instance.SetDialogue(_dialogueData);
    }

    // Update is called once per frame
    void Start()
    {
        DialogueManager.Instance.PlayDialogue();
    }
}
