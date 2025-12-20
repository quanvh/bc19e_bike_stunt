// Thanks to Zan_Kievit: https://forum.unity.com/threads/hierarchy-selection-select-first-child-of-selection-and-expand-collapse-selection.1059023/
// As this is based on a script posted freely in the UnityForum this Kamgam.HierarchySelection code is also free to use.

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kamgam.HierarchySelection
{
    public static class HierarchySelection
    {
        // Cache reflection results
        public static Type TypeHierarchyWindow;
        public static MethodInfo MethodSetExpandedRecursive;
        public static MethodInfo MethodExpandTreeViewItem;

        /// <summary>
        /// Expand or collapse object in Hierarchy recursively
        /// </summary>
        /// <param name="obj">The object to expand or collapse</param>
        /// <param name="expand">A boolean to indicate if you want to expand or collapse the object</param>
        public static void SetExpandedRecursive(GameObject obj, bool expand)
        {
            if(MethodSetExpandedRecursive == null)
                MethodSetExpandedRecursive = GetHierarchyWindowType().GetMethod("SetExpandedRecursive");

            MethodSetExpandedRecursive.Invoke(GetHierarchyWindow(), new object[] { obj.GetInstanceID(), expand });
        }

        /// <summary>
        ///  Expand or collapse object in Hierarchy
        /// </summary>
        /// <param name="obj">The object to expand or collapse</param>
        /// <param name="expand">A boolean to indicate if you want to expand or collapse the object</param>
        public static void SetExpanded(GameObject obj, bool expand)
        {
            object sceneHierarchy = GetHierarchyWindowType().GetProperty("sceneHierarchy").GetValue(GetHierarchyWindow());

            if (MethodExpandTreeViewItem == null)
                MethodExpandTreeViewItem = sceneHierarchy.GetType().GetMethod("ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodExpandTreeViewItem.Invoke(sceneHierarchy, new object[] { obj.GetInstanceID(), expand });
        }

        static Type GetHierarchyWindowType()
        {
            if(TypeHierarchyWindow == null)
                return typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            return TypeHierarchyWindow;
        }

        static EditorWindow GetHierarchyWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            return EditorWindow.focusedWindow;
        }
    }
}