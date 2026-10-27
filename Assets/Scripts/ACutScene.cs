using System;
using UnityEngine;

public abstract class ACutScene : MonoBehaviour
{
    public virtual Color? DialogueTextColor => null;   // null이면 색을 건드리지 않음
    public abstract void Show(int mode, Action<ACutScene> onComplete = null);
    public abstract void Hide(int mode, Action<ACutScene> onComplete = null);
}
