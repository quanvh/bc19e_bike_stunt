using UnityEngine;
using UnityEditor;
using System.IO;

namespace Kamgam.Terrain25DLib
{
	[InitializeOnLoad]
	public class InstallGizmo
	{
        static InstallGizmo()
        {
            string srcFile = Application.dataPath + "/BikeRacingTemplate2.5D/Terrain/2.5D Terrain/Terrain25D.tiff";
            if (File.Exists(srcFile))
            {
                string dstFile = Application.dataPath + "/Gizmos"; 

                if (!Directory.Exists(dstFile))
                    Directory.CreateDirectory(dstFile);

                dstFile += "/Terrain25D.tiff";

                if (!File.Exists(dstFile))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(dstFile)))
                        Directory.CreateDirectory(Path.GetDirectoryName(dstFile));

                    File.Copy(srcFile, dstFile, true);
                }
            }
        }
    }
}