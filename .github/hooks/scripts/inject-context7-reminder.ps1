# inject-context7-reminder.ps1
# Reads the UserPromptSubmit hook payload from stdin.
# If the prompt references PrimeNG or a need for documentation,
# injects a systemMessage reminding the agent to use Context7.

$input_json = $null
try {
    $raw = [Console]::In.ReadToEnd()
    $input_json = $raw | ConvertFrom-Json
} catch {
    exit 0
}

$prompt = ""
if ($input_json.PSObject.Properties["prompt"]) {
    $prompt = $input_json.prompt
}

$keywords = @(
    "primeng",
    "prime ng",
    "p-",
    "primeicons",
    "look up",
    "check docs",
    "documentation",
    "how to use",
    "api reference",
    "library docs",
    "component docs",
    "upgrade",
    "migration"
)

$matched = $false
foreach ($kw in $keywords) {
    if ($prompt -match [regex]::Escape($kw)) {
        $matched = $true
        break
    }
}

if ($matched) {
    $output = @{
        systemMessage = "REMINDER: You must use Context7 MCP for up-to-date library documentation. Do NOT rely on training data. Steps: (1) call resolve-library-id to get the Context7 library ID, (2) call get-library-docs with mode='code' for API/examples or mode='info' for concepts. Apply this before writing any PrimeNG component code or answering any library question."
    }
    $output | ConvertTo-Json -Compress
}

exit 0
