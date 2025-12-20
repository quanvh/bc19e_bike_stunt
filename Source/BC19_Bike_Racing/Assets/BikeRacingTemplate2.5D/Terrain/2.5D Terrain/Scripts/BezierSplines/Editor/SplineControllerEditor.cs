using UnityEditor;
using UnityEngine;


namespace Kamgam.Terrain25DLib
{
    [CustomEditor(typeof(SplineController))]
    public class SplineControllerEditor : Editor
    {
        SplineController controller;

        protected Terrain25D terrain;

        public Terrain25D Terrain
        {
            get
            {
                if (terrain == null && target != null)
                {
                    terrain = ((SplineController)target).GetComponentInParent<Terrain25D>();
                }
                return terrain;
            }
        }

        void OnEnable()
        {
            controller = (SplineController)target;
        }

        public override void OnInspectorGUI()
        {
            if (controller == null)
                return;

            Terrain25DEditor.DrawInspectorGUI(Terrain);
            DrawInspectorGUI(controller);
        }

        public static void DrawInspectorGUI(SplineController controller)
        {
            EditorGUILayout.Space();

            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("S Spline Controller", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();

            // Edit Mode
            bool wasInEditMode = controller.EditMode;

            GUI.enabled = !controller.EditMode;
            if (GUILayout.Button("Start Editing"))
                controller.EditMode = true;

            GUI.enabled = controller.EditMode;
            if (GUILayout.Button("Stop Editing"))
                StopEditing(controller);

            GUILayout.EndHorizontal();
            GUI.enabled = true;

            if (!controller.EditMode)
            {
                GUILayout.Label("Details only available while editing. Press 'Start Editing'.");
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            else
            {
                // settings
                controller.UpdateCollider = GUILayout.Toggle(controller.UpdateCollider, new GUIContent("Auto update Collider", "Automatically updates the collider if the curve is changed.\nHint: Enable 'Always Show Colliders' under Preferences > Physics 2D > Gizmos to see the changes."));
                controller.UpdateMesh = GUILayout.Toggle(controller.UpdateMesh, new GUIContent("Auto update Mesh (beta)", "Automatically recreates the Mesh if the curve is changed.\nThis is very slow and does not automatically apply Combine, Erosion or Smoothing for performace reasons."));

                bool wasDrawShapeFill = controller.PreviewFill;
                controller.PreviewFill = GUILayout.Toggle(controller.PreviewFill, new GUIContent("Preview Fill", "Fills the combined spline shape to preview the changes while editing."));
                if (wasDrawShapeFill != controller.PreviewFill)
                {
                    EditorWindow view = EditorWindow.GetWindow<SceneView>();
                    view.Repaint();
                }

                bool addShape = GUILayout.Button("Add Shape +");
                if (addShape)
                {
                    Vector3 position;
                    intersectEditorCameraRayWithXYInTransform(controller.transform, out position, Vector3.zero);

                    var spline = controller.AddCurve(position, BezierSpline.BooleanOp.Add);
                    EditorGUIUtility.PingObject(spline.gameObject);
                    Selection.objects = new GameObject[] { spline.gameObject };
                }

                bool addHole = GUILayout.Button("Add Hole -");
                if (addHole)
                {
                    Vector3 position;
                    intersectEditorCameraRayWithXYInTransform(controller.transform, out position, Vector3.zero);
                    position += Vector3.right * 0.2f;

                    var spline = controller.AddCurve(position, BezierSpline.BooleanOp.Subtract);
                    EditorGUIUtility.PingObject(spline.gameObject);
                    Selection.objects = new GameObject[] { spline.gameObject };
                }

                GUILayout.EndVertical();

                EditorGUILayout.Space();
            }

            // started editing?
            if (controller.EditMode && !wasInEditMode)
            {
                PrepareToEdit(controller);
            }
        }

        public static void StopEditing(SplineController controller)
        {
            controller.EditMode = false;
            if (Terrain25DSettings.GetOrCreateSettings().CollapseHierarchyAfterSplineEdit)
            {
                HierarchySelection.HierarchySelection.SetExpanded(controller.gameObject, false);
                // select controller instead of child
                var go = Selection.activeGameObject;
                if (go != null && go.transform.IsChildOf(controller.transform))
                {
                    Selection.objects = new GameObject[] { controller.gameObject };
                }
            }
        }

        /// <summary>
        /// Call this immediately after EditMode has been set to true.
        /// </summary>
        /// <param name="controller"></param>
        public static void PrepareToEdit(SplineController controller)
        {
            // started editing?
            if (!controller.EditMode)
                return;

            controller.RefreshSplines();

            // Select a spline within the controller (if not yet selected).
            if (Selection.activeGameObject == null || !Selection.activeGameObject.transform.IsChildOf(controller.transform))
            {
                if (controller.Splines.Length > 0)
                {
                    if (controller.Splines[0].PointCount > 0)
                    {
                        Selection.objects = new GameObject[] { controller.Splines[0].Points[0].gameObject };
                    }
                    else
                    {
                        Selection.objects = new GameObject[] { controller.Splines[0].gameObject };
                    }
                }
            }

            if (SceneView.sceneViews.Count > 0)
            {
                SceneView sceneView = (SceneView)SceneView.sceneViews[0];
                sceneView.Focus();
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