using UnityEngine;
using System.Collections;
using System;

namespace Kamgam.BikeAndCharacter25D
{
    /// <summary>
    /// Is used by the bike to determine if it hits the ground.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GroundTouchTrigger : MonoBehaviour
    {
        public LayerMask GroundLayers;

        [HideInInspector]
        public bool IsTouching = false;

        [HideInInspector]
        public bool HasChanged = false;

        protected float lastChangeTime = 0.0f;
        protected bool lastIsTouchingState = false;

        [HideInInspector]
        public Collider2D Other = null;

        [System.NonSerialized]
        public Collider2D Collider;

        [HideInInspector]
        public float TimeSinceLastChange
        {
            get
            {
                return Time.time - lastChangeTime;
            }
        }

        int defaultLayerIndex;
        Collider2D[] contacts = new Collider2D[10];

        void Awake()
        {
            Collider = this.GetComponent<Collider2D>();
            defaultLayerIndex = LayerMask.NameToLayer("Default");
        }

        void FixedUpdate()
        {
            IsTouching = Collider.IsTouchingLayers(GroundLayers); // returns true for triggers too.

            // Fallback for if the user has not set a specific ground layer (we assume ground is the default layer)
            // Check if it is touching triggers or solid colliders.
            // You can remove this if you are certain you will set the ground to another layer than default.
            if ((GroundLayers.value & (1 << defaultLayerIndex)) > 0)
            {
                IsTouching = false;
                int numOfContacts = Collider.GetContacts(contacts);
                for (int i = 0; i < numOfContacts; i++)
                {
                    if (!contacts[i].isTrigger)
                    {
                        IsTouching = true;
                        break;
                    }
                }
            }

            if (lastIsTouchingState != IsTouching)
            {
                lastChangeTime = Time.time;
                lastIsTouchingState = IsTouching;
                HasChanged = true;
            }
            else
            {
                HasChanged = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            Other = collider;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnTriggerEnter2D(collision.collider);
        }
    }
}
