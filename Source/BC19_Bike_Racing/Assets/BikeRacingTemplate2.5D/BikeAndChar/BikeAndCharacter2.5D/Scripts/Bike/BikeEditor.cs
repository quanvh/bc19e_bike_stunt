#if UNITY_EDITOR
/*
using UnityEngine;
using System.Collections.Generic;
using Kamgam.Helpers;
using System;
using UnityEditor;

namespace Kamgam.BikeAndCharacter25D
{
    [CustomEditor(typeof(Bike))]
    public class BikeEditor : Editor
    {
        protected AnimationCurve localVelocityCurve;
        protected int localVelocityCurveKeyIndex;
        protected float refSize = 30f;
        protected float durationInSec = 5f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var bike = target as Bike;

            if (localVelocityCurve == null)
            {
                localVelocityCurve = new AnimationCurve();
            }

            float value = bike.VelocityVector.y;

            GUILayout.Label("Y Ref Value:");
            refSize = EditorGUILayout.FloatField(refSize);

            GUILayout.Label("X DurationInSec.:");
            durationInSec = EditorGUILayout.FloatField(durationInSec);

            if (localVelocityCurve.keys.Length < 10)
            {
                int len = Application.targetFrameRate * 3;
                for (int i = 0; i < len; i++)
                {
                    localVelocityCurve.AddKey((float)i / (Application.targetFrameRate * durationInSec), 0f);
                }
                localVelocityCurveKeyIndex = 0;
            }

            var magnitude = bike.VelocityVectorLocal.magnitude;
            if(EditorApplication.isPlaying && EditorApplication.isPaused == false)
            {
                localVelocityCurveKeyIndex++;
                if (localVelocityCurveKeyIndex >= localVelocityCurve.keys.Length)
                {
                    Keyframe[] keyframes = localVelocityCurve.keys;
                    for (int i = 0; i < keyframes.Length-1; i++)
                    {
                        keyframes[i].value = keyframes[i + 1].value;
                    }
                    localVelocityCurve.keys = keyframes;
                    ChangeKey(localVelocityCurve, keyframes.Length-1, value);
                }
                else
                {
                    ChangeKey(localVelocityCurve, localVelocityCurveKeyIndex, value);
                }
            }
            EditorGUILayout.CurveField(localVelocityCurve, GUILayout.Height(300));
        }

        void ChangeKey(AnimationCurve curve, int index, float value)
        {
            Keyframe[] keyframes = curve.keys;
            keyframes[index].value = value;
            curve.keys = keyframes;
        }
    }
}
*/
#endif