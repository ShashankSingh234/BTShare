using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Bluetooth.LE;
using System.Collections.Generic;
using Android.Bluetooth;
using Java.Util;
using System.Text;
using Android.Text;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;
using BTShare2.Callbacks;
using BTShare2.Services;
using Android.Content;
using BTShare2.BroadcastReciever;
using static BTShare2.DeviceListScanner;
using Android.Util;

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

        private BluetoothLeScanner mBluetoothLeScanner;
        private Handler mHandler = new Handler();

        public const string UUIDString = "0000b81d-0000-1000-8000-00805f9b34fb";


        const string TAG = "BluetoothChatFragment";

        const int REQUEST_CONNECT_DEVICE_SECURE = 1;
        const int REQUEST_CONNECT_DEVICE_INSECURE = 2;
        const int REQUEST_ENABLE_BT = 3;

        String connectedDeviceName = "";
        StringBuilder outStringBuffer;
        BluetoothAdapter bluetoothAdapter = null;
        BluetoothChatService chatService = null;

        bool requestingPermissionsSecure, requestingPermissionsInsecure;

        DiscoverableModeReceiver discoverableModeReceiver;
        //ChatHandler handler;
        DeviceDiscoveredReceiver deviceDiscoveredReceiver;

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

            mBluetoothLeScanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;

            if (!BluetoothAdapter.DefaultAdapter.IsMultipleAdvertisementSupported)
            {
                Toast.MakeText(this, "Multiple advertisement not supported", ToastLength.Long).Show();
                mAdvertiseButton.Enabled = false;
                mDiscoverButton.Enabled = false;
            }

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;


            //receiver = new DiscoverableModeReceiver();
            //receiver.BluetoothDiscoveryModeChanged += (sender, e) =>
            //{
            //    InvalidateOptionsMenu();
            //};

            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available.", ToastLength.Long).Show();
                FinishAndRemoveTask();
            }

            //handler = new ChatHandler(this);

            // Register for broadcasts when a device is discovered
            deviceDiscoveredReceiver = new DeviceDiscoveredReceiver();
            var filter = new IntentFilter(BluetoothDevice.ActionFound);
            RegisterReceiver(deviceDiscoveredReceiver, filter);

            // Register for broadcasts when discovery has finished
            filter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver(deviceDiscoveredReceiver, filter);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (!bluetoothAdapter.IsEnabled)
            {
                var enableIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableIntent, REQUEST_ENABLE_BT);
            }
            else if (chatService == null)
            {
                SetupChat();
            }

            // Register for when the scan mode changes
            var filter = new IntentFilter(BluetoothAdapter.ActionScanModeChanged);
            RegisterReceiver(discoverableModeReceiver, filter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Make sure we're not doing discovery anymore
            if (bluetoothAdapter != null)
            {
                bluetoothAdapter.CancelDiscovery();
            }

            // Unregister broadcast listeners
            UnregisterReceiver(deviceDiscoveredReceiver);
        }
        protected override void OnResume()
        {
            base.OnResume();
            if (chatService != null)
            {
                if (chatService.GetState() == BluetoothChatService.STATE_NONE)
                {
                    chatService.Start();
                }
            }
            mDiscoverButton.Click += MDiscoverButton_Click;
            mAdvertiseButton.Click += MAdvertiseButton_Click;
            DeviceListScanner.NewItemAdded += DeviceListScanner_NewItemAdded;
        }

        protected override void OnPause()
        {
            base.OnPause();
            mDiscoverButton.Click -= MDiscoverButton_Click;
            mAdvertiseButton.Click -= MAdvertiseButton_Click;
            DeviceListScanner.NewItemAdded -= DeviceListScanner_NewItemAdded;
        }

        void SetupChat()
        {
            //conversationArrayAdapter = new ArrayAdapter<string>(Activity, Resource.Layout.message);
            //conversationView.Adapter = conversationArrayAdapter;

            //outEditText.SetOnEditorActionListener(writeListener);
            //sendButton.Click += (sender, e) =>
            //{
            //    var textView = View.FindViewById<TextView>(Resource.Id.edit_text_out);
            //    var msg = textView.Text;
            //    SendMessage(msg);
            //};

            chatService = new BluetoothChatService(this);
            outStringBuffer = new StringBuilder("");
        }

        void SendMessage(String message)
        {
            if (chatService.GetState() != BluetoothChatService.STATE_CONNECTED)
            {
                Toast.MakeText(this, "Not connected to any device", ToastLength.Long).Show();
                return;
            }

            if (message.Length > 0)
            {
                var bytes = Encoding.ASCII.GetBytes(message);
                chatService.Write(bytes);
            }
        }

        void ConnectDevice(string info, bool secure)
        {
            var address = info.Substring(info.Length - 17);//data.Extras.GetString(DeviceListScanner.EXTRA_DEVICE_ADDRESS);
            var device = bluetoothAdapter.GetRemoteDevice(address);
            chatService.Connect(device, secure);
        }

        //void EnsureDiscoverable()
        //{
        //    if (bluetoothAdapter.ScanMode != Android.Bluetooth.ScanMode.ConnectableDiscoverable)
        //    {
        //        var discoverableIntent = new Intent(BluetoothAdapter.ActionRequestDiscoverable);
        //        discoverableIntent.PutExtra(BluetoothAdapter.ExtraDiscoverableDuration, 300);
        //        StartActivity(discoverableIntent);
        //    }
        //}

        private void DeviceListScanner_NewItemAdded(object sender, EventArgs e)
        {
            if (DeviceListScanner.newDevicesArrayAdapter.Find(x => x.Contains("OnePlus 6")) != null)
            {
                ConnectDevice(DeviceListScanner.newDevicesArrayAdapter.Find(x => x.Contains("OnePlus 6")), false);
                DeviceListScanner.NewItemAdded -= DeviceListScanner_NewItemAdded;
            }
        }

        private void MAdvertiseButton_Click(object sender, System.EventArgs e)
        {
            //Configuring to get devices list
            DeviceListScanner deviceListScanner = new DeviceListScanner(bluetoothAdapter);

            //Advertise();
        }

        private void MDiscoverButton_Click(object sender, System.EventArgs e)
        {
            SendMessage("Hellodshgkjdshdfcksayhcujkasdhcjagcgyuahzschasgcyascgfagscvhasfcasfucgysakhcuigascygiygcugchsaygucsayugacsyguacsyguygucasyguacsyguscagyuascgyuscayguugidgiuuiacsuacsiuachuacshacshcsahacshuacsacsuiacsscahscaacacscashacshihacsicahisacsacsascacsuiacsuicascas");
            //Discover();
        }

        //private void Discover()
        //{
        //    List<ScanFilter> filters = new List<ScanFilter>();

        //    ScanFilter filter = new ScanFilter.Builder()
        //            .SetServiceUuid(new ParcelUuid(UUID.FromString(UUIDString)))
        //            .Build();
        //    filters.Add(filter);

        //    ScanSettings settings = new ScanSettings.Builder()
        //            .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
        //            .Build();

        //    MyScanCallback myScanCallback = new MyScanCallback(this);

        //    mBluetoothLeScanner.StartScan(filters, settings, myScanCallback);

        //    Action action = () =>
        //    {
        //        mBluetoothLeScanner.StopScan(myScanCallback);
        //    };

        //    mHandler.PostDelayed(action, 10000);
        //}

        private void Advertise()
        {
            BluetoothLeAdvertiser advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;

            AdvertiseSettings settings = new AdvertiseSettings.Builder()
                    .SetAdvertiseMode(AdvertiseMode.LowLatency)
                    .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                    .SetConnectable(false)
                    .Build();

            ParcelUuid pUuid = new ParcelUuid(UUID.FromString(UUIDString));

            AdvertiseData data = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(true)
                    .AddServiceUuid(pUuid)
                    .AddServiceData(pUuid, Encoding.UTF8.GetBytes("Data send"))
                    .Build();

            MyAdvertiseCallback myAdvertiseCallback = new MyAdvertiseCallback();

            advertiser.StartAdvertising(settings, data, myAdvertiseCallback);

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
