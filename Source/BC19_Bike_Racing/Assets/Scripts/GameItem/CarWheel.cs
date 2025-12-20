using Bacon;
using UnityEngine;

public class CarWheel : MonoBehaviour
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
        var carWheel = Instantiate(DataManager.Instance.CurrentWheel.Source, transform);
        carWheel.transform.localPosition = Vector3.zero;
        carWheel.transform.localEulerAngles = Vector3.zero;
    }

    public void OnCustomLoaded(CustomModel customModel)
    {
        if (customModel.Type == CUSTOM_TYPE.WHEEL)
        {
            if (transform.childCount > 0)
            {
                Destroy(transform.GetChild(0).gameObject);
                var carWheel = Instantiate(customModel.Source, transform);
                carWheel.transform.localPosition = Vector3.zero;
                carWheel.transform.localEulerAngles = Vector3.zero;
            }
        }
    }

}
