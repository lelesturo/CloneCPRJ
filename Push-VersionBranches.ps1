$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$remote = git remote
if (-not $remote) {
    throw "Nessun remote configurato. Prima esegui: git remote add origin <URL_REPOSITORY>"
}

git push -u origin main

$branches = git branch --format "%(refname:short)" | Where-Object { $_ -like "_versione_*" }
foreach ($branch in $branches) {
    git push -u origin $branch
}

Write-Host "Push completato per main e branch versione."
