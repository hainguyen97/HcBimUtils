using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using HcBimUtils.JsonData.Ribbons;

namespace HcBimUtils.RibbonUtils
{
    public static class RibbonUtils
    {
        public static PushButtonData ToPushButtonData(this CustomButtonData customButtonData, bool isSetLargeImage = true, bool isSetImages = true)
        {
            var data = new PushButtonData(customButtonData.Name, customButtonData.Text, Path.Combine(Constants.VersionFolder, customButtonData.AssemblyName), customButtonData.ClassName)
            {
                ToolTip = customButtonData.Tooltip,
            };
            if (isSetImages)
            {
                var icon32FilePath = Path.Combine(Constants.ResourcesFolder, customButtonData.Icon);
                var icon16FilePath = Path.Combine(Constants.ResourcesFolder, customButtonData.Icon.Replace(".png", "16.png"));
                if (isSetLargeImage)
                {
                    if (File.Exists(icon32FilePath))
                    {
                        data.LargeImage = new BitmapImage(new Uri(icon32FilePath));
                    }
                }
                else
                {
                    if (File.Exists(icon16FilePath))
                    {
                        data.LargeImage = new BitmapImage(new Uri(icon16FilePath));
                    }
                }
                if (File.Exists(icon16FilePath))
                {
                    data.Image = new BitmapImage(new Uri(icon16FilePath));
                }
            }

            if (!string.IsNullOrEmpty(customButtonData.HelpPath))
            {
                var help = new ContextualHelp(ContextualHelpType.Url, customButtonData.HelpPath);
                data.SetContextualHelp(help);
            }
            return data;
        }

        public static SplitButtonData ToSplitButtonData(this CustomButtonData customButtonData)
        {
            var sbd = new SplitButtonData(customButtonData.Name, customButtonData.Text);
            return sbd;
        }

        public static PulldownButtonData ToPullDownButtonData(this CustomButtonData customButtonData)
        {
            var icon16FilePath = Path.Combine(Constants.ResourcesFolder, customButtonData.Icon.Replace(".png", "16.png"));
            var icon32FilePath = Path.Combine(Constants.ResourcesFolder, customButtonData.Icon);
            var data = new PulldownButtonData(customButtonData.Name, customButtonData.Text)
            {
                ToolTip = customButtonData.Tooltip,
            };
            if (File.Exists(icon32FilePath))
            {
                data.LargeImage = new BitmapImage(new Uri(icon32FilePath));
            }
            if (File.Exists(icon16FilePath))
            {
                data.Image = new BitmapImage(new Uri(icon16FilePath));
            }
            if (!string.IsNullOrEmpty(customButtonData.HelpPath))
            {
                var help = new ContextualHelp(ContextualHelpType.Url, customButtonData.HelpPath);
                data.SetContextualHelp(help);
            }
            return data;
        }

        public static void Create(this CustomButton customButton, RibbonPanel panel)
        {
            switch (customButton.ButtonType)
            {
                case ButtonType.PushButton:
                    CreatePushButton(panel, customButton);
                    break;

                case ButtonType.Split:
                    CreateSplitButton(panel, customButton);
                    break;

                case ButtonType.DropDown:
                    CreateDropdownButton(panel, customButton);
                    break;

                case ButtonType.TwoStackedItems:
                    CreateTwoStackedItems(panel, customButton);
                    break;

                case ButtonType.ThreeStackedItems:
                    CreateThreeStackedItems(panel, customButton);
                    break;

                case ButtonType.TwoStackedSplitItems:
                    CreateTwoStackedSplitItems(panel, customButton);
                    break;

                case ButtonType.ThreeStackedSplitItems:
                    CreateThreeStackedSplitItems(panel, customButton);
                    break;
            }
        }

        private static void CreatePushButton(RibbonPanel panel, CustomButton customButton)
        {
            foreach (var pushButtonDataCustom in customButton.PushButtonDataCustoms)
            {
                var pushButtonData = pushButtonDataCustom.ToPushButtonData();
                panel.AddItem(pushButtonData);
            }
        }

        private static void CreateSplitButton(RibbonPanel panel, CustomButton customButton)
        {
            var sbd = customButton.ButtonData.ToSplitButtonData();
            var sb = panel.AddItem(sbd) as SplitButton;
            foreach (var pushButtonDataCustom in customButton.PushButtonDataCustoms)
            {
                var pushButtonData = pushButtonDataCustom.ToPushButtonData();
                sb?.AddPushButton(pushButtonData);
            }
        }

        private static void CreateDropdownButton(RibbonPanel panel, CustomButton customButton)
        {
            var pbd = customButton.ButtonData.ToPullDownButtonData();
            var pb = panel.AddItem(pbd) as PulldownButton;
            foreach (var pushButtonDataCustom in customButton.PushButtonDataCustoms)
            {
                var pushButtonData = pushButtonDataCustom.ToPushButtonData();
                pb?.AddPushButton(pushButtonData);
            }
        }

        private static void CreateThreeStackedSplitItems(RibbonPanel panel, CustomButton customButton)
        {
            if (customButton.PushButtonDataCustoms.Count == 3)
            {
                var pbd1 = customButton.PushButtonDataCustoms[0].ToPushButtonData();
                var pbd2 = customButton.PushButtonDataCustoms[1].ToPushButtonData();
                var pbd3 = customButton.PushButtonDataCustoms[2].ToPushButtonData();
                panel.AddStackedItems(pbd1, pbd2, pbd3);
            }
            else
            {
                SplitButtonData pbd1;
                SplitButtonData pbd2;
                SplitButtonData pbd3;
                pbd1 = customButton.ButtonData1.ToSplitButtonData();
                pbd2 = customButton.ButtonData2.ToSplitButtonData();
                pbd3 = customButton.ButtonData3.ToSplitButtonData();
                var ribbonItems = panel.AddStackedItems(pbd1, pbd2, pbd3);
                {
                    var pb = ribbonItems[0] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List1)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
                {
                    var pb = ribbonItems[1] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List2)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
                {
                    var pb = ribbonItems[2] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List3)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
            }
        }

        private static void CreateThreeStackedItems(RibbonPanel panel, CustomButton customButton)
        {
            if (customButton.PushButtonDataCustoms.Count == 3)
            {
                var pbd1 = customButton.PushButtonDataCustoms[0].ToPushButtonData();
                var pbd2 = customButton.PushButtonDataCustoms[1].ToPushButtonData();
                var pbd3 = customButton.PushButtonDataCustoms[2].ToPushButtonData();
                panel.AddStackedItems(pbd1, pbd2, pbd3);
            }
            else
            {
                PulldownButtonData pbd1;
                PulldownButtonData pbd2;
                PulldownButtonData pbd3;
                pbd1 = customButton.ButtonData1.ToPullDownButtonData();
                pbd2 = customButton.ButtonData2.ToPullDownButtonData();
                pbd3 = customButton.ButtonData3.ToPullDownButtonData();
                var ribbonItems = panel.AddStackedItems(pbd1, pbd2, pbd3);
                {
                    var pb = ribbonItems[0] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List1)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
                {
                    var pb = ribbonItems[1] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List2)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
                {
                    var pb = ribbonItems[2] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List3)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
            }
        }

        private static void CreateTwoStackedItems(RibbonPanel panel, CustomButton customButton)
        {
            if (customButton.PushButtonDataCustoms.Count == 2)
            {
                var pbd1 = customButton.PushButtonDataCustoms[0].ToPushButtonData();
                var pbd2 = customButton.PushButtonDataCustoms[1].ToPushButtonData();
                panel.AddStackedItems(pbd1, pbd2);
            }
            else
            {
                PulldownButtonData pbd1;
                PulldownButtonData pbd2;
                pbd1 = customButton.ButtonData1.ToPullDownButtonData();
                pbd2 = customButton.ButtonData2.ToPullDownButtonData();
                var ribbonItems = panel.AddStackedItems(pbd1, pbd2);
                {
                    var pb = ribbonItems[0] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List1)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
                {
                    var pb = ribbonItems[1] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List2)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
            }
        }

        private static void CreateTwoStackedSplitItems(RibbonPanel panel, CustomButton customButton)
        {
            if (customButton.PushButtonDataCustoms.Count == 2)
            {
                var pbd1 = customButton.PushButtonDataCustoms[0].ToPushButtonData();
                var pbd2 = customButton.PushButtonDataCustoms[1].ToPushButtonData();
                panel.AddStackedItems(pbd1, pbd2);
            }
            else
            {
                SplitButtonData pbd1;
                SplitButtonData pbd2;
                pbd1 = customButton.ButtonData1.ToSplitButtonData();
                pbd2 = customButton.ButtonData2.ToSplitButtonData();
                var ribbonItems = panel.AddStackedItems(pbd1, pbd2);
                {
                    var pb = ribbonItems[0] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List1)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
                {
                    var pb = ribbonItems[1] as PulldownButton;
                    foreach (var pushButtonDataCustom in customButton.List2)
                    {
                        if (pushButtonDataCustom.Name == "Separator")
                        {
                            pb?.AddSeparator();
                        }
                        else
                        {
                            var pushButtonData = pushButtonDataCustom.ToPushButtonData(false);
                            pb?.AddPushButton(pushButtonData);
                        }
                    }
                }
            }
        }
    }
}