using UnityEngine;

public class SingleTonGeneric<T> : MonoBehaviour where T : Component // ex) public class TimeManager : SingleTonGeneric<TimeManager> 
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if(instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);

                    instance = obj.AddComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = this as T;

            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public static void Release() //1. 싱글톤의 특성에 따라 씬 전환 시 인스턴스를 새로 만들어야 할 때 호출 
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }
    
}
