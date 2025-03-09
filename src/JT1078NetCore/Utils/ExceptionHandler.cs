using System.Diagnostics;
using System.Text;

namespace JT1078NetCore.Utils
{
    public class ExceptionHandler
    {
        public static void ExceptionProcess(System.Exception ex)
        {
            Log.WriteExceptionLog(GetExceptionMessage(ex));
        }
        public static string GetExceptionMessage(System.Exception exception)
        {
            DateTime now = DateTime.Now;

            StringBuilder builder = new StringBuilder();
            builder.Append("Date:              " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + Environment.NewLine);
            Process[] processesByName = Process.GetProcessesByName("System");
            if ((Environment.OSVersion.Version.Major < 6) && (processesByName.Length > 0))
            {
                builder.Append("System up time:    " + ((TimeSpan)(DateTime.Now - processesByName[0].StartTime)).ToString() + Environment.NewLine);
            }
            builder.Append("App up time:       " + ((TimeSpan)(DateTime.Now - Process.GetCurrentProcess().StartTime)).ToString() + Environment.NewLine);
            builder.Append("Exception class:   " + exception.GetType().ToString() + Environment.NewLine);
            builder.Append("Exception message: " + GetExceptionStack(exception) + Environment.NewLine);
            //builder.Append(Environment.NewLine);
            builder.Append("Stack Trace:");
            builder.Append(Environment.NewLine);
            builder.Append(exception.StackTrace);
            return builder.ToString();
        }

        public static string GetExceptionStack(System.Exception e)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(e.Message);
            while (e.InnerException != null)
            {
                e = e.InnerException;
                builder.Append(Environment.NewLine);
                builder.Append(e.Message);
            }
            return builder.ToString();
        }
    }
}
