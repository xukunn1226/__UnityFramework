using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Application.Runtime
{
    public class AssembyBuilder
    {
        // [MenuItem("Tools/Build Assembly Sync")]
        private static void BuildAssemblySync()
        {
            BuildAssembly(true);
        }

        static public void BuildAssembly(bool wait, bool release = true)
        {
            string buildOutput = "Temp/MyAssembly";
            var outputAssembly = string.Format($"{buildOutput}/{ILStartup.dllFilename}.dll");
            Directory.CreateDirectory(buildOutput);

            string[] scriptPaths = Directory.GetFiles("Assets/Application/hotfix", "*.cs", SearchOption.AllDirectories);

            var assemblyBuilder = new AssemblyBuilder(outputAssembly, scriptPaths);
            assemblyBuilder.additionalReferences    = null; // GetAdditionalReferences();
            assemblyBuilder.additionalDefines       = new string[] { "DISABLE_ILRUNTIME_DEBUG" };
            assemblyBuilder.excludeReferences       = GetExcludeReferences();
            assemblyBuilder.buildTarget             = EditorUserBuildSettings.activeBuildTarget;
            assemblyBuilder.buildTargetGroup        = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            assemblyBuilder.flags                   = release ? AssemblyBuilderFlags.None : AssemblyBuilderFlags.DevelopmentBuild;
            assemblyBuilder.referencesOptions       = ReferencesOptions.UseEngineModules;
            
            assemblyBuilder.compilerOptions.AllowUnsafeCode             = true;
            assemblyBuilder.compilerOptions.CodeOptimization            = release ? CodeOptimization.Release : CodeOptimization.Debug;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel       = PlayerSettings.GetApiCompatibilityLevel(assemblyBuilder.buildTargetGroup);
            assemblyBuilder.compilerOptions.AdditionalCompilerArguments = new string[] { "-optimize", "-deterministic" };

            // Called on main thread
            assemblyBuilder.buildStarted += delegate (string assemblyPath)
            {
                Debug.LogFormat("Assembly build started for {0}", assemblyPath);
            };

            // Called on main thread
            assemblyBuilder.buildFinished += delegate (string assemblyPath, CompilerMessage[] compilerMessages)
            {
                var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Assembly build finished for {0}", assemblyPath);

                for(int i = 0; i < warningCount; ++i)
                {
                    if(compilerMessages[i].type == CompilerMessageType.Warning)
                    {
                        Debug.Log($"{compilerMessages[i].message}   file: {compilerMessages[i].file}  line: {compilerMessages[i].line}  column: {compilerMessages[i].column}");
                    }
                }
                for(int i = 0; i < errorCount; ++i)
                {
                    if(compilerMessages[i].type == CompilerMessageType.Error)
                    {
                        Debug.Log($"{compilerMessages[i].message}   file: {compilerMessages[i].file}  line: {compilerMessages[i].line}  column: {compilerMessages[i].column}");
                    }
                }
                Debug.LogFormat("Warnings: {0} - Errors: {0}", errorCount, warningCount);

                if (errorCount == 0)
                {
                    string dstPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}");
                    if(!Directory.Exists(dstPath))
                        Directory.CreateDirectory(dstPath);
                    File.Copy(string.Format($"{buildOutput}/{ILStartup.dllFilename}.dll"), string.Format($"{dstPath}/{ILStartup.dllFilename}.dll"), true);
                    File.Copy(string.Format($"{buildOutput}/{ILStartup.dllFilename}.pdb"), string.Format($"{dstPath}/{ILStartup.dllFilename}.pdb"), true);
                }
            };

            // Start build of assembly
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
                return;
            }

            if (wait)
            {
                while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
                    System.Threading.Thread.Sleep(10);
            }
        }

        static private string[] GetAdditionalReferences()
        {
            return new string[] {"Library/ScriptAssemblies/Framework.Core.Runtime.dll",
                                 "Library/ScriptAssemblies/Application.Runtime.dll",
                                 "Library/ScriptAssemblies/UnityEngine.UI.dll",
                                 "Library/ScriptAssemblies/FMODUnity.dll",
                                 "Library/ScriptAssemblies/Cinemachine.dll"};
        }

        static private string[] GetExcludeReferences()
        {
            return new string[] { "Library/ScriptAssemblies/Application.Logic.dll" };
        }
    }
}