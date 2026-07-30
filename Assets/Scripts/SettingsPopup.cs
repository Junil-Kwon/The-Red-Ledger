using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    public void OpenPopup()
    {
        gameObject.SetActive(true);
        RefreshSliders(); // 팝업이 열릴 때 슬롯을 새로고침
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    private void RefreshSliders()
    {
        // 게임 데이터 팝업이 활성화될 때, 현재 저장된 게임 데이터를 불러옵니다.
        GameSettings settings = DataManager.Instance.CurrentSettings;

        // 각 슬라이더에 현재 설정 값을 반영
        masterVolumeSlider.value = settings.MasterVolume;
        bgmVolumeSlider.value = settings.BgmVolume;
        sfxVolumeSlider.value = settings.SfxVolume;
    }

    public void OnMasterVolumeChanged(float value)
    {
        Debug.Log($"Master Volume changed to: {value}");
        DataManager.Instance.CurrentSettings.SetMasterVolume(value);
        DataManager.Instance.SaveSettings();
    }

    public void OnBgmVolumeChanged(float value)
    {
        Debug.Log($"BGM Volume changed to: {value}");
        DataManager.Instance.CurrentSettings.SetBgmVolume(value);
        DataManager.Instance.SaveSettings();
    }

    public void OnSfxVolumeChanged(float value)
    {
        Debug.Log($"SFX Volume changed to: {value}");
        DataManager.Instance.CurrentSettings.SetSfxVolume(value);
        DataManager.Instance.SaveSettings();
    }

    void Start()
    {
        // 슬라이더 값이 변경될 때마다 호출되는 이벤트에 메서드 연결
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }
}
