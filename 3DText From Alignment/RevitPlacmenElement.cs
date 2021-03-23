using Autodesk.Revit.DB;

namespace _3DText_From_Alignment
{

    class RevitPlacmenElement
    {
        public XYZ InsertPoint { get; set; }
        public double Station { get; set; }
        public double Elevation { get; set; }
        public double AbstandX { get; set; }
        public double AbstandY { get; set; }
    }
}
