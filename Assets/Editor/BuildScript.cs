using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

public class BuildScript{

	 static string[] SCENES = FindEnabledEditorScenes();

        static string APP_NAME = "historyline";
        static string TARGET_DIR = "builds";

        [MenuItem ("Custom/CI/Build Mac OS X")]
        static void PerformMacOSXBuild ()
        {
                 string target_dir = APP_NAME + ".app";
                 GenericBuild(SCENES, TARGET_DIR + "/" + target_dir, BuildTarget.StandaloneOSXIntel,BuildOptions.None);
        }
	
		[MenuItem ("Custom/CI/Build Web")]
		static void PerformWebBuild ()
        {
                 string target_dir = APP_NAME + ".unity3d";
                 GenericBuild(SCENES, TARGET_DIR + "/web/" + target_dir, BuildTarget.WebPlayer,BuildOptions.None);
        }
		[MenuItem ("Custom/CI/Do Unit Test")]
		static void DoNunitTest(){
			  var arguments = System.Environment.GetCommandLineArgs();
    		  Debug.Log("GetCommandLineArgs: {0}" +  String.Join(", ", arguments));	
/*		
		Invalid option: -projectPath
Invalid option: -quit
Invalid option: -batchmode
Invalid option: -executeMethod
	*/
		string[] unityCommands = new string[]{"projectPath","quit","batchmode","executeMethod"};
		List<string> commandsfiltered = new List<string>();
		foreach(string command in arguments){ //filter out all unity commands
			foreach(var unityc in unityCommands){
				if(command.Contains(unityc))
					continue;
			}
			commandsfiltered.Add(command);
		}		
			NUnitLiteUnityRunner.RunTests(commandsfiltered.ToArray());
		}

	private static string[] FindEnabledEditorScenes() {
		List<string> EditorScenes = new List<string>();
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path);
		}
		return EditorScenes.ToArray();
	}

        static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
        {
                EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
                string res = BuildPipeline.BuildPlayer(scenes,target_dir,build_target,build_options);
                if (res.Length > 0) {
                        throw new Exception("BuildPlayer failure: " + res);
                }
        }
	
}
