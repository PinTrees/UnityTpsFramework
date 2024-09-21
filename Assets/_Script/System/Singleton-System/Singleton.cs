using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly object __lock = new object();   // 멀티스레딩 잠금 객체
    private static T instance;                              // 싱글톤 인스턴스를 저장하는 정적 변수입니다

    /// <summary>
    /// 싱글톤 인스턴스에 대한 접근자입니다.
    /// 인스턴스가 없는 경우 새로 생성하고, 있으면 기존 인스턴스를 반환합니다.
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (__lock) 
            {
                if (instance == null) 
                {
                    instance = FindFirstObjectByType<T>(); 

                    if (instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
    }


    /// <summary>
    /// MonoBehaviour의 Awake 메서드를 오버라이드 합니다.
    /// 이 메서드는 오브젝트가 활성화될 때 호출됩니다.
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)       // 인스턴스가 없는 경우 현재 오브젝트를 인스턴스로 설정합니다.
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)  // 이미 다른 인스턴스가 존재하는 경우, 현재 오브젝트를 파괴합니다.
        {
            Debug.LogError($"Another instance of {typeof(T).Name} already exists. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 사용자 정의 초기화를 위한 메서드입니다. 상속받은 클래스에서 필요에 따라 구현할 수 있습니다.
    /// 먕시적으로 호줄되어야 합니다.
    /// </summary>
    public virtual void Init()
    {
    }
}