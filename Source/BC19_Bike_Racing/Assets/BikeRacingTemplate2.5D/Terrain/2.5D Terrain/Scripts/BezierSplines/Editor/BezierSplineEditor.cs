using UnityEditor;
using UnityEngine;
using static Kamgam.Terrain25DLib.BezierSpline;

namespace Kamgam.Terrain25DLib
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineEditor : Editor
    {
        BezierSpline spline;

        public TerrainData TerrainData;


        static bool foldoutSettings = false;

        protected Terrain25D terrain;
        public Terrain25D Terrain
        {
            get
            {
                if (terrain == null && target != null)
                {
                    terrain = ((BezierSpline)target).GetComponentInParent<Terrain25D>();
                }
                return terrain;
            }
        }

        void OnEnable()
        {
            spline = (BezierSpline)target;
        }

        void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            Terrain25DEditor.DrawInspectorGUI(Terrain);
            SplineControllerEditor.DrawInspectorGUI(spline.Controller);
            DrawInspectorGUI(spline);
        }

        public static void DrawInspectorGUI(BezierSpline spline)
        {
            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("~ Spline", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Show edit mode only if no SplineController exists
            if (spline.Controller == null)
                spline.EditMode = EditorGUILayout.Toggle("Edit Mode", spline.EditMode);

            if (!spline.EditMode)
            {
                if (spline.Controller != null)
                    GUILayout.Label("Details only available while editing. Press 'Start Editing' in the Controller.");
                else
                    GUILayout.Label("Details only available while editing. Check 'Edit Mode' first.");
                GUILayout.EndVertical();
                EditorGUILayout.Space();
                return;
            }

            int oldResolution = spline.Resolution;
            spline.Resolution = EditorGUILayout.IntSlider(
                new GUIContent("Resolution", "Number of segments between each pair of bezier points. The higher the number the better the approximation."),
                spline.Resolution,
                1, 20
                );

            var oldCombinationType = spline.CombinationType;
            spline.CombinationType = (BooleanOp)EditorGUILayout.EnumPopup(
                new GUIContent("Combination Type", "Use 'Subtract' to punch holes into the terrain.")
                , spline.CombinationType);

            if (GUILayout.Button("Add Point ."))
            {
                BezierPoint newPoint;
                // check if a point is selected
                var selection = Selection.gameObjects;
                BezierPoint selectedPoint = null;
                foreach (var go in selection)
                {
                    if (go.transform.IsChildOf(spline.transform))
                        selectedPoint = go.GetComponent<BezierPoint>();
                }

                if (selectedPoint != null)
                {
                    // add new point between the selected and the next
                    if (spline.IsValid)
                    {
                        int index = spline.GetPointIndex(selectedPoint) + 1 % spline.PointCount;
                        Vector3 newPos = selectedPoint.transform.localPosition + (spline.GetPointAtIndex(index).transform.localPosition - selectedPoint.transform.localPosition) * 0.5f;
                        newPoint = spline.AddPointAt(newPos);
                    }
                    else
                    {
                        newPoint = spline.AddPointAt(selectedPoint.transform.localPosition + Vector3.right);
                    }
                }
                else
                {
                    // add point near center of camera
                    Vector3 position;
                    intersectEditorCameraRayWithXYInTransform(spline.transform, out position, Vector3.zero);
                    selectedPoint = spline.GetClosestPointToPos(position);

                    if (selectedPoint != null)
                    {
                        newPoint = spline.AddPointAt(selectedPoint.transform.InverseTransformPoint(position) + Vector3.right);
                    }
                    else
                    {
                        newPoint = spline.AddPointAt(Vector3.zero);
                    }
                }
                if (newPoint != null)
                {
                    Selection.objects = new GameObject[] { newPoint.gameObject };
                    Undo.RegisterCreatedObjectUndo(newPoint.gameObject, "Add new point");
                }
            }

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            foldoutSettings = EditorGUILayout.Foldout(foldoutSettings, new GUIContent("Additional Settings"));
            if (foldoutSettings)
            {
                GUILayout.Label("Use this to flip the spline orientation in case of error:");
                if (GUILayout.Button("Reverse Point Order"))
                {
                    spline.ReversePointOrder();
                }
            }
            EditorGUI.indentLevel--;

            GUILayout.EndVertical();

            EditorGUILayout.Space();

            // Did change?
            if (
                   oldResolution != spline.Resolution
                || oldCombinationType != spline.CombinationType
                )
            {
                spline.SetDirty();
                EditorUtility.SetDirty(spline.gameObject);
            }
        }

        void OnSceneGUI()
        {
            DrawSceneGUI(spline);
        }

        protected static double lastMouseDownPressTime = 0f;
        protected static double lastAddPointModeKeyPressTime = 0f;

        public static void DrawSceneGUI(BezierSpline spline)
        {
            if (!spline.EditMode)
                return;

            // check if anything hanged?
            if (spline.HasPointCountChanged() || spline.HasTransformChanged())
            {
                spline.SetDirty();
            }

            // add new point handle
            var addNewPointKey = Terrain25DSettings.GetOrCreateSettings().AddCurvePointKey;
            if (Event.current.keyCode == addNewPointKey)
            {
                lastAddPointModeKeyPressTime = EditorApplication.timeSinceStartup;
            }
            if (Event.current.type == EventType.KeyUp && (Event.current.keyCode == addNewPointKey))
            {
                lastAddPointModeKeyPressTime = 0d;
            }

            var timeSinceLastKeyDown = EditorApplication.timeSinceStartup - lastAddPointModeKeyPressTime;
            bool addPointKeyIsPressed = timeSinceLastKeyDown < 1f;
            if (addPointKeyIsPressed)
            {
                // Allow new point only if no other control is hit (code disabled since we now use a dedicated key for that)
                //int defaultControlID = GUIUtility.GetControlID(FocusType.Passive);
                //HandleUtility.AddDefaultControl(defaultControlID);
                //if (HandleUtility.nearestControl == defaultControlID)
                {
                    var closestPoint = HandleUtility.ClosestPointToPolyLine(spline.SampledPoints);
                    var size = HandleUtility.GetHandleSize(closestPoint) * 0.1f;
                    Handles.color = Color.white;
                    Handles.DrawSolidDisc(closestPoint, spline.transform.TransformDirection(Vector3.back), size);
                    Handles.color = Color.green;
                    Handles.DrawSolidDisc(closestPoint, spline.transform.TransformDirection(Vector3.back), size * 0.7f);

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        float distance = HandleUtility.DistanceToCircle(closestPoint, size);
                        if (distance < 0.1f)
                        {
                            var timeSinceLastDown = EditorApplication.timeSinceStartup - lastMouseDownPressTime;
                            if (timeSinceLastDown > 1d)
                            {
                                var point = spline.AddPointAt(spline.transform.InverseTransformPoint(closestPoint));
                                Selection.activeGameObject = point.gameObject;
                                Undo.RegisterCreatedObjectUndo(point, "New Point");

                                // Schedule to select the newly created point even if something else was selected (other scene objects are selected on MouseUp)
                                var controller = spline.transform.GetComponentInParent<SplineController>();
                                if (controller != null)
                                    controller.ScheduleReselect(point.gameObject, 3.0f);
                            }
                        }
                    }
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(spline);
            }

            DrawPoints(spline, showHandles: !addPointKeyIsPressed);

            // force repaint every frame
            HandleUtility.Repaint();
        }

        public static void DrawPoints(BezierSpline curve, bool showHandles = true)
        {
            BezierPoint selectedPoint = Selection.activeGameObject?.GetComponent<BezierPoint>();
            for (int i = 0; i < curve.PointCount; i++)
            {
                bool showHandlesForPoint =
                       (curve.Points[(i + curve.PointCount - 1) % curve.PointCount] == selectedPoint)
                    || (curve.Points[i] == selectedPoint)
                    || (curve.Points[(i + 1) % curve.PointCount] == selectedPoint);
                BezierPointEditor.DrawSceneGUI(curve.Points[i], showHandles && showHandlesForPoint);
            }
        }

        /// <summary>
        /// Calculates the interection point of a ray from the center of the EditorSceneView camera forward.
        /// </summary>
        /// <param name="planeTransform">The transfrom which the XY plane will be constructed from.</param>
        /// <param name="position">The result (world space position of the hit).</param>
        /// <param name="defaultPosition">position value will be set to this if not hit.</param>
        /// <returns>True if hit, False if not hit.</returns>
        protected static bool intersectEditorCameraRayWithXYInTransform(Transform planeTransform, out Vector3 position, Vector3 defaultPosition)
        {
            // raycast from the editor camera center to the controller XY plane and save result in position.
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam != null)
            {
                var rect = cam.pixelRect;
                var ray = cam.ScreenPointToRay(
                    new Vector2(
                        rect.width * 0.5f, rect.height * 0.5f
                        )
                    );
                var plane = new Plane(planeTransform.TransformDirection(Vector3.forward), planeTransform.TransformPoint(Vector3.zero));
                float enter;
                if (plane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    position = hitPoint;
                    return true;
                }
            }

            position = defaultPosition;
            return false;
        }
    }
}
