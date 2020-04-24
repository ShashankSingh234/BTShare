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
using BTShare2.Helpers;
using BTShare2.Services;
using Java.Lang;

namespace BTShare2.Threads
{
    /// <summary>
    /// This thread runs while listening for incoming connections. It behaves
    /// like a server-side client. It runs until a connection is accepted
    /// (or until cancelled).
    /// </summary>
    public class AcceptThread : Thread
    {
        // The local server socket
        BluetoothServerSocket serverSocket;
        string socketType;
        BluetoothChatService service;

        public AcceptThread(BluetoothChatService service, bool secure)
        {
            BluetoothServerSocket tmp = null;
            socketType = secure ? "Secure" : "Insecure";
            this.service = service;

            try
            {
                if (secure)
                {
                    tmp = service.btAdapter.ListenUsingRfcommWithServiceRecord(Constants.NAME_SECURE, Constants.MY_UUID);
                }
                else
                {
                    tmp = service.btAdapter.ListenUsingInsecureRfcommWithServiceRecord(Constants.NAME_INSECURE, Constants.MY_UUID);
                }

            }
            catch (Java.IO.IOException e)
            {
                Log.Error(Constants.TAG, "listen() failed", e);
            }
            serverSocket = tmp;
            service.state = Constants.STATE_LISTEN;
        }

        public override void Run()
        {
            Name = $"AcceptThread_{socketType}";
            BluetoothSocket socket = null;

            while (service.GetState() != Constants.STATE_CONNECTED)
            {
                try
                {
                    socket = serverSocket.Accept();
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(Constants.TAG, "accept() failed", e);
                    break;
                }

                if (socket != null)
                {
                    lock (this)
                    {
                        switch (service.GetState())
                        {
                            case Constants.STATE_LISTEN:
                            case Constants.STATE_CONNECTING:
                                // Situation normal. Start the connected thread.
                                service.Connected(socket, socket.RemoteDevice, socketType);
                                break;
                            case Constants.STATE_NONE:
                            case Constants.STATE_CONNECTED:
                                try
                                {
                                    socket.Close();
                                }
                                catch (Java.IO.IOException e)
                                {
                                    Log.Error(Constants.TAG, "Could not close unwanted socket", e);
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void Cancel()
        {
            try
            {
                serverSocket.Close();
            }
            catch (Java.IO.IOException e)
            {
                Log.Error(Constants.TAG, "close() of server failed", e);
            }
        }
    }
}