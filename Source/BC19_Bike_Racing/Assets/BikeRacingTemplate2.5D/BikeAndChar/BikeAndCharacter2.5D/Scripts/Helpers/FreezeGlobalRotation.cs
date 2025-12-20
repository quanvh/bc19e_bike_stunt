using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{
    [ExecuteAlways]
    public class FreezeGlobalRotation : MonoBehaviour
    {
        public bool Active;
        public Vector3 RotationEulerAngles;
        
        private void Awake()
        {
            setRotationIfActive();
        }

        void LateUpdate()
        {
            setRotationIfActive();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            setRotationIfActive();
        }
#endif

        protected void setRotationIfActive()
        {
            if (!Active)
                return;

            transform.rotation = Quaternion.Euler(RotationEulerAngles.x, RotationEulerAngles.y, RotationEulerAngles.z);
        }
    }
}