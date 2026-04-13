using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour를 상속하는 싱글톤.
/// 씬이 바뀌어도 사라지지 않는다.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            // 앱이 종료 중이라면 새로운 인스턴스를 만들지 않고 null 반환
            if (_isQuitting) return null;
            
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    GameObject container = new GameObject(typeof(T).Name);
                    _instance = container.AddComponent<T>();
                    DontDestroyOnLoad(container);
                }
            }
            return _instance;
        }
    }

    // 앱 종료 시 플래그 설정
    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
    
    protected virtual void OnDestroy()
    {
        // 에디터에서 플레이 모드를 끌 때도 안전하게 처리
        _isQuitting = true;
    }

    protected virtual void Awake()
    {
        RemoveDuplicates();
    }

    private void RemoveDuplicates()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }

        if (_instance == this)
        {
            // 이미 Instance 프로퍼티를 통해 나 자신으로 설정된 경우라도 
            // 여기서 파괴 방지를 한 번 더 보장해줍니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 정말로 중복된 다른 객체라면 파괴
            Destroy(gameObject);
        }
    }
}