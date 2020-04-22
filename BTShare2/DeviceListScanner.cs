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

namespace BTShare2
{
    public class DeviceListScanner
    {
        const string TAG = "DeviceListActivity";
        public const string EXTRA_DEVICE_ADDRESS = "device_address";

        public BluetoothAdapter btAdapter;
        public static List<string> newDevicesArrayAdapter;

        public static event EventHandler NewItemAdded;

        public DeviceListScanner(BluetoothAdapter btAdapter)
        {
            List<string> pairedDevicesArrayAdapter = new List<string>();
            newDevicesArrayAdapter = new List<string>();

            //DeviceListView_ItemClick();

            // Get the local Bluetooth adapter
            this.btAdapter = btAdapter;

            // Get a set of currently paired devices
            var pairedDevices = btAdapter.BondedDevices;

            // If there are paired devices, add each on to the ArrayAdapter
            if (pairedDevices.Count > 0)
            {
                foreach (var device in pairedDevices)
                {
                    pairedDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                }
            }
            else
            {
                pairedDevicesArrayAdapter.Add("No device found");
            }

            DoDiscovery();
        }

        /// <summary>
        /// Start device discovery with the BluetoothAdapter
        /// </summary>
        void DoDiscovery()
        {
            // If we're already discovering, stop it
            if (btAdapter.IsDiscovering)
            {
                btAdapter.CancelDiscovery();
            }
            
            // Request discover from BluetoothAdapter
            var x = btAdapter.StartDiscovery();
        }

        void DeviceListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            btAdapter.CancelDiscovery();

            // Get the device MAC address, which is the last 17 chars in the View
            var info = ((TextView)e.View).Text;
            var address = info.Substring(info.Length - 17);

            // Create the result intent and include MAC address.
            var intent = new Intent();
            intent.PutExtra(EXTRA_DEVICE_ADDRESS, address);
        }

        public class DeviceDiscoveredReceiver : BroadcastReceiver
        {
            public DeviceDiscoveredReceiver()
            {
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;

                // When discovery finds a device
                if (action == BluetoothDevice.ActionFound)
                {
                    // Get the BluetoothDevice object from the Intent
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    // If it's already paired, skip it, because it's been listed already
                    if (device.BondState != Bond.Bonded)
                    {
                        newDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                        NewItemAdded?.Invoke(null, EventArgs.Empty);
                    }
                    // When discovery is finished, change the Activity title
                }
                else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    if (newDevicesArrayAdapter.Count == 0)
                    {
                        newDevicesArrayAdapter.Add("No device found");
                    }
                }
            }
        }
    }
}