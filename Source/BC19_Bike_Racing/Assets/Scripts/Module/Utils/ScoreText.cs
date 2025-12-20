using DG.Tweening;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class ScoreText : MonoBehaviour
    {
        public ParticleLockAt currencyParticle = null;
        [SerializeField] GameObject hardFXPos;
        [SerializeField] Transform startDefault;
        [SerializeField] SoundFx soundFx;

        public static Action OnEmitComplete;

        public void Emit(Transform _start = null, Transform _end = null, bool lookAtTarget = false, float _moneyValue = 0)
        {
            if (_moneyValue > 0)
            {
                //AudioController.Instance.ClampGold();
                if (currencyParticle != null)
                    currencyParticle.Emit(_start != null ? _start : startDefault,
                        _end != null ? _end : hardFXPos?.transform, null, lookAtTarget);
            }
        }

        public IEnumerator AddCurrency(int targetValue)
        {
            yield return new WaitForSeconds(0.2f);
            Emit(null, null, true, targetValue);
            yield return new WaitForSeconds(1.5f);
            yield return StartAnim(_value + targetValue, true);
        }


        public float ScaleDuration = 0.15f;

        public float CountDuration = 0.5f;

        public Transform Icon;

        public Text txtValue;

        public bool ResetValue = true;

        private int _value;

        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                txtValue.text = value.ToString("N0",
                        CultureInfo.CreateSpecificCulture("en-US")); ;
            }
        }

        public IEnumerator StartAnim(int targetValue, bool sound = false)
        {
            if (ResetValue) Value = 0;
            else Value = _value;

            if (Icon && targetValue > 0)
            {
                Icon.DOScale(1.5f, ScaleDuration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    DOTween.To(() => Value, x => Value = x, targetValue, CountDuration).OnUpdate(() =>
                    {
                        if (sound && soundFx)
                            soundFx.PlaySound();
                    }).OnComplete(() =>
                    {
                        Icon.DOScale(1f, ScaleDuration);
                        OnEmitComplete?.Invoke();
                    });
                });
                yield return new WaitForSeconds(2 * ScaleDuration + CountDuration);
            }
            else
            {
                DOTween.To(() => Value, x => Value = x, targetValue, CountDuration)
                    .OnComplete(() => OnEmitComplete?.Invoke());
                yield return null;
            }
        }

        private void OnDisable()
        {
            DOTween.CompleteAll();
        }
    }
}