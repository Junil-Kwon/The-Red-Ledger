using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 플레이 모드 종료
    #else
        Application.Quit(); // 빌드된 게임 종료
    #endif
    }

    void Awake()
    {
        // 게임 시작 시 게임 클리어 여부에 따른 배경 이미지 설정
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    }
}
