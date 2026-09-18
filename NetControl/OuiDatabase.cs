using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace NetControl
{
    public static class OuiDatabase
    {
        private static readonly FrozenDictionary<int, string> s_oui = new Dictionary<int, string>
        {
            // Apple
            {0x00_1B_63, "Apple"}, {0x3C_22_FB, "Apple"}, {0xA4_83_E7, "Apple"},
            {0xF0_DB_E2, "Apple"}, {0x14_BD_61, "Apple"}, {0x68_FE_F7, "Apple"},
            {0xAC_BC_32, "Apple"}, {0xDC_56_E7, "Apple"}, {0x78_67_D7, "Apple"},
            {0x28_CF_DA, "Apple"}, {0x40_CB_C0, "Apple"}, {0xC8_69_CD, "Apple"},
            {0xB8_53_AC, "Apple"}, {0x70_DE_E2, "Apple"}, {0xE0_C7_67, "Apple"},
            {0x8C_85_90, "Apple"}, {0xF4_5C_89, "Apple"}, {0xBC_52_B7, "Apple"},
            // Samsung
            {0x00_15_99, "Samsung"}, {0x00_1E_E2, "Samsung"}, {0x78_47_1D, "Samsung"},
            {0xC0_97_27, "Samsung"}, {0xF0_25_B7, "Samsung"}, {0xA8_F2_74, "Samsung"},
            {0x10_D5_42, "Samsung"}, {0x54_92_BE, "Samsung"}, {0x8C_F5_A3, "Samsung"},
            {0xBC_72_B1, "Samsung"}, {0x34_23_BA, "Samsung"}, {0x44_6D_57, "Samsung"},
            {0xC4_73_1E, "Samsung"}, {0x50_01_BB, "Samsung"},
            // Intel
            {0x00_1B_21, "Intel"}, {0x3C_97_0E, "Intel"}, {0x68_05_CA, "Intel"},
            {0xA4_C4_94, "Intel"}, {0x80_86_F2, "Intel"}, {0xB4_69_21, "Intel"},
            {0x48_51_B7, "Intel"}, {0xDC_71_96, "Intel"}, {0x00_1E_67, "Intel"},
            {0xBC_F1_71, "Intel"}, {0x00_15_00, "Intel"}, {0x00_13_02, "Intel"},
            {0x48_4D_7E, "Intel"}, {0x24_EE_9A, "Intel"}, {0x94_E6_F7, "Intel"},
            {0xF8_59_71, "Intel"}, {0x7C_2A_31, "Intel"}, {0x18_5E_0F, "Intel"},
            {0xA0_C5_89, "Intel"}, {0x80_9B_20, "Intel"}, {0x34_E6_AD, "Intel"},
            {0x00_28_F8, "Intel"}, {0x4C_79_6E, "Intel"}, {0x64_49_7D, "Intel"},
            {0x84_FD_D1, "Intel"},
            // Xiaomi
            {0x64_CC_2E, "Xiaomi"}, {0x78_11_DC, "Xiaomi"}, {0x28_6C_07, "Xiaomi"},
            {0xF8_A4_5F, "Xiaomi"}, {0x9C_99_A0, "Xiaomi"}, {0x50_64_2B, "Xiaomi"},
            {0x0C_1D_AF, "Xiaomi"}, {0xAC_C1_EE, "Xiaomi"},
            // Huawei / Honor
            {0x00_18_82, "Huawei"}, {0x00_1E_10, "Huawei"}, {0x48_46_FB, "Huawei"},
            {0x88_66_39, "Huawei"}, {0xCC_A2_23, "Huawei"}, {0xE0_19_1D, "Huawei"},
            {0x70_8A_09, "Huawei"}, {0x24_69_A5, "Huawei"}, {0x04_F9_38, "Huawei"},
            // Realtek (WiFi/Ethernet chipsets)
            {0x00_E0_4C, "Realtek"}, {0x48_5D_36, "Realtek"}, {0xDC_02_8E, "Realtek"},
            {0x52_54_00, "Realtek"}, {0x00_10_E0, "Realtek"}, {0x00_0A_CD, "Realtek"},
            {0x10_7B_44, "Realtek"}, {0x20_76_93, "Realtek"}, {0x74_D4_35, "Realtek"},
            // Espressif (ESP32/ESP8266 IoT)
            {0x24_0A_C4, "Espressif"}, {0xA4_CF_12, "Espressif"}, {0x30_AE_A4, "Espressif"},
            {0xAC_67_B2, "Espressif"}, {0xCC_50_E3, "Espressif"}, {0x24_6F_28, "Espressif"},
            {0x30_83_98, "Espressif"}, {0x40_22_D8, "Espressif"}, {0x7C_DF_A1, "Espressif"},
            // TP-Link
            {0x50_C7_BF, "TP-Link"}, {0xC0_06_C3, "TP-Link"}, {0x14_CC_20, "TP-Link"},
            {0xEC_08_6B, "TP-Link"}, {0x60_32_B1, "TP-Link"}, {0xB0_BE_76, "TP-Link"},
            {0xA8_42_A1, "TP-Link"}, {0x30_DE_4B, "TP-Link"},
            // Cisco / Linksys
            {0x00_1A_A1, "Cisco"}, {0x00_22_55, "Cisco"}, {0x58_AC_78, "Cisco"},
            {0x00_1C_10, "Cisco/Linksys"}, {0x68_7F_74, "Cisco/Linksys"},
            // Google / Nest / Chromecast
            {0xF4_F5_D8, "Google"}, {0xA4_77_33, "Google"}, {0x54_60_09, "Google"},
            {0x18_D6_C7, "Google/Nest"}, {0x64_16_66, "Google/Nest"},
            {0x38_8B_59, "Google"}, {0xF4_F5_E8, "Google"}, {0xD4_F5_47, "Google"},
            {0x48_D6_D5, "Google"}, {0x6C_AD_F8, "Google"},
            // Amazon (Echo / Fire)
            {0x74_C2_46, "Amazon"}, {0xFC_65_DE, "Amazon"}, {0x40_B4_CD, "Amazon"},
            {0x68_37_E9, "Amazon"}, {0xA0_02_DC, "Amazon"},
            // Sony Interactive (PlayStation)
            {0x00_04_1F, "Sony Interactive"}, {0x00_D9_D1, "Sony Interactive"},
            {0x28_3F_69, "Sony Interactive"}, {0x70_9E_29, "Sony Interactive"},
            // Nintendo
            {0x00_19_FD, "Nintendo"}, {0x00_1F_32, "Nintendo"}, {0x00_22_AA, "Nintendo"},
            {0x00_22_D7, "Nintendo"}, {0x7C_BB_8A, "Nintendo"}, {0x98_B6_E9, "Nintendo"},
            {0xE0_0C_7F, "Nintendo"}, {0x04_03_D6, "Nintendo"},
            // Microsoft (Xbox / Surface)
            {0x7C_ED_8D, "Microsoft"}, {0x28_18_78, "Microsoft"}, {0x60_45_BD, "Microsoft"},
            {0xC8_3F_26, "Microsoft"}, {0x00_50_F2, "Microsoft"},
            // LG Electronics / LG Innotek
            {0xA8_23_FE, "LG Electronics"}, {0xCC_2D_8C, "LG Electronics"},
            {0x10_68_3F, "LG Electronics"}, {0xBC_F5_AC, "LG Electronics"},
            {0x00_1E_75, "LG Electronics"}, {0x4C_BA_D7, "LG Innotek"},
            {0x88_C9_D0, "LG Innotek"}, {0x34_4D_F7, "LG Innotek"},
            // Roku
            {0xB0_A7_37, "Roku"}, {0xD4_E2_2F, "Roku"}, {0xB8_3E_59, "Roku"},
            // NETGEAR
            {0xC4_3D_C7, "NETGEAR"}, {0x20_0C_C8, "NETGEAR"}, {0xA4_2B_8C, "NETGEAR"},
            {0x6C_B0_CE, "NETGEAR"},
            // ASUS
            {0x00_1A_92, "ASUS"}, {0x1C_87_2C, "ASUS"}, {0x2C_56_DC, "ASUS"},
            {0x04_D9_F5, "ASUS"}, {0x60_45_CB, "ASUS"},
            // HP (Printers)
            {0x00_1A_4B, "HP"}, {0x3C_D9_2B, "HP"}, {0x80_CE_62, "HP"},
            {0x10_1F_74, "HP"}, {0xB0_5A_DA, "HP"},
            // Epson
            {0x00_26_AB, "Epson"}, {0x64_EB_8C, "Epson"},
            // Canon
            {0x00_1E_8F, "Canon"}, {0x18_0C_AC, "Canon"},
            // Dell
            {0x00_14_22, "Dell"}, {0x18_03_73, "Dell"}, {0xA4_BB_6D, "Dell"},
            // Lenovo
            {0x28_D2_44, "Lenovo"}, {0xE8_2A_EA, "Lenovo"}, {0x50_5B_C2, "Lenovo"},
            // Sonos
            {0xB8_E9_37, "Sonos"}, {0x54_2A_1B, "Sonos"}, {0x78_28_CA, "Sonos"},
            // Tuya (Smart Home IoT)
            {0xD8_1F_12, "Tuya"}, {0x7C_F6_66, "Tuya"},
            // Ubiquiti
            {0x04_18_D6, "Ubiquiti"}, {0x24_5A_4C, "Ubiquiti"}, {0xF4_92_BF, "Ubiquiti"},
            // Arris / Motorola (cable modems)
            {0x00_1D_CF, "Arris"}, {0xF8_0B_BE, "Arris"},
            // OnePlus / OPPO / Vivo
            {0x94_65_2D, "OnePlus"}, {0xC0_EE_FB, "OnePlus"},
            {0xA0_3B_1B, "OPPO"}, {0xE8_61_7E, "OPPO"},
            {0xE4_46_DA, "Vivo"}, {0x2C_8D_B1, "Vivo"},
            // Raspberry Pi Foundation
            {0xB8_27_EB, "Raspberry Pi"}, {0xDC_A6_32, "Raspberry Pi"}, {0xE4_5F_01, "Raspberry Pi"},
            // D-Link
            {0x00_1B_11, "D-Link"}, {0x1C_7E_E5, "D-Link"}, {0xC8_D3_A3, "D-Link"},
            // ZTE (ISP routers / modems)
            {0x98_00_6A, "ZTE"}, {0x00_19_CB, "ZTE"}, {0x00_1E_73, "ZTE"},
            {0x34_4B_50, "ZTE"}, {0x54_22_F8, "ZTE"}, {0x68_77_24, "ZTE"},
            {0x90_D8_F3, "ZTE"}, {0xC8_7B_23, "ZTE"}, {0xE0_19_54, "ZTE"},
            // Sagemcom (ISP routers)
            {0x00_1E_74, "Sagemcom"}, {0x2C_39_96, "Sagemcom"}, {0xE8_F1_B0, "Sagemcom"},
            // Lite-On Technology
            {0xD0_39_57, "Lite-On"}, {0x00_26_18, "Lite-On"}, {0x40_F0_2F, "Lite-On"},
            // MediaTek (WiFi chipsets)
            {0x00_0C_E7, "MediaTek"}, {0xC4_E9_0A, "MediaTek"},
            {0x08_E8_4F, "MediaTek"}, {0x14_F8_93, "MediaTek"},
            {0x3C_55_76, "MediaTek"}, {0x74_C6_3B, "MediaTek"}, {0x90_F1_AA, "MediaTek"},
            // Qualcomm
            {0x00_03_7F, "Qualcomm"}, {0x04_BD_88, "Qualcomm"},
            // Broadcom
            {0x00_10_18, "Broadcom"}, {0x20_DB_AB, "Broadcom"},
            // Motorola / Motorola Mobility
            {0x00_08_0E, "Motorola"}, {0xC8_14_51, "Motorola"},
            {0xF8_F1_B6, "Motorola"}, {0x40_88_05, "Motorola"},
            // Aruba / HPE Networking
            {0x00_0B_86, "Aruba"}, {0x24_DE_C6, "Aruba"}, {0xD8_C7_C8, "Aruba"},
            // Honeywell / IoT
            {0x00_20_85, "Honeywell"},
            // JM Zengge (smart LED controllers)
            {0x08_65_F0, "Zengge/LED"},
            // Arcadyan (Telmex/Infinitum, Izzi routers)
            {0x04_B1_67, "Arcadyan"}, {0x10_C6_1F, "Arcadyan"}, {0x28_28_5D, "Arcadyan"},
            {0x74_31_70, "Arcadyan"}, {0x88_C3_97, "Arcadyan"}, {0xAC_22_05, "Arcadyan"},
            // Technicolor (Izzi, Megacable modems)
            {0x34_7A_60, "Technicolor"}, {0x6C_2E_85, "Technicolor"}, {0xCC_7D_37, "Technicolor"},
            {0xA0_1B_29, "Technicolor"}, {0x90_B6_86, "Technicolor"},
            // Zhone / Dasan / DZS (Totalplay ONTs)
            {0x00_09_02, "Zhone/DZS"}, {0x00_19_4F, "Zhone/DZS"}, {0x00_0E_6A, "Dasan"},
            {0x00_15_F9, "Dasan"}, {0xF8_C0_01, "Dasan"}, {0xC8_B3_73, "Dasan"},
            // CIG / Shanghai Bell (Totalplay, Telmex GPON)
            {0x08_7A_4C, "CIG/Shanghai Bell"}, {0x58_D5_6E, "CIG/Shanghai Bell"},
            // More Huawei (Telmex HG8245/HG8546 series)
            {0x00_25_9E, "Huawei"}, {0x20_F3_A3, "Huawei"}, {0x34_6B_D3, "Huawei"},
            {0x48_AD_08, "Huawei"}, {0x4C_8B_EF, "Huawei"}, {0x54_A5_1B, "Huawei"},
            {0x60_DE_44, "Huawei"}, {0x78_F5_FD, "Huawei"}, {0x80_B6_86, "Huawei"},
            {0x84_A8_E4, "Huawei"}, {0xA8_CA_7B, "Huawei"}, {0xB4_15_13, "Huawei"},
            {0xD0_7A_B5, "Huawei"}, {0xE4_68_A3, "Huawei"}, {0xF4_C7_14, "Huawei"},
            {0xFC_48_EF, "Huawei"},
            // More ZTE (Telmex / Izzi / Axtel modems)
            {0x14_B9_68, "ZTE"}, {0x1C_7B_21, "ZTE"}, {0x34_DE_34, "ZTE"},
            {0x54_BE_53, "ZTE"}, {0x64_13_6C, "ZTE"}, {0x78_31_C1, "ZTE"},
            {0x84_74_60, "ZTE"}, {0xA0_EC_F9, "ZTE"}, {0xB0_75_D5, "ZTE"},
            {0xC8_64_C7, "ZTE"}, {0xE0_CA_94, "ZTE"}, {0xF8_4A_BF, "ZTE"},
            // More Arris (Izzi / Megacable cable modems)
            {0x00_26_D9, "Arris"}, {0x20_3D_66, "Arris"}, {0x44_E1_37, "Arris"},
            {0x6C_C1_D2, "Arris"}, {0x84_E0_58, "Arris"}, {0x90_B1_1C, "Arris"},
            {0xBC_14_01, "Arris"}, {0xE8_ED_05, "Arris"},
            // More Apple (expanded coverage)
            {0x00_03_93, "Apple"}, {0x04_0C_CE, "Apple"}, {0x10_DD_B1, "Apple"},
            {0x18_AF_61, "Apple"}, {0x24_A0_74, "Apple"}, {0x34_C0_59, "Apple"},
            {0x44_2A_60, "Apple"}, {0x58_B0_35, "Apple"}, {0x6C_94_66, "Apple"},
            {0x7C_D1_C3, "Apple"}, {0x84_38_35, "Apple"}, {0x90_8D_6C, "Apple"},
            {0x98_01_A7, "Apple"}, {0xA4_D1_8C, "Apple"}, {0xB0_19_C6, "Apple"},
            {0xC0_B6_58, "Apple"}, {0xD0_25_98, "Apple"}, {0xE0_B9_BA, "Apple"},
            {0xF0_C1_F1, "Apple"}, {0xFC_FC_48, "Apple"},
            // More Samsung (expanded)
            {0x00_21_19, "Samsung"}, {0x08_D4_2B, "Samsung"}, {0x14_49_E0, "Samsung"},
            {0x18_22_7E, "Samsung"}, {0x24_18_1D, "Samsung"}, {0x30_96_FB, "Samsung"},
            {0x38_01_97, "Samsung"}, {0x40_4E_36, "Samsung"}, {0x5C_49_7D, "Samsung"},
            {0x6C_F3_73, "Samsung"}, {0x78_BD_BC, "Samsung"}, {0x84_25_DB, "Samsung"},
            {0x94_35_0A, "Samsung"}, {0x9C_02_98, "Samsung"}, {0xAC_5F_3E, "Samsung"},
            {0xB4_3A_28, "Samsung"}, {0xC4_50_06, "Samsung"}, {0xD0_22_BE, "Samsung"},
            {0xE4_7C_F9, "Samsung"}, {0xF4_42_8F, "Samsung"},
            // More Xiaomi / Redmi
            {0x04_CF_8C, "Xiaomi"}, {0x18_59_36, "Xiaomi"}, {0x34_CE_00, "Xiaomi"},
            {0x44_23_7C, "Xiaomi"}, {0x58_44_98, "Xiaomi"}, {0x7C_1D_D9, "Xiaomi"},
            {0x8C_DE_52, "Xiaomi"}, {0xB0_E2_35, "Xiaomi"}, {0xD4_61_DA, "Xiaomi"},
            {0xEC_D0_9F, "Xiaomi"},
            // More Google / Nest / Chromecast
            {0x00_1A_11, "Google"}, {0x20_DF_B9, "Google"}, {0x30_FD_38, "Google"},
            {0x44_07_0B, "Google"}, {0x94_EB_2C, "Google"}, {0xE4_F0_42, "Google"},
        }.ToFrozenDictionary();

        /// <summary>
        /// Resolves OUI vendor from MAC address bytes in O(1).
        /// </summary>
        public static string Lookup(byte[] mac)
        {
            if (mac == null || mac.Length < 3) return null;
            int key = (mac[0] << 16) | (mac[1] << 8) | mac[2];
            return s_oui.TryGetValue(key, out var vendor) ? vendor : null;
        }

        /// <summary>
        /// Resolves OUI vendor from a ReadOnlySpan of MAC bytes in O(1).
        /// </summary>
        public static string Lookup(ReadOnlySpan<byte> mac)
        {
            if (mac.Length < 3) return null;
            int key = (mac[0] << 16) | (mac[1] << 8) | mac[2];
            return s_oui.TryGetValue(key, out var vendor) ? vendor : null;
        }

        /// <summary>
        /// Detects if a MAC address is locally administered (randomized).
        /// Modern iOS 14+ and Android 10+ randomize MAC addresses for privacy.
        /// Bit 1 of the first byte (the "locally administered" bit) is set.
        /// </summary>
        public static bool IsRandomizedMac(byte[] mac)
        {
            if (mac == null || mac.Length < 1) return false;
            return (mac[0] & 0x02) != 0;
        }
    }
}
