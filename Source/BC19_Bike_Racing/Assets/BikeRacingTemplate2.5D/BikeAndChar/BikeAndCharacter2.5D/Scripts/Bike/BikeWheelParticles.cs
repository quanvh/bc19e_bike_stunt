using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{
    public class BikeWheelParticles : MonoBehaviour
    {
        protected float brakeParticlesForwardInitialStartSpeedMultiplier;
        protected float brakeParticlesBackwardInitialStartSpeedMultiplier;

        public ParticleSystem BrakeParticlesForward;
        public ParticleSystem BrakeParticlesBackward;

        private void Awake()
        {
            brakeParticlesForwardInitialStartSpeedMultiplier = BrakeParticlesForward.main.startSpeedMultiplier;
            brakeParticlesBackwardInitialStartSpeedMultiplier = BrakeParticlesBackward.main.startSpeedMultiplier;
        }

        /// <summary>
        /// Show brake particles, forward or backward.
        /// </summary>
        /// <param name="forward"></param>
        public void StartBrakeParticles(bool forward, float currentSpeed)
        {
            var ps = forward ? BrakeParticlesForward : BrakeParticlesBackward;
            if (!ps.isPlaying)
            {
                ps.Play(true);
            }

            // Update particle speed based on current bike speed.
            // The default startSpeedMultiplier seems to be  the max of the start speed constants.
            var main = ps.main;
            main.startSpeedMultiplier = Mathf.Clamp(currentSpeed / 4, 0.2f, forward ? brakeParticlesForwardInitialStartSpeedMultiplier : brakeParticlesBackwardInitialStartSpeedMultiplier);
        }

        public void StopBrakeParticles()
        {
            if (BrakeParticlesForward.isPlaying)
            {
                BrakeParticlesForward.Stop();
            }

            if (BrakeParticlesBackward.isPlaying)
            {
                BrakeParticlesBackward.Stop();
            }
        }

        public void SetPaused(bool paused)
        {
            if (BrakeParticlesForward.isPlaying)
            {
                if(paused)
                {
                    BrakeParticlesForward.Pause(true);
                }
                else if (BrakeParticlesForward.isPaused)
                {
                    BrakeParticlesBackward.Play(true);
                }
            }

            if (BrakeParticlesBackward.isPlaying)
            {
                if (paused)
                {
                    BrakeParticlesBackward.Pause(true);
                }
                else if( BrakeParticlesBackward.isPaused)
                {
                    BrakeParticlesBackward.Play(true);
                }
            }
        }
    }
}
