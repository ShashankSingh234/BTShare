using System;
using Android.Bluetooth;
using Android.Util;
using Java.Util;

namespace BTShare2.Services
{
    class BluetoothClassicServer : TimerTask
    {
        bool started = false;
        private BluetoothServerSocket serverSocket;

        BluetoothSyncService bluetoothSyncService;

        public BluetoothClassicServer(BluetoothSyncService bluetoothSyncService, UUID uuid)
        {
            try
            {
                this.bluetoothSyncService = bluetoothSyncService;
                serverSocket = bluetoothSyncService.bluetoothAdapter.ListenUsingInsecureRfcommWithServiceRecord(Constants.TAG, uuid);
            }
            catch (Exception e)
            {
                Log.Error(Constants.TAG, "Failed to set up Bluetooth Classic connection as a server", e);
            }
        }

        public override void Run()
        {
            BluetoothSocket socket = null;

            while (started)
            {
                String macAddress = null;
                try
                {
                    // This will block until there is a connection
                    //Log.d(TAG, "Bluetooth Classic server is listening for a client");
                    socket = serverSocket.Accept();
                    macAddress = socket.RemoteDevice.Address;
                    if (!bluetoothSyncService.openConnections.Contains(macAddress))
                    {
                        bluetoothSyncService.openConnections.Add(macAddress);
                        StreamSync.bidirectionalSync(socket.getInputStream(), socket.getOutputStream());
                    }
                    socket.Close();
                }
                catch (Exception connectException)
                {
                    Log.Error(Constants.TAG, "Failed to start a Bluetooth Classic connection as a server", connectException);

                    try
                    {
                        if (socket != null)
                            socket.Close();
                    }
                    catch (Exception closeException)
                    {
                        //Log.e(TAG, "Failed to close a Bluetooth Classic connection as a server", closeException);
                    }
                }
                if (macAddress != null)
                    bluetoothSyncService.openConnections.Remove(macAddress);
            }
        }
    }
}