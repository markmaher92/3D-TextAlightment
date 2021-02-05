using Autodesk.Revit.DB;

namespace _3DText_From_Alignment
{
    internal class TextObject
    {
        public TextObject(double stationStart, double stationText, XYZ pointInsert)
        {
            StationStart = stationStart;
            StationText = stationText;
            PointInsert = pointInsert;
        }

        public double StationStart { get; set; }
        public double StationText { get; set; }
        public XYZ PointInsert { get; set; }
    }
}