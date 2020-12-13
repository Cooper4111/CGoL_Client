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
    static class Structs
    {
        public static int[] Glider(int x, int y, string gliderDir)
        {
            if (gliderDir == "NE")
            {
                return new int[] {
                    Helpers.Crd2hash(0 + x,0 + y),
                    Helpers.Crd2hash(1 + x,0 + y),
                    Helpers.Crd2hash(2 + x,0 + y),
                    Helpers.Crd2hash(2 + x,1 + y),
                    Helpers.Crd2hash(1 + x,2 + y)
                };
            }
            if (gliderDir == "NW")
            {
                return new int[] {
                    Helpers.Crd2hash(0 + x,0 + y),
                    Helpers.Crd2hash(1 + x,0 + y),
                    Helpers.Crd2hash(2 + x,0 + y),
                    Helpers.Crd2hash(0 + x,1 + y),
                    Helpers.Crd2hash(1 + x,2 + y)
                };
            }
            if (gliderDir == "SW")
            {
                return new int[] {
                    Helpers.Crd2hash(0 + x,2 + y),
                    Helpers.Crd2hash(1 + x,2 + y),
                    Helpers.Crd2hash(2 + x,2 + y),
                    Helpers.Crd2hash(0 + x,1 + y),
                    Helpers.Crd2hash(1 + x,0 + y)
                };
            }
            if (gliderDir == "SE")
            {
                return new int[] {
                    Helpers.Crd2hash(0 + x,2 + y),
                    Helpers.Crd2hash(1 + x,2 + y),
                    Helpers.Crd2hash(2 + x,2 + y),
                    Helpers.Crd2hash(2 + x,1 + y),
                    Helpers.Crd2hash(1 + x,0 + y)
                };
            }
            else
            {
                return null;
            }
        }
    }
}
