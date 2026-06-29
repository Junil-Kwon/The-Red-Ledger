using UnityEngine;
using TMPro;
using System;
public class ChoiceButton : MonoBehaviour
{
    private TextMeshProUGUI frontText;
    private TextMeshProUGUI backText;
    
    //private Action onTextChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        backText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        frontText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text)
    {
        frontText.text = text;
        backText.text = text;
    }

    public void SetTextColor(Color color)
    {
        frontText.color = color;
        backText.color = color;
    }
}
