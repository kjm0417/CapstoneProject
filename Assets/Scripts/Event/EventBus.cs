using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static Dictionary<string, Delegate> eventDelegateDictionary = new Dictionary<string, Delegate>();
    private static Dictionary<string, Action> eventActionDictionary = new Dictionary<string, Action>();

    // 파라미터 없는 구독
    public static void Subscribe(string eventName, Action listener)
    {
        if (eventActionDictionary.ContainsKey(eventName))
            eventActionDictionary[eventName] += listener;
        else
            eventActionDictionary[eventName] = listener;
    }

    public static void UnSubscribe(string eventName, Action listener)
    {
        if (eventActionDictionary.ContainsKey(eventName))
        {
            eventActionDictionary[eventName] -=  listener;
            if (eventActionDictionary[eventName] == null)
                eventActionDictionary.Remove(eventName);
        }
    }

    public static void Publish(string eventName)
    {
        if (eventActionDictionary.ContainsKey(eventName))
            eventActionDictionary[eventName]?.Invoke();
        else
            Debug.Log($"해당 {eventName} 이벤트는 존재하지 않는다.");
    }

    // 필요시, 파라미터 있는 구독
    public static void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDelegateDictionary.ContainsKey(eventName))
            eventDelegateDictionary[eventName] = (Action<T>)eventDelegateDictionary[eventName] + listener;
        else
            eventDelegateDictionary[eventName] = listener;
    }

    public static void UnSubscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDelegateDictionary.ContainsKey(eventName))
        {
            eventDelegateDictionary[eventName] = (Action<T>)eventDelegateDictionary[eventName] - listener;
            if (eventDelegateDictionary[eventName] == null)
                eventDelegateDictionary.Remove(eventName);
        }
    }

    public static void Publish<T>(string eventName, T param)
    {
        if (eventDelegateDictionary.ContainsKey(eventName))
            (eventDelegateDictionary[eventName] as Action<T>)?.Invoke(param);
        else
            Debug.Log($"해당 {eventName} 이벤트는 존재하지 않는다.");
    }

}