using UnityEngine;

public class SingleTonGeneric<T> : MonoBehaviour where T : Component // ex) public class TimeManager : SingleTonGeneric<TimeManager> 
{
    private static T instance;

    private static Transform parent; //부모 오브젝트 저장, static으로 공유 가능하게. -> 같은 부모를 가지게

    public static T Instance { get => instance;}


    protected virtual void Awake()
    {
        if (transform.parent != null && transform.root != null) // 해당 오브젝트의 부모가 있다면
        {
            DontDestroyOnLoad(this.transform.root.gameObject); // 부모 오브젝트를 DontDestroyOnLoad 처리
        }
        else
        {
            DontDestroyOnLoad(this.gameObject); // 해당 오브젝트가 최 상위 오브젝트라면 자신을 DontDestroyOnLoad 처리
        }
    }


    public static void Init()
    {
        Debug.Log((typeof(T).Name) + "초기화 완료");

        if (instance == null)
        {
            instance = FindFirstObjectByType<T>();

            if (instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);

                instance = obj.AddComponent<T>();
            }
        }

    }

    public static void Release() //1. 싱글톤의 특성에 따라 씬 전환 시 인스턴스를 새로 만들어야 할 때 호출 
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
        //}

    }
}