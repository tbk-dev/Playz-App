using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using UnityEditor.iOS.Xcode;


public static class XcodeOption
{

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            {
                string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);

                string target = pbxProject.TargetGuidByName("Unity-iPhone");
                pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
                //pbxProject.AddFrameworkToProject(target, "User")

                pbxProject.WriteToFile(projectPath);
            }

            {
                string infoPlistPath = path + "/Info.plist";

                PlistDocument plistDoc = new PlistDocument();
                plistDoc.ReadFromFile(infoPlistPath);
                if (plistDoc.root != null)
                {
                    plistDoc.root.SetBoolean("tttttttbool", false);
                    plistDoc.root.SetString("tttttttString", "MY APP NAME");
                    plistDoc.root.SetString("FirebaseMessagingAutoInitEnabled", "No");
                    plistDoc.WriteToFile(infoPlistPath);
                }
                else
                {
                    Debug.LogError("ERROR: Can't open " + infoPlistPath);
                }
            }

            //ITSAppUsesNonExemptEncryption
        }
    }

}