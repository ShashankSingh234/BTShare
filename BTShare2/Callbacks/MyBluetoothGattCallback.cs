﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local connected successfully";
                });

                gatt.RequestMtu(512);

                //Log.i(TAG, "Connected to GATT server.");
            }
            else if (newState == ProfileState.Disconnected)
            {
                mainActivity.RunOnUiThread(() =>
                {
                    mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local disconnected";
                });
                DisconnectGattServer();
                //Log.i(TAG, "Disconnected from GATT server.");
            }
            else
            {
                DisconnectGattServer();
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
            catch(Exception ex)
            {

            }
        }

        //public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        //{
        //    base.OnDescriptorWrite(gatt, descriptor, status);
        //    if (Constants.MY_UUID.Equals(descriptor.Uuid))
        //    {
        //        BluetoothGattCharacteristic characteristic = gatt.GetService(Constants.MY_UUID)
        //          .GetCharacteristic(Constants.MY_UUID);
        //        var t = gatt.ReadCharacteristic(characteristic);
        //    }
        //}

        //public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        //{
        //    base.OnCharacteristicRead(gatt, characteristic, status);

        //    mainActivity.RunOnUiThread(() =>
        //    {
        //        mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Gatt local characterstic read";
        //    });

        //    byte[] data = characteristic.GetValue();
        //    if (data != null)
        //    {
        //        var value = Encoding.UTF8.GetString(data);
        //        mainActivity.RunOnUiThread(() =>
        //        {
        //            mainActivity.mText.Text = mainActivity.mText.Text + " " + value;
        //        });
        //    }
        //    else
        //    {
        //        mainActivity.RunOnUiThread(() =>
        //        {
        //            mainActivity.logTextView.Text = mainActivity.logTextView.Text + "Recieved null data";
        //        });
        //    }
        //}

        //public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        //{
        //    base.OnCharacteristicChanged(gatt, characteristic);
        //}

        //public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        //{
        //    base.OnDescriptorRead(gatt, descriptor, status);
        //}

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            mainActivity.RunOnUiThread(() =>
            {
                mainActivity.logTextView.Text = mainActivity.logTextView.Text + "On Characteristic changed";
            });

            var byteRecieved = characteristic.GetValue();
            if (byteRecieved != null)
            {
                var value = Encoding.UTF8.GetString(byteRecieved);
                value = value + " Client";
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
                mainActivity.bluetoothGatt.Disconnect();
                mainActivity.bluetoothGatt.Close();
            }
        }
    }
}