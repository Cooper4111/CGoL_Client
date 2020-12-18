using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    static class PerfMon
    {
        public static Stopwatch fieldProcessing = new Stopwatch();
        public static Stopwatch netListening    = new Stopwatch();
        public static Stopwatch global          = new Stopwatch();
        public static void CalcPerf()
        {
            TimeSpan GTS  = global.Elapsed;
            TimeSpan NLTS = netListening.Elapsed;
            TimeSpan FPTS = fieldProcessing.Elapsed;


            Trace.WriteLine(
                $"Net_part:  {NLTS.TotalMilliseconds / GTS.TotalMilliseconds}\t" +
                $"GUI_part:  {FPTS.TotalMilliseconds / GTS.TotalMilliseconds}\t" +
                $"Net_total: {NLTS}\t" +
                $"GUI_total: {FPTS}\t" +
                $"Total:     {GTS}\t"
                );
        }

    }
}
