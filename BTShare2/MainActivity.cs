using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android.Bluetooth.LE;
using Android.Bluetooth;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;

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

        //public const string UUIDString = "5ac825f4-6084-42a6-0000-000000000000";

        //public readonly UUID SERVICE_UUID_HALF = UUID.FromString(UUIDString);

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


        }


        //protected override void OnResume()
        //{
        //    base.OnResume();
        //    mDiscoverButton.Click += MDiscoverButton_Click;
        //    mAdvertiseButton.Click += MAdvertiseButton_Click;
        //}

        //protected override void OnPause()
        //{
        //    base.OnPause();
        //    mDiscoverButton.Click -= MDiscoverButton_Click;
        //    mAdvertiseButton.Click -= MAdvertiseButton_Click;
        //}

        //private void MAdvertiseButton_Click(object sender, System.EventArgs e)
        //{
        //    Advertise();
        //}

        //private void MDiscoverButton_Click(object sender, System.EventArgs e)
        //{
        //    Discover();
        //}

        //private void Discover()
        //{
        //    List<ScanFilter> filters = new List<ScanFilter>();

        //    //ScanFilter filter = new ScanFilter.Builder()
        //    //        .SetServiceUuid(new ParcelUuid(UUID.FromString(UUIDString)))
        //    //        .Build();
        //    //filters.Add(filter);

        //    ScanSettings.Builder builder = new ScanSettings.Builder();
        //    builder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower);

        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.M /* Marshmallow */)
        //    {
        //        builder.SetMatchMode(BluetoothScanMatchMode.Aggressive);
        //        builder.SetNumOfMatches((int)BluetoothScanMatchNumber.MaxAdvertisement);
        //        builder.SetCallbackType(ScanCallbackType.AllMatches);
        //    }

        //    var settings = builder.Build();

        //    MyScanCallback myScanCallback = new MyScanCallback(this);

        //    mBluetoothLeScanner.StartScan(null, settings, myScanCallback); //null filter in noise app

        //    Action action = () =>
        //    {
        //        mBluetoothLeScanner.StopScan(myScanCallback);
        //    };

        //    mHandler.PostDelayed(action, 10000);
        //}

        //private void Advertise()
        //{
        //    BluetoothLeAdvertiser advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;

        //    AdvertiseSettings settings = new AdvertiseSettings.Builder()
        //            .SetAdvertiseMode(AdvertiseMode.LowPower)
        //            //.SetTxPowerLevel(AdvertiseTx.PowerHigh)
        //            .SetTimeout(0)
        //            .SetConnectable(false)
        //            .Build();

        //    ParcelUuid pUuid = new ParcelUuid(UUID.FromString(UUIDString));

        //    AdvertiseData data = new AdvertiseData.Builder()
        //            .SetIncludeDeviceName(false)
        //            .AddServiceUuid(pUuid)
        //            //.AddServiceData(pUuid, Encoding.UTF8.GetBytes("Data send"))
        //            .Build();

        //    MyAdvertiseCallback myAdvertiseCallback = new MyAdvertiseCallback();

        //    advertiser.StartAdvertising(settings, data, myAdvertiseCallback);

        //}
    }
}
