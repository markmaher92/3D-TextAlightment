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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            string FamilyName = "3DAlignment_Tool";

            using (Transaction T = new Transaction(uiDoc.Document, "Create labes"))
            {
                T.Start();

                try
                {
                    uiDoc.Document.LoadFamily(FamilyPath.Text);
                    FamilyName = System.IO.Path.GetFileNameWithoutExtension(FamilyPath.Text);

                }
                catch (Exception)
                {

                }
                try
                {
                    IList<Reference> Lines = uiDoc.Selection.PickObjects(ObjectType.Element, "Select Line");

                    var StationStart = double.Parse(this.StartingStationTxt.Text);
                    StationStart = AcheStationingAndFamilyInsert(Lines, StationStart, FamilyName);

                }
                catch (Exception)
                {

                }

                T.Commit();
            }
        }

        private double AcheStationingAndFamilyInsert(IList<Reference> Lines, double StationStart, string FamilyName)
        {
            foreach (var item in Lines)
            {
                var Element = uiDoc.Document.GetElement(item);
                if (Element is ModelLine)
                {
                    var Length = (Element as ModelLine).GeometryCurve.Length;

                    var MetricLength = UnitUtils.ConvertFromInternalUnits(Length, DisplayUnitType.DUT_METERS);

                    var StationDsitance = double.Parse(this.StatoinDistanceTxt.Text);


                    for (double i = 0; i < (MetricLength + StationDsitance); i += StationDsitance)
                    {
                        StationStart = InsertFamilyAtStation(StationStart, Element, MetricLength, i, FamilyName);

                    }
                }
            }

            return StationStart;
        }

        private double InsertFamilyAtStation(double StationStart, Element Element, double MetricLength, double i, string FamilyName)
        {
            if (i < MetricLength)
            {
                var NewI = UnitUtils.ConvertToInternalUnits(i, DisplayUnitType.DUT_METERS);

                var Point = (Element as ModelLine).GeometryCurve.Evaluate(NewI, false);
                var Point2 = (Element as ModelLine).GeometryCurve.GetEndPoint(1);

                XYZ NormalVector = (Point2 - Point).Normalize();
                double Angle = (Math.PI / 2) - NormalVector.AngleTo(XYZ.BasisX);
                FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);


                FillParameters(i, Point, Fam, Angle, StationStart + i);
            }
            else
            {
                StationStart = Math.Round(StationStart + MetricLength, 3);

            }

            return StationStart;
        }

        private void FillParameters(double i, XYZ Point, FamilySymbol Fam, double angle, double stationStart)
        {
            Fam.Activate();
            FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Point, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            try
            {
                if (!string.IsNullOrWhiteSpace(this.ElevationTxt.Text))
                {
                    var elevation = UnitUtils.ConvertToInternalUnits(double.Parse(this.ElevationTxt.Text), DisplayUnitType.DUT_METERS);
                    FamIns.LookupParameter("Elevation").Set(elevation);
                }

            }
            catch (Exception)
            {
            }
            try
            {
                //var Station = double.Parse(this.StartingStationTxt.Text) + i;
                FamIns.LookupParameter("Text").Set(stationStart.ToString());
            }
            catch (Exception)
            {
            }
            try
            {
                var HorizontalDistance = UnitUtils.ConvertToInternalUnits(double.Parse(this.HorizontalDistancetext.Text), DisplayUnitType.DUT_METERS);
                FamIns.LookupParameter("Horizontal Distance").Set(HorizontalDistance);
            }
            catch (Exception)
            {
            }

            ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id, Autodesk.Revit.DB.Line.CreateBound(Point, new XYZ(Point.X, Point.Y, (Point.Z + 100))), angle);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog F = new OpenFileDialog();
            if (F.ShowDialog() == true)
            {
                FamilyPath.Text = F.FileName;
            }

        }
    }
}
