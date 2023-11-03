using System.IO;
using Newtonsoft.Json;

namespace HcBimUtils.JsonData
{
    public static class JsonUtils
    {
        public static T GetSettingFromFile<T>(string filePath)
        {
            T obj = default;
            if (!File.Exists(filePath)) return obj;
            try
            {
                using var streamReader = File.OpenText(filePath);
                obj = (T)new JsonSerializer().Deserialize(streamReader, typeof(T));
            }
            catch
            {
                // ignored
            }
            return obj;
        }

        public static T GetSettingFromString<T>(string s)
        {
            T obj = default;

            try
            {
                obj = JsonConvert.DeserializeObject<T>(s);
            }
            catch
            {
                // ignored
            }

            return obj;
        }

        public static T GetSettingFromFile2<T>(string filePath)
        {
            T obj = default;
            if (!File.Exists(filePath)) return obj;
            try
            {
                var text = File.ReadAllText(filePath);
                obj = JsonConvert.DeserializeObject<T>(text);
            }
            catch
            {
                // ignored
            }
            return obj;
        }

        public static void SaveSettingToFile<T>(T setting, string filePath)
        {
            var contents = JsonConvert.SerializeObject(setting, Formatting.Indented);
            var path = Path.GetDirectoryName(filePath);
            if (path != null)
            {
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
            }
            File.WriteAllText(filePath, contents);
        }
        public static string SaveSettingToString<T>(T setting)
        {
            var contents = JsonConvert.SerializeObject(setting);
            return contents;
        }
        public static string SaveSettingToString2<T>(T setting)
        {
            var contents = JsonConvert.SerializeObject(setting, Formatting.Indented);
            return contents;
        }

        public static T GetSettingDecrypt<T>(string filePath)
        {
            T obj = default;
            if (!File.Exists(filePath)) return obj;
            try
            {
                var s = File.ReadAllText(filePath);
                var ss = EncryptDecrypt.Decrypt(s);
                obj = (T)JsonConvert.DeserializeObject(ss);
            }
            catch
            {
                // ignored
            }
            return obj;
        }

        public static void SaveSettingEncrypt<T>(T setting, string filePath)
        {
            var contents = JsonConvert.SerializeObject(setting, Formatting.Indented);
            contents = EncryptDecrypt.Encrypt(contents);
            File.WriteAllText(filePath, contents);
        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}