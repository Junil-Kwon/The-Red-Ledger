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
            // 앱이 완전히 종료되는 중이면 null 반환 (에디터 플레이 종료 시 에러 방지)
            if (_isQuitting) return null;

            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    GameObject container = new GameObject(typeof(T).Name);
                    _instance = container.AddComponent<T>();
                }
            }
            return _instance;
        }
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
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // 중복으로 생성된 씬의 오브젝트 삭제
            Destroy(gameObject);
        }
    }

    // 앱이 실제 종료될 때만 플래그 설정
    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        // 중복 객체가 삭제될 때 _isQuitting을 건드리지 않도록 함
        // 자신이 진짜 싱글톤 인스턴스였을 때만 static 참조 해제
        if (_instance == this)
        {
            _instance = null;
        }
    }
}