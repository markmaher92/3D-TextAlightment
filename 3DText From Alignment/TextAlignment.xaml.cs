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

            var Obects = ExtractFromLandXML();

            AddTextFamilies(Obects);
        }


        private (List<TextObject>, List<XYZ>) ExtractFromLandXML()
        {
            List<TextObject> TextObjectFromLandXml = new List<TextObject>();
            List<XYZ> HeighPoints = new List<XYZ>();

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
                                StationStart = ExtractLandXmlLine(TextObjectFromLandXml, StationStart, LineItem);
                            }
                        }

                        ExtractHeightsFromLandXml(Alignment, HeighPoints);

                        ExtractHeightsFromProfile(TextObjectFromLandXml, HeighPoints);

                    }
                }
            }
            return (TextObjectFromLandXml, HeighPoints);
        }

        private static double ExtractLandXmlLine(List<TextObject> TextObjectFromLandXml, double StationStart, Line LineItem)
        {
            var Ls = LineItem.staStart;

            var Point = LineItem.Start.Text;
            var PointLife = Point[0].Split(' ');
            Double X;
            Double Y;


            double.TryParse(PointLife[0], out X);
            double.TryParse(PointLife[1], out Y);

            XYZ PointStart = new XYZ(Y, X, 0);


            var PointendX = LineItem.End.Text;
            var PointEndARr = PointendX[0].Split(' ');
            Double XEnd;
            Double YEnd;

            double.TryParse(PointEndARr[0], out XEnd);
            double.TryParse(PointEndARr[1], out YEnd);

            XYZ PointEnd = new XYZ(YEnd, XEnd, 0);

            var LineLength = LineItem.length;

            double StationEnd = StationStart + LineLength;
            TextObjectFromLandXml.Add(new TextObject(StationStart, StationEnd, StationStart, PointStart, PointEnd, LineLength));
            StationStart += LineLength;
            return StationStart;
        }

        private static void ExtractHeightsFromProfile(List<TextObject> TextObjectFromLandXml, List<XYZ> HeighPoints)
        {
            for (int i = 0; i < TextObjectFromLandXml.Count; i++)
            {
                var XStationStart = TextObjectFromLandXml[i].StationStart;

                var XStationEnd = TextObjectFromLandXml[i].StationEnd;

                ExtractHeightForTextObkect(TextObjectFromLandXml[i], HeighPoints, XStationStart, XStationEnd);
            }
        }

        private static void ExtractHeightForTextObkect(TextObject TextObject, List<XYZ> HeighPoints, double XStationStart, double XStationEnd)
        {
            for (int J = 0; J < HeighPoints.Count; J++)
            {
                bool Cond = false;
                var InsertPoint = TextObject.PointInsert;
                if (J == 0)
                {
                    Cond = ExtrachtHeightForStation(ref InsertPoint, XStationStart,TextObject.LineLength ,HeighPoints[J], null);
                }
                else
                {
                    Cond = ExtrachtHeightForStation(ref InsertPoint, XStationStart, TextObject.LineLength, HeighPoints[J], HeighPoints[J - 1]);
                }
                TextObject.PointInsert = InsertPoint;
                if (Cond)
                {
                    break;
                }

            }
            for (int J = 0; J < HeighPoints.Count; J++)
            {
                var InsertEnd = TextObject.PointEnd;

                bool Cond = false;
                if (J == 0)
                {
                    Cond = ExtrachtHeightForStation(ref InsertEnd, XStationEnd, TextObject.LineLength, HeighPoints[J], null);
                }
                else
                {
                    Cond = ExtrachtHeightForStation(ref InsertEnd, XStationEnd, TextObject.LineLength, HeighPoints[J], HeighPoints[J - 1]);
                }
                TextObject.PointEnd = InsertEnd;
                if (Cond)
                {
                    break;
                }

            }
        }

        private static bool ExtrachtHeightForStation(ref XYZ PointToFill, double StationToStudy, double lineLength, XYZ HeightPoint, XYZ PointBeforeIt = null)
        {
            if (HeightPoint.X == StationToStudy)
            {
                PointToFill = new XYZ(PointToFill.X, PointToFill.Y, HeightPoint.Z);
                return true;
            }
            else
            {
                if (HeightPoint.X > StationToStudy)
                {
                    if (PointBeforeIt != null)
                    {
                        LineX LL = LineX.CreateBound(PointBeforeIt, HeightPoint);
                        XYZ Vector = HeightPoint - PointBeforeIt;
                        var Angle = XYZ.BasisX.AngleTo(Vector) * 180/Math.PI;
                        var XPoint = (StationToStudy - PointBeforeIt.X);
                        var point = LL.Evaluate((XPoint / lineLength), true);
                        PointToFill = new XYZ(PointToFill.X, PointToFill.Y, point.Z );
                    }
                    return true;
                }
            }
            return false;
        }

        private static void ExtractHeightsFromLandXml(Alignment Alignment, List<XYZ> HeighPoints)
        {
            foreach (Profile Prof in Alignment.Items.OfType<Profile>())
            {
                foreach (var Profilealign in Prof.Items.OfType<ProfAlign>())
                {
                    foreach (var PVI in Profilealign.Items.OfType<PVI>())
                    {
                        var Tx = PVI.Text[0].Split(' ');
                        Double PVIX;
                        Double PVIZ;

                        double.TryParse(Tx[0], out PVIX);
                        double.TryParse(Tx[1], out PVIZ);

                        HeighPoints.Add(new XYZ(PVIX, 0, PVIZ));
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
        private void AddTextFamilies((List<TextObject>, List<XYZ>) obects)
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
                catch (Exception de)
                {

                }

                T.Commit();
            }
        }

        private void AcheStationingAndFamilyInsert((List<TextObject>, List<XYZ>) obects, string FamilyName)
        {
            foreach (TextObject Object in obects.Item1)
            {
                InsertFamilyAtStation(Object, FamilyName, false);

                InsertElementsBetweenStations(Object, obects.Item2, FamilyName);
            }
            InsertFamilyAtStation(obects.Item1.Last(), FamilyName, true);

        }

        private void InsertElementsBetweenStations(TextObject Object, List<XYZ> HeightPoints, string FamilyName)
        {
            double Angle = ModifyRotationAngle(Object);
            double StationIncrement = ExtractStationDisntace();

            FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
            Fam.Activate();

            LineX L = LineX.CreateBound(Object.PointInsert, Object.PointEnd);
            int J = 1;
            for (double i = StationIncrement; i < (L.Length - StationIncrement); i += StationIncrement)
            {
                TextObject MidPointTE = CreateMidPoint(Object, HeightPoints, L, StationIncrement, J, i);

                FamilyInstance FamIns = null;
                FamIns = InsertFamilyInstance(MidPointTE.ConvertInsertpointsToInternal(), Angle, Fam);

                if (StationIncrement == 0)
                {
                    return;
                }
                J++;
            }
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

        private void InsertFamilyAtStation(TextObject Object, string FamilyName, bool last)
        {
            double Angle = ModifyRotationAngle(Object);

            FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
            Fam.Activate();

            FamilyInstance FamIns = null;


            //sHOULD cONVERT
            if (last)
            {
                FamIns = InsertLastFamilyInstance(Object.ConvertInsertpointsToInternal(), Angle, Fam);

            }
            else
            {
                FamIns = InsertFamilyInstance(Object.ConvertInsertpointsToInternal(), Angle, Fam);
            }



        }

        private static TextObject CreateMidPoint(TextObject Object, List<XYZ> HeightPoints, LineX L, double StationIncrement, int J, double i)
        {
            var MidPointTE = new TextObject(Object);

            var StationText = (MidPointTE.StationStart + (StationIncrement * J));
            var StationXRatio = (StationIncrement * J) / Object.LineLength;
            
            MidPointTE.PointInsert = L.Evaluate(StationXRatio, true);
            MidPointTE.StationText = StationText;
            ExtractHeightForTextObkect(MidPointTE, HeightPoints, StationText, MidPointTE.StationEnd);
            return MidPointTE;
        }

        private double ModifyRotationAngle(TextObject Object)
        {
            XYZ NormalVector = (Object.PointEnd - Object.PointInsert).Normalize();
            double Angle = (Math.PI / 2) - NormalVector.AngleTo(XYZ.BasisX);
            try
            {
                Double RotationAngle = UnitUtils.ConvertToInternalUnits(double.Parse(DegreesTxt.Text), DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                Angle = Angle + RotationAngle;
            }
            catch (Exception)
            {

            }
            return Angle;
        }

        private FamilyInstance InsertFamilyInstance(TextObject Object, double Angle, FamilySymbol Fam)
        {
            FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Object.PointInsert, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            FillParameters(FamIns, Object.StationText);
            XYZ EndPoint = new XYZ(Object.PointInsert.X, Object.PointInsert.Y, (Object.PointInsert.Z + 100));
            LineX L = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, EndPoint);
            ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id, L, Angle);
            return FamIns;
        }

        private FamilyInstance InsertLastFamilyInstance(TextObject Object, double Angle, FamilySymbol Fam)
        {
            FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Object.PointEnd, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            Object.StationText = Object.LineLength + Object.StationText;
            FillParameters(FamIns, Object.StationText);
            XYZ EndPoint = new XYZ(Object.PointEnd.X, Object.PointEnd.Y, (Object.PointEnd.Z + 100));
            LineX L = Autodesk.Revit.DB.Line.CreateBound(Object.PointEnd, EndPoint);
            ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id, L, Angle);
            return FamIns;
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
