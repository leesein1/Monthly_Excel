param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$Owner,

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

function Get-Sha256 {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -Algorithm SHA256 -Path $Path).Hash.ToUpperInvariant()
}

function Normalize-RelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)
    return $Path.Replace('\', '/').TrimStart('/')
}

function Get-RelativePathCompat {
    param(
        [Parameter(Mandatory = $true)][string]$BasePath,
        [Parameter(Mandatory = $true)][string]$TargetPath
    )

    $baseUri = [System.Uri]((Resolve-Path $BasePath).Path.TrimEnd('\') + '\')
    $targetUri = [System.Uri](Resolve-Path $TargetPath).Path
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

function Get-AssetName {
    param(
        [Parameter(Mandatory = $true)][string]$Prefix,
        [Parameter(Mandatory = $true)][string]$RelativePath
    )

    $normalized = Normalize-RelativePath $RelativePath
    return ($Prefix + '__' + ($normalized -replace '/', '__'))
}

function Copy-Tree {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$DestinationRoot
    )

    Get-ChildItem -Path $SourceRoot -Recurse -File | ForEach-Object {
        $relative = [System.IO.Path]::GetRelativePath($SourceRoot, $_.FullName)
        $destination = Join-Path $DestinationRoot $relative
        $directory = Split-Path -Parent $destination
        if (-not [string]::IsNullOrWhiteSpace($directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }

        Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tag = if ($Version.StartsWith('v')) { $Version } else { "v$Version" }
$safeVersion = $tag.TrimStart('v')

$mainProject = Join-Path $repoRoot 'Monthly_Excel\Monthly_Excel.csproj'
$launcherProject = Join-Path $repoRoot 'Monthly_Excel.Launcher\Monthly_Excel.Launcher.csproj'
$mainOutput = Join-Path $repoRoot 'Monthly_Excel\bin\Release\net8.0-windows'
$launcherOutput = Join-Path $repoRoot 'Monthly_Excel.Launcher\bin\Release\net8.0-windows'

if (-not $SkipBuild) {
    Write-Host "Building Monthly_Excel Release..."
    dotnet build $mainProject -c Release | Out-Host

    Write-Host "Building Monthly_Excel.Launcher Release..."
    dotnet build $launcherProject -c Release | Out-Host
}

if (-not (Test-Path $mainOutput)) {
    throw "Main release output not found: $mainOutput"
}

if (-not (Test-Path $launcherOutput)) {
    throw "Launcher release output not found: $launcherOutput"
}

$artifactRoot = Join-Path $repoRoot ".artifacts\releases\$tag"
$uploadRoot = Join-Path $artifactRoot 'upload'
$portableRoot = Join-Path $artifactRoot 'portable'
$portableZipPath = Join-Path $uploadRoot "Monthly_Excel_portable_$safeVersion.zip"

if (Test-Path $artifactRoot) {
    Remove-Item -LiteralPath $artifactRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $uploadRoot -Force | Out-Null
New-Item -ItemType Directory -Path $portableRoot -Force | Out-Null

$excludeExtensions = @('.pdb', '.xml')

$manifestEntries = New-Object System.Collections.Generic.List[object]

Write-Host 'Collecting main application files...'
Get-ChildItem -Path $mainOutput -Recurse -File |
    Where-Object { $excludeExtensions -notcontains $_.Extension.ToLowerInvariant() } |
    ForEach-Object {
        $relativePath = Normalize-RelativePath (Get-RelativePathCompat -BasePath $mainOutput -TargetPath $_.FullName)
        $assetName = Get-AssetName -Prefix 'app' -RelativePath $relativePath
        $uploadPath = Join-Path $uploadRoot $assetName
        Copy-Item -LiteralPath $_.FullName -Destination $uploadPath -Force

        $portablePath = Join-Path $portableRoot $relativePath
        $portableDirectory = Split-Path -Parent $portablePath
        if (-not [string]::IsNullOrWhiteSpace($portableDirectory)) {
            New-Item -ItemType Directory -Path $portableDirectory -Force | Out-Null
        }

        Copy-Item -LiteralPath $_.FullName -Destination $portablePath -Force

        $manifestEntries.Add([pscustomobject]@{
            path        = $relativePath
            sha256      = Get-Sha256 -Path $_.FullName
            downloadUrl = "https://github.com/$Owner/$Repository/releases/download/$tag/$assetName"
            size        = $_.Length
        })
    }

Write-Host 'Collecting launcher files...'
Get-ChildItem -Path $launcherOutput -Recurse -File |
    Where-Object { $excludeExtensions -notcontains $_.Extension.ToLowerInvariant() } |
    ForEach-Object {
        $relativePath = Normalize-RelativePath (Get-RelativePathCompat -BasePath $launcherOutput -TargetPath $_.FullName)
        $assetName = Get-AssetName -Prefix 'launcher' -RelativePath $relativePath
        $uploadPath = Join-Path $uploadRoot $assetName
        Copy-Item -LiteralPath $_.FullName -Destination $uploadPath -Force

        $portablePath = Join-Path $portableRoot $relativePath
        $portableDirectory = Split-Path -Parent $portablePath
        if (-not [string]::IsNullOrWhiteSpace($portableDirectory)) {
            New-Item -ItemType Directory -Path $portableDirectory -Force | Out-Null
        }

        Copy-Item -LiteralPath $_.FullName -Destination $portablePath -Force
    }

$manifest = [pscustomobject]@{
    version = $safeVersion
    files   = $manifestEntries | Sort-Object path
}

$manifestPath = Join-Path $uploadRoot 'manifest.json'
$manifest | ConvertTo-Json -Depth 6 | Set-Content -Path $manifestPath -Encoding UTF8

if (Test-Path $portableZipPath) {
    Remove-Item -LiteralPath $portableZipPath -Force
}

Compress-Archive -Path (Join-Path $portableRoot '*') -DestinationPath $portableZipPath -CompressionLevel Optimal

$instructionPath = Join-Path $artifactRoot 'README_UPLOAD.txt'
@"
Release tag: $tag

Upload every file inside:
$uploadRoot

What is included:
- manifest.json
- app__* assets for partial update
- launcher__* assets for initial distribution
- Monthly_Excel_portable_$safeVersion.zip for first-time install

After publishing the release:
1. Confirm manifest.json was uploaded to the release assets
2. LauncherConfiguration.cs should keep using releases/latest/download/manifest.json
3. No launcher rebuild is required unless the launcher code itself changed
"@ | Set-Content -Path $instructionPath -Encoding UTF8

Write-Host ''
Write-Host 'Done.'
Write-Host "Upload folder: $uploadRoot"
Write-Host "Portable zip: $portableZipPath"
Write-Host "Guide file: $instructionPath"
