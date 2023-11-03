using System.IO;
using System.Net;

namespace HcBimUtils.JsonData.License
{
    public class Setting
    {
        public int RevitToolVersion { get; set; }
        public int InstallVersion { get; set; }
        public string Revit2017 { get; set; }
        public string Revit2018 { get; set; }
        public string Revit2019 { get; set; }
        public string Revit2020 { get; set; }
        public string WhatsNew { get; set; }
        public string Installer { get; set; }

        public Setting()
        {
        }

        public static Setting GetSetting(string company = "")
        {
            var deserializedSetting = new Setting();
            var settingDownLoadLink =
                "";

            if (company == "DZC")
            {
                settingDownLoadLink =
                   "";
            }
            if (company == "DS")
            {
                settingDownLoadLink =
                    "";
            }
            try
            {
                string text = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".json");
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(settingDownLoadLink, text);
                }
                string text2 = File.ReadAllText(text);
                if (!string.IsNullOrWhiteSpace(text2))
                {
                    deserializedSetting = JsonUtils.GetSettingFromFile<Setting>(text);
                }
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
            }
            catch
            {
                //
            }
            return deserializedSetting;
        }

        public bool IsValid()
        {
            return RevitToolVersion > 0
                   && InstallVersion > 0
                   && string.IsNullOrEmpty(Revit2017) == false
                   && string.IsNullOrEmpty(Revit2018) == false
                   && string.IsNullOrEmpty(Revit2019) == false
                   && string.IsNullOrEmpty(Revit2020) == false
                   && string.IsNullOrEmpty(Installer) == false;
        }
    }
}