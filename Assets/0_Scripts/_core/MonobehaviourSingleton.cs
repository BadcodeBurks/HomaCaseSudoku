using UnityEngine;

namespace Burk.Core
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _i;

        public static T I
        {
            get
            {
                if (_i == null) TryFindInstanceInScene();
                return _i;
            }
        }
        
        private void Awake()
        {
            _i = this as T;
        }

        private static void TryFindInstanceInScene()
        {
            _i = FindObjectOfType<T>();
            
            if(_i == null) Debug.LogWarning("There are no instances of " + nameof(T) + " and you are trying to access it.");
        }

        public virtual void Init()
        {
            Debug.Log("Initialized " + nameof(T) + " Instance");
        }
    }
}