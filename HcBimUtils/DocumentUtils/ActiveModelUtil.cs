using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HcBimUtils.GeometryUtils;

namespace HcBimUtils.DocumentUtils
{
    public class AC
    {
        public static UIDocument UiDoc;
        public static Document Document;
        public static Autodesk.Revit.ApplicationServices.Application Application;
        public static UIApplication UiApplication;
        public static Autodesk.Revit.UI.Selection.Selection Selection;
        public static View ActiveView;
        public static string Username;
        public static BPlane ViewPlane;
        public static string HCSettingPath;
        public static string HCInstallPath;
        public static string CurrentCommand;
        public static Dictionary<string, string> DicCommandYoutubeLink;
        public static string Version;
        private static ExternalEvent _externalEvent;
        private static ExternalEventHandler _externalEventHandler;
        private static ExternalEventHandlers _externalEventHandlers;
        public static string ErrorLog = "";
        public static LanguageType LanguageType = LanguageType.English_USA;

        public static ExternalEvent ExternalEvent
        {
            get => _externalEvent ??= ExternalEvent.Create(ExternalEventHandler);
            set => _externalEvent = value;
        }

        public static ExternalEventHandler ExternalEventHandler
        {
            get => _externalEventHandler ??= new ExternalEventHandler();
            set => _externalEventHandler = value;
        }

        public static ExternalEventHandlers ExternalEventHandlers
        {
            get => _externalEventHandlers ??= new ExternalEventHandlers();
            set => _externalEventHandlers = value;
        }

        public static void GetInformation(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
            Document = uiDoc.Document;
            Application = uiDoc.Application.Application;
            UiApplication = uiDoc.Application;
            Selection = uiDoc.Selection;
            Username = Application.Username;
            ActiveView = Document.ActiveView;
            try
            {
                ViewPlane = BPlane.CreateByNormalAndOrigin(ActiveView.ViewDirection, ActiveView.Origin);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
            ErrorLog = string.Empty;
            SetPath();
        }

        public static void GetInformation(ExternalCommandData data, string currentCommand)
        {
            CurrentCommand = currentCommand;
            var uiDoc = data.Application.ActiveUIDocument;
            UiDoc = uiDoc;
            Document = uiDoc.Document;
            Application = uiDoc.Application.Application;
            UiApplication = uiDoc.Application;
            Selection = uiDoc.Selection;
            Username = Application.Username;
            ActiveView = Document.ActiveView;
            ViewPlane = BPlane.CreateByNormalAndOrigin(ActiveView.ViewDirection, ActiveView.Origin);
            ErrorLog = string.Empty;
            SetPath();
        }

        private static void SetPath()
        {
            //Setting Path
            Version = UiApplication.Application.VersionNumber;
            HCSettingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\HCSetting";
            HCInstallPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\HC.bundle\Contents";
            HCInstallPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\HC.bundle\Contents";
        }

        public static string Log(string log, object obj = null)
        {
            ErrorLog += Environment.NewLine + "- " + log;
            if (obj != null)
            {
                ErrorLog += Environment.NewLine + obj.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
            }
            return ErrorLog;
        }

        public static string Log(string log, Exception e, object obj = null)
        {
            ErrorLog += Environment.NewLine + "- " + log + Environment.NewLine + e.Message;
            if (obj != null)
            {
                ErrorLog += Environment.NewLine + obj.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
            }
            return ErrorLog;
        }
    }

}