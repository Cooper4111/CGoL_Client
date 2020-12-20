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
    // This one has lower performance comparing to ComplexFB, but is more stable.
    class FieldProcessorSimple : FieldProcessor
    {
        readonly EventWaitHandle FPCtrl;
        readonly EventWaitHandle FPReady;
        readonly Canvas CanvasField;
        readonly int sqSide;
        readonly int FWidth;
        Stack<Rectangle> rectStack;
        Dispatcher MainWinDisp;

        public FieldProcessorSimple(EventWaitHandle FPCtrl, EventWaitHandle FPReady)
        {
            MainWinDisp  = Settings.MainWinDisp;
            FWidth       = Settings.FWidth;
            CanvasField  = Settings.CanvasField;
            sqSide       = Settings.SqSide;
            this.FPCtrl  = FPCtrl;
            this.FPReady = FPReady;

        }

        int[] Hash2crd(int h)
        {
            int x = h % FWidth;
            return new int[2] { x, (h - x) / FWidth };
        }
        async void ClearFieldAsync(Stack<Rectangle> rectStackOld)
        {
            Rectangle rect;
            if (rectStackOld.Count == 0)
                return;
            int stackCount = rectStackOld.Count;
            await MainWinDisp.BeginInvoke((ThreadStart)delegate ()
                {
                    for (int i = 0; i < stackCount; i++)
                    {
                        rect = rectStackOld.Pop();
                        CanvasField.Children.Remove(rect);
                    }
                },
                DispatcherPriority.Normal);
        }
        async Task<Stack<Rectangle>> InhabitCellColor(int[] nextGen)
        {
            SolidColorBrush localCellColor;
            Stack<Rectangle> result = new Stack<Rectangle>();

            await MainWinDisp.BeginInvoke((ThreadStart)delegate ()
            {
                int curIndex = 0;
                int endIndex = 0;

                while (curIndex < nextGen.Length)
                {
                    endIndex += nextGen[curIndex];
                    curIndex++;
                    byte[] byteColor = nextGen[curIndex].ToByte();
                    localCellColor = new SolidColorBrush(Color.FromArgb(byteColor[0], byteColor[1], byteColor[2], byteColor[3]));
                    curIndex++;
                    while (curIndex < endIndex)
                    {
                        int[] crd = Hash2crd(nextGen[curIndex]);
                        Rectangle rect = new Rectangle
                        {
                            Width = sqSide,
                            Height = sqSide,
                            Fill = localCellColor
                        };
                        CanvasField.Children.Add(rect);
                        Canvas.SetLeft(rect, crd[0] * sqSide);
                        Canvas.SetTop(rect, crd[1] * sqSide);
                        result.Push(rect);
                        curIndex++;
                    }
                }
            },
            DispatcherPriority.Normal);
            return result;
        }

        override public void Process()
        {
            rectStack = new Stack<Rectangle>();
            while (true)
            {
                this.FPCtrl.WaitOne();
                int[] nextGen = ThreadMaster.UpcomingGeneration;
                if (nextGen.Length != 0){
                    ClearFieldAsync(rectStack);
                    rectStack = InhabitCellColor(nextGen).Result;
                }
                FPReady.Set();
            }
        }
    }
}
