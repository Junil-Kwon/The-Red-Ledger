using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private float maxWidth;
    [SerializeField] private bool isStretchable = true;
    private TextMeshProUGUI textMeshPro;

    public void CheckLineBreak()
    {
        if (!isStretchable) return; // 줄 바꿈 체크 비활성화된 경우 무시

        float currentWidth = textMeshPro.rectTransform.sizeDelta.x;
        if (currentWidth > maxWidth)
        {
            textMeshPro.GetComponent<LayoutElement>().preferredWidth = maxWidth; // preferredWidth 설정
        }
    }

    public void LineBreak()
    {
        if (!isStretchable) return; // 줄 바꿈 체크 비활성화된 경우 무시

        textMeshPro.GetComponent<LayoutElement>().preferredWidth = maxWidth; // preferredWidth 설정
    }

    public void BubbleInit()
    {
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>(true);
        textMeshPro.text = "";
        
        if (!isStretchable) return; // 줄 바꿈 체크 비활성화된 경우 무시

        textMeshPro.GetComponent<LayoutElement>().preferredWidth = -1f; // preferredWidth 초기화
    }
}
