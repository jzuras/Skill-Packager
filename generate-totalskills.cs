#!/usr/bin/dotnet run
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run generate-totalskills.cs <skills-directory> [output-path]");
    Console.WriteLine("  skills-directory: Path to the directory containing skill folders");
    Console.WriteLine("  output-path: Optional output path (file or directory). Defaults to ./TotalSkills.md");
    return 1;
}

string skillsDir = args[0];
string outputPath = args.Length > 1 ? args[1] : "TotalSkills.md";

// Validate skills directory
if (!Directory.Exists(skillsDir))
{
    Console.WriteLine($"Error: Skills directory not found: {skillsDir}");
    return 1;
}

// Determine output file path
string outputFile = outputPath;
if (Directory.Exists(outputPath))
{
    outputFile = Path.Combine(outputPath, "TotalSkills.md");
}
else if (!outputPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
{
    // If no extension, assume it's meant to be a directory
    Directory.CreateDirectory(outputPath);
    outputFile = Path.Combine(outputPath, "TotalSkills.md");
}

Console.WriteLine($"Scanning skills in: {skillsDir}");
Console.WriteLine($"Output file: {outputFile}");
Console.WriteLine();

// Get all skill directories
var skillDirs = Directory.GetDirectories(skillsDir);
var skills = new List<(string name, string description, string allowedTools, string path)>();

foreach (var dir in skillDirs)
{
    string skillMdPath = Path.Combine(dir, "SKILL.md");

    if (!File.Exists(skillMdPath))
    {
        Console.WriteLine($"Warning: Skipping '{Path.GetFileName(dir)}' - no SKILL.md found");
        continue;
    }

    try
    {
        var (name, description, allowedTools) = ParseSkillFrontmatter(skillMdPath);

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine($"Warning: Skipping '{Path.GetFileName(dir)}' - invalid frontmatter (missing name or description)");
            continue;
        }

        // Format path for cross-platform compatibility
        string formattedPath = FormatSkillPath(skillsDir, skillMdPath);

        skills.Add((name, description, allowedTools, formattedPath));
        Console.WriteLine($"✓ Found skill: {name}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Skipping '{Path.GetFileName(dir)}' - error parsing: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine($"Total skills found: {skills.Count}");

// Generate the markdown file
var markdown = GenerateMarkdown(skills);
File.WriteAllText(outputFile, markdown);

Console.WriteLine($"✓ Generated: {outputFile}");
return 0;

// Helper function to parse frontmatter
(string name, string description, string allowedTools) ParseSkillFrontmatter(string filePath)
{
    var lines = File.ReadAllLines(filePath);
    bool inFrontmatter = false;
    string name = "";
    string description = "";
    string allowedTools = "";

    foreach (var line in lines)
    {
        if (line.Trim() == "---")
        {
            if (!inFrontmatter)
            {
                inFrontmatter = true;
                continue;
            }
            else
            {
                // End of frontmatter
                break;
            }
        }

        if (inFrontmatter)
        {
            if (line.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
            {
                name = line.Substring(5).Trim();
            }
            else if (line.StartsWith("description:", StringComparison.OrdinalIgnoreCase))
            {
                description = line.Substring(12).Trim();
            }
            else if (line.StartsWith("allowed-tools:", StringComparison.OrdinalIgnoreCase))
            {
                allowedTools = line.Substring(14).Trim();
            }
        }
    }

    return (name, description, allowedTools);
}

// Helper function to format skill path for cross-platform compatibility
string FormatSkillPath(string skillsDir, string skillMdPath)
{
    // Normalize to forward slashes for portability
    string normalizedPath = skillMdPath.Replace('\\', '/');

    // Try to replace home directory with tilde
    string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace('\\', '/');

    if (!string.IsNullOrEmpty(homeDir) && normalizedPath.StartsWith(homeDir, StringComparison.OrdinalIgnoreCase))
    {
        normalizedPath = "~" + normalizedPath.Substring(homeDir.Length);
    }

    return normalizedPath;
}

// Helper function to generate the markdown content
string GenerateMarkdown(List<(string name, string description, string allowedTools, string path)> skills)
{
    var sb = new StringBuilder();

    sb.AppendLine("# Skills");
    sb.AppendLine();
    sb.AppendLine("Skills are modular, discoverable capabilities that extend agent functionality. Unlike slash commands (user-invoked), **skills are agent-invoked—the agent autonomously decides when to use them based on your request** and the skill's description.");
    sb.AppendLine();
    sb.AppendLine("## How to Use Skills");
    sb.AppendLine();
    sb.AppendLine("When a user's request matches a skill's description:");
    sb.AppendLine("1. Read the full skill file from the path provided");
    sb.AppendLine("2. Follow the instructions in that skill file");
    sb.AppendLine("3. Execute the skill's workflow autonomously");
    sb.AppendLine();
    sb.AppendLine("## Available Skills");
    sb.AppendLine();

    foreach (var skill in skills)
    {
        string toolsInfo = FormatAllowedTools(skill.allowedTools);
        string entry = $"- **{skill.name}**: {skill.description}";

        if (!string.IsNullOrWhiteSpace(toolsInfo))
        {
            entry += $" {toolsInfo}";
        }

        entry += $" (Full skill: `{skill.path}`)";

        sb.AppendLine(entry);
        sb.AppendLine();
    }

    return sb.ToString();
}

// Helper function to format allowed-tools for display
string FormatAllowedTools(string allowedTools)
{
    if (string.IsNullOrWhiteSpace(allowedTools))
    {
        return "";
    }

    // Handle empty array: []
    if (allowedTools.Trim() == "[]")
    {
        return "[Tools: none]";
    }

    // Handle bracket format: [Read, Bash, Write]
    if (allowedTools.StartsWith("[") && allowedTools.EndsWith("]"))
    {
        string content = allowedTools.Trim('[', ']').Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return "[Tools: none]";
        }
        return $"[Tools: {content}]";
    }

    // If it's just a plain value, return as-is
    return $"[Tools: {allowedTools}]";
}
