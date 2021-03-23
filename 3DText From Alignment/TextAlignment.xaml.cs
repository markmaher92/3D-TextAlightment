using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using LineX = Autodesk.Revit.DB.Line;

namespace _3DText_From_Alignment
{
    /// <summary>
    /// Interaction logic for TextAlignment.xaml
    /// </summary>
    public partial class TextAlignment : Window
    {
        UIDocument uiDoc;
        public TextAlignment(UIDocument UIDOC)
        {
            InitializeComponent();
            uiDoc = UIDOC;
        }

        private void Run_click(object sender, RoutedEventArgs e)
        {
            var LandXmlObjects = XmlParserLandXml.ParseLandXml(LandXmlPath.Text);

            RevitHelper.PlaceRevitFamilies(LandXmlObjects,uiDoc ,FamilyPath.Text);

            this.Close();
        }

        private static TextLineObject CreateMidPoint(TextLineObject Object, List<XYZ> HeightPoints, LineX L, double StationIncrement, int J)
        {
            var MidPointTE = new TextLineObject(Object);
            var StationText = (MidPointTE.StationStart + (StationIncrement * J));
            var StationXRatio = ((StationIncrement * J)) / (Object.StationEnd - Object.StationStart);
            MidPointTE.PointInsert = L.Evaluate(StationXRatio, true);
            MidPointTE.StationText = StationText;
            ExtractHeightForTextObkect(MidPointTE, HeightPoints, StationText, MidPointTE.StationEnd);
            return MidPointTE;
        }

        private static TextLineObject CreateMidPoint1(TextLineObject Object, List<XYZ> HeightPoints, LineX L, double i)
        {
            var MidPointTE = new TextLineObject(Object);
            var StationText = i;
            var StationXRatio = (i - Object.StationStart) / (Object.StationEnd - Object.StationStart);
            MidPointTE.PointInsert = L.Evaluate(StationXRatio, true);
            MidPointTE.StationText = StationText;
            ExtractHeightForTextObkect(MidPointTE, HeightPoints, StationText, MidPointTE.StationEnd);
            return MidPointTE;
        }

        private double ExtractStationDisntace()
        {
            if (string.IsNullOrEmpty(this.StationDistanceTxt.Text))
            {
                this.StationDistanceTxt.Text = 0.ToString();
            }
            var StationIncrement = double.Parse(this.StationDistanceTxt.Text);

            return StationIncrement;
        }

        private void RevitBrowserClick(object sender, RoutedEventArgs e)
        {
            WindowDialogs.OpenDialogRev(FamilyPath);
        }
        private void LandXmlPathBut(object sender, RoutedEventArgs e)
        {
            WindowDialogs.LandXmlOpenDialog(LandXmlPath);
        }
    }
}
