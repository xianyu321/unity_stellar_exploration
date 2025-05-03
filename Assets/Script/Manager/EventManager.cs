using System;
using System.Collections.Generic;

public static class EventManager
{
    private static Dictionary<string, Action<object>> eventDictionaryWithParams = new Dictionary<string, Action<object>>();
    private static Dictionary<string, Action> eventDictionaryWithoutParams = new Dictionary<string, Action>();

    // 注册监听器（带参数）
    public static void On(string eventName, Action<object> listener)
    {
        if (!eventDictionaryWithParams.ContainsKey(eventName))
        {
            eventDictionaryWithParams[eventName] = null;
        }
        
        eventDictionaryWithParams[eventName] += listener;
    }

    // 移除监听器（带参数）
    public static void Off(string eventName, Action<object> listener)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
        {
            eventDictionaryWithParams[eventName] -= listener;
        }
    }

    // 触发事件（带参数）
    public static void Send(string eventName, object data = null)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
        {
            eventDictionaryWithParams[eventName]?.Invoke(data);
        }
    }

    // 注册监听器（不带参数）
    public static void On(string eventName, Action listener)
    {
        if (!eventDictionaryWithoutParams.ContainsKey(eventName))
        {
            eventDictionaryWithoutParams[eventName] = null;
        }
        
        eventDictionaryWithoutParams[eventName] += listener;
    }

    // 移除监听器（不带参数）
    public static void Off(string eventName, Action listener)
    {
        if (eventDictionaryWithoutParams.ContainsKey(eventName))
        {
            eventDictionaryWithoutParams[eventName] -= listener;
        }
    }

    // 触发事件（不带参数）
    public static void Send(string eventName)
    {
        if (eventDictionaryWithoutParams.ContainsKey(eventName))
        {
            eventDictionaryWithoutParams[eventName]?.Invoke();
        }
    }
}