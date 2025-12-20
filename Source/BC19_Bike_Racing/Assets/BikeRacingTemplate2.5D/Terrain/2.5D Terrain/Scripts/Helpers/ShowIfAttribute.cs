using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Kamgam.Terrain25DLib.Helpers
{
    /// <summary>
    /// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
    /// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        #region Fields

        public string comparedPropertyName { get; private set; }
        public object comparedValue { get; private set; }
        public DisablingType disablingType { get; private set; }

        /// <summary>
        /// Types of comperisons.
        /// </summary>
        public enum DisablingType
        {
            ReadOnly = 2,
            DontDraw = 3
        }

        #endregion

        /// <summary>
        /// Only draws the field only if a condition is met. Supports enum and bools.
        /// </summary>
        /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
        /// <param name="comparedValue">The value the property is being compared to.</param>
        /// <param name="disablingType">The type of disabling that should happen if the condition is NOT met. Use "Invert" to hide if the condition IS met. Defaulted to DisablingType.DontDraw.</param>
        public ShowIfAttribute(string comparedPropertyName, object comparedValue, DisablingType disablingType = DisablingType.DontDraw)
        {
#if UNITY_EDITOR
            this.comparedPropertyName = comparedPropertyName;
            this.comparedValue = comparedValue;
            this.disablingType = disablingType;
#endif
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        #region Fields

        // Reference to the attribute on the property.
        ShowIfAttribute drawIf;

        // Field that is being compared.
        SerializedProperty comparedField;

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShowMe(property) == false && drawIf.disablingType == ShowIfAttribute.DisablingType.DontDraw)
                return 0f;

            // The height of the property should be defaulted to the default height.
            return base.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// Errors default to showing the property.
        /// </summary>
        private bool ShowMe(SerializedProperty property)
        {
            drawIf = attribute as ShowIfAttribute;

            // Replace propertyname to the value from the parameter
            string basePath = property.propertyPath;
            if (basePath.EndsWith("]")) // avoid failure if the property itself is an array.
            {
                basePath = System.Text.RegularExpressions.Regex.Replace(basePath, "\\.Array\\.data\\[[0-9]+\\]$", "");
            }
            string path = basePath.Contains(".") ? System.IO.Path.ChangeExtension(basePath, drawIf.comparedPropertyName) : drawIf.comparedPropertyName;

            comparedField = property.serializedObject.FindProperty(path);
            if (comparedField == null)
            {
                Debug.LogError("Cannot find property with name: " + path);
                return true;
            }

            bool result;

            // get the value & compare based on types
            switch (comparedField.type)
            { // Possible extend cases to support your own type
                case "bool":
                    result = comparedField.boolValue.Equals(drawIf.comparedValue);
                    break;
                case "Enum":
                    result = comparedField.enumValueIndex.Equals((int)drawIf.comparedValue);
                    break;
                default:
                    Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                    result =  true;
                    break;
            }

            // Debug.Log("Checking " + basePath + " > " + result);

            return result;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // If the condition is met, simply draw the field.
            if (ShowMe(property))
            {
                EditorGUI.PropertyField(position, property);
            } //...check if the disabling type is read only. If it is, draw it disabled
            else if (drawIf.disablingType == ShowIfAttribute.DisablingType.ReadOnly)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property);
                GUI.enabled = true;
            }
        }
    }
#endif
}
