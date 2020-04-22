using System;
using Android.Bluetooth;
using Android.Util;
using Java.Util;

namespace BTShare2.Services
{
    public class BluetoothClassicClient : TimerTask
    {
        BluetoothSocket socket = null;
        string macAddress = null;
        BluetoothSyncService bluetoothSyncService;

        public BluetoothClassicClient(BluetoothSyncService bluetoothSyncService, BluetoothDevice remoteDevice, UUID uuid)
        {
            this.bluetoothSyncService = bluetoothSyncService;
            macAddress = remoteDevice.Address;
            try
            {
                socket = remoteDevice.CreateInsecureRfcommSocketToServiceRecord(uuid);
            }
            catch (Exception connectException)
            {
                Log.Error(Constants.TAG, "Failed to set up a Bluetooth Classic connection as a client", connectException);
            }
        }

        public override void Run()
        {
            if (bluetoothSyncService.openConnections.Contains(macAddress))
                return;

            bluetoothSyncService.openConnections.Add(macAddress);
            try
            {
                // This will block until there is a connection
                Log.Debug(Constants.TAG, "Bluetooth Classic client is attempting to connect to a server");
                socket.Connect();

                StreamSync.bidirectionalSync(socket.InputStream, socket.OutputStream);
                socket.Close();
            }
            catch (Exception connectException)
            {
                Log.Error(Constants.TAG, "Failed to start a Bluetooth Classic connection as a client", connectException);

                try
                {
                    socket.Close();
                }
                catch (Exception closeException)
                {
                    Log.Error(Constants.TAG, "Failed to close a Bluetooth Classic connection as a client", closeException);
                }
            }
            bluetoothSyncService.openConnections.Remove(macAddress);
        }
    }
}