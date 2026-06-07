// using UnityEditor;
// using UnityEditor.Build;
// using UnityEditor.Build.Reporting;
// using UnityEngine;
// using System;

// public class AutoSetAppVersion : IPreprocessBuildWithReport
// {
//     public int callbackOrder => 0;

//     public AppVersionStatus status = AppVersionStatus.INDEV;

//     private string GetVersionString()
//     {
//         return $"{status.ToString()} {FormatVersionFromUtc(DateTime.UtcNow)}";
//     }

//     public void OnPreprocessBuild(BuildReport report)
//     {
//         string version = GetVersionString();

//         PlayerSettings.bundleVersion = version;

//         Debug.Log($"[AutoSetAppVersion] Set bundleVersion={version}");
//     }

//     string FormatVersionFromUtc(DateTime utc)
//     {
//         return string.Format("{0:D2}{1:D2}{2:D2}{3:D2}{4:D2}",
//             utc.Year % 100,
//             utc.Day,
//             utc.Month,
//             utc.Hour,
//             utc.Minute);
//     }
// }

// public enum AppVersionStatus
// {
//     INDEV,
//     PLAYTEST,
//     ALPHA,
//     BETA
// }