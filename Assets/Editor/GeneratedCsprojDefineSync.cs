using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

public sealed class GeneratedCsprojDefineSync : AssetPostprocessor
{
    static readonly Regex DefineConstantsRegex = new Regex(
        @"<DefineConstants>([^<]*)</DefineConstants>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    static string OnGeneratedCSProject(string path, string content)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (string.IsNullOrWhiteSpace(scriptingDefines))
            return content;

        string normalizedDefines = scriptingDefines.Replace(',', ';');
        string updated = DefineConstantsRegex.Replace(content, match =>
        {
            string existing = match.Groups[1].Value;
            string merged = MergeDefineConstants(existing, normalizedDefines);
            return $"<DefineConstants>{merged}</DefineConstants>";
        });

        return updated;
    }

    static string MergeDefineConstants(string existingConstants, string additionalConstants)
    {
        var merged = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        AddConstants(existingConstants, merged, seen);
        AddConstants(additionalConstants, merged, seen);

        return string.Join(";", merged);
    }

    static void AddConstants(string constants, List<string> merged, HashSet<string> seen)
    {
        foreach (string constant in constants.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = constant.Trim();
            if (trimmed.Length == 0)
                continue;

            if (seen.Add(trimmed))
                merged.Add(trimmed);
        }
    }
}