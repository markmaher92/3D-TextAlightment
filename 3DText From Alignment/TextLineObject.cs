using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace _3DText_From_Alignment
{
    internal class TextLineObject : LandXmlAlignmentObjects
    {
        public TextLineObject(double stationStart, XYZ pointInsert, XYZ pointEnd, double lineLength)
        {
            StationStart = stationStart;
            StationEnd = stationStart + lineLength;
            PointInsert = pointInsert;
            PointEnd = pointEnd;
            Length = lineLength;
        }

      
        public XYZ PointInsert { get; set; }
        public XYZ PointEnd { get; set; }

        public TextLineObject(TextLineObject OBJ)
        {
            StationStart = OBJ.StationStart;
            StationEnd = OBJ.StationEnd;
            PointInsert = OBJ.PointInsert;
            PointEnd = OBJ.PointEnd;
            Length = OBJ.Length;
        }
        public TextLineObject ConvertInsertpointsToInternal()
        {
             var PointInsert = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointInsert.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointInsert.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointInsert.Z, DisplayUnitType.DUT_METERS));
             var PointEnd = new XYZ(UnitUtils.ConvertToInternalUnits(this.PointEnd.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointEnd.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(this.PointEnd.Z, DisplayUnitType.DUT_METERS));
            //var LineLength = UnitUtils.ConvertToInternalUnits(this.LineLength, DisplayUnitType.DUT_METERS);
            return new TextLineObject(this.StationStart, PointInsert, PointEnd, Length);
        }

     
    }
}