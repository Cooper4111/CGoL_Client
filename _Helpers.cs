using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    abstract class FieldProcessor
    {
        abstract public void Process();
    }

    class eventHandlerArr
    {
        private EventWaitHandle foo;
        private EventWaitHandle bar;

        public EventWaitHandle Foo
        {
            get => foo;
            private set => foo = value;
        }
        public EventWaitHandle Bar
        {
            get => bar;
            private set => bar = value;
        }
        public eventHandlerArr(EventWaitHandle F, EventWaitHandle B)
        {
            foo = F;
            bar = B;
        }
    }
    public class NetCodes
    {
        // Singletone
        private static Dictionary<string, int> dict;
        private static NetCodes instance;

        private NetCodes()
        {
            dict = new Dictionary<string, int>
            {
                { "wrongConnectionCode",    -1 },
                { "dialogue",                1 },
                { "connectionSuccessful",    2 },
                { "getFieldDimensions",      3 },
                { "getStream",               4 },
                { "authorizationRequest",    5 },
                { "authorizationBegin",      6 },
                { "authorizationSuccessful", 7 },
                { "authorizationFailed",     8 },
                { "struct",                  9 },
                { "acceptStruct",            10},
            };
        }

        public static NetCodes getInst()
        {
            if (instance == null)
            {
                instance = new NetCodes();
            }
            return instance;
        }

        public int this[string key]
        {
            get =>
                dict[key];
            private set =>
                dict.Add(key, value);
        }
    }

    public static class Helpers
    {
        public static int Crd2hash(int x, int y)
        {
            x = (x + Settings.FWidth) % Settings.FWidth;
            y = (y + Settings.FHeight) % Settings.FHeight;
            return x + y * Settings.FWidth;
        }
        public static int[] Hash2crd(int h)
        {
            int x = h % Settings.FWidth;
            return new int[2] { x, (h - x) / Settings.FWidth };
        }
        public static byte[] ToByte(this int i)
        {
            int[] foo = new int[] { i };
            byte[] bar = new byte[sizeof(int)];
            Buffer.BlockCopy(foo, 0, bar, 0, sizeof(int));
            return bar;
        }
    }
}
