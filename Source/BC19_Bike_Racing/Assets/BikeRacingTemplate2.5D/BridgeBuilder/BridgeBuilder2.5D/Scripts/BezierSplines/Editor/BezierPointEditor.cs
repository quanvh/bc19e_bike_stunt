using UnityEngine;
using UnityEditor;
using Kamgam.BridgeBuilder25D.Helpers;
using System;

namespace Kamgam.BridgeBuilder25D
{
    [CustomEditor(typeof(BezierPoint))]
    [CanEditMultipleObjects]
    public class BezierPointEditor : Editor
    {
        BezierPoint point;

        public static KeyCode SnapKey = KeyCode.None;

        protected Bridge25D bridge;
        public Bridge25D Bridge
        {
            get
            {
                if (bridge == null && target != null)
                {
                    bridge = ((BezierPoint)target).GetComponentInParent<Bridge25D>();
                }
                return bridge;
            }
        }

        void OnEnable()
        {
            point = (BezierPoint)target;

            Tools.current = Tool.None;

            Undo.undoRedoPerformed -= onEditorUndoRedo;
            Undo.undoRedoPerformed += onEditorUndoRedo;

            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;

            EditorApplication.playModeStateChanged -= playmodeStateChanged;
            EditorApplication.playModeStateChanged += playmodeStateChanged;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= onEditorUndoRedo;
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.playModeStateChanged -= playmodeStateChanged;
        }

        void onEditorUndoRedo()
        {
            point.Spline.SetDirty();
            Bridge25DEditor.OnEditorUndoRedo(Bridge);
        }

        protected void onEditorUpdate()
        {
            Bridge25DEditor.OnEditorUpdate(Bridge);
        }

        private void playmodeStateChanged(PlayModeStateChange obj)
        {
            Bridge25DEditor.OnPlaymodeStateChanged(Bridge, obj);
        }

        public override void OnInspectorGUI()
        {
            if (point.Spline != null)
            {
                Bridge25DEditor.DrawInspectorHeaderGUI(Bridge);
                BezierSplineEditor.DrawInspectorGUI(point.Spline);
            }

            bool changed = false;

            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label(". Point", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            bool isInEditMode = point.Spline != null && point.Spline.EditMode;
            if (!isInEditMode)
            {
                GUILayout.Label("Details only available while editing. Check 'Edit Mode' first.");
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (isInEditMode)
            {

                // Handle type
                BezierPoint.HandleType handleType = point.handleType;

                // Hande type UI is not needed
                /*
                BezierPoint.HandleType newHandleType = handleType;

                GUILayout.Label("Handle Type:");
                GUILayout.BeginHorizontal();

                GUI.enabled = handleType != BezierPoint.HandleType.Mirrored;
                if (GUILayout.Button("-- Mirrored"))
                    newHandleType = BezierPoint.HandleType.Mirrored;

                GUI.enabled = handleType != BezierPoint.HandleType.Broken;
                if (GUILayout.Button("\\_ Broken"))
                    newHandleType = BezierPoint.HandleType.Broken;

                GUI.enabled = handleType != BezierPoint.HandleType.None;
                if (GUILayout.Button(". None"))
                    newHandleType = BezierPoint.HandleType.None;

                GUI.enabled = true;

                GUILayout.EndHorizontal();

                if (newHandleType != handleType)
                {
                    changed = true;
                    point.handleType = newHandleType;
                }
                */

                // Handle positions
                if (handleType != BezierPoint.HandleType.None)
                {
                    Vector3 handle1 = point.Handle1LocalPos;
                    Vector3 newHandle1 = EditorGUILayout.Vector3Field("Handle 1", point.Handle1LocalPos);
                    Vector3 handle2 = point.Handle2LocalPos;
                    Vector3 newHandle2 = EditorGUILayout.Vector3Field("Handle 2", point.Handle2LocalPos);

                    if (handle1 != newHandle1 || handle2 != newHandle2)
                    {
                        changed = true;
                        if (point.handleType == BezierPoint.HandleType.Mirrored)
                        {
                            if (handle1 != newHandle1)
                                point.Handle1LocalPos = newHandle1;
                            else
                                point.Handle2LocalPos = newHandle2;
                        }
                        else if (point.handleType == BezierPoint.HandleType.Broken)
                        {
                            point.Handle1LocalPos = newHandle1;
                            point.Handle2LocalPos = newHandle2;
                        }
                    }

                    if (point.handleType == BezierPoint.HandleType.Mirrored && point.Handle1LocalPos == Vector3.zero)
                    {
                        point.Handle1LocalPos = new Vector3(-2f, 0f, 0f);
                    }
                }


                GUILayout.EndVertical();
                EditorGUILayout.Space();

                if (changed)
                {
                    if (point.Spline != null)
                        point.Spline.SetDirty();

                    EditorUtility.SetDirty(target);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }

            if (point.Spline != null)
            {
                Bridge25DEditor.DrawDefaultInspectorGUI(Bridge);
            }
        }

        void OnSceneGUI()
        {
            if (point == null || point.Spline == null)
                return;

            DrawSceneGUI(point, showNeighbourHandles: true);
            BezierSplineEditor.DrawSceneGUI(point.Spline);
        }

        public static Color HandleLineColor = new Color(0.7f, 0.7f, 0.7f, 0.1f);
        public static Color HandleLineSelectedColor = new Color(1f, 1f, 0.0f, 1f);
        public static Color HandleCapColor = new Color(1f, 1f, 1f, 0.2f);

        protected static double lastFreeFormHandleClickTime = 0;

        protected static double tmpSelectPointIfNotDragedTime = 0;
        protected static GameObject tmpPointToSelectIfNotDragged = null;

        protected static double lastSnapKeyPressTime = 0;
        protected static RaycastHit2D[] tmpSnapHitList = new RaycastHit2D[5];

        public static void DrawSceneGUI(BezierPoint point, bool showNeighbourHandles)
        {
            if (point == null || point.Spline == null)
                return;

            if (!point.Spline.EditMode)
                return;

            bool selected = Selection.gameObjects.Contains(point.gameObject);
            int positionHandleControlID = point.GetInstanceID();

            // is snap key pressed?
            bool snapKeyPressed;
            if (SnapKey == KeyCode.None)
            {
                SnapKey = UtilsEditor.TryGetKeyCodeForBinding("Scene View/Vertex Snapping", KeyCode.V);
            }
            if (Event.current.isKey && Event.current.keyCode == SnapKey)
                lastSnapKeyPressTime = EditorApplication.timeSinceStartup;
            snapKeyPressed = EditorApplication.timeSinceStartup - lastSnapKeyPressTime < 0.05;

            // Position
            if (Tools.current != Tool.Move) // only draw the move handle if the current tool is not the move tool.
            {
                Handles.color = selected ? Color.yellow : Color.white;
                EditorGUI.BeginChangeCheck();
#if UNITY_2022_1_OR_NEWER
                Vector3 newPosition = Handles.FreeMoveHandle(positionHandleControlID, point.Position, HandleUtility.GetHandleSize(point.Position) * 0.2f, Vector3.zero, Handles.SphereHandleCap);
#else
                Vector3 newPosition = Handles.FreeMoveHandle(positionHandleControlID, point.Position, point.transform.rotation, HandleUtility.GetHandleSize(point.Position) * 0.2f, Vector3.zero, Handles.SphereHandleCap);
#endif
                if (EditorGUI.EndChangeCheck())
                {
                    // snapping to surface of 2d collider
                    bool snapped = false;
                    if (snapKeyPressed)
                    {
                        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        var numOfHits = Physics2D.RaycastNonAlloc(ray.origin, ray.direction, tmpSnapHitList);
                        if (numOfHits > 0)
                        {
                            RaycastHit2D validHit = new RaycastHit2D();
                            for (int i = 0; i < numOfHits; i++)
                            {
                                var hit = tmpSnapHitList[i];
                                if (hit.collider != null && !hit.transform.IsChildOf(point.transform.parent))
                                {
                                    validHit = hit;
                                }
                            }
                            if (validHit.collider != null)
                            {
                                var snapTargetInWorldSpace = validHit.collider.ClosestPoint(validHit.point);
                                newPosition = snapTargetInWorldSpace;
                                snapped = true;
                            }
                        }
                    }

                    if (!snapped)
                        newPosition = MouseToZAxisWorldPos(point.Spline.transform, newPosition);

                    Undo.RecordObject(point.gameObject.transform, "Move First Point");
                    var positionDelta = newPosition - point.Position;
                    point.Position = newPosition;

                    // if multiple points are selected then move them all
                    if (Selection.gameObjects.Length > 1)
                    {
                        foreach (var go in Selection.gameObjects)
                        {
                            if (go != point.gameObject)
                            {
                                var additionalPoint = go.GetComponent<BezierPoint>();
                                if (additionalPoint != null)
                                {
                                    Undo.RecordObject(go.transform, "Move Point");
                                    var pos = additionalPoint.transform.position;
                                    pos += positionDelta;
                                    additionalPoint.transform.position = pos;
                                }
                            }
                        }
                    }

                    tmpPointToSelectIfNotDragged = null;
                }
            }

            // Handle click on FreeFormHandle (if Alt is not pressed)
            if (!Event.current.alt
                && GUIUtility.hotControl == positionHandleControlID
                && Event.current != null && Event.current.type == EventType.Used
                && EditorApplication.timeSinceStartup - lastFreeFormHandleClickTime > 0.2f)
            {
                lastFreeFormHandleClickTime = EditorApplication.timeSinceStartup;

                // Select point if not yet selected
                if (Event.current.shift)
                {
                    var objects = Selection.gameObjects;
                    if (!objects.Contains(point.gameObject))
                        ArrayUtility.Add(ref objects, point.gameObject);
                    else
                        ArrayUtility.Remove(ref objects, point.gameObject);
                    Selection.objects = objects;
                }
                else
                {
                    if (Selection.gameObjects.Length < 1 || !Selection.Contains(point.gameObject))
                        Selection.activeGameObject = point.gameObject;
                    else
                    {
                        // remember the point and select it later if no drag has been started on the multiple points
                        tmpSelectPointIfNotDragedTime = EditorApplication.timeSinceStartup;
                        tmpPointToSelectIfNotDragged = point.gameObject;
                    }
                }
            }

            // select if no drag occurred
            if (EditorApplication.timeSinceStartup - tmpSelectPointIfNotDragedTime > 0.2f && tmpPointToSelectIfNotDragged != null)
            {
                Selection.objects = new GameObject[] { tmpPointToSelectIfNotDragged };
                tmpPointToSelectIfNotDragged = null;
            }

            // Handles
            if (showNeighbourHandles || selected)
            {
                Handles.color = selected ? Color.yellow : HandleCapColor;
                if (point.handleType != BezierPoint.HandleType.None)
                {
                    bool drawHandle1 = point.Spline.Closed || !point.IsFirst;

                    if (drawHandle1)
                    {
                        EditorGUI.BeginChangeCheck();
#if UNITY_2022_1_OR_NEWER
                        Vector3 newGlobal1 = Handles.FreeMoveHandle(point.Handle1Pos, HandleUtility.GetHandleSize(point.Handle1Pos) * 0.05f, Vector3.zero, Handles.DotHandleCap);
#else
                        Vector3 newGlobal1 = Handles.FreeMoveHandle(point.Handle1Pos, Quaternion.identity, HandleUtility.GetHandleSize(point.Handle1Pos) * 0.05f, Vector3.zero, Handles.DotHandleCap);
#endif
                        newGlobal1 = MouseToZAxisWorldPos(point.Spline.transform, newGlobal1);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(point, "Move Handle1");
                            point.Handle1Pos = newGlobal1;
                        }
                    }

                    bool drawHandle2 = point.Spline.Closed || !point.IsLast;
                    if (drawHandle2)
                    {
                        EditorGUI.BeginChangeCheck();
#if UNITY_2022_1_OR_NEWER
                        Vector3 newGlobal2 = Handles.FreeMoveHandle(point.Handle2Pos, HandleUtility.GetHandleSize(point.Handle2Pos) * 0.05f, Vector3.zero, Handles.DotHandleCap);
#else
                        Vector3 newGlobal2 = Handles.FreeMoveHandle(point.Handle2Pos, Quaternion.identity, HandleUtility.GetHandleSize(point.Handle2Pos) * 0.05f, Vector3.zero, Handles.DotHandleCap);
#endif
                        newGlobal2 = MouseToZAxisWorldPos(point.Spline.transform, newGlobal2);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(point, "Move Handle2");
                            point.Handle2Pos = newGlobal2;
                        }
                    }

                    Handles.color = selected ? HandleLineSelectedColor : HandleLineColor;
                    if (drawHandle1)
                        Handles.DrawDottedLine(point.Position, point.Handle1Pos, 5f);
                    if (drawHandle2)
                        Handles.DrawDottedLine(point.Position, point.Handle2Pos, 5f);
                }
            }
        }

        /// <summary>
        /// Casts a ray going through the mouse position onto the local XY plane of the transform and returns the intersection point world pos.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Vector3 MouseToZAxisWorldPos(Transform transform, Vector3 defaultValue)
        {
            if (Event.current == null)
                return defaultValue;

            var cameraRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var plane = new Plane(transform.TransformDirection(Vector3.forward), transform.TransformPoint(Vector3.zero));

            float enter;
            if (plane.Raycast(cameraRay, out enter))
            {
                Vector3 hitPoint = cameraRay.GetPoint(enter);
                return hitPoint;
            }

            return defaultValue;
        }
    }
}