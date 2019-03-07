using System;
using System.Collections.Generic;

public static class EventManager<E> where E : GameEvent
{
    private static readonly Dictionary<Type, List<IEventListener<E>>> listeners = new Dictionary<Type, List<IEventListener<E>>>();

    public static void AddListener(IEventListener<E> eventListener)
    {
        if (listeners.TryGetValue(typeof(E), out List<IEventListener<E>> eventListeners)) eventListeners.Add(eventListener);
        else
        {
            listeners.Add(typeof(E), new List<IEventListener<E>>()
            {
                eventListener
            });
        }
    }

    public static void RemoveListener(IEventListener<E> eventListener)
    {
        if (listeners.TryGetValue(typeof(E), out List<IEventListener<E>> eventListeners)) eventListeners.Remove(eventListener);
    }

    public static void CallEvent(E eventData)
    {
        if (listeners.TryGetValue(typeof(E), out List<IEventListener<E>> eventListeners))
        {
            foreach (IEventListener<E> eventListener in eventListeners)
            {
                eventListener.OnEvent(eventData);
            }
        }
    }
}

public abstract class GameEvent { }

public interface IEventListener<E> where E : GameEvent
{
    void OnEvent(E eventData);
}
