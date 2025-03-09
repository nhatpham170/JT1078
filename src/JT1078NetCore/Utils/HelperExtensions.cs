using System.Text;

namespace JT1078NetCore.Utils
{
    public static class HelperExtensions
    {
        public static string LengthHexByte(this string value)
        {
            return HexByte(value.Length / 2);
        }
        public static string LengthHexInt16(this string value)
        {
            return HexInt16(value.Length / 2);
        }
        public static string HexByte(this int value)
        {
            return string.Format("{0:X2}", value & 0xFF);
        }
        public static string HexInt16(this int value)
        {
            return BytesToHex(BitConverter.GetBytes(value), 2);
        }
        public static string HexInt32(this int value)
        {
            return BytesToHex(BitConverter.GetBytes(value), 4);
        }
        public static string HexInt64(this long value)
        {
            return BytesToHex(BitConverter.GetBytes(value), 8);
        }
        public static string HexToStr(this string s)
        {
            byte[] bytes = new byte[s.Length / 2];
            int num = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                bytes[num++] = Convert.ToByte(s.Substring(i, 2), 0x10);
            }
            return Encoding.Default.GetString(bytes);
        }
        public static byte ToByte(this string str)
        {
            return Convert.ToByte(str);
        }
        public static short ToInt16(this string str)
        {
            return Convert.ToInt16(str);
        }
        public static int ToInt32(this string str)
        {
            return Convert.ToInt32(str);
        }
        public static long ToInt64(this string str)
        {
            return Convert.ToInt64(str);
        }
        public static int HexLength(this string str)
        {
            return str.Length / 2;
        }
        public static byte[] HexToBytes(this string hexString)
        {
            hexString = hexString.Replace(" ", "");
            byte[] buf = new byte[hexString.Length / 2];
            ReadOnlySpan<char> readOnlySpan = hexString.AsSpan();
            for (int i = 0; i < hexString.Length; i++)
            {
                if (i % 2 == 0)
                {
                    buf[i / 2] = Convert.ToByte(readOnlySpan.Slice(i, 2).ToString(), 16);
                }
            }
            return buf;
        }
        public static string ToHex(this string str, bool reverse = false)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in str.ToCharArray())
            {
                int num = Convert.ToInt32(ch);
                string str2 = string.Format("{0:X}", num);
                builder.Append(str2);
            }
            if (reverse)
            {
                return HexReverse(builder.ToString());
            }
            else return builder.ToString();
        }

        public static string HexReverse(this string str)
        {

            string result = string.Empty;
            string tmp = str;
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                if ((i % 2) == 1)
                {
                    result = $"{str[i - 1]}{str[i]}" + result;
                }
            }
            return result;
        }
        private static string BytesToHex(byte[] bytes, int count, bool reverse = false)
        {
            string result = "";
            if (reverse)
            {
                for (int i = 0; i < count; i++)
                {
                    byte tmp = 0;
                    if (i < bytes.Length)
                    {
                        tmp = bytes[i];

                    }
                    result += string.Format("{0:X2}", tmp);
                }
            }
            else
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    byte tmp = 0;
                    if (i < bytes.Length)
                    {
                        tmp = bytes[i];

                    }
                    result += string.Format("{0:X2}", tmp);
                }
            }

            return result;
        }
    }
}
