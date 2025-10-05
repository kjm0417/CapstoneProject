using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private T prefab; // UI or Object

    private Transform parent; //생성할 Pool의 부모

    private Queue<T> pool = new Queue<T>(); //Pool은 큐를 통해 관리

    /// <summary>
    /// 생성자 역할을 하며, Get() 호출 전에 미리 풀을 어떻게 만들 것인지 세팅
    /// : 매개변수(생성할 pool , pool의 초기 사이즈 , 생성할 pool의 부모(없을수도 있으므로 == null 처리)
    /// </summary>
    public ObjectPool(T prefab, int initPoolSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for(int i = 0; i < initPoolSize; i++)  //pool의 초기 사이즈만큼 pool의 공간을 미리 마련
        {
            AddNewObject();
        }
    }

    /// <summary>
    ///  오브젝트를 풀링하여 큐에 pool을 넣고 반환
    /// </summary>
    private T AddNewObject()
    {
        T obj = Object.Instantiate(prefab, parent); //pool 생성

        obj.gameObject.SetActive(false); //해당 pool은 비활성화 상태로 시작
        pool.Enqueue(obj); //해당 pool을 큐에 넣음

        return obj;
    }


    /// <summary>
    /// 큐의 데이터(Pool)이 1개 이상이라면, 해당 데이터(Pool)을 반환하고, 활성화
    /// 큐의 데이터(Pool)이 없다면  AddNewObject() 호출하여 데이터를 Add
    /// 
    /// </summary>
    public T Get()
    {
        if(pool.Count > 0) //큐의 데이터가 1개 이상이라면
        {
            T obj = pool.Dequeue(); //해당 pool을 큐에서 빼냄
            obj.gameObject.SetActive(true); //해당 pool은 활성화 상태
            return obj;
        }
        else //큐의 데이터가 없다면, 
        {
            return AddNewObject();
        }
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
