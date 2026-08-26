using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
public class TextBoxLayer
{
    public string Name { get; }
    private readonly Dictionary<int, ITextBoxTarget> _targets = new();

    public TextBoxLayer(string name = null)
    {
        Name = name ?? "Layer";
    }

    public TextBoxLayer Set(int index, ITextBoxTarget target)
    {
        _targets[index] = target;
        return this; // 체이닝: layer.Set(0, boxA).Set(1, boxB)
    }

    public TextBoxLayer Remove(int index)
    {
        _targets.Remove(index);
        return this; // 체이닝: layer.Remove(0).Remove(1)
    }

    public bool TryGet(int index, out ITextBoxTarget target)
        => _targets.TryGetValue(index, out target);
}
*/

public class TextBoxLayer
{
    public string Name { get; }
    private readonly Dictionary<int, TextMeshProUGUI> _targets = new();

    public TextBoxLayer(string name = null)
    {
        Name = name ?? "Layer";
    }

    public TextBoxLayer Set(int index, TextMeshProUGUI target)
    {
        _targets[index] = target;
        return this; // 체이닝: layer.Set(0, boxA).Set(1, boxB)
    }

    public TextBoxLayer Remove(int index)
    {
        _targets.Remove(index);
        return this; // 체이닝: layer.Remove(0).Remove(1)
    }

    public bool TryGet(int index, out TextMeshProUGUI target)
        => _targets.TryGetValue(index, out target);

    // 디버그 메소드
    public void LogTargetKeys()
    {
        if (_targets.Count == 0)
        {
            Debug.Log($"[TextBoxLayer:{Name}] _targets has no registered keys.");
            return;
        }

        var keys = new List<int>(_targets.Keys);
        keys.Sort();
        Debug.Log($"[TextBoxLayer:{Name}] _targets keys: {string.Join(", ", keys)}");
    }
}