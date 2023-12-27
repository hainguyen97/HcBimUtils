using Autodesk.Revit.UI;
using HcBimUtils.Commands;
using System.Reflection;

namespace HcBimUtils.RibbonUtils
{
    public static class RibbonExtension
    {
        public static PushButton AddPushButton<TCommand>(this RibbonPanel panel) where TCommand : IExternalCommand
        {
            var type = typeof(TCommand);
            var commandClass = typeof(Command);
            var attrs = Attribute.GetCustomAttributes(commandClass);
            var buttonText = type.FullName;
            var buttonLargeImage = "";
            var buttonImage = "";
            foreach (var attr in attrs)
            {
                if (attr is not CommandAttribute commandAttribute) continue;
                buttonText = commandAttribute.Name ?? type.FullName;
                buttonLargeImage = commandAttribute.LargeImage ?? "";
                buttonImage = commandAttribute.Image ?? "";
            }

            var itemData = new PushButtonData(type.FullName, buttonText, Assembly.GetAssembly(type).Location, type.FullName);
            var pushButton = (PushButton)panel.AddItem(itemData);
            pushButton.SetImage(buttonImage);
            pushButton.SetLargeImage(buttonLargeImage);
            return pushButton;
        }
    }
}
