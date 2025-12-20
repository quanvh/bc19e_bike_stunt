using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    public class Terrain25D : MonoBehaviour
    {
        protected SplineController splineController;
        public SplineController SplineController
        {
            get
            {
                if (splineController == null)
                {
                    splineController = this.GetComponentInChildren<SplineController>(includeInactive: true);
                }
                return splineController;
            }
        }

        protected MeshGenerator meshGenerator;
        public MeshGenerator MeshGenerator
        {
            get
            {
                if (meshGenerator == null)
                {
                    meshGenerator = this.GetComponentInChildren<MeshGenerator>(includeInactive: true);
                }
                return meshGenerator;
            }
        }

        protected Collider2DGenerator collider2DGenerator;
        public Collider2DGenerator Collider2DGenerator
        {
            get
            {
                if (collider2DGenerator == null)
                {
                    collider2DGenerator = this.GetComponentInChildren<Collider2DGenerator>(includeInactive: true);
                }
                return collider2DGenerator;
            }
        }

        protected FoliageGenerator foliageGenerator;
        public FoliageGenerator FoliageGenerator
        {
            get
            {
                if (foliageGenerator == null)
                {
                    foliageGenerator = this.GetComponentInChildren<FoliageGenerator>(includeInactive: true);
                }
                return foliageGenerator;
            }
        }

        public void AddSplineController(bool init = true)
        {
            var controller = new GameObject("SplineController", typeof(SplineController)).GetComponent<SplineController>();
            controller.transform.parent = transform;
            controller.transform.localPosition = Vector3.zero;
            controller.transform.localRotation = Quaternion.identity;
            controller.transform.localScale = Vector3.one;

            if (init)
                controller.AddCurve(controller.transform.position);
        }


        public void AddMeshGenerator()
        {
            var generator = new GameObject("MeshGenerator", typeof(MeshGenerator)).GetComponent<MeshGenerator>();
            generator.transform.parent = transform;
            generator.transform.localPosition = Vector3.zero;
            generator.transform.localRotation = Quaternion.identity;
            generator.transform.localScale = Vector3.one;

#if UNITY_EDITOR
            if (generator.Material == null)
                generator.Material = Terrain25DSettings.GetOrCreateSettings().DefaultMaterial;
#endif
        }

        public void AddCollider2DGenerator()
        {
            var generator = new GameObject("Collider2DGenerator", typeof(Collider2DGenerator)).GetComponent<Collider2DGenerator>();
            generator.transform.parent = transform;
            generator.transform.localPosition = Vector3.zero;
            generator.transform.localRotation = Quaternion.identity;
            generator.transform.localScale = Vector3.one;
        }

        public void AddFoliageGenerator()
        {
            var generator = new GameObject("FoliageGenerator", typeof(FoliageGenerator)).GetComponent<FoliageGenerator>();
            generator.transform.parent = transform;
            generator.transform.localPosition = Vector3.zero;
            generator.transform.localRotation = Quaternion.identity;
            generator.transform.localScale = Vector3.one;
        }
    }
}