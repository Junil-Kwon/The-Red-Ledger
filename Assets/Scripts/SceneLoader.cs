using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    /*
    // 슬롯 클릭 또는 로드 실행 시 호출
    public void LoadGameFromSlot(int slotIndex)
    {
        // 1. 슬롯 데이터 가져오기
        GameSaveData saveData = DataManager.Instance.PeekGameData(slotIndex);

        if (saveData == null)
        {
            Debug.LogWarning("해당 슬롯에 세이브 데이터가 없습니다.");
            return;
        }

        // 2. 현재 읽어온 데이터를 글로벌 DataManager의 'activeData'로 설정
        DataManager.Instance.SetCurrentSaveData(saveData);

        // 3. 데이터를 바탕으로 이동할 씬 결정
        string targetSceneName = GetTargetSceneName(saveData);

        // 4. 결정된 씬으로 이동 (비동기 씬 로드 등)
        SceneManager.LoadSceneAsync(targetSceneName);
    }

    // 로드된 데이터에 따라 이동할 씬 이름을 결정해주는 매핑 함수
    private string GetTargetSceneName(GameSaveData saveData)
    {
        // Case A: 데이터에 씬 이름이 직접 저장된 경우
        if (!string.IsNullOrEmpty(saveData.lastSceneName))
        {
            return saveData.lastSceneName;
        }

        // Case B: 챕터/단계 ID에 따라 씬을 분기시키는 경우
        /*
        switch (saveData.chapterId)
        {
            case 1: return "Scene_Chapter_01";
            case 2: return "Scene_Chapter_02";
            default: return "Scene_Prologue";
        }
        */
/*
        return "Scene_Prologue"; // 기본 예외 처리
    }
    */
}
