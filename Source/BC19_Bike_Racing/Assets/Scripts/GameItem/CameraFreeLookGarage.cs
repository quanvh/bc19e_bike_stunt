using Cinemachine;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFreeLookGarage : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook cameraFreeLook;
    [SerializeField] private float orbitXSpeedAuto = 1f;
    [SerializeField] private float orbitXSpeed = 50f;
    [SerializeField] private float durationAutoDrag = 0.5f;

    private float orbitX;
    private bool isDrag;
    private Coroutine coroutine;

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!this.isDrag)
        {
            float vl = this.orbitXSpeedAuto * Time.deltaTime;
            this.cameraFreeLook.m_XAxis.Value += vl;
        }
        else
        {
            float vl = this.orbitX * Time.deltaTime;
            this.cameraFreeLook.m_XAxis.Value += vl;
        }
    }
    float x;
    float x1;
    float xValue
    {
        get => x;
        set => x = value;
    }
    public void OnDrag(PointerEventData data)
    {
        if (this.cameraFreeLook != null)
        {
            if (data.delta.x >= 1f)
            {
                x1 = 1f;
            }
            if (data.delta.x < 1f)
            {
                x1 = -1f;
            }
            if (x != x1)
            {
                _ = DOTween.To(() => this.xValue, x => this.xValue = x, x1, 1f);
            }
            this.orbitX = x * orbitXSpeed;
            this.isDrag = true;

            if (this.coroutine != null)
            {
                StopCoroutine(this.coroutine);
            }
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        this.coroutine = StartCoroutine(this.EndDrag());
    }

    private IEnumerator EndDrag()
    {
        _ = DOTween.To(() => this.xValue, x => this.xValue = x, 0f, 1f);
        this.orbitX = x * orbitXSpeed;
        yield return new WaitForSeconds(this.durationAutoDrag);
        this.isDrag = false;
    }

    private void Reset()
    {
        if (this.cameraFreeLook == null)
        {
            this.cameraFreeLook = this.GetComponent<CinemachineFreeLook>();
        }
    }
}
