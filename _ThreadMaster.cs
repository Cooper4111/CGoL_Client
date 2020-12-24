using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;

namespace ClientApp
{
    class PlayerCells
    {
        static byte[] color;
        static int[] cells;
    }


    static class ThreadMaster
    {
        static GolTcpClient GTCListener;
        static GolTcpClient GTCDialogue;
        static Thread clientListenThread;
        static Thread clientDialogueThread;
        static FieldProcessor FP;
        static EventWaitHandle FPCtrl;
        static EventWaitHandle srvDialCtrl;
        static EventWaitHandle gotDataFromServer;
        static EventWaitHandle FPReady;
        static object UGlocker = new object();
        static int[] upcomingGeneration;
        static string ServerIP = "127.0.0.1";
        public static PlayerCells[] upcomingGenerationColored;
        public static PlayerCells[] UpcomingGenerationColored
        {
            set
            {
                lock (UGlocker) { upcomingGenerationColored = value; }
            }
            get
            {
                lock (UGlocker)
                {
                    return upcomingGenerationColored;
                }
            }
        }
        public static int[] UpcomingGeneration
        {
            set
            {
                lock (UGlocker) { upcomingGeneration = value; }
            }
            get
            {
                lock (UGlocker) {
                    int[] dst = new int[upcomingGeneration.Length];
                    System.Buffer.BlockCopy(upcomingGeneration, 0, dst, 0, upcomingGeneration.Length * sizeof(int));
                    return dst; }
            }
        }



        static void RunNetClientDialogue()
        {
            try
            {
                GTCDialogue                       = new GolTcpClient(ServerIP);
                clientDialogueThread              = new Thread(GTCDialogue.ServerDialog);
                clientDialogueThread.IsBackground = true;
                clientDialogueThread.Start(new EventWaitHandle[2] { gotDataFromServer, srvDialCtrl });
            }
            catch(Exception E)
            {
                Console.WriteLine(E);
            }
        }
        static void RunNetClientListener()
        {
            try
            {
                GTCListener                     = new GolTcpClient(ServerIP);
                clientListenThread              = new Thread(GTCListener.ListenServerBroadcast);
                clientListenThread.IsBackground = true;
                clientListenThread.Start(FPReady);
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }
        }

        public static void RunFieldProcessor()
        {
            
            FP                    = new FieldProcessorSimple(FPCtrl, FPReady);
            Thread FPThread       = new Thread(FP.Process);
            FPThread.IsBackground = true;
            FPThread.Start();
        }
        public static void ResumeFieldProcessing() =>
            FPCtrl.Set();
        public static void Send2Srv(int x, int y, string gliderDir)
        {
            if(gliderDir == null || GTCDialogue == null)
            {
                return;
            }
            GTCDialogue.gliderDir = gliderDir;
            GTCDialogue.structX   = (int)(x - 15) / Settings.SqSide;
            GTCDialogue.structY   = (int)(y - 15) / Settings.SqSide;
            GTCDialogue.Cmd       = "struct";
            srvDialCtrl.Set();
        }
        
        public static void RUN(Canvas field, Dispatcher D, string targetIP, string login, string password)
        {
            FPCtrl               = new AutoResetEvent(false);
            FPReady              = new AutoResetEvent(true);    // 
            srvDialCtrl          = new AutoResetEvent(false);   // вынести это в соответствующие методы
            gotDataFromServer    = new AutoResetEvent(false);   //
            ServerIP             = targetIP;
            Settings.CanvasField = field;
            Settings.MainWinDisp = D;
            Settings.SqSide      = 5;
            Settings.login       = login;
            Settings.pass        = password;
            RunNetClientDialogue();
            gotDataFromServer.WaitOne();
            RunNetClientListener();
            RunFieldProcessor();
            
        }
    }
            
}
