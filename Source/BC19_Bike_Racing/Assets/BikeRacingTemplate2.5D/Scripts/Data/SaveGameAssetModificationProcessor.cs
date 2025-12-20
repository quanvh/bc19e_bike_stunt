#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Kamgam.BikeRacing25D
{
    /*
    /// <summary>
    /// In the editor if AssetDatabase.SaveAssets() is called then every asset is saved which causes a hickup in the editor.<br />
    /// To avoid this this class is enabled before calling AssetDatabase.SaveAssets() and disabled afterwards.<br />
    /// It filters the assets to save and only leaves the SaveGameFile to be saved, which speeds up the saving process.
    /// </summary>
    public class SaveGameAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        static bool enabled = false;
        static string filename = "";
        static string[] singleFileList = new string[1];

    public static void Enable(string savegameFileName)
        {
            filename = savegameFileName;
            enabled = true;
        }

        public static void Disable()
        {
            enabled = false;
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (!enabled && !EditorApplication.isPlaying)
                return paths;

            foreach (string path in paths)
            {
                if (path.EndsWith(filename))
                {
                    singleFileList[0] = path;
                    Debug.Log(singleFileList[0]);
                    return singleFileList;
                }
            }
            Debug.Log("none");

            return new string[] { };
        }
    }
    */
}
#endif
