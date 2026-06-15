using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// 泛型事件总线，基于 UniRx Subject 实现发布/订阅。
/// 注册为 VContainer Singleton 后，静态方法自动转发到单例实例。
/// </summary>
public class EventBus
{
    private static EventBus _instance;
    private readonly Dictionary<Type, object> _subjects = new();

    public EventBus()
    {
        _instance = this;
    }

    public static void Publish<T>(T eventData)
    {
        if (_instance == null)
        {
            Debug.LogError("[EventBus] 尚未初始化，请确认 GameBootstrapper 已执行 Build。");
            return;
        }

        _instance.PublishInstance(eventData);
    }

    public static IDisposable Subscribe<T>(Action<T> handler)
    {
        if (_instance == null)
        {
            Debug.LogError("[EventBus] 尚未初始化，请确认 GameBootstrapper 已执行 Build。");
            return null;
        }

        return _instance.SubscribeInstance(handler);
    }

    public void PublishInstance<T>(T eventData)
    {
        GetOrCreateSubject<T>().OnNext(eventData);
    }

    public IDisposable SubscribeInstance<T>(Action<T> handler)
    {
        return GetOrCreateSubject<T>().Subscribe(handler);
    }

    private Subject<T> GetOrCreateSubject<T>()
    {
        var key = typeof(T);
        if (!_subjects.TryGetValue(key, out var existing))
        {
            existing = new Subject<T>();
            _subjects[key] = existing;
        }

        return (Subject<T>)existing;
    }
}
