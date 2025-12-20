using System;
using UnityEngine;

namespace Kamgam.BridgeBuilder25D
{
    public class DamageReceiver : MonoBehaviour
    {
        public Action<DamageReceiver, float> OnDamageReceived;

        public void OnDamage(float damage)
        {
            OnDamageReceived?.Invoke(this, damage);
        }
    }
}
