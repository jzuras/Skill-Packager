#!/usr/bin/env python3
"""
Skill Packager Script
Packages Claude Code CLI skills into .skill files for Claude.ai web upload.
"""

import os
import sys
import zipfile
import re
from pathlib import Path


def validate_skill_name(name):
    """Validate skill name format: lowercase letters, numbers, hyphens only."""
    if not re.match(r'^[a-z0-9-]+$', name):
        return False, "Skill name must contain only lowercase letters, numbers, and hyphens"
    if len(name) > 64:
        return False, "Skill name must be 64 characters or less"
    return True, None


def validate_frontmatter(skill_md_path):
    """Validate SKILL.md has proper YAML frontmatter."""
    try:
        with open(skill_md_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # Check for YAML frontmatter delimiters
        if not content.startswith('---'):
            return False, "SKILL.md must start with YAML frontmatter (---)"

        # Find the closing delimiter
        lines = content.split('\n')
        frontmatter_end = -1
        for i, line in enumerate(lines[1:], start=1):
            if line.strip() == '---':
                frontmatter_end = i
                break

        if frontmatter_end == -1:
            return False, "SKILL.md frontmatter must end with ---"

        # Extract frontmatter
        frontmatter = '\n'.join(lines[1:frontmatter_end])

        # Check for required fields
        if 'name:' not in frontmatter:
            return False, "SKILL.md frontmatter must include 'name:' field"
        if 'description:' not in frontmatter:
            return False, "SKILL.md frontmatter must include 'description:' field"

        return True, None

    except Exception as e:
        return False, f"Error reading SKILL.md: {str(e)}"


def package_skill(skill_name, source_path=None, output_path=None):
    """
    Package a skill into a .skill file.

    Args:
        skill_name: Name of the skill to package
        source_path: Path to skill directory (default: current directory)
        output_path: Where to save the .skill file (default: current directory)
    """
    # Validate skill name
    valid, error = validate_skill_name(skill_name)
    if not valid:
        print(f"❌ Error: {error}")
        return False

    # Determine source path
    if source_path is None:
        source_path = Path.cwd() / skill_name
    else:
        source_path = Path(source_path) / skill_name

    if not source_path.exists():
        print(f"❌ Error: Skill directory not found: {source_path}")
        return False

    # Check for SKILL.md
    skill_md = source_path / "SKILL.md"
    if not skill_md.exists():
        print(f"❌ Error: SKILL.md not found in {source_path}")
        return False

    # Validate frontmatter
    valid, error = validate_frontmatter(skill_md)
    if not valid:
        print(f"❌ Error: {error}")
        return False

    print(f"✓ Skill structure validated: {skill_name}")

    # Determine output path
    if output_path is None:
        output_path = Path.cwd()
    else:
        output_path = Path(output_path)

    output_file = output_path / f"{skill_name}.skill"

    # Create ZIP file with proper structure
    print(f"📦 Creating package: {output_file}")

    try:
        with zipfile.ZipFile(output_file, 'w', zipfile.ZIP_DEFLATED) as zipf:
            # Walk through all files in the skill directory
            for root, dirs, files in os.walk(source_path):
                for file in files:
                    file_path = Path(root) / file
                    # Calculate the archive name (preserve skill folder structure)
                    arcname = file_path.relative_to(source_path.parent)
                    zipf.write(file_path, arcname)
                    print(f"  + {arcname}")

        # Get file size
        size_kb = output_file.stat().st_size / 1024

        print(f"\n✓ Skill packaged successfully!")
        print(f"  File: {output_file}")
        print(f"  Size: {size_kb:.1f} KB")
        print(f"\n📤 To upload to Claude.ai:")
        print(f"  1. Go to Settings -> Capabilities")
        print(f"  2. Click 'Upload Custom Skill'")
        print(f"  3. Select {output_file.name}")
        print(f"  4. The skill will be available in future chats")

        return True

    except Exception as e:
        print(f"❌ Error creating package: {str(e)}")
        return False


def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python package_skill.py <skill-name> [source-path] [output-path]")
        print("\nExamples:")
        print("  python package_skill.py simple-greeter")
        print("  python package_skill.py my-skill /path/to/skills .")
        sys.exit(1)

    skill_name = sys.argv[1]
    source_path = sys.argv[2] if len(sys.argv) > 2 else None
    output_path = sys.argv[3] if len(sys.argv) > 3 else None

    print(f"🔧 Packaging skill: {skill_name}\n")

    success = package_skill(skill_name, source_path, output_path)
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
