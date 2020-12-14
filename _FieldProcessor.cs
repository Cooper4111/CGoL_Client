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
    class FieldProcessor
    {
        readonly Canvas CanvasField;
        readonly Dictionary<int, Rectangle> cellDict; // MAKE IT PROPERTY OR CLASS!!!!!!
        int sqSide;
        int FWidth;
        int FHeight;
        Dispatcher MainWinDisp;
        SolidColorBrush cellColor;
        EventWaitHandle FPCtrl;
        object dickLocker = new object();

        
        //      MAKE
        //       IT
        //   SINGLETONE
        //      !!!!
        public FieldProcessor(EventWaitHandle FPCtrl)
        {
            MainWinDisp      = Settings.MainWinDisp;
            this.FWidth      = Settings.FWidth;
            this.FHeight     = Settings.FHeight;
            this.CanvasField = Settings.CanvasField;
            cellDict         = new Dictionary<int, Rectangle>();
            sqSide           = Settings.SqSide;
            cellColor        = new SolidColorBrush(Color.FromArgb(0xFF,0x0,0xFF,0x0));
            this.FPCtrl      = FPCtrl;
        }

        int[] Hash2crd(int h)
        {
            int x = h % FWidth;
            return new int[2] { x, (h - x) / FWidth };
        }

        /// <param name="nextGen"> The array of hashed cell coordinates in upcoming generation </param>
        /// <returns> HashSet with hashed coordinates of cells, that must born in upcoming generation </returns
        HashSet<int> GetCellsToBorn(int[] nextGen)
        {
            lock (dickLocker) { return nextGen.Except(cellDict.Keys).ToHashSet<int>(); }
        }
            
        void MakeRect(int hash)
        {
            int[] crd = Hash2crd(hash);
            MainWinDisp.BeginInvoke((ThreadStart)delegate ()
            {
                Rectangle rec = new Rectangle
                {
                    Width  = sqSide,
                    Height = sqSide,
                    Fill   = cellColor
                };
                CanvasField.Children.Add(rec);
                Canvas.SetLeft(rec, crd[0] * sqSide);
                Canvas.SetTop(rec, crd[1] * sqSide);
                lock(dickLocker)
                {
                    //cellDict.Add(hash, rec);
                    cellDict[hash] = rec;     // More stable, but doesn't sove the problem
                }
            },
            DispatcherPriority.Normal);
        }
        void Growth(int[] nextGen)
        {
            List<int> bar = GetCellsToBorn(nextGen).ToList();
            foreach (int foo in bar)
            {
                MakeRect(foo);
            }
        }
        /// <param name="nextGen"> The array of hashed cell coordinates in upcoming generation </param>
        /// <returns> HashSet with hashed coordinates of cells, that must die in upcoming generation </returns>
        HashSet<int> GetCellsToDelete(int[] nextGen)
        {
            lock (dickLocker) { return cellDict.Keys.Except(nextGen).ToHashSet<int>(); }
        }
        void DelRect(int hash)
        {
            int[] crd = Hash2crd(hash);
            Rectangle rec = cellDict[hash];

            MainWinDisp.BeginInvoke((ThreadStart)delegate ()
            {
                CanvasField.Children.Remove(rec);
                lock (dickLocker)
                {
                    cellDict.Remove(hash);
                }
            },
            DispatcherPriority.Normal);
        }
        void Apoptosis(int[] nextGen)
        {
            List<int> bar = GetCellsToDelete(nextGen).ToList();
            foreach (int foo in bar)
            {
                DelRect(foo);
            }
        }
        public void Process()
        {
            while (true)
            {
                this.FPCtrl.WaitOne();
                //int[] nextGen = ThreadMaster.UpcomingGeneration;
                Apoptosis(ThreadMaster.UpcomingGeneration);
                Growth(ThreadMaster.UpcomingGeneration);
            }
        }
    }
}
