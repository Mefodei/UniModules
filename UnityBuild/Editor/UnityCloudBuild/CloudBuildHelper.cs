using System;
using UniGreenModules.UnityBuild.Editor.ClientBuild;
using UniGreenModules.UnityBuild.Editor.ClientBuild.BuildConfiguration;
using UniGreenModules.UnityBuild.Editor.ClientBuild.Commands;
using UniGreenModules.UnityBuild.Editor.ClientBuild.Extensions;
using UnityEngine;

    //Use UniGreenModules.CloudBuildHelper.[PreExport || PostExport]
namespace UniGreenModules
{
    using UnityBuild.Editor.UnityCloudBuild;

    //https://docs.unity3d.com/Manual/UnityCloudBuildManifestAsScriptableObject.html
    //https://docs.unity3d.com/Manual/UnityCloudBuildManifest.html
    public static class CloudBuildHelper
    {
        private static CloudBuildArgs args;

#if UNITY_CLOUD_BUILD
    public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest) {
#else
        public class DummyManifest
        {
            public T GetValue<T>()           => default(T);
            public T GetValue<T>(string key) => default(T);
        }

        public static void PreExport(DummyManifest manifest)
        {
#endif
            args = new CloudBuildArgs(
                manifest.GetValue<int>("buildNumber"),
                manifest.GetValue<string>("bundleId"),
                manifest.GetValue<string>("projectId"),
                manifest.GetValue<string>("scmCommitId"),
                manifest.GetValue<string>("scmBranch"),
                manifest.GetValue<string>("cloudBuildTargetName")
            );

            var parameters = CreateCommandParameters();
            var builder    = new UnityPlayerBuilder();

            builder.ExecuteCommands<UnityPreBuildCommand>(x => x.Execute(parameters));
        }

        public static void PostExport(string exportPath)
        {
            if (string.IsNullOrEmpty(exportPath)) {
                Debug.LogError("ExportPath is EMPTY PreExport methods can be skipped");
            }

            if (args == null) {
                Debug.LogError("Error: PostExport skipped because args is NULL");
                return;
            }

            var parameters = CreateCommandParameters();
            var builder    = new UnityPlayerBuilder();

            builder.ExecuteCommands<UnityPostBuildCommand>(x => x.Execute(parameters, null));
        }

        private static IUniBuilderConfiguration CreateCommandParameters()
        {
            var argumentsProvider = new ArgumentsProvider(Environment.GetCommandLineArgs());

            Debug.Log("[CloudBuildHelper] " + argumentsProvider);

            var buildTarget      = argumentsProvider.GetBuildTarget();
            var buildTargetGroup = argumentsProvider.GetBuildTargetGroup();
            var buildParameters = new BuildParameters(buildTarget, buildTargetGroup, argumentsProvider) {
                buildNumber     = args.BuildNumber,
                buildTarget     = buildTarget,
                projectId       = args.ProjectId,
                bundleId        = args.BundleId,
                environmentType = BuildEnvironmentType.UnityCloudBuild,
                branch          = args.ScmBranch,
            };

            var result = new EditorBuildConfiguration(argumentsProvider, buildParameters);
            return result;
        }
    }
}