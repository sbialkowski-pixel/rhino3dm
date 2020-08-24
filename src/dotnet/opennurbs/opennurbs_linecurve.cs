using System;
using Rhino.Display;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
    /// <summary>
    /// Represents a linear curve.
    /// </summary>
    [Serializable]
    public class LineCurve : Curve
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineCurve"/> class.
        /// </summary>
        /// <since>5.0</since>
        public LineCurve()
        {
            IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New(IntPtr.Zero);    
            ConstructNonConstObject(ptr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineCurve"/> class, by
        /// copying values from another linear curve.
        /// </summary>
        /// <since>5.0</since>
        public LineCurve(LineCurve other)
        {
            IntPtr pOther = IntPtr.Zero;
            if (null != other)
                pOther = other.ConstPointer();
            IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New(pOther);
            ConstructNonConstObject(ptr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineCurve"/> class, by
        /// setting start and end point from two <see cref="Point2d">2D points</see>.</summary>
        /// <param name="from">A start point.</param>
        /// <param name="to">An end point.</param>
        /// <since>5.0</since>
        public LineCurve(Point2d from, Point2d to)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New2(from, to);
            ConstructNonConstObject(ptr);
        }
        /// <example>
        /// <code source='examples\vbnet\ex_addtruncatedcone.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_addtruncatedcone.cs' lang='cs'/>
        /// <code source='examples\py\ex_addtruncatedcone.py' lang='py'/>
        /// </example>
        /// <summary>
        /// Initializes a new instance of the <see cref="LineCurve"/> class, by
        /// setting start and end point from two <see cref="Point3d">3D points</see>.</summary>
        /// <param name="from">A start point.</param>
        /// <param name="to">An end point.</param>
        /// <since>5.0</since>
        public LineCurve(Point3d from, Point3d to)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New3(from, to);
            ConstructNonConstObject(ptr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineCurve"/> class, by
        /// retrieving its value from a <see cref="Line">line</see>.
        /// </summary>
        /// <param name="line">A line to use as model.</param>
        /// <since>5.0</since>
        public LineCurve(Line line)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New3(line.From, line.To);
            ConstructNonConstObject(ptr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineCurve"/> class, by
        /// retrieving its value from a <see cref="Line">line</see> and setting the domain.
        /// </summary>
        /// <param name="line">A line to use as model.</param>
        /// <param name="t0">The new domain start.</param>
        /// <param name="t1">The new domain end.</param>
        /// <since>5.0</since>
        public LineCurve(Line line, double t0, double t1)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New4(line.From, line.To, t0, t1);
            ConstructNonConstObject(ptr);
        }
        internal LineCurve(IntPtr ptr, object parent, int subobject_index)
          : base(ptr, parent, subobject_index)
        {
        }

        /// <summary>
        /// Protected constructor used in serialization.
        /// </summary>
        /// <param name="info">Serialization data.</param>
        /// <param name="context">Serialization stream.</param>
        protected LineCurve(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        internal override GeometryBase DuplicateShallowHelper()
        {
            return new LineCurve(IntPtr.Zero, null, -1);
        }

#if RHINO_SDK
    internal override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr ptr = ConstPointer();
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.ON_LineCurve_Draw(ptr, pDisplayPipeline, argb, thickness);
    }
#endif

        #region VIRTUAL METHODS
        /// <summary>
        /// Gets the length of the curve with a fractional tolerance of 1.0e-8.
        /// </summary>
        /// <returns>The length of the curve on success, or zero on failure.</returns>
        public override double GetLength()
        {
            return Line.Length;
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
            if (segmentLength > length) return false;
            if (length == 0.0) return false;
            t = length / segmentLength;
            return true;
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
        /// A fractional tolerance of 1e-8 is used in this version of the function.
        /// </summary>
        /// <param name="s">
        /// Normalized arc length parameter. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="t">
        /// Parameter such that the length of the curve from its start to t is arc_length.
        /// </param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public override bool NormalizedLengthParameter(double s, out double t)
        {
            t = 0.0;
            return LengthParameter(s, out t);
            //return NormalizedLengthParameter(s, out t, 1.0e-8);
        }

        /// <summary>
        /// Compute the center point of the LineCurve. ame as MidPoint
        /// </summary>
        /// <returns>The weighted average of all segments.</returns>
        public override Point3d CenterPoint()
        {
            return this.Line.PointAt(0.5);
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
            Point3d pp = this.Line.ClosestPoint(testPoint, true, out t);
            if(maximumDistance <= 0.0) return true;
            if (pp.DistanceTo(testPoint) > maximumDistance)
            {
                t = 0.0;
                return false;
            }
            return true;
        }

        //TODO: TO Finish
        /// <summary>
        /// TGets closest points between this and another curves.
        /// </summary>
        /// <param name="otherCurve">The other curve.</param>
        /// <param name="pointOnThisCurve">The point on this curve. This out parameter is assigned during this call.</param>
        /// <param name="pointOnOtherCurve">The point on other curve. This out parameter is assigned during this call.</param>
        /// <returns>true on success; false on error.</returns>
        /// <since>5.0</since>

        public override bool ClosestPoints(Curve otherCurve, out Point3d pointOnThisCurve, out Point3d pointOnOtherCurve)
        {


            pointOnThisCurve = Point3d.Unset;
            pointOnOtherCurve = Point3d.Unset;
            return false;
        }


        #endregion

        /// <summary>
        /// Gets or sets the Line value inside this curve.
        /// </summary>
        /// <since>5.0</since>
        public Line Line
        {
            get
            {
                IntPtr ptr = ConstPointer();
                Line line = new Line();
                UnsafeNativeMethods.ON_LineCurve_GetSetLine(ptr, false, ref line);
                return line;
            }
            set
            {
                IntPtr ptr = NonConstPointer();
                UnsafeNativeMethods.ON_LineCurve_GetSetLine(ptr, true, ref value);
            }
        }
        //public static implicit operator PolylineCurve(LineCurve crv) => new PolylineCurve(new Point3d[] { crv.Line.From, crv.Line.To });
        public static explicit operator PolylineCurve(LineCurve crv) => new PolylineCurve(new Point3d[] { crv.Line.From, crv.Line.To });


    }
}
