using System;
using System.Collections.Generic;
using System.IO;
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
using BTShare2.Helpers;
using BTShare2.Services;
using Java.Lang;

namespace BTShare2.Threads
{
    /// <summary>
    /// This thread runs during a connection with a remote device.
    /// It handles all incoming and outgoing transmissions.
    /// </summary>
    public class ConnectedThread : Thread
    {
        BluetoothSocket socket;
        Stream inStream;
        Stream outStream;
        BluetoothChatService service;

        public ConnectedThread(BluetoothSocket socket, BluetoothChatService service, string socketType)
        {
            Log.Debug(Constants.TAG, $"create ConnectedThread: {socketType}");
            this.socket = socket;
            this.service = service;
            Stream tmpIn = null;
            Stream tmpOut = null;

            // Get the BluetoothSocket input and output streams
            try
            {
                tmpIn = socket.InputStream;
                tmpOut = socket.OutputStream;
            }
            catch (Java.IO.IOException e)
            {
                Log.Error(Constants.TAG, "temp sockets not created", e);
            }

            inStream = tmpIn;
            outStream = tmpOut;
            service.state = Constants.STATE_CONNECTED;
        }

        public override void Run()
        {
            Log.Info(Constants.TAG, "BEGIN mConnectedThread");
            byte[] buffer = new byte[1024];
            int bytes;

            // Keep listening to the InputStream while connected
            while (service.GetState() == Constants.STATE_CONNECTED)
            {
                try
                {
                    // Read from the InputStream
                    bytes = inStream.Read(buffer, 0, buffer.Length);
                    var t = Encoding.ASCII.GetString(buffer);
                    service.mainActivity.RunOnUiThread(() =>
                    {
                        service.mainActivity.mText.Text = t;
                    });
                    // Send the obtained bytes to the UI Activity
                    //service.mainActivity
                    //       .ObtainMessage(MESSAGE_READ, bytes, -1, buffer)
                    //       .SendToTarget();
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(Constants.TAG, "disconnected", e);
                    service.ConnectionLost();
                    break;
                }
            }
        }

        /// <summary>
        /// Write to the connected OutStream.
        /// </summary>
        /// <param name='buffer'>
        /// The bytes to write
        /// </param>
        public void Write(byte[] buffer)
        {
            try
            {
                outStream.Write(buffer, 0, buffer.Length);
            }
            catch (Java.IO.IOException e)
            {
                Log.Error(Constants.TAG, "Exception during write", e);
            }
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