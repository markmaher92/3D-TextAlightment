using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace _3DText_From_Alignment
{
    internal class TextObject
    {
        public TextObject(double stationStart, double stationEnd, double stationText, XYZ pointInsert, XYZ pointEnd, double lineLength)
        {
            StationStart = stationStart;
            StationEnd = stationEnd;
            StationText = stationText;
            PointInsert = pointInsert;
            PointEnd = pointEnd;
            LineLength = lineLength;
        }

        public double StationStart { get; set; }
        public double StationEnd { get; set; }
        public double StationText { get; set; }
        public XYZ PointInsert { get; set; }
        public XYZ PointEnd { get; set; }
        public double LineLength { get; set; }

        public TextObject(TextObject OBJ)
        {
            StationStart = OBJ.StationStart;
            StationEnd = OBJ.StationEnd;
            StationText = OBJ.StationText;
            PointInsert = OBJ.PointInsert;
            PointEnd = OBJ.PointEnd;
            LineLength = OBJ.LineLength;
        }
        public TextObject ConvertInsertpointsToInternal()
        {
             var PointInsert = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointInsert.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointInsert.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointInsert.Z, DisplayUnitType.DUT_METERS));
             var PointEnd = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointEnd.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointEnd.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointEnd.Z, DisplayUnitType.DUT_METERS));
            //var LineLength = UnitUtils.ConvertToInternalUnits(this.LineLength, DisplayUnitType.DUT_METERS);
            return new TextObject(this.StationStart, this.StationEnd, this.StationText, PointInsert, PointEnd, LineLength);
        }

    }
}