param(
    [Parameter(Mandatory = $true)]
    [string]$FromVersion,

    [Parameter(Mandatory = $true)]
    [string]$ToVersion
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$from = $FromVersion.Trim().ToUpperInvariant()
$to = $ToVersion.Trim().ToUpperInvariant()

if ($from -notmatch '^V\d+(_\d+)?$') {
    throw "FromVersion deve avere formato V3, V4, V2_1, ecc."
}

if ($to -notmatch '^V\d+(_\d+)?$') {
    throw "ToVersion deve avere formato V3, V4, V5, ecc."
}

$source = Join-Path $root $from
$target = Join-Path $root $to

if (-not (Test-Path -LiteralPath $source)) {
    throw "Versione sorgente non trovata: $source"
}

if (Test-Path -LiteralPath $target) {
    throw "Versione destinazione gia esistente: $target"
}

git checkout main
Copy-Item -LiteralPath $source -Destination $target -Recurse

git add $to VERSIONING_RULES.md README.md .gitignore New-VersionBranch.ps1
git commit -m "Add $to"

$branch = "_versione_" + $to.ToLowerInvariant()
git checkout -b $branch

Write-Host "Creati cartella $to e branch $branch."
Write-Host "Per pubblicare: git push -u origin $branch"
