#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
#endregion

namespace _3DText_From_Alignment
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonControl Ribbon = ComponentManager.Ribbon;
            Autodesk.Windows.RibbonTab AfryTab = null;
            Autodesk.Windows.RibbonPanel Alignmentpanel = null;

            foreach (var Tab in Ribbon.Tabs)
            {
                if ("AFRY" == Tab.Title)
                {
                    AfryTab = Tab;
                    foreach (var item in Tab.Panels)
                    {
                        if (item.Source.Title == "3D Text Alignment")
                        {
                            Alignmentpanel = item;
                        }
                    }
                }
            }
            if (Alignmentpanel == null)
            {
                if (AfryTab == null)
                {
                    try
                    {
                        a.CreateRibbonTab("AFRY");

                    }
                    catch (Exception)
                    {

                    }
                    try
                    {
                        a.CreateRibbonPanel("AFRY", "3D Text Alignment");
                    }
                    catch (Exception)
                    {

                    }

                }
                else
                {
                    a.CreateRibbonPanel("AFRY", "3D Text Alignment");

                }

            }


            var Locationath = Assembly.GetExecutingAssembly().Location;
            PushButtonData Create3d = new PushButtonData("Create", "Create 3D Text alignment", Locationath, "_3DText_From_Alignment.Command");
            Create3d.ToolTip = "This Command creates 3D text alignments when u select a Model line"; // Can be changed to a more descriptive text.
            Create3d.Image = new BitmapImage(new Uri(Path.GetDirectoryName(Locationath) + "\\Export.png"));
            Create3d.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(Locationath) + "\\Export.png"));

            a.GetRibbonPanels("AFRY").Find(E => E.Name == "3D Text Alignment").AddItem(Create3d);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
