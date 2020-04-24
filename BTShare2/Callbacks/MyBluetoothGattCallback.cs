//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Bluetooth;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;

//namespace BTShare2.Callbacks
//{
//    internal class MyBluetoothGattCallback : BluetoothGattCallback
//    {
//        //BluetoothGattCharacteristic characteristic;

//        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
//        {
//            base.OnConnectionStateChange(gatt, status, newState);

//            if (newState == ProfileState.Connected)
//            {
//                //Log.i(TAG, "Connected to GATT server.");
//                gatt.DiscoverServices();
//            }
//            else if (newState == ProfileState.Disconnected)
//            {

//                //Log.i(TAG, "Disconnected from GATT server.");
//            }

//            byte[] newValue = characteristic.GetValue();
//            //if (newState == ProfileState.Connected)
//            //{
//            //    gatt.DiscoverServices();
//            //}
//        }

//        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
//        {
//            base.OnServicesDiscovered(gatt, status);

//            if (status != GattStatus.Success)
//                return;

//            var characteristic = gatt.GetService(Constants.MY_UUID).GetCharacteristic(Constants.CHARACTERISTIC_COUNTER_UUID);

//            gatt.SetCharacteristicNotification(characteristic, true);

//            BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(Constants.DESCRIPTOR_CONFIG_UUID);

//            descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue);
//            gatt.WriteDescriptor(descriptor);
//        }

//        public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
//        {
//            base.OnDescriptorWrite(gatt, descriptor, status);
//            if (Constants.DESCRIPTOR_CONFIG_UUID.Equals(descriptor.Uuid))
//            {
//                BluetoothGattCharacteristic characteristic = gatt.GetService(Constants.MY_UUID)
//                  .getCharacteristic(Constants.CHARACTERISTIC_COUNTER_UUID);
//                gatt.ReadCharacteristic(characteristic);
//            }
//        }

//        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
//        {
//            base.OnDescriptorRead(gatt, descriptor, status);
//        }
//    }
//}