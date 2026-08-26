using UnityEngine;
using TMPro;

[System.Serializable]
public class GameDataSlot
{
    [SerializeField] private TextMeshProUGUI _chapterText;
    [SerializeField] private TextMeshProUGUI _ddayText;

    public void SetData(GameSaveData saveData)
    {
        //chapterText.text = $"Chapter: {saveData.ChapterIndex}";
        //ddayText.text = $"D-Day: {saveData.DDayIndex}";
        _chapterText.text = $"{saveData.ChapterIndex}";
        _ddayText.text = $"{saveData.DDayIndex}";
    }
}

public class GameDataPopup : MonoBehaviour
{
    [ReadOnly] private bool _isLoad = true; // 저장/불러오기 모드
    [SerializeField] private GameDataSlot[] _gameDataSlots; // 게임 데이터 슬롯들을 배열로 관리

    [SerializeField] private DialogueData _dialogueData; // 슬롯에 표시할 DialogueData

    public void OpenPopup(bool isLoad)
    {
        _isLoad = isLoad;
        gameObject.SetActive(true);
        RefreshSlots(); // 팝업이 열릴 때 슬롯을 새로고침
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    // 슬롯 클릭 시 호출되는 콜백
    public void OnSlotClicked(int slotIndex)
    {
        if (_isLoad)
        {
            // 로드 모드일 때: 데이터 로드 확인 팝업 띄우기 -> 씬 전환/데이터 로드
            if (DataManager.Instance.PeekGameData(slotIndex) == null)
            {
                Debug.Log($"선택한 슬롯 {slotIndex}에는 저장된 데이터가 없습니다.");
                return; // 저장된 데이터가 없는 경우 로드하지 않음
            }
            // 씬 전환 또는 필요한 후속 작업 수행
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            LoadingSceneController.Instance.LoadScene(SceneNames.InvestigationScene, () =>
            {
                //DialogueManager.Instance.SetDialogue(_dialogueData);
                DataManager.Instance.LoadGameData(slotIndex);
                Debug.Log($"게임 데이터가 {slotIndex}번 슬롯에서 로드되었습니다.");
            });
        }
        else
        {
            DataManager.Instance.SaveGameData(DataManager.Instance.currentSaveData, slotIndex);
            Debug.Log($"게임 데이터가 {slotIndex}번 슬롯에 저장되었습니다.");
            RefreshSlots(); // 저장 후 슬롯을 새로고침하여 변경 사항 반영
        }
    }

    private void RefreshSlots()
    {
        // 게임 데이터 팝업이 활성화될 때, 현재 저장된 게임 데이터를 불러옵니다.
        foreach (var slot in _gameDataSlots)
        {
            GameSaveData saveData = DataManager.Instance.PeekGameData(System.Array.IndexOf(_gameDataSlots, slot));

            if (saveData != null)
            {
                // 저장된 데이터가 있는 경우, 슬롯에 데이터를 표시합니다.
                slot.SetData(saveData);
            }
        }
    }

    void Awake()
    {
        //RefreshSlots(); // 팝업이 활성화될 때 슬롯을 새로고침
        //gameObject.SetActive(false); // 처음에는 팝업을 비활성화 상태로 설정
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
