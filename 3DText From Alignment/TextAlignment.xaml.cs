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

            List<TextObject> Obects = ExtractFromLandXML();

            AddTextFamilies(Obects);
        }

        private List<TextObject> ExtractFromLandXML()
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
                                var Ls = LineItem.staStart;

                                var Point = LineItem.Start.Text;
                                var PointLife = Point[0].Split(' ');
                                Double X;
                                Double Y;


                                double.TryParse(PointLife[0], out X);
                                double.TryParse(PointLife[1], out Y);

                                var Xcon = UnitUtils.ConvertToInternalUnits(X,DisplayUnitType.DUT_METERS);
                                var Ycon = UnitUtils.ConvertToInternalUnits(Y, DisplayUnitType.DUT_METERS);
                                XYZ PointStart = new XYZ(Xcon, Ycon, 0);


                                var PointendX = LineItem.End.Text;
                                var PointEndARr = PointendX[0].Split(' ');
                                Double XEnd;
                                Double YEnd;

                                double.TryParse(PointEndARr[0], out XEnd);
                                double.TryParse(PointEndARr[1], out YEnd);

                                var XconEnd = UnitUtils.ConvertToInternalUnits(XEnd, DisplayUnitType.DUT_METERS);
                                var YconEnd = UnitUtils.ConvertToInternalUnits(YEnd, DisplayUnitType.DUT_METERS);
                                XYZ PointEnd = new XYZ(XconEnd, YconEnd, 0);
                            
                                var LineLength = LineItem.length;

                                TextObjectFromLandXml.Add(new TextObject(StationStart, StationText, PointStart, PointEnd, LineLength));

                                StationText += LineLength;
                            }
                        }
                    }


                }

            }

            return TextObjectFromLandXml;
        }
        public static LandXML Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LandXML));

            StreamReader reader = new StreamReader(path);
            LandXML Schema = (LandXML)serializer.Deserialize(reader);
            reader.Close();

            return Schema;
        }
        private void AddTextFamilies(List<TextObject> obects)
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
                    AcheStationingAndFamilyInsert(obects, FamilyName);

                }
                catch (Exception)
                {

                }

                T.Commit();
            }
        }

        private void AcheStationingAndFamilyInsert(List<TextObject> Objects, string FamilyName)
        {
            
            foreach (TextObject Object in Objects)
            {
               
                try
                {
                    var geomLine = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, Object.PointEnd);
                    Autodesk.Revit.DB.Line LineX = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, Object.PointEnd);
                    var line = uiDoc.Document.Create.NewDetailCurve(uiDoc.ActiveView ,geomLine);


                    //XYZ origin = Object.PointInsert;
                    //XYZ normal = new XYZ(Object.PointInsert.X, Object.PointInsert.Y, 1);
                    //Plane geomPlane = Plane.CreateByThreePoints(XYZ.BasisX, XYZ.BasisY);
                    //Plane geomPlane = Plane.CreateByNormalAndOrigin(normal, origin);

                    //XYZ origin = new XYZ(0, 0, 0);
                    //XYZ normal = new XYZ(1, 1, 0);
                    //Plane geomPlane = Plane.CreateByNormalAndOrigin(normal, origin);
                    //SketchPlane sketch = SketchPlane.Create(uiDoc.Document, geomPlane);
                    //ModelLine line1 = uiDoc.Document.Create.NewModelCurve(geomLine, sketch) as ModelLine;

                }
                catch (Exception e)
                {

                }
              

                InsertFamilyAtStation(Object, FamilyName);

               
            }

        }

        private void InsertFamilyAtStation(TextObject Object, string FamilyName)
        {
            XYZ NormalVector = (Object.PointEnd - Object.PointInsert).Normalize();
            //double Angle = (Math.PI / 2) - NormalVector.AngleTo(XYZ.BasisX);
            double Angle = NormalVector.AngleTo(XYZ.BasisX) + (Math.PI/2) + Math.PI;

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
            FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Object.PointInsert, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            FillParameters(FamIns, Object.StationText);

            XYZ EndPoint = new XYZ(Object.PointInsert.X, Object.PointInsert.Y, (Object.PointInsert.Z + 100));
            LineX L = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, EndPoint);
            ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id, L, Angle);

        }

        private void FillParameters(FamilyInstance FamIns, double Stationtext)
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
                var RoundedX = Math.Round(Stationtext, 3);
                //var Station = double.Parse(this.StartingStationTxt.Text) + i;
                FamIns.LookupParameter("Text").Set(RoundedX.ToString());
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
