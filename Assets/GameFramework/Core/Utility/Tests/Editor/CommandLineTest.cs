using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Core;

namespace Tests
{
    [TestFixture]
    public class CommandLineTest
    {
        [Test]
        public void TestCmdLine()
        {
            string commandLine = @"C:\Program Files\Unity\Editor\Unity.exe -quit -batchmode -projectPath C:\Users\UserName\Documents\MyProject -executeMethod MyEditorScript.PerformBuild";
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
    }
}