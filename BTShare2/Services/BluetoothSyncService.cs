using System;
using System.Collections.Generic;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using BTShare2.Callbacks;
using Java.Util;

namespace BTShare2.Services
{
    public class BluetoothSyncService : Service
    {
        private bool started = false;
        private UUID serviceUuidAndAddress;
        public BluetoothAdapter bluetoothAdapter;
        private BluetoothLeAdvertiser bluetoothLeAdvertiser;
        private BluetoothLeScanner bluetoothLeScanner;
        BluetoothClassicServer bluetoothClassicServer;

        public List<string> openConnections;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        // TODO: On some phones, this incorrectly returns false when the Bluetooth radio is off even though BLE advertise is supported
        public static bool IsSupported(Context context)
        {
            PackageManager packageManager = context.PackageManager;
            if (!packageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe) || !packageManager.HasSystemFeature(PackageManager.FeatureBluetooth))
                return false;

            BluetoothManager bluetoothManager = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
            BluetoothAdapter bluetoothAdapter = bluetoothManager.Adapter;
            return bluetoothAdapter != null && Helper.GetBluetoothAdapterAddress(bluetoothAdapter) != null && bluetoothAdapter.IsMultipleAdvertisementSupported;
        }

        public static bool IsStartable(Context context)
        {
            if (!IsSupported(context))
                return false;

            BluetoothManager bluetoothManager = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
            BluetoothAdapter bluetoothAdapter = bluetoothManager.Adapter;
            return bluetoothAdapter != null && bluetoothAdapter.IsEnabled;
        }

        public static void StartOrPromptBluetooth(Context context)
        {
            if (!BluetoothSyncService.IsSupported(context))
            {
                Log.Debug(Constants.TAG, "BLE not supported, not starting BLE sync service");
                Toast.MakeText(context, "Bluetooth not supported", ToastLength.Long).Show();
                return;
            }

            if (BluetoothSyncService.IsStartable(context))
            {
                Log.Debug(Constants.TAG, "BLE supported and Bluetooth is on; starting BLE sync service");
                context.StartService(new Intent(context, typeof(BluetoothSyncService)));
            }
            else
            {
                Log.Debug(Constants.TAG, "BLE supported but Bluetooth is off; will prompt for Bluetooth and start once it's on");
                Toast.MakeText(context, "Enable bluetooth to sync data.", ToastLength.Long).Show();
                context.StartActivity(new Intent(BluetoothAdapter.ActionRequestEnable));
                // BluetoothSyncServiceManager will start this service once Bluetooth is on.
            }
        }

        private void StartBluetoothLeDiscovery(int startId)
        {
            StartBluetoothLeAdvertise();

            ScanSettings.Builder builder = new ScanSettings.Builder();
            builder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M /* Marshmallow */)
            {
                builder.SetMatchMode(BluetoothScanMatchMode.Aggressive);
                builder.SetNumOfMatches((int)BluetoothScanMatchNumber.MaxAdvertisement);
                builder.SetCallbackType(ScanCallbackType.AllMatches);
            }

            var settings = builder.Build();

            MyScanCallback myScanCallback = new MyScanCallback(this);

            bluetoothLeScanner.StartScan(null, settings, myScanCallback);
        }

        private void StartBluetoothLeAdvertise()
        {
            //BluetoothLeAdvertiser advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;

            AdvertiseSettings settings = new AdvertiseSettings.Builder()
                    .SetAdvertiseMode(AdvertiseMode.LowPower)
                    //.SetTxPowerLevel(AdvertiseTx.PowerHigh)
                    .SetTimeout(0)
                    .SetConnectable(false)
                    .Build();

            ParcelUuid pUuid = new ParcelUuid(serviceUuidAndAddress);

            AdvertiseData data = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(false)
                    .AddServiceUuid(pUuid)
                    //.AddServiceData(pUuid, Encoding.UTF8.GetBytes("Data send"))
                    .Build();

            MyAdvertiseCallback myAdvertiseCallback = new MyAdvertiseCallback();

            bluetoothLeAdvertiser.StartAdvertising(settings, data, myAdvertiseCallback);

        }

        private void StopBluetoothLeDiscovery()
        {
            if (bluetoothLeAdvertiser != null)
            {
                bluetoothLeAdvertiser.StopAdvertising(new MyAdvertiseCallback());
            }

            bluetoothLeScanner.StopScan(new MyScanCallback(this));
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            if (!IsStartable(this))
            {
                Log.Error(Constants.TAG, "Trying to start the service even though Bluetooth is off or BLE is unsupported");
                StopSelf(startId);
                return StartCommandResult.NotSticky;
            }

            if (started)
            {
                Log.Debug(Constants.TAG, "Started again");
                return StartCommandResult.Sticky;
            }

            BluetoothManager bluetoothManager = (BluetoothManager)GetSystemService(Context.BluetoothService);
            bluetoothAdapter = bluetoothManager.Adapter;

            // First half identifies that the advertisement is for Noise.
            // Second half is the MAC address of this device's Bluetooth adapter so that clients know how to connect to it.
            // These are not listed separately in the advertisement because a UUID is 16 bytes and ads are limited to 31 bytes.
            string macAddress = Helper.GetBluetoothAdapterAddress(bluetoothAdapter);
            if (macAddress == null)
            {
                Log.Error(Constants.TAG, "Unable to get this device's Bluetooth MAC address");
                StopSelf(startId);
                return StartCommandResult.NotSticky;
            }
            serviceUuidAndAddress = new UUID(Constants.SERVICE_UUID_HALF.MostSignificantBits, Helper.LongFromMacAddress(macAddress));

            bluetoothLeAdvertiser = bluetoothAdapter.BluetoothLeAdvertiser;
            bluetoothLeScanner = bluetoothAdapter.BluetoothLeScanner;
            StartBluetoothLeDiscovery(startId);

            started = true;
            bluetoothClassicServer = new BluetoothClassicServer(this, serviceUuidAndAddress);
            bluetoothClassicServer.Run();

            openConnections = new List<string>();

            Log.Debug(Constants.TAG, "Started");
            Toast.MakeText(this, "Bluetooth sync started", ToastLength.Long).Show();
            
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            StopBluetoothLeDiscovery();

            // TODO: Verify that this actually stops the thread
            bluetoothClassicServer.UnregisterFromRuntime(); //Java userd inturrept()

            // TODO: Stop all BluetoothClassicClient threads

            Toast.MakeText(this, "Bluetooth sync stopped", ToastLength.Long).Show();
            started = false;
            Log.Debug(Constants.TAG, "Stopped");
            base.OnDestroy();
        }
    }
}