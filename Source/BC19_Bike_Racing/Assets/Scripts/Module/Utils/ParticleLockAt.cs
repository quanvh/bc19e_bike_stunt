using System;
using UnityEngine;


namespace Bacon
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleLockAt : MonoBehaviour
    {
        public Transform target;

        protected ParticleSystem particle;

        protected ParticleSystem.MainModule mainModule;

        protected ParticleSystem.EmissionModule emisionModule;

        public float drift = 10f;

        public float deltaAngle = 90f;

        [Range(0.1f, 1f)]
        public float lockAtTime = 1f;

        private float startLifetime = 5f;

        public float distanceToKill = 0.025f;

        public bool isRotation;

        public float posZ;

        protected Vector3 nextPos;

        protected ParticleSystem.Particle[] mParticles;

        protected int numParticlesAlive;

        protected Vector3 targetPosition;

        protected Vector3 pos;

        public float StartLifetime
        {
            get
            {
                return startLifetime;
            }
            set
            {
                startLifetime = value;
                mainModule.startLifetime = startLifetime;
            }
        }

        public event Action<int> OnTarget = delegate
        {
        };

        public event Action<int> OnEmitDone = delegate
        {
        };

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            if (particle == null)
            {
                particle = GetComponent<ParticleSystem>();
            }

            if (particle != null)
            {
                mainModule = particle.main;
                emisionModule = particle.emission;
            }

            if (mParticles == null || mParticles.Length < mainModule.maxParticles)
            {
                mParticles = new ParticleSystem.Particle[mainModule.maxParticles];
            }

            StartLifetime = mainModule.startLifetimeMultiplier;
        }

        private void LateUpdate()
        {
            if (target == null || particle.isStopped)
            {
                return;
            }

            targetPosition = target.position;
            targetPosition.z = posZ;
            pos = base.transform.localPosition;
            pos.z = target.position.z;
            base.transform.localPosition = pos;
            numParticlesAlive = particle.GetParticles(mParticles);
            for (int i = 0; i < numParticlesAlive; i++)
            {
                if (mParticles[i].remainingLifetime < lockAtTime * startLifetime)
                {
                    float num = (lockAtTime - mParticles[i].remainingLifetime) / lockAtTime;
                    mParticles[i].position = Vector3.Lerp(mParticles[i].position, targetPosition, num);
                    if (isRotation)
                    {
                        Vector3 vector = target.position - base.transform.position;
                        vector.x -= mParticles[i].position.x;
                        vector.y -= mParticles[i].position.y;
                        float b = Mathf.Atan2(0f - vector.y, vector.x) * 57.29578f + deltaAngle;
                        mParticles[i].rotation = Mathf.Lerp(mParticles[i].rotation, b, num * Time.deltaTime * drift);
                    }

                    if (Vector3.Distance(target.position, mParticles[i].position) <= distanceToKill)
                    {
                        mParticles[i].remainingLifetime = 0f;
                    }

                    particle.SetParticles(mParticles, numParticlesAlive);
                }

                if (mParticles[i].remainingLifetime <= 0.1f)
                {
                    mParticles[i].remainingLifetime = 0f;
                    this.OnEmitDone?.Invoke(numParticlesAlive);
                    if (i == 0)
                    {
                        this.OnTarget?.Invoke(i);
                    }
                }
            }
        }

        public void Emit(Transform start, Transform end = null, ParticleSystem.Burst[] bursts = null, bool lookAtTarget = false)
        {
            if (lookAtTarget)
            {
                base.transform.LookAt(end, Vector3.up);
            }

            SetTransfromParticle(start);
            SetTargetTransform(end);
            if (bursts != null && bursts.Length != 0)
            {
                emisionModule.SetBursts(bursts);
            }

            particle.Play();
        }

        private void SetTransfromParticle(Transform from)
        {
            if (from != null)
            {
                base.transform.position = new Vector3(from.position.x, from.position.y, base.transform.position.z);
            }
            else
            {
                base.transform.position = Vector3.zero;
            }
        }

        public void SetTargetTransform(Transform value)
        {
            target = value;
        }
    }
}