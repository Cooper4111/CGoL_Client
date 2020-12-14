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
        static object UGlocker = new object();
        static int[] upcomingGeneration;
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
                    System.Buffer.BlockCopy(upcomingGeneration, 0, dst, 0, upcomingGeneration.Length);
                    return upcomingGeneration; }
            }
        }

        static void RunNetClientDialogue()
        {
            try
            {
                GTCDialogue                       = new GolTcpClient();
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
                GTCListener                     = new GolTcpClient();
                clientListenThread              = new Thread(GTCListener.ListenServer);
                clientListenThread.IsBackground = true;
                clientListenThread.Start();
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }
        }

        public static void RunFieldProcessor()
        {
            FPCtrl                = new AutoResetEvent(false);
            FP                    = new FieldProcessor(FPCtrl);
            Thread FPThread       = new Thread(FP.Process);
            FPThread.IsBackground = true;
            FPThread.Start();
        }
        public static void ResumeFieldProcessing()
        {
            FPCtrl.Set();
        }
        public static void Send2Srv(int x, int y, string gliderDir)
        {
            if(gliderDir == null)
            {
                return;
            }
            GTCDialogue.gliderDir = gliderDir;
            GTCDialogue.structX   = (int)(x - 15) / Settings.SqSide;
            GTCDialogue.structY   = (int)(y - 15) / Settings.SqSide;
            GTCDialogue.Cmd       = "struct";
            srvDialCtrl.Set();
        }
        
        public static void RUN(Canvas field, Dispatcher D)
        {
            srvDialCtrl          = new AutoResetEvent(false);
            gotDataFromServer    = new AutoResetEvent(false);
            Settings.CanvasField = field;
            Settings.MainWinDisp = D;
            Settings.SqSide      = 5;
            RunNetClientDialogue();
            gotDataFromServer.WaitOne();
            RunNetClientListener();
            RunFieldProcessor();
        }
    }
            
}
