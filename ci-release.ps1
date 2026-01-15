$ErrorActionPreference = 'Stop'

# Validate required variables
if ([string]::IsNullOrWhiteSpace($env:CI_COMMIT_TAG)) {
    Write-Error 'CI_COMMIT_TAG is not set. This job should only run on tags.'
    exit 1
}

if ([string]::IsNullOrWhiteSpace($env:GITHUB_TOKEN)) {
    Write-Error 'GITHUB_TOKEN is not set. Please configure it in GitLab CI/CD variables.'
    exit 1
}

# Validate exe exists
$exePath = Join-Path $env:PUBLISH_DIR 'Cortex.exe'
if (-not (Test-Path $exePath)) {
    Write-Error "Cortex.exe not found at: $exePath"
    exit 1
}

Write-Host "Found Cortex.exe at: $exePath"
$exeInfo = Get-Item $exePath
Write-Host "File size: $($exeInfo.Length) bytes"

# Extract tag name (remove 'refs/tags/' prefix if present)
$tagName = $env:CI_COMMIT_TAG -replace '^refs/tags/', ''
Write-Host "Creating release for tag: $tagName"

# GitHub API endpoint
$apiUrl = "https://api.github.com/repos/$env:GITHUB_REPO/releases"

# Create release payload
$releaseBody = @{
    tag_name = $tagName
    name = "Cortex $tagName"
    body = "Automated release from GitLab CI/CD pipeline`n`nBuilt on: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')`nCommit: $env:CI_COMMIT_SHA"
    draft = $false
    prerelease = $false
} | ConvertTo-Json

Write-Host "Creating release via GitHub API..."

# Create release
$headers = @{
    'Authorization' = "token $env:GITHUB_TOKEN"
    'Accept' = 'application/vnd.github.v3+json'
    'Content-Type' = 'application/json'
}

try {
    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers -Body $releaseBody
    $releaseId = $response.id
    Write-Host "Release created successfully. Release ID: $releaseId"
}
catch {
    Write-Host "Error creating release: $_"
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse -and $errorResponse.message -like '*already exists*') {
        Write-Host "Release already exists. Attempting to get existing release..."
        # Try to get existing release
        $getUrl = "https://api.github.com/repos/$env:GITHUB_REPO/releases/tags/$tagName"
        try {
            $response = Invoke-RestMethod -Uri $getUrl -Method Get -Headers $headers
            $releaseId = $response.id
            Write-Host "Found existing release. Release ID: $releaseId"
        }
        catch {
            Write-Error "Failed to get existing release: $_"
            exit 1
        }
    }
    else {
        Write-Error "Failed to create release: $_"
        exit 1
    }
}

# Check if exe asset already exists and delete it if needed
Write-Host "Checking for existing Cortex.exe asset..."
$assetsUrl = "https://api.github.com/repos/$env:GITHUB_REPO/releases/$releaseId/assets"
try {
    $existingAssets = Invoke-RestMethod -Uri $assetsUrl -Headers $headers -Method Get
    foreach ($asset in $existingAssets) {
        if ($asset.name -eq "Cortex.exe") {
            Write-Host "Found existing Cortex.exe asset (ID: $($asset.id)). Deleting to replace with new build..."
            $deleteUrl = "https://api.github.com/repos/$env:GITHUB_REPO/releases/assets/$($asset.id)"
            try {
                Invoke-RestMethod -Uri $deleteUrl -Headers $headers -Method Delete
                Write-Host "Deleted existing asset successfully."
            }
            catch {
                Write-Host "Warning: Failed to delete existing asset: $_"
                Write-Host "Will attempt to upload anyway (may result in duplicate or error)..."
            }
            break
        }
    }
}
catch {
    Write-Host "Could not check for existing assets: $_"
    Write-Host "Continuing with upload..."
}

# Upload exe as release asset
$uploadUrl = "https://uploads.github.com/repos/$env:GITHUB_REPO/releases/$releaseId/assets?name=Cortex.exe"
Write-Host "Uploading Cortex.exe to release..."

$uploadHeaders = @{
    'Authorization' = "token $env:GITHUB_TOKEN"
    'Accept' = 'application/vnd.github.v3+json'
    'Content-Type' = 'application/octet-stream'
}

try {
    $fileBytes = [System.IO.File]::ReadAllBytes($exePath)
    $response = Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $uploadHeaders -Body $fileBytes
    Write-Host "Successfully uploaded Cortex.exe to release!"
    Write-Host "Download URL: $($response.browser_download_url)"
    Write-Host "Release URL: https://github.com/$env:GITHUB_REPO/releases/tag/$tagName"
}
catch {
    $errorDetails = $_.ErrorDetails.Message
    Write-Host "Error uploading exe: $_"
    Write-Host "Error details: $errorDetails"
    
    # Check if it's a duplicate asset error
    if ($errorDetails -like '*already_exists*' -or $errorDetails -like '*duplicate*') {
        Write-Host "Asset already exists. Attempting to get release assets and update link..."
        try {
            $assets = Invoke-RestMethod -Uri $assetsUrl -Headers $headers -Method Get
            foreach ($asset in $assets) {
                if ($asset.name -eq "Cortex.exe") {
                    Write-Host "Found existing Cortex.exe asset:"
                    Write-Host "  Download URL: $($asset.browser_download_url)"
                    Write-Host "  Release URL: https://github.com/$env:GITHUB_REPO/releases/tag/$tagName"
                    Write-Host "Note: Using existing asset. If you need to replace it, delete the release and re-run."
                    exit 0
                }
            }
        }
        catch {
            Write-Error "Failed to get existing assets: $_"
            exit 1
        }
    }
    else {
        Write-Error "Failed to upload exe: $_"
        exit 1
    }
}

Write-Host "Release process completed successfully!"
