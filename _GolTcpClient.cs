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
        NetCodes NetCode;
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
            formatter = new BinaryFormatter();
            client    = null;
            NetCode   = NetCodes.getInst();

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
                formatter.Serialize(stream, NetCode["dialogue"]);
                msgCode     = (int)formatter.Deserialize(stream);
                if (msgCode == NetCode["connectionSuccessful"])
                {
                    formatter.Serialize(stream, NetCode["getFieldDimensions"]);
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
                                formatter.Serialize(stream, NetCode[cmd]);
                                msgCode = (int)formatter.Deserialize(stream);
                                if (msgCode == NetCode["acceptStruct"])
                                {
                                    int[] glider = Structs.Glider(structX, structY, gliderDir);
                                    formatter.Serialize(stream, glider);
                                }
                            }
                        }
                    }
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
                formatter.Serialize(stream, NetCode["getStream"]);
                int bufferSize = 1024; //1073741824; // 1MB //Settings.FWidth * Settings.FHeight * 2; // Assuming there will be never more than 0.5 field size cells
                byte[] dataBuffer;
                int[] data;
                int bytesCount;
                dataBuffer = new byte[bufferSize];
                while (true)
                {
                    bytesCount = 0;
                    do
                    {
                        int foo = stream.Read(dataBuffer, 0, sizeof(int));
                        data = new int[sizeof(int)];
                        Buffer.BlockCopy(dataBuffer, 0, data, 0, sizeof(int));

                        dataBuffer = new byte[data[0]*sizeof(int)];
                        bytesCount = stream.Read(dataBuffer, 0, dataBuffer.Length);
                        data = new int[bytesCount / sizeof(int)];
                        Buffer.BlockCopy(dataBuffer, 0, data, 0, bytesCount);
                    }
                    while (stream.DataAvailable);
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
