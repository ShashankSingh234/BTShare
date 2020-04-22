using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
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

            StringBuilder builder = new StringBuilder(result.Device.Name);
            builder.Append(" ").Append(result.Device.Address);
            byte[] data;
            result.ScanRecord.ServiceData.TryGetValue(result.ScanRecord.ServiceUuids[0], out data);
            builder.Append("\n").Append(Encoding.UTF8.GetString(data));

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
}