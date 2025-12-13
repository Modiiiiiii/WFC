using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modi.Event
{
    public class EventDispatcher
    {
        private static Dictionary<string, EventEntity> _eventTable = new Dictionary<string, EventEntity>();

        public static Dictionary<string, EventEntity> EventTable { get { return _eventTable; } }

        public static void AddEventListener(string eventName, Action handler)
        {
            if (!_eventTable.ContainsKey(eventName))
            {
                _eventTable.Add(eventName, new Signal());
            }
            EventEntity eventEntity = _eventTable[eventName];
            ((Signal)eventEntity).Subscribe(handler);
        }

        public static void AddEventListener<T>(string eventName, Action<T> handler)
        {

            if (!_eventTable.ContainsKey(eventName))
            {
                _eventTable.Add(eventName, new Signal<T>());
            }
            EventEntity eventEntity = _eventTable[eventName];
            if (eventEntity is Signal<T> t)
            {
                t.Subscribe(handler);
            }
            else
            {
                Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
            }
        }

        public static void AddEventListener<T, V>(string eventName, Action<T, V> handler)
        {

            if (!_eventTable.ContainsKey(eventName))
            {
                _eventTable.Add(eventName, new Signal<T, V>());
            }
            EventEntity eventEntity = _eventTable[eventName];
            if (eventEntity is Signal<T, V> t)
            {
                t.Subscribe(handler);
            }
            else
            {
                Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
            }
        }

        public static void AddEventListener<T, V, A>(string eventName, Action<T, V, A> handler)
        {
            if (!_eventTable.ContainsKey(eventName))
            {
                _eventTable.Add(eventName, new Signal<T, V, A>());
            }
            EventEntity eventEntity = _eventTable[eventName];
            if (eventEntity is Signal<T, V, A> t)
            {
                t.Subscribe(handler);
            }
            else
            {
                Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
            }
        }

        public static void AddEventListener<T, V, A, B>(string eventName, Action<T, V, A, B> handler)
        {
            if (!_eventTable.ContainsKey(eventName))
            {
                _eventTable.Add(eventName, new Signal<T, V, A, B>());
            }
            EventEntity eventEntity = _eventTable[eventName];
            if (eventEntity is Signal<T, V, A, B> t)
            {
                t.Subscribe(handler);
            }
            else
            {
                Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
            }
        }

        public static void RemoveEventListener(string eventName, Action handler)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                ((Signal)eventEntity).UnSubscribe(handler);
                if (eventEntity.IsNullOrEmpty()) RemoveEvent(eventName);
            }
        }

        public static void RemoveEventListener<T>(string eventName, Action<T> handler)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T> t)
                {
                    t.UnSubscribe(handler);
                }
                else
                {
                    Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
                }
                if (eventEntity.IsNullOrEmpty()) RemoveEvent(eventName);
            }
        }

        public static void RemoveEventListener<T, V>(string eventName, Action<T, V> handler)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T, V> t)
                {
                    t.UnSubscribe(handler);
                }
                else
                {
                    Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
                }
                if (eventEntity.IsNullOrEmpty()) RemoveEvent(eventName);
            }
        }

        public static void RemoveEventListener<T, V, A>(string eventName, Action<T, V, A> handler)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T, V, A> t)
                {
                    t.UnSubscribe(handler);
                }
                else
                {
                    Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
                }
                if (eventEntity.IsNullOrEmpty()) RemoveEvent(eventName);
            }
        }

        public static void RemoveEventListener<T, V, A, B>(string eventName, Action<T, V, A, B> handler)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T, V, A, B> t)
                {
                    t.UnSubscribe(handler);
                }
                else
                {
                    Debug.LogError($"{eventName} 对应的类型错误 :{handler.GetType().FullName} ");
                }
                if (eventEntity.IsNullOrEmpty()) RemoveEvent(eventName);
            }
        }


        public static void RemoveEvent(string eventName)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                _eventTable[eventName].Dispose();
                _eventTable.Remove(eventName);
            }
        }

        public static void Clear()
        {
            foreach (var item in _eventTable)
            {
                item.Value.Dispose();
            }
            _eventTable.Clear();
        }

        public static void TriggerEvent(string eventName)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal t)
                {
                    t.Run();
                }
            }
        }

        public static void TriggerEvent<T>(string eventName, T arg1)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T> t)
                {
                    t.Run(arg1);
                }
            }
        }

        public static void TriggerEvent<T, U>(string eventName, T arg1, U arg2)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T, U> t)
                {
                    t.Run(arg1, arg2);
                }
            }
        }

        public static void TriggerEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T, U, V> t)
                {
                    t.Run(arg1, arg2, arg3);
                }
            }
        }

        public static void TriggerEvent<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                EventEntity eventEntity = _eventTable[eventName];
                if (eventEntity is Signal<T, U, V, W> t)
                {
                    t.Run(arg1, arg2, arg3, arg4);
                }
            }
        }
    }
}

