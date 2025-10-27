#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packages Claude Code CLI skills into .skill files for Claude.ai web upload.

.DESCRIPTION
    Creates a properly structured ZIP archive with .skill extension that can be
    uploaded to Claude.ai via Settings->Capabilities menu.

.PARAMETER SkillName
    Name of the skill to package (must match directory name)

.PARAMETER SourcePath
    Path containing the skill directory (defaults to current directory)

.PARAMETER OutputPath
    Where to save the .skill file (defaults to current directory)

.EXAMPLE
    .\package_skill.ps1 simple-greeter

.EXAMPLE
    .\package_skill.ps1 my-skill -SourcePath C:\Users\Jim\.claude\skills -OutputPath .
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$SkillName,

    [Parameter(Mandatory=$false)]
    [string]$SourcePath = $PWD,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = $PWD
)

function Test-SkillName {
    param([string]$Name)

    if ($Name -notmatch '^[a-z0-9-]+$') {
        return @{Valid=$false; Error="Skill name must contain only lowercase letters, numbers, and hyphens"}
    }
    if ($Name.Length -gt 64) {
        return @{Valid=$false; Error="Skill name must be 64 characters or less"}
    }
    return @{Valid=$true; Error=$null}
}

function Test-Frontmatter {
    param([string]$SkillMdPath)

    try {
        $content = Get-Content -Path $SkillMdPath -Raw -Encoding UTF8

        if (-not $content.StartsWith('---')) {
            return @{Valid=$false; Error="SKILL.md must start with YAML frontmatter (---)"}
        }

        $lines = $content -split "`n"
        $frontmatterEnd = -1

        for ($i = 1; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Trim() -eq '---') {
                $frontmatterEnd = $i
                break
            }
        }

        if ($frontmatterEnd -eq -1) {
            return @{Valid=$false; Error="SKILL.md frontmatter must end with ---"}
        }

        $frontmatter = $lines[1..($frontmatterEnd-1)] -join "`n"

        if ($frontmatter -notmatch 'name:') {
            return @{Valid=$false; Error="SKILL.md frontmatter must include 'name:' field"}
        }
        if ($frontmatter -notmatch 'description:') {
            return @{Valid=$false; Error="SKILL.md frontmatter must include 'description:' field"}
        }

        return @{Valid=$true; Error=$null}

    } catch {
        return @{Valid=$false; Error="Error reading SKILL.md: $_"}
    }
}

# Main script
Write-Host "[+] Packaging skill: $SkillName`n" -ForegroundColor Cyan

# Validate skill name
$nameCheck = Test-SkillName -Name $SkillName
if (-not $nameCheck.Valid) {
    Write-Host "[x] Error: $($nameCheck.Error)" -ForegroundColor Red
    exit 1
}

# Determine source path
$skillDir = Join-Path $SourcePath $SkillName
if (-not (Test-Path $skillDir -PathType Container)) {
    Write-Host "[x] Error: Skill directory not found: $skillDir" -ForegroundColor Red
    exit 1
}

# Check for SKILL.md
$skillMd = Join-Path $skillDir "SKILL.md"
if (-not (Test-Path $skillMd -PathType Leaf)) {
    Write-Host "[x] Error: SKILL.md not found in $skillDir" -ForegroundColor Red
    exit 1
}

# Validate frontmatter
$frontmatterCheck = Test-Frontmatter -SkillMdPath $skillMd
if (-not $frontmatterCheck.Valid) {
    Write-Host "[x] Error: $($frontmatterCheck.Error)" -ForegroundColor Red
    exit 1
}

Write-Host "[v] Skill structure validated: $SkillName" -ForegroundColor Green

# Create output file path
$outputFile = Join-Path $OutputPath "$SkillName.skill"

Write-Host "[+] Creating package: $outputFile" -ForegroundColor Cyan

try {
    # Remove existing file if present
    if (Test-Path $outputFile) {
        Remove-Item $outputFile -Force
    }

    # Create ZIP using native PowerShell compression
    # Get parent directory to preserve skill folder in archive
    $parentDir = Split-Path $skillDir -Parent

    # Get all files to include
    $files = Get-ChildItem -Path $skillDir -Recurse -File

    # Create temporary zip first
    $tempZip = [System.IO.Path]::GetTempFileName()
    Remove-Item $tempZip -Force  # Remove the temp file so we can create a fresh zip

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::Open($tempZip, 'Create')

    foreach ($file in $files) {
        # Calculate relative path from parent directory (to include skill folder name)
        $relativePath = $file.FullName.Substring($parentDir.Length + 1)
        # Convert backslashes to forward slashes for ZIP standard
        $relativePath = $relativePath -replace '\\', '/'

        Write-Host "  + $relativePath" -ForegroundColor Gray

        # Add file to ZIP
        $entry = $zip.CreateEntry($relativePath)
        $entryStream = $entry.Open()
        $fileStream = [System.IO.File]::OpenRead($file.FullName)
        $fileStream.CopyTo($entryStream)
        $fileStream.Close()
        $entryStream.Close()
    }

    $zip.Dispose()

    # Move to final location with .skill extension
    Move-Item -Path $tempZip -Destination $outputFile -Force

    # Get file size
    $sizeKB = [math]::Round((Get-Item $outputFile).Length / 1KB, 1)

    Write-Host "`n[v] Skill packaged successfully!" -ForegroundColor Green
    Write-Host "  File: $outputFile" -ForegroundColor White
    Write-Host "  Size: $sizeKB KB" -ForegroundColor White
    Write-Host "`n[+] To upload to Claude.ai:" -ForegroundColor Cyan
    Write-Host "  1. Go to Settings -> Capabilities" -ForegroundColor White
    Write-Host "  2. Click 'Upload Custom Skill'" -ForegroundColor White
    Write-Host "  3. Select $SkillName.skill" -ForegroundColor White
    Write-Host "  4. The skill will be available in future chats" -ForegroundColor White

    exit 0

} catch {
    Write-Host "`n[x] Error creating package: $_" -ForegroundColor Red
    exit 1
}