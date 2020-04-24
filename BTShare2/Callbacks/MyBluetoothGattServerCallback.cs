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
//    internal class MyBluetoothGattServerCallback : BluetoothGattServerCallback
//    {
//        MainActivity mainActivity;
//        public MyBluetoothGattServerCallback(MainActivity mainActivity)
//        {
//            this.mainActivity = mainActivity;
//        }
//        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
//        {
//            base.OnCharacteristicReadRequest(device, requestId, offset, characteristic);

//            if (Constants.CHARACTERISTIC_COUNTER_UUID.Equals(characteristic.Uuid))
//            {
//                byte[] value = Ints.toByteArray(currentCounterValue);
//                mainActivity.bluetoothGattServer.SendResponse(device, requestId, GattStatus.Success, 0, value);
//            }
//        }

//        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
//        {
//            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);
//            if (Constants.CHARACTERISTIC_INTERACTOR_UUID.Equals(characteristic.Uuid))
//            {
//                currentCounterValue++;
//                notifyRegisteredDevices();
//            }
//        }

//        public override void OnDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
//        {
//            base.OnDescriptorWriteRequest(device, requestId, descriptor, preparedWrite, responseNeeded, offset, value);

//            if (Constants.DESCRIPTOR_CONFIG_UUID.Equals(descriptor.Uuid))
//            {
//                if (Arrays.Equals(ENABLE_NOTIFICATION_VALUE, value))
//                {
//                    mRegisteredDevices.add(device);
//                }
//                else if (Arrays.equals(DISABLE_NOTIFICATION_VALUE, value))
//                {
//                    mRegisteredDevices.remove(device);
//                }

//                if (responseNeeded)
//                {
//                    mGattServer.sendResponse(device, requestId, GATT_SUCCESS, 0, null);
//                }
//            }
//        }
//    }
//}