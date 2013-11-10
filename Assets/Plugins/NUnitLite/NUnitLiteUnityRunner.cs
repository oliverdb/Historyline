// Copyright (C) 2013 by Andrew Zhilin <andrew_zhilin@yahoo.com>

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnitLite;
using NUnitLite.Runner;
using UnityEngine;

#endregion

/* NOTE:
 *
 * This is a test runner for NUnitLite, that redirects test results
 * to Unity console.
 *
 * After compilation of C# files Unity gives you two assemblies:
 *
 * - Assembly-CSharp-firstpass.dll for 'Plugins' and 'Standard Assets'
 * - Assembly-CSharp.dll           for another scripts
 *
 * Then, if you want have tests in both places - you should call
 * NUnitLiteUnityRunner.RunTests() from both places. One call per assembly
 * is enough, but you can call it as many times as you want - all
 * calls after first are ignored.
 *
 * You can use 'MonoBahavior' classes for tests, but Unity give you
 * one harmless warning per class. Using special Test classes would be
 * better idea.
 */


public static class NUnitLiteUnityRunner
{
    private static readonly HashSet<Assembly> Tested =
        new HashSet<Assembly>();

    public static Action<string, ResultSummary> Presenter { get; set; }

    static NUnitLiteUnityRunner()
    {
        Presenter = UnityConsolePresenter;
    }
	
	public static void RunTests(string[] commandArgs)//"/out:console-test","/result:console-test-res-xml"
    {
        RunTests(Assembly.GetCallingAssembly(),commandArgs);
    }

    public static void RunTests()
    {
        RunTests(Assembly.GetCallingAssembly(),null);
    }
	
	

    private static void RunTests(Assembly assembly,string[] commandArgs)
    {
        Debug.Log("Running Assembly: " + assembly.FullName);
		if (assembly == null)
            throw new ArgumentNullException("assembly");

      //  if (Tested.Contains(assembly))
        //    return;
        Tested.Add(assembly);

        using (var sw = new StringWriter())
        {
           if(commandArgs != null){
				//var runner = new NunitTextUnityTestUI(sw);
				var runner = new TextUI();
            	runner.Execute(commandArgs);
			}else{
				var runner = new NUnitStreamUI(sw);
				var resultSummary = runner.Summary;
            	var resultText = sw.GetStringBuilder().ToString();
           		Presenter(resultText, resultSummary);
			}
			
          
        }
    }


    private static void UnityConsolePresenter(string longResult, ResultSummary result)
    {
        if (result != null && (result.ErrorCount > 0 || result.FailureCount > 0))
            Debug.LogWarning(longResult);
        else
            Debug.Log(longResult);
    }
}
