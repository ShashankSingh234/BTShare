using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public class MyBluetoothGattCallback : BluetoothGattCallback
    {
        //BluetoothGattCharacteristic characteristic;
        MainActivity mainActivity;
        bool gattConnectedOnce = false;
        public MyBluetoothGattCallback(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            if (newState == ProfileState.Connected)
            {
                mainActivity.RunOnUiThread(() =>
                {
                    gattConnectedOnce = true;
                    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local connected successfully";
                });

                Thread.Sleep(2000);

                gatt.RequestMtu(50);

                //mainActivity.RunOnUiThread(() =>
                //{
                //    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local discover started";
                //});
                //gatt.DiscoverServices();

                //Log.i(TAG, "Connected to GATT server.");
            }
            else if (newState == ProfileState.Disconnected)
            {
                Thread.Sleep(5000);
                if (!gattConnectedOnce && mainActivity.connectedDeviceMac.Contains(gatt.Device.Address))
                    mainActivity.connectedDeviceMac.Remove(gatt.Device.Address);
                gattConnectedOnce = false;
                mainActivity.RunOnUiThread(() =>
                {
                    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local disconnected";
                });
                mainActivity.isGattConnected = false;
                DisconnectGattServer();
                //Log.i(TAG, "Disconnected from GATT server.");
            }
            else
            {
                if (!gattConnectedOnce && mainActivity.connectedDeviceMac.Contains(gatt.Device.Address))
                    mainActivity.connectedDeviceMac.Remove(gatt.Device.Address);
                gattConnectedOnce = false;
                //mainActivity.isGattConnected = false;
                gatt.Disconnect();
                //DisconnectGattServer();
            }
            //byte[] newValue = characteristic.GetValue();
            //if (newState == ProfileState.Connected)
            //{
            //    gatt.DiscoverServices();
            //}
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local discovered called";
            });

            if (status != GattStatus.Success)
                return;

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local discovered successfully";
            });

            try
            {
                var characteristic = gatt.GetService(Constants.MY_UUID)
                      .GetCharacteristic(Constants.MY_UUID);

                characteristic.WriteType = GattWriteType.Default;
                //var t = gatt.ReadCharacteristic(characteristic);

                var mInitialized = gatt.SetCharacteristicNotification(characteristic, true);

                //BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(Constants.MY_UUID);

                //byte[] writeValue = Encoding.UTF8.GetBytes("Hello from reciever");

                //descriptor.SetValue(writeValue);
                //gatt.WriteDescriptor(descriptor);

                var t = Encoding.UTF8.GetBytes(mainActivity.dataToTransmit);
                characteristic.SetValue(t);

                var success = mainActivity.bluetoothGatt.WriteCharacteristic(characteristic);
            }
            catch (Exception ex)
            {

            }
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "On Characteristic changed";
            });

            mainActivity.connectedDeviceMac.Add(gatt.Device.Address);

            var byteRecieved = characteristic.GetValue();
            if (byteRecieved != null)
            {
                var value = Encoding.UTF8.GetString(byteRecieved);
                value = value + " Client";
                mainActivity.recievedUserId.Add(value);
                mainActivity.RunOnUiThread(() =>
                {
                    mainActivity.mText.Text = mainActivity.mText.Text + "\n" + value;
                });
            }
            else
            {
                mainActivity.RunOnUiThread(() =>
                {
                    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Recieved null data";
                });
            }
            //DisconnectGattServer();
            gatt.Disconnect();
        }

        public override void OnMtuChanged(BluetoothGatt gatt, int mtu, [GeneratedEnum] GattStatus status)
        {
            base.OnMtuChanged(gatt, mtu, status);

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local discover started";
            });
            gatt.DiscoverServices();
        }

        void DisconnectGattServer()
        {
            if (mainActivity.bluetoothGatt != null)
            {
                try
                {
                    mainActivity.bluetoothGatt.Disconnect();
                    gattConnectedOnce = false;
                    mainActivity.isGattConnected = false;
                    mainActivity.bluetoothGatt.Close();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    gattConnectedOnce = false;
                    mainActivity.isGattConnected = false;
                }
            }
        }
    }
}