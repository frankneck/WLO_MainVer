using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class RspCleaner
{
[MenuItem("Tools/Clean RSP Files")]
    public static void CleanEmptyDefines()
    {
        // Get the project root path
        string projectPath = Directory.GetCurrentDirectory();

        // Find all .rsp files in the project
        var rspFiles = Directory.GetFiles(projectPath, "*.rsp", SearchOption.AllDirectories);
        int fixedCount = 0;

        foreach (var file in rspFiles)
        {
            var lines = File.ReadAllLines(file);
            // Filter out lines that are exactly "-define:" with nothing else
            var cleanedLines = lines.Where(line => !line.Trim().Equals("-define:")).ToArray();

            // Only write back if changes were made
            if (lines.Length != cleanedLines.Length)
            {
                File.WriteAllLines(file, cleanedLines);
                fixedCount++;
                Debug.Log($"[RspCleaner] Fixed file: {file}");
            }
        }

        Debug.Log($"<b>Done!</b> Fixed {fixedCount} .rsp files.");
        AssetDatabase.Refresh();
    }
}