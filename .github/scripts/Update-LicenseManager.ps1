<#
.SYNOPSIS
    Fetch the latest (or a specific) LicenseManager release from GitHub and
    install native library artifacts into ThunderPropagator.Application/runtimes/.

.DESCRIPTION
    Works like a lightweight NuGet restore for the native LicenseManager binaries.
    Path layout and library names are derived from LicenseManagerInterop.cs which uses
    NativeLibrary.SetDllImportResolver with:

        runtimes/{platform}-{arch}/native/{libName}

    where:
        platform = win | linux | macos     (NOT standard .NET RIDs)
        arch     = x64 | arm64
        libName  = LicenseManager.dll         (Windows, no "lib" prefix)
                   libLicenseManager.so        (Linux)
                   libLicenseManager.dylib     (macOS)

    The ThunderPropagator.Application.csproj already includes all runtimes/ files via:
        <None Include="runtimes\**\*.*" Pack="true" CopyToOutputDirectory="Always" />
    so no .csproj changes are needed.

.PARAMETER Version
    Specific release tag to install (e.g. "v2025.Q4.128").
    Defaults to the latest published release.

.PARAMETER GitHubToken
    PAT for private repos or to avoid API rate-limiting.
    Falls back to GH_TOKEN then GITHUB_TOKEN environment variables.

.PARAMETER Platforms
    Subset of platforms to install.
    Default: all 6 supported platforms.

.PARAMETER Check
    Print installed vs latest version and exit without installing.

.PARAMETER Force
    Re-install even if already at the requested version.

.EXAMPLE
    pwsh .github/scripts/Update-LicenseManager.ps1
    pwsh .github/scripts/Update-LicenseManager.ps1 -Version v2025.Q4.200
    pwsh .github/scripts/Update-LicenseManager.ps1 -Platforms linux-x64,linux-arm64
    pwsh .github/scripts/Update-LicenseManager.ps1 -Check
    pwsh .github/scripts/Update-LicenseManager.ps1 -WhatIf
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [string]   $Version     = "",
    [string]   $GitHubToken = "",

    [ValidateSet(
        'linux-x64','linux-arm64',
        'windows-x64','windows-arm64',
        'macos-x64','macos-arm64')]
    [string[]] $Platforms = @(
        'linux-x64','linux-arm64',
        'windows-x64','windows-arm64',
        'macos-x64','macos-arm64'),

    [switch] $Check,
    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

# ── Resolve GitHub token (GH_TOKEN takes precedence over GITHUB_TOKEN) ────────

if ([string]::IsNullOrWhiteSpace($GitHubToken)) {
    $GitHubToken = if ($env:GH_TOKEN)     { $env:GH_TOKEN }
                   elseif ($env:GITHUB_TOKEN) { $env:GITHUB_TOKEN }
                   else { "" }
}

# ── Path constants derived from LicenseManagerInterop.cs ──────────────────────
#
# GetNativeLibRelativePath() builds:
#   Path.Combine("runtimes", "{platform}-{arch}", "native", "{libName}")
#
# NOTE: macOS folder is "macos-x64" NOT "osx-x64".
#       Windows library has no "lib" prefix.
#       Both .so and .dll use capital L: libLicenseManager / LicenseManager.
#       Build artifacts use lowercase l: liblicenseManager / liblicenseManager.
#       The script renames on copy so the interop resolver finds them.

$REPO            = "KiarashMinoo/LicenseManager"
$API_BASE        = "https://api.github.com/repos/$REPO"
$ARTIFACT_PREFIX = "LicenseManager"
$VERSION_FILE    = "license-manager.version"

# LicenseManager platform  -->  interop runtime folder (as used in LicenseManagerInterop.cs)
$INTEROP_FOLDER = [ordered]@{
    'linux-x64'    = 'linux-x64'
    'linux-arm64'  = 'linux-arm64'
    'windows-x64'  = 'win-x64'
    'windows-arm64'= 'win-arm64'
    'macos-x64'    = 'macos-x64'
    'macos-arm64'  = 'macos-arm64'
}

# Name the file must have inside runtimes/{folder}/native/ (LicenseManagerInterop expectation)
$INTEROP_NAME = [ordered]@{
    'linux-x64'    = 'libLicenseManager.so'
    'linux-arm64'  = 'libLicenseManager.so'
    'windows-x64'  = 'LicenseManager.dll'
    'windows-arm64'= 'LicenseManager.dll'
    'macos-x64'    = 'libLicenseManager.dylib'
    'macos-arm64'  = 'libLicenseManager.dylib'
}

# Primary file names as produced by LicenseManager build-all.ps1 (lowercase l)
$BUILD_NAME = [ordered]@{
    'linux-x64'    = 'liblicenseManager.so'
    'linux-arm64'  = 'liblicenseManager.so'
    'windows-x64'  = 'liblicenseManager.dll'
    'windows-arm64'= 'liblicenseManager.dll'
    'macos-x64'    = 'liblicenseManager.dylib'
    'macos-arm64'  = 'liblicenseManager.dylib'
}

# ── Resolve target runtimes directory ─────────────────────────────────────────
# Canonical: ThunderPropagator.Application/runtimes/
# Covered by the existing .csproj glob -- no patching required.

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$appSrc   = Join-Path (Join-Path $repoRoot "src") "ThunderPropagator.Application"

if (-not (Test-Path $appSrc)) {
    $found = Get-ChildItem -Path $repoRoot -Recurse -Filter "ThunderPropagator.Application.csproj" |
             Select-Object -First 1
    if ($found) {
        $appSrc = Split-Path $found.FullName -Parent
    } else {
        throw "Cannot locate ThunderPropagator.Application. Run from the repo root."
    }
}

$runtimesDir = Join-Path $appSrc "runtimes"

# ── Helpers ───────────────────────────────────────────────────────────────────

function Write-Step { param($m) Write-Host "  -> $m" -ForegroundColor Cyan }
function Write-Ok   { param($m) Write-Host "  OK $m" -ForegroundColor Green }
function Write-Warn { param($m) Write-Host "  !! $m" -ForegroundColor Yellow }

function Invoke-GitHubApi {
    param([string]$Endpoint)
    $headers = @{ 'User-Agent' = 'Update-LicenseManager.ps1' }
    if ($GitHubToken) { $headers['Authorization'] = "Bearer $GitHubToken" }
    try {
        Invoke-RestMethod -Uri "$API_BASE/$Endpoint" -Headers $headers
    } catch {
        $response = $_.Exception.Response
        $code     = if ($response) { [int]$response.StatusCode } else { 0 }
        if ($code -eq 404) { throw "GitHub 404 -- release not found or repo is private. Pass -GitHubToken." }
        if ($code -eq 403) { throw "GitHub 403 -- rate-limited. Set GH_TOKEN or pass -GitHubToken." }
        throw
    }
}

function Get-InstalledVersion {
    $vf = Join-Path $runtimesDir $VERSION_FILE
    if (Test-Path $vf) { return (Get-Content $vf -Raw).Trim() }
    return $null
}

# ── Banner ────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "LicenseManager Updater" -ForegroundColor White
Write-Host "  Repo     : $REPO"
Write-Host "  Target   : $appSrc"
Write-Host "  Runtimes : $runtimesDir"
Write-Host "  Token    : $(if ($GitHubToken) { 'set' } else { 'not set (public API, rate-limited)' })"
Write-Host ""

# ── Resolve release ───────────────────────────────────────────────────────────

Write-Step "Querying GitHub releases..."

$release = if ($Version) {
    $tag = $Version.TrimStart('v')
    Invoke-GitHubApi "releases/tags/v$tag"
} else {
    Invoke-GitHubApi "releases/latest"
}

$latestTag  = $release.tag_name
$versionNum = $latestTag.TrimStart('v')

Write-Ok "Latest: $latestTag ($($release.name))"
Write-Host "  Published : $($release.published_at)"
Write-Host "  Assets    : $($release.assets.Count)"

# ── Check mode ────────────────────────────────────────────────────────────────

$installed = Get-InstalledVersion

if ($Check) {
    Write-Host ""
    if ($installed) {
        Write-Host "  Installed : $installed"
        Write-Host "  Latest    : $latestTag"
        if ($installed -eq $latestTag) { Write-Ok "Up-to-date." }
        else { Write-Warn "Update available: $installed --> $latestTag" }
    } else {
        Write-Warn "Not installed. Latest available: $latestTag"
    }
    exit 0
}

if ((-not $Force) -and ($installed -eq $latestTag)) {
    Write-Ok "Already at $latestTag. Use -Force to reinstall."
    exit 0
}

if ($installed) { Write-Warn "Upgrading: $installed --> $latestTag" }
else            { Write-Step "Fresh install of $latestTag" }

# ── Download + install each platform ─────────────────────────────────────────

$guid    = [System.Guid]::NewGuid().ToString('N').Substring(0,8)
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "lm-update-$guid"
$null    = New-Item -ItemType Directory -Path $tempDir -Force

try {
    $okCount = 0

    foreach ($platform in $Platforms) {
        $folder      = $INTEROP_FOLDER[$platform]
        $interopName = $INTEROP_NAME[$platform]
        $buildName   = $BUILD_NAME[$platform]
        $destDir     = Join-Path (Join-Path $runtimesDir $folder) "native"

        # Match release asset: LicenseManager-{platform}-{version}.zip
        $pattern = "$ARTIFACT_PREFIX-$platform-$versionNum"
        $asset   = $release.assets |
                   Where-Object { $_.name -like "$pattern*.zip" } |
                   Select-Object -First 1

        if (-not $asset) {
            Write-Warn "[$platform] No asset '$pattern*.zip' in release -- skipping"
            continue
        }

        $sizeMb = [math]::Round($asset.size / 1048576, 1)
        Write-Step "[$platform] Downloading $($asset.name) ($sizeMb MB)..."

        $zipPath    = Join-Path $tempDir $asset.name
        $extractDir = Join-Path $tempDir $platform

        # For private repos browser_download_url returns 404 with a Bearer token.
        # Use the GitHub Assets API with Accept: application/octet-stream instead --
        # it performs the auth-aware redirect and works for both public and private repos.
        $assetApiUrl = "https://api.github.com/repos/$REPO/releases/assets/$($asset.id)"
        $dlHeaders   = @{
            'User-Agent' = 'Update-LicenseManager.ps1'
            'Accept'     = 'application/octet-stream'
        }
        if ($GitHubToken) { $dlHeaders['Authorization'] = "Bearer $GitHubToken" }

        if ($PSCmdlet.ShouldProcess($assetApiUrl, "Download")) {
            Invoke-WebRequest -Uri $assetApiUrl `
                              -Headers $dlHeaders `
                              -OutFile $zipPath
        }

        if ($PSCmdlet.ShouldProcess($zipPath, "Extract to $destDir")) {
            $null = New-Item -ItemType Directory -Path $extractDir -Force
            Expand-Archive -Path $zipPath -DestinationPath $extractDir -Force

            # Find primary library (by build name, lowercase)
            $primaryFile = Get-ChildItem -Path $extractDir -Filter $buildName -Recurse |
                           Sort-Object Length -Descending | Select-Object -First 1

            if (-not $primaryFile) {
                # Fallback: match by extension
                $ext         = [System.IO.Path]::GetExtension($buildName)
                $primaryFile = Get-ChildItem -Path $extractDir -Recurse |
                               Where-Object { $_.Extension -eq $ext -and $_.Name -notlike '*.a' } |
                               Sort-Object Length -Descending | Select-Object -First 1
            }

            if (-not $primaryFile) {
                Write-Warn "[$platform] Primary library not found in zip -- skipping"
                continue
            }

            $null = New-Item -ItemType Directory -Path $destDir -Force

            # Copy with the name expected by LicenseManagerInterop.cs
            $destPrimary = Join-Path $destDir $interopName
            Copy-Item -Path $primaryFile.FullName -Destination $destPrimary -Force
            Write-Ok "  [$platform] $interopName --> runtimes/$folder/native/"

            # Also copy versioned companion files (.so.2025, .so.2025.4.128)
            # so the Linux SONAME chain stays intact.
            $srcDir      = Split-Path $primaryFile.FullName -Parent
            $interopStem = [System.IO.Path]::GetFileNameWithoutExtension($interopName)
            $interopExt  = [System.IO.Path]::GetExtension($interopName)

            $companions = Get-ChildItem -Path $srcDir |
                          Where-Object {
                              $_.Name -ne $primaryFile.Name -and
                              $_.Name.StartsWith($buildName)
                          }

            foreach ($companion in $companions) {
                $suffix       = $companion.Name.Substring($buildName.Length)
                $companionDst = Join-Path $destDir "$interopStem$interopExt$suffix"
                Copy-Item -Path $companion.FullName -Destination $companionDst -Force
                Write-Host "             + $([System.IO.Path]::GetFileName($companionDst))" -ForegroundColor DarkGray
            }

            $okCount++
        } else {
            Write-Host "  [WhatIf] Would install $interopName to runtimes/$folder/native/" -ForegroundColor DarkGray
        }
    }

    # ── Write version lock-file ───────────────────────────────────────────────

    if ($okCount -gt 0) {
        $vf = Join-Path $runtimesDir $VERSION_FILE
        if ($PSCmdlet.ShouldProcess($vf, "Write version lock-file")) {
            $null = New-Item -ItemType Directory -Path $runtimesDir -Force
            Set-Content -Path $vf -Value $latestTag -Encoding UTF8
        }
        Write-Ok "Lock-file written: $VERSION_FILE ($latestTag)"
    }

    # ── Summary ───────────────────────────────────────────────────────────────

    Write-Host ""
    Write-Host "----------------------------------------------------" -ForegroundColor White
    if ($okCount -gt 0) {
        Write-Host "  LicenseManager $latestTag installed ($okCount platform(s))" -ForegroundColor Green
    } else {
        Write-Warn "No platforms were installed (see warnings above)."
    }
    Write-Host "  Runtimes : $runtimesDir"
    Write-Host "  Note: ThunderPropagator.Application.csproj glob" -ForegroundColor DarkGray
    Write-Host "        <None Include=""runtimes\**\*.*"" /> picks these up." -ForegroundColor DarkGray
    Write-Host "        No .csproj changes required." -ForegroundColor DarkGray
    Write-Host "----------------------------------------------------" -ForegroundColor White

} finally {
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
