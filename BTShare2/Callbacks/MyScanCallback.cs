using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using BTShare2.Services;
using Java.Util;

namespace BTShare2.Callbacks
{
    public class MyScanCallback : ScanCallback
    {
        BluetoothSyncService bluetoothSyncService;

        public MyScanCallback(BluetoothSyncService service)
        {
            this.bluetoothSyncService = service;
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            if (result == null || result.Device == null || TextUtils.IsEmpty(result.Device.Name))
                return;

            //StringBuilder builder = new StringBuilder(result.Device.Name);
            //builder.Append(" ").Append(result.Device.Address);
            //byte[] data;
            //result.ScanRecord.ServiceData.TryGetValue(result.ScanRecord.ServiceUuids[0], out data);
            //builder.Append("\n").Append(Encoding.UTF8.GetString(data));

            //mainActivity.success++;
            //mainActivity.mSuccessCountText.Text = mainActivity.success.ToString();
            //mainActivity.mText.Text = mainActivity.mText.Text + builder.ToString();

            foreach (var uuid in result.ScanRecord.ServiceUuids)
            {
                if (!Helper.MatchesServiceUuid(uuid.Uuid))
                    continue;

                string remoteDeviceMacAddress = Helper.MacAddressFromLong(uuid.Uuid.LeastSignificantBits);
                BluetoothDevice remoteDevice = bluetoothSyncService.bluetoothAdapter.GetRemoteDevice(remoteDeviceMacAddress);

                //Start client thread
                // TODO: Interrupt this thread when the service is stopping
                new BluetoothClassicClient(bluetoothSyncService ,remoteDevice, uuid.Uuid).Run();
            }
        }

        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
            //mainActivity.failed++;
            //mainActivity.mFailedCountText.Text = mainActivity.failed.ToString();
        }
    }
}