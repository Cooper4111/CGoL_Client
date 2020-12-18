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
    static class Settings
    {
        private static int sqSide  = 0;
        private static int fWidth  = 0;
        private static int fHeight = 0;
        private static Canvas canvasField        = null;
        private static Dispatcher mainWinDisp    = null;
        //private static SolidColorBrush cellColor = null; // TODO

        public static Canvas CanvasField
        {
            get => canvasField;
            set
            {
                if (canvasField == null)
                    canvasField = value;
            }
        }
        public static Dispatcher MainWinDisp
        {
            get => mainWinDisp;
            set
            {
                if (mainWinDisp == null)
                    mainWinDisp = value;
            }
        }

        public static int FWidth
        {
            get => fWidth;
            set
            {
                if (fWidth == 0)
                    fWidth = value;
            }
        }
        public static int FHeight
        {
            get => fHeight;
            set
            {
                if (fHeight == 0)
                    fHeight = value;
            }
        }
        public static int SqSide
        {
            get => sqSide;
            set
            {
                if (sqSide == 0)
                    sqSide = value;
            }
        }
    }
}
