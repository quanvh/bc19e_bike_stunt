using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The SaveGameProvider is a Scriptable Object which is meant to
    /// be linked (via the Inspector) to and MonoBehaviour which needs
    /// access to the SaveGame. Think of it as a very simple 
    /// DependencyInjection via the Inspector.<br />
    /// <br />
    /// The SaveGameProvider also contais the Save and Load functionality.
    /// You can choose between using a static ScriptableObject (Editor only)
    /// and a locally saved json file. It uses the Unity JsonUtility, therefore
    /// the serializable datatype are very limited. The advantage however is that
    /// the data can be stored and inspected in the Editor as it's SrciptableObject
    /// compatible.<br />
    /// REMEMBER: on an actual devices (builds) ONLY the json file will be used. The 
    /// ScriptableObject Assets are only supported to make testing in the editor easier.<br />
    /// ScriptableObjects can not be changed at runtime if outside the Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveGameProvider", menuName = "BikeRacingTemplate2.5D/SaveGameProvider", order = 1)]
    public class SaveGameProvider : ScriptableObject
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        [Tooltip("EDITOR ONLY: Check to load the file 'FileName' from disk instead of using the savegame Asset referenced in 'SaveGameAsset'.\nAt runtime this will always be false!")]
        [SerializeField]
        protected bool useFromDiskInEditor = true;

        /// <summary>
        /// Always true if not in editor. If in editor then the value of LoadFromDiskInEditor will be returned.
        /// </summary>
        public bool UseFileFromDisk
        {
            get
            {
#if UNITY_EDITOR
                return useFromDiskInEditor;
#else
				return true;
#endif
            }
        }

        /// <summary>
        /// The filesname under which the json file will be saved if UseFileFromDisk is set to true.
        /// </summary>
        [Tooltip("The filesname under which the json file will be saved if UseFileFromDisk is set to true.")]
        public string FileName;

        [System.NonSerialized]
        protected SaveGame loadedSaveGame;

        /// <summary>
        /// A flag to ensure the asset file is not initialized every time it is accessed.
        /// </summary>
        [System.NonSerialized]
        // System.NonSerialized needs to be added to avoid being memorized as Unity serializes
        // private fields on application reload if they are available in memory (which they
        // are until you restart the editor).
        // See: https://docs.unity3d.com/Manual/script-Serialization.html
        protected bool initializedAsset = false;

        [SerializeField]
        protected SaveGame saveGameAsset;

        public SaveGame SaveGame
        {
            get
            {
                return LoadIfNotYetLoaded();
            }
        }

        public bool HasInitializedSaveGame()
        {
            if (UseFileFromDisk)
            {
                return loadedSaveGame != null && loadedSaveGame.Initialized == true;
            }
            else
            {
                return saveGameAsset != null && saveGameAsset.Initialized == true;
            }
        }

        public void CreateNew()
        {
            if (UseFileFromDisk)
            {
                loadedSaveGame = ScriptableObject.CreateInstance<SaveGame>();
                loadedSaveGame.InitAfterLoad();
                Save();
            }
            else
            {
#if UNITY_EDITOR
                saveGameAsset = CreateNewAsset(saveGameAsset);
#endif
            }
        }

        public SaveGame LoadIfNotYetLoaded()
        {
            if (UseFileFromDisk)
            {
                if (loadedSaveGame == null)
                {
                    loadedSaveGame = ScriptableObject.CreateInstance<SaveGame>();
                    loadFileIntoObject(FileName, loadedSaveGame);
                }
                return loadedSaveGame;
            }
            else
            {
                if (initializedAsset == false)
                {
                    initializedAsset = true;
                    saveGameAsset.InitAfterLoad();
                }
                return saveGameAsset;
            }
        }

        protected void loadFileIntoObject(string fileName, SaveGame saveGame)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning("SaveGame: Can not load data from disk due to missing FILENAME. Maybe SavegameProvider.LoadFromDisk is set to false?");
                return;
            }

            var path = GetPersistentFilePath(fileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, saveGame);
                // Debug.Log( string.Format( "LOAD: Path:{0}, Content:{1}", path, json ) );
            }

            if (saveGame != null)
                saveGame.InitAfterLoad();
        }

        public static string GetPersistentFilePath(string fileName)
        {
#if UNITY_SWITCH && !UNITY_EDITOR
			// Reading Application.persistentDataPath should not be used on the NINNTENDO SWITCH
			return fileName;
#else
            return Application.persistentDataPath + "/" + fileName;
#endif
        }

        /// <summary>
        /// Will store the savegame data on the local drive.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            if (UseFileFromDisk)
            {
                return SaveObjectIntoFile(loadedSaveGame, FileName);
            }
            else
            {
#if UNITY_EDITOR
                if (saveGameAsset != null)
                {
                    saveGameAsset.PrepareForSaving();
                    EditorUtility.SetDirty(saveGameAsset);

                    // Schedule saving the asset at playmode exit.
                    // Doing it right here would cause some hickups in the editor
                    // as AssetDatabase operations are synchronous and very slow.
                    EditorApplication.playModeStateChanged -= onPlayModeChanged;
                    EditorApplication.playModeStateChanged += onPlayModeChanged;
                }
#endif
                return true;
            }
        }

#if UNITY_EDITOR
        void onPlayModeChanged(PlayModeStateChange stateChange)
        {
            // remove listener
            EditorApplication.playModeStateChanged -= onPlayModeChanged;

            // save on enter or exit of play mode
            if (stateChange == PlayModeStateChange.ExitingPlayMode)
            {
#if UNITY_5_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(saveGameAsset);
#else
				AssetDatabase.SaveAssets();
#endif
            }
        }
#endif

        public bool SaveObjectIntoFile(SaveGame saveGame, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning("SaveGame: Can not save data on disk due to missing FILENAME. Maybe SavegameProvider.LoadFromDisk is set to false?");
                return false;
            }

            saveGame.PrepareForSaving();

            var json = JsonUtility.ToJson(saveGame);
            var path = GetPersistentFilePath(fileName);
            File.WriteAllText(path, json);
            // Debug.Log( string.Format( "SAVE: Path:{0}, Content:{1}", path, json ) );
            return true;
        }

        /// <summary>
        /// Deletes the file from disk or creates a new empty asset in editor.
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            if (UseFileFromDisk)
            {
                return DeleteFromDisk(FileName);
            }
            else
            {
#if UNITY_EDITOR
                if (saveGameAsset != null)
                {
                    saveGameAsset = CreateNewAsset(saveGameAsset);
                }
                else
                {
                    Debug.LogError("SaveGame Asset was null, can not replace it without path info");
                }
#endif
                return true;
            }
        }

        /// <summary>
        /// Deletes the file on disk.
        /// </summary>
        /// <param name="fileName">optional, set to null to keep the current filename</param>
        /// <returns></returns>
        public bool DeleteFromDisk(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var path = GetPersistentFilePath(fileName);
            File.Delete(path);
            return true;
        }

#if UNITY_EDITOR
        public SaveGame CreateNewAsset(SaveGame assetForPathFinding = null)
        {
            string path;
            if (assetForPathFinding != null)
            {
                path = AssetDatabase.GetAssetPath(assetForPathFinding);
                int slash = path.LastIndexOf("/");
                if (slash > 0)
                    path = path.Substring(0, slash + 1) + "SaveGame.asset";
            }
            else
            {
                path = "Assets/BikeRacingTemplate2.5D/Data/SaveGame.asset";
                AssetDatabase.DeleteAsset(path);
            }

            var newAsset = ScriptableObject.CreateInstance<SaveGame>();
            newAsset.InitAfterLoad();
            AssetDatabase.CreateAsset(newAsset, path.Replace(".asset", "-new.asset"));
            AssetDatabase.SaveAssets();

            return newAsset;
        }

        /// <summary>
        /// Used in SaveGameProviderEditor (why are there no friend classes in c#).
        /// </summary>
        public SaveGame _EditorSaveGameAsset
        {
            get => saveGameAsset;
            set => saveGameAsset = value;
        }

        public void _EditorSaveAssetToJson()
        {
            if (saveGameAsset != null)
            {
                loadedSaveGame = saveGameAsset;
                bool result = SaveObjectIntoFile(saveGameAsset, FileName);
                if (result)
                {
                    Debug.Log("SaveGameProvider: Asset serialized into " + FileName);
                }
                else
                {
                    Debug.LogError("SaveGameProvider: Asset serialization failed.");
                }
            }
            else
            {
                Debug.LogError("SaveGameProvider: Asset is null. Aborted.");
            }
            loadedSaveGame = null;
        }

        /// <summary>
        /// Tries to find a SaveGameProvider SO in the assets.
        /// EDITOR ONLY!
        /// </summary>
        /// <returns></returns>
        public static SaveGameProvider FindInstanceInAssets()
        {
            var providerGUIDs = AssetDatabase.FindAssets("t:SaveGameProvider");
            if (providerGUIDs != null && providerGUIDs.Length > 0)
            {
                var provider = AssetDatabase.LoadAssetAtPath<SaveGameProvider>(AssetDatabase.GUIDToAssetPath(providerGUIDs[0]));
                if (provider != null)
                {
                    return provider;
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to find a SaveGame SO in the assets.
        /// EDITOR ONLY!
        /// </summary>
        /// <returns></returns>
        public static SaveGame FindSaveGameInstanceInAssets()
        {
            var guids = AssetDatabase.FindAssets("t:SaveGame");
            if (guids != null && guids.Length > 0)
            {
                var saveGame = AssetDatabase.LoadAssetAtPath<SaveGame>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (saveGame != null)
                {
                    return saveGame;
                }
            }

            return null;
        }

        public void Awake()
        {
            checkAndInitValues();
        }

        public void OnEnable()
        {
            if (saveGameAsset == null)
            {
                saveGameAsset = FindSaveGameInstanceInAssets();
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            checkAndInitValues();
        }

        protected void checkAndInitValues()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = "savegame.json";
            }
        }
#endif

    }
}