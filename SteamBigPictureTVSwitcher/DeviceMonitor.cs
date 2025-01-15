using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using DotNetEnv;

namespace SteamBigPictureTVSwitcher;

public record MonitoredDevice(string Id, string Name, bool IsConnected, string? InstanceId = null);

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

    private static async Task<List<MonitoredDevice>> GetConnectedBluetoothDevicesAsync()
    {
        var usagePage = (short)Env.GetInt("DEVICE_USAGE_PAGE", -1);
        var usageId = (short)Env.GetInt("DEVICE_USAGE_ID", -1);
        var vendorId = (short)Env.GetInt("DEVICE_VENDOR_ID", -1);
        var productId = (short)Env.GetInt("DEVICE_PRODUCT_ID", -1);

        string xboxDeviceSelector = GetDeviceSelector(usagePage, usageId, vendorId, productId);
        List<DeviceInformation> xboxDevices = (await DeviceInformation.FindAllAsync(xboxDeviceSelector)).Where(device => device.IsEnabled).ToList();

        return xboxDevices.Select(ToMonitoredDevice).ToList();
    }

    private static string GetDeviceSelector(short usagePage, short usageId, short vendorId, short productId)
    {
        if (usagePage < 0)
        {
            throw new ArgumentException($"Usage page must be set to a positive number (was {usagePage})");
        }

        if (usageId < 0)
        {
            throw new ArgumentException($"Usage page must be set to a positive number (was {usageId})");
        }

        if (vendorId == -1 || productId == -1)
        {
            return HidDevice.GetDeviceSelector((ushort)usagePage, (ushort)usageId);
        }

        return HidDevice.GetDeviceSelector((ushort)usagePage, (ushort)usageId, (ushort)vendorId, (ushort)productId);
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