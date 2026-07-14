using System.Collections.Generic;
using UnityEngine;

public class TextBoxRouter : MonoBehaviour
{
    public static TextBoxRouter Instance { get; private set; }

    private TextBoxLayer _baseLayer;
    private readonly Stack<TextBoxLayer> _layers = new(); // 아래(0)→위(끝) 순서, base 제외

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _baseLayer = new TextBoxLayer("Base");
    }

    /// 기본(항상 최하단) 레이어에 말풍선 등록
    public void RegisterTextBox(int index, ITextBoxTarget target)
    {
        _baseLayer.Set(index, target);
    }

    public void DeRegisterTextBox(int index)
    {
        _baseLayer.Remove(index);
    }

    /// 여러 인덱스를 한 레이어로 묶어서 push (원자적)
    public TextBoxLayer PushLayer(TextBoxLayer layer)
    {
        _layers.Push(layer);
        return layer;
    }

    /// 새 레이어를 만들어서 바로 push (체이닝용)
    public TextBoxLayer BeginLayer(string name = null)
    {
        var layer = new TextBoxLayer(name);
        _layers.Push(layer);
        return layer;
    }

    public void PopLayer(TextBoxLayer layer)
    {
        _layers.Pop(); // 가장 최근에 push된 레이어 제거
    }

    public ITextBoxTarget GetTextBox(int index)
    {
        /*
        // 위에서부터(가장 최근에 push된 레이어부터) 탐색
        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            if (_layers[i].TryGet(index, out var t) && t != null && t.GameObject != null)
                return t;
        }
        */
        foreach (var layer in _layers)
        {
            if (layer.TryGet(index, out var t) && t != null && t.GameObject != null)
                return t;
        }
        if (_baseLayer.TryGet(index, out var baseTarget))
            return baseTarget;

        return null;
    }
}