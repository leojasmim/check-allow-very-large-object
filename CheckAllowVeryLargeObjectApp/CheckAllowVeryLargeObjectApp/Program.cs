using System;
using System.Configuration;
using System.IO;
using System.Runtime.Caching;

namespace CheckAllowVeryLargeObjectApp
{
    public class Fool
    {
        const int b = 1;
        const int k = 1024 * b;
        const int m = 1024 * k;
        const int g = 1024 * m;
        public string key { get; set; }
        //Objeto com aprox 100 MB
        byte[] array = new byte[100 * m];
    }
    class Program
    {
        public const int b = 1;
        public const int k = 1024;
        public const int m = 1024 * k;
        public const int g = 1024 * m;

        public const string logFilename = "../../../log.csv";

        static void Main(string[] args)
        {
            if (File.Exists(logFilename))
                File.Delete(logFilename);
            LogInitialize();
            WithArray();
            WithMemoryCache();
        }

        private static void WithArray()
        {
            //Memória cumulativa já alocada 
            LogArrayAction("WithArray...");
            try
            {
                //Array com 1 bilhão de elemento de 1 byte = 1GB
                var a_byte = new byte[g];
                LogArrayAction("WithArray 1GB");

                //Array com 1 bilhão de elemento de 2 bytes = 2GB
                var a_2bytes = new Int16[g];
                LogArrayAction("WithArray 2GB");

                //Array com 1 bilhão de elemento de 8 bytes = 4GB
                var a_4bytes = new Int32[g];
                LogArrayAction("WithArray 4B");

                //Array com 1 bilhão de elemento de 16 bytes = 8GB
                var a_8bytes = new Int64[g];
                LogArrayAction("WithArray 8GB");
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        private static void WithMemoryCache()
        {
            ObjectCache cache = new MemoryCache("cache");

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            policy.Priority = CacheItemPriority.NotRemovable;

            LogObjectCacheAction("WithMemoryCache...", cache);

            try
            {
                for (int i = 1; i < k; i++)
                {
                    //Objeto com aprox 100 MB
                    var a = new Fool() { key = i.ToString() };
                    cache.Set(a.key, a, policy);

                    LogObjectCacheAction(String.Format("WithMemoryCache - Iteração:{0}", i), cache);
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }

        }

        private static void LogArrayAction(string message)
        {
            string plataform = Environment.Is64BitProcess ? "x64" : "x86";
            string gcAllowVeryLargeObject = ConfigurationManager.AppSettings["gcAllowVeryLargeObjectsEnabled"];
            var memory = (decimal)GC.GetTotalMemory(false) / (decimal)m;

            using (StreamWriter file = new StreamWriter(logFilename, true))
            {
                var line = String.Join(";",
                    DateTime.Now.ToString(),
                    plataform,
                    gcAllowVeryLargeObject,
                    message,
                    memory.ToString("F3"),
                    "");

                file.WriteLine(line);
            }
        }
        private static void LogObjectCacheAction(string message, ObjectCache cache)
        {
            string plataform = Environment.Is64BitProcess ? "x64" : "x86";
            string gcAllowVeryLargeObject = ConfigurationManager.AppSettings["gcAllowVeryLargeObjectsEnabled"];
            var memory = (decimal)GC.GetTotalMemory(false) / (decimal)m;
            var cacheLimit = (cache as MemoryCache).CacheMemoryLimit / (decimal)m;

            using (StreamWriter file = new StreamWriter(logFilename, true))
            {
                var line = String.Join(";",
                   DateTime.Now.ToString(),
                   plataform,
                   gcAllowVeryLargeObject,
                   message,
                   memory.ToString("F3"),
                   cacheLimit.ToString("F3"));

                file.WriteLine(line);
            }
        }
        private static void LogException(Exception e)
        {
            string plataform = Environment.Is64BitProcess ? "x64" : "x86";
            string gcAllowVeryLargeObject = ConfigurationManager.AppSettings["gcAllowVeryLargeObjectsEnabled"];
            var memory = (decimal)GC.GetTotalMemory(false) / (decimal)m;

            using (StreamWriter file = new StreamWriter(logFilename, true))
            {
                var line = String.Join(";",
                   DateTime.Now.ToString(),
                   plataform,
                   gcAllowVeryLargeObject,
                   e.GetType().ToString(),
                   memory.ToString("F3"), "");

                file.WriteLine(line);
            }
        }
        private static void LogInitialize()
        {
            using (StreamWriter file = new StreamWriter(logFilename, true))
            {
                var line = String.Join(";",
                   "Time",
                   "Plataform",
                   "GCAllowVeryLargeObject",
                   "Message",
                   "AllocatedMemory (MB)",
                   "CacheMemoryLimit (MB)");

                file.WriteLine(line);
            }
        }
    }
}
