using Autodesk.Revit.UI;

namespace HcBimUtils.MessageBoxUtils
{
    public static class MessageUtils
    {
        public static void ShowInfoMessageBox(string sAppName, string sTitle, string sMessage)
        {
            Show(sAppName, sTitle, sMessage, null, null, false);
        }

        public static void ShowWarningMessageBox(string sAppName, string sTitle, string sMessage, string sSubMessage)
        {
            Show(sAppName, sTitle, sMessage, sSubMessage, null, true);
        }

        public static bool ShowYesNoQuestion(string sAppName, string sTitle, string sQuestion, string sMainContent)
        {
            string text = sAppName;
            if (!string.IsNullOrEmpty(sTitle))
            {
                text += $" - {sTitle}";
            }

            TaskDialog taskDialog = new TaskDialog(text) { TitleAutoPrefix = false, MainInstruction = sQuestion };
            if (!string.IsNullOrEmpty(sMainContent))
            {
                taskDialog.MainContent = sMainContent;
            }
            taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconNone;
            taskDialog.CommonButtons = (TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
            taskDialog.DefaultButton = TaskDialogResult.Yes;
            return TaskDialogResult.Yes == taskDialog.Show();
        }

        private static TaskDialogResult Show(string sAppName, string sTitle, string sMainInstructions, string sMainContent, string sExpandedContent, bool bWarning)
        {
            string text = sAppName;
            if (!string.IsNullOrEmpty(sTitle))
            {
                text += $" - {sTitle}";
            }

            TaskDialog taskDialog = new TaskDialog(text) { TitleAutoPrefix = false, MainInstruction = sMainInstructions };
            if (!string.IsNullOrEmpty(sMainContent))
            {
                taskDialog.MainContent = sMainContent;
            }
            if (!string.IsNullOrEmpty(sExpandedContent))
            {
                taskDialog.ExpandedContent = sExpandedContent;
            }
            taskDialog.MainIcon = (bWarning ? TaskDialogIcon.TaskDialogIconWarning : TaskDialogIcon.TaskDialogIconNone);
            taskDialog.CommonButtons = TaskDialogCommonButtons.Close;
            taskDialog.DefaultButton = TaskDialogResult.Close;
            return taskDialog.Show();
        }
    }
}