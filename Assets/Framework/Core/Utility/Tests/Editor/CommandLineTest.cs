using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Framework.Core;

namespace Framework.Core.Tests
{
    [TestFixture]
    public class CommandLineTest
    {
        [Test]
        public void TestCmdLine()
        {
            string commandLine = @"C:\Program Files\Unity\Editor\Unity.exe -quit -batchmode -projectPath C:\Users\UserName\Documents\MyProject -executeMethod MyEditorScript.PerformBuild -UseAPKExpansionFiles";
            CommandLineReader.ParseCommandLine(ToArray(commandLine, ' '));

            string v1 = string.Empty;
            string v2 = string.Empty;
            bool b1 = CommandLineReader.GetCommand("quit", ref v1);
            bool b2 = CommandLineReader.GetCommand("projectpath", ref v2);
        }

        private string[] ToArray(string commandLine, char separator)
        {
            return commandLine.Split(new char[] { separator });
        }

        [Test]
        public void TestUnsafeToLower()
        {
            UnityEngine.Profiling.Profiler.BeginSample("11111111111111");
            string str = "AcBD";
            
            string bb = str.ToLower_NoAlloc();
            UnityEngine.Profiling.Profiler.EndSample();

            Debug.Log($"{str.GetHashCode()}     {bb.GetHashCode()}");
        }
    }
}