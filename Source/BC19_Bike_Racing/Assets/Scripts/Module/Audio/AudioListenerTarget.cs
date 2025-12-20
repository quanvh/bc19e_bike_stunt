using System;
using UnityEngine;


namespace Bacon
{
    public class AudioListenerTarget : MonoBehaviour
    {
        private static AudioListenerTarget Instance = null;

        public static bool IsExist
        {
            get
            {
                return Instance != null;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public static void LookTarget(Transform _target)
        {

            if (!IsExist)
            {
                throw new Exception("Could not find the AudioListenerTarget prefab");
            }
            Instance.transform.parent = _target;
            Instance.transform.localPosition = Vector3.zero;
        }
    }
}