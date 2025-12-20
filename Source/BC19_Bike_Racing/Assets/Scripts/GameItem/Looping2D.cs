using UnityEngine;

public class Looping2D : MonoBehaviour
{
    public MeshRenderer loopingMesh;

    public void ChangeMat(Material _mat)
    {
        loopingMesh.material = _mat;
    }
}
