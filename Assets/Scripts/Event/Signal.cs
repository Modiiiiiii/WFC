using System;
namespace Modi.Event
{
    public interface EventEntity
    {
        void Dispose();
        bool IsNullOrEmpty();
    }

    public class Signal : EventEntity
    {
        Action action;

        public void Run()
        {
            this.action?.Invoke();
        }

        public void UnSubscribe(Action action)
        {
            this.action -= action;
        }

        public void Subscribe(Action action)
        {
            this.action += action;
        }

        public void Dispose()
        {
            this.action = null;
        }

        public bool IsNullOrEmpty()
        {
            return this.action == null;
        }

    }

    public class Signal<T> : EventEntity
    {
        Action<T> action;

        public void Run(T o)
        {
            action?.Invoke(o);
        }

        public void UnSubscribe(Action<T> action)
        {
            this.action -= action;
        }

        public void Subscribe(Action<T> action)
        {
            this.action += action;
        }

        public void Dispose()
        {
            this.action = null;
        }

        public bool IsNullOrEmpty()
        {
            return this.action == null;
        }
    }

    public class Signal<T, A> : EventEntity
    {
        Action<T, A> action;

        public void Run(T o, A a)
        {
            action?.Invoke(o, a);
        }

        public void UnSubscribe(Action<T, A> action)
        {
            this.action -= action;
        }

        public void Subscribe(Action<T, A> action)
        {
            this.action += action;
        }

        public void Dispose()
        {
            this.action = null;
        }

        public bool IsNullOrEmpty()
        {
            return this.action == null;
        }
    }

    public class Signal<T, A, B> : EventEntity
    {
        Action<T, A, B> action;

        public void Run(T o, A a, B b)
        {
            action?.Invoke(o, a, b);
        }

        public void UnSubscribe(Action<T, A, B> action)
        {
            this.action -= action;
        }

        public void Subscribe(Action<T, A, B> action)
        {
            this.action += action;
        }

        public void Dispose()
        {
            this.action = null;
        }

        public bool IsNullOrEmpty()
        {
            return this.action == null;
        }
    }

    public class Signal<T, A, B, C> : EventEntity
    {
        Action<T, A, B, C> action;

        public void Run(T o, A a, B b, C c)
        {
            action?.Invoke(o, a, b, c);
        }

        public void UnSubscribe(Action<T, A, B, C> action)
        {
            this.action -= action;
        }

        public void Subscribe(Action<T, A, B, C> action)
        {
            this.action += action;
        }

        public void Dispose()
        {
            this.action = null;
        }

        public bool IsNullOrEmpty()
        {
            return this.action == null;
        }
    }
}



