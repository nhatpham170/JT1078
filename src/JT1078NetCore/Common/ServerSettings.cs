namespace JT1078NetCore.Common
{
    public static class ServerSettings
    {
        public static bool IsSsl
        {
            get
            {
                string ssl = CommonHelper.Configuration["ssl"];
                return !string.IsNullOrEmpty(ssl) && bool.Parse(ssl);
            }
        }

        public static int Port => int.Parse(CommonHelper.Configuration["port"]);

        public static bool UseLibuv
        {
            get
            {
                string libuv = CommonHelper.Configuration["libuv"];
                return !string.IsNullOrEmpty(libuv) && bool.Parse(libuv);
            }
        }
    }
}
