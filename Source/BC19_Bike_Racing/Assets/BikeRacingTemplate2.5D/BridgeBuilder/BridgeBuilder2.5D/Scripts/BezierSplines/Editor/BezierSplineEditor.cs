using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using static Kamgam.BridgeBuilder25D.BezierSpline;

namespace Kamgam.BridgeBuilder25D
{
	[CustomEditor(typeof(BezierSpline))]
	public class BezierSplineEditor : Editor
	{
		BezierSpline spline;

		protected Bridge25D bridgeBuilder;
		public Bridge25D BridgeBuilder
		{
			get
			{
				if (bridgeBuilder == null && target != null)
				{
					bridgeBuilder = ((BezierSpline)target).GetComponentInParent<Bridge25D>();
				}
				return bridgeBuilder;
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
			// Already drawn by the bridge.
			// DrawInspectorGUI(spline);
		}

		public static void DrawInspectorGUI(BezierSpline spline)
		{
			var s = new GUIStyle(GUI.skin.label);
			s.fontStyle = FontStyle.Bold;
			GUILayout.Label("Spline", s);

			GUILayout.BeginVertical(EditorStyles.helpBox);

			int oldResolution = spline.Resolution;
			spline.Resolution = EditorGUILayout.IntSlider(
				new GUIContent("Resolution", "Number of segments between each pair of bezier points. The higher the number the better the approximation."),
				spline.Resolution,
				1, 20
				);

			GUILayout.EndVertical();

			EditorGUILayout.Space();

			// Did change?
			if (oldResolution != spline.Resolution)
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
			bool pcC = spline.HasPointCountChanged();
			bool tC = spline.HasTransformChanged();
			if (pcC || tC)
			{
				spline.SetDirty();
			}

			DrawPoints(spline, showHandles: true);

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
