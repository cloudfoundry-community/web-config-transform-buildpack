Param(
  [Parameter(Mandatory=$True)]
  [string]$BuildDir,
  [Parameter(Mandatory=$True)]
  [string]$CacheDir,
  [Parameter(Mandatory=$True)]
  [string]$DepsDir,
  [Parameter(Mandatory=$True)]
  [string]$Index
)
$ErrorActionPreference = "Stop"

echo "=== Web.config Transform Buildpack"
& $PSScriptRoot\config-transform.exe $BuildDir
