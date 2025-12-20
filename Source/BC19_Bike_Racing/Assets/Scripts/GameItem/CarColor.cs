using Bacon;
using UnityEngine;

public class CarColor : MonoBehaviour
{
    public MeshRenderer colorMesh;


    private void OnEnable()
    {
        LevelController.OnCarLoaded += OnChangeCar;
        LevelController.OnColorSelect += OnChangeColor;
    }

    private void OnDisable()
    {
        LevelController.OnCarLoaded -= OnChangeCar;
        LevelController.OnColorSelect -= OnChangeColor;
    }

    private void OnChangeCar(int carId)
    {
        colorMesh.material = DataManager.Instance.BaseColor(carId).Mat;
    }

    private void OnChangeColor(int colorId)
    {
        colorMesh.material = DataManager.Instance.GetColor(colorId).Mat;
    }
}
