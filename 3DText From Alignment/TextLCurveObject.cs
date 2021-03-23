using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace _3DText_From_Alignment
{
    internal class TextCurveObject : LandXmlAlignmentObjects
    {
        public TextCurveObject(double stationStart, XYZ pointInsert, XYZ pointcenter ,XYZ pointEnd, XYZ pointPI ,double CurveLength, double CurveRadius)
        {
            StationStart = stationStart;
            StationEnd = stationStart + Length;
            PointStart = pointInsert;
            PointCenter = pointcenter;
            PointEnd = pointEnd;
            Length = CurveLength;
            CurveRadius = CurveRadius;
            PointPI = pointPI;
        }

        public double StationStart { get; set; }
        public double StationEnd { get; set; }
        public double StationText { get; set; }
        public XYZ PointStart { get; set; }
        public XYZ PointCenter { get; set; }
        public XYZ PointEnd { get; set; }
        public XYZ PointPI { get; set; }

        public double CurveRadius { get; set; }

        public TextCurveObject(TextCurveObject OBJ)
        {
            StationStart = OBJ.StationStart;
            StationEnd = OBJ.StationEnd;
            StationText = OBJ.StationText;
            PointStart = OBJ.PointStart;
            PointCenter = OBJ.PointCenter;
            PointEnd = OBJ.PointEnd;
            Length = OBJ.Length;
            CurveRadius = OBJ.CurveRadius;
        }
        public TextCurveObject ConvertInsertpointsToInternal()
        {
             var PointInsert = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointStart.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointStart.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointStart.Z, DisplayUnitType.DUT_METERS));
             var PointCenter = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointCenter.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointCenter.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointCenter.Z, DisplayUnitType.DUT_METERS));
             var PointEnd = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointEnd.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointEnd.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointEnd.Z, DisplayUnitType.DUT_METERS));
             var PIEnd = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointPI.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointPI.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointPI.Z, DisplayUnitType.DUT_METERS));

            //var LineLength = UnitUtils.ConvertToInternalUnits(this.LineLength, DisplayUnitType.DUT_METERS);
            return new TextCurveObject(this.StationStart,PointInsert,PointCenter, PointEnd, PIEnd, CurveRadius, Length);
        }

       
    }
}