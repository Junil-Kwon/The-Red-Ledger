using TMPro;
using UnityEngine;

public class TMPTextBoxAdapter : MonoBehaviour, ITextBoxTarget
{
    private TextMeshProUGUI _tmp;
    public GameObject GameObject => gameObject;

    public void Init(TextMeshProUGUI tmp) => _tmp = tmp;
    public void BubbleInit() { if (_tmp) _tmp.text = string.Empty; }
    public void UpdateBubble(string text) { if (_tmp) _tmp.text = text; }
    public void SetTextColor(Color color) { if (_tmp) _tmp.color = color; }

    private void Awake()
    {
        if (!_tmp) _tmp = GetComponent<TextMeshProUGUI>();
    }
}