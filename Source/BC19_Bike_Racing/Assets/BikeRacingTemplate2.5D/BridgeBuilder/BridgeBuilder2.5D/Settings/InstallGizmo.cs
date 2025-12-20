using UnityEngine;
using UnityEditor;
using System.IO;

namespace Kamgam.BridgeBuilder25D
{
	[InitializeOnLoad]
	public class InstallGizmo
	{
        static InstallGizmo()
        {
            string srcFile = Application.dataPath + "/BikeRacingTemplate2.5D/BridgeBuilder/BridgeBuilder2.5D/BridgeBuilder25D.tiff";
            if (File.Exists(srcFile))
            {
                string dstFile = Application.dataPath + "/Gizmos"; 

                if (!Directory.Exists(dstFile))
                    Directory.CreateDirectory(dstFile);

                dstFile += "/BridgeBuilder25D.tiff";

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