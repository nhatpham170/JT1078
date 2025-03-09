using static System.Net.Mime.MediaTypeNames;

namespace JT1078NetCore.Utils
{
    public class Log
    {
        //public static readonly string LogPathStr = ConfigurationManager.AppSettings["LogPath"];
        public static readonly string LogPathStr = "E:/demo/";
        public static void WriteExceptionLog(string context)
        {
            string path = LogPath() + DateTime.Now.ToString("yyyyMMdd") + "\\ExceptionLog.txt";

            TextWriter textWriter = (TextWriter)null;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                textWriter = TextWriter.Synchronized((TextWriter)File.AppendText(path));
                textWriter.Write(Environment.NewLine + context);
            }
            catch { }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        public static void WriteDeviceLog(string context, string SN)
        {
            if ((!string.IsNullOrEmpty(SN)) && (SN.Length > 3))
            {
                string path = LogPath() + DateTime.Now.ToString("yyyyMMdd") + @"\Devices\" + SN + @"\DeviceRawLog." + SN + ".txt";
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                TextWriter writer = null;
                try
                {
                    writer = TextWriter.Synchronized(File.AppendText(path));
                    writer.Write(string.Format("\r\n{0} : {1}\r\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), (object)context));
                }
                catch
                {
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                }
            }
        }

        public static void WriteFile(string context, string SN)
        {
            if ((!string.IsNullOrEmpty(SN)) && (SN.Length > 3))
            {
                string path = LogPath() + DateTime.Now.ToString("yyyyMMdd") + @"\" + SN + ".txt";
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                TextWriter writer = null;
                try
                {
                    writer = TextWriter.Synchronized(File.AppendText(path));
                    writer.Write(string.Format("{0}\n", (object)context));
                }
                catch
                {
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                }
            }
        }

        public static void WriteFeatureLog(string context, string feature)
        {
            string path = LogPath() + DateTime.Now.ToString("yyyyMMdd") + "\\feature_" + feature + ".txt";
            TextWriter textWriter = (TextWriter)null;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                textWriter = TextWriter.Synchronized((TextWriter)File.AppendText(path));
                textWriter.Write(string.Format("\r\n{0}: {1}\r\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), (object)context));
            }
            catch { }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        public static void WriteStatusLog(string context)
        {
            try
            {
                string path = LogPath() + DateTime.Now.ToString("yyyyMMdd") + "\\StatusLog.txt";
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                TextWriter textWriter = (TextWriter)null;
                try
                {
                    textWriter = TextWriter.Synchronized((TextWriter)File.AppendText(path));
                    textWriter.Write(context + Environment.NewLine + Environment.NewLine);
                }
                catch { }
                finally
                {
                    if (textWriter != null)
                        textWriter.Close();
                }
            }
            catch (Exception)
            { }

        }

        public static string LogPath()
        {
            //return string.IsNullOrEmpty(LogPathStr) ? Application.StartupPath : LogPathStr;
            return "E:/demo";
        }
    }
}
