using Base.Configurations;
using Hake.Extension.ValueRecord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileDesktop.Helpers
{
    public static class ConfigurationHelper
    {
        public static SetRecord Combine(this SetRecord dest, SetRecord source)
        {
            if (dest == null)
                throw new ArgumentNullException(nameof(dest));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            RecordBase temp;
            foreach (var pair in source)
            {
                if (dest.TryGetValue(pair.Key, out temp))
                {
                    if (pair.Value is SetRecord srcset && temp is SetRecord dstset)
                        Combine(dstset, srcset);
                    else
                        dest[pair.Key] = pair.Value;
                }
                else
                    dest[pair.Key] = pair.Value;
            }
            return dest;
        }

        public static IConfiguartion ReadConfiguration()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("FileDesktop.default.json");
            SetRecord record = Hake.Extension.ValueRecord.Json.Converter.ReadJson(stream) as SetRecord;
            stream.Dispose();

            string configurationFilePath = App.ConfigurationFilePath;
            if (File.Exists(configurationFilePath))
            {
                try
                {
                    Stream fileStream = File.OpenRead(configurationFilePath);
                    SetRecord userConfig = Hake.Extension.ValueRecord.Json.Converter.ReadJson(fileStream) as SetRecord;
                    fileStream.Dispose();
                    if (userConfig != null && record != null)
                        record.Combine(userConfig);
                    else if (userConfig != null && record == null)
                        record = userConfig;
                }
                catch
                {

                }
            }
            else
            {
                Stream defaultStream = assembly.GetManifestResourceStream("FileDesktop.default.json");
                try
                {
                    FileStream userStream = File.Create(configurationFilePath);
                    defaultStream.CopyTo(userStream);
                    userStream.Flush();
                    userStream.Dispose();

                }
                catch
                {
                }
                defaultStream.Dispose();
            }
            if (record == null)
                throw new Exception("failed to load configurations");
            else
                return ConfigurationReader.Read(record);
        }
    }
}
