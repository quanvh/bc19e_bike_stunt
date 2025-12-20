using Bacon;
using UnityEngine;

public class CarHelmet : MonoBehaviour
{
    private void OnEnable()
    {
        if (GarageController.Instance)
        {
            GarageController.Instance.OnCustomLoaded += OnCustomLoaded;
        }
    }

    private void OnDisable()
    {
        if (GarageController.Instance)
        {
            GarageController.Instance.OnCustomLoaded -= OnCustomLoaded;
        }
    }
    private void Start()
    {
        var carHelmet = Instantiate(DataManager.Instance.CurrentHelmet.Source, transform);
        carHelmet.transform.localPosition = Vector3.zero;
        carHelmet.transform.localEulerAngles = Vector3.zero;
    }

    public void OnCustomLoaded(CustomModel customModel)
    {
        if (customModel.Type == CUSTOM_TYPE.HELMET)
        {
            if (transform.childCount > 0)
            {
                Destroy(transform.GetChild(0).gameObject);
                var carHelmet = Instantiate(customModel.Source, transform);
                carHelmet.transform.localPosition = Vector3.zero;
                carHelmet.transform.localEulerAngles = Vector3.zero;
            }
        }
    }
}
