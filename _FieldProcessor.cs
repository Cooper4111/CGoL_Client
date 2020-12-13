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
        readonly Dictionary<int, Rectangle> cellDict;
        int sqSide;
        int FWidth;
        int FHeight;
        Dispatcher MainWinDisp;
        SolidColorBrush cellColor;
        EventWaitHandle wh;
        
        //      MAKE
        //       IT
        //   SINGLETONE
        //      !!!!
        public FieldProcessor(EventWaitHandle wh)
        {
            MainWinDisp      = Settings.MainWinDisp;
            this.FWidth      = Settings.FWidth;
            this.FHeight     = Settings.FHeight;
            this.CanvasField = Settings.CanvasField;
            cellDict         = new Dictionary<int, Rectangle>();
            sqSide           = Settings.SqSide;
            cellColor        = new SolidColorBrush(Color.FromArgb(0xFF,0x0,0xFF,0x0));
            this.wh          = wh;
        }

        public void Process()
        {
            while (true)
            {
                this.wh.WaitOne();
                int[] hoo = ThreadMaster.UpcomingGeneration;
                Apoptosis(hoo);
                Growth(hoo);
            }
        }

        int Crd2hash(int x, int y)
        {
            x = (x + FWidth) % FWidth;
            y = (y + FHeight) % FHeight;
            return x + y * FWidth;
        }
        int[] Hash2crd(int h)
        {
            int x = h % FWidth;
            return new int[2] { x, (h - x) / FWidth };
        }

        /// <param name="nextGen"> The array of hashed cell coordinates in upcoming generation </param>
        /// <returns> HashSet with hashed coordinates of cells, that must die in upcoming generation </returns>
        HashSet<int> GetCellsToDelete(int[] nextGen)=>
            cellDict.Keys.Except(nextGen).ToHashSet<int>();
        /// <param name="nextGen"> The array of hashed cell coordinates in upcoming generation </param>
        /// <returns> HashSet with hashed coordinates of cells, that must born in upcoming generation </returns
        HashSet<int> GetCellsToBorn(int[] nextGen) =>
            nextGen.Except(cellDict.Keys).ToHashSet<int>();

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
                cellDict.Add(hash, rec);
            },
            DispatcherPriority.Normal);
        }
        void DelRect(int hash)
        {
            int[] crd = Hash2crd(hash);
            Rectangle rec = cellDict[hash];

            MainWinDisp.BeginInvoke((ThreadStart)delegate ()
            {
                CanvasField.Children.Remove(rec);
                cellDict.Remove(hash);
            },
            DispatcherPriority.Normal);
        }
        void Growth(int[] hashArr)
        {
            foreach (int foo in GetCellsToBorn(hashArr))
            {
                MakeRect(foo);
            }
        }
        void Apoptosis(int[] hashArr)
        {
            foreach(int foo in GetCellsToDelete(hashArr))
            {
                DelRect(foo);
            }
        }
    }
}
