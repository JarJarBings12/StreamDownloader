using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload
{
    public class Utils
    {
        public static int RoundDown(int value)
        {
            return value - value % 10;
        }
    }
}
