using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBoxRouter : Singleton<TextBoxRouter>
{
    //public static TextBoxRouter Instance { get; private set; }

    //private TextBoxLayer _baseLayer;
    private readonly List<TextBoxLayer> _layers = new(); // 아래(0)→위(끝) 순서, base 제외

    private TextBoxLayer TopLayer => _layers.Count > 0 ? _layers[_layers.Count - 1] : null;

    protected override void Awake()
    {   
        /*
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
         _baseLayer = new TextBoxLayer("Base");
        */

        base.Awake();
    }

    /// 기본(항상 최하단) 레이어에 말풍선 등록
    /*
    public void RegisterTextBox(int index, ITextBoxTarget target)
    {
        _baseLayer.Set(index, target);
    }
    */
    public void RegisterTextBox(int index, TextMeshProUGUI target)
    {
        if (_layers.Count == 0) 
        {
            Debug.LogWarning($"TextBoxRouter: No active layer. Creating a default layer.");
            BeginLayer("DefaultLayer");
        }
        //_baseLayer.Set(index, target);
        TopLayer.Set(index, target); // 현재 최상단 레이어에 등록
    }

    public void DeRegisterTextBox(int index)
    {
        if (_layers.Count == 0)
        {
            Debug.LogError($"TextBoxRouter: No active layer. method will be ignored.");
            return;
        }
        TopLayer.Remove(index);
    }

    /// 여러 인덱스를 한 레이어로 묶어서 push (원자적)
    public TextBoxLayer PushLayer(TextBoxLayer layer)
    {
        _layers.Add(layer);
        return layer;
    }

    /// 새 레이어를 만들어서 바로 push (체이닝용)
    public TextBoxLayer BeginLayer(string name = "")
    {
        var layer = new TextBoxLayer(name);
        _layers.Add(layer);
        return layer;
    }

    // DEPRECATED: PopLayer()는 더 이상 사용되지 않음. 대신 RemoveLayer(layer)를 사용하세요.
    public void PopLayer()
    {
        if (_layers.Count > 0)
        {
            _layers.RemoveAt(_layers.Count - 1); // 가장 최근에 push된 레이어 제거
        }
    }

    public bool RemoveLayer(TextBoxLayer layer)
    {
        if (layer == null)
        {
            Debug.LogWarning("TextBoxRouter: Attempted to remove a null layer.");
            return false;
        }
        
        bool isRemoved = _layers.Remove(layer);
    
        if (isRemoved)
        {
            Debug.Log($"TextBoxRouter: Layer '{layer.Name}' removed successfully.");
        }
        else
        {
            Debug.LogWarning($"TextBoxRouter: Layer '{layer.Name}' not found in the list.");
        }

        return isRemoved;
    }
    /*
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
        /*
        foreach (var layer in _layers)
        {
            if (layer.TryGet(index, out var t) && t != null && t.GameObject != null)
                return t;
        }
        if (_baseLayer.TryGet(index, out var baseTarget))
            return baseTarget;

        return null;
    }
    */

    public TextMeshProUGUI GetTextBox(int index)
    {
        if (TopLayer != null && TopLayer.TryGet(index, out var target) && target != null)
        {
            return target;
        }

        Debug.LogWarning($"TextBoxRouter: TextBox not found at index {index}.");
        return null;
    }

    // 디버그 메소드
    public void PrintLayers()
    {
        Debug.Log("Current TextBox Layers:");
        foreach (var layer in _layers)
        {
            Debug.Log($"Layer: {layer.Name}");
        }
    }
}