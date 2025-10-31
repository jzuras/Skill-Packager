# Skill Packager

Package Claude Code CLI skills for use in Claude.ai web version.

## Overview

**skill-packager** is a Claude Code skill that enables you to create skills in the CLI and then package them as `.skill` files for upload to Claude.ai web version via Settings → Capabilities.

This bridges the gap between the two environments, allowing you to:
- Develop skills with CLI's superior file operations and testing capabilities
- Share skills with web-only users
- Maintain a single skill codebase that works in both environments

## Features

- ✅ **Validates skill structure** - Checks YAML frontmatter and directory structure
- ✅ **Cross-platform support** - PowerShell script for Windows, Python script for Linux/macOS/WSL
- ✅ **Proper ZIP packaging** - Creates correctly structured `.skill` files
- ✅ **File overwrite protection** - Prompts before overwriting existing packages
- ✅ **Detailed error messages** - Clear feedback when validation fails
- ✅ **Compatible with both old and modern PowerShell** - Works with `powershell.exe` and `pwsh`

> **Note:** This repository also includes `generate-totalskills.cs`, a utility script for generating skills manifests for non-Claude Code agents. See [Skills Manifest Generator](#skills-manifest-generator) for details.

## Installation

### For Claude Code CLI Users

1. **Clone or download** this repository to your Claude skills directory:

```bash
# Personal skills (available in all projects)
cd ~/.claude/skills
git clone https://github.com/jzuras/skill-packager.git

# OR for project-specific installation
cd your-project/.claude/skills
git clone https://github.com/jzuras/skill-packager.git
```

2. **Verify installation**:

```bash
ls ~/.claude/skills/skill-packager/
# Should show: SKILL.md, scripts/, README.md
```

3. **That's it!** Claude Code will automatically recognize the skill.

### Requirements

**Windows:**
- PowerShell 5.1+ or PowerShell Core 7+
- No additional dependencies

**Linux/macOS/WSL:**
- Python 3.6+
- No additional packages required (uses standard library only)

## Usage

### In Claude Code CLI

Simply ask Claude to package a skill:

```
"Package my glyph-list skill for web use"
```

Claude will:
1. Validate the skill structure
2. Check if the output file already exists (and prompt if it does)
3. Run the appropriate packaging script for your platform
4. Create a `.skill` file ready for upload

**Default Output Location:** The `.skill` file is created in the directory where the command runs. When Claude packages skills, it typically runs from `~/.claude/skills/` (or `.claude/skills/` for project skills), placing the output file alongside your skill directories.

### Manual Usage (Advanced)

You can also run the packaging scripts directly:

#### Windows (PowerShell)

```powershell
cd ~/.claude/skills
pwsh ./skill-packager/scripts/package_skill.ps1 skill-name

# With custom paths
pwsh ./skill-packager/scripts/package_skill.ps1 my-skill -SourcePath ~/.claude/skills -OutputPath ./output
```

#### Linux/macOS/WSL (Python)

```bash
cd ~/.claude/skills
python3 skill-packager/scripts/package_skill.py skill-name

# With custom paths
python3 skill-packager/scripts/package_skill.py my-skill ~/.claude/skills ./output
```

### Upload to Claude.ai Web

After packaging:

1. Go to **Settings → Capabilities** in Claude.ai
2. Click **"Upload Custom Skill"**
3. Select your `.skill` file
4. The skill will be available in future web chats

## Skill Structure Requirements

Skills must follow this structure to be packaged:

```
skill-name/
├── SKILL.md              (required: YAML frontmatter + instructions)
├── scripts/              (optional: executable code)
├── references/           (optional: additional documentation)
└── assets/               (optional: templates, images, etc.)
```

### SKILL.md Frontmatter

```yaml
---
name: skill-name
description: "Clear description of what this skill does and when to use it"
---
```

**Requirements:**
- `name`: lowercase letters, numbers, hyphens only (max 64 chars)
- `description`: clear explanation (max 1024 chars)

Optional fields:
- `license`: License information
- `allowed-tools`: Array of tool names the skill can use

## Examples

### Example 1: Basic Packaging

```
User: "Package the glyph-list skill"

Claude:
[+] Packaging skill: glyph-list
[v] Skill structure validated: glyph-list
[+] Creating package: ~/.claude/skills/glyph-list.skill
  + glyph-list/SKILL.md
[v] Skill packaged successfully!
  File: ~/.claude/skills/glyph-list.skill
  Size: 0.6 KB
```

### Example 2: Handling Existing File

```
User: "Package glyph-list for web"

Claude: "I found that ~/.claude/skills/glyph-list.skill already exists.
What would you like to do?"

Options:
- Overwrite existing file
- Use different output directory

[User selects option, then Claude proceeds with packaging]
```

### Example 3: Creating and Packaging

```
User: "Create a skill that greets users, then package it for web"

Claude:
1. Creates the skill in ~/.claude/skills/my-greeter/
2. Writes SKILL.md with proper frontmatter
3. Tests it locally
4. Packages it to my-greeter.skill
5. Provides upload instructions
```

## Platform Compatibility

### Tool Availability

Both CLI and web versions have similar tool access:
- ✅ Bash commands
- ✅ File operations
- ✅ Python/script execution

Document any environment-specific considerations in your skill's SKILL.md.

### PowerShell Compatibility

The PowerShell script uses ASCII bracket notation (`[+]`, `[x]`, `[v]`) instead of emojis to ensure compatibility with:
- Modern PowerShell Core (`pwsh`)
- Legacy Windows PowerShell 5.1 (`powershell.exe`)

## Troubleshooting

### "Skill directory not found"

**Problem:** Script can't locate the skill folder.

**Solution:**
- Verify the skill name matches the directory name exactly
- Check you're running from the correct parent directory
- Use absolute paths with `-SourcePath` parameter

### "SKILL.md must start with YAML frontmatter"

**Problem:** SKILL.md doesn't have proper frontmatter delimiters.

**Solution:** Ensure SKILL.md starts with:
```yaml
---
name: skill-name
description: "Description here"
---
```

### "ZIP doesn't contain skill folder at root"

**Problem:** The packaged file has the wrong structure.

**Solution:** This shouldn't happen with the provided scripts. If it does, please file an issue with:
- Your platform (Windows/Linux/macOS)
- The command you ran
- The actual ZIP structure (`unzip -l skill-name.skill`)

### "Web version can't find SKILL.md"

**Problem:** After upload, Claude.ai doesn't recognize the skill.

**Solution:**
- Verify the skill name in frontmatter matches the folder name
- Check the ZIP structure contains `skill-name/SKILL.md`
- Re-package using the provided scripts

## Development Workflow

**Recommended workflow for skill development:**

1. **Create in CLI** - Use Claude Code's superior tooling
2. **Test in CLI** - Validate behavior with actual file operations
3. **Package** - Use this skill to create `.skill` file
4. **Upload to web** - Share via Settings → Capabilities
5. **Test in web** - Verify it works in web environment
6. **Iterate** - Make changes in CLI, re-package, re-upload

## Skills Manifest Generator

This repository includes `generate-totalskills.cs`, a .NET 10 script that generates a lightweight skills manifest file for use with AI agents that don't have native Claude Code skills support (like Gemini CLI, Codex CLI, etc.).

### Purpose

The script creates a `TotalSkills.md` file that mirrors how Claude Code discovers skills:
- Lists all available skills with their names and descriptions from YAML frontmatter
- Includes optional `allowed-tools` restrictions if defined
- Provides file paths to full skill instructions
- Uses minimal context (like Claude Code's startup behavior) - agents only load full skills when needed

This enables other AI agents to use Claude Code skills by:
1. Reading the lightweight TotalSkills.md manifest to discover available skills
2. Loading the full SKILL.md file only when a user request matches a skill's description
3. Following the skill's instructions autonomously

### Usage

```bash
# Basic usage (output to current directory)
dotnet run generate-totalskills.cs ~/.claude/skills

# Specify output file
dotnet run generate-totalskills.cs ~/.claude/skills /path/to/TotalSkills.md

# Specify output directory (creates TotalSkills.md inside)
dotnet run generate-totalskills.cs ~/.claude/skills /path/to/output/
```

**Parameters:**
- `skills-directory` (required): Path to directory containing skill folders
- `output-path` (optional): Output file or directory path (defaults to `./TotalSkills.md`)

**Requirements:**
- .NET 10 RC (uses `dotnet run app.cs` feature)
- Cross-platform: Works on Windows and WSL/Linux

### Example Output

```
Scanning skills in: C:\Users\Jim\.claude\skills
Output file: TotalSkills.md

✓ Found skill: dad-joke-rhymer
✓ Found skill: endpoint-add
✓ Found skill: endpoint-edit
✓ Found skill: endpoint-list
✓ Found skill: file-analyzer-poc
✓ Found skill: glyph-add
✓ Found skill: glyph-list
✓ Found skill: simple-greeter
✓ Found skill: skill-packager

Total skills found: 9
✓ Generated: TotalSkills.md
```

### Real-World Usage: Gemini CLI

Here's an example of Gemini CLI successfully using the generated TotalSkills.md to discover and execute the `glyph-list` skill:

```
> read the totalskills md file in the current dir

╭────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
│ ✓  ReadFile TotalSkills.md                                                                                 │
╰────────────────────────────────────────────────────────────────────────────────────────────────────────────╯

> list my glyphs

╭────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
│ ✓  ReadFile ..\glyph-list\SKILL.md                                                                         │
╰────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ Mystical Glyphs (3 total):

   1. Glyph of Aether (power: 42)
   2. Glyph of Zephyr (power: 31)
   3. Glyph of Ember (power: 55)

  What would you like to do?
   - Add a new glyph
   - Edit an existing glyph
   - Delete a glyph
   - Inspect glyph details
```

**What happened:**
1. Gemini CLI read TotalSkills.md and learned about available skills
2. User request "list my glyphs" matched the `glyph-list` skill description
3. Gemini automatically loaded the full `SKILL.md` from the referenced path
4. Executed the skill's instructions successfully

### Integration with Agent Configurations

Instead of manually telling the agent to read TotalSkills.md each time, you can automate this by updating agent configuration files:

**For Gemini CLI** (or similar tools with startup configuration):
- Add to `Agent.md` or equivalent startup file
- Include instructions to read TotalSkills.md on initialization
- This gives the agent automatic awareness of all available skills

**Example Agent.md snippet:**
```markdown
On startup, read the TotalSkills.md file in the skills directory to discover available skills.
When a user request matches a skill description, load and execute that skill's instructions.
```

This approach provides skill discovery without native skills support, using minimal context tokens.

## Copyright and License

### Code

Copyright (©) 2025 Jzuras

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

## Trademarks

All trademarks are the property of their respective owners.
Any trademarks used in this project are used in a purely descriptive manner and to state compatibility.

## Related Resources

- [Claude Code Documentation](https://docs.claude.com/en/docs/claude-code/overview)
- [Claude.ai](https://claude.ai)
- [Creating Custom Skills Guide](https://docs.claude.com/en/docs/claude-code/skills.md)

---

**Made with Claude Code** 🤖
