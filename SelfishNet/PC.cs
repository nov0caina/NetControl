using System;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Threading;

namespace SelfishNet
{
    public class PC : INotifyPropertyChanged
    {
        // ── Display properties (XAML bindings) ──

        public string IpDisplay => Ip?.ToString() ?? "—";
        public string MacDisplay => Mac?.ToString() ?? "—";

        /// <summary>
        /// Formatted colon-delimited MAC address (e.g. "74:D8:3E:6E:1B:C6") for tabular readability.
        /// </summary>
        public string MacFormatted
        {
            get
            {
                if (Mac == null) return "—";
                string macStr = Mac.ToString();
                if (macStr.Length == 12)
                {
                    return $"{macStr[0..2]}:{macStr[2..4]}:{macStr[4..6]}:{macStr[6..8]}:{macStr[8..10]}:{macStr[10..12]}";
                }
                return macStr;
            }
        }

        // ── Core network properties ──

        private IPAddress _ip;
        public IPAddress Ip
        {
            get => _ip;
            set { if (_ip != value) { _ip = value; OnPropertyChanged(); OnPropertyChanged(nameof(IpDisplay)); } }
        }

        private PhysicalAddress _mac;
        public PhysicalAddress Mac
        {
            get => _mac;
            set { if (_mac != value) { _mac = value; OnPropertyChanged(); OnPropertyChanged(nameof(MacDisplay)); OnPropertyChanged(nameof(MacFormatted)); OnPropertyChanged(nameof(DeviceLabel)); } }
        }

        public string Name { get; set; } = "Unknown";

        // ── Device identification ──

        private string _vendor;
        public string Vendor
        {
            get => _vendor;
            set { if (_vendor != value) { _vendor = value; OnPropertyChanged(); OnPropertyChanged(nameof(DeviceLabel)); } }
        }

        private string _hostname;
        public string Hostname
        {
            get => _hostname;
            set { if (_hostname != value) { _hostname = value; OnPropertyChanged(); OnPropertyChanged(nameof(DeviceLabel)); } }
        }

        private DeviceType _deviceCategory = DeviceType.Unknown;
        public DeviceType DeviceCategory
        {
            get => _deviceCategory;
            set { if (_deviceCategory != value) { _deviceCategory = value; OnPropertyChanged(); OnPropertyChanged(nameof(DeviceLabel)); OnPropertyChanged(nameof(CategoryName)); } }
        }

        public string CategoryName => _deviceCategory != DeviceType.Unknown ? _deviceCategory.ToString() : "Device";

        /// <summary>
        /// Computed display label for UI: combines device type, vendor, and hostname.
        /// Falls back to formatted MAC address when no identification is available.
        /// </summary>
        public string DeviceLabel
        {
            get
            {
                string type = _deviceCategory != DeviceType.Unknown ? _deviceCategory.ToString() : null;
                string name = !string.IsNullOrEmpty(_hostname) ? _hostname.Trim() : null;
                bool isRandomized = string.Equals(_vendor, "Randomized MAC", StringComparison.Ordinal);
                string vendor = !string.IsNullOrEmpty(_vendor) && !isRandomized ? _vendor.Trim() : null;

                bool vendorRedundant = false;
                if (name != null && vendor != null)
                {
                    if (name.Contains(vendor, StringComparison.OrdinalIgnoreCase) ||
                        vendor.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        vendorRedundant = true;
                    }
                    else
                    {
                        var tokens = vendor.Split(new[] { ' ', ',', '-', '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length > 0 && tokens[0].Length >= 3 && name.Contains(tokens[0], StringComparison.OrdinalIgnoreCase))
                        {
                            vendorRedundant = true;
                        }
                    }
                }

                string rawLabel;
                if (type != null && name != null && vendor != null)
                {
                    if (vendorRedundant)
                        rawLabel = $"{type} — {name}";
                    else
                        rawLabel = $"{type} — {name} ({vendor})";
                }
                else if (type != null && name != null)
                {
                    rawLabel = $"{type} — {name}";
                }
                else if (type != null && vendor != null)
                {
                    rawLabel = $"{type} ({vendor})";
                }
                else if (name != null && vendor != null)
                {
                    if (vendorRedundant)
                        rawLabel = name;
                    else
                        rawLabel = $"{name} ({vendor})";
                }
                else if (name != null)
                {
                    rawLabel = name;
                }
                else if (vendor != null)
                {
                    rawLabel = vendor;
                }
                else if (Mac != null)
                {
                    string macStr = Mac.ToString();
                    if (macStr.Length == 12)
                    {
                        string prefix = isRandomized ? "Randomized " : "";
                        rawLabel = $"{prefix}{macStr[0..2]}:{macStr[2..4]}:{macStr[4..6]}:{macStr[6..8]}:{macStr[8..10]}:{macStr[10..12]}";
                    }
                    else
                    {
                        rawLabel = "Unknown Device";
                    }
                }
                else
                {
                    rawLabel = "Unknown Device";
                }

                return TruncateWithEllipsis(rawLabel, 55);
            }
        }

        private static string TruncateWithEllipsis(string value, int maxLen = 55)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLen) return value;
            return string.Concat(value.AsSpan(0, maxLen - 1), "…");
        }

        private bool _redirect = false;
        public bool Redirect
        {
            get => _redirect;
            set
            {
                if (CanControl && _redirect != value)
                {
                    _redirect = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsThrottlingEnabled));
                    if (_redirect && _block)
                    {
                        _block = false;
                        OnPropertyChanged(nameof(Block));
                    }
                }
            }
        }

        private bool _block = false;
        public bool Block
        {
            get => _block;
            set
            {
                if (CanControl && _block != value)
                {
                    _block = value;
                    OnPropertyChanged();
                    if (_block && _redirect)
                    {
                        _redirect = false;
                        OnPropertyChanged(nameof(Redirect));
                        OnPropertyChanged(nameof(IsThrottlingEnabled));
                    }
                }
            }
        }

        private int _bandwidthLimitKb = 0;
        /// <summary>Allowed bandwidth limit in KB/s (0 = Unlimited).</summary>
        public int BandwidthLimitKb
        {
            get => _bandwidthLimitKb;
            set
            {
                int clamped = Math.Max(0, value);
                if (_bandwidthLimitKb != clamped)
                {
                    _bandwidthLimitKb = clamped;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BandwidthLimitDisplay));
                }
            }
        }

        public string BandwidthLimitDisplay => _bandwidthLimitKb == 0 ? "Unlimited" : $"{_bandwidthLimitKb} KB/s";

        private bool _isGateway;
        public bool IsGateway
        {
            get => _isGateway;
            set
            {
                if (_isGateway != value)
                {
                    _isGateway = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanControl));
                    OnPropertyChanged(nameof(IsThrottlingEnabled));
                    OnPropertyChanged(nameof(HasRoleBadge));
                    OnPropertyChanged(nameof(RoleBadgeText));
                }
            }
        }

        private bool _isLocalPc;
        public bool IsLocalPc
        {
            get => _isLocalPc;
            set
            {
                if (_isLocalPc != value)
                {
                    _isLocalPc = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanControl));
                    OnPropertyChanged(nameof(IsThrottlingEnabled));
                    OnPropertyChanged(nameof(HasRoleBadge));
                    OnPropertyChanged(nameof(RoleBadgeText));
                }
            }
        }

        public bool HasRoleBadge => IsLocalPc || IsGateway;
        public string RoleBadgeText => IsLocalPc ? "YOU" : (IsGateway ? "GATEWAY" : "");

        public bool CanControl => !IsLocalPc && !IsGateway;

        public bool IsThrottlingEnabled => CanControl && _redirect;

        // ── Tracking ──

        public DateTime TimeSinceLastArp { get; set; }

        private long _bytesSent;
        private long _bytesReceived;

        /// <summary>Bytes sent in current monitoring cycle.</summary>
        public long BytesSent
        {
            get => Interlocked.Read(ref _bytesSent);
            set => Interlocked.Exchange(ref _bytesSent, value);
        }

        /// <summary>Bytes received in current monitoring cycle.</summary>
        public long BytesReceived
        {
            get => Interlocked.Read(ref _bytesReceived);
            set => Interlocked.Exchange(ref _bytesReceived, value);
        }

        public void AddBytesSent(long count) => Interlocked.Add(ref _bytesSent, count);
        public void AddBytesReceived(long count) => Interlocked.Add(ref _bytesReceived, count);

        public (long rx, long tx) ResetByteAccumulators()
        {
            long rx = Interlocked.Exchange(ref _bytesReceived, 0);
            long tx = Interlocked.Exchange(ref _bytesSent, 0);
            return (rx, tx);
        }

        // ── Smoothed Rate Tracking Properties ──

        private double _downloadSpeedKbps = 0.0;
        public double DownloadSpeedKbps
        {
            get => _downloadSpeedKbps;
            set
            {
                if (Math.Abs(_downloadSpeedKbps - value) > 0.01)
                {
                    _downloadSpeedKbps = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DownloadSpeedFormatted));
                    OnPropertyChanged(nameof(DownloadSpeed));
                    OnPropertyChanged(nameof(HasActiveTraffic));
                    OnPropertyChanged(nameof(HasActiveDownload));
                    OnPropertyChanged(nameof(DownloadTrafficBrushHex));
                    OnPropertyChanged(nameof(TrafficBrushHex));
                }
            }
        }

        private double _uploadSpeedKbps = 0.0;
        public double UploadSpeedKbps
        {
            get => _uploadSpeedKbps;
            set
            {
                if (Math.Abs(_uploadSpeedKbps - value) > 0.01)
                {
                    _uploadSpeedKbps = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UploadSpeedFormatted));
                    OnPropertyChanged(nameof(DownloadSpeed));
                    OnPropertyChanged(nameof(HasActiveTraffic));
                    OnPropertyChanged(nameof(HasActiveUpload));
                    OnPropertyChanged(nameof(UploadTrafficBrushHex));
                    OnPropertyChanged(nameof(TrafficBrushHex));
                }
            }
        }

        public string DownloadSpeedFormatted => FormatSpeed(_downloadSpeedKbps);
        public string UploadSpeedFormatted => FormatSpeed(_uploadSpeedKbps);

        // ── Bindable download / combined speed ──

        private string _downloadSpeed = "—";
        public string DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                if (_downloadSpeed != value)
                {
                    _downloadSpeed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasActiveTraffic));
                    OnPropertyChanged(nameof(TrafficBrushHex));
                }
            }
        }

        public bool HasActiveTraffic => _downloadSpeedKbps >= 0.1 || _uploadSpeedKbps >= 0.1 || (!string.IsNullOrEmpty(_downloadSpeed) && _downloadSpeed != "—" && !_downloadSpeed.Contains("0 KB/s"));
        public bool HasActiveDownload => _downloadSpeedKbps >= 0.1;
        public bool HasActiveUpload => _uploadSpeedKbps >= 0.1;

        public string DownloadTrafficBrushHex => HasActiveDownload ? "#3FB950" : "#6E7681";
        public string UploadTrafficBrushHex => HasActiveUpload ? "#58A6FF" : "#6E7681";
        public string TrafficBrushHex => HasActiveTraffic ? "#3FB950" : "#6E7681";

        public void UpdateTransferRates(double downloadKbps, double uploadKbps)
        {
            double down = downloadKbps < 0.05 ? 0.0 : downloadKbps;
            double up = uploadKbps < 0.05 ? 0.0 : uploadKbps;

            string prevDownText = FormatSpeed(_downloadSpeedKbps);
            string prevUpText = FormatSpeed(_uploadSpeedKbps);
            string newDownText = FormatSpeed(down);
            string newUpText = FormatSpeed(up);

            bool textChanged = prevDownText != newDownText || prevUpText != newUpText;
            bool rateChanged = Math.Abs(_downloadSpeedKbps - down) > 0.05 || Math.Abs(_uploadSpeedKbps - up) > 0.05;

            if (textChanged || rateChanged)
            {
                _downloadSpeedKbps = down;
                _uploadSpeedKbps = up;
                _downloadSpeed = $"↓{newDownText} ↑{newUpText}";

                OnPropertyChanged(nameof(DownloadSpeedKbps));
                OnPropertyChanged(nameof(UploadSpeedKbps));
                OnPropertyChanged(nameof(DownloadSpeedFormatted));
                OnPropertyChanged(nameof(UploadSpeedFormatted));
                OnPropertyChanged(nameof(DownloadSpeed));
                OnPropertyChanged(nameof(HasActiveTraffic));
                OnPropertyChanged(nameof(HasActiveDownload));
                OnPropertyChanged(nameof(HasActiveUpload));
                OnPropertyChanged(nameof(DownloadTrafficBrushHex));
                OnPropertyChanged(nameof(UploadTrafficBrushHex));
                OnPropertyChanged(nameof(TrafficBrushHex));
            }
        }

        public static string FormatSpeed(double kbps)
        {
            if (kbps < 0.1) return "0 KB/s";
            if (kbps >= 1024.0) return $"{kbps / 1024.0:F1} MB/s";
            return $"{kbps:F1} KB/s";
        }

        // ── INotifyPropertyChanged ──

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            if (Dispatcher.UIThread != null && !Dispatcher.UIThread.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Post(() => handler(this, new PropertyChangedEventArgs(propertyName)), DispatcherPriority.Background);
                }
                catch
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}