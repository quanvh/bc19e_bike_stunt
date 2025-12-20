using Bacon;
using DG.Tweening;
using Kamgam.BikeRacing25D;
using Kamgam.BridgeBuilder25D;
using Kamgam.Terrain25DLib;
using Kamgam.Terrain25DLib.Helpers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("GENERAL")]
    public Camera Camera;
    public SpriteRenderer Sky;
    public BC_CameraFollow cameraman = new();

    public ThemeData themes = null;


    [Header("GEN MAP")]
    public MeshGenerator meshGenerator;

    public FoliageGenerator foliageGenerator;

    public SplineController splineGenerator;

    public SingleUnityLayer GroundLayer;

    public TerrainLevelData terrainData;

    public Transform LevelObject;

    public Transform Bridges;


    protected Transform StartPoint;

    protected Goal FinishPoint;

    public List<Looping2D> looping2D;

    protected bool isReady;

    protected bool isLoaded;


    [Button]
    public void SaveMap()
    {
#if UNITY_EDITOR
        if (Camera != null)
        {
            terrainData.CameraZoom = Camera.transform.position;
        }
        splineGenerator.SaveShape(terrainData);

        terrainData.Objects.Clear();
        terrainData.Objects = new List<SplinePoint>();
        terrainData.foliagePos.Clear();
        terrainData.foliagePos = new List<FolliagePos>();
        terrainData.Bridges.Clear();
        terrainData.Bridges = new List<BridgePoint>();

        //Save object info to load from resources
        foreach (Transform child in LevelObject)
        {
            var newPoint = new SplinePoint();
            newPoint.Name = child.name;
            newPoint.Position = child.localPosition;
            newPoint.Rotation = child.localEulerAngles;
            terrainData.Objects.Add(newPoint);
        }

        //Save bridges 
        if (Bridges != null)
        {
            foreach (Transform child in Bridges)
            {
                var Point1 = child.GetComponent<Kamgam.BridgeBuilder25D.BezierSpline>().Points[0];
                var Point2 = child.GetComponent<Kamgam.BridgeBuilder25D.BezierSpline>().Points[1];

                var newBridge = new BridgePoint();
                newBridge.Position = child.localPosition;
                newBridge.Rotation = child.localEulerAngles;
                newBridge.Scale = child.localScale;
                newBridge.Pos1 = Point1.LocalPosition;
                newBridge.Pos2 = Point2.LocalPosition;
                newBridge.Handle1 = Point2.Handle1LocalPos;
                newBridge.Handle2 = Point1.Handle2LocalPos;

                terrainData.Bridges.Add(newBridge);
            }
        }

        //Save folliage position
        foreach (var point in foliageGenerator.GeneratorSets)
        {
            var newPos = new List<Vector3>();
            for (int i = 0; i < point.FoliagePos.Count; i++)
            {
                newPos.Add(point.FoliagePos[i]);
            }
            terrainData.foliagePos.Add(new FolliagePos(newPos));
        }
        EditorUtility.SetDirty(terrainData);
#endif
    }


    public void GenMap()
    {
        StartCoroutine(GenMapCoroutine());
    }


    public IEnumerator GenMapCoroutine()
    {
        GameObject go = new GameObject("Terrain", typeof(Terrain25D));
        go.transform.parent = transform;
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        var terrain = go.GetComponent<Terrain25D>();
        terrain.AddSplineController(false);

        //add point
        foreach (var spline in terrainData.Shapes)
        {
            var bezier = terrain.SplineController.AddCurve(spline.Position,
                spline.CombinationType, spline.Points, spline.Mode2D);
            bezier.transform.localEulerAngles = spline.Rotation;
        }
        yield return new WaitForSeconds(0.2f);

        terrain.AddCollider2DGenerator();
        terrain.Collider2DGenerator.Layer = GroundLayer;
        terrain.Collider2DGenerator.GenerateColliders();

        terrain.AddMeshGenerator();
        meshGenerator = terrain.MeshGenerator;
        meshGenerator.OnPostMeshGenerated += OnGenMeshComplete;

        terrain.AddFoliageGenerator();
        foliageGenerator = terrain.FoliageGenerator;

        GameObject go2 = new GameObject("Objects");
        go2.transform.parent = transform;
        go2.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        foreach (SplinePoint point in terrainData.Objects)
        {
            //Debug.Log("instantiate object: " + point.Name);
            var newObject = Instantiate(Resources.Load("GameItem/" + point.Name),
                        point.Position, Quaternion.Euler(point.Rotation)) as GameObject;
            newObject.name = point.Name;
            newObject.transform.parent = go2.transform;

            if (point.Name == "StartPoint")
            {
                StartPoint = newObject.transform;
            }
            if (point.Name == "Looping")
            {
                looping2D.Add(newObject.GetComponent<Looping2D>());
            }
            if (point.Name == "FinishPoint")
            {
                FinishPoint = newObject.GetComponent<Goal>();
            }
        }

        GameObject bridges = new GameObject("Bridges");
        bridges.transform.SetParent(transform);
        foreach (var bridgePoint in terrainData.Bridges)
        {
            CreateBridge(bridgePoint, bridges.transform);
        }

        yield return new WaitForSeconds(0.2f);
        isLoaded = true;

        if (!DataManager.Instance)
        {
            GenMesh(THEME_NAME.AUTUMN);
        }
    }

    private void CreateBridge(BridgePoint bridgePoint, Transform parent = null, THEME_NAME _theme = THEME_NAME.AUTUMN)
    {

        GameObject go = new GameObject("2.5D Bridge", typeof(Bridge25D));
        go.transform.SetParent(parent);
        go.transform.SetLocalPositionAndRotation(bridgePoint.Position, Quaternion.Euler(bridgePoint.Rotation));
        go.transform.localScale = bridgePoint.Scale;
        var bridge = go.GetComponent<Bridge25D>();
        //go.transform.position = position;

        var spline = go.GetComponent<Kamgam.BridgeBuilder25D.BezierSpline>();
        spline.Mode2D = true;

        // Add points
        var p0 = spline.AddPointAt(bridgePoint.Pos1);
        p0.handleType = Kamgam.BridgeBuilder25D.BezierPoint.HandleType.Broken;
        p0.Handle2LocalPos = bridgePoint.Handle2;

        var p1 = spline.AddPointAt(bridgePoint.Pos2);
        p1.handleType = Kamgam.BridgeBuilder25D.BezierPoint.HandleType.Broken;
        p1.Handle1LocalPos = bridgePoint.Handle1;


        ThemeModel theme = themes.listthemes.Where(t => t._name == _theme).FirstOrDefault();
        bridge.BridgePartPrefab = theme.BridgePartPrefab;
        bridge.BridgeEdgePartPrefab = theme.BridgeEdgePartPrefab;
        bridge.AutoCreate = true;
        bridge.Recreate();
    }

    public void SetupCamera(Rigidbody2D torso)
    {
        cameraman.SetObjectToTrack(torso);
        cameraman.SetCameraToMove(Camera.transform);
        GetCamera().gameObject.SetActive(true);
    }

    private bool isZoom = false;
    public void LateUpdate()
    {
        if (isZoom) { }
        else
        {
            cameraman.LateUpdate();
        }
    }

    private Vector3 camPosition;
    private Vector3 camRotate;
    public void ZoomeMap(bool _isZoom)
    {
        if (_isZoom)
        {
            Camera.transform.DOLocalRotate(camRotate, 1f);
            Camera.transform.DOLocalMove(camPosition, 1f).OnComplete(() =>
            {
                isZoom = false;
            });
        }
        else
        {
            isZoom = true;
            camPosition = Camera.main.transform.localPosition;
            camRotate = Camera.main.transform.localEulerAngles;
            Camera.transform.DOLocalRotate(Vector3.zero, 1f).SetDelay(0.1f);
            Camera.transform.DOLocalMove(terrainData.CameraZoom, 1f).SetDelay(0.1f);
        }
    }

    public Camera GetCamera()
    {
        return Camera;
    }

    public Transform GetBikeSpawnPosition()
    {
        return StartPoint;
    }

    public Goal GetGoal()
    {
        return FinishPoint;
    }

    void Awake()
    {
        isReady = false;
        isLoaded = false;
        isZoom = false;
        GetCamera().gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        if (meshGenerator != null)
            meshGenerator.OnPostMeshGenerated -= OnGenMeshComplete;
    }

    public void InitAfterLoad()
    {
    }

    private readonly List<string> lstName = new List<string> {
        "Trees", "Bushes", "Grass"
    };
    public void GenMesh(THEME_NAME _theme, bool replace = false)
    {
        ThemeModel theme = themes.listthemes.Where(t => t._name == _theme).FirstOrDefault();
        theme ??= themes.listthemes.FirstOrDefault();

        if (theme.Sky != null)
        {
            Sky.sprite = theme.Sky;
        }

        foreach (Looping2D looping in looping2D)
        {
            looping.ChangeMat(theme.Mat);
        }

        meshGenerator.Material = theme.Mat;
        meshGenerator.GenerateMesh();

        foliageGenerator.GeneratorSets = new FoliageGeneratorSet[theme.foliageSettings.Length];
        int i = 0;
        foreach (var curve in theme.Curves)
        {
            var newSet = new FoliageGeneratorSet();
            newSet.Name = lstName[i];
            newSet.Enabled = true;
            newSet.Settings = theme.foliageSettings[i];
            newSet.StaticMeshes = true;
            newSet.Start = 0;
            newSet.End = 100;
            newSet.Curve = curve;
            newSet.FoliagePos = terrainData.foliagePos[i].Position;
            foliageGenerator.GeneratorSets[i] = newSet;
            i++;
        }

        foliageGenerator.Generate(replace);
    }

    private void OnGenMeshComplete(List<MeshFilter> lstMest)
    {
        isReady = true;
    }

    public bool IsReady()
    {
        return isReady;
    }

    public bool IsLoaded()
    {
        return isLoaded;
    }
}
