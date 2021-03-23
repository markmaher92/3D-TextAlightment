using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace _3DText_From_Alignment
{
    class XmlParserLandXml
    {
        public static LandXML Deserialize(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlSerializer serializer = new XmlSerializer(typeof(LandXML));
            StreamReader reader = new StreamReader(path);
            LandXML Schema = (LandXML)serializer.Deserialize(reader);
            reader.Close();
            return Schema;
        }

       
        public static List<RevitPlacmenElement> ParseLandXml(string LandXmlPath)
        {
            LandXML Landx = XmlParserLandXml.Deserialize(LandXmlPath);
            List<RevitPlacmenElement> RevitPlacmentPoints  = ExtractPlacementPoints(Landx);
            return RevitPlacmentPoints;
        }

        private static List<RevitPlacmenElement> ExtractPlacementPoints(LandXML Landx, double StationIncrement)
        {
            List<RevitPlacmenElement> RevitPlacementPoints = new List<RevitPlacmenElement>();

            foreach (Alignments Alignments in Landx.Items.OfType<Alignments>())
            {
                foreach (var Alignment in Alignments.Alignment)
                {
                    var StartStation = Alignment.staStart;
                    for (double i = StartStation; i <= Alignment.length + StartStation; i +=  StationIncrement)
                    {
                        var Station = 
                    }
                    var StartStation = StartStation + 
                    foreach (CoordGeom CoordGeom in Alignment.Items.OfType<CoordGeom>())
                    {
                        var CurrentStation = StationStart;
                        foreach (object CoordGeoItem in CoordGeom.Items)
                        {
                            
                            var NextStation = XmlParserLandXml.ExtractRevitPlacementPoints(RevitPlacementPoints , StationStart, CoordGeoItem);
                            ExtractPointHeight(Alignment, StationStart);
                        }

                    }
                }
            }

            return LandXmlTextObjects;
        }

        private static double ExtractRevitPlacementPoints(List<RevitPlacmenElement> revitPlacementPoints, double stationStart, object coordGeoItem)
        {
            throw new NotImplementedException();
        }

        private static void ExtractPointHeight(Alignment Alignment, double stationStart)
        {
            foreach (var Profile in Alignment.Items.OfType<Profile>())
            {
                foreach (var Profilealign in Profile.Items.OfType<ProfAlign>())
                {
                    foreach (var PVIItem in Profilealign.Items)
                    {
                        if (PVIItem is PVI)
                        {
                            XYZ Point = ExtractPVIPoint(PVIItem);
                        }
                        if (PVIItem is CircCurve)
                        {
                            //Handle Curves
                        }
                    }
                }
            }
        }

        private static void ExtractHeightsFromProfile(List<LandXmlAlignmentObjects> TextObjectFromLandXml, List<XYZ> HeighPoints)
        {
            for (int i = 0; i < TextObjectFromLandXml.Count; i++)
            {
                var XStationStart = TextObjectFromLandXml[i].StationStart;
                var XStationEnd = TextObjectFromLandXml[i].StationEnd;

                ExtractHeightForTextObkect(TextObjectFromLandXml[i], HeighPoints, XStationStart, XStationEnd);
            }
        }

        private static void ExtractHeightsFromLandXml(Alignment Alignment, List<XYZ> HeighPoints)
        {
            foreach (Profile Prof in Alignment.Items.OfType<Profile>())
            {
                foreach (var Profilealign in Prof.Items.OfType<ProfAlign>())
                {
                    foreach (var PVIItem in Profilealign.Items)
                    {
                        if (PVIItem is PVI)
                        {
                            ExtractPVIPoint(PVIItem);
                        }
                        if (PVIItem is CircCurve)
                        {
                            //Handle Curves
                        }
                    }
                }
            }
        }

        private static XYZ ExtractPVIPoint(object PVIItem)
        {
            var Tx = ((PVI)PVIItem).Text[0].Split(' ');
            Double PVIX;
            Double PVIZ;

            double.TryParse(Tx[0], out PVIX);
            double.TryParse(Tx[1], out PVIZ);

            return new XYZ(PVIX, 0, PVIZ);
        }
    }
}
