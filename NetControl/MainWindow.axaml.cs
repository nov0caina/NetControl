using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SharpPcap.LibPcap;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NetControl
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CArp _engine;
        private IDeviceIdentifierService _identifierService;
        public ObservableCollection<PC> DetectedPCs { get; set; }

        // ── Dashboard Metrics Properties ──

        private int _totalDevicesCount = 0;
        public int TotalDevicesCount
        {
            get => _totalDevicesCount;
            set
            {
                if (_totalDevicesCount != value)
                {
                    _totalDevicesCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasNoDevices));
                    OnPropertyChanged(nameof(HasDevices));
                }
            }
        }

        public bool HasNoDevices => _totalDevicesCount == 0;
        public bool HasDevices => _totalDevicesCount > 0;

        private int _spoofedCount = 0;
        public int SpoofedCount
        {
            get => _spoofedCount;
            set { if (_spoofedCount != value) { _spoofedCount = value; OnPropertyChanged(); } }
        }

        private int _blockedCount = 0;
        public int BlockedCount
        {
            get => _blockedCount;
            set { if (_blockedCount != value) { _blockedCount = value; OnPropertyChanged(); } }
        }

        private string _gatewayIpDisplay = "—";
        public string GatewayIpDisplay
        {
            get => _gatewayIpDisplay;
            set { if (_gatewayIpDisplay != value) { _gatewayIpDisplay = value; OnPropertyChanged(); } }
        }

        // ── Warning & Disclaimer Banner Properties ──

        private bool _isElevationWarningVisible;
        public bool IsElevationWarningVisible
        {
            get => _isElevationWarningVisible;
            set { if (_isElevationWarningVisible != value) { _isElevationWarningVisible = value; OnPropertyChanged(); } }
        }

        private string _elevationGuidance = string.Empty;
        public string ElevationGuidance
        {
            get => _elevationGuidance;
            set { if (_elevationGuidance != value) { _elevationGuidance = value; OnPropertyChanged(); } }
        }

        private bool _isDisclaimerVisible = true;
        public bool IsDisclaimerVisible
        {
            get => _isDisclaimerVisible;
            set { if (_isDisclaimerVisible != value) { _isDisclaimerVisible = value; OnPropertyChanged(); } }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Proactively verify privileges and capabilities
            bool elevated = PrivilegeDetector.IsElevated();
            bool hasCaps = PrivilegeDetector.HasCaptureCapabilities();
            if (!elevated && !hasCaps)
            {
                IsElevationWarningVisible = true;
                ElevationGuidance = PrivilegeDetector.GetElevationGuidance();
            }

            DetectedPCs = new ObservableCollection<PC>();
            DetectedPCs.CollectionChanged += OnDetectedPcsCollectionChanged;

            var listBox = this.FindControl<ListBox>("PcListBox");
            if (listBox != null)
            {
                listBox.ItemsSource = DetectedPCs;
            }

            AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();

            try
            {
                System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += (s, e) =>
                {
                    Dispatcher.UIThread.InvokeAsync(LoadInterfaces);
                };
            }
            catch { }

            LoadInterfaces();

            if (IsElevationWarningVisible)
            {
                SetStatus("Warning: Superuser privileges or CAP_NET_RAW required.", "warning");
            }
        }

        public void OnDismissDisclaimerClick(object sender, RoutedEventArgs e)
        {
            IsDisclaimerVisible = false;
        }

        private void OnDetectedPcsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PC pc in e.NewItems)
                {
                    pc.PropertyChanged += OnPcPropertyChanged;
                    if (pc.IsGateway && pc.Ip != null)
                    {
                        GatewayIpDisplay = pc.Ip.ToString();
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (PC pc in e.OldItems)
                {
                    pc.PropertyChanged -= OnPcPropertyChanged;
                }
            }

            RecalculateDashboardMetrics();
        }

        private void OnPcPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PC.Redirect) ||
                e.PropertyName == nameof(PC.Block) ||
                e.PropertyName == nameof(PC.IsGateway))
            {
                RecalculateDashboardMetrics();
            }
        }

        private void RecalculateDashboardMetrics()
        {
            TotalDevicesCount = DetectedPCs.Count;
            SpoofedCount = DetectedPCs.Count(p => p.Redirect);
            BlockedCount = DetectedPCs.Count(p => p.Block);

            var gateway = DetectedPCs.FirstOrDefault(p => p.IsGateway);
            if (gateway?.Ip != null)
            {
                GatewayIpDisplay = gateway.Ip.ToString();
            }
        }

        private void LoadInterfaces()
        {
            try
            {
                var devices = SharpPcap.CaptureDeviceList.Instance;
                var validDevices = devices.OfType<LibPcapLiveDevice>().Where(d => d.Addresses.Any(a =>
                    a.Addr.ipAddress != null &&
                    a.Addr.ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)).ToList();

                var combo = this.FindControl<ComboBox>("NetInterfacesBox");
                if (combo != null)
                {
                    combo.ItemsSource = validDevices;
                    combo.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

                    if (validDevices.Any())
                    {
                        combo.SelectedIndex = 0;
                        SetStatus($"{validDevices.Count} interface(s) detected.", "success");
                    }
                    else
                    {
                        SetStatus("No valid network interfaces detected.", "warning");
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading interfaces: {ex.Message}", "error");
            }
        }

        public async void OnScanClick(object sender, RoutedEventArgs e)
        {
            var combo = this.FindControl<ComboBox>("NetInterfacesBox");
            var device = combo?.SelectedItem as LibPcapLiveDevice;
            if (device == null)
            {
                SetStatus("Select a network interface first.", "warning");
                return;
            }

            var btnScan = this.FindControl<Button>("BtnScan");
            var btnSpoof = this.FindControl<Button>("BtnSpoof");
            var spoofText = this.FindControl<TextBlock>("SpoofButtonText");
            var spoofIcon = this.FindControl<Avalonia.Controls.PathIcon>("SpoofButtonIcon");

            if (btnScan != null) btnScan.IsEnabled = false;
            if (btnSpoof != null) btnSpoof.IsEnabled = false;

            // If engine exists, stop previous operations asynchronously
            if (_engine != null || _identifierService != null)
            {
                SetStatus("Stopping previous scan and releasing network resources...", "warning");
                var oldEngine = _engine;
                var oldIdService = _identifierService;
                if (oldEngine != null)
                {
                    oldEngine.OnInterfaceDisconnected -= OnEngineInterfaceDisconnected;
                }
                _engine = null;
                _identifierService = null;

                await Task.Run(() =>
                {
                    try
                    {
                        oldIdService?.StopMdnsListener();
                        oldIdService?.Dispose();
                        oldEngine?.StopArpListener();
                        oldEngine?.StopDiscovery();
                        oldEngine?.StopSpoofing();
                        oldEngine?.StopTrafficMonitor();
                        oldEngine?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CLEANUP ERROR] {ex.Message}");
                    }
                });

                if (btnSpoof != null)
                {
                    btnSpoof.Classes.Clear();
                    btnSpoof.Classes.Add("danger");
                }
                if (spoofText != null) spoofText.Text = "START ARP SPOOF";
                if (spoofIcon != null && Application.Current?.Resources.TryGetResource("IconZap", null, out var zapGeom) == true)
                {
                    spoofIcon.Data = zapGeom as Geometry;
                }
            }

            DetectedPCs.Clear();
            GatewayIpDisplay = "—";

            try
            {
                SetStatus("Initializing network scan...", "info");

                var deviceList = new PcList();
                deviceList.SetOnDeviceAdded((newPc) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        DetectedPCs.Add(newPc);
                        SetStatus($"{DetectedPCs.Count} device(s) detected on the network.", "success");
                    });
                });

                CArp newEngine = null;
                IDeviceIdentifierService newIdService = null;

                await Task.Run(() =>
                {
                    newEngine = new CArp(device, deviceList);
                    newIdService = new DeviceIdentifierService(device, deviceList);
                    deviceList.SetIdentifierService(newIdService);

                    newEngine.StartArpListener();
                    newEngine.StartArpDiscovery();
                    newEngine.StartTrafficMonitor();
                    newIdService.StartMdnsListener();
                });

                _engine = newEngine;
                _identifierService = newIdService;
                _engine.OnInterfaceDisconnected += OnEngineInterfaceDisconnected;

                // Show detected network info in metadata footer
                string localIp = _engine.LocalIp != null ? new System.Net.IPAddress(_engine.LocalIp).ToString() : "N/A";
                string routerIp = _engine.RouterIp != null ? new System.Net.IPAddress(_engine.RouterIp).ToString() : "N/A";
                string subnet = _engine.SubnetMask != null ? new System.Net.IPAddress(_engine.SubnetMask).ToString() : "/24 (fallback)";

                if (!string.IsNullOrEmpty(routerIp) && routerIp != "N/A")
                {
                    GatewayIpDisplay = routerIp;
                }

                var localIpText = this.FindControl<TextBlock>("LocalIpText");
                if (localIpText != null) localIpText.Text = $"Local: {localIp}";

                var subnetText = this.FindControl<TextBlock>("SubnetText");
                if (subnetText != null) subnetText.Text = $"Subnet: {subnet}";

                SetStatus($"Active scan on {device.Name}", "info");
            }
            catch (Exception ex)
            {
                SetStatus($"Error starting scan: {ex.Message}", "error");
                Console.WriteLine($"[SCAN ERROR] {ex}");
            }
            finally
            {
                if (btnScan != null) btnScan.IsEnabled = true;
                if (btnSpoof != null) btnSpoof.IsEnabled = true;
            }
        }

        public async void OnSpoofClick(object sender, RoutedEventArgs e)
        {
            if (!PrivilegeDetector.HasCaptureCapabilities())
            {
                SetStatus("Cannot start spoof: superuser (root/admin) or CAP_NET_RAW required.", "error");
                IsElevationWarningVisible = true;
                return;
            }

            if (_engine == null)
            {
                SetStatus("Scan the network before starting spoof.", "warning");
                return;
            }

            var btn = this.FindControl<Button>("BtnSpoof");
            var spoofText = this.FindControl<TextBlock>("SpoofButtonText");
            var spoofIcon = this.FindControl<Avalonia.Controls.PathIcon>("SpoofButtonIcon");
            if (btn == null) return;
            btn.IsEnabled = false;

            try
            {
                bool isStarting = spoofText?.Text?.Contains("START") == true;

                if (isStarting)
                {
                    if (_engine.RouterMac == null)
                    {
                        SetStatus("Router MAC not detected. Scan again.", "error");
                        return;
                    }

                    await Task.Run(() => _engine.StartSpoofing());

                    btn.Classes.Clear();
                    btn.Classes.Add("active-stop");
                    if (spoofText != null) spoofText.Text = "STOP SPOOF";
                    if (spoofIcon != null && Application.Current?.Resources.TryGetResource("IconStopCircle", null, out var stopGeom) == true)
                    {
                        spoofIcon.Data = stopGeom as Geometry;
                    }

                    SetStatus("ARP Spoof active — intercepting traffic.", "error");
                }
                else
                {
                    SetStatus("Stopping spoof and restoring network tables...", "warning");
                    await Task.Run(() => _engine.StopSpoofing());

                    btn.Classes.Clear();
                    btn.Classes.Add("danger");
                    if (spoofText != null) spoofText.Text = "START ARP SPOOF";
                    if (spoofIcon != null && Application.Current?.Resources.TryGetResource("IconZap", null, out var zapGeom) == true)
                    {
                        spoofIcon.Data = zapGeom as Geometry;
                    }

                    SetStatus("Spoof stopped. Network restored.", "success");
                }
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Updates the status pill with a message and semantic level (success, info, warning, error).
        /// Thread-safe: can be called from any thread.
        /// </summary>
        private void SetStatus(string message, string level = "info")
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                var status = this.FindControl<TextBlock>("StatusText");
                var dot = this.FindControl<Avalonia.Controls.Shapes.Ellipse>("StatusDot");
                var border = this.FindControl<Border>("StatusPillBorder");

                if (status != null)
                {
                    status.Text = message;
                }

                Color dotColor;
                Color borderBg;

                switch (level.ToLowerInvariant())
                {
                    case "success":
                        dotColor = Color.Parse("#3FB950");
                        borderBg = Color.Parse("#161B22");
                        break;
                    case "warning":
                        dotColor = Color.Parse("#D29922");
                        borderBg = Color.Parse("#271E0B");
                        break;
                    case "error":
                        dotColor = Color.Parse("#F85149");
                        borderBg = Color.Parse("#261214");
                        break;
                    case "info":
                    default:
                        dotColor = Color.Parse("#58A6FF");
                        borderBg = Color.Parse("#161B22");
                        break;
                }

                if (dot != null)
                {
                    dot.Fill = new SolidColorBrush(dotColor);
                }
                if (border != null)
                {
                    border.Background = new SolidColorBrush(borderBg);
                }
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() => SetStatus(message, level));
            }
        }

        private void OnEngineInterfaceDisconnected(string reason)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SetStatus($"Network disconnected: {reason}", "error");

                var btnSpoof = this.FindControl<Button>("BtnSpoof");
                var spoofText = this.FindControl<TextBlock>("SpoofButtonText");
                var spoofIcon = this.FindControl<Avalonia.Controls.PathIcon>("SpoofButtonIcon");

                if (btnSpoof != null)
                {
                    btnSpoof.Classes.Clear();
                    btnSpoof.Classes.Add("danger");
                    btnSpoof.IsEnabled = true;
                }
                if (spoofText != null) spoofText.Text = "START ARP SPOOF";
                if (spoofIcon != null && Application.Current?.Resources.TryGetResource("IconZap", null, out var zapGeom) == true)
                {
                    spoofIcon.Data = zapGeom as Geometry;
                }

                LoadInterfaces();
            });
        }

        private void Cleanup()
        {
            try
            {
                if (_engine != null)
                {
                    _engine.OnInterfaceDisconnected -= OnEngineInterfaceDisconnected;
                }
                _identifierService?.StopMdnsListener();
                _identifierService?.Dispose();
                _identifierService = null;
                _engine?.Dispose();
                _engine = null;
            }
            catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            Cleanup();
            base.OnClosed(e);
        }

        // ── INotifyPropertyChanged ──

        public new event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}