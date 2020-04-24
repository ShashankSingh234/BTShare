using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace BTShare2.Helpers
{
    public class Constants
    {
        public const string TAG = "BluetoothChatService";

        public const string NAME_SECURE = "BluetoothChatSecure";
        public const string NAME_INSECURE = "BluetoothChatInsecure";

        public const string UUIDString = "0000b81d-0000-1000-8000-00805f9b34fb";
        public static UUID MY_UUID = UUID.FromString(UUIDString);

        public static UUID MY_UUID_SECURE = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");
        public static UUID MY_UUID_INSECURE = UUID.FromString("8ce255c0-200a-11e0-ac64-0800200c9a66");
        public static UUID CHARACTERISTIC_COUNTER_UUID = UUID.FromString("31517c58-66bf-470c-b662-e352a6c80cba");
        public static UUID CHARACTERISTIC_INTERACTOR_UUID = UUID.FromString("0b89d2d4-0ea6-4141-86bb-0c5fb91ab14a");
        public static UUID DESCRIPTOR_CONFIG_UUID = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        public const int STATE_NONE = 0;       // we're doing nothing
        public const int STATE_LISTEN = 1;     // now listening for incoming connections
        public const int STATE_CONNECTING = 2; // now initiating an outgoing connection
        public const int STATE_CONNECTED = 3;  // now connected to a remote device

        public const int MESSAGE_STATE_CHANGE = 1;
        public const int MESSAGE_READ = 2;
        public const int MESSAGE_WRITE = 3;
        public const int MESSAGE_DEVICE_NAME = 4;
        public const int MESSAGE_TOAST = 5;

        public const string DEVICE_NAME = "device_name";
        public const string TOAST = "toast";

    }
}