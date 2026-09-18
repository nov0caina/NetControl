#!/bin/bash
# ═══════════════════════════════════════════════════════
#  NetControl - Linux Launcher
#  Enables IP forwarding and runs with sudo
# ═══════════════════════════════════════════════════════

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/NetControl"
BINARY="$PROJECT_DIR/bin/Release/net8.0/NetControl"

echo -e "${CYAN}"
echo "╔══════════════════════════════════════╗"
echo "║     NetControl - Linux Launcher      ║"
echo "╚══════════════════════════════════════╝"
echo -e "${NC}"

# ── Check if built ──
if [ ! -f "$BINARY" ] && [ ! -f "$BINARY.dll" ]; then
    echo -e "${RED}[ERROR] NetControl not built. Run ./install_linux.sh first.${NC}"
    exit 1
fi

# ── Check for root ──
if [ "$EUID" -ne 0 ]; then
    echo -e "${YELLOW}[!] NetControl requires root privileges for network access.${NC}"
    echo -e "${YELLOW}    Relaunching with sudo...${NC}"
    echo ""
    exec sudo bash "$0" "$@"
fi

# ── Cleanup Handler ──
cleanup() {
    echo ""
    echo -e "${YELLOW}[Cleanup] Restoring network state...${NC}"
    if [ -n "$CURRENT_FWD" ]; then
        sysctl -w net.ipv4.ip_forward=$CURRENT_FWD > /dev/null 2>&1
        echo -e "${GREEN}[OK] IP forwarding restored to previous state ($CURRENT_FWD).${NC}"
    fi

    # Flush residual tc qdiscs if tc is present
    if command -v tc >/dev/null 2>&1; then
        for iface in $(ip -o link show 2>/dev/null | awk -F': ' '{print $2}' | cut -d'@' -f1 | grep -v "lo"); do
            tc qdisc del dev "$iface" root >/dev/null 2>&1 || true
        done
    fi
}
trap cleanup EXIT

# ── Enable IP forwarding (required for MITM) ──
echo -e "${YELLOW}[1/2] Enabling IP forwarding...${NC}"
CURRENT_FWD=$(cat /proc/sys/net/ipv4/ip_forward)
sysctl -w net.ipv4.ip_forward=1 > /dev/null 2>&1
echo -e "${GREEN}[OK] IP forwarding enabled.${NC}"

# ── Launch ──
echo -e "${YELLOW}[2/2] Launching NetControl...${NC}"
echo ""

cd "$PROJECT_DIR"
dotnet run --configuration Release --no-build
EXIT_CODE=$?

exit $EXIT_CODE
