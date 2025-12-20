using UnityEngine;

/// <summary>
/// Inspired by Attack, Sustain and Release as described in
/// "Game Feel" by Steve Swink (chapter 7, response metrics).
/// 
/// Value
///  ^
///  | +- Attack phase (starts if On = true)
///  | |   +- Sustain phase (starts with end of attack phase)
///  | |   |    +- Release phase (starts if On = false)
///  | v   v    v
///  |   _____
///  |  /     \
///  | /       \
///  |/         \
///  +--------------------------------------> Time
///   aaa                <= attack phase  (attack value = the start value, sutain value = end value)
///      sssss           <= sustain phase (sutain value all the time)
///           rrr        <= release phase (sutain value = the start value, release value = end value)
///               nnn... <= none phase (nothing is happening, phase is none before and afterwards)
/// </summary>
namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public class InputASR
    {
        public enum Phases { Idle = 0, Attack = 1, Sustain = 2, Release = 3 }

        // configs
        public float AttackDurationInSec;
        public float AttackValue;

        public float SustainValue;

        public float ReleaseDurationInSec;
        public float ReleaseValue;

        // input
        public bool On;

        // output
        public float Value
        {
            get { return _value; }
            protected set { }
        }
        protected float _value;

        public float OneMinusValue
        {
            get { return 1.0f - _value; }
            protected set { }
        }

        public Phases Phase
        {
            get { return _phase; }
            protected set { }
        }
        protected Phases _phase;

        /// <summary>
        /// Will be true if it's idle (value is offValue).
        /// </summary>
        public bool IsIdle { get => _phase == Phases.Idle; }

        /// <summary>
        /// Will be true if it's NOT idle (i.e. has a value different from offValue).
        /// </summary>
        public bool IsActive { get => _phase != Phases.Idle; }

        // internal
        protected float _attackTime;
        protected float _releaseTime;
        protected bool _initiallyOn;

        public InputASR(float offValue = 0.0f, float onValue = 1.0f,
                         float attackDuration = 0.0f, float releaseDuration = 0.0f,
                         bool initiallyOn = false)
        {
            _initiallyOn = initiallyOn;
            _value = initiallyOn ? onValue : offValue;
            On = initiallyOn;

            AttackDurationInSec = attackDuration;
            AttackValue = offValue;

            SustainValue = onValue;

            ReleaseDurationInSec = releaseDuration;
            ReleaseValue = offValue;

            _attackTime = initiallyOn ? attackDuration : 0.0f;
            _releaseTime = initiallyOn ? 0.0f : releaseDuration;
        }

        // Update needs to be called every frame
        public void Update()
        {
            if (On)
            {
                // If activated during release then take the current value into account
                // by setting the remaining release time as new attackTime.
                if (!Mathf.Approximately(_releaseTime, 0.0f) && _releaseTime < ReleaseDurationInSec)
                {
                    _attackTime = ((ReleaseDurationInSec - _releaseTime) / ReleaseDurationInSec) * AttackDurationInSec;
                }

                _attackTime += Time.deltaTime;
                _releaseTime = 0.0f;

                if (_attackTime >= AttackDurationInSec)
                {
                    _value = SustainValue;
                    _phase = Phases.Sustain;
                }
                else
                {
                    // linear attack interpolation
                    _value = Mathf.Lerp(AttackValue, SustainValue, _attackTime / AttackDurationInSec);
                    _phase = Phases.Attack;
                }
            }
            else
            {
                // If deactivated during attack then take the current value into account
                // by setting the remaining attackTime as the new release time.
                if (!Mathf.Approximately(_attackTime, 0.0f) && _attackTime < AttackDurationInSec)
                {
                    _releaseTime = ((AttackDurationInSec - _attackTime) / AttackDurationInSec) * ReleaseDurationInSec;
                }

                _attackTime = 0.0f;
                _releaseTime += Time.deltaTime;

                if (_releaseTime >= ReleaseDurationInSec)
                {
                    _value = ReleaseValue;
                    _phase = Phases.Idle;
                }
                else
                {
                    // linear release interpolation
                    _value = Mathf.Lerp(SustainValue, ReleaseValue, _releaseTime / ReleaseDurationInSec);
                    _phase = Phases.Release;
                }
            }
        }

        public void SetPhaseAndValue(Phases phase, float? value = null)
        {
            switch (phase)
            {
                case Phases.Idle:
                    _attackTime = _initiallyOn ? AttackDurationInSec : 0.0f;
                    _releaseTime = _initiallyOn ? 0.0f : ReleaseDurationInSec;
                    _value = _initiallyOn ? SustainValue : AttackValue;
                    _phase = phase;
                    break;

                case Phases.Attack:
                    if (value.HasValue)
                    {
                        float attackValue = Mathf.Clamp(value.Value, AttackValue, SustainValue);
                        // invert linear attack interpolation
                        _attackTime = AttackDurationInSec * (attackValue - AttackValue) / (SustainValue - AttackValue);
                        _value = attackValue;
                        _phase = phase;
                    }
                    break;

                case Phases.Sustain:
                    _attackTime = AttackDurationInSec;
                    _releaseTime = 0;
                    _value = SustainValue;
                    _phase = phase;
                    break;

                case Phases.Release:
                    if (value.HasValue)
                    {
                        float releaseValue = Mathf.Clamp(value.Value, ReleaseValue, SustainValue);
                        // invert linear release interpolation
                        _releaseTime = ReleaseDurationInSec * (1.0f - (releaseValue - ReleaseValue) / (SustainValue - ReleaseValue));
                        _value = releaseValue;
                        _phase = phase;
                    }
                    break;
            }
        }

        public void Reset()
        {
            _value = _initiallyOn ? SustainValue : ReleaseValue;
            On = _initiallyOn;

            AttackValue = ReleaseValue;

            _attackTime = _initiallyOn ? AttackDurationInSec : 0.0f;
            _releaseTime = _initiallyOn ? 0.0f : ReleaseDurationInSec;

            if (On)
            {
                if (_attackTime >= AttackDurationInSec)
                {
                    _phase = Phases.Sustain;
                }
                else
                {
                    _phase = Phases.Attack;
                }
            }
            else
            {
                if (_releaseTime >= ReleaseDurationInSec)
                {
                    _phase = Phases.Idle;
                }
                else
                {
                    _phase = Phases.Release;
                }
            }
        }
    }
}
