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

namespace BTShare2
{
    public class Constants
    {
        public static readonly String TAG = "BluetoothSyncService";

        public static readonly UUID SERVICE_UUID_HALF = UUID.FromString("5ac825f4-6084-42a6-0000-000000000000");

        public static readonly string FAKE_MAC_ADDRESS = "02:00:00:00:00:00";
    }
}