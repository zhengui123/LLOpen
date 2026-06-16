using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// 泛型事件总线，基于 UniRx <see cref="Subject{T}"/> 实现发布/订阅。
/// <para>
/// 设计说明：
/// - 每种事件类型 T 对应一个独立的 Subject，互不干扰。
/// - 通过 VContainer 注册为 Singleton；构造时写入 <see cref="_instance"/>，
///   业务层可直接调用静态 <see cref="Publish{T}"/> / <see cref="Subscribe{T}"/>。
/// - 须在 <c>GameLifetimeScope.Configure</c> 中通过 RegisterBuildCallback 强制 Resolve，
///   否则仅 Register 而未解析时，静态方法会因 _instance 为空而失败。
/// </para>
/// <para>
/// 使用示例：
/// <code>
/// var sub = EventBus.Subscribe&lt;BagUpdatedEvent&gt;(e => RefreshUI(e));
/// EventBus.Publish(new BagUpdatedEvent { Durians = list });
/// sub.Dispose(); // 页面关闭时取消订阅
/// </code>
/// </para>
/// </summary>
public class EventBus
{
    /// <summary>静态 API 转发的单例引用，由构造函数赋值。</summary>
    private static EventBus _instance;

    /// <summary>按事件类型缓存 Subject，键为 typeof(T)。</summary>
    private readonly Dictionary<Type, object> _subjects = new();

    /// <summary>VContainer 创建单例时调用，建立静态访问入口。</summary>
    public EventBus()
    {
        _instance = this;
    }

    /// <summary>
    /// 发布事件。无订阅者时静默忽略，不会抛出异常。
    /// </summary>
    /// <typeparam name="T">事件结构体类型，如 MarketRefreshedEvent。</typeparam>
    public static void Publish<T>(T eventData)
    {
        if (_instance == null)
        {
            Debug.LogError("[EventBus] 尚未初始化，请确认 GameBootstrapper 已执行 Build。");
            return;
        }

        _instance.PublishInstance(eventData);
    }

    /// <summary>
    /// 订阅指定类型事件。返回的 IDisposable 须在 OnDisable/OnDestroy 中 Dispose，避免泄漏与重复回调。
    /// </summary>
    /// <typeparam name="T">事件结构体类型。</typeparam>
    /// <param name="handler">收到事件时的回调。</param>
    /// <returns>用于取消订阅的句柄；未初始化时返回 null。</returns>
    public static IDisposable Subscribe<T>(Action<T> handler)
    {
        if (_instance == null)
        {
            Debug.LogError("[EventBus] 尚未初始化，请确认 GameBootstrapper 已执行 Build。");
            return null;
        }

        return _instance.SubscribeInstance(handler);
    }

    /// <summary>实例版发布，供静态方法或测试注入后调用。</summary>
    public void PublishInstance<T>(T eventData)
    {
        GetOrCreateSubject<T>().OnNext(eventData);
    }

    /// <summary>实例版订阅，内部委托给 UniRx Subject.Subscribe。</summary>
    public IDisposable SubscribeInstance<T>(Action<T> handler)
    {
        return GetOrCreateSubject<T>().Subscribe(handler);
    }

    /// <summary>
    /// 懒创建 Subject：首次 Publish/Subscribe 某类型时才分配通道。
    /// Subject 为热 Observable，仅通知订阅建立之后发生的事件。
    /// </summary>
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
