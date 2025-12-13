using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modi
{
	public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<T>();

					if (instance == null)
					{
						GameObject obj = new GameObject();
						obj.name = typeof(T).Name;
						instance = obj.AddComponent<T>();
					}
				}

				return instance;
			}
		}
	}

	//public abstract class SingletonMono<T> : MonoBehaviour where T :Component
	//{
	//    protected static T instance_m;
	//    public static T Instance 
	//    {
	//        get 
	//        {
	//            if(instance_m==null)
	//            {
	//                GameObject obj = new GameObject(typeof(T).Name);
	//                obj.AddComponent(typeof(T));
	//                instance_m = obj.GetComponent<T>();
	//                print("Create"+obj.name);
	//            }
	//            return instance_m;
	//        }
	//    }

	//    public virtual void Awake()
	//    {
	//        //DontDestroyOnLoad(gameObject);
	//        if(instance_m == null)
	//        {
	//            instance_m = this as T;
	//        }
	//    }

	//}

	public class Singleton<T> where T : new()
	{
		private static readonly object _lock = new object();
		private static T _instance;

		protected Singleton()
		{
			Debug.Assert(_instance == null);
		}

		public static bool Exists
		{
			get
			{
				return _instance != null;
			}
		}

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						if (_instance == null)
						{
							_instance = new T();
						}
					}
				}
				return _instance;
			}
		}
	}
}


