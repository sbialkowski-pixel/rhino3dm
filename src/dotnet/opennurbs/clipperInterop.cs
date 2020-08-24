using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipperLib;

namespace Rhino.Geometry
{
    public static class ClipperInterop
    {
        public static List<IntPoint> ToClipper(Curve crv)
        {
            NurbsCurve ncrv = crv.ToNurbsCurve();
            List<IntPoint> intPath = new List<IntPoint>(ncrv.Points.Count);
            foreach (ControlPoint cpt in ncrv.Points)
            {
                intPath.Add(ToClipper(cpt.Location));
            }
            intPath = UnifyClipper(intPath);
            return intPath;
        }

        public static IntPoint ToClipper(Point3d pt) {
            Point3d tempPt = pt * Factor();
            return new IntPoint((long)tempPt.X, (long)tempPt.Y);
        }


        public static PolylineCurve FromClipper(List<IntPoint> path)
        {

            List<Point3d> points = new List<Point3d>();
            foreach (IntPoint intPt in path)
            {
                points.Add(FromClipper(intPt));
            }
            points.Add(FromClipper(path[0]));
            if (points.Count > 2)
            {
                Polyline ply = new Polyline(points);
                return ply.ToPolylineCurve();
            }
            else return null;
        }

        public static Point3d FromClipper(IntPoint pt)
        {
            double fraction = 1.0 / Factor();
            return new Point3d((double)pt.X * fraction, (double)pt.Y * fraction, 0);
        }
        private static List<IntPoint> UnifyClipper(List<IntPoint> path)
        {
            if (!Clipper.Orientation(path))
            {
                path.Reverse();
            }
            return path;
        }
        private static double Factor()
        {
            return 1000000;
        }

    }
}
