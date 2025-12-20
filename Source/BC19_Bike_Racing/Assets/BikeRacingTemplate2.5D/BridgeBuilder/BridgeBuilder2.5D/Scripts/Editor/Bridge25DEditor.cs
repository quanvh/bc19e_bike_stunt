using UnityEngine;
using UnityEditor;
using Kamgam.BridgeBuilder25D.Helpers;
using System;

namespace Kamgam.BridgeBuilder25D
{
	[CustomEditor(typeof(Bridge25D), true)]
	[CanEditMultipleObjects]
	public class Bridge25DEditor : Editor
	{
        Bridge25D bridge;

		protected static double lastChangeCheckTime;
		protected static bool valuesChanged = false;

		static SerializedObject tmpSerializedObject = null;

		void OnEnable()
		{
			bridge = (Bridge25D)target;
			bridge.RefreshElementsList();

			EditorApplication.update -= onEditorUpdate;
			EditorApplication.update += onEditorUpdate;

			Undo.undoRedoPerformed -= onEditorUndoRedo;
			Undo.undoRedoPerformed += onEditorUndoRedo;

			EditorApplication.playModeStateChanged -= playmodeStateChanged;
			EditorApplication.playModeStateChanged += playmodeStateChanged;
		}

		void OnDisable()
		{
			EditorApplication.update -= onEditorUpdate;
			Undo.undoRedoPerformed -= onEditorUndoRedo;
			EditorApplication.playModeStateChanged -= playmodeStateChanged;
		}

		public override void OnInspectorGUI()
		{
            DrawInspectorHeaderGUI(bridge);

			// Spline
			if (bridge.Spline != null)
				BezierSplineEditor.DrawInspectorGUI(bridge.Spline);

			// Point
			DrawDefaultInspectorGUI(bridge);
		}

		public static void DrawInspectorHeaderGUI(Bridge25D bridge)
		{
			if (bridge == null)
				return;

			EditorGUILayout.Space();

			var s = new GUIStyle(GUI.skin.label);
			s.fontStyle = FontStyle.Bold;
			GUILayout.Label("2.5D Bridge", s);

			GUILayout.BeginVertical(EditorStyles.helpBox);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Create"))
			{
				bridge.Create(bridge.Spline, forceRebuild: true);
				EditorUtility.SetDirty(bridge);
			}
			if (GUILayout.Button("Clear"))
			{
				bridge.AutoCreate = false;
				bridge.Clear();
				EditorUtility.SetDirty(bridge);
			}
			GUI.enabled = EditorApplication.isPlaying;
			if (GUILayout.Button("Break"))
			{
				bridge.BreakAt((bridge.Spline.GetPointAt(-0.1f) + bridge.Spline.GetPointAt(1.1f)) * 0.5f);
				EditorUtility.SetDirty(bridge);
			}
			GUI.enabled = true;
			if (GUILayout.Button("UpdateJoints"))
			{
				bridge.UpdateJoints();
				EditorUtility.SetDirty(bridge);
			}
			if (bridge.ProximityTrigger == null)
			{
				if (GUILayout.Button("Add Trigger"))
				{
					bridge.AddProximityTrigger();
					EditorUtility.SetDirty(bridge);
				}
			}
            else
            {
				if (GUILayout.Button("Del. Trigger"))
				{
					bridge.RemoveProximityTrigger();
					EditorUtility.SetDirty(bridge);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			EditorGUILayout.Space();
		}

		public static void DrawDefaultInspectorGUI(Bridge25D bridge)
        {
			EditorGUILayout.Space();

			var s = new GUIStyle(GUI.skin.label);
			s.fontStyle = FontStyle.Bold;
			GUILayout.Label("2.5D Bridge Details", s);

			// draw default properties
			{
				if (tmpSerializedObject == null || tmpSerializedObject.targetObject != bridge)
					tmpSerializedObject = new SerializedObject(bridge);

				tmpSerializedObject.Update();
				SerializedProperty serializedProperty = tmpSerializedObject.GetIterator();

				// Script
				serializedProperty.NextVisible(enterChildren: true);
				EditorGUILayout.PropertyField(serializedProperty); // remove to skip

				while (serializedProperty.NextVisible(enterChildren: false))
				{
					if (serializedProperty.name.Contains("PartLayerIfBroken"))
					{
						bridge.PartLayerIfBroken = EditorGUILayout.LayerField("ElementLayerIfBroken", bridge.PartLayerIfBroken);
					}
					else if (serializedProperty.name.Contains("StartAwake"))
					{
						EditorGUILayout.PropertyField(serializedProperty);
						if (!serializedProperty.boolValue && bridge.ProximityTrigger == null)
                        {
							EditorGUILayout.HelpBox("You should add a ProximityTrigger or call SetPhysicsActive() to start simulation.", MessageType.None);
						}
					}
					else
					{
						EditorGUILayout.PropertyField(serializedProperty);
					}
				}
			}

			if (tmpSerializedObject.hasModifiedProperties)
            {
				valuesChanged = true;
				tmpSerializedObject.ApplyModifiedProperties();

				bridge.UpdateProximityTrigger();

				// Auto recreate
				if (bridge.AutoCreate)
				{
					bridge.Create(bridge.Spline);
					EditorUtility.SetDirty(bridge);
				}
            }
		}

		protected void onEditorUndoRedo()
		{
			OnEditorUndoRedo(target as Bridge25D);
		}

		public static void OnEditorUndoRedo(Bridge25D bridge)
		{
			bridge.Create(bridge.Spline, true);
		}

		protected void onEditorUpdate()
		{
			OnEditorUpdate(target as Bridge25D);
		}

		public static void OnEditorUpdate(Bridge25D bridge)
		{
			if (bridge == null)
				return;

			if (EditorApplication.timeSinceStartup - lastChangeCheckTime > 0.05)
			{
				if (bridge.AutoCreate)
				{
					lastChangeCheckTime = EditorApplication.timeSinceStartup;

					if (bridge.Spline.WasDirty || valuesChanged)
					{
						valuesChanged = false;
						var settings = BridgeBuilder25DSettings.GetOrCreateSettings();
						bridge.Spline.WasDirty = false;
						bridge.Create(
							bridge.BridgePartPrefab == null ? settings.DefaultBridgePart : bridge.BridgePartPrefab,
							bridge.BridgeEdgePartPrefab == null ? settings.DefaultBridgeEdgePart : bridge.BridgeEdgePartPrefab,
							settings.DefaultBridgePhysicsMaterial, bridge.Spline
							);
						EditorUtility.SetDirty(bridge);
					}
				}
			}
		}

		private void playmodeStateChanged(PlayModeStateChange obj)
		{
			OnPlaymodeStateChanged(target as Bridge25D, obj);
		}

		public static void OnPlaymodeStateChanged(Bridge25D bridge, PlayModeStateChange obj)
		{
			if (obj == PlayModeStateChange.ExitingEditMode)
			{
				bridge.AutoCreate = false;
			}
		}

		[MenuItem("GameObject/Create 2.5D Bridge", false, 10)]
		[MenuItem("Tools/2.5D Bridge/Create 2.5D Bridge", false, 10)]
		static void CreateBridge25D(MenuCommand menuCommand)
		{
			var parent = menuCommand.context as GameObject;

			if (parent != null)
			{
				var terrainInParent = parent.GetComponentInChildren<Bridge25D>();
				if (terrainInParent != null)
				{
					Debug.LogWarning("Can not add 2.5D Bridge inside another 2.5D Bridge. Will add to parent instead.");
					if (parent.transform.parent != null)
						parent = parent.transform.parent.gameObject;
					else
						parent = null;
				}
			}

			GameObject go = new GameObject("2.5D Bridge", typeof(Bridge25D));
			GameObjectUtility.SetParentAndAlign(go, parent);
			Selection.activeObject = go;
			var bridge = go.GetComponent<Bridge25D>();

			Vector3 position;
			UtilsEditor.IntersectEditorCameraRayWithXYInTransform(go.transform.parent, out position, Vector3.zero);
			go.transform.position = position;

			var spline = go.GetComponent<BezierSpline>();
			spline.Mode2D = true;

			// Add points
			var p0 = spline.AddPointAt(Vector3.left * 2f);
			p0.handleType = BezierPoint.HandleType.Broken;
			p0.Handle2LocalPos = new Vector3(0.5f, -0.5f, 0f);

			var p1 = spline.AddPointAt(Vector3.right * 2f);
			p1.handleType = BezierPoint.HandleType.Broken;
			p1.Handle1LocalPos = new Vector3(-0.5f, -0.5f, 0f);

			Undo.RegisterCompleteObjectUndo(go, "Create " + go.name);

			bridge.AutoCreate = true;

			EditorGUIUtility.PingObject(go);
		}

		[MenuItem("Tools/2.5D Bridge/Manual (v" + BridgeBuilder25DSettings.Version + ")", false, 501)]
		static void OpenManual(MenuCommand menuCommand)
		{
			EditorUtility.OpenWithDefaultApp("Assets/BridgeBuilder2.5D/BridgeBuilder2.5DManual.pdf");
		}

		[MenuItem("Tools/2.5D Bridge/Please write a review :-)", false, 601)]
		static void OpenAssetStore(MenuCommand menuCommand)
		{
			Application.OpenURL("https://assetstore.unity.com/packages/slug/225692");
		}

		[MenuItem("Tools/2.5D Bridge/Check out my other assets.", false, 602)]
		static void OpenAssetStorePublisher(MenuCommand menuCommand)
		{
			Application.OpenURL("https://assetstore.unity.com/publishers/37829");
		}
	}
}