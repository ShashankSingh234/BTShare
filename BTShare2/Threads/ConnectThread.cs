using Android.Bluetooth;
using Android.Util;
using BTShare2.Helpers;
using BTShare2.Services;
using Java.Lang;

namespace BTShare2.Threads
{
    /// <summary>
    /// This thread runs while attempting to make an outgoing connection
    /// with a device. It runs straight through; the connection either
    /// succeeds or fails.
    /// </summary>
    public class ConnectThread : Thread
    {
        BluetoothSocket socket;
        BluetoothDevice device;
        BluetoothChatService service;
        string socketType;

        public ConnectThread(BluetoothDevice device, BluetoothChatService service, bool secure)
        {
            this.device = device;
            this.service = service;
            BluetoothSocket tmp = null;
            socketType = secure ? "Secure" : "Insecure";

            try
            {
                if (secure)
                {
                    tmp = device.CreateRfcommSocketToServiceRecord(Constants.MY_UUID);
                }
                else
                {
                    tmp = device.CreateInsecureRfcommSocketToServiceRecord(Constants.MY_UUID);
                }

            }
            catch (Java.IO.IOException e)
            {
                Log.Error(Constants.TAG, "create() failed", e);
            }
            socket = tmp;
            service.state = Constants.STATE_CONNECTING;
        }

        public override void Run()
        {
            Name = $"ConnectThread_{socketType}";

            // Always cancel discovery because it will slow down connection
            service.btAdapter.CancelDiscovery();

            // Make a connection to the BluetoothSocket
            try
            {
                // This is a blocking call and will only return on a
                // successful connection or an exception
                socket.Connect();
            }
            catch (Java.IO.IOException e)
            {
                // Close the socket
                try
                {
                    socket.Close();
                }
                catch (Java.IO.IOException e2)
                {
                    Log.Error(Constants.TAG, $"unable to close() {socketType} socket during connection failure.", e2);
                }

                // Start the service over to restart listening mode
                service.ConnectionFailed();
                return;
            }

            // Reset the ConnectThread because we're done
            lock (this)
            {
                service.connectThread = null;
            }

            // Start the connected thread
            service.Connected(socket, device, socketType);
        }

        public void Cancel()
        {
            try
            {
                socket.Close();
            }
            catch (Java.IO.IOException e)
            {
                Log.Error(Constants.TAG, "close() of connect socket failed", e);
            }
        }
    }

}