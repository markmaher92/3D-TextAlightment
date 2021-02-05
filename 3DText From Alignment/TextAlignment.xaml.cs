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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            ExtractFromLandXML();

            AddTextFamilies();
        }

        private void ExtractFromLandXML()
        {
            List<TextObject> TextObjectFromLandXml = new List<TextObject>();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "LandXML files (*.xml)|*.xml";

            dlg.Title = "Import LandXML and " + "Create 3D Alignments";

            
            if (dlg.ShowDialog() == true)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(dlg.FileName);
                LandXML Landx = Deserialize(dlg.FileName);

                foreach (Alignments Alignments in Landx.Items.OfType<Alignments>())
                {
                    foreach (var Alignment in Alignments.Alignment)
                    {
                        var StationStart = Alignment.staStart;

                        var StationText = StationStart;

                        foreach (CoordGeom CoordGeom in Alignment.Items.OfType<CoordGeom>())
                        {
                            foreach (Line LineItem in CoordGeom.Items.OfType<Line>())
                            {
                                var Ls =  LineItem.staStart;

                                var Point = LineItem.Start.Text;
                                var PointLife = Point[0].Split(' ');
                                double X;
                                Double Y;

                                double.TryParse(PointLife[0], out X);
                                double.TryParse(PointLife[1], out Y);

                                XYZ PointInsert = new XYZ(X, Y, 0);

                                var LineLength = LineItem.length;

                                TextObjectFromLandXml.Add(new TextObject(StationStart, StationText, PointInsert));
                                StationText += LineLength;
                            }
                        }
                    }
                    
                    
                }
                
            }
        }
        public static LandXML Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LandXML));

            StreamReader reader = new StreamReader(path);
            LandXML Schema = (LandXML)serializer.Deserialize(reader);
            reader.Close();

            return Schema;
        }
        private void AddTextFamilies()
        {
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

                try
                {

                    Double RotationAngle = UnitUtils.ConvertToInternalUnits(double.Parse(DegreesTxt.Text), DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                    Angle = Angle + RotationAngle;
                }
                catch (Exception)
                {

                }


                FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);

                Fam.Activate();
                FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Point, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                FillParameters(FamIns, StationStart + i);

                //rotaiton in xy
                XYZ EndPoint = new XYZ(Point.X, Point.Y, (Point.Z + 100));
                LineX L = Autodesk.Revit.DB.Line.CreateBound(Point, EndPoint);
                ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id,L, Angle);

                //rotaiton in xz
                

            }
            else
            {
                StationStart = Math.Round(StationStart + MetricLength, 3);

            }

            return StationStart;
        }

        private void FillParameters(FamilyInstance FamIns, double stationStart)
        {

            try
            {
                if (!string.IsNullOrWhiteSpace(this.ElevationTxt.Text))
                {
                    var elevation = UnitUtils.ConvertToInternalUnits(double.Parse(this.ElevationTxt.Text), DisplayUnitType.DUT_MILLIMETERS);
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
                var HorizontalDistance = UnitUtils.ConvertToInternalUnits(double.Parse(this.HorizontalDistancetext.Text), DisplayUnitType.DUT_MILLIMETERS);
                FamIns.LookupParameter("Horizontal Distance").Set(HorizontalDistance);
            }
            catch (Exception)
            {
            }
            try
            {
                var HorizontalDistance = UnitUtils.ConvertToInternalUnits(double.Parse(this.TextHeightTxt.Text), DisplayUnitType.DUT_MILLIMETERS);
                FamIns.LookupParameter("TextDepth").Set(HorizontalDistance);
            }
            catch (Exception)
            {
            }
            try
            {
                Double Inclinnation = UnitUtils.ConvertToInternalUnits(double.Parse(InclinationTxt.Text), DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                FamIns.LookupParameter("InclinationAngle").Set(Inclinnation);


            }
            catch (Exception)
            {

            }
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
