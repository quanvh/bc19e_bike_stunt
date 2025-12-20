using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class FixIAP4_12 : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        SessionState.SetBool("SelfDeclaredAndroidDependenciesDisabled:com.unity.purchasing", true);
    }

}
