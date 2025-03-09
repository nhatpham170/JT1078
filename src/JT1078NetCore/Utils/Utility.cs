using System.Globalization;
using System.Text;

namespace JT1078NetCore.Utils
{
    public class Utility
    {

        public static int HexToInt(string hex)
        {
            int num = 0;
            try
            {
                num = int.Parse(hex, NumberStyles.HexNumber);
            }
            catch (Exception exception)
            {
                ExceptionHandler.ExceptionProcess(exception);
            }
            return num;
        }
        public static string ReverseByte(string hexString)
        {
            byte[] array = HexToBytes(hexString);
            Array.Reverse(array);
            return BitConverter.ToString(array, 0, array.Length).Replace("-", string.Empty);
        }
        public static string HexToStr(string s)
        {
            byte[] bytes = new byte[s.Length / 2];
            int num = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                bytes[num++] = Convert.ToByte(s.Substring(i, 2), 0x10);
            }
            return Encoding.Default.GetString(bytes);
        }

        public static string ByteToHex(byte[] s)
        {
            string hex = BitConverter.ToString(s).Replace("-", "");
            return hex;
        }
        public static byte[] HexToBytes(string s)
        {
            byte[] bytes = new byte[s.Length / 2];
            int num = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                bytes[num++] = Convert.ToByte(s.Substring(i, 2), 0x10);
            }
            return bytes;
        }

        public static string IntToHexByte(int value, int count = 1)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            string result = "";
            if (count <= 0)
            {
                count = bytes.Length;
            }
            for (int i = 0; i < count; i++)
            {
                result += string.Format("{0:X2}", bytes[i]);

            }
            return result;
        }

        public static string IntToHexByteRe(int value, int count = 1)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            string result = "";
            if (count <= 0)
            {
                count = bytes.Length;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                result += string.Format("{0:X2}", bytes[i]);

            }
            return result;
        }

        public static string StrToHex(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in str.ToCharArray())
            {
                int num = Convert.ToInt32(ch);
                string str2 = string.Format("{0:X}", num);
                builder.Append(str2);
            }
            return builder.ToString();
        }

    }
}
