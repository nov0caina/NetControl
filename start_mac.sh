#!/bin/bash
# ═══════════════════════════════════════════════════════
#  NetControl - macOS Launcher
#  Enables IP forwarding and runs with sudo
# ═══════════════════════════════════════════════════════

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/SelfishNet"
BINARY="$PROJECT_DIR/bin/Release/net8.0/NetControl"

echo -e "${CYAN}"
echo "╔══════════════════════════════════════╗"
echo "║     NetControl - macOS Launcher      ║"
echo "╚══════════════════════════════════════╝"
echo -e "${NC}"

# ── Check if built ──
if [ ! -f "$BINARY" ] && [ ! -f "$BINARY.dll" ] && [ ! -f "$PROJECT_DIR/bin/Release/net8.0/SelfishNet" ] && [ ! -f "$PROJECT_DIR/bin/Release/net8.0/SelfishNet.dll" ]; then
    echo -e "${RED}[ERROR] NetControl not built. Run ./install_mac.sh first.${NC}"
    exit 1
fi

# ── Check for root ──
if [ "$EUID" -ne 0 ]; then
    echo -e "${YELLOW}[!] NetControl requires root privileges for network access.${NC}"
    echo -e "${YELLOW}    Relaunching with sudo...${NC}"
    echo ""
    exec sudo bash "$0" "$@"
fi

# ── Enable IP forwarding (required for MITM on macOS) ──
echo -e "${YELLOW}[1/2] Enabling IP forwarding...${NC}"
CURRENT_FWD=$(sysctl -n net.inet.ip.forwarding)
sysctl -w net.inet.ip.forwarding=1 > /dev/null 2>&1
echo -e "${GREEN}[OK] IP forwarding enabled.${NC}"

# ── Launch ──
echo -e "${YELLOW}[2/2] Launching NetControl...${NC}"
echo ""

cd "$PROJECT_DIR"
dotnet run --configuration Release --no-build
EXIT_CODE=$?

# ── Restore IP forwarding to previous state ──
sysctl -w net.inet.ip.forwarding=$CURRENT_FWD > /dev/null 2>&1
echo -e "${GREEN}[OK] IP forwarding restored to previous state ($CURRENT_FWD).${NC}"

exit $EXIT_CODE
