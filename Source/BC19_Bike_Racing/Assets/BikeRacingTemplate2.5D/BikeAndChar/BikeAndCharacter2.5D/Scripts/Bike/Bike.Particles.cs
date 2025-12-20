using UnityEngine;
using Kamgam.BikeAndCharacter25D.Helpers;

namespace Kamgam.BikeAndCharacter25D
{
    public partial class Bike // .Particles
    {
        // Particles
        [Header("Particles")]
        public BikeWheelParticles FrontWheelParticles;
        public BikeWheelParticles BackWheelParticles;
        public ParticleSystem ExhaustParticleSystem;
        public ParticleSystem SpeedUpParticles;

        protected float speedUpParticlesEmissionRateMinDefault;
        protected float speedUpParticlesEmissionRateMaxDefault;
        protected float speedUpParticlesEmissionRateOverDistanceDefault;

        protected InputASR exhaustSpeedUpASR = new InputASR(0f, 1f, 1.0f, 0f, false);

        protected void awakeParticles()
        {
            speedUpParticlesEmissionRateMinDefault = SpeedUpParticles.emission.rateOverTime.constantMin;
            speedUpParticlesEmissionRateMaxDefault = SpeedUpParticles.emission.rateOverTime.constantMax;
            speedUpParticlesEmissionRateOverDistanceDefault = SpeedUpParticles.emission.rateOverDistance.constant;
        }

        protected void setParticlesPaused(bool paused)
        {
            FrontWheelParticles.SetPaused(paused);
            BackWheelParticles.SetPaused(paused);
            if (paused)
            {
                ExhaustParticleSystem.Pause(true);
                SpeedUpParticles.Pause();
            }
            else
            {
                if (ExhaustParticleSystem.isPaused)
                {
                    ExhaustParticleSystem.Play(true);
                }
                if (SpeedUpParticles.isPaused)
                {
                    SpeedUpParticles.Play(true);
                }
            }
        }

        protected void setSpeedUpParticlesActive(bool active)
        {
            SpeedUpParticles.gameObject.SetActive(active);
        }

        protected void updateParticles(float nextSpeedIncrement)
        {
            exhaustSpeedUpASR.Update();

            if (isSpeedingUp)
            {
                UtilsParticles.StartParticleEmission(ExhaustParticleSystem);
            }
            else
            {
                UtilsParticles.StopParticleEmission(ExhaustParticleSystem);
            }

            // make exhaust darker if speeding up btn is pressed
            exhaustSpeedUpASR.On = isSpeedingUp;
            if (isSpeedingUp)
            {
                var main = ExhaustParticleSystem.main;

                // darken color of particles
                float alpha = (60f + 166f * exhaustSpeedUpASR.OneMinusValue) / 256f; // values need to be in 0-1 range.;
                UtilsParticles.SetParticleColorAlpha(ExhaustParticleSystem, alpha);

                // speed up particles
                var mainStartSpeed = main.startSpeed;
                //mainStartSpeed.constant = 8f + 10f * exhaustSpeedUpASR.Value;
                main.startSpeed = mainStartSpeed;
            }
            else
            {
                // no exhaust particles are emitted, see updateParticles()
            }

            // Dirt particles, show them only if we are really speeding up
            if (nextSpeedIncrement > 0)
            {
                if (BackWheelGroundTouchTrigger.IsTouching)
                {
                    bool canSpeedUp = lookingDirectionIsOppositeToVelocity || Velocity < configMaxVelocity;
                    // show dirt particles
                    if (canSpeedUp)
                    {
                        startDriveParticles();
                    }
                    else
                    {
                        stopDriveParticles();
                    }
                }
                else
                {
                    stopDriveParticles();
                }
            }
            else
            {
                stopDriveParticles();
            }

            // brake particles
            if (brakeForceASR != null && brakeForceASR.Value > 0.01f && Velocity > 2)
            {
                // true = forward, false = backward
                bool forward = this.transform.InverseTransformVector(new Vector3(BikeBody.velocity.x, BikeBody.velocity.y, 0)).x > 0;

                // front wheel
                if (FrontWheelGroundTouchTrigger.IsTouching)
                {
                    FrontWheelParticles.StartBrakeParticles(forward, Velocity);
                }
                else if (!FrontWheelGroundTouchTrigger.IsTouching)
                {
                    FrontWheelParticles.StopBrakeParticles();
                }
                // back wheel
                if (BackWheelGroundTouchTrigger.IsTouching)
                {
                    BackWheelParticles.StartBrakeParticles(forward, Velocity);
                }
                else if (!BackWheelGroundTouchTrigger.IsTouching)
                {
                    BackWheelParticles.StopBrakeParticles();
                }
            }
            else
            {
                FrontWheelParticles.StopBrakeParticles();
                BackWheelParticles.StopBrakeParticles();
            }

        }

        protected void startDriveParticles()
        {
            // Start playback once, after that it is always playing, we are just enable-/disabling the emitter.
            UtilsParticles.StartParticleEmission(SpeedUpParticles);
        }

        protected void stopDriveParticles()
        {
            UtilsParticles.StopParticleEmission(SpeedUpParticles);
        }

    }
}
