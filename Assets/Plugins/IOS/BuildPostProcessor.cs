using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Linq;

public class BuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // Get project into C#
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);


            // Get targetGUID
            //var targetName = PBXProject.GetUnityTargetName();
            //var targetGUID = project.TargetGuidByName(targetName);

            //-- GetUnityTargetName  TargetGuidByName 대체
            var targetGUID = project.GetUnityFrameworkTargetGuid();
            Debug.Log("targetGUID : " + targetGUID);
            var g = project.GetUnityMainTargetGuid();
            Debug.Log("GetUnityMainTargetGuid : " + g);
            //--

            // Add Shell Script to copy folders and files after running successfully
            // 성공적으로 실행 한 후 폴더 및 파일을 복사하기위한 쉘 스크립트 추가
            var shellScriptName = "Copy edited Objective C folders and files back to Unity";
            var shellPath = "/bin/sh";
            var shellScript = $"cp -r \"$PROJECT_DIR/Libraries/Plugins/iOS\" {Directory.GetCurrentDirectory()}/Assets/Plugins";

            var allBuildPhasesGUIDS = project.GetAllBuildPhasesForTarget(targetGUID);

            var foundShellScript = allBuildPhasesGUIDS.Where(buildPhasesGUID => project.GetBuildPhaseName(buildPhasesGUID) == shellScriptName).FirstOrDefault();
            if (foundShellScript == null)
            {
                project.AddShellScriptBuildPhase(targetGUID, shellScriptName, shellPath, shellScript);
                Debug.Log($"Added custom shell script: {shellScriptName}");
            }


            // Overwrite
            project.WriteToFile(projectPath);
        }
    }
}