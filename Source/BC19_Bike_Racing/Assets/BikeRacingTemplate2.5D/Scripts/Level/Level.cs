using Kamgam.Terrain25DLib;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// Implementation base for ILevel. Used by the demo levels.<br />
    /// The Level object is reponsible for initializing everything the level needs.
    /// Once the initialization is done IsReady() should return true.
    /// </summary>
	public class Level : MonoBehaviour, ILevel
    {
        public Camera Camera;
        public Transform BikeSpawnPosition;
        public Rigidbody2D Torsobogy;
        public Goal Goal;
        public SpriteRenderer Sky;

        protected bool isReady;

        public Cameraman2D cameraman;

        public MeshGenerator meshGenerator;


        private void Start()
        {
            cameraman = new Cameraman2D();
        }

        public void SetupCamera(Rigidbody2D torso)
        {
            Torsobogy = torso;
            cameraman.SetObjectToTrack(Torsobogy);
            cameraman.SetCameraToMove(Camera.transform);
            GetCamera().gameObject.SetActive(true);
        }

        public void LateUpdate()
        {
            cameraman.LateUpdate();
        }

        public Camera GetCamera()
        {
            return Camera;
        }

        public Transform GetBikeSpawnPosition()
        {
            return BikeSpawnPosition;
        }

        public Goal GetGoal()
        {
            return Goal;
        }

        void Awake()
        {
            isReady = false;
            GetCamera().gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            meshGenerator.OnPostMeshGenerated += OnGenMeshComplete;
        }

        private void OnDisable()
        {
            meshGenerator.OnPostMeshGenerated -= OnGenMeshComplete;
        }

        public void InitAfterLoad()
        {
            GenMesh();
        }

        public void GenMesh()
        {
            //meshGenerator.Material =  ;
            meshGenerator.GenerateMesh();
        }

        private void OnGenMeshComplete(List<MeshFilter> lstMest)
        {
            isReady = true;
        }

        public bool IsReady()
        {
            return isReady;
        }
    }
}

