param(
    [string]$Platform = "Windows",
    [string]$AppId = "",
    [string]$AppiumServer = "http://127.0.0.1:4723"
)

function Resolve-AppPath {
    param(
        [string]$RootPath
    )

    $searchRoots = @(
        (Join-Path $RootPath "bin\Debug"),
        (Join-Path $RootPath "bin\Release")
    )

    $candidates = @()
    foreach ($searchRoot in $searchRoots) {
        if (Test-Path $searchRoot) {
            $candidates += Get-ChildItem -Path $searchRoot -Filter "check-yo-self-ai.exe" -Recurse -ErrorAction SilentlyContinue
        }
    }

    return $candidates |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1
}

$workspaceRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ([string]::IsNullOrWhiteSpace($AppId)) {
    $detectedApp = Resolve-AppPath -RootPath $workspaceRoot.Path
    if ($null -ne $detectedApp) {
        $AppId = $detectedApp.FullName
        Write-Host "Detected UI_TEST_APP_ID: $AppId"
    }
    else {
        Write-Warning "Could not auto-detect app executable. Build the app first or pass -AppId explicitly."
    }
}

$env:UI_TEST_PLATFORM = $Platform
$env:UI_TEST_APP_ID = $AppId
$env:UI_TEST_APPIUM_SERVER = $AppiumServer

Write-Host "Running UI tests with platform '$Platform' against '$AppiumServer'..."
dotnet test Tests/CheckYoSelfAI.Tests.csproj --filter "Category=UI"
