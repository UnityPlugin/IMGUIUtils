using UnityEngine;
using UnityEditor;

namespace UnityPlugin.EditorUtils
{
    public class BaseEditor<T> : Editor where T : MonoBehaviour
    {
        protected T _target;

        protected virtual void OnEnable()
        {
            _target = target as T;
        }
    }
}