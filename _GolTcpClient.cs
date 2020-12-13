using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;

namespace ClientApp
{
    class GolTcpClient
    {
        const int port = 8888;              // MOVE TO SETTINGS
        const string address = "127.0.0.1"; // MOVE TO SETTINGS
        BinaryFormatter formatter;
        TcpClient client;
        NetCodes NetCode;
        NetworkStream stream;
        EventWaitHandle gotSettingsFromServer;
        EventWaitHandle srvDialCtrl;
        readonly object cmdLocker = new object();
        string cmd;

        public int structX = 0;
        public int structY = 0;
        public string gliderDir = "NE";
        public GolTcpClient()
        {
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
            this.gotSettingsFromServer = ((EventWaitHandle[])HandlerArr)[0];
            this.srvDialCtrl           = ((EventWaitHandle[])HandlerArr)[1];
            try
            {
                client      = new TcpClient(address, port);
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

        public void ListenServer()
        {
            try
            {
                client = new TcpClient(address, port);
                stream = client.GetStream();
                formatter.Serialize(stream, NetCode["getStream"]);
                byte[] rawData;
                int[] data;
                while (true)
                {
                    /* 
                        Расспараллелить потом нормально эту хуергу,
                        чтобы он чанками по 1КБ отдавал и можно было обрабатывать
                        предыдущий чанк, пока грузится текущий
                        (Если будет поле 2000х1000 хотя бы на половину заселено -- это уже ~1МБ).
                        Нет, сейчас так не работает. Наверное. Может сломаться, когда данные в два чанка залетят.
                        А может и не сломаться. ¯\_(ツ)_/¯
                    */
                    rawData = new byte[1024];
                    int bytesCount = 0;
                    do
                    {
                        bytesCount = stream.Read(rawData, 0, rawData.Length);
                        data = new int[bytesCount / sizeof(int)];
                        Buffer.BlockCopy(rawData, 0, data, 0, bytesCount);
                        ThreadMaster.UpcomingGeneration = data;
                        ThreadMaster.ResumeFieldProcessing();
                    }
                    while (stream.DataAvailable);
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
