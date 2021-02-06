using Autodesk.Revit.DB;

namespace _3DText_From_Alignment
{
    internal class TextObject
    {
        public TextObject(double stationStart, double stationText, XYZ pointInsert, XYZ pointEnd, double lineLength)
        {
            StationStart = stationStart;
            StationText = stationText;
            PointInsert = pointInsert;
            PointEnd = pointEnd;
            LineLength = lineLength;
        }

        public double StationStart { get; set; }
        public double StationText { get; set; }
        public XYZ PointInsert { get; set; }
        public XYZ PointEnd { get; set; }
        public double LineLength { get; set; }
    }
}