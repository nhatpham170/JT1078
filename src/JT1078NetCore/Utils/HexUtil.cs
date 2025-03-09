namespace JT1078NetCore.Utils
{
    public enum DataType
    {
        Byte = 1,
        Int16 = 2,
        Int32 = 4,
        Int64 = 8,
        BDC12 = 6,
        BDC10 = 5,
        Datetime = 6,
        Time = 3,
        Date = 3,
    }
    public class HexUtil
    {
        private string _hex = string.Empty;
        private int _index = 0;
        private int _length = 0;

        public HexUtil(string strHex)
        {
            _index = 0;
            _length = strHex.Length / 2;
            _hex = strHex;
        }
        public int Index()
        {
            return _index;
        }
        public string Slice(int start, int length = 0)
        {
            _index = 0;
            if (length == 0)
            {
                _hex = _hex.Substring(start * 2);
            }
            else
            {
                _hex = _hex.Substring(start * 2, length * 2);
            }
            _index = 0;
            _length = _hex.Length / 2;
            return _hex;
        }

        public string ReadStart(int start, int length = 0)
        {
            string result = "";
            _index = length;
            if (length == 0)
            {
                result = _hex.Substring(start * 2);
            }
            else
            {
                result = _hex.Substring(start * 2, length * 2);
            }
            _index = 0;
            _hex = _hex.Substring(length * 2);
            _length = _hex.Length / 2;
            return result;
        }
        public string Release()
        {
            _hex = _hex.Substring(_index * 2);
            _index = 0;
            _length = _hex.Length / 2;
            return _hex;
        }
        public string Reset(int index)
        {
            _index = 0;
            _hex = _hex.Substring(index * 2);
            _length = _hex.Length / 2;
            return _hex;
        }

        public void SetIndex(int index) { _index = index; }
        public string Copy(int start, int length = 0)
        {
            if (length == 0)
            {
                return _hex.Substring(start * 2);
            }
            else
            {
                return _hex.Substring(start * 2, length * 2);
            }

        }
        /// <summary>
        /// Index byte
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public int FindIndex(string search, int start = 0)
        {
            if (Remain() > 0)
            {
                string hex = _hex.Substring(start * 2);
                int index = hex.IndexOf(search);
                if (index >= 0)
                {
                    return index / 2;
                }
            }

            return -1;
        }
        /// <summary>
        /// Bytes remain
        /// </summary>
        /// <returns></returns>
        public int Remain()
        {
            return _length - _index;
        }

        public string ReadType(DataType type)
        {
            return ReadBytes((int)type);
        }
        public string ReadBCD(int count = 6)
        {
            return ReadBytes(count);
        }
        public long ReadInt64(bool reserve = false)
        {
            return (long)Convert.ToUInt64(ReadBytes(8, reserve), 0x10);
        }
        public int ReadInt32(bool reserve = false)
        {
            return (int)Convert.ToUInt32(ReadBytes(4, reserve), 0x10);
        }
        public int ReadInt16(bool reserve = false)
        {
            return Convert.ToUInt16(ReadBytes(2, reserve), 0x10);
        }
        public byte ReadByte()
        {
            return Convert.ToByte(ReadBytes(1), 0x10);
        }
        public void Skip(int length)
        {
            _index += length;
        }
        public void Skip(DataType type)
        {
            _index += (int)type;
        }

        public string ReadRemain()
        {
            int count = _length - _index;
            return ReadBytes(count);
        }

        public string ReadBytes(int length, bool reserve = false)
        {
            string str = string.Empty;
            if (reserve)
            {
                for (int i = 0; i < length; i++)
                {
                    str = _hex.Substring((_index + i) * 2, 2) + str;
                }
            }
            else
            {
                str = _hex.Substring(_index * 2, length * 2);
            }
            _index += length;
            return str;
        }
    }
}
