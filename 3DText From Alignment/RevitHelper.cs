using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DText_From_Alignment
{
    class RevitHelper
    {
        private FamilyInstance InsertFamilyInstance(TextLineObject Object, double Angle, FamilySymbol Fam)
        {
            FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Object.PointInsert, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            FillParameters(FamIns, Object.StationText);
            XYZ EndPoint = new XYZ(Object.PointInsert.X, Object.PointInsert.Y, (Object.PointInsert.Z + 100));
            LineX L = Autodesk.Revit.DB.Line.CreateBound(Object.PointInsert, EndPoint);
            ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id, L, Angle);
            return FamIns;
        }

        private FamilyInstance InsertLastFamilyInstance(TextLineObject Object, double Angle, FamilySymbol Fam)
        {
            FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(Object.PointEnd, Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            Object.StationText = Object.LineLength + Object.StationText;
            FillParameters(FamIns, Object.StationText);
            XYZ EndPoint = new XYZ(Object.PointEnd.X, Object.PointEnd.Y, (Object.PointEnd.Z + 100));
            LineX L = Autodesk.Revit.DB.Line.CreateBound(Object.PointEnd, EndPoint);
            ElementTransformUtils.RotateElement(FamIns.Document, FamIns.Id, L, Angle);
            return FamIns;
        }
        private double ModifyRotationAngle(LandXmlAlignmentObjects Object)
        {
            XYZ NormalVector = ((Object as TextLineObject).PointEnd - (Object as TextLineObject).PointInsert).Normalize();
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
        private void InsertFamilyAtStation(LandXmlAlignmentObjects Object, string FamilyName, bool last)
        {
            double Angle = ModifyRotationAngle(Object);

            FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
            Fam.Activate();

            FamilyInstance FamIns = null;


            //sHOULD cONVERT
            if (last)
            {
                FamIns = InsertLastFamilyInstance((Object as TextLineObject).ConvertInsertpointsToInternal(), Angle, Fam);

            }
            else
            {
                FamIns = InsertFamilyInstance((Object as TextLineObject).ConvertInsertpointsToInternal(), Angle, Fam);
            }



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
        private void InsertElementsBetweenStations(TextLineObject Object, List<XYZ> HeightPoints, string FamilyName, double LastLineLength)
        {
            double Angle = ModifyRotationAngle(Object);
            double StationIncrement = ExtractStationDisntace();

            FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
            Fam.Activate();

            LineX L = LineX.CreateBound(Object.PointInsert, Object.PointEnd);
            double LineLength = Object.StationEnd - Object.StationStart;

            if (StationIncrement == 0)
            {
                return;
            }


            double Modification = 0;

            var Value = Math.Ceiling(LastLineLength / StationIncrement);
            var ValueNew = (Value * StationIncrement);

            for (double i = ValueNew; i <= (LineLength + Object.StationStart); i += StationIncrement)
            {
                try
                {
                    TextLineObject MidPointTE = CreateMidPoint1(Object, HeightPoints, L, i);
                    FamilyInstance FamIns = null;
                    FamIns = InsertFamilyInstance(MidPointTE.ConvertInsertpointsToInternal(), Angle, Fam);
                }
                catch (Exception ee)
                {

                }
            }
        }
        public static void PlaceRevitFamilies(List<LandXmlAlignmentObjects> obects, UIDocument uiDoc, String FamilyPath)
        {
            string FamilyName = "3DAlignment_Tool";

            using (Transaction T = new Transaction(uiDoc.Document, "Create labes"))
            {
                T.Start();
                try
                {
                    uiDoc.Document.LoadFamily(FamilyPath);
                    FamilyName = System.IO.Path.GetFileNameWithoutExtension(FamilyPath);
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
        private void AcheStationingAndFamilyInsert(List<LandXmlAlignmentObjects> obects, string FamilyName)
        {
            double LastLineLength = 0;
            for (int i = 0; i < obects.Count; i++)
            {
                InsertFamilyAtStation(obects[i], FamilyName, false);

                if (i != 0)
                {
                    LastLineLength = obects[i - 1].StationEnd;
                }
                InsertElementsBetweenStations(obects[i], obects.Item2, FamilyName, LastLineLength);

            }
            InsertFamilyAtStation(obects.Last(), FamilyName, true);

        }
    }
}
