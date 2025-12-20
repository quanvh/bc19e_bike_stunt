using UnityEngine;
using System.Collections.Generic;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public static class UtilsParticles
    {
        public static void PauseParticles(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (ps.isPlaying)
            {
                ps.Pause(true);
            }
        }

        public static void UnpauseParticles(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (ps.isPaused)
            {
                ps.Play(true);
            }
        }

        public static void StartParticles(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (!ps.isPlaying)
            {
                ps.Play();
            }
        }

        public static void RestartParticles(ParticleSystem ps, bool withChildren = true)
        {
            if (ps == null)
                return;

            if (ps.isPlaying)
            {
                ps.Stop(withChildren);
            }
            ps.Simulate(0, withChildren, true);
            ps.Play(withChildren);
        }

        public static void StopParticles(ParticleSystem ps, bool withChildren = true)
        {
            if (ps == null)
                return;

            if (ps.isPlaying)
            {
                ps.Stop(withChildren);
            }
        }

        public static void StopParticlesInChildren(Transform transform, bool includeInactive = false, bool withChildren = true)
        {
            var systems = transform.GetComponentsInChildren<ParticleSystem>(includeInactive);
            if(systems.Length > 0)
            {
                foreach (var ps in systems)
                {
                    if (ps.isPlaying)
                    {
                        ps.Stop(withChildren);
                    }
                }
            }
        }

        // Start playback once, after that it is always playing, we are just enable-/disabling the emitter.
        // Thanks to https://answers.unity.com/questions/1210223/particle-system-wont-restart-emitting-before-last.html
        public static void StartParticleEmission(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (!ps.isPlaying)
            {
                ps.Play();
            }
            var emissionModule = ps.emission;
            emissionModule.enabled = true;
        }

        public static void StartParticleEmission(ParticleSystem[] ps)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                StartParticleEmission(ps[i]);
            }
        }

        public static void StopParticleEmission(ParticleSystem ps)
        {
            if (ps == null)
                return;

            var emissionModule = ps.emission;
            emissionModule.enabled = false;
        }

        public static void StopParticleEmission(ParticleSystem[] ps)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                StopParticleEmission(ps[i]);
            }
        }

        public static bool IsPlayingAndEmitting(ParticleSystem ps)
        {
            return ps.isPlaying && ps.emission.enabled;
        }

        public static ParticleSystem.MinMaxGradient GetParticleColor(ParticleSystem ps)
        {
            return ps.main.startColor;
        }

        public static Color GetParticleColorConst(ParticleSystem ps)
        {
            return ps.main.startColor.color;
        }

        public static void SetParticleColor(ParticleSystem ps, ParticleSystem.MinMaxGradient color)
        {
            var mainModule = ps.main;
            mainModule.startColor = color;
        }

        public static void SetParticleColor(ParticleSystem ps, Color color)
        {
            var mainModule = ps.main;
            mainModule.startColor = color;
        }

        public static void SetParticleColor(ParticleSystem ps, Color colorMin, Color colorMax)
        {
            var mainModule = ps.main;
            var minMaxGradient = mainModule.startColor;
            minMaxGradient.colorMin = colorMin;
            minMaxGradient.colorMax = colorMax;
            mainModule.startColor = minMaxGradient;
        }

        public static void SetParticleColor(ParticleSystem ps, Color colorMin, Color colorMax, ParticleSystemGradientMode mode)
        {
            var mainModule = ps.main;
            var minMaxGradient = mainModule.startColor;
            minMaxGradient.mode = mode;
            minMaxGradient.colorMin = colorMin;
            minMaxGradient.colorMax = colorMax;
            mainModule.startColor = minMaxGradient;
        }

        public static void SetParticleColor(ParticleSystem ps, Gradient gradientMin, Gradient gradientMax)
        {
            var mainModule = ps.main;
            var minMaxGradient = mainModule.startColor;
            minMaxGradient.gradientMin = gradientMin;
            minMaxGradient.gradientMax = gradientMax;
            mainModule.startColor = minMaxGradient;
        }

        /// <summary>
        /// Null tolerant (will do nothing if ps is null).
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="multiplier"></param>
        public static void SetStartSizeMultiplierNullTolerant(ParticleSystem ps, float multiplier)
        {
            if (ps == null)
                return;

            SetStartSizeMultiplier(ps, multiplier);
        }

        public static void SetStartSizeMultiplier(ParticleSystem ps, float multiplier)
        {
            var mainModule = ps.main;
            mainModule.startSizeMultiplier = multiplier;
        }

        /// <summary>
        /// Will overwrite alpha keys if gradients are used.
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="alpha"></param>
        public static void SetParticleColorAlpha(ParticleSystem ps, float alpha)
        {
            var mainModule = ps.main;
            var startColor = mainModule.startColor;
            switch (startColor.mode)
            {
                case ParticleSystemGradientMode.Color:
                    var color = startColor.color;
                    color.a = alpha;
                    startColor.color = color;
                    break;

                case ParticleSystemGradientMode.Gradient:
                    var gradient = startColor.gradient;
                    var alphaKeys = gradient.alphaKeys;
                    for (int i = 0; i < alphaKeys.Length; i++)
                    {
                        alphaKeys[i].alpha = alpha;
                    }
                    gradient.SetKeys(gradient.colorKeys, alphaKeys);
                    startColor.gradient = gradient;
                    break;

                case ParticleSystemGradientMode.TwoColors:
                case ParticleSystemGradientMode.RandomColor:
                    var colorMin = startColor.colorMin;
                    colorMin.a = alpha;
                    startColor.colorMin = colorMin;
                    var colorMax = startColor.colorMax;
                    colorMax.a = alpha;
                    startColor.colorMax = colorMax;
                    break;

                case ParticleSystemGradientMode.TwoGradients:
                    var gradientMin = startColor.gradientMin;
                    var alphaKeysMin = gradientMin.alphaKeys;
                    for (int i = 0; i < alphaKeysMin.Length; i++)
                    {
                        alphaKeysMin[i].alpha = alpha;
                    }
                    gradientMin.SetKeys(gradientMin.colorKeys, alphaKeysMin);
                    startColor.gradientMin = gradientMin;

                    var gradientMax = startColor.gradientMax;
                    var alphaKeysMax = gradientMax.alphaKeys;
                    for (int i = 0; i < alphaKeysMax.Length; i++)
                    {
                        alphaKeysMax[i].alpha = alpha;
                    }
                    gradientMax.SetKeys(gradientMax.colorKeys, alphaKeysMax);
                    startColor.gradientMax = gradientMax;
                    break;

                default:
                    break;
            }
            mainModule.startColor = startColor;
        }

        #region EmissionRates
        public static void SetEmissionRateOverDistance(ParticleSystem ps, float constant)
        {
            if (ps == null)
                return;

            var emission = ps.emission;
            var rate = emission.rateOverDistance;
            rate.constant = constant;
            emission.rateOverDistance = rate;
        }

        public static void SetEmissionRateOverTime(ParticleSystem ps, float constant)
        {
            if (ps == null)
                return;

            var emission = ps.emission;
            var rate = emission.rateOverTime;
            rate.constant = constant;
            emission.rateOverTime = rate;
        }

        public static void SetEmissionRateOverTime(ParticleSystem ps, float constantMin, float constantMax)
        {
            if (ps == null)
                return;

            var emission = ps.emission;
            var rate = emission.rateOverTime;
            rate.constantMin = constantMin;
            rate.constantMax = constantMax;
            emission.rateOverTime = rate;
        }

        /// <summary>
        /// Format:
        ///  key = InstanceID of the ParticleSystem
        ///  value = (int)mode, (float)const or constMin, (float)constMax
        /// </summary>
        public static Dictionary<int, float[]> EmissionRateMultiplierMemory;

        /// <summary>
        /// This memorizes the particle system values based on their InstanceID.
        /// Be sure to clear the cache if you unload a scene or it may contain wrong values.
        /// 
        /// The whole particle rate over time multiplier is a mess, see:
        /// https://forum.unity.com/threads/particlesystem-rateovertimemultiplier-and-rateoverdistancemultiplier-not-working-as-expected.460672/
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="multiplier"></param>
        public static void SetEmissionRateOverTimeMultiplier(ParticleSystem ps, float multiplier)
        {
            if (ps == null)
                return;

            var emission = ps.emission;
            if (emission.rateOverTime.mode == ParticleSystemCurveMode.Curve || emission.rateOverTime.mode == ParticleSystemCurveMode.TwoCurves)
            {
                emission.rateOverTimeMultiplier = multiplier;
            }
            else
            {
                if (EmissionRateMultiplierMemory == null)
                {
                    EmissionRateMultiplierMemory = new Dictionary<int, float[]>();
                }

                var id = ps.GetInstanceID();
                bool memoryExists = EmissionRateMultiplierMemory.ContainsKey(id);

                // remove from memory if mode has changed
                if (memoryExists && (int)EmissionRateMultiplierMemory[id][0] != (int)emission.rateOverTime.mode)
                {
                    EmissionRateMultiplierMemory.Remove(id);
                    memoryExists = false;
                }

                // create memory and set new values
                if (emission.rateOverTime.mode == ParticleSystemCurveMode.Constant)
                {
                    if (memoryExists == false)
                    {
                        EmissionRateMultiplierMemory[id] = new float[] {
                            (float)emission.rateOverTime.mode,
                            emission.rateOverTime.constant,
                            0f
                        };
                    }

                    // finally we can do this
                    var rate = emission.rateOverTime;
                    rate.constant = EmissionRateMultiplierMemory[id][1] * multiplier;
                    emission.rateOverTime = rate;
                }
                else if (emission.rateOverTime.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    if (memoryExists == false)
                    {
                        EmissionRateMultiplierMemory[id] = new float[] {
                            (float)emission.rateOverTime.mode,
                            emission.rateOverTime.constantMin,
                            emission.rateOverTime.constantMax
                        };
                    }

                    // finally we can do this
                    var rate = emission.rateOverTime;
                    rate.constantMin = EmissionRateMultiplierMemory[id][1] * multiplier;
                    rate.constantMax = EmissionRateMultiplierMemory[id][2] * multiplier;
                    emission.rateOverTime = rate;
                }
            }
        }

        /// <summary>
        /// Removes the cached values for the given particle system.
        /// </summary>
        /// <param name="ps"></param>
        public static void ClearEmissionRateOverTimeMultiplierCacheFor(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (EmissionRateMultiplierMemory == null)
                return;

            EmissionRateMultiplierMemory.Remove(ps.GetInstanceID());
        }

        /// <summary>
        /// Clear all memories memories. Don't do this ich you still have some changes cached.
        /// </summary>
        public static void ClearEmissionRateOverTimeMultiplierCache()
        {
            if (EmissionRateMultiplierMemory == null)
                return;

            EmissionRateMultiplierMemory.Clear();
        }

        public static void SetBurstEmissionRate(ParticleSystem ps, int burstIndex, int amount)
        {
            if (ps == null)
                return;

            var emission = ps.emission;

            if (burstIndex >= emission.burstCount)
                return;

            emission.SetBurst(burstIndex, new ParticleSystem.Burst(0, amount));
        }
        #endregion

        /// <summary>
        /// ENables the forces module.
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="enabled"></param>
        public static void SetExternalForcesEnabled(ParticleSystem ps, bool enabled)
        {
            if (ps == null)
                return;

            var forces = ps.externalForces;
            forces.enabled = enabled;
        }

        /// <summary>
        /// ENables the forces module and sets the filter mehtod.
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="enabled"></param>
        /// <param name="filter"></param>
        public static void SetExternalForcesEnabled(ParticleSystem ps, bool enabled, ParticleSystemGameObjectFilter filter)
        {
            if (ps == null)
                return;

            var forces = ps.externalForces;
            forces.enabled = enabled;
            forces.influenceFilter = filter;
        }

        public static void AddExternalForceFields(ParticleSystem ps, params ParticleSystemForceField[] forceFields)
        {
            if (ps == null)
                return;

            var forces = ps.externalForces;
            for (int i = 0; i < forceFields.Length; i++)
            {
                forces.AddInfluence(forceFields[i]);
            }
        }

        public static void RemoveExternalForceFields(ParticleSystem ps, params ParticleSystemForceField[] forceFields)
        {
            if (ps == null)
                return;

            var forces = ps.externalForces;
            for (int i = 0; i < forceFields.Length; i++)
            {
                forces.RemoveInfluence(forceFields[i]);
            }
        }

        public static void ClearExternalForceFields(ParticleSystem ps, params ParticleSystemForceField[] forceFields)
        {
            if (ps == null)
                return;

            var forces = ps.externalForces;
            forces.RemoveAllInfluences();
        }
    }
}
