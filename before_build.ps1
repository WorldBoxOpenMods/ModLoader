$commit = "unknown"

if (Get-Command git -ErrorAction SilentlyContinue) {
    $git_commit = git rev-parse HEAD 2>$null
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($git_commit)) {
        $commit = $git_commit.Trim()
    }
}

Set-Content -Path "resources/commit" -Value $commit -NoNewline
exit 0
