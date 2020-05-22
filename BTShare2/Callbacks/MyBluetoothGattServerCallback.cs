using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BTShare2.Helpers;

namespace BTShare2.Callbacks
{
    public class MyBluetoothGattServerCallback : BluetoothGattServerCallback
    {
        MainActivity mainActivity;
        List<BluetoothDevice> connectedDevices;
        public MyBluetoothGattServerCallback(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
            connectedDevices = new List<BluetoothDevice>();
        }

        public override void OnConnectionStateChange(BluetoothDevice device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(device, status, newState);

            mainActivity.RunOnUiThread(() => {
                mainActivity.remoteAddressText.Text = device.Address;
            });
            
            if(newState == ProfileState.Connected)
            {
                connectedDevices.Add(device);
                //mainActivity.connectedDeviceMac.Add(device.Address);
                //device.ConnectGatt(mainActivity, false, mainActivity.myBluetoothGattCallback);
            }
            else
            {
                connectedDevices.Remove(connectedDevices.Find(x => x.Address.Equals(device.Address)));
            }
        }

        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);
            if (Constants.MY_UUID.Equals(characteristic.Uuid))
            {
                var t = Encoding.UTF8.GetString(value);
                t = t + " Server";
                mainActivity.RunOnUiThread(() =>
                {
                    mainActivity.mText.Text = mainActivity.mText.Text + "\n" + t;
                });

                mainActivity.bluetoothGattServer.SendResponse(device, requestId, GattStatus.Success, 0, null);

                characteristic.SetValue(mainActivity.dataToTransmit);

                foreach (var connectedDevice in connectedDevices)
                {
                    mainActivity.bluetoothGattServer.NotifyCharacteristicChanged(connectedDevice, characteristic, false);
                }
            }
        }

        public override void OnDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnDescriptorWriteRequest(device, requestId, descriptor, preparedWrite, responseNeeded, offset, value);

            if (Constants.MY_UUID.Equals(descriptor.Uuid))
            {
                //if (Arrays.Equals(ENABLE_NOTIFICATION_VALUE, value))
                //{
                //    mRegisteredDevices.add(device);
                //}
                //else if (Arrays.equals(DISABLE_NOTIFICATION_VALUE, value))
                //{
                //    mRegisteredDevices.remove(device);
                //}

                if (responseNeeded)
                {
                    mainActivity.bluetoothGattServer.SendResponse(device, requestId, GattStatus.Success, 0, null);
                }
            }
        }
    }
}