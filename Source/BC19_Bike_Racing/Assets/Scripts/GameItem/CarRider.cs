using Bacon;
using Kamgam.BikeAndCharacter25D;
using UnityEngine;

public class CarRider : MonoBehaviour
{

    public CharacterBones bones;

    private CarRiderRef riderRef;

    private CustomModel currentChar;
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
        currentChar = DataManager.Instance.CurrentChar;
        AddCarRider(currentChar.Source);
    }


    private void AddCarRider(GameObject _source, bool memory = true)
    {
        bones.boneLoaded = false;
        var carRider = Instantiate(_source, transform);
        carRider.transform.localPosition = Vector3.zero;
        carRider.transform.localEulerAngles = Vector3.zero;
        riderRef = carRider.GetComponent<CarRiderRef>();


        bones.HeadBone = riderRef.HeadBone;
        bones.TorsoBone = riderRef.TorsoBone;
        bones.RightUpperArmBone = riderRef.RightUpperArmBone;
        bones.RightLowerArmBone = riderRef.RightLowerArmBone;
        bones.RightUpperLegBone = riderRef.RightUpperLegBone;
        bones.RightLowerLegBone = riderRef.RightLowerLegBone;
        bones.LeftUpperArmBone = riderRef.LeftUpperArmBone;
        bones.LeftLowerArmBone = riderRef.LeftLowerArmBone;
        bones.LeftUpperLegBone = riderRef.LeftUpperLegBone;
        bones.LeftLowerLegBone = riderRef.LeftLowerLegBone;

        if (memory)
            bones.memorize();
        bones.boneLoaded = true;
    }

    public void OnCustomLoaded(CustomModel customModel)
    {
        if (customModel.Type == CUSTOM_TYPE.CHARACTER)
        {
            if (transform.childCount > 0 && currentChar != customModel)
            {
                currentChar = customModel;
                Destroy(transform.GetChild(0).gameObject);
                AddCarRider(customModel.Source, false);
            }
        }
    }
}
