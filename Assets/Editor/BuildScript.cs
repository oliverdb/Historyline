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
				if(Application.isEditor){
					arguments = new string[]{"nunitargs","\"-out:nunit_out.txt -result:historyline-test-res.xml -format:nunit2 Assembly-CSharp\""};
				}
		
    		  Debug.Log("GetCommandLineArgs: " +  String.Join(", ", arguments));
				var nunitArgs =new string[]{ "Assembly-CSharp","/out:nunit_out.txt","/result:historyline-test-res.xml"};
			  //find the nunit parameter
				for(int i=0;i<arguments.Length;i++){
					if(arguments[i].Contains("nunitargs")){
						nunitArgs = arguments[i+1].Replace("\"","").Split(' ');
						Debug.Log("Nunit args Application.dataPathfound: " + String.Join(",", nunitArgs));
					}
				}		
			NUnitLiteUnityRunner.RunTests(nunitArgs);
		
			
	
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
