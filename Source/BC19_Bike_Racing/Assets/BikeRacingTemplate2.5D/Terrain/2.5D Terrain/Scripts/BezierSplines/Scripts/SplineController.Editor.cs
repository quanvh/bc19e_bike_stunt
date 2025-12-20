#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace Kamgam.Terrain25DLib
{
    /// <summary>
    /// An Editor "only" extension of the SplineController.
    /// </summary>
	[ExecuteInEditMode]
    public partial class SplineController : MonoBehaviour
    {
        // Unity < 2021 does not take well to EditorOnly serialized properties in builds, thus we keep those.

        [System.NonSerialized]
        public bool EditMode = false;
        protected bool previousEditMode = false;

        [System.NonSerialized]
        public bool PreviewFill = false;

        [System.NonSerialized]
        public bool UpdateCollider = true;

        [System.NonSerialized]
        public bool UpdateMesh = false;

#if UNITY_EDITOR
        protected bool _tmpRequiresUpdate = true;

        protected bool initialized;
        protected bool selectionChanged;
        protected double nextUpdateTime;
        protected double lastAutoMeshGenerationTime;

        private void Awake()
        {
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;

            Selection.selectionChanged -= onSelectionChanged;
            Selection.selectionChanged += onSelectionChanged;

            SceneView.duringSceneGui -= onSceneGUI;
            SceneView.duringSceneGui += onSceneGUI;
        }

        private void OnEnable()
        {
            initialized = false;

            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;

            Selection.selectionChanged -= onSelectionChanged;
            Selection.selectionChanged += onSelectionChanged;

            SceneView.duringSceneGui -= onSceneGUI;
            SceneView.duringSceneGui += onSceneGUI;
        }

        private void OnDestroy()
        {
            EditMode = false;

            EditorApplication.update -= onEditorUpdate;
            Selection.selectionChanged -= onSelectionChanged;
            SceneView.duringSceneGui -= onSceneGUI;
        }

        private void OnTransformChildrenChanged()
        {
            _splines = null;
            setEditModeOnSplines(EditMode);
        }

        void onEditorUpdate()
        {
            if (gameObject == null)
                return;

            // TODO: investigate why initialization also happens on undo in a child (BezierSpline)
            if (!initialized)
            {
                _splines = null;
                initialized = true;

                // Debug.Log("SplineController (" + this.name + ") initialized " + EditorApplication.timeSinceStartup);
            }

            // Edit Mode change
            if (EditMode != previousEditMode)
            {
                previousEditMode = EditMode;
                setEditModeOnSplines(EditMode);

                // Ensure gizmos are turen on
                if (EditMode)
                {
#if UNITY_EDITOR
                    SceneView sv = EditorWindow.GetWindow<SceneView>(null, false);
                    sv.drawGizmos = true;
#endif
                }
            }

            // Ensure objects are selectable while in edit mode
            if (EditMode && SplineCount > 0 && SceneVisibilityManager.instance.IsPickingDisabled(Splines[0].gameObject, false))
            {
                setEditModeOnSplines(EditMode);
            }


            if (selectionChanged)
            {
                selectionChanged = false;
                if (scheduleSelectionTimeLimit != 0 && EditorApplication.timeSinceStartup < scheduleSelectionTimeLimit)
                {
                    Selection.objects = new GameObject[] { scheduledSelectionObject };
                    scheduleSelectionTimeLimit = 0;
                    scheduledSelectionObject = null;
                }
            }

            if (EditMode)
                setSplinesPosZToZero();

            if (EditMode && _tmpRequiresUpdate && (UpdateCollider || UpdateMesh) && EditorApplication.timeSinceStartup > nextUpdateTime)
            {
                nextUpdateTime = EditorApplication.timeSinceStartup + 0.1; // 30 fps
                _tmpRequiresUpdate = false;

                if (UpdateCollider)
                {
                    if (transform.parent != null)
                    {
                        var colliderGenerator = transform.parent.GetComponentInChildren<Collider2DGenerator>();
                        if (colliderGenerator != null)
                            colliderGenerator.GenerateColliders();
                    }
                }

                if (UpdateMesh)
                {
                    if (transform.parent != null)
                    {
                        var meshGenerator = transform.parent.GetComponentInChildren<MeshGenerator>();
                        if (meshGenerator != null)
                        {
                            try
                            {
                                meshGenerator.GenerateMesh(ignoreSmoothCombineAndErosion: true);
                                lastAutoMeshGenerationTime = EditorApplication.timeSinceStartup;
                            }
                            catch (System.Exception e)
                            {
                                if (Terrain25DSettings.GetOrCreateSettings().ShowLogs)
                                    Debug.Log("Auto Mesh generation failed. It happens (working on it). Error: " + e.Message);
                            }
                        }
                    }
                }
            }

            // Full mesh gen 0.5 sec after auto update
            if (EditMode && UpdateMesh && lastAutoMeshGenerationTime > 0)
            {
                if (transform.parent != null)
                {
                    var meshGenerator = transform.parent.GetComponentInChildren<MeshGenerator>();
                    if (meshGenerator != null)
                    {
                        if (EditorApplication.timeSinceStartup - lastAutoMeshGenerationTime > 0.5)
                        {
#if UNITY_EDITOR
                            try
                            {
#endif
                                meshGenerator.GenerateMesh(ignoreSmoothCombineAndErosion: false);
                                lastAutoMeshGenerationTime = 0;
#if UNITY_EDITOR
                            }
                            catch (Poly2Tri.PointOnEdgeException e)
                            {
                                if (Terrain25DSettings.GetOrCreateSettings().ShowLogs)
                                    Debug.Log("Mesh generation failed: " + e.Message);
                            }
#endif
                        }
                    }
                }
            }
        }

        protected static List<Vector3[]> _tmpShapeFillCache = new List<Vector3[]>();

        private void onSceneGUI(SceneView view)
        {
            if (!EditMode || SplineCount == 0)
                return;

            bool wasDirty = false;
            foreach (var spline in Splines)
            {
                if (spline.WasDirty)
                {
                    wasDirty = true;
                }
                spline.WasDirty = false;
            }

            if (wasDirty)
            {
                _tmpShapeFillCache.Clear();
                _tmpRequiresUpdate = true;
            }

            CombineAndRememberInfo();

            var combinedSplineColor = Terrain25DSettings.GetOrCreateSettings().CombinedColor;

            var shapesInTargetSpace = CombinationResult.Shapes;
            var holesInTargetSpace = CombinationResult.Holes;
            float z = 0f; // It was assumed the the localPosition.z of the points was 0, thus is still is 0.
            if (holesInTargetSpace != null)
            {
                foreach (Vector3[] solutionPoints in holesInTargetSpace)
                {
                    // DrawAAPolyLine as outline requires global coordinates
                    Handles.color = combinedSplineColor;
                    Handles.DrawAAPolyLine(7f, solutionPoints);
                }
            }

            foreach (Vector3[] solutionPoints in shapesInTargetSpace)
            {
                // DrawAAPolyLine as outline requires global coordinates
                Handles.color = combinedSplineColor;
                Handles.DrawAAPolyLine(7f, solutionPoints);

                // split into convex polygons and draw them as filled.
                // Turns out this is pretty slow, thus we only do it if enabled (TODO: investigate > improve algorithm)
                if (PreviewFill)
                {
                    if (wasDirty)
                    {
                        // convert to Vector2
                        List<Vector2> vertices = new List<Vector2>(solutionPoints.Length);
                        for (int i = 0; i < solutionPoints.Length; i++)
                        {
                            vertices.Add((Vector2)transform.InverseTransformPoint(solutionPoints[i]));
                        }

                        // split into convex polys (slow)
                        List<List<Vector2>> listOfConvexPolygonPoints = Farseer.BayazitDecomposer.ConvexPartition(vertices);

                        // convert to Vector3 and cache to avoid doing the slow ConvexPartition every frame
                        foreach (List<Vector2> pointsOfIndivualConvexPolygon in listOfConvexPolygonPoints)
                        {
                            List<Vector2> currentPolygonVertices = pointsOfIndivualConvexPolygon;

                            var convexFillPoints = new Vector3[currentPolygonVertices.Count];
                            for (int i = 0; i < convexFillPoints.Length; i++)
                            {
                                var point = new Vector3(currentPolygonVertices[i].x, currentPolygonVertices[i].y, z);
                                convexFillPoints[i] = transform.TransformPoint(point);
                            }

                            _tmpShapeFillCache.Add(convexFillPoints);
                        }
                    }
                }
            }

            if (PreviewFill)
            {
                // draw fill
                var col1 = combinedSplineColor;
                col1.a = 0.2f;
                Handles.color = col1;
                foreach (var polygon in _tmpShapeFillCache)
                {
                    Handles.DrawAAConvexPolygon(polygon);
                }
            }
        }

        protected void setEditModeOnSplines(bool editMode)
        {
            if (SplineCount == 0)
                return;

            foreach (var spline in Splines)
            {
                spline.EditMode = editMode;
                if (editMode)
                {
                    SceneVisibilityManager.instance.EnablePicking(spline.gameObject, includeDescendants: true);
                }
                else
                {
                    SceneVisibilityManager.instance.DisablePicking(spline.gameObject, includeDescendants: true);
                }
            }
        }

        protected void setSplinesPosZToZero()
        {
            if (SplineCount == 0)
                return;

            foreach (var spline in Splines)
            {
                // Enforce z = 0 for splines.
                var localPos = spline.gameObject.transform.localPosition;
                if (localPos.z != 0)
                {
                    localPos.z = 0;
                    spline.gameObject.transform.localPosition = localPos;
                }
            }
        }

        private void onSelectionChanged()
        {
            selectionChanged = true;
        }

        protected GameObject scheduledSelectionObject;
        protected double scheduleSelectionTimeLimit;

        public void ScheduleReselect(GameObject obj, double maxDelayInSec)
        {
            scheduledSelectionObject = obj;
            scheduleSelectionTimeLimit = EditorApplication.timeSinceStartup + maxDelayInSec;
        }

        protected void reselectDelayed()
        {
            if (scheduledSelectionObject != null)
            {
                Selection.objects = new GameObject[] { scheduledSelectionObject };
            }
        }
#endif
    }
}