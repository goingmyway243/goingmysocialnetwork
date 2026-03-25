#!/bin/bash

# GoingMy Social Network - Launcher
# Usage: ./run.sh [web|services|all]

set -e

# Colors for output
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Default target
TARGET="${1:-all}"

# Validate target
if [[ ! "$TARGET" =~ ^(web|services|all)$ ]]; then
    echo -e "${RED}ERROR: Invalid target: '$TARGET'${NC}"
    echo -e "${YELLOW}Valid options: web, services, all${NC}"
    exit 1
fi

start_web_app() {
    echo -e "\n${CYAN}======================================${NC}"
    echo -e "${GREEN}Starting Angular Web Application${NC}"
    echo -e "${CYAN}======================================\n${NC}"
    
    local webPath="src/GoingMy.Web"
    
    if [ ! -d "$webPath" ]; then
        echo -e "${RED}ERROR: Web project not found at $webPath${NC}"
        return 1
    fi
    
    pushd "$webPath" > /dev/null
    echo -e "${YELLOW}Current directory: $(pwd)${NC}"
    
    # Check if node_modules doesn't exist
    if [ ! -d "node_modules" ]; then
        echo -e "${YELLOW}Installing npm dependencies...${NC}"
        npm install
        if [ $? -ne 0 ]; then
            echo -e "${RED}ERROR: npm install failed${NC}"
            popd > /dev/null
            return 1
        fi
    fi
    
    echo -e "${YELLOW}Running: npm start\n${NC}"
    npm start
    local result=$?
    
    popd > /dev/null
    return $result
}

start_services() {
    echo -e "\n${CYAN}======================================${NC}"
    echo -e "${GREEN}Starting .NET Aspire Services${NC}"
    echo -e "${CYAN}======================================\n${NC}"
    
    local appHostPath="src/GoingMy.AppHost"
    
    if [ ! -d "$appHostPath" ]; then
        echo -e "${RED}ERROR: AppHost project not found at $appHostPath${NC}"
        return 1
    fi
    
    echo -e "${YELLOW}Launching Services in a new terminal window...${NC}\n"
    
    # Detect OS and use appropriate terminal command
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        open -a Terminal "$(pwd)/$appHostPath" --args "dotnet run"
        # Create a script to run in the terminal
        osascript <<EOF
tell application "Terminal"
    activate
    do script "cd '$(pwd)/$appHostPath' && echo 'Current directory: '$(pwd)' && echo 'Running: dotnet run' && dotnet run && read -p 'Press Enter to close this window'"
end tell
EOF
    elif command -v gnome-terminal &> /dev/null; then
        # Linux with GNOME Terminal
        gnome-terminal -- bash -c "cd '$appHostPath' && echo 'Current directory: \$(pwd)' && echo 'Running: dotnet run' && dotnet run && read -p 'Press Enter to close this window'"
    elif command -v xterm &> /dev/null; then
        # Linux with xterm
        xterm -e "cd '$appHostPath' && dotnet run && read -p 'Press Enter to close this window'" &
    else
        echo -e "${RED}ERROR: No suitable terminal found${NC}"
        return 1
    fi
    
    return 0
}

# Display banner
echo -e "${CYAN}=====================================
   GoingMy Social Network - Launcher
=====================================
Target: $TARGET
${NC}"

case $TARGET in
    web)
        start_web_app
        ;;
    services)
        start_services
        ;;
    all)
        echo -e "${CYAN}Starting both Web App and Services...\n${NC}"
        
        # Start services in new terminal window
        start_services
        
        echo -e "${YELLOW}Waiting 3 seconds for services to initialize...\n${NC}"
        sleep 3
        
        # Start web in foreground
        start_web_app
        ;;
esac

echo -e "\n${GREEN}Completed${NC}"
