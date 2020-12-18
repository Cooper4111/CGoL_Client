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
    // This one has higher performance comparing to SimpleFP, but inevitably throws KeyNotFound exception. Sooner or later.
    class FieldProcessorComplex : FieldProcessor
    {
        readonly Canvas CanvasField;
        readonly Dictionary<int, Rectangle> cellDict; // MAKE IT PROPERTY OR CLASS!!!!!!
        Stack<Rectangle> rectStack;
        int sqSide;
        int FWidth;
        int FHeight;
        Dispatcher MainWinDisp;
        SolidColorBrush cellColor;
        EventWaitHandle FPCtrl;
        EventWaitHandle FPReady;
        object dickLocker = new object();

        
        //      MAKE
        //       IT
        //   SINGLETONE
        //      !!!!
        public FieldProcessorComplex(EventWaitHandle FPCtrl, EventWaitHandle FPReady)
        {
            MainWinDisp      = Settings.MainWinDisp;
            this.FWidth      = Settings.FWidth;
            this.FHeight     = Settings.FHeight;
            this.CanvasField = Settings.CanvasField;
            cellDict         = new Dictionary<int, Rectangle>();
            rectStack        = new Stack<Rectangle>();
            sqSide           = Settings.SqSide;
            cellColor        = new SolidColorBrush(Color.FromArgb(0xFF,0x0,0xFF,0x0));
            this.FPCtrl      = FPCtrl;
            this.FPReady     = FPReady;
        }

        int[] Hash2crd(int h)
        {
            int x = h % FWidth;
            return new int[2] { x, (h - x) / FWidth };
        }   
        async void GrowthAsync(int[] CellsToBorn)
        {
            foreach (int hash in CellsToBorn)
            {
                int[] crd = Hash2crd(hash);
                await MainWinDisp.BeginInvoke((ThreadStart)delegate ()
                {
                    Rectangle rec = new Rectangle
                    {
                        Width = sqSide,
                        Height = sqSide,
                        Fill = cellColor
                    };
                    CanvasField.Children.Add(rec);
                    Canvas.SetLeft(rec, crd[0] * sqSide);
                    Canvas.SetTop(rec, crd[1] * sqSide);
                    lock (dickLocker)
                    {
                        //cellDict.Add(hash, rec);
                        cellDict[hash] = rec;     // More stable, but doesn't solve the problem
                    }
                },
                DispatcherPriority.Normal);
            }
        }
        async void ApoptosisAsync(int[] CellsToDelete)
        {
            Rectangle rec;
            foreach (int hash in CellsToDelete)
            {
                int[] crd = Hash2crd(hash);
                try
                {
                    rec = cellDict[hash];
                }
                catch (Exception E)
                {
                    continue;
                }
                await MainWinDisp.BeginInvoke((ThreadStart)delegate ()
                {
                    CanvasField.Children.Remove(rec);
                    lock (dickLocker)
                    {
                        cellDict.Remove(hash);
                    }
                },
                DispatcherPriority.Normal);
            }
        }
        int[] GetCellsToDelete(int[] nextGen)
        {
            lock (dickLocker) { return (int[])cellDict.Keys.Except(nextGen).ToArray(); }
        }
        int[] GetCellsToBorn(int[] nextGen)
        {
            lock (dickLocker) { return (int[])nextGen.Except(cellDict.Keys).ToArray(); }
        }
        override public void Process()
        {
            while (true)
            {
                this.FPCtrl.WaitOne();
                int[] nextGen = ThreadMaster.UpcomingGeneration;
                if (nextGen.Length != 0)
                {
                    int[] CellsToBorn   = GetCellsToBorn(nextGen);
                    int[] CellsToDelete = GetCellsToDelete(nextGen);
                    ApoptosisAsync(CellsToDelete);
                    GrowthAsync(CellsToBorn);
                }
                FPReady.Set();
            }
        }
    }
}
