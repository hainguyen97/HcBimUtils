using System.Windows;
using Autodesk.Revit.UI;
using HcBimUtils.JsonData.Ribbons;

namespace HcBimUtils.RibbonUtils
{
    public class RibbonCreator
    {
        public static void CreateRibonTab(UIControlledApplication a, CustomRibbon newRibbon)
        {
            string name = newRibbon.Name;
            try
            {
                a.CreateRibbonTab(name);
            }
            catch
            {
                //
            }
            foreach (CustomRibbonPanel panel in newRibbon.Panels)
            {
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(name, panel.Name);
                foreach (var customButton in panel.Buttons)
                {
                    try
                    {
                        customButton.Create(ribbonPanel);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        public static void CreateRibonTab(UIApplication a, CustomRibbon newRibbon)
        {
            string name = newRibbon.Name;
            a.CreateRibbonTab(name);
            foreach (CustomRibbonPanel panel in newRibbon.Panels)
            {
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(name, panel.Name);
                foreach (var customButton in panel.Buttons)
                {
                    try
                    {
                        customButton.Create(ribbonPanel);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}