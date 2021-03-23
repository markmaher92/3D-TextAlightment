using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace _3DText_From_Alignment
{
    class WindowDialogs
    {
        public static void OpenDialogRev(TextBox FamilyPath)
        {
            OpenFileDialog F = new OpenFileDialog();
            F.Filter = "Revit files (*.rfa)|*.rfa";
            F.Title = "Select RevitFamily To Place";
            if (F.ShowDialog() == true)
            {
                FamilyPath.Text = F.FileName;
            }
        }
        public static void LandXmlOpenDialog(TextBox LandXmlPath)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "LandXML files (*.xml)|*.xml";
            dlg.Title = "Import LandXML and " + "Create 3D Alignments";

            if (dlg.ShowDialog() == true)
            {
                LandXmlPath.Text = dlg.FileName;
            }
        }
    }
}
