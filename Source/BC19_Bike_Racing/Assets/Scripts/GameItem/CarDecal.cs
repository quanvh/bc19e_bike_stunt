using UnityEngine;

public class CarDecal : MonoBehaviour
{
    public MeshRenderer decalMesh;

    private void OnEnable()
    {
        //BC_CarController.OnCarSpawned += OnCarSpawned;
    }

    private void OnDisable()
    {
        //BC_CarController.OnCarSpawned -= OnCarSpawned;
    }

    private void OnCarSpawned()
    {
        Material[] newMat = new Material[2];

        //newMat[0] = decalMesh.material;
        //newMat[1] = DataManager.Instance.CurrentDecal.Mat;
        decalMesh.materials = newMat;

    }
}
