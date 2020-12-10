using System;
using System.Linq;
using Pixel.Rhino.Display;
using System.Runtime.Serialization;
using Pixel.Rhino.Runtime.InteropWrappers;
using Pixel.Rhino.Runtime;

namespace Pixel.Rhino.Geometry
{
    /// <summary>
    /// Represents the geometry of a set of linked line segments.
    /// <para>This is fundamentally a class that derives from <see cref="Curve"/>
    /// and internally contains a <see cref="Polyline"/>.</para>
    /// </summary>
    [Serializable]
    public class PolylineCurve : Curve
    {
        #region constructors

        /// <summary>
        /// Initializes a new empty polyline curve.
        /// </summary>
        /// <since>5.0</since>
        public PolylineCurve()
        {
            IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New(IntPtr.Zero);
            ConstructNonConstObject(ptr);
        }

        /// <summary>
        /// Initializes a new polyline curve by copying its content from another polyline curve.
        /// </summary>
        /// <param name="other">Another polyline curve.</param>
        /// <since>5.0</since>
        public PolylineCurve(PolylineCurve other)
        {
            IntPtr pOther = IntPtr.Zero;
            if (null != other)
                pOther = other.ConstPointer();
            IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New(pOther);
            ConstructNonConstObject(ptr);
            GC.KeepAlive(other);
        }

        /// <summary>
        /// Initializes a new polyline curve by copying its content from another set of points.
        /// </summary>
        /// <param name="points">A list, an array or any enumerable set of points to copy from.
        /// This includes a <see cref="Polyline"/> object.</param>
        /// <since>5.0</since>
        public PolylineCurve(System.Collections.Generic.IEnumerable<Point3d> points)
        {
            int count;
            Point3d[] ptArray = Pixel.Rhino.Collections.RhinoListHelpers.GetConstArray(points, out count);
            IntPtr ptr;
            if (null == ptArray || count < 1)
            {
                ptr = UnsafeNativeMethods.ON_PolylineCurve_New(IntPtr.Zero);
            }
            else
            {
                ptr = UnsafeNativeMethods.ON_PolylineCurve_New2(count, ptArray);
            }
            ConstructNonConstObject(ptr);
        }

        internal PolylineCurve(IntPtr ptr, object parent, int subobject_index)
          : base(ptr, parent, subobject_index)
        {
        }

        /// <summary>
        /// Protected constructor for internal use.
        /// </summary>
        /// <param name="info">Serialization data.</param>
        /// <param name="context">Serialization stream.</param>
        protected PolylineCurve(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
        #endregion

        /// <summary>
        /// [Giulio - 2018 03 29] This static factory method skips all checks and simply calls the C++ instantiator.
        /// You are responsible for providing a correct count, that is: larger than 2 and less or equal points.Length.
        /// Use the public PolylineCurve constructor with IEnumerable when in doubt. See RH-45133.
        /// </summary>
        internal static PolylineCurve Internal_FromArray(Point3d[] points, int count)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New2(count, points);
            return new PolylineCurve(ptr, null, -1);
        }

        internal override GeometryBase DuplicateShallowHelper()
        {
            return new PolylineCurve(IntPtr.Zero, null, -1);
        }

#if RHINO_SDK
    internal override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr ptr = ConstPointer();
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.ON_PolylineCurve_Draw(ptr, pDisplayPipeline, argb, thickness);
    }
#endif

        /// <summary>
        /// Gets the number of points in this polyline.
        /// </summary>
        /// <since>5.0</since>
        public int PointCount
        {
            get
            {
                IntPtr ptr = ConstPointer();
                return UnsafeNativeMethods.ON_PolylineCurve_PointCount(ptr);
            }
        }

        /// <summary>
        /// Gets a point at a specified index in the polyline curve.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <returns>A point.</returns>
        /// <since>5.0</since>
        [ConstOperation]
        public Point3d Point(int index)
        {
            IntPtr ptr = ConstPointer();
            Point3d pt = new Point3d();
            UnsafeNativeMethods.ON_PolylineCurve_GetSetPoint(ptr, index, ref pt, false);
            return pt;
        }


        /// <summary>
        /// Sets a point at a specified index in the polyline curve.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <param name="point">A point location to set.</param>
        /// <since>5.0</since>
        public void SetPoint(int index, Point3d point)
        {
            IntPtr ptr = NonConstPointer();
            UnsafeNativeMethods.ON_PolylineCurve_GetSetPoint(ptr, index, ref point, true);
        }


        /// <summary>
        /// Gets a parameter at a specified index in the polyline curve.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <returns>A parameter.</returns>
        /// <since>6.0</since>
        [ConstOperation]
        public double Parameter(int index)
        {
            IntPtr ptr = ConstPointer();
            double t = 0.0;
            UnsafeNativeMethods.ON_PolylineCurve_GetSetParameter(ptr, index, ref t, false);
            return t;
        }


        /// <summary>
        /// Sets a parameter at a specified index in the polyline curve.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <param name="parameter">A parameter to set.</param>
        /// <since>6.0</since>
        public void SetParameter(int index, double parameter)
        {
            IntPtr ptr = NonConstPointer();
            UnsafeNativeMethods.ON_PolylineCurve_GetSetParameter(ptr, index, ref parameter, true);
        }
        #region VIRTUAL METHODS
        public override double GetLength()
        {
            return this.ToPolyline().Length;
        }

        /// <summary>
        /// Gets the parameter along the curve which coincides with a given length along the curve. 
        /// </summary>
        /// <param name="segmentLength">
        /// Length of segment to measure. Must be less than or equal to the length of the curve.
        /// </param>
        /// <param name="t">
        /// Parameter such that the length of the curve from the curve start point to t equals length.
        /// </param>
        /// <returns>true on success, false on failure.</returns>
        public override bool LengthParameter(double segmentLength, out double t)
        {
            t = 0.0;
            double length = GetLength();
            if (length == 0.0) return false;
            if (segmentLength > length)
            {
                t = 1.0;
                return false;
            }

            if (segmentLength < 0.0) return false;
            
            Polyline pline = this.ToPolyline();
            t = pline.ParameterAtLength(segmentLength);
            t = Interval.Map(t, new Interval(0, (double)pline.SegmentCount), Domain);
         //   t = (t / (double)pline.SegmentCount);
            return true;
            /*
            t = 0.0;
            bool rc = NormalizedLengthParameter(segmentLength, out t );
            if (!rc) return false;
            t *= Domain.T1;
            return true;
            */
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is length from the start of the curve. 
        /// </summary>
        /// <param name="segmentLength">
        /// Normalized arc length parameter. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="t">
        /// Parameter such that the length of the curve from its start to t is arc_length.
        /// </param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public override bool NormalizedLengthParameter(double segmentLength, out double t)
        {
            t = 0.0;
            bool rc = LengthParameter(segmentLength* GetLength() , out t);
            if (!rc) return false;
         //   t = Interval.Map(t, Domain, new Interval(0,1));
            return true;

            /*
            t = 0.0;
            double length = GetLength();
            if (segmentLength > 1.0 || segmentLength < 0.0) return false;
            if (length == 0.0) return false;

            Polyline pline = this.ToPolyline();
            t = pline.ParameterAtLength(segmentLength);
            t = (t / (double)pline.SegmentCount);
            return true;
            */
        }

        /// <summary>
        /// Compute the center point of the PolylineCurve as the weighted average of all segments.
        /// </summary>
        /// <returns>The weighted average of all segments.</returns>
        [ConstOperation]
        public override Point3d CenterPoint()
        {
            return this.ToPolyline().CenterPoint();
        }

        /// <summary>
        /// Compute area of the closed PolylineCurve
        /// </summary>
        /// <returns>PolylineCurve rea; only if closed, else area will be 0.0.</returns>
        public override double Area()
        {
            return this.ToPolyline().Area();
        }

        /// <summary>
        /// Looks for segments that are shorter than tolerance that can be removed. 
        /// Does not change the domain, but it will change the relative parameterization.
        /// </summary>
        /// <param name="tolerance">Tolerance which defines "short" segments.</param>
        /// <returns>
        /// true if removable short segments were found. 
        /// false if no removable short segments were found.
        /// </returns>
        /// <since>5.0</since>
        public override bool RemoveShortSegments(double tolerance)
        {
            Polyline pline = this.ToPolyline();
            int rc = pline.DeleteShortSegments(tolerance);
            if (rc > 0)
            {
                int count;
                Point3d[] ptArray = Pixel.Rhino.Collections.RhinoListHelpers.GetConstArray(pline, out count);
                IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New2(count, ptArray);
                ConstructNonConstObject(ptr);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the parameter of the point on a curve that is closest to testPoint.
        /// If the maximumDistance parameter is > 0, then only points whose distance
        /// to the given point is &lt;= maximumDistance will be returned.  Using a 
        /// positive value of maximumDistance can substantially speed up the search.
        /// </summary>
        /// <param name="testPoint">Point to project.</param>
        /// <param name="t">parameter of local closest point returned here.</param>
        /// <param name="maximumDistance">The maximum allowed distance.
        /// <para>Past this distance, the search is given up and false is returned.</para>
        /// <para>Use 0 to turn off this parameter.</para></param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public override bool ClosestPoint(Point3d testPoint, out double t, double maximumDistance)
        {
            Polyline pline = this.ToPolyline();
            Point3d pp = pline.ClosestPoint(testPoint, out t);
            t = Interval.Map(t, new Interval(0, pline.Count-1) , this.Domain);
            if (maximumDistance <= 0.0) return true;
            if (pp.DistanceTo(testPoint) > maximumDistance)
            {
                t = 0.0;
                return false;
            }
            return true;
        }

        #endregion
        /// <summary>
        /// Returns the underlying Polyline, or points.
        /// </summary>
        /// <returns>The Polyline if successful, null of the curve has no points.</returns>
        /// <since>6.0</since>
        [ConstOperation]
        public Polyline ToPolyline()
        {
            // http://mcneel.myjetbrains.com/youtrack/issue/RH-30969
            IntPtr const_ptr_this = ConstPointer();
            using (var output_points = new SimpleArrayPoint3d())
            {
                IntPtr output_points_ptr = output_points.NonConstPointer();
                UnsafeNativeMethods.ON_PolylineCurve_CopyValues(const_ptr_this, output_points_ptr);
                return Polyline.PolyLineFromNativeArray(output_points);
            }
        }
        public static explicit operator PolylineCurve(LineCurve crv) => new PolylineCurve(new Point3d[] { crv.Line.From, crv.Line.To });
        public static explicit operator LineCurve(PolylineCurve plineCrv) => new LineCurve(new Line(plineCrv.ToPolyline().First(), plineCrv.ToPolyline().Last()));

    }
}
