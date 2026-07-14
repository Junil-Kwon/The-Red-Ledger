using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour, ITextBoxTarget
{
    [SerializeField] private float maxWidth;
    [SerializeField] private bool isStretchable = true;
    public GameObject GameObject => gameObject;
    private TextMeshProUGUI textMeshPro;

    public void UpdateBubble(string text)
    {
        textMeshPro.text = text; // 임시 텍스트 설정
        if (!isStretchable) return; // 줄 바꿈 체크 비활성화된 경우 무시

        //textMeshPro.CalculateLayoutInputHorizontal();
        float currentWidth = textMeshPro.preferredWidth;
        if (currentWidth > maxWidth)
        {
            textMeshPro.GetComponent<LayoutElement>().preferredWidth = maxWidth; // preferredWidth 설정

            //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }

    public void BubbleInit()
    {
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>(true);
        textMeshPro.text = "";
        
        if (!isStretchable) return; // 줄 바꿈 체크 비활성화된 경우 무시

        textMeshPro.GetComponent<LayoutElement>().preferredWidth = -1f; // preferredWidth 초기화
    }

    public void SetTextColor(Color color)
    {
        GetComponentInChildren<TextMeshProUGUI>().color = color;
    }
}
