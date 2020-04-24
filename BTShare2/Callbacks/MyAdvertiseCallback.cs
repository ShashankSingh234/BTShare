using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BTShare2.Helpers;

namespace BTShare2.Callbacks
{
    public class MyAdvertiseCallback : AdvertiseCallback
    {
        MainActivity mainActivity;

        public MyAdvertiseCallback(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            base.OnStartSuccess(settingsInEffect);

            mainActivity.isAdvertising = true;

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Advertise started successfully";
            });
        }

        public override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode)
        {
            base.OnStartFailure(errorCode);

            mainActivity.isAdvertising = true;

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Advertise failed";
            });
            //Log.e("BLE", "Advertising onStartFailure: " + errorCode);
        }
    }
}