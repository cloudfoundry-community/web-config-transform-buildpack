Param(
  [Parameter(Mandatory=$True,Position=1)]
    [string]$BuildDir
)
$ErrorActionPreference = "Stop"

echo "=== Testing"
dir $BuildDir
