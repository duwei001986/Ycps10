using CardWriteReadT10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WorkLog;

namespace Mytest 
{
    class CardReadT10
    {
        public static String byteToChar(int length, byte[] data)
        {
            StringBuilder stringbuiler = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                String temp = data[i].ToString("x");
                if (temp.Length == 1)
                {
                    stringbuiler.Append("0" + temp);
                }
                else
                {
                    stringbuiler.Append(temp);
                }
            }
            return (stringbuiler.ToString());
        }

        public static int ReadCard(out string resetCode)
        {
            resetCode = "";
            string strInstruct = "";
            int iHandle;
            int st;
            byte[] snr = new byte[128];
            byte rlen = 0;
            byte[] rbuff = new byte[128];
            iHandle = T10ReaderHelp.dc_init(100, 115200);
            if (iHandle < 0)
            {
                WriteWorkLog.WriteLogs("IC卡操作日志", "Err", "设备未打开");
                return -1;
            }
            st = T10ReaderHelp.dc_setcpu(iHandle, 0x0c);
            if (st != 0)
            {
                WriteWorkLog.WriteLogs("IC卡操作日志", "Err", "dc set cpu error");
                T10ReaderHelp.dc_exit(iHandle);
                return -101;
            }
            st = T10ReaderHelp.dc_cpureset(iHandle, ref rlen, snr);
            if (st != 0)
            {
                WriteWorkLog.WriteLogs("IC卡操作日志", "Info", "IC复位失败");
                T10ReaderHelp.dc_exit(iHandle);
                return -2;
            }
            else
            {
                WriteWorkLog.WriteLogs("IC卡操作日志", "Info", "复位成功:" + byteToChar(rlen, snr));
            }
            resetCode = byteToChar(rlen, snr);
            T10ReaderHelp.dc_beep(iHandle, 10);
            T10ReaderHelp.dc_exit(iHandle);
            return 0;
        }
      public static string ByteToString(byte[] InBytes)
        {
            string StringOut = "";
            foreach (byte InByte in InBytes)
            {
                StringOut = StringOut + String.Format("{0:X2}", InByte);
            }
            return StringOut;
        }
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        /// <summary>
        /// 从汉字转换到16进制
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetHexFromChs(string s)
        {
            //if ((s.Length % 2) != 0)
            //{
            //    s += " ";//空格
            //             //throw new ArgumentException("s is not valid chinese string!");
            //}

            System.Text.Encoding chs = System.Text.Encoding.GetEncoding("gb2312");

            byte[] bytes = chs.GetBytes(s);

            string str = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("{0:X}", bytes[i]);
            }

            return str;
        }
        /// <summary>
        /// 从16进制转换成汉字
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string GetChsFromHex(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            //if (hex.Length % 2 != 0)
            //{
            //    hex += "20";//空格
            //                //throw new ArgumentException("hex is not a valid number!", "hex");
            //}
            // 需要将 hex 转换成 byte 数组。
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                        System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message.
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }

            // 获得 GB2312，Chinese Simplified。
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding("gb2312");


            return chs.GetString(bytes);
        }
        private static bool SendCommand(int icdev, string strInstruct, out string strRData)
        {
            strRData = "";
            byte rlen = 0;
            byte[] rbuff = new byte[128];
            int iRet = T10ReaderHelp.dc_cpuapdu(icdev, Convert.ToByte(strInstruct.Length / 2), strToToHexByte(strInstruct), ref rlen, rbuff);
            if (iRet == 0)
            {
                strRData = byteToChar(rlen, rbuff); // ByteToString(rbuff);
                return true;
            }
            else
            {
                WriteWorkLog.WriteLogs("IC卡操作日志", "Info", strInstruct + "命令发送失败");
                return false;
            }
        }

    }
}
