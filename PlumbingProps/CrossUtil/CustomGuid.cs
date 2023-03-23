using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlumbingProps.CrossUtil
{
    public class CustomGuid
    {
        private static System.Guid GUID
        {
            get
            {
                return System.Guid.NewGuid();
            }
        }

        public static String GetGuid()
        {
            byte[] bArr = GUID.ToByteArray();
            int autonum = BitConverter.ToInt32(bArr, 0);
            autonum = Math.Abs(autonum) + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
            String _sGUID = autonum.ToString();
            return _sGUID;
        }
    }
}
