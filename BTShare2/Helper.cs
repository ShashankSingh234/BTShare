using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace BTShare2
{
    public class Helper
    {
        public static string GetBluetoothAdapterAddress(BluetoothAdapter bluetoothAdapter)
        {
            //@SuppressLint("HardwareIds") // Pair-free peer-to-peer communication should qualify as an "advanced telephony use case".
            string address = bluetoothAdapter.Address;

            if (address.Equals(Constants.FAKE_MAC_ADDRESS) && Build.VERSION.SdkInt < BuildVersionCodes.O /* Oreo */)
            {
                Log.Warn(Constants.TAG, "bluetoothAdapter.Address did not return the physical address");

                // HACK HACK HACK: getAddress is intentionally broken (but not deprecated?!) on Marshmallow and up:
                //   * https://developer.android.com/about/versions/marshmallow/android-6.0-changes.html#behavior-notifications
                //   * https://code.google.com/p/android/issues/detail?id=197718
                // However, we need it to establish pair-free Bluetooth Classic connections:
                //   * All BLE advertisements include a MAC address, but Android broadcasts a temporary, randomly-generated address.
                //   * Currently, it is only possible to listen for connections using the device's physical address.
                // So we use reflection to get it anyway: http://stackoverflow.com/a/35984808
                // This hack won't be necessary if getAddress is ever fixed (unlikely) or (preferably) we can listen using an arbitrary address.

                Object bluetoothManagerService = new Mirror().on(bluetoothAdapter).get().field("mService");
                if (bluetoothManagerService == null)
                {
                    Log.Warn(Constants.TAG, "Couldn't retrieve bluetoothAdapter.mService using reflection");
                    return null;
                }

                Object internalAddress = new Mirror().on(bluetoothManagerService).invoke().method("getAddress").withoutArgs();
                if (internalAddress == null || !(internalAddress is string)) {
                    Log.Warn(Constants.TAG, "Couldn't call bluetoothAdapter.mService.getAddress() using reflection");
                    return null;
                }

                address = (String)internalAddress;
            }

            // On Oreo and above, Android will throw a SecurityException if we try to get the MAC address with reflection
            // https://android-developers.googleblog.com/2017/04/changes-to-device-identifiers-in.html
            // https://stackoverflow.com/a/35984808/702467
            if (address.Equals(Constants.FAKE_MAC_ADDRESS))
            {
                Log.Warn(Constants.TAG, "Android is actively blocking requests to get the MAC address");
                return null;
                // TODO: In this case, present UI that asks the user to manually copy the MAC address from settings
            }

            return address;
        }

        public static bool MatchesServiceUuid(UUID uuid)
        {
            return Constants.SERVICE_UUID_HALF.MostSignificantBits == uuid.MostSignificantBits;
        }

        public static long LongFromMacAddress(string macAddress)
        {
            return Convert.ToInt64(macAddress.Replace(":", ""), 16);
        }

        public static string MacAddressFromLong(long macAddressLong)
        {
            byte[] macAddressByte = { (byte)(macAddressLong >> 40),
                (byte)(macAddressLong >> 32),
                (byte)(macAddressLong >> 24),
                (byte)(macAddressLong >> 16),
                (byte)(macAddressLong >> 8),
                (byte)(macAddressLong) };
            return BitConverter.ToString(macAddressByte).Replace("-", ":");
        }

    }
}