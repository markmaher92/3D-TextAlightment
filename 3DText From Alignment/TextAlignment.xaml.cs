﻿using Autodesk.Revit.DB;
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
                var XStationEnd = TextObjectFromLandXml[i].StationStart;

                for (int J = 0; J < HeighPoints.Count; J++)
                {
                    if (HeighPoints[J].X > XStation)
                    {
                        //var STABeforeX = HeighPoints[J - 1].X;
                        //var STAAfterX = HeighPoints[J].X;
                        //var STABeforeZ = HeighPoints[J - 1].Z;
                        //var STAAfterZ = HeighPoints[J].Z;

                        //var TrianleBottom = STAAfterZ - STABeforeZ;
                        //var TriangleSide = STAAfterX - STABeforeX;

                        //var ZAtPoint = ((XStation - STABeforeX) / TriangleSide) * TrianleBottom;

                        LineX LL = LineX.CreateBound(HeighPoints[J - 1], HeighPoints[J]);
                        var point = LL.Evaluate((XStation - HeighPoints[J - 1].X) / LL.Length, true);
                        TextObjectFromLandXml[i].PointInsert = new XYZ(TextObjectFromLandXml[i].PointInsert.X, TextObjectFromLandXml[i].PointInsert.Y, point.Z);

                        break;

                    }
                }
                //var R = HermitCurve.Project(new XYZ(TextObjectFromLandXml[i].StationText, 0, 0));
                //LineX L = LineX.CreateUnbound(new XYZ(TextObjectFromLandXml[i].StationText, 0, 0), new XYZ(TextObjectFromLandXml[i].StationText, 0, 1));

                //IntersectionResultArray Result = null;
                //HermitCurve.Intersect(L, out Result);
                ////var XAcon = UnitUtils.ConvertToInternalUnits(R.XYZPoint.Z, DisplayUnitType.DUT_METERS);
                ////TextObjectFromLandXml[i].PointInsert = new XYZ(TextObjectFromLandXml[i].PointInsert.X, TextObjectFromLandXml[i].PointInsert.Y, XAcon);
                //TextObjectFromLandXml[i].PointInsert = new XYZ(TextObjectFromLandXml[i].PointInsert.X, TextObjectFromLandXml[i].PointInsert.Y, R.XYZPoint.Z);

                //var R2 = HermitCurve.Project(new XYZ(TextObjectFromLandXml[i].StationEnd, 0, 0));

                //var Parameter = TextObjectFromLandXml[i].StationEnd / HermitCurve.Length;
                //var Test = HermitCurve.Evaluate(Parameter,true);

                //LineX L2 = LineX.CreateUnbound(new XYZ(TextObjectFromLandXml[i].StationEnd, 0, 0), new XYZ(TextObjectFromLandXml[i].StationEnd, 0, 100));

                //IntersectionResultArray Result2 = null;
                //HermitCurve.Intersect(L2, out Result2);
                ////var XAcon2 = UnitUtils.ConvertToInternalUnits(R2.XYZPoint.Z, DisplayUnitType.DUT_METERS);
                ////TextObjectFromLandXml[i].PointEnd = new XYZ(TextObjectFromLandXml[i].PointEnd.X, TextObjectFromLandXml[i].PointEnd.Y, XAcon2);
                //TextObjectFromLandXml[i].PointEnd = new XYZ(TextObjectFromLandXml[i].PointEnd.X, TextObjectFromLandXml[i].PointEnd.Y, R2.XYZPoint.Z);

                //var Param = TextObjectFromLandXml[i].StationEnd;
                //HermitCurve.Evaluate(Param, false);

                //var Param1 = TextObjectFromLandXml[i].StationEnd / HermitCurve.Length;
                //HermitCurve.Evaluate(Param1, true);

                //var Param11 = TextObjectFromLandXml[i].StationEnd / HermitCurve.ApproximateLength;
                //HermitCurve.Evaluate(Param11, true);


            }
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
                catch (Exception)
                {

                }

                T.Commit();
            }
        }

        private void AcheStationingAndFamilyInsert((List<TextObject>, List<XYZ>) obects, string FamilyName)
        {
            foreach (TextObject Object in obects.Item1)
            {
                //var geomLine = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, Object.PointEnd);
                //Autodesk.Revit.DB.Line LineX = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, Object.PointEnd);
                //try
                //{
                //    var line = uiDoc.Document.Create.NewDetailCurve(uiDoc.ActiveView, geomLine);
                //}
                //catch (Exception e)
                //{
                //}
                InsertFamilyAtStation(Object, FamilyName, obects.Item2);
            }
            InsertFamilyAtStation(obects.Item1.Last(), FamilyName, true);
        }

        private void InsertFamilyAtStation(TextObject Object, string FamilyName, bool last = false)
        {
            double Angle = ModifyRotationAngle(Object);

            FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
            Fam.Activate();

            FamilyInstance FamIns = null;
            if (last)
            {
                FamIns = InsertLastFamilyInstance(Object.ConvertInsertpointsToInternal(), Angle, Fam);
            }
            //else
            //{
            //    FamIns = InsertFamilyInstance(Object, Angle, Fam);
            //}
        }

        private void InsertFamilyAtStation(TextObject Object, string FamilyName, List<XYZ> HermitCurve)
        {
            double Angle = ModifyRotationAngle(Object);

            FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
            Fam.Activate();

            FamilyInstance FamIns = null;


            //sHOULD cONVERT

            FamIns = InsertFamilyInstance(Object.ConvertInsertpointsToInternal(), Angle, Fam);

            if (string.IsNullOrEmpty(this.StationDistanceTxt.Text))
            {
                this.StationDistanceTxt.Text = 0.ToString();
            }
            var StationIncrement = double.Parse(this.StationDistanceTxt.Text);
            var StationIncementConv = UnitUtils.ConvertToInternalUnits(double.Parse(this.StationDistanceTxt.Text), DisplayUnitType.DUT_METERS);


            LineX L = LineX.CreateBound(Object.PointInsert, Object.PointEnd);
            int J = 1;
            for (double i = StationIncementConv; i < L.Length; i += StationIncementConv)
            {
                //error
                //TextObject MidPointTE = CreateMidPoint(Object, HermitCurve, L, StationIncrement, J, i);

                //FamIns = InsertFamilyInstance(MidPointTE, Angle, Fam);

                //if (StationIncementConv == 0)
                //{
                //    return;
                //}
                //J++;
            }

        }

        private static TextObject CreateMidPoint(TextObject Object, Autodesk.Revit.DB.Curve HermitCurve, LineX L, double StationIncrement, int J, double i)
        {
            var MidPointTE = new TextObject(Object);
            MidPointTE.PointInsert = L.Evaluate(i, false);
            //MidPointTE.StationText = MidPointTE.StationText + (StationIncrement * J);
            MidPointTE.StationText = MidPointTE.StationStart + (StationIncrement * J);

            var R = HermitCurve.Project(new XYZ(MidPointTE.StationText, 0, 0));
            var XAcon = UnitUtils.ConvertToInternalUnits(R.XYZPoint.Z, DisplayUnitType.DUT_METERS);
            MidPointTE.PointInsert = new XYZ(MidPointTE.PointInsert.X, MidPointTE.PointInsert.Y, XAcon);
            return MidPointTE;
        }

        private double ModifyRotationAngle(TextObject Object)
        {
            XYZ NormalVector = (Object.PointEnd - Object.PointInsert).Normalize();
            double Angle = (Math.PI / 2) - NormalVector.AngleTo(XYZ.BasisX);
            //double Angle = NormalVector.AngleTo(XYZ.BasisX) + (Math.PI / 2) + Math.PI;
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
