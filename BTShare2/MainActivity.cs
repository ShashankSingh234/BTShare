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

        public const string UUIDString = "CDB7950D-73F1-4D4D-8E47-C090502DBD63";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

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


        protected override void OnResume()
        {
            base.OnResume();
            mDiscoverButton.Click += MDiscoverButton_Click;
            mAdvertiseButton.Click += MAdvertiseButton_Click;
        }

        protected override void OnPause()
        {
            base.OnPause();
            mDiscoverButton.Click -= MDiscoverButton_Click;
            mAdvertiseButton.Click -= MAdvertiseButton_Click;
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
                    .SetServiceUuid(new ParcelUuid(UUID.FromString(UUIDString)))
                    .Build();
            filters.Add(filter);

            ScanSettings settings = new ScanSettings.Builder()
                    .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                    .Build();

            MyScanCallback myScanCallback = new MyScanCallback(this);

            mBluetoothLeScanner.StartScan(filters, settings, myScanCallback);

            Action action = () =>
            {
                mBluetoothLeScanner.StopScan(myScanCallback);
            };

            mHandler.PostDelayed(action, 10000);
        }

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
                    //.AddServiceData(pUuid, Encoding.UTF8.GetBytes("Data"))
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

    internal class MyScanCallback : ScanCallback
    {
        MainActivity mainActivity;

        public MyScanCallback(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            if (result == null || result.Device == null || TextUtils.IsEmpty(result.Device.Name))
                return;

            StringBuilder builder = new StringBuilder(result.Device.Name);

            //builder.Append("\n").Append(new string(result.ScanRecord.ServiceData(result.getScanRecord().getServiceUuids().get(0)), Charset.forName("UTF-8")));

            mainActivity.success++;
            mainActivity.mSuccessCountText.Text = mainActivity.success.ToString();
            mainActivity.mText.Text = mainActivity.mText.Text + builder.ToString();
        }

        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
            mainActivity.failed++;
            mainActivity.mFailedCountText.Text = mainActivity.failed.ToString();
        }
    }

    internal class MyAdvertiseCallback : AdvertiseCallback
    {
        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            base.OnStartSuccess(settingsInEffect);
        }

        public override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode)
        {
            base.OnStartFailure(errorCode);
            //Log.e("BLE", "Advertising onStartFailure: " + errorCode);
        }
    }
}
