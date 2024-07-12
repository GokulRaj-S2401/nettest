using Microsoft.Extensions.Configuration;
using static ZOI.BAL.Utilities.Constants;

namespace ZOI.BAL.Utilities
{
    public static class CommonFunction
    {
        public static string GetConnectionString(string key)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);
            var root = configurationBuilder.Build();
            return root.GetConnectionString(key);
        }

        public static string BsonSerializer(dynamic data)
        {
            var strwrtr = new System.IO.StringWriter();
            var writer = new MongoDB.Bson.IO.JsonWriter(strwrtr, new MongoDB.Bson.IO.JsonWriterSettings());
            MongoDB.Bson.Serialization.BsonSerializer.Serialize(writer, data);
            return strwrtr.ToString();
        }

        public static string AppendTimeStamp(string fileName)
        {
            return string.Concat(
            Path.GetFileNameWithoutExtension(fileName),
            DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            Path.GetExtension(fileName)
            );
        }

        public static void GetAppString(string key)
        {
            //var configurationBuilder = new ConfigurationBuilder();
            //var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            //configurationBuilder.AddJsonFile(path, false);

            //var root = configurationBuilder.Build();
            //return root.GetSection("appSettings").GetValue<string>(key);
        }

        public static string GetCurrentDateTime()
        {
            return DateTime.Now.ToString(DateTimeFormat.DateTimeWith24HrsFormat);
        }
    }
}
