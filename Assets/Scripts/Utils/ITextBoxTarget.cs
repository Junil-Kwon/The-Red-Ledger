using UnityEngine;

public interface ITextBoxTarget
{
    void BubbleInit();
    void UpdateBubble(string text);
    void SetTextColor(Color color);
    GameObject GameObject { get; } // 파괴 여부 체크용
}