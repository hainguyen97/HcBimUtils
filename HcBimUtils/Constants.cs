using System.Globalization;
using System.IO;
using System.Reflection;
using HcBimUtils.Models;

namespace HcBimUtils
{
    public class Constants
    {
        public static string SettingFolder
        {
            get
            {
                string settingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Constants.AppName + "Setting";
                if (Directory.Exists(settingFolder) == false)
                {
                    Directory.CreateDirectory(settingFolder);
                }
                return settingFolder;
            }
        }

        public static string VersionFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ContentsFolder => Directory.GetParent(Constants.VersionFolder).FullName;
        public static string ResourcesFolder => Path.Combine(Constants.ContentsFolder, "Resources");
        public static string AppName = "HC";
        public const string AppNameMEP = "HC";
        public const string FamilyFittingName = "00.REC_Offset";

        private static string LocalAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public const double Eps = 1.0e-9;
        public static double VolumeTolerance { get; set; }
        public static double AreaTolerance { get; set; }
        public static double AngleTolerance { get; set; }
        public static double ShortCurveTolerance { get; set; }

#if RELEASE
      public static int CurrentVersion = Convert.ToInt32(DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
#else
        public const int CurrentVersion = 20210808;
#endif
        public static bool IsEnglishLanguage = false;
        public static bool IsStudentPackage = false;

        public static string RevitVersion
        {
            get
            {
#if Version2018
            return "2018";
#elif Version2019
                return "2019";
#elif Version2020
            return "2020";
#elif Version2021
            return "2021";
#elif Version2022
            return "2022";
#elif Version2023
            return "2023";
#else
            return "";
#endif
            }
        }


        public static List<DSConsFitting> GetSplipMEPFittings()
        {
            List<DSConsFitting> fittingInfos = new List<DSConsFitting>
         {
            new DSConsFitting
            {
               FamilyName="04.ROD_Tap (REC -ROD)",
               ParaAndValues = new List<DSConsParaAndValue>
               {
                  new DSConsParaAndValue
                  {
                     ParaName = "Chiều dài côn vuông tròn"
                  },
                  new DSConsParaAndValue
                  {
                     ParaName ="Chiều rộng TAP"
                  }
               }
            },
            new DSConsFitting
            {
               FamilyName="02.REC Tap -REC(TDC Flanged)",
               ParaAndValues = new List<DSConsParaAndValue>
               {
                  new DSConsParaAndValue
                  {
                     ParaName = "Takeoff Fixed Length"
                  }
               }
            }
         };
            return fittingInfos;
        }

        #region Message

        public const string NO_DUCT_SELECTION = "There is no Duct in current selections.";
        public const string PICK_DUCT_SPLIT = "Pick a Duct to split";
        public const string PICK_FIRST_ELEMENT = "Pick first Element";
        public const string PICK_FIRST_ELEMENT_ERROR = "User has to pick first Element.";
        public const string PICK_START_POINT = "Pick start point";
        public const string PICK_FAMILYINSTANCE_COPY = "Pick FamilyInstance to copy";
        public const string PICK_DESTINATION_OBJECT = "Pick destination Object";
        public const string PICK_MEPCURVE_MOVE = "Pick MEPCurve will be moved";

        #endregion Message
    }
}