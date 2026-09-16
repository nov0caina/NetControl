#!/bin/bash
# ═══════════════════════════════════════════════════════
#  NetControl - Linux Installer
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
echo "║     NetControl - Linux Installer     ║"
echo "╚══════════════════════════════════════╝"
echo -e "${NC}"

# ── Detect package manager ──
if command -v apt &> /dev/null; then
    PKG_MGR="apt"
elif command -v dnf &> /dev/null; then
    PKG_MGR="dnf"
elif command -v pacman &> /dev/null; then
    PKG_MGR="pacman"
else
    echo -e "${RED}[ERROR] No supported package manager found (apt/dnf/pacman).${NC}"
    exit 1
fi
echo -e "${GREEN}[OK] Package manager: ${PKG_MGR}${NC}"

# ── Install libpcap ──
echo -e "${YELLOW}[1/3] Installing libpcap...${NC}"
case $PKG_MGR in
    apt)
        sudo apt update -qq
        sudo apt install -y libpcap-dev
        ;;
    dnf)
        sudo dnf install -y libpcap-devel
        ;;
    pacman)
        sudo pacman -Sy --noconfirm libpcap
        ;;
esac
echo -e "${GREEN}[OK] libpcap installed.${NC}"

# ── Install .NET 8 SDK ──
echo -e "${YELLOW}[2/3] Checking .NET 8 SDK...${NC}"
if command -v dotnet &> /dev/null && dotnet --list-sdks 2>/dev/null | grep -q "^8\."; then
    echo -e "${GREEN}[OK] .NET 8 SDK already installed.${NC}"
else
    echo -e "${YELLOW}    Installing .NET 8 SDK...${NC}"
    case $PKG_MGR in
        apt)
            # Try the Ubuntu/Debian package first
            if apt-cache show dotnet-sdk-8.0 &> /dev/null; then
                sudo apt install -y dotnet-sdk-8.0
            else
                # Fallback to snap
                sudo snap install dotnet-sdk --classic
            fi
            ;;
        dnf)
            sudo dnf install -y dotnet-sdk-8.0
            ;;
        pacman)
            sudo pacman -Sy --noconfirm dotnet-sdk-8.0
            ;;
    esac
    echo -e "${GREEN}[OK] .NET 8 SDK installed.${NC}"
fi

# ── Build project ──
echo -e "${YELLOW}[3/3] Building NetControl...${NC}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/SelfishNet"
dotnet restore
dotnet build --configuration Release

echo ""
echo -e "${GREEN}╔══════════════════════════════════════╗${NC}"
echo -e "${GREEN}║      Installation complete!          ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════╝${NC}"
echo ""
echo -e "Run ${CYAN}./start_linux.sh${NC} to launch NetControl."
echo -e "${YELLOW}Note: Requires sudo for network access.${NC}"
