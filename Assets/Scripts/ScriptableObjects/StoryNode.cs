using System;
using System.Collections.Generic;
using StoryFlags;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryNode", menuName = "Scriptable Objects/Story/StoryNode")]
public class StoryNode : ScriptableObject
{
    public DialogueData dialogue;          // 이 노드에서 재생할 대화
    public string sceneName;                       // 씬 전환이 필요하면 씬 이름
    public List<StoryTransition> transitions;
    public StoryNode defaultNext;          // 어떤 조건도 안 맞을 때
}

[Serializable]
public class StoryTransition
{
    public EStoryFlag[] requiredFlags;     // 모두 true여야 통과
    public StoryNode next;
}
