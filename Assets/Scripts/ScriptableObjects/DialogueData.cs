using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Pair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    // 생성자 (코드에서 생성할 때 편의용)
    public Pair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Game Data/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Ink 데이터")]
    public TextAsset inkJSON; // 실행할 Ink 파일

    [Header("배경 이미지")]
    public Sprite backgroundImage; // 배경 이미지

    [Header("캐릭터 이미지 및 위치(기본 좌측부터)")]
    public List<Pair<Sprite, Vector3>> characters; // 캐릭터 이미지 및 위치 배열
    
    [Header("사운드")]
    public AudioClip bgm; // 대화 시작 시 재생할 BGM
}