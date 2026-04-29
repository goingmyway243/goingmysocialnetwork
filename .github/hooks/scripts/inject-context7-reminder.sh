#!/usr/bin/env bash
# inject-context7-reminder.sh
# Reads the UserPromptSubmit hook payload from stdin.
# If the prompt references PrimeNG or documentation needs,
# injects a systemMessage reminding the agent to use Context7.

raw=$(cat)
prompt=$(echo "$raw" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('prompt',''))" 2>/dev/null || echo "")

keywords=("primeng" "prime ng" "p-" "primeicons" "look up" "check docs" "documentation" "how to use" "api reference" "library docs" "component docs" "upgrade" "migration")

matched=false
lower_prompt=$(echo "$prompt" | tr '[:upper:]' '[:lower:]')

for kw in "${keywords[@]}"; do
    if [[ "$lower_prompt" == *"$kw"* ]]; then
        matched=true
        break
    fi
done

if $matched; then
    echo '{"systemMessage":"REMINDER: You must use Context7 MCP for up-to-date library documentation. Do NOT rely on training data. Steps: (1) call resolve-library-id to get the Context7 library ID, (2) call get-library-docs with mode='\''code'\'' for API/examples or mode='\''info'\'' for concepts. Apply this before writing any PrimeNG component code or answering any library question."}'
fi

exit 0
