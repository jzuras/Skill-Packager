---
name: skill-packager
description: Packages Claude Code CLI skills into .skill files for upload to Claude.ai web version via Settings->Capabilities menu. Creates properly structured ZIP archives with skill folder, SKILL.md, and supporting files.
---

# Skill Packager

## Purpose

This skill helps package Claude Code CLI skills for use in Claude.ai web version. It creates properly structured `.skill` files (ZIP archives) that can be uploaded through Settings->Capabilities in the web interface.

## When to Use This Skill

Use this skill when:
- You've created or modified a skill in CLI and want to use it on the web
- You need to share a skill with others who use Claude.ai web version
- You want to validate a skill's structure before packaging
- You need to package multiple skills at once

## Skill Structure Requirements

Both CLI and web versions expect the same structure:

```
skill-name/
├── SKILL.md              (required: YAML frontmatter + instructions)
├── scripts/              (optional: executable code - Python, bash, etc.)
├── references/           (optional: additional documentation)
└── assets/               (optional: templates, images, etc.)
```

### Required YAML Frontmatter

```yaml
---
name: skill-name
description: "Clear description of what this skill does and when to use it"
---
```

Optional fields:
- `license`: License information
- `allowed-tools`: Array of tool names the skill can use

## Packaging Workflow

### Step 1: Validate Skill Structure

Before packaging, verify:
1. **SKILL.md exists** and contains valid YAML frontmatter
2. **Name format**: lowercase letters, numbers, hyphens only (max 64 chars)
3. **Description**: Clear explanation (max 1024 chars)
4. **Supporting files**: Properly organized in subdirectories

### Step 2: Create ZIP with Proper Structure

The ZIP must preserve the skill folder structure:

```
skill-name.skill
└── skill-name/
    ├── SKILL.md
    ├── scripts/
    └── references/
```

**CRITICAL**: The skill folder itself must be in the ZIP, not just its contents.

### Step 3: Rename to .skill Extension

The final package should have `.skill` extension (which is just a ZIP file).

### Step 4: Verify Package

Before uploading, verify:
- ZIP contains skill folder at root
- All relative paths are preserved
- SKILL.md is readable with valid frontmatter

## Tool Differences Between Environments

**Important**: Both CLI and web versions have similar tool access:
- Bash commands work in both
- File operations work in both
- Python scripts can be included in `scripts/` folder

However, document environment-specific considerations in your SKILL.md if needed.

## Packaging Instructions for Claude

**⚠️ CRITICAL REMINDER**: Always check if the output .skill file already exists BEFORE running the packaging script. The script overwrites files automatically. See Step 3 below for mandatory file check procedure.

When a user asks you to package a skill, follow these steps:

### 1. Locate the Skill

```bash
# Personal skill (available everywhere)
~/.claude/skills/skill-name/

# Project skill (shared via git)
.claude/skills/skill-name/
```

### 2. Validate Structure

Read the SKILL.md and verify:
- Valid YAML frontmatter
- Name matches directory name
- Description is clear and concise
- No invalid characters in name

### 3. Check for Existing Package

**CRITICAL - DO NOT SKIP THIS STEP**: Before running the packaging script, you MUST check if the output file already exists.

**Why this matters**: The packaging script automatically overwrites existing files without prompting. You must ask the user first to avoid accidentally destroying their existing package.

```bash
# Check in the skills parent directory (default output location)
# Linux/macOS/WSL:
ls ~/.claude/skills/skill-name.skill 2>/dev/null
# Windows (PowerShell):
dir ~/.claude/skills/skill-name.skill 2>$null

# OR for project skills:
# Linux/macOS/WSL:
ls .claude/skills/skill-name.skill 2>/dev/null
# Windows (PowerShell):
dir .claude/skills/skill-name.skill 2>$null
```

**If the file exists, you MUST:**
1. **STOP** - Do not proceed to packaging yet
2. Use AskUserQuestion tool to prompt: "File {full-path-to-skill-file} already exists. What would you like to do?"
   - Example: "File ~/.claude/skills/simple-greeter.skill already exists. What would you like to do?"
3. Provide these options:
   - "Overwrite existing file" - proceed with default location (file will be replaced)
   - "Use different output directory" - ask for custom path, use output path parameter
4. Wait for user's choice before proceeding

**If the file does not exist**: Proceed directly to Step 4 (packaging)

### 4. Use the Packaging Script

**Default Output Location**: The script creates `skill-name.skill` in the directory where you run the command.

**Recommended Workflow**: Run from the skills parent directory so the `.skill` file is created alongside the skill directories.

**Platform Detection**: Automatically detect the platform and use the appropriate script:
- **Windows**: Use PowerShell script (package_skill.ps1)
- **Linux/macOS**: Use Python script (package_skill.py)

Check your environment `Platform` value (available in <env>) to determine which script to use.

**Default Location Usage**:

```bash
# Detect platform and choose script
# If Platform is "windows":
cd ~/.claude/skills
~/.claude/skills/skill-packager/scripts/package_skill.ps1 skill-name

# If Platform is "linux" or "darwin":
cd ~/.claude/skills
python3 ~/.claude/skills/skill-packager/scripts/package_skill.py skill-name
```

**Output**: Creates `~/.claude/skills/skill-name.skill` (or `.claude/skills/skill-name.skill` for project skills)

**Custom Output Location** (if user chose different directory):

```bash
# Windows (PowerShell)
~/.claude/skills/skill-packager/scripts/package_skill.ps1 skill-name -SourcePath ~/.claude/skills -OutputPath /path/to/output

# Linux/macOS (Python)
python3 ~/.claude/skills/skill-packager/scripts/package_skill.py skill-name ~/.claude/skills /path/to/output
```

**Script Parameters** (identical for both scripts):
- Argument 1 (required): `skill-name` - name of the skill to package
- Argument 2 (optional): `source-path` - directory containing the skill folder (default: current directory)
- Argument 3 (optional): `output-path` - where to save the .skill file (default: current directory)

### 5. Inform User

Tell the user (include the full path to the output file):
```
✓ Skill packaged successfully: {full-path-to-output-file}

Example:
✓ Skill packaged successfully: ~/.claude/skills/simple-greeter.skill

To upload to Claude.ai:
1. Go to Settings -> Capabilities
2. Click "Upload Custom Skill"
3. Select the .skill file
4. The skill will be available in future chats
```

## Examples

**User:** "Package my simple-greeter skill for web use"

**Your Actions:**
1. Validate `~/.claude/skills/simple-greeter/SKILL.md`
2. Check if `~/.claude/skills/simple-greeter.skill` exists
3. If exists, ask user: "File ~/.claude/skills/simple-greeter.skill already exists. What would you like to do?" (Overwrite / Use different directory)
4. Detect platform from <env> and run appropriate script (PS1 for Windows, Python for Linux/macOS)
5. Inform user with full path: "✓ Skill packaged successfully: ~/.claude/skills/simple-greeter.skill"
6. Provide upload instructions

**User:** "I want to create a skill here and use it on the web"

**Your Actions:**
1. Help create the skill in CLI
2. Test it locally first
3. Package it using this skill (follow steps 1-6 above)
4. Provide the .skill file location (full path) for download/upload

## Best Practices

1. **Test in CLI first**: Always test skills in CLI before packaging for web
2. **Keep SKILL.md concise**: Move detailed docs to `references/` folder
3. **Document tool usage**: If using specific tools, note compatibility
4. **Version your skills**: Consider adding version info in description
5. **Validate before packaging**: Catch errors early

## Troubleshooting

**Problem:** ZIP doesn't contain skill folder at root
- **Solution:** Ensure packaging creates `skill-name/` inside ZIP, not just contents

**Problem:** Web version can't find SKILL.md
- **Solution:** Verify SKILL.md is directly inside `skill-name/` folder in ZIP

**Problem:** Frontmatter not recognized
- **Solution:** Check YAML syntax, ensure proper `---` delimiters

**Problem:** Skill not appearing in web version
- **Solution:** Verify name in frontmatter matches folder name exactly

## Notes

- The `.skill` extension is just a renamed `.zip` file
- All paths in the ZIP maintain the skill folder structure
- Skills can be large if they include many supporting files
- Web upload has size limits (verify current limits in Settings->Capabilities)
- **Output file location**: By default, the `.skill` file is created in the directory where you run the script
- **Filename format**: Always `{skill-name}.skill` (exact match to skill directory name)
- **Cross-platform support**: Automatically uses PowerShell on Windows, Python on Linux/macOS
- **Platform detection**: Check the `Platform` value in your <env> to determine which script to use
