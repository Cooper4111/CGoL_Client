using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace ClientApp
{
    class GolTcpClient
    {
        const int port = 8888;
        readonly string ServerIP;
        BinaryFormatter formatter;
        TcpClient client;
        NetCodes NetCodes;
        NetworkStream stream;
        EventWaitHandle gotSettingsFromServer;
        EventWaitHandle srvDialCtrl;
        EventWaitHandle FPReady;
        readonly object cmdLocker = new object();
        string cmd;

        public int structX = 0;
        public int structY = 0;
        public string gliderDir = "NE";
        public GolTcpClient(string ServerIP)
        {
            this.ServerIP = ServerIP;
            formatter     = new BinaryFormatter();
            client        = null;
            NetCodes      = NetCodes.getInst();

        }
        public string Cmd
        {
            get
            {
                lock (cmdLocker) { return cmd; }
            }
            set
            {
                lock (cmdLocker) { cmd = value; }
            }
        }
        
        /// <summary>
        /// Accepts array of TWO <c>EventWaitHandles</c>.
        /// Zero elem signals about successful aquirance of settings from server.
        /// First elem awaits for commnad to start dialog with server.
        /// </summary>
        /// <param name="HandlerArr"></param>
        public void ServerDialog(object HandlerArr)
        {
            this.gotSettingsFromServer = ((EventWaitHandle[])HandlerArr)[0]; // Sets when gotSettingsFromServer
            this.srvDialCtrl           = ((EventWaitHandle[])HandlerArr)[1];
            try
            {
                client      = new TcpClient(ServerIP, port);
                stream      = client.GetStream();
                int msgCode = 0;
                formatter.Serialize(stream, NetCodes["dialogue"]);
                msgCode     = (int)formatter.Deserialize(stream);
                if (msgCode == NetCodes["connectionSuccessful"])
                {
                    formatter.Serialize(stream, NetCodes["authorizationRequest"]);
                    msgCode = (int)formatter.Deserialize(stream);
                    if (msgCode == NetCodes["authorizationBegin"])
                    {
                        string loginPass     = Settings.login + " " + Settings.pass;
                        byte[] byteLoginPass = Encoding.Unicode.GetBytes(loginPass);
                        stream.Write(byteLoginPass, 0, byteLoginPass.Length);
                        msgCode = (int)formatter.Deserialize(stream);
                        if (msgCode == NetCodes["authorizationSuccessful"])
                        {
                            // Further code here
                        }
                        else
                        {
                            // AuthFailedMsg to client
                            // Retry
                        }
                    }
                    else
                    {
                        // AuthErrorMsg to client
                        // Retry
                    }
                    formatter.Serialize(stream, NetCodes["getFieldDimensions"]);
                    int[] dim = (int[])formatter.Deserialize(stream);
                    Settings.FWidth  = dim[0];
                    Settings.FHeight = dim[1];
                    gotSettingsFromServer.Set();
                    Trace.WriteLine("Starting listener");
                    while (true)
                    {
                        srvDialCtrl.WaitOne();
                        lock (cmdLocker)
                        {
                            if(Cmd == "struct")
                            {
                                formatter.Serialize(stream, NetCodes[cmd]);
                                msgCode = (int)formatter.Deserialize(stream);
                                if (msgCode == NetCodes["acceptStruct"])
                                {
                                    int[] glider = Structs.Glider(structX, structY, gliderDir);
                                    formatter.Serialize(stream, glider);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // ConnErrorMsg to client
                    // Retry
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                if(client != null)
                    client.Close();
            }
        }

        public void ListenServer(object Handler)
        {
            this.FPReady = (EventWaitHandle)Handler;
            try
            {
                client = new TcpClient(ServerIP, port);
                stream = client.GetStream();
                formatter.Serialize(stream, NetCodes["getStream"]);
                int bufferSize    = sizeof(int);
                byte[] dataBuffer = new byte[bufferSize];
                int[] data        = { };
                int[] msgLen      = { }; // array with one elem -- length in bytes
                int chunkLen      = 0;
                while (true)
                {
                    // get message length in bytes
                    stream.Read(dataBuffer, 0, sizeof(int));
                    msgLen = new int[1];
                    Buffer.BlockCopy(dataBuffer, 0, msgLen, 0, sizeof(int));

                    // get message
                    dataBuffer = new byte[msgLen[0] * sizeof(int)];
                    chunkLen = stream.Read(dataBuffer, 0, dataBuffer.Length);
                    data = new int[chunkLen / sizeof(int)];
                    Buffer.BlockCopy(dataBuffer, 0, data, 0, chunkLen);

                    // if (need_to_skip_frame) { data = new int[0]; } // and better do this before reading from stream
                    ThreadMaster.UpcomingGeneration = data;
                    FPReady.WaitOne();
                    ThreadMaster.ResumeFieldProcessing();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}
