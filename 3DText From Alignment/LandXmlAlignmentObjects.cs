using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DText_From_Alignment
{
    class LandXmlAlignmentObjects
    {
        public String AlignmentName { get; set; }
        public double StationStart { get; set; }
        public double StationEnd { get; set; }
        public double Length { get; set; }
        public CoordGeom LandXmlTextObject { get; set; }
    }
    private static double ExtractLandXmlLine(List<LandXmlAlignmentObjects> TextObjectFromLandXml, double StationStart, Line LineItem)
    {
        var Ls = LineItem.staStart;
        var Point = LineItem.Start.Text;
        XYZ PointStart = ExtractPoint(Point);

        var PointendX = LineItem.End.Text;
        XYZ PointEnd = ExtractPoint(PointendX);

        var LineLength = LineItem.length;

        double StationEnd = StationStart + LineLength;
        TextObjectFromLandXml.Add(new TextLineObject(StationStart, StationEnd, StationStart, PointStart, PointEnd, LineLength));
        StationStart += LineLength;
        return StationStart;
    }
    private double ExtractLandXmlCurve(List<LandXmlAlignmentObjects> textObjectFromLandXml, double stationStart, Curve Cs)
    {
        var LS = Cs.staStart;
        var StartPoint = (Cs.Items[0] as PointType).Text;
        var CenterPoint = (Cs.Items[1] as PointType).Text;
        var EndPoint = (Cs.Items[2] as PointType).Text;

        XYZ PointPI = null;
        if (Cs.Items.Count() > 3)
        {
            var PI = (Cs.Items[3] as PointType).Text;
            PointPI = ExtractPoint(PI);
            var Radius = Cs.radius;
            var CurveLength = Cs.length;

            double stationEnd = stationStart + Cs.length;

            XYZ PointStart = ExtractPoint(StartPoint);
            XYZ PointCenter = ExtractPoint(CenterPoint);
            XYZ PointEnd = ExtractPoint(EndPoint);

            TextCurveObject CUrveEle = new TextCurveObject(stationStart, stationEnd, stationStart, PointStart, PointCenter, PointEnd, PointPI, Radius, CurveLength);
            var ElementCnv = CUrveEle.ConvertInsertpointsToInternal();
            Arc HS = Arc.Create(ElementCnv.PointStart, ElementCnv.PointEnd, ElementCnv.PointPI);

            // Arc Arcy = Arc.Create()

            textObjectFromLandXml.Add(CUrveEle);

            using (Transaction se = new Transaction(uiDoc.Document, "New Tra"))
            {
                se.Start();
                uiDoc.Document.Create.NewDetailCurve(uiDoc.ActiveView, HS);
                se.Commit();
            }
        }

        return stationStart;
    }

    private static void ExtractHeightForTextObkect(LandXmlAlignmentObjects TextObject, List<XYZ> HeighPoints, double XStationStart, double XStationEnd)
    {
        if (TextObject is Line)
        {
            for (int J = 0; J < HeighPoints.Count; J++)
            {
                var InsertPoint = (TextObject as TextLineObject).PointInsert;

                bool Cond = HeightExtraction(HeighPoints, XStationStart, J, ref InsertPoint);

                (TextObject as TextLineObject).PointInsert = InsertPoint;
                if (Cond)
                {
                    break;
                }

            }
            for (int J = 0; J < HeighPoints.Count; J++)
            {
                var InsertEnd = (TextObject as TextLineObject).PointEnd;
                bool Cond = HeightExtraction(HeighPoints, XStationEnd, J, ref InsertEnd);
                (TextObject as TextLineObject).PointEnd = InsertEnd;

                if (Cond)
                {
                    break;
                }

            }
        }
    }

    private static bool HeightExtraction(List<XYZ> HeighPoints, double XStationStart, int J, ref XYZ InsertPoint)
    {
        bool Cond;
        if (J == 0)
        {
            Cond = ExtrachtHeightForStation(ref InsertPoint, XStationStart, HeighPoints[J], null);
        }
        else
        {
            Cond = ExtrachtHeightForStation(ref InsertPoint, XStationStart, HeighPoints[J], HeighPoints[J - 1]);
        }

        return Cond;
    }

    private static bool ExtrachtHeightForStation(ref XYZ PointToFill, double StationToStudy, XYZ HeightPoint, XYZ PointBeforeIt = null)
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
                    var Angle = XYZ.BasisX.AngleTo(Vector) * 180 / Math.PI;
                    var XPoint = (StationToStudy - PointBeforeIt.X);
                    var point = LL.Evaluate((XPoint / (HeightPoint.X - PointBeforeIt.X)), true);
                    PointToFill = new XYZ(PointToFill.X, PointToFill.Y, point.Z);
                }
                return true;
            }
        }
        return false;
    }


    private static XYZ ExtractPoint(string[] Point)
    {
        var PointLife = Point[0].Split(' ');
        Double X;
        Double Y;

        double.TryParse(PointLife[0], out X);
        double.TryParse(PointLife[1], out Y);

        XYZ PointStart = new XYZ(Y, X, 0);
        return PointStart;
    }

}
