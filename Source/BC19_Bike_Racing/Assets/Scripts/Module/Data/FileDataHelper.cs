using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace Bacon
{
    public class FileDataHelper
    {
        private static bool LogDebug => RouteController.Instance.logDebug;
        public static void Save<T>(string fileName, object data, Action<Exception> error = null)
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                string text = Path.Combine(Application.persistentDataPath, fileName + ".gd");
                if (File.Exists(text))
                {
                    using (FileStream fileStream = File.Open(text, FileMode.OpenOrCreate))
                    {
                        binaryFormatter.Serialize(fileStream, (T)data);
                        fileStream.Close();
                        if (LogDebug)
                            Debug.Log("[FileData] Save Done: " + fileName + " " + DateTime.Now.ToString() + "\n" + text);
                    }
                }
                else
                {
                    using (FileStream fileStream2 = File.Create(text))
                    {
                        binaryFormatter.Serialize(fileStream2, (T)data);
                        fileStream2.Close();
                        if (LogDebug)
                            Debug.Log("[FileData] Save Done: " + fileName + "\n" + text);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                error?.Invoke(ex);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        public static object Load<T>(string fileName)
        {
            try
            {
                T val = default(T);
                string path = Path.Combine(Application.persistentDataPath, fileName + ".gd");
                if (File.Exists(path))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    using (FileStream fileStream = File.Open(path, FileMode.Open))
                    {
                        val = (T)binaryFormatter.Deserialize(fileStream);
                        fileStream.Close();
                        if (LogDebug)
                            Debug.Log("[LoadData] Done: " + fileName);
                    }

                    return val;
                }
                if (LogDebug)
                    Debug.LogWarning("[LoadData] Error " + fileName + " NOT found");
                return null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        public static void Delete(string fileName)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, fileName + ".gd");
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log("ResetData DONE!");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}