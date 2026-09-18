#!/bin/bash
# ═══════════════════════════════════════════════════════
#  NetControl - macOS Installer
#  Installs .NET 8 SDK, libpcap, and builds the project
# ═══════════════════════════════════════════════════════

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${CYAN}"
echo "╔══════════════════════════════════════╗"
echo "║     NetControl - macOS Installer     ║"
echo "╚══════════════════════════════════════╝"
echo -e "${NC}"

# ── Check Homebrew ──
if ! command -v brew &> /dev/null; then
    echo -e "${YELLOW}[!] Homebrew not found. Installing...${NC}"
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
    echo -e "${GREEN}[OK] Homebrew installed.${NC}"
else
    echo -e "${GREEN}[OK] Homebrew found.${NC}"
fi

# ── Install libpcap (usually pre-installed on macOS) ──
echo -e "${YELLOW}[1/3] Checking libpcap...${NC}"
if [ -f "/usr/lib/libpcap.dylib" ] || [ -f "/usr/local/lib/libpcap.dylib" ] || [ -f "/opt/homebrew/lib/libpcap.dylib" ]; then
    echo -e "${GREEN}[OK] libpcap found (pre-installed).${NC}"
else
    echo -e "${YELLOW}    Installing libpcap via Homebrew...${NC}"
    brew install libpcap
    echo -e "${GREEN}[OK] libpcap installed.${NC}"
fi

# ── Install .NET 8 SDK ──
echo -e "${YELLOW}[2/3] Checking .NET 8 SDK...${NC}"
if command -v dotnet &> /dev/null && dotnet --list-sdks 2>/dev/null | grep -q "^8\."; then
    echo -e "${GREEN}[OK] .NET 8 SDK already installed.${NC}"
else
    echo -e "${YELLOW}    Installing .NET 8 SDK via Homebrew...${NC}"
    brew install --cask dotnet-sdk
    echo -e "${GREEN}[OK] .NET 8 SDK installed.${NC}"
fi

# ── Build project ──
echo -e "${YELLOW}[3/3] Building NetControl...${NC}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/NetControl"
dotnet restore
dotnet build --configuration Release

echo ""
echo -e "${GREEN}╔══════════════════════════════════════╗${NC}"
echo -e "${GREEN}║      Installation complete!          ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════╝${NC}"
echo ""
echo -e "Run ${CYAN}./start_mac.sh${NC} to launch NetControl."
echo -e "${YELLOW}Note: Requires sudo for network access.${NC}"
