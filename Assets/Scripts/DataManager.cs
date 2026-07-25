using System.IO;
using System.Collections.Generic;
using UnityEngine;
using StoryFlags; // StoryFlag enum이 정의된 네임스페이스를 임포트

[System.Serializable]
public class GameSettings
{
    [SerializeField] private float BgmVolume = 1.0f;
    [SerializeField] private float SfxVolume = 1.0f;
    //private ScriptSpeedState ScriptSpeed = ScriptSpeedState.Normal;
}

[System.Serializable]
public class GameSaveData
{
    /*
    public enum EInGamePart
    {
        Investigation,
        Interpretation,
    }
    */

    // HashSet 대신 JsonUtility가 인식할 수 있는 List로 변경: HashSet은 JsonUtility에서 직렬화되지 않으므로 List로 변경
    [SerializeField] private List<EStoryFlag> storyFlags = new List<EStoryFlag>();
    [SerializeField] private int chapterIndex = 1; // 현재 진행 중인 챕터 인덱스
    [SerializeField] private int dayIndex = 1; // 현재 진행 중인 날짜 인덱스

    public List<EStoryFlag> StoryFlags => storyFlags;
    public int ChapterIndex => chapterIndex;
    public int DayIndex => dayIndex;

    public void AddStoryFlag(EStoryFlag flag)
    {
        // HashSet처럼 중복 방지를 위해 확인 후 추가
        if (!storyFlags.Contains(flag))
        {
            storyFlags.Add(flag);
        }
    }

    public void ChapterClear()
    {
        chapterIndex++; // 챕터 인덱스 증가
        dayIndex = 1; // 챕터가 바뀌면 날짜 인덱스는 초기화
    }

    public void NextDay()
    {
        dayIndex++; // 날짜 인덱스 증가
    }
}

public class DataManager : Singleton<DataManager>
{
    public GameSaveData currentSaveData {get; private set;} = null;
    //private int currentSaveSlot = 0; // 현재 선택된 세이브 슬롯
    public GameSettings currentSettings {get; private set;} = new GameSettings();
    private string saveFolderPath => Path.Combine(Application.persistentDataPath, "SaveData");

    protected override void Awake()
    {
        base.Awake();

        // 저장용 폴더가 없다면 미리 생성
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            Debug.Log("Save folder created at: " + saveFolderPath);
        }

        CreateNewGame(); // 새 게임 데이터 초기화(이후에는 수정)
    }

    #region [1] 설정 파일 (System Settings) 관련
    private string SettingsPath => Path.Combine(saveFolderPath, "settings.json");

    public void SaveSettings(GameSettings settings)
    {
        string json = JsonUtility.ToJson(settings, true); // true로 주면 가독성 좋게 정렬됨
        File.WriteAllText(SettingsPath, json);
    }

    public void LoadSettings()
    {
        if (!File.Exists(SettingsPath)) return; // 파일 없으면 기본값 반환

        string json = File.ReadAllText(SettingsPath);
        currentSettings = JsonUtility.FromJson<GameSettings>(json);
    }
    #endregion

    #region [2] 세이브 파일 (Game Save Data) 관련
    // 슬롯 번호(1, 2, 3...)를 받아 각각 다른 파일로 저장
    private string GameSavePath(int slotIndex) => Path.Combine(saveFolderPath, $"save_slot_{slotIndex:D2}.json");

    public void CreateNewGame()
    {
        currentSaveData = new GameSaveData();
    }

    public void SaveGameData(GameSaveData data, int slotIndex)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GameSavePath(slotIndex), json);
    }

    public void LoadGameData(int slotIndex)
    {
        string path = GameSavePath(slotIndex);
        if (!File.Exists(path)) return; // 세이브 파일이 없으면 null 반환 (새 게임 시작용)

        string json = File.ReadAllText(path);
        currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
    }

    public void DeleteGameData(int slotIndex)
    {
        string path = GameSavePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file deleted: {path}");
        }
    }

    public void TriggerStoryFlag(EStoryFlag flag)
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("No save data loaded. Cannot set story flag.");
            return; // 세이브 데이터가 없으면 경고 후 종료
        }
        
        currentSaveData.AddStoryFlag(flag);
    }

    public void ClearChapter()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("No save data loaded. Cannot clear chapter.");
            return; // 세이브 데이터가 없으면 경고 후 종료
        }

        currentSaveData.ChapterClear();
    }

    public void AdvanceToNextDay()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("No save data loaded. Cannot advance to next day.");
            return; // 세이브 데이터가 없으면 경고 후 종료
        }

        currentSaveData.NextDay();
    }

    #endregion
}