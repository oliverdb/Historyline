using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnitLite.Runner;
using NUnitLite;

public class NunitTextUnityTestUI  {

        private CommandLineOptions commandLineOptions;
        private int reportCount = 0;

        private NUnit.ObjectList assemblies = new NUnit.ObjectList();

        private TextWriter writer;

        private ITestAssemblyRunner runner;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextUI"/> class.
        /// </summary>
        public NunitTextUnityTestUI() : this(ConsoleWriter.Out) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextUI"/> class.
        /// </summary>
        /// <param name="writer">The TextWriter to use.</param>
        public NunitTextUnityTestUI(TextWriter writer)
        {
            // Set the default writer - may be overridden by the args specified
            this.writer = writer;
            this.runner = new NUnitLiteTestAssemblyRunner(new NUnitLiteTestAssemblyBuilder());
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Execute a test run based on the aruments passed
        /// from Main.
        /// </summary>
        /// <param name="args">An array of arguments</param>
        /// <param name="assembly">assembly to be tested</param>
        public void Execute(string[] args)
        {
            // NOTE: Execute must be directly called from the
            // test assembly in order for the mechanism to work.
          //  Assembly callingAssembly = assembly; //Modified here

            this.commandLineOptions = ProcessArguments( args );

            if (!commandLineOptions.ShowHelp && !commandLineOptions.Error)
            {
                if (commandLineOptions.Wait && commandLineOptions.OutFile != null)
                    writer.WriteLine("Ignoring /wait option - only valid for Console");

                IDictionary loadOptions = new Hashtable();
                //if (options.Load.Count > 0)
                //    loadOptions["LOAD"] = options.Load;

                IDictionary runOptions = new Hashtable();
                if (commandLineOptions.TestCount > 0)
                    runOptions["RUN"] = commandLineOptions.Tests;

                try
                {
                    foreach (string name in commandLineOptions.Parameters)
                        assemblies.Add(Assembly.Load(name));

                   // if (assemblies.Count == 0)
                     //   assemblies.Add(assembly);

                    // TODO: For now, ignore all but first assembly
                    Assembly assembly = assemblies[0] as Assembly;

                    if (!runner.Load(assembly, loadOptions))
                    {
                        Console.WriteLine("No tests found in assembly {0}", assembly.GetName().Name);
                        return;
                    }

                    if (commandLineOptions.Explore)
                        ExploreTests();
                    else
                        RunTests();
                }
                catch (FileNotFoundException ex)
                {
                    writer.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    writer.WriteLine(ex.ToString());
                }
                finally
                {
                    if (commandLineOptions.OutFile == null)
                    {
                        /*if (commandLineOptions.Wait)
                        {
                            Console.WriteLine("Press Enter key to continue . . .");
                            Console.ReadLine();
                        }*/
                    }
                    else
                    {
                        writer.Close();
                    }
                }
            }
        }

        private void RunTests()
        {
            ITestResult result = runner.Run(TestListener.NULL, TestFilter.Empty);
            ReportResults(result);
            string resultFile = commandLineOptions.ResultFile;
            if (resultFile != null)
            {
                XmlTextWriter resultWriter = new XmlTextWriter(resultFile, System.Text.Encoding.UTF8);
                resultWriter.Formatting = Formatting.Indented;
                result.ToXml(true).WriteTo(resultWriter);
                resultWriter.Close();
            }
        }

        private void ExploreTests()
        {
     
		NUnit.Framework.Api.XmlNode testNode = runner.LoadedTest.ToXml(true);

            string listFile = commandLineOptions.ExploreFile;
            XmlTextWriter testWriter = listFile != null && listFile.Length > 0
                ? new XmlTextWriter(listFile, System.Text.Encoding.UTF8)
                : new XmlTextWriter(Console.Out);
            testWriter.Formatting = Formatting.Indented;
            testNode.WriteTo(testWriter);
            testWriter.Close();
        }

        /// <summary>
        /// Reports the results.
        /// </summary>
        /// <param name="result">The result.</param>
        private void ReportResults( ITestResult result )
        {
            ResultSummary summary = new ResultSummary(result);

            writer.WriteLine("{0} Tests : {1} Failures, {2} Errors, {3} Not Run",
                summary.TestCount, summary.FailureCount, summary.ErrorCount, summary.NotRunCount);

            if (summary.FailureCount > 0 || summary.ErrorCount > 0)
                PrintErrorReport(result);

            if (summary.NotRunCount > 0)
                PrintNotRunReport(result);

            if (commandLineOptions.Full)
                PrintFullReport(result);
        }
        #endregion

        #region Helper Methods
        private CommandLineOptions ProcessArguments(string[] args)
        {
            this.commandLineOptions = new CommandLineOptions();
            commandLineOptions.Parse(args);

            if (commandLineOptions.OutFile != null)
                this.writer = new StreamWriter(commandLineOptions.OutFile);
            else
                this.writer = ConsoleWriter.Out;

            if (!commandLineOptions.NoHeader)
                WriteCopyright();

            if (commandLineOptions.ShowHelp)
                writer.Write(commandLineOptions.HelpText);
            else if (commandLineOptions.Error)
                writer.WriteLine(commandLineOptions.ErrorMessage);

            return commandLineOptions;
        }

        private void WriteCopyright()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
#if NUNITLITE
            string title = "NUnitLite";
#else
            string title = "NUNit Framework";
#endif
            System.Version version = executingAssembly.GetName().Version;
            string copyright = "Copyright (C) 2012, Charlie Poole";
            string build = "";

#if !NETCF_1_0
            object[] attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attrs.Length > 0)
            {
                AssemblyTitleAttribute titleAttr = (AssemblyTitleAttribute)attrs[0];
                title = titleAttr.Title;
            }

            attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attrs.Length > 0)
            {
                AssemblyCopyrightAttribute copyrightAttr = (AssemblyCopyrightAttribute)attrs[0];
                copyright = copyrightAttr.Copyright;      
            }

            attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            if (attrs.Length > 0)
            {
                AssemblyConfigurationAttribute configAttr = (AssemblyConfigurationAttribute)attrs[0];
                build = string.Format("({0})", configAttr.Configuration); 
            }
#endif

            writer.WriteLine(String.Format("{0} {1} {2}", title, version.ToString(3), build));
            writer.WriteLine(copyright);
            writer.WriteLine();

            string clrPlatform = Type.GetType("Mono.Runtime", false) == null ? ".NET" : "Mono";
            writer.WriteLine("Runtime Environment -");
            writer.WriteLine("    OS Version: {0}", Environment.OSVersion);
            writer.WriteLine("  {0} Version: {1}", clrPlatform, Environment.Version);
            writer.WriteLine();
        }

        private void PrintErrorReport(ITestResult result)
        {
            reportCount = 0;
            writer.WriteLine();
            writer.WriteLine("Errors and Failures:");
            PrintErrorResults(result);
        }

        private void PrintErrorResults(ITestResult result)
        {
            if (result.HasChildren)
                foreach (ITestResult r in result.Children)
                    PrintErrorResults(r);
            else if (result.ResultState == ResultState.Error || result.ResultState == ResultState.Failure)
            {
                writer.WriteLine();
                writer.WriteLine("{0}) {1} ({2})", ++reportCount, result.Name, result.FullName);
                //if (options.ListProperties)
                //    PrintTestProperties(result.Test);
                writer.WriteLine(result.Message);
#if !NETCF_1_0
                writer.WriteLine(result.StackTrace);
#endif
            }
        }

        private void PrintNotRunReport(ITestResult result)
        {
            reportCount = 0;
            writer.WriteLine();
            writer.WriteLine("Tests Not Run:");
            PrintNotRunResults(result);
        }

        private void PrintNotRunResults(ITestResult result)
        {
            if (result.HasChildren)
                foreach (ITestResult r in result.Children)
                    PrintNotRunResults(r);
            else if (result.ResultState == ResultState.Ignored || result.ResultState == ResultState.NotRunnable || result.ResultState == ResultState.Skipped)
            {
                writer.WriteLine();
                writer.WriteLine("{0}) {1} ({2}) : {3}", ++reportCount, result.Name, result.FullName, result.Message);
                //if (options.ListProperties)
                //    PrintTestProperties(result.Test);
            }
        }

        private void PrintTestProperties(ITest test)
        {
            foreach (PropertyEntry entry in test.Properties)
                writer.WriteLine("  {0}: {1}", entry.Name, entry.Value);            
        }

        private void PrintFullReport(ITestResult result)
        {
            writer.WriteLine();
            writer.WriteLine("All Test Results:");
            PrintAllResults(result, " ");
        }

        private void PrintAllResults(ITestResult result, string indent)
        {
            string status = null;
            switch (result.ResultState.Status)
            {
                case TestStatus.Failed:
                    status = "FAIL";
                    break;
                case TestStatus.Skipped:
                    status = "SKIP";
                    break;
                case TestStatus.Inconclusive:
                    status = "INC ";
                    break;
                case TestStatus.Passed:
                    status = "OK  ";
                    break;
            }

            writer.Write(status);
            writer.Write(indent);
            writer.WriteLine(result.Name);

            if (result.HasChildren)
                foreach (ITestResult r in result.Children)
                    PrintAllResults(r, indent + "  ");
        }
        #endregion
}
