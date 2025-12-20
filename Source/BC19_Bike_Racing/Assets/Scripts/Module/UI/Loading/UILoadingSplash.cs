using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILoadingSplash : MonoBehaviour
{
    public Image imgFill;

    [SerializeField] private float timeWait = 4.5f;
    [SerializeField] private string startScene = "0_MainScene";

    private void Start()
    {
        if (imgFill)
        {
            imgFill.fillAmount = 0;
            imgFill.DOFillAmount(.9f, timeWait);
        }
        StartCoroutine(DOStart());
    }

    private IEnumerator DOStart()
    {
        yield return new WaitForSeconds(0.1f);

        AsyncOperation async = SceneManager.LoadSceneAsync(startScene, LoadSceneMode.Single);

        async.allowSceneActivation = false;
        while (async.progress < 0.9f)
        {
            yield return new WaitForSeconds(0.25f);
        }
        async.allowSceneActivation = true;
        while (!async.isDone)
        {
            yield return new WaitForSeconds(0.25f);
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(startScene));

    }
    private void OnDestroy()
    {
        imgFill.DOKill();
    }
}
