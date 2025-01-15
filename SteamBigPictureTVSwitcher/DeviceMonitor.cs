using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;

namespace SteamBigPictureTVSwitcher;

public record MonitoredDevice(string Id, string Name, bool IsConnected, string? InstanceId = null);

public delegate void MyEventHandler(object sender, EventArgs e);

public class DeviceMonitor : IDisposable
{
    public event TypedEventHandler<DeviceMonitor, MonitoredDevice>? DeviceAdded;
    public event TypedEventHandler<DeviceMonitor, MonitoredDevice>? DeviceRemoved;

    private readonly Task _monitoringTask;
    private List<MonitoredDevice> _currentDevices;
    private readonly CancellationToken _cancellationToken;

    public DeviceMonitor(CancellationToken cancellationToken = default)
    {
        _cancellationToken = cancellationToken;
        _currentDevices = [];
        _monitoringTask = new Task(MonitorBluetoothDevices, _cancellationToken, TaskCreationOptions.LongRunning);
    }

    public void StartMonitoring()
    {
        _monitoringTask.Start();
    }

    private void MonitorBluetoothDevices()
    {
        _currentDevices = GetConnectedBluetoothDevices();
        Console.WriteLine($"Found {_currentDevices.Count} devices - [{string.Join(",", _currentDevices.Select(device => device.Name))}]");

        while (!_cancellationToken.IsCancellationRequested)
        {
            List<MonitoredDevice> devicesInstanceIdToDevice = GetConnectedBluetoothDevices();

            devicesInstanceIdToDevice.Except(_currentDevices).ForEach(OnDeviceAdded);
            _currentDevices.Except(devicesInstanceIdToDevice).ForEach(OnDeviceRemoved);

            _currentDevices = devicesInstanceIdToDevice;
            Task.Delay(100, _cancellationToken).GetAwaiter().GetResult();
        }
    }

    private List<MonitoredDevice> GetConnectedBluetoothDevices()
    {
        return Task.Run(GetConnectedBluetoothDevicesAsync, _cancellationToken).GetAwaiter().GetResult();
    }

    private async Task<List<MonitoredDevice>> GetConnectedBluetoothDevicesAsync()
    {
        // ushort vendorId = 0x045E;
        // ushort productId = 0x0b13;
        const ushort usagePage = 0x0001;
        const ushort usageId = 0x0005;

        string xboxDeviceSelector = HidDevice.GetDeviceSelector(usagePage, usageId);
        List<DeviceInformation> xboxDevices = (await DeviceInformation.FindAllAsync(xboxDeviceSelector)).Where(device => device.IsEnabled).ToList();

        return xboxDevices.Select(ToMonitoredDevice).ToList();
    }

    private static MonitoredDevice ToMonitoredDevice(DeviceInformation device)
    {
        return new MonitoredDevice(device.Id, device.Name, device.IsEnabled, device.Properties.GetValueOrDefault("System.Devices.DeviceInstanceId", "").ToString());
    }

    private void OnDeviceAdded(MonitoredDevice args)
    {
        DeviceAdded?.Invoke(this, args);
    }

    private void OnDeviceRemoved(MonitoredDevice args)
    {
        DeviceRemoved?.Invoke(this, args);
    }

    public void Dispose()
    {
        _monitoringTask.Dispose();
    }
}