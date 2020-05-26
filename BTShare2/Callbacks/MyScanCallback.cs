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
using Android.Text;
using Android.Views;
using Android.Widget;

namespace BTShare2.Callbacks
{
    public class MyScanCallback : ScanCallback
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

            if (mainActivity.isGattConnected)
                return;
            mainActivity.isGattConnected = true;

            if (mainActivity.connectedDeviceMac.Contains(result.Device.Address))
                return;
            //mainActivity.connectedDeviceMac.Add(result.Device.Address);

            var tttt = mainActivity.recievedUserId;

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Discover result called";
            });

            StringBuilder builder = new StringBuilder(result.Device.Name);
            builder.Append(" ").Append(result.Device.Address);
            byte[] data;
            //result.ScanRecord.ServiceData.TryGetValue(result.ScanRecord.ServiceUuids[0], out data);
            //builder.Append("\n").Append(Encoding.UTF8.GetString(data));

            //mainActivity.success++;
            mainActivity.mSuccessCountText.Text = mainActivity.connectedDeviceMac.Count().ToString() + " " + mainActivity.connectedDeviceMac.Distinct().Count().ToString(); //mainActivity.success.ToString();
            //mainActivity.mText.Text = mainActivity.mText.Text + builder.ToString();


            mainActivity.RunOnUiThread(() =>
                {
                    mainActivity.remoteAddressText.Text = result.Device.Address;
                    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Connect gatt";
                });

            mainActivity.bluetoothGatt = null;
            mainActivity.bluetoothGatt = result.Device.ConnectGatt(mainActivity, false, mainActivity.myBluetoothGattCallback);

            //mainActivity.bluetoothLeScanner.StopScan(mainActivity.myScanCallback);

            //if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            //{
            //    mainActivity.RunOnUiThread(() =>
            //    {
            //        mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Connect gatt Marshmallow a";
            //    });
            //    mainActivity.bluetoothGatt = result.Device.ConnectGatt(mainActivity, false, mainActivity.myBluetoothGattCallback);
            //}
            //else
            //{
            //    mainActivity.RunOnUiThread(() =>
            //    {
            //        mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Connect gatt marshmallow previous";
            //    });
            //    mainActivity.bluetoothGatt = result.Device.ConnectGatt(mainActivity, false, mainActivity.myBluetoothGattCallback);
            //}
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
            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Discover failed" + errorCode.ToString();
            });
        }
    }
}