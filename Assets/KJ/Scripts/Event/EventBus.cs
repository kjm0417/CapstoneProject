using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus 
{
    private static Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    public static void Subscribe(string eventName,Action listener)
    {
        if(eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] += listener; 
        }
        else
        {
            eventDictionary[eventName] = listener;
        }
    }

    public static void UnSubscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;

            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public static void Publish(string eventName)
    {

        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName]?.Invoke();
        }
        else
        {
            Debug.Log($"해당 {eventName}는 eventDictionary의 존재하지 않는다.");
        }
           
    }
}
