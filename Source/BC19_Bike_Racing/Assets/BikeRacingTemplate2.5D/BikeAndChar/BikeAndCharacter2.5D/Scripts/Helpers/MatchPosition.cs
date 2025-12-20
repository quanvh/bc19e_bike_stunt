using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public class MatchPosition : MonoBehaviour
    {
        public Transform transformToMatch;
        public bool MatchRotation = false;

        void FixedUpdate()
        {
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (transformToMatch == null)
                return;

            this.transform.position = transformToMatch.position;
            if (MatchRotation)
            {
                this.transform.rotation = transformToMatch.rotation;
            }
        }
    }
}
