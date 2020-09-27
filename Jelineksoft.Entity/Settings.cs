using System;
using System.Collections.Generic;
using System.Data;

namespace Jelineksoft.Entity
{
    public static class Settings
    {

        public static string ConnectionString { get; set; }
        public static string DefaultEngine { get; set; } = "innodb";
        public static ProviderBase DefaultProvider { get; set; }
        public static bool SetDBNullToDefaultClassValue { get; set; } = false;

        public static Log Log { get; set; } = new Log();

    }

}