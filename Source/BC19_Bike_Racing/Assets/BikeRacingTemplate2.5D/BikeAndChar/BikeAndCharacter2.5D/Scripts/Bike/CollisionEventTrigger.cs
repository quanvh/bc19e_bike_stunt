using UnityEngine;
using System.Collections;
using System;

namespace Kamgam.BikeAndCharacter25D
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionEventTrigger : MonoBehaviour
    {
        public System.Action<CollisionEventTrigger, Collision2D> OnCollision;

        /// <summary>
        /// The layer which will trigger a collision event if hitting this collider.
        /// </summary>
        public LayerMask CollisionLayers;

        protected Collider2D _collider2D;
        public Collider2D Collider2D
        {
            get
            {
                if (_collider2D == null)
                {
                    _collider2D = this.GetComponent<Collider2D>();
                }
                return _collider2D;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Collider2D.IsTouchingLayers(CollisionLayers))
            {
                OnCollision?.Invoke(this, collision);
            }
        }
    }
}
