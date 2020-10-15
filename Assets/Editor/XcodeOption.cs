using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using UnityEditor.iOS.Xcode;


public class XcodeOption : MonoBehaviour
{
    static string _projPath;
    static string _path;
    static PBXProject _project;
    //[PostProcessBuild(999)]
    [PostProcessBuild]
    private static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            _path = path;

            AddFramework();

            AddInfoPlist();

            var projCapability = new ProjectCapabilityManager(_projPath, "Unity-iPhone/mmk.entitlements", "Unity-iPhone");

            projCapability.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            projCapability.AddPushNotifications(Debug.isDebugBuild);
            projCapability.WriteToFile();
        }


    }

    private static void AddInfoPlist()
    {
        string infoPlistPath = _path + "/Info.plist";

        PlistDocument plistDoc = new PlistDocument();
        plistDoc.ReadFromFile(infoPlistPath);




        if (plistDoc.root != null)
        {
            var rootDict = plistDoc.root;
            if (rootDict["NSAppTransportSecurity"] == null)
            {
                rootDict.CreateDict("NSAppTransportSecurity");
            }

            rootDict["NSAppTransportSecurity"].AsDict().SetBoolean("NSAllowsArbitraryLoads", true);
            //rootDict["NSAppTransportSecurity"].AsDict().SetBoolean("NSAllowsArbitraryLoadsInWebContent", true);

            //서브도메인 http 예외 코드
            //{
                //var exceptionDomains = rootDict["NSAppTransportSecurity"].AsDict().CreateDict("NSExceptionDomains");
                //var domain = exceptionDomains.CreateDict("YOURDOMAIN.com");

                ////PlistElementDict exceptionsDict = allowsDict.CreateDict("NSExceptionDomains");
                ////PlistElementDict domainDict = exceptionsDict.CreateDict("amazonaws.com");

                //domain.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
                //domain.SetBoolean("NSIncludesSubdomains", true);
            //}

            rootDict.SetString("UIRequiresFullScreen", "No");
            rootDict.SetString("Status bar is initially hidden", "NO");
            rootDict.SetString("View controller-based status bar appearance", "YES");
            //plistDoc.root.SetString("FirebaseMessagingAutoInitEnabled", "No");
            plistDoc.WriteToFile(infoPlistPath);
        }
        else
        {
            Debug.LogError("ERROR: Can't open " + infoPlistPath);
        }
    }


    private static void AddFramework()
    {
        _projPath = _path + "/Unity-iPhone.xcodeproj/project.pbxproj";

        _project = new PBXProject();
        _project.ReadFromFile(_projPath);

        var _target = _project.GetUnityMainTargetGuid();
        _project.AddFrameworkToProject(_target, "UserNotifications.framework", false);
        _project.WriteToFile(_projPath);
    }




    private static void AddGameKitCapability()
    {
        string infoPlistPath = _path + "/Info.plist";

        var plistParser = new PlistDocument();
        plistParser.ReadFromFile(infoPlistPath);


        plistParser.root["UIRequiredDeviceCapabilities"].AsArray().AddString("gamekit");

        plistParser.WriteToFile(infoPlistPath);
    }




}



