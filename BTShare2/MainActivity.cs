using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android.Bluetooth;
using System.Text;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;
using BTShare2.Services;
using Android.Content;
using BTShare2.BroadcastReciever;
using static BTShare2.DeviceListScanner;
using BTShare2.Helpers;

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
        private Button discoverDeviceButton;
        private Button sendMessageButton;

        public const string UUIDString = "0000b81d-0000-1000-8000-00805f9b34fb";

        const int REQUEST_ENABLE_BT = 3;

        String connectedDeviceName = "";
        StringBuilder outStringBuffer;
        BluetoothAdapter bluetoothAdapter = null;
        BluetoothChatService chatService = null;

        DiscoverableModeReceiver discoverableModeReceiver;
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
            sendMessageButton = FindViewById<Button>(Resource.Id.discover_btn);
            discoverDeviceButton = FindViewById<Button>(Resource.Id.advertise_btn);

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available.", ToastLength.Long).Show();
                FinishAndRemoveTask();
            }

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
                chatService = new BluetoothChatService(this);
                outStringBuffer = new StringBuilder("");
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
                if (chatService.GetState() == Constants.STATE_NONE)
                {
                    chatService.Start();
                }
            }
            sendMessageButton.Click += SendMessageButton_Click;
            discoverDeviceButton.Click += DiscoverDeviceButton_Click;
            DeviceListScanner.AllItemDiscovered += DeviceListScanner_AllItemDiscovered;
        }

        protected override void OnPause()
        {
            base.OnPause();
            sendMessageButton.Click -= SendMessageButton_Click;
            discoverDeviceButton.Click -= DiscoverDeviceButton_Click;
            DeviceListScanner.AllItemDiscovered -= DeviceListScanner_AllItemDiscovered;
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

        private void DeviceListScanner_AllItemDiscovered(object sender, EventArgs e)
        {
            if (DeviceListScanner.newDevicesArrayAdapter.Find(x => x.Contains("OnePlus 6")) != null)
            {
                ConnectDevice(DeviceListScanner.newDevicesArrayAdapter.Find(x => x.Contains("OnePlus 6")), false);
                //DeviceListScanner.AllItemDiscovered -= DeviceListScanner_NewItemAdded;
            }
        }

        private void DiscoverDeviceButton_Click(object sender, System.EventArgs e)
        {
            //Configuring to get devices list
            DeviceListScanner deviceListScanner = new DeviceListScanner(bluetoothAdapter);
        }

        private void SendMessageButton_Click(object sender, System.EventArgs e)
        {
            var messageToSend = "Hellodshgkjdshdfcksayhcujkasdhcjagcgyuahzschasgcyascgfagscvhasfcasfucgysakhcuigascygiygcugchsaygucsayugacsyguacsyguygucasyguacsyguscagyuascgyuscayguugidgiuuiacsuacsiuachuacshacshcsahacshuacsacsuiacsscahscaacacscashacshihacsicahisacsacsascacsuiacsuicascas";
            if (chatService.GetState() != Constants.STATE_CONNECTED)
            {
                Toast.MakeText(this, "Not connected to any device", ToastLength.Long).Show();
                return;
            }

            if (messageToSend.Length > 0)
            {
                var bytes = Encoding.ASCII.GetBytes(messageToSend);
                chatService.Write(bytes);
            }
        }
    }
}
