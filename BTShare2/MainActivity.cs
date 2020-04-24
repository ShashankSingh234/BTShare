using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Bluetooth.LE;
using System.Collections.Generic;
using Android.Bluetooth;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;
using Android.Content;
using BTShare2.Helpers;
using BTShare2.Services;
using BTShare2.Callbacks;
using System.Text;

namespace BTShare2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public TextView mSuccessCountText;
        public TextView mFailedCountText;
        public TextView mText;
        public int success = 0;
        public int failed = 0;
        private Button mAdvertiseButton;
        private Button mDiscoverButton;

        BluetoothAdapter bluetoothAdapter;

        BluetoothLeScanner bluetoothLeScanner;
        BluetoothLeAdvertiser bluetoothLeAdvertiser;

        MyScanCallback myScanCallback;
        MyAdvertiseCallback myAdvertiseCallback;

        private Handler mHandler = new Handler();

        BluetoothChatService bluetoothChatService;

        int REQUEST_ENABLE_BT = 1;

        public BluetoothGatt bluetoothGatt;
        public BluetoothGattServer bluetoothGattServer;
        private BluetoothManager mBluetoothManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            AppCenter.Start("5c06e9f4-aa80-40ff-8cf4-f957345a44c5",
                   typeof(Analytics), typeof(Crashes));

            mSuccessCountText = FindViewById<TextView>(Resource.Id.successCountText);
            mFailedCountText = FindViewById<TextView>(Resource.Id.failedCountText);
            mText = FindViewById<TextView>(Resource.Id.text);
            mDiscoverButton = FindViewById<Button>(Resource.Id.discover_btn);
            mAdvertiseButton = FindViewById<Button>(Resource.Id.advertise_btn);

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available.", ToastLength.Long).Show();
                FinishAndRemoveTask();
            }

            bluetoothChatService = new BluetoothChatService(this);

            bluetoothLeScanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;

            mBluetoothManager = (BluetoothManager)GetSystemService(Context.BluetoothService);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (!bluetoothAdapter.IsEnabled)
            {
                var enableIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableIntent, REQUEST_ENABLE_BT);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            mDiscoverButton.Click += MDiscoverButton_Click;
            mAdvertiseButton.Click += MAdvertiseButton_Click;

            //if (bluetoothChatService != null)
            //{
            //    if (bluetoothChatService.GetState() == Constants.STATE_NONE)
            //    {
            //        bluetoothChatService.Start();
            //    }
            //}
        }

        protected override void OnPause()
        {
            base.OnPause();
            mDiscoverButton.Click -= MDiscoverButton_Click;
            mAdvertiseButton.Click -= MAdvertiseButton_Click;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (bluetoothLeAdvertiser != null && myAdvertiseCallback != null)
                bluetoothLeAdvertiser.StopAdvertising(myAdvertiseCallback);

            if (bluetoothLeScanner != null && myScanCallback != null)
                bluetoothLeScanner.StopScan(myScanCallback);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == REQUEST_ENABLE_BT && !bluetoothAdapter.IsMultipleAdvertisementSupported)
            {
                Toast.MakeText(this, "Multiple advertisement not supported", ToastLength.Long).Show();
                mAdvertiseButton.Enabled = false;
                mDiscoverButton.Enabled = false;
            }
        }

        private BluetoothGattService CreateService()
        {
            BluetoothGattService service = new BluetoothGattService(Constants.MY_UUID, GattServiceType.Primary);

            // Counter characteristic (read-only, supports subscriptions)
            BluetoothGattCharacteristic counter = new BluetoothGattCharacteristic(Constants.CHARACTERISTIC_COUNTER_UUID, GattProperty.Read | GattProperty.Notify, GattPermission.Read);
            BluetoothGattDescriptor counterConfig = new BluetoothGattDescriptor(Constants.DESCRIPTOR_CONFIG_UUID, GattDescriptorPermission.Read | GattDescriptorPermission.Write);
            counter.AddDescriptor(counterConfig);

            // Interactor characteristic
            BluetoothGattCharacteristic interactor = new BluetoothGattCharacteristic(Constants.CHARACTERISTIC_INTERACTOR_UUID, GattProperty.WriteNoResponse, GattPermission.Write);

            service.AddCharacteristic(counter);
            service.AddCharacteristic(interactor);
            return service;
        }

        public void ConnectDevice(string info, bool secure)
        {
            var address = info.Substring(info.Length - 17);//data.Extras.GetString(DeviceListScanner.EXTRA_DEVICE_ADDRESS);
            var device = bluetoothAdapter.GetRemoteDevice(address);
            bluetoothChatService.Connect(device, secure);
        }

        private void MAdvertiseButton_Click(object sender, System.EventArgs e)
        {
            Advertise();
        }

        private void MDiscoverButton_Click(object sender, System.EventArgs e)
        {
            Discover();
        }

        private void Discover()
        {
            List<ScanFilter> filters = new List<ScanFilter>();

            ScanFilter filter = new ScanFilter.Builder()
                    .SetServiceUuid(new ParcelUuid(Constants.MY_UUID))
                    .Build();
            filters.Add(filter);

            //ScanSettings settings = new ScanSettings.Builder()
            //        .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
            //        .Build();

            ScanSettings.Builder builder = new ScanSettings.Builder();
            builder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M /* Marshmallow */)
            {
                builder.SetMatchMode(BluetoothScanMatchMode.Aggressive);
                builder.SetNumOfMatches((int)BluetoothScanMatchNumber.MaxAdvertisement);
                builder.SetCallbackType(ScanCallbackType.AllMatches);
            }

            var settings = builder.Build();

            myScanCallback = new MyScanCallback(this);

            bluetoothLeScanner.StartScan(filters, settings, myScanCallback);
        }

        private void Advertise()
        {
            bluetoothLeAdvertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;

            AdvertiseSettings settings = new AdvertiseSettings.Builder()
                    .SetAdvertiseMode(AdvertiseMode.LowLatency)
                    .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                    .SetTimeout(0)
                    .SetConnectable(true)
                    .Build();

            ParcelUuid pUuid = new ParcelUuid(Constants.MY_UUID);

            AdvertiseData data = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(true)
                    .AddServiceUuid(pUuid)
                    .AddServiceData(pUuid, Encoding.UTF8.GetBytes("Data send"))
                    .Build();

            myAdvertiseCallback = new MyAdvertiseCallback();

            bluetoothLeAdvertiser.StartAdvertising(settings, data, myAdvertiseCallback);

            //MyBluetoothGattServerCallback myBluetoothGattServerCallback = new MyBluetoothGattServerCallback(this);

            //bluetoothGattServer = mBluetoothManager.OpenGattServer(this, myBluetoothGattServerCallback);
            //bluetoothGattServer.AddService(CreateService());
        }
    }
}
