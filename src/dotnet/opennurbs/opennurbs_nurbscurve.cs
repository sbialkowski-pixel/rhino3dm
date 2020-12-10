using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Pixel.Rhino.Display;
using Pixel.Rhino.Collections;
using Pixel.Rhino.Runtime.InteropWrappers;
using Pixel.Rhino.Runtime;
using System.Linq;


namespace Pixel.Rhino.Geometry
{
    /// <summary>
    /// Represents a Non Uniform Rational B-Splines (NURBS) curve.
    /// </summary>
    [Serializable]
    public class NurbsCurve : Curve, IEpsilonComparable<NurbsCurve>
    {
        #region statics
        /// <summary>
        /// Gets a non-rational, degree 1 NURBS curve representation of the line.
        /// </summary>
        /// <returns>Curve on success, null on failure.</returns>
        /// <since>5.0</since>
        public static NurbsCurve CreateFromLine(Line line)
        {
            if (!line.IsValid) { return null; }

            NurbsCurve crv = new NurbsCurve(3, false, 2, 2);
            // Using implicit operator on Point3d
            crv.Points[0] = line.From;// new ControlPoint(line.From);
            crv.Points[1] = line.To;// new ControlPoint(line.To);

            crv.Knots.CreateUniformKnots(1.0);
            return crv;
        }

        /// <summary>
        /// Gets a rational degree 2 NURBS curve representation
        /// of the arc. Note that the parameterization of NURBS curve
        /// does not match arc's transcendental parameterization.
        /// </summary>
        /// <param name="arc"></param>
        /// <returns>Curve on success, null on failure.</returns>
        /// <since>5.0</since>
        public static NurbsCurve CreateFromArc(Arc arc)
        {
            IntPtr ptr_nurbs_curve = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
            int success = UnsafeNativeMethods.ON_Arc_GetNurbForm(ref arc, ptr_nurbs_curve);
            if (0 == success)
            {
                UnsafeNativeMethods.ON_Object_Delete(ptr_nurbs_curve);
                return null;
            }
            return CreateGeometryHelper(ptr_nurbs_curve, null) as NurbsCurve;
        }

#if RHINO_SDK

    /// <summary>
    /// Calculates the u, V, and N directions of a NURBS curve at a parameter similar to the method used by Rhino's MoveUVN command.
    /// </summary>
    /// <param name="t">The evaluation parameter.</param>
    /// <param name="uDir">The U direction.</param>
    /// <param name="vDir">The V direction.</param>
    /// <param name="nDir">The N direction.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <since>7.0</since>
    public bool UVNDirectionsAt(double t, out Vector3d uDir, out Vector3d vDir, out Vector3d nDir)
    {
      uDir = vDir = nDir = Vector3d.Unset;
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.RHC_RhinoNurbsCurveDirectionsAt(ptr_const_this, t, ref uDir, ref vDir, ref nDir);
    }

    /// <summary>
    /// For expert use only. From the input curves, make an array of compatible NURBS curves.
    /// </summary>
    /// <param name="curves">The input curves.</param>
    /// <param name="startPt">The start point. To omit, specify Point3d.Unset.</param>
    /// <param name="endPt">The end point. To omit, specify Point3d.Unset.</param>
    /// <param name="simplifyMethod">The simplify method.</param>
    /// <param name="numPoints">The number of rebuild points.</param>
    /// <param name="refitTolerance">The refit tolerance.</param>
    /// <param name="angleTolerance">The angle tolerance in radians.</param>
    /// <returns>The output NURBS surfaces if successful.</returns>
    /// <since>6.0</since>
    public static NurbsCurve[] MakeCompatible(IEnumerable<Curve> curves, Point3d startPt, Point3d endPt,
      int simplifyMethod, int numPoints, double refitTolerance, double angleTolerance)
    {
      using (var input = new SimpleArrayCurvePointer(curves))
      using (var output = new SimpleArrayCurvePointer())
      {
        IntPtr ptr_input = input.NonConstPointer();
        IntPtr ptr_output = output.NonConstPointer();
        bool rc = UnsafeNativeMethods.RHC_RhinoMakeCompatibleNurbs(ptr_input, startPt, endPt, simplifyMethod, numPoints,
          refitTolerance, angleTolerance, ptr_output);
        if (!rc)
          return new NurbsCurve[0];

        int count = UnsafeNativeMethods.ON_CurveArray_Count(ptr_output);
        if (count < 1)
          return new NurbsCurve[0];

        NurbsCurve[] result = new NurbsCurve[count];
        for (int i = 0; i < count; i++)
        {
          IntPtr ptr_curve = UnsafeNativeMethods.ON_CurveArray_Get(ptr_output, i);
          result[i] = CreateGeometryHelper(ptr_curve, null) as NurbsCurve;
        }
        GC.KeepAlive(curves);
        return result;
      }
    }

    /// <summary>
    /// Creates a parabola from vertex and end points.
    /// </summary>
    /// <param name="vertex">The vertex point.</param>
    /// <param name="startPoint">The start point.</param>
    /// <param name="endPoint">The end point</param>
    /// <returns>A 2 degree NURBS curve if successful, false otherwise.</returns>
    /// <since>6.0</since>
    public static NurbsCurve CreateParabolaFromVertex(Point3d vertex, Point3d startPoint, Point3d endPoint)
    {
      IntPtr ptr_nurbs_curve = UnsafeNativeMethods.RHC_RhinoCreateParabolaFromVertex(vertex, startPoint, endPoint);
      if (ptr_nurbs_curve == IntPtr.Zero)
        return null;
      return CreateGeometryHelper(ptr_nurbs_curve, null) as NurbsCurve;
    }

    /// <summary>
    /// Creates a parabola from focus and end points.
    /// </summary>
    /// <param name="focus">The focal point.</param>
    /// <param name="startPoint">The start point.</param>
    /// <param name="endPoint">The end point</param>
    /// <returns>A 2 degree NURBS curve if successful, false otherwise.</returns>
    /// <since>6.0</since>
    public static NurbsCurve CreateParabolaFromFocus(Point3d focus, Point3d startPoint, Point3d endPoint)
    {
      IntPtr ptr_nurbs_curve = UnsafeNativeMethods.RHC_RhinoCreateParabolaFromFocus(focus, startPoint, endPoint);
      if (ptr_nurbs_curve == IntPtr.Zero)
        return null;
      return CreateGeometryHelper(ptr_nurbs_curve, null) as NurbsCurve;
    }

    /// <summary>
    /// Create a uniform non-rational cubic NURBS approximation of an arc.
    /// </summary>
    /// <param name="arc"></param>
    /// <param name="degree">&gt;=1</param>
    /// <param name="cvCount">CV count &gt;=5</param>
    /// <returns>NURBS curve approximation of an arc on success</returns>
    /// <since>6.0</since>
    public static NurbsCurve CreateFromArc(Arc arc, int degree, int cvCount)
    {
      var ptr_nurbs_curve = UnsafeNativeMethods.TLC_DeformableArc(ref arc, degree, cvCount);
      return CreateGeometryHelper(ptr_nurbs_curve, null) as NurbsCurve;
    }

    /// <summary>
    /// Construct an H-spline from a sequence of interpolation points
    /// </summary>
    /// <param name="points">Points to interpolate</param>
    /// <returns></returns>
    /// <since>7.0</since>
    public static NurbsCurve CreateHSpline(IEnumerable<Point3d> points)
    {
      return CreateHSpline(points, Vector3d.Unset, Vector3d.Unset);
    }

    /// <summary>
    /// Construct an H-spline from a sequence of interpolation points and
    /// optional start and end derivative information
    /// </summary>
    /// <param name="points">Points to interpolate</param>
    /// <param name="startTangent">Unit tangent vector or Unset</param>
    /// <param name="endTangent">Unit tangent vector or Unset</param>
    /// <returns></returns>
    /// <since>7.0</since>
    public static NurbsCurve CreateHSpline(IEnumerable<Point3d> points, Vector3d startTangent, Vector3d endTangent)
    {
      var pts = new List<Point3d>(points);
      var pointsArray = pts.ToArray();
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoCreateHSpline(pointsArray, pointsArray.Length, startTangent, endTangent);
      if (rc == IntPtr.Zero)
        return null;
      return new NurbsCurve(rc, null, -1);
    }

    /// <summary>
    /// Computes planar rail sweep frames at specified parameters.
    /// </summary>
    /// <param name="parameters">A collection of curve parameters.</param>
    /// <param name="normal">Unit normal to the plane.</param>
    /// <returns>An array of planes if successful, or an empty array on failure.</returns>
    /// <since>7.0</since>
    public Plane[] CreatePlanarRailFrames(IEnumerable<double> parameters, Vector3d normal)
    {
      using (var in_params = new SimpleArrayDouble(parameters))
      using (var out_frames = new SimpleArrayPlane())
      {
        var ptr_this = ConstPointer();
        var ptr_in_params = in_params.ConstPointer();
        var ptr_out_frames = out_frames.NonConstPointer();
        var rc = UnsafeNativeMethods.TLC_GetPlanarRailFrames(ptr_this, ptr_in_params, normal, ptr_out_frames);
        if (rc)
          return out_frames.ToArray();
        return new Plane[0];
      }
    }

    /// <summary>
    /// Computes relatively parallel rail sweep frames at specified parameters.
    /// </summary>
    /// <param name="parameters">A collection of curve parameters.</param>
    /// <returns>An array of planes if successful, or an empty array on failure.</returns>
    /// <since>7.0</since>
    public Plane[] CreateRailFrames(IEnumerable<double> parameters)
    {
      using (var in_params = new SimpleArrayDouble(parameters))
      using (var out_frames = new SimpleArrayPlane())
      {
        var ptr_this = ConstPointer();
        var ptr_in_params = in_params.ConstPointer();
        var ptr_out_frames = out_frames.NonConstPointer();
        var rc = UnsafeNativeMethods.TLC_GetRailFrames(ptr_this, ptr_in_params, ptr_out_frames);
        if (rc)
          return out_frames.ToArray();
        return new Plane[0];
      }
    }

#endif

        /// <summary>
        /// Gets a rational degree 2 NURBS curve representation
        /// of the circle. Note that the parameterization of NURBS curve
        /// does not match circle's transcendental parameterization.  
        /// Use GetRadianFromNurbFormParameter() and
        /// GetParameterFromRadian() to convert between the NURBS curve 
        /// parameter and the transcendental parameter.
        /// </summary>
        /// <returns>Curve on success, null on failure.</returns>
        /// <since>5.0</since>
        public static NurbsCurve CreateFromCircle(Circle circle)
        {
            IntPtr ptr_nurbs_curve = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
            int success = UnsafeNativeMethods.ON_Circle_GetNurbForm(ref circle, ptr_nurbs_curve);
            if (0 == success)
            {
                UnsafeNativeMethods.ON_Object_Delete(ptr_nurbs_curve);
                return null;
            }
            return CreateGeometryHelper(ptr_nurbs_curve, null) as NurbsCurve;
        }

#if RHINO_SDK
    /// <summary>
    /// Create a uniform non-rational cubic NURBS approximation of a circle.
    /// </summary>
    /// <param name="circle"></param>
    /// <param name="degree">&gt;=1</param>
    /// <param name="cvCount">CV count &gt;=5</param>
    /// <returns>NURBS curve approximation of a circle on success</returns>
    /// <since>6.0</since>
    public static NurbsCurve CreateFromCircle(Circle circle, int degree, int cvCount)
    {
      var ptr_nurbs_curve = UnsafeNativeMethods.TLC_DeformableCircle(ref circle, degree, cvCount);
      return CreateGeometryHelper(ptr_nurbs_curve, null) as NurbsCurve;
    }
#endif

        /// <summary>
        /// Gets a rational degree 2 NURBS curve representation of the ellipse.
        /// <para>Note that the parameterization of the NURBS curve does not match
        /// with the transcendental parameterization of the ellipsis.</para>
        /// </summary>
        /// <returns>A NURBS curve representation of this ellipse or null if no such representation could be made.</returns>
        /// <since>5.0</since>
        public static NurbsCurve CreateFromEllipse(Ellipse ellipse)
        {
            NurbsCurve nc = CreateFromCircle(new Circle(ellipse.Plane, 1.0));
            if (nc == null) { return null; }

            Transform scale = Geometry.Transform.Scale(ellipse.Plane, ellipse.Radius1, ellipse.Radius2, 1.0);
            nc.Transform(scale);

            return nc;

            // Ellipses use Plane which is not castable to ON_Plane. Also, the OpenNurbs ON_Ellipse->ON_NurbsCurve logic fails on 
            // zero radii. Making a unit circle and scaling seems to be a good intermediate solution to this problem.


            //IntPtr pNC = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
            //Plane plane = ellipse.Plane;
            //int success = UnsafeNativeMethods.ON_Ellipse_GetNurbForm2(ref plane, ellipse.Radius1, ellipse.Radius2, pNC);
            //if (0 == success)
            //{
            //  UnsafeNativeMethods.ON_Object_Delete(pNC);
            //  return null;
            //}
            //return new NurbsCurve(pNC, null, null);
        }

        /// <summary>
        /// Determines if two curves are similar.
        /// </summary>
        /// <param name="curveA">First curve used in comparison.</param>
        /// <param name="curveB">Second curve used in comparison.</param>
        /// <param name="ignoreParameterization">if true, parameterization and orientation are ignored.</param>
        /// <param name="tolerance">tolerance to use when comparing control points.</param>
        /// <returns>true if curves are similar within tolerance.</returns>
        /// <since>5.0</since>
        public static bool IsDuplicate(NurbsCurve curveA, NurbsCurve curveB, bool ignoreParameterization, double tolerance)
        {
            IntPtr const_ptr_a = curveA.ConstPointer();
            IntPtr const_ptr_b = curveB.ConstPointer();
            if (const_ptr_a == const_ptr_b)
                return true;

            bool rc = UnsafeNativeMethods.ON_NurbsCurve_IsDuplicate(const_ptr_a, const_ptr_b, ignoreParameterization, tolerance);
            Runtime.CommonObject.GcProtect(curveA, curveB);
            return rc;
        }

        /// <summary>
        /// Constructs a 3D NURBS curve from a list of control points.
        /// </summary>
        /// <param name="periodic">If true, create a periodic uniform curve. If false, create a clamped uniform curve.</param>
        /// <param name="degree">(>=1) degree=order-1.</param>
        /// <param name="points">control vertex locations.</param>
        /// <returns>
        /// new NURBS curve on success
        /// null on error.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_addnurbscurve.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_addnurbscurve.cs' lang='cs'/>
        /// <code source='examples\py\ex_addnurbscurve.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public static NurbsCurve Create(bool periodic, int degree, System.Collections.Generic.IEnumerable<Point3d> points)
        {
            if (degree < 1)
                return null;

            const int dimension = 3;
            const double knot_delta = 1.0;
            int count;
            int order = degree + 1;
            Point3d[] point_array = RhinoListHelpers.GetConstArray(points, out count);
            if (null == point_array || count < 2)
                return null;

            NurbsCurve nc = new NurbsCurve();
            IntPtr ptr_curve = nc.NonConstPointer();

            bool rc = periodic ? UnsafeNativeMethods.ON_NurbsCurve_CreatePeriodicUniformNurbs(ptr_curve, dimension, order, count, point_array, knot_delta) :
                                 UnsafeNativeMethods.ON_NurbsCurve_CreateClampedUniformNurbs(ptr_curve, dimension, order, count, point_array, knot_delta);

            if (false == rc)
            {
                nc.Dispose();
                return null;
            }
            return nc;
        }

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a NURBS curve by copying its values from another NURBS curve.
        /// </summary>
        /// <param name="other">The other curve. This value can be null.</param>
        /// <since>5.0</since>
        public NurbsCurve(NurbsCurve other)
        {
            IntPtr const_ptr_other = IntPtr.Zero;
            if (other != null)
                const_ptr_other = other.ConstPointer();
            IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(const_ptr_other);
            GC.KeepAlive(other);
            ConstructNonConstObject(ptr);
        }
        internal NurbsCurve()
        {
            IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
            ConstructNonConstObject(ptr);
        }

        /// <summary>
        /// Protected constructor for internal use.
        /// </summary>
        /// <param name="info">Serialization data.</param>
        /// <param name="context">Serialization stream.</param>
        protected NurbsCurve(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        //[skipping]
        // public ON_NurbsCurve(ON_BezierCurve)

        /// <summary>
        /// Constructs a new NURBS curve with a specific degree and control point count.
        /// </summary>
        /// <param name="degree">Degree of curve. Must be equal to or larger than 1 and smaller than or equal to 11.</param>
        /// <param name="pointCount">Number of control-points.</param>
        /// <since>5.0</since>
        public NurbsCurve(int degree, int pointCount)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
            ConstructNonConstObject(ptr);
            Create(3, false, degree + 1, pointCount);
        }

        /// <summary>
        /// Constructs a new NURBS curve with knot and CV memory allocated.
        /// </summary>
        /// <param name="dimension">&gt;=1.</param>
        /// <param name="rational">true to make a rational NURBS.</param>
        /// <param name="order">(&gt;= 2) The order=degree+1.</param>
        /// <param name="pointCount">(&gt;= order) number of control vertices.</param>
        /// <example>
        /// <code source='examples\vbnet\ex_addnurbscircle.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_addnurbscircle.cs' lang='cs'/>
        /// <code source='examples\py\ex_addnurbscircle.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public NurbsCurve(int dimension, bool rational, int order, int pointCount)
        {
            IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
            ConstructNonConstObject(ptr);
            Create(dimension, rational, order, pointCount);
        }
        private bool Create(int dimension, bool isRational, int order, int cvCount)
        {
            // keeping internal for now
            IntPtr ptr = NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_Create(ptr, dimension, isRational, order, cvCount);
        }

        internal NurbsCurve(IntPtr ptr, object parent, int subobjectIndex)
          : base(ptr, parent, subobjectIndex)
        {
        }

        internal override GeometryBase DuplicateShallowHelper()
        {
            return new NurbsCurve(IntPtr.Zero, null, -1);
        }

        #endregion

        #region properties
        private Collections.NurbsCurveKnotList m_knots;
        private Collections.NurbsCurvePointList m_points;

        /// <summary>
        /// Gets the order of the curve. Order = Degree + 1.
        /// </summary>
        /// <since>5.0</since>
        public int Order
        {
            get { return GetInt(idxOrder); }
        }

        /// <summary>
        /// Gets a value indicating whether or not the curve is rational. 
        /// Rational curves have control-points with custom weights.
        /// </summary>
        /// <since>5.0</since>
        public bool IsRational
        {
            get
            {
                IntPtr ptr = ConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, idxIsRational);
            }
        }

        /// <summary>
        /// Gets access to the knots (or "knot vector") of this NURBS curve.
        /// </summary>
        /// <example>
        /// <code source='examples\vbnet\ex_addnurbscircle.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_addnurbscircle.cs' lang='cs'/>
        /// <code source='examples\py\ex_addnurbscircle.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public Collections.NurbsCurveKnotList Knots
        {
            get { return m_knots ?? (m_knots = new Pixel.Rhino.Geometry.Collections.NurbsCurveKnotList(this)); }
        }

        /// <summary>
        /// Gets access to the control points of this NURBS curve.
        /// </summary>
        /// <since>5.0</since>
        public Collections.NurbsCurvePointList Points
        {
            get { return m_points ?? (m_points = new Collections.NurbsCurvePointList(this)); }
        }
        #endregion

        #region constants
        // GetBool indices
        internal const int idxIsRational = 0;
        internal const int idxIsClampedStart = 1;
        internal const int idxIsClampedEnd = 2;
        internal const int idxZeroCVs = 3;
        internal const int idxClampStart = 4;
        internal const int idxClampEnd = 5;
        internal const int idxMakeRational = 6;
        internal const int idxMakeNonRational = 7;
        internal const int idxHasBezierSpans = 8;

        // GetInt indices
        internal const int idxCVSize = 0;
        internal const int idxOrder = 1;
        internal const int idxCVCount = 2;
        internal const int idxKnotCount = 3;
        internal const int idxCVStyle = 4;
        internal int GetInt(int which)
        {
            IntPtr ptr = ConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_GetInt(ptr, which);
        }
        #endregion

#if RHINO_SDK
    /// <summary> What end conditions to set </summary>
    public enum NurbsCurveEndConditionType
    {
      /// <summary>
      /// Set nothing
      /// </summary>
      Nothing = UnsafeNativeMethods.NurbsCurveEndConditionType.Nothing,
      /// <summary>
      /// Set endpoint
      /// </summary>
      Position = UnsafeNativeMethods.NurbsCurveEndConditionType.Position,
      /// <summary>
      /// Set tangent
      /// </summary>
      Tangency = UnsafeNativeMethods.NurbsCurveEndConditionType.Tangency,
      /// <summary>
      /// Set curvature
      /// </summary>
      Curvature = UnsafeNativeMethods.NurbsCurveEndConditionType.Curvature
    }

    /// <summary>
    /// Set end condition of a NURBS curve to point, tangent and curvature.
    /// </summary>
    /// <param name="bSetEnd">true: set end of curve, false: set start of curve </param>
    /// <param name="continuity">Position: set start or end point, Tangency: set point and tangent, Curvature: set point, tangent and curvature </param>
    /// <param name="point">point to set </param>
    /// <param name="tangent">tangent to set</param>
    /// <returns>true on success, false on failure.</returns>
    /// <since>6.0</since>
    public bool SetEndCondition(
      bool bSetEnd,
      NurbsCurveEndConditionType continuity,
      Point3d point,
      Vector3d tangent)
    {
      var curvature = Vector3d.Unset;
      return SetEndCondition(bSetEnd, continuity, point, tangent, curvature);
    }

    /// <summary>
    /// Set end condition of a NURBS curve to point, tangent and curvature.
    /// </summary>
    /// <param name="bSetEnd">true: set end of curve, false: set start of curve </param>
    /// <param name="continuity">Position: set start or end point, Tangency: set point and tangent, Curvature: set point, tangent and curvature </param>
    /// <param name="point">point to set </param>
    /// <param name="tangent">tangent to set</param>
    /// <param name="curvature">curvature to set</param>
    /// <returns>true on success, false on failure.</returns>
    /// <since>6.0</since>
    public bool SetEndCondition(
      bool bSetEnd,
      NurbsCurveEndConditionType continuity,
      Point3d point,
      Vector3d tangent,
      Vector3d curvature)
    {
      var pointer = NonConstPointer();
      int rc = UnsafeNativeMethods.RHC_SetNurbsCurveEndCondition(
        pointer,
        bSetEnd ? 1 : 0,
        (UnsafeNativeMethods.NurbsCurveEndConditionType)continuity,
        point,
        tangent,
        curvature
        );
      if (0 == rc)
        return false;
      else
        return true;
    }
#endif
        /// <summary>
        /// Increase the degree of this curve.
        /// </summary>
        /// <param name="desiredDegree">The desired degree. 
        /// Degrees should be number between and including 1 and 11.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_nurbscurveincreasedegree.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_nurbscurveincreasedegree.cs' lang='cs'/>
        /// <code source='examples\py\ex_nurbscurveincreasedegree.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public bool IncreaseDegree(int desiredDegree)
        {
            IntPtr ptr = NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_IncreaseDegree(ptr, desiredDegree);
        }

        /// <summary>
        /// Returns true if the NURBS curve has Bezier spans (all distinct knots have multiplicity = degree)
        /// </summary>
        /// <since>5.0</since>
        public bool HasBezierSpans
        {
            get
            {
                IntPtr const_ptr_this = ConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_GetBool(const_ptr_this, idxHasBezierSpans);
            }
        }

        /// <summary>
        /// Clamps ends and adds knots so the NURBS curve has Bezier spans 
        /// (all distinct knots have multiplicity = degree).
        /// </summary>
        /// <param name="setEndWeightsToOne">
        /// If true and the first or last weight is not one, then the first and
        /// last spans are re-parameterized so that the end weights are one.
        /// </param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool MakePiecewiseBezier(bool setEndWeightsToOne)
        {
            IntPtr ptr_this = NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_MakePiecewiseBezier(ptr_this, setEndWeightsToOne);
        }

        /// <summary>
        /// Use a linear fractional transformation to re-parameterize the NURBS curve.
        /// This does not change the curve's domain.
        /// </summary>
        /// <param name="c">
        /// re-parameterization constant (generally speaking, c should be > 0). The
        /// control points and knots are adjusted so that
        /// output_nurbs(t) = input_nurbs(lambda(t)), where lambda(t) = c*t/( (c-1)*t + 1 ).
        /// Note that lambda(0) = 0, lambda(1) = 1, lambda'(t) > 0, 
        /// lambda'(0) = c and lambda'(1) = 1/c.
        /// </param>
        /// <returns>true if successful.</returns>
        /// <remarks>
        /// The CV and knot values are values are changed so that output_nurbs(t) = input_nurbs(lambda(t)).
        /// </remarks>
        /// <since>5.0</since>
        public bool Reparameterize(double c)
        {
            IntPtr ptr = NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_Reparameterize(ptr, c);
        }

        #region greville point methods
        /// <summary>
        /// Gets the greville (edit point) parameter that belongs 
        /// to the control point at the specified index.
        /// </summary>
        /// <param name="index">Index of Greville (Edit) point.</param>
        /// <since>5.0</since>
        [ConstOperation]
        public double GrevilleParameter(int index)
        {
            IntPtr ptr = ConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_GrevilleAbcissa(ptr, index);
        }

        /// <summary>
        /// Gets the Greville parameter that belongs 
        /// to the control point at the specified index.
        /// </summary>
        /// <param name="index">Index of Greville point.</param>
        /// <since>5.0</since>
        [ConstOperation]
        public Point3d GrevillePoint(int index)
        {
            double t = GrevilleParameter(index);
            return PointAt(t);
        }

        /// <summary>
        /// Gets all Greville parameters for this curve.
        /// </summary>
        /// <since>5.0</since>
        [ConstOperation]
        public double[] GrevilleParameters()
        {
            int count = Points.Count;
            double[] rc = new double[count];
            IntPtr ptr = ConstPointer();
            bool success = UnsafeNativeMethods.ON_NurbsCurve_GetGrevilleAbcissae(ptr, rc);
            if (!success) { return null; }
            return rc;
        }

        /// <summary>
        /// Gets all Greville points for this curve.
        /// </summary>
        /// <since>5.0</since>
        [ConstOperation]
        public Point3dList GrevillePoints()
        {
            double[] gr_ab = GrevilleParameters();
            if (gr_ab == null) { return null; }

            Point3dList gr_pts = new Point3dList(gr_ab.Length);

            foreach (double t in gr_ab)
            {
                gr_pts.Add(PointAt(t));
            }

            return gr_pts;
        }
        #endregion

#if SB_IMP

        #region OVERRID METHODS

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
            double[] crvParams;
            t = 0.0;
            // Get curve approximation
            Polyline pline = TryGetPolyline(out crvParams);
            List<Tuple<double, int>> segments = new List<Tuple<double, int>>();
            // Get distance to all verticeis of pline
            for (int i = 0; i < pline.Count; i++)
            {
                double distance = pline[i].DistanceTo(testPoint);
                if (distance < maximumDistance || maximumDistance <= 0)
                {
                    segments.Add(Tuple.Create(distance, i));
                }
            }

            if (segments.Count == 0) return false;
            segments = segments.OrderBy(a => a.Item1).ToList();
            List<Tuple<double, double>> finalData = new List<Tuple<double, double>>();

            // Run detailed search for N closes segments and figure out which is closest.
            for (int i = 0; i < Math.Min(2, segments.Count); i++)
            {
                Interval trim = new Interval(0, 0);
                trim.T0 = crvParams[(int)Math.Max(0, (segments[i].Item2 - 1))];
                trim.T1 = crvParams[(int)Math.Min(crvParams.Length - 1, (segments[i].Item2 + 1))];
                Curve trimed = Trim(trim);
                double parameter = CloseSearch(trimed, testPoint);
                finalData.Add(Tuple.Create(trimed.PointAt(parameter).DistanceTo(testPoint), parameter));
            }

            double finalParam = finalData.OrderBy(b => b.Item1).First().Item2;
            t = finalParam;
            return true;

        }

        private static double CloseSearch(Curve crv, Point3d testPoint)
        {
            double tolerance = 1e-7;
            double step = crv.Domain.Length;
            double tolDistance = double.MaxValue;
            double parameter = crv.Domain.T0;
            int safeCounter = 0;
            while (tolDistance > tolerance)
            {
                Point3d p0 = crv.PointAt(parameter);
                double newParameter = parameter + step;

                if (!crv.Domain.IncludesParameter(newParameter))
                {
                    step = -step / 2;
                    newParameter = parameter + step;
                }

                Point3d p1 = crv.PointAt(newParameter);
                double distance0 = crv.PointAt(parameter).DistanceTo(testPoint);
                double distance1 = crv.PointAt(newParameter).DistanceTo(testPoint);
                if (distance1 < distance0)
                {
                    parameter = newParameter;
                    step = step / 2;
                }
                else
                {
                    step = -step / 2;
                }
                tolDistance = p0.DistanceTo(p1);
                safeCounter++;
                if (safeCounter > 50) break;
            }
            return parameter;
        }
        /*   LEGACY
        public override bool ClosestPoint(Point3d testPoint, out double t, double maximumDistance)
        {
            t = 0.0;
            int testSamples = 100;
            Tuple<double, Interval, Curve> LEVEL_1 = AproximateClosesPoint(this, testPoint, testSamples);
            Curve level_1 = LEVEL_1.Item3.Trim(LEVEL_1.Item2);

            if (level_1.PointAt(LEVEL_1.Item1).DistanceTo(testPoint) > maximumDistance) return false;

            Tuple<double, Interval, Curve> LEVEL_2 = AproximateClosesPoint(level_1, testPoint, testSamples);

            double normalizedT0Param1 = LEVEL_1.Item2.T0 / ((double)(testSamples - 1));
            double normalizedParam2 = LEVEL_2.Item1 / ((double)(testSamples - 1)) * LEVEL_2.Item2.Length; 
            double normalizedParam = normalizedT0Param1 + (normalizedParam2 / ((double)(testSamples - 1)));
            double realParam = this.Domain.ParameterAt(normalizedParam);
            t = realParam;
            if (maximumDistance <= 0.0) return true;
            return true;
        }

        public Tuple<double, Interval, Curve> AproximateClosesPoint(Curve crv, Point3d pt, int samples)
        {
            NurbsCurve reparamCurve = crv.ToNurbsCurve();
            reparamCurve.Domain = new Interval(0.0, samples - 1);

            List<double> testParams = Enumerable.Range(0, samples).Select(x => (double)x).ToList();
            List<Point3d> testPoints = testParams.Select(paramT => reparamCurve.PointAt(paramT)).ToList();
            Polyline pline = new Polyline(testPoints);
            double pLine_param_1 = pline.ClosestParameter(pt);
            Interval range = new Interval((double)Math.Max(reparamCurve.Domain.T0, (pLine_param_1 - 0.5)), (double)Math.Min(pLine_param_1 + 0.5, reparamCurve.Domain.T1));
            return Tuple.Create(pLine_param_1, range, (Curve)reparamCurve);
        }
        */

        /// <summary>
        /// Gets closest points between this and another curves (Proximity)
        /// revaluate to use Polyline.FastProximity 
        /// </summary>
        /// <param name="otherCurve">The other curve.</param>
        /// <param name="pointOnThisCurve">The point on this curve. This out parameter is assigned during this call.</param>
        /// <param name="pointOnOtherCurve">The point on other curve. This out parameter is assigned during this call.</param>
        /// <returns>true on success; false on error.</returns>
        /// <since>5.0</since>
        [ConstOperation]
        public override bool ClosestPoints(Curve otherCurve, out Point3d pointOnThisCurve, out Point3d pointOnOtherCurve)
        {
            double tolerance = 1e-7;

            double[] paramsOnA;
            Polyline pLineA = this.TryGetPolyline(out paramsOnA);
            double[] paramsOnB;
            Polyline pLineB = otherCurve.TryGetPolyline(out paramsOnB);
            Tuple<double, double, double> proxParameters;
            Tuple<Point3d, Point3d> proxPoints = pLineA.Proximity(pLineB, out proxParameters);

            double valA = Interval.Map(proxParameters.Item2, new Interval(0, pLineA.Count - 1), new Interval(paramsOnA.First(), paramsOnA.Last()));
            double valB = Interval.Map(proxParameters.Item3, new Interval(0, pLineB.Count - 1), new Interval(paramsOnB.First(), paramsOnB.Last()));
            int segIdA = (int)Math.Floor(proxParameters.Item2);
            int segIdB = (int)Math.Floor(proxParameters.Item3);
            Interval trimA = new Interval(0, 0);
            trimA.T0 = paramsOnA[(int)Math.Max(0, (segIdA - 1))];
            trimA.T1 = paramsOnA[(int)Math.Min(paramsOnA.Length - 1, (segIdA + 2))];

            Interval trimB = new Interval(0, 0);
            trimB.T0 = paramsOnB[(int)Math.Max(0, (segIdB - 1))];
            trimB.T1 = paramsOnB[(int)Math.Min(paramsOnB.Length - 1, (segIdB + 2))];
            Curve trimedA = this.Trim(trimA);
            Curve trimedB = otherCurve.Trim(trimB);
            if (trimedA != null && trimedB != null)
            {
                Tuple<Point3d, Point3d> result = Find(trimedA, trimedB, proxParameters.Item2, tolerance);
                pointOnThisCurve = result.Item1;
                pointOnOtherCurve = result.Item2;
                return true;
            }
            else
            {
                pointOnThisCurve = Point3d.Unset;
                pointOnOtherCurve = Point3d.Unset;
                return false;
            }

        }
        /*    LEGACY
        public override bool ClosestPoints(Curve otherCurve, out Point3d pointOnThisCurve, out Point3d pointOnOtherCurve)
        {
            Tuple<Interval, Interval> output = FindInterval(this, otherCurve, 100);
            double fixedParamA = (output.Item1.T0 + output.Item1.T1) / 2;
            Tuple<Point3d, Point3d> a = Find(this, otherCurve, fixedParamA, 50);
            pointOnThisCurve = a.Item1;
            pointOnOtherCurve = a.Item2;
            return true;
        }

        [ConstOperation]
        public Tuple<Interval, Interval> FindInterval(Curve curveA, Curve curveB, int div)
        {
            return FindInterval(curveA, curveB, div, curveA.Domain, curveB.Domain);
        }

        [ConstOperation]
        private static Tuple<Interval, Interval> FindInterval(Curve crvA, Curve crvB, int div, Interval subInterA, Interval subInterB)
        {
            Curve[] part_crvA = crvA.Split(new double[] { subInterA.T0, subInterA.T1 });
            Curve[] part_crvB = crvB.Split(new double[] { subInterB.T0, subInterB.T1 });

            foreach (Curve crv in part_crvA)
            {
                if (subInterA.IncludesParameter((crv.Domain.T1 + crv.Domain.T0) / 2)) crvA = crv;
            }

            foreach (Curve crv in part_crvB)
            {
                if (subInterB.IncludesParameter((crv.Domain.T1 + crv.Domain.T0) / 2)) crvB = crv;
            }

            double[] t_params_A = crvA.DivideByCount(div, true);
            SortedList<double, Tuple<Point3d, double, Point3d, double>> result = new SortedList<double, Tuple<Point3d, double, Point3d, double>>(div);

            foreach (double paramA in t_params_A)
            {
                double paramB;
                Point3d pointA = crvA.PointAt(paramA);
                crvB.ClosestPoint(pointA, out paramB);
                //t_params_B.Add(t_param_B);
                Point3d pointB = crvB.PointAt(paramB);
                double distance = pointA.DistanceTo(pointB);
                result.Add(distance, Tuple.Create(pointA, paramA, pointB, paramB));
            }
            return Tuple.Create(new Interval(result.Values[0].Item2, result.Values[1].Item2), new Interval(result.Values[0].Item4, result.Values[1].Item4));

        }
        */

        private static Tuple<Point3d, Point3d> Find(Curve curveA, Curve curveB, double seedParamA, double tolerance)
        {
            double ParamA = seedParamA;
            double ParamB = 0;
            for (int i = 0; i < 50; i++)
            {
                Point3d testPtA = curveA.PointAt(ParamA);
                double newParamB;
                curveB.ClosestPoint(testPtA, out newParamB);
                double paramB_diff = newParamB - ParamB;
                ParamB = newParamB;
                Point3d testPtB = curveB.PointAt(ParamB);
                double newParamA;
                curveA.ClosestPoint(testPtB, out newParamA);
                double paramA_diff = newParamA - ParamA;
                ParamA = newParamA;
                if (Math.Abs(paramA_diff) < tolerance || Math.Abs(paramB_diff) < tolerance) break;
            }
            return Tuple.Create(curveA.PointAt(ParamA), curveB.PointAt(ParamB));
        }



        #endregion

#endif

#if RHINO_SDK

    /// <summary>
    /// Gets Greville points for this curve.
    /// </summary>
    /// <param name="all">If true, then all Greville points are returns. If false, only edit points are returned.</param>
    /// <returns>A list of points if successful, null otherwise.</returns>
    /// <since>6.18</since>
    [ConstOperation]
    public Point3dList GrevillePoints(bool all)
    {
      if (all)
        return GrevillePoints();

      using (var out_points = new SimpleArrayPoint3d())
      {
        IntPtr ptr_out_points = out_points.NonConstPointer();
        IntPtr ptr_const_curve = ConstPointer();
        bool rc = UnsafeNativeMethods.TLC_GetEditNurbsCurvePoints(ptr_const_curve, ptr_out_points);
        if (rc)
          return new Point3dList(out_points.ToArray());
      }  
      return null;
    }

    /// <summary>
    /// Sets all Greville edit points for this curve.
    /// </summary>
    /// <param name="points">
    /// The new point locations. The number of points should match 
    /// the number of point returned by NurbsCurve.GrevillePoints(false).
    /// </param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <since>6.4</since>
    public bool SetGrevillePoints(IEnumerable<Point3d> points)
    {
      if (null == points)
        throw new ArgumentNullException(nameof(points));

      IntPtr ptr_this = NonConstPointer();

      int count;
      Point3d[] pt_array = RhinoListHelpers.GetConstArray(points, out count);

      return UnsafeNativeMethods.TLC_EditNurbsCurvePoints(ptr_this, count, pt_array);
    }
#endif

        /// <summary>
        /// Check that all values in other are within epsilon of the values in this
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        /// <since>5.4</since>
        [ConstOperation]
        public bool EpsilonEquals(NurbsCurve other, double epsilon)
        {
            if (null == other) throw new ArgumentNullException("other");

            if (ReferenceEquals(this, other))
                return true;

            if (IsRational != other.IsRational)
                return false;

            if (Degree != other.Degree)
                return false;

            if (Points.Count != other.Points.Count)
                return false;

            if (!Knots.EpsilonEquals(other.Knots, epsilon))
                return false;

            if (!Points.EpsilonEquals(other.Points, epsilon))
                return false;

            return true;
        }
        //[skipping]
        //   bool RepairBadKnots(
        //[skipping - slightly unusual for plugin devs]
        //public bool ChangeDimension(int desiredDimension)
        //{
        //  IntPtr ptr = NonConstPointer();
        //  return UnsafeNativeMethods.ON_NurbsCurve_ChangeDimension(ptr, desiredDimension);
        //}

        //[skipping - slightly unusual for plugin devs]
        //public bool Append(NurbsCurve curve)
        //{
        //  if (null == curve)
        //    return false;
        //  IntPtr ptr = NonConstPointer();
        //  IntPtr pCurve = curve.ConstPointer();
        //  return UnsafeNativeMethods.ON_NurbsCurve_Append(ptr, pCurve);
        //}

        //[skipping]
        //  bool ReserveCVCapacity(
        //  bool ReserveKnotCapacity(

        //[skipping]
        //  bool ConvertSpanToBezier(


#if RHINO_SDK
    /// <summary>
    /// Creates a C1 cubic NURBS approximation of a helix or spiral. For a helix,
    /// you may have radius0 == radius1. For a spiral radius0 == radius0 produces
    /// a circle. Zero and negative radii are permissible.
    /// </summary>
    /// <param name="axisStart">Helix's axis starting point or center of spiral.</param>
    /// <param name="axisDir">Helix's axis vector or normal to spiral's plane.</param>
    /// <param name="radiusPoint">
    /// Point used only to get a vector that is perpendicular to the axis. In
    /// particular, this vector must not be (anti)parallel to the axis vector.
    /// </param>
    /// <param name="pitch">
    /// The pitch, where a spiral has a pitch = 0, and pitch > 0 is the distance
    /// between the helix's "threads".
    /// </param>
    /// <param name="turnCount">The number of turns in spiral or helix. Positive
    /// values produce counter-clockwise orientation, negative values produce
    /// clockwise orientation. Note, for a helix, turnCount * pitch = length of
    /// the helix's axis.
    /// </param>
    /// <param name="radius0">The starting radius.</param>
    /// <param name="radius1">The ending radius.</param>
    /// <returns>NurbsCurve on success, null on failure.</returns>
    /// <since>5.2</since>
    public static NurbsCurve CreateSpiral(Point3d axisStart, Vector3d axisDir, Point3d radiusPoint,
      double pitch, double turnCount, double radius0, double radius1)
    {
      NurbsCurve curve = new NurbsCurve();
      IntPtr ptr_curve = curve.NonConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhinoCreateSpiral0(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1, ptr_curve);
      if (!rc)
      {
        curve.Dispose();
        return null;
      }
      return curve;
    }

    /// <summary>
    /// Create a C2 non-rational uniform cubic NURBS approximation of a swept helix or spiral.
    /// </summary>
    /// <param name="railCurve">The rail curve.</param>
    /// <param name="t0">Starting portion of rail curve's domain to sweep along.</param>
    /// <param name="t1">Ending portion of rail curve's domain to sweep along.</param>
    /// <param name="radiusPoint">
    /// Point used only to get a vector that is perpendicular to the axis. In
    /// particular, this vector must not be (anti)parallel to the axis vector.
    /// </param>
    /// <param name="pitch">
    /// The pitch. Positive values produce counter-clockwise orientation,
    /// negative values produce clockwise orientation.
    /// </param>
    /// <param name="turnCount">
    /// The turn count. If != 0, then the resulting helix will have this many
    /// turns. If = 0, then pitch must be != 0 and the approximate distance
    /// between turns will be set to pitch. Positive values produce counter-clockwise
    /// orientation, negative values produce clockwise orientation.
    /// </param>
    /// <param name="radius0">
    /// The starting radius. At least one radii must be nonzero. Negative values
    /// are allowed.
    /// </param>
    /// <param name="radius1">
    /// The ending radius. At least one radii must be nonzero. Negative values
    /// are allowed.
    /// </param>
    /// <param name="pointsPerTurn">
    /// Number of points to interpolate per turn. Must be greater than 4.
    /// When in doubt, use 12.
    /// </param>
    /// <returns>NurbsCurve on success, null on failure.</returns>
    /// <since>5.2</since>
    public static NurbsCurve CreateSpiral(Curve railCurve, double t0, double t1, Point3d radiusPoint, double pitch,
      double turnCount, double radius0, double radius1, int pointsPerTurn)
    {
      IntPtr const_ptr_rail = railCurve.ConstPointer();
      NurbsCurve curve = new NurbsCurve();
      IntPtr ptr_curve = curve.NonConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhinoCreateSpiral1(const_ptr_rail, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn, ptr_curve);
      if (!rc)
      {
        curve.Dispose();
        return null;
      }
      GC.KeepAlive(railCurve);
      return curve;
    }

    private IntPtr CurveDisplay()
    {
      if (IntPtr.Zero == m_pCurveDisplay)
      {
        IntPtr const_ptr_this = ConstPointer();
        m_pCurveDisplay = UnsafeNativeMethods.CurveDisplay_FromNurbsCurve(const_ptr_this);
      }
      return m_pCurveDisplay;
    }

    internal override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr ptr_display_pipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      IntPtr ptr_curve_display = CurveDisplay();
      UnsafeNativeMethods.CurveDisplay_Draw(ptr_curve_display, ptr_display_pipeline, argb, thickness);
    }
#endif
    }

    /// <summary>
    /// Represents control point geometry with three-dimensional position and weight.
    /// </summary>
    [Serializable]
    public struct ControlPoint : IEpsilonComparable<ControlPoint>, IEquatable<ControlPoint>
    {
        #region members
        internal Point4d m_vertex;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new world 3-D, or Euclidean, control point.
        /// The 4-D representation is (x, y, z, 1.0).
        /// </summary>
        /// <param name="x">X coordinate of the control point.</param>
        /// <param name="y">Y coordinate of the control point.</param>
        /// <param name="z">Z coordinate of the control point.</param>
        /// <since>5.0</since>
        public ControlPoint(double x, double y, double z)
        {
            m_vertex = new Point4d(x, y, z, 1.0);
        }

        /// <summary>
        /// Constructs a new homogeneous control point, where the 4-D representation is (x, y, z, w).
        /// The world 3-D, or Euclidean, representation is (x/w, y/w, z/w).
        /// </summary>
        /// <param name="x">X coordinate of the control point.</param>
        /// <param name="y">Y coordinate of the control point.</param>
        /// <param name="z">Z coordinate of the control point.</param>
        /// <param name="weight">Weight factor of the control point. You should not use weights less than or equal to zero.</param>
        /// <remarks>
        /// For expert use only. If you do not understand homogeneous coordinates, then
        /// use an override that accepts world 3-D, or Euclidean, coordinates as input.
        /// </remarks>
        /// <since>5.0</since>
        public ControlPoint(double x, double y, double z, double weight)
        {
            m_vertex = new Point4d(x, y, z, weight);
        }

        /// <summary>
        /// Constructs a new world 3-D, or Euclidean, control point.
        /// The 4-D representation of this is (x, y, z, 1.0).
        /// </summary>
        /// <param name="pt">Coordinates of the control point.</param>
        /// <since>5.0</since>
        public ControlPoint(Point3d pt)
        {
            m_vertex = new Point4d(pt.X, pt.Y, pt.Z, 1.0);
        }

        /// <summary>
        /// Constructs a control point from a world 3-D, or Euclidean, location and a weight.
        /// The world 3-D, or Euclidean, representation is (x/w, y/w, z/w).
        /// </summary>
        /// <param name="euclideanPt">Coordinates of the control point.</param>
        /// <param name="weight">Weight factor of the control point. You should not use weights less than or equal to zero.</param>
        /// <since>5.0</since>
        public ControlPoint(Point3d euclideanPt, double weight)
        {
            double w = (weight != 1.0 && weight != 0.0) ? weight : 1.0;
            m_vertex = new Point4d(euclideanPt.m_x * w, euclideanPt.m_y * w, euclideanPt.m_z * w, w);
        }

        /// <summary>
        /// Constructs a new homogeneous control point, where the 4-D representation is (x, y, z, w).
        /// The world 3-D, or Euclidean, representation is (x/w, y/w, z/w).
        /// </summary>
        /// <param name="pt">Coordinates of the control point.</param>
        /// <remarks>
        /// For expert use only. If you do not understand homogeneous coordinates, then
        /// use an override that accepts world 3-D, or Euclidean, coordinates as input.
        /// </remarks>
        /// <since>5.0</since>
        public ControlPoint(Point4d pt)
        {
            m_vertex = pt;
        }

        /// <summary>
        /// Gets the predefined, unset control point.
        /// </summary>
        /// <since>5.0</since>
        public static ControlPoint Unset
        {
            get
            {
                ControlPoint rc = new ControlPoint();
                Point3d unset = Point3d.Unset;
                rc.m_vertex.m_x = unset.m_x;
                rc.m_vertex.m_y = unset.m_y;
                rc.m_vertex.m_z = unset.m_z;
                rc.m_vertex.m_w = 1.0;
                return rc;
            }
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets or sets world 3-D, or Euclidean location of the control point. 
        /// </summary>
        /// <since>5.0</since>
        public Point3d Location
        {
            // https://mcneel.myjetbrains.com/youtrack/issue/RH-37341
            get
            {
                // Note, the constructor for Point3d will properly convert a
                // homogenous 4-D point to a Euclidean 3-D point.
                return new Point3d(m_vertex);
            }
            set
            {
                double w = (m_vertex.m_w != 1.0 && m_vertex.m_w != 0.0) ? m_vertex.m_w : 1.0;
                m_vertex.m_x = value.m_x * w;
                m_vertex.m_y = value.m_y * w;
                m_vertex.m_z = value.m_z * w;
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate of the control point.
        /// </summary>
        /// <since>6.0</since>
        public double X
        {
            get { return m_vertex.m_x; }
            set { m_vertex.m_x = value; }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the control point.
        /// </summary>
        /// <since>6.0</since>
        public double Y
        {
            get { return m_vertex.m_y; }
            set { m_vertex.m_y = value; }
        }

        /// <summary>
        /// Gets or sets the Z coordinate of the control point.
        /// </summary>
        /// <since>6.0</since>
        public double Z
        {
            get { return m_vertex.m_z; }
            set { m_vertex.m_z = value; }
        }

        /// <summary>
        /// Gets or sets the weight of this control point.
        /// </summary>
        /// <since>5.0</since>
        public double Weight
        {
            get { return m_vertex.m_w; }
            set { m_vertex.m_w = value; }
        }
        #endregion

        /// <summary>
        /// Check that all values in other are within epsilon of the values in this
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        /// <since>5.4</since>
        [ConstOperation]
        public bool EpsilonEquals(ControlPoint other, double epsilon)
        {
            return m_vertex.EpsilonEquals(other.m_vertex, epsilon);
        }

        /// <summary>
        /// Determines if two points exactly match.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>True if the other control point exactly matches this one.</returns>
        /// <since>6.0</since>
        [ConstOperation]
        public bool Equals(ControlPoint other)
        {
            return other.m_vertex == this.m_vertex;
        }
    }
}

namespace Pixel.Rhino.Geometry.Collections
{
    /// <summary>
    /// Provides access to the knot vector of a NURBS curve.
    /// </summary>
    public sealed class NurbsCurveKnotList :
      IEnumerable<double>, IRhinoTable<double>, IEpsilonComparable<NurbsCurveKnotList>,
      IList<double>
    {
        private readonly NurbsCurve m_curve;

        #region constructors
        internal NurbsCurveKnotList(NurbsCurve ownerCurve)
        {
            m_curve = ownerCurve;
        }
        #endregion

        #region properties

        /// <summary>Total number of knots in this curve.</summary>
        /// <since>5.0</since>
        public int Count
        {
            get
            {
                return m_curve.GetInt(NurbsCurve.idxKnotCount);
            }
        }

        /// <summary>
        /// Gets or sets the knot vector value at the given index.
        /// </summary>
        /// <param name="index">Index of knot to access.</param>
        /// <returns>The knot value at [index]</returns>
        public double this[int index]
        {
            get
            {
                if (index < 0) { throw new IndexOutOfRangeException("Index must be larger than or equal to zero."); }
                if (index >= Count) { throw new IndexOutOfRangeException("Index must be less than the number of knots."); }
                IntPtr ptr = m_curve.ConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_Knot(ptr, index);
            }
            set
            {
                if (index < 0) { throw new IndexOutOfRangeException("Index must be larger than or equal to zero."); }
                if (index >= Count) { throw new IndexOutOfRangeException("Index must be less than the number of knots."); }
                IntPtr ptr = m_curve.NonConstPointer();
                UnsafeNativeMethods.ON_NurbsCurve_SetKnot(ptr, index, value);
            }
        }
        #endregion

        #region knot utility methods
        /// <summary>
        /// If you want to keep a copy of this class around by holding onto it in a variable after a command
        /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
        /// call this function as many times as you want.
        /// </summary>
        /// <since>5.0</since>
        public void EnsurePrivateCopy()
        {
            m_curve.EnsurePrivateCopy();
        }

        /// <summary>
        /// Inserts a knot and update control point locations.
        /// Does not change parameterization or locus of curve.
        /// </summary>
        /// <param name="value">Knot value to insert.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_insertknot.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_insertknot.cs' lang='cs'/>
        /// <code source='examples\py\ex_insertknot.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public bool InsertKnot(double value)
        {
            return InsertKnot(value, 1);
        }

        /// <summary>
        /// Inserts a knot and update control point locations.
        /// Does not change parameterization or locus of curve.
        /// </summary>
        /// <param name="value">Knot value to insert.</param>
        /// <param name="multiplicity">Multiplicity of knot to insert.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool InsertKnot(double value, int multiplicity)
        {
            IntPtr ptr = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_InsertKnot(ptr, value, multiplicity);
        }

        /// <summary>Get knot multiplicity.</summary>
        /// <param name="index">Index of knot to query.</param>
        /// <returns>The multiplicity (valence) of the knot.</returns>
        /// <since>5.0</since>
        public int KnotMultiplicity(int index)
        {
            IntPtr ptr = m_curve.ConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_KnotMultiplicity(ptr, index);
        }

        /// <summary>
        /// Compute a clamped, uniform knot vector based on the current
        /// degree and control point count. Does not change values of control
        /// vertices.
        /// </summary>
        /// <param name="knotSpacing">Spacing of subsequent knots.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool CreateUniformKnots(double knotSpacing)
        {
            IntPtr ptr = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_MakeUniformKnotVector(ptr, knotSpacing, true);
        }

        /// <summary>
        /// Compute a clamped, uniform, periodic knot vector based on the current
        /// degree and control point count. Does not change values of control
        /// vertices.
        /// </summary>
        /// <param name="knotSpacing">Spacing of subsequent knots.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool CreatePeriodicKnots(double knotSpacing)
        {
            IntPtr ptr = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_MakeUniformKnotVector(ptr, knotSpacing, false);
        }

        /// <summary>
        /// Gets a value indicating whether or not the knot vector is clamped at the start of the curve. 
        /// Clamped curves start at the first control-point. This requires fully multiple knots.
        /// </summary>
        /// <since>5.0</since>
        public bool IsClampedStart
        {
            get
            {
                IntPtr ptr = m_curve.ConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxIsClampedStart);
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the knot vector is clamped at the end of the curve. 
        /// Clamped curves are coincident with the first and last control-point. This requires fully multiple knots.
        /// </summary>
        /// <since>5.0</since>
        public bool IsClampedEnd
        {
            get
            {
                IntPtr ptr = m_curve.ConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxIsClampedEnd);
            }
        }

        bool ICollection<double>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Clamp end knots. Does not modify control point locations.
        /// </summary>
        /// <param name="end">Curve end to clamp.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool ClampEnd(CurveEnd end)
        {
            IntPtr ptr = m_curve.NonConstPointer();
            bool rc = true;
            if (CurveEnd.Start == end || CurveEnd.Both == end)
                rc = UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxClampStart);
            if (CurveEnd.End == end || CurveEnd.Both == end)
                rc = rc && UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxClampEnd);
            return rc;
        }

        /// <summary>
        /// Computes the knots that are superfluous because they are not used in NURBs evaluation.
        /// These make it appear so that the first and last curve spans are different from interior spans.
        /// <para>http://wiki.mcneel.com/developer/onsuperfluousknot</para>
        /// </summary>
        /// <param name="start">true if the query targets the first knot. Otherwise, the last knot.</param>
        /// <returns>A component.</returns>
        /// <since>5.0</since>
        public double SuperfluousKnot(bool start)
        {
            IntPtr const_ptr_curve = m_curve.ConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_SuperfluousKnot(const_ptr_curve, start ? 0 : 1);
        }

#if RHINO_SDK

    /// <summary> Remove multiple knots from this curve. </summary>
    /// <param name="minimumMultiplicity">
    /// Remove knots with multiplicity > minimumKnotMultiplicity.
    /// </param>
    /// <param name="maximumMultiplicity">
    /// Remove knots with multiplicity &lt; maximumKnotMultiplicity.
    /// </param>
    /// <param name="tolerance">
    /// When you remove knots, the shape of the curve is changed.
    /// If tolerance is RhinoMath.UnsetValue, any amount of change is permitted.
    /// If tolerance is >=0, the maximum distance between the input and output
    /// curve is restricted to be &lt;= tolerance. 
    /// </param>
    /// <returns>number of knots removed on success. 0 if no knots were removed</returns>
    /// <since>6.0</since>
    public int RemoveMultipleKnots(int minimumMultiplicity, int maximumMultiplicity, double tolerance)
    {
      IntPtr ptr_curve = m_curve.NonConstPointer();
      return UnsafeNativeMethods.RHC_RemoveMultiKnotCrv(ptr_curve, minimumMultiplicity, maximumMultiplicity, tolerance);
    }

    /// <summary>
    /// Remove knots from a curve and adjusts the remaining control points to maintain curve position as closely as possible.
    /// The knots from Knots[index0] through Knots[index1 - 1] will be removed.
    /// </summary>
    /// <param name="index0">The starting knot index, where Degree-1 &lt; index0 &lt; index1 &lt;= Points.Count-1.</param>
    /// <param name="index1">The ending knot index, where Degree-1 &lt; index0 &lt; index1 &lt;= Points.Count-1.</param>
    /// <returns>true if successful, false on failure.</returns>
    /// <since>6.0</since>
    public bool RemoveKnots(int index0, int index1)
    {
      IntPtr ptr_curve = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_RemoveKnots(ptr_curve, index0, index1);
    }

    /// <summary>
    /// Remove a knot from a curve and adjusts the remaining control points to maintain curve position as closely as possible.
    /// </summary>
    /// <param name="t">The parameter on the curve that is closest to the knot to be removed.</param>
    /// <returns>true if successful, false on failure.</returns>
    /// <since>6.0</since>
    public bool RemoveKnotAt(double t)
    {
      // This is an "easy to use for scripters" function that emulates the RemoveKnot command.
      var rc = false;
      if (m_curve.Domain.IncludesParameter(t, false))
      {
        var point = m_curve.PointAt(t);
        var index = FindClosestKnot(point);
        if (index >= 0)
          rc = RemoveKnots(index, index + 1);
      }
      return rc;
    }

    internal int FindClosestKnot(Point3d point)
    {
      var min_index = -1;
      var min_dist = double.MaxValue;
      for (var i = 0; i < m_curve.Knots.Count; i++)
      {
        var knot_t = m_curve.Knots[i];
        if (m_curve.Domain.IncludesParameter(knot_t, false))
        {
          var knot_point = m_curve.PointAt(knot_t);
          var dist = knot_point.DistanceToSquared(point);
          if (dist < min_dist)
          {
            min_dist = dist;
            min_index = i;
          }
        }
      }
      return min_index;
    }
#endif

        #endregion

        #region IEnumerable<double> Members
        IEnumerator<double> IEnumerable<double>.GetEnumerator()
        {
            return new TableEnumerator<NurbsCurveKnotList, double>(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TableEnumerator<NurbsCurveKnotList, double>(this);
        }
        #endregion

        /// <summary>
        /// Checks that all values in the other list are sequentially equal within epsilon to the values in this list.
        /// </summary>
        /// <param name="other">The other list.</param>
        /// <param name="epsilon">The epsilon value.</param>
        /// <returns>True if values are, orderly, equal within epsilon. False otherwise.</returns>
        /// <since>5.4</since>
        public bool EpsilonEquals(NurbsCurveKnotList other, double epsilon)
        {
            if (null == other)
                throw new ArgumentNullException("other");

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count)
                return false;

            // check for span equality
            for (int i = 1; i < Count; ++i)
            {
                double my_delta = this[i] - this[i - 1];
                double their_delta = other[i] - other[i - 1];
                if (!RhinoMath.EpsilonEquals(my_delta, their_delta, epsilon))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the first item in the list.
        /// </summary>
        /// <param name="item">The value.</param>
        /// <returns>The index, or -1 if no index is found.</returns>
        /// <since>6.0</since>
        public int IndexOf(double item)
        {
            return GenericIListImplementation.IndexOf<double>(this, item);
        }

        /// <summary>
        /// Returns an indication of the presence of a value in the knot list.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if present, false otherwise.</returns>
        /// <since>6.0</since>
        public bool Contains(double item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary>
        /// Copies the list to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index into copy will begin.</param>
        /// <since>6.0</since>
        public void CopyTo(double[] array, int arrayIndex)
        {
            GenericIListImplementation.CopyTo<double>(this, array, arrayIndex);
        }

        void IList<double>.Insert(int index, double item)
        {
            throw new NotSupportedException("NurbsCurveKnotList.Insert is not supported.");
        }

        void IList<double>.RemoveAt(int index)
        {
            throw new NotSupportedException("NurbsCurveKnotList.RemoveAt is not supported.");
        }

        void ICollection<double>.Add(double item)
        {
            throw new NotSupportedException("NurbsCurveKnotList.Add is not supported.");
        }

        void ICollection<double>.Clear()
        {
            throw new NotSupportedException("NurbsCurveKnotList.Clear is not supported.");
        }

        bool ICollection<double>.Remove(double item)
        {
            throw new NotSupportedException("NurbsCurveKnotList.Remove is not supported.");
        }
    }

    /// <summary>
    /// Provides access to the control points of a NURBS curve.
    /// </summary>
    public class NurbsCurvePointList :
      IEnumerable<ControlPoint>, IRhinoTable<ControlPoint>, IEpsilonComparable<NurbsCurvePointList>,
      IList<ControlPoint>
    {
        private readonly NurbsCurve m_curve;

        #region constructors
        internal NurbsCurvePointList(NurbsCurve ownerCurve)
        {
            m_curve = ownerCurve;
        }
        #endregion

        /// <summary>
        /// If you want to keep a copy of this class around by holding onto it in a variable after a command
        /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
        /// call this function as many times as you want.
        /// </summary>
        /// <since>5.0</since>
        public void EnsurePrivateCopy()
        {
            m_curve.EnsurePrivateCopy();
        }

        #region properties
        /// <summary>
        /// Gets the number of control points in this curve.
        /// </summary>
        /// <since>5.0</since>
        public int Count
        {
            get
            {
                return m_curve.GetInt(NurbsCurve.idxCVCount);
            }
        }

        /// <summary>
        /// Gets or sets the control point location at the given index.
        /// </summary>
        /// <param name="index">Index of control vertex to access.</param>
        /// <returns>The control vertex at [index]</returns>
        public ControlPoint this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException("Index must be greater than or equal to zero.");
                if (index >= Count)
                    throw new IndexOutOfRangeException("Index must be less than the number of control points.");

                Point4d pt = new Point4d();
                IntPtr ptr = m_curve.ConstPointer();
                if (UnsafeNativeMethods.ON_NurbsCurve_GetCV4(ptr, index, ref pt))
                    return new ControlPoint(pt);

                return ControlPoint.Unset;
            }
            set
            {
                if (index < 0)
                    throw new IndexOutOfRangeException("Index must be greater than or equal to zero.");
                if (index >= Count)
                    throw new IndexOutOfRangeException("Index must be less than the number of control points.");
                IntPtr ptr = m_curve.NonConstPointer();

                Point4d pt = value.m_vertex;
                UnsafeNativeMethods.ON_NurbsCurve_SetCV4(ptr, index, ref pt);
            }
        }

        /// <summary>
        /// Gets the length of the polyline connecting all control points.
        /// </summary>
        /// <since>5.0</since>
        public double ControlPolygonLength
        {
            get
            {
                IntPtr ptr = m_curve.ConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_ControlPolygonLength(ptr);
            }
        }

        /// <summary>
        /// Constructs a polyline through all the control points. 
        /// Note that periodic curves generate a closed polyline with <i>fewer</i> 
        /// points than control-points.
        /// </summary>
        /// <returns>A polyline connecting all control points.</returns>
        /// <since>5.0</since>
        public Polyline ControlPolygon()
        {
            int count = Count;
            int i_max = count;
            if (m_curve.IsPeriodic) { i_max -= (m_curve.Degree - 1); }

            Polyline rc = new Polyline(count);
            for (int i = 0; i < i_max; i++)
            {
                rc.Add(this[i].Location);
            }

            return rc;
        }
        #endregion

        #region methods
        /// <summary>
        /// Use a combination of scaling and reparameterization to change the end weights to the specified values.
        /// </summary>
        /// <param name="w0">Weight for first control point.</param>
        /// <param name="w1">Weight for last control point.</param>
        /// <returns>true on success, false on failure.</returns>
        ///<remarks>
        /// The domain, Euclidean locations of the control points, and locus of the curve
        /// do not change, but the weights, homogeneous CV values and internal knot values
        /// may change. If w0 and w1 are 1 and the curve is not rational, the curve is not changed.
        ///</remarks>
        /// <since>5.0</since>
        public bool ChangeEndWeights(double w0, double w1)
        {
            IntPtr ptr = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_ChangeEndWeights(ptr, w0, w1);
        }

        /// <summary>
        /// Converts the curve to a Rational NURBS curve. Rational NURBS curves have weighted control points.
        /// </summary>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool MakeRational()
        {
            IntPtr ptr = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxMakeRational);
        }

        /// <summary>
        /// Converts the curve to a Non-rational NURBS curve. Non-rational curves have unweighted control points.
        /// </summary>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool MakeNonRational()
        {
            IntPtr ptr = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxMakeNonRational);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index must be greater than or equal to zero.");
            if (index >= Count)
                throw new IndexOutOfRangeException("Index must be less than the number of control points.");
        }

        /// <summary>
        /// Sets a world 3-D, or Euclidean, control point at the given index.
        /// The 4-D representation is (x, y, z, 1.0).
        /// </summary>
        /// <param name="index">Index of control point to set.</param>
        /// <param name="x">X coordinate of control point.</param>
        /// <param name="y">Y coordinate of control point.</param>
        /// <param name="z">Z coordinate of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>6.0</since>
        public bool SetPoint(int index, double x, double y, double z)
        {
            return SetPoint(index, new Point3d(x, y, z));
        }

        /// <summary>
        /// Sets a homogeneous control point at the given index, where the 4-D representation is (x, y, z, w).
        /// The world 3-D, or Euclidean, representation is (x/w, y/w, z/w).
        /// </summary>
        /// <param name="index">Index of control point to set.</param>
        /// <param name="x">X coordinate of control point.</param>
        /// <param name="y">Y coordinate of control point.</param>
        /// <param name="z">Z coordinate of control point.</param>
        /// <param name="weight">Weight of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <remarks>
        /// For expert use only. If you do not understand homogeneous coordinates, then
        /// use an override that accepts world 3-D, or Euclidean, coordinates as input.
        /// </remarks>
        /// <since>5.0</since>
        public bool SetPoint(int index, double x, double y, double z, double weight)
        {
            return SetPoint(index, new Point4d(x, y, z, weight));
        }

        /// <summary>
        /// Sets a world 3-D, or Euclidean, control point at the given index.
        /// The 4-D representation is (x, y, z, 1.0).
        /// </summary>
        /// <param name="index">Index of control point to set.</param>
        /// <param name="point">Coordinate of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public bool SetPoint(int index, Point3d point)
        {
            ValidateIndex(index);
            IntPtr ptr_curve = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_SetCV3(ptr_curve, index, ref point);
        }

        /// <summary>
        /// Sets a homogeneous control point at the given index, where the 4-D representation is (x, y, z, w).
        /// The world 3-D, or Euclidean, representation is (x/w, y/w, z/w).
        /// </summary>
        /// <param name="index">Index of control point to set.</param>
        /// <param name="point">Coordinate and weight of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <remarks>
        /// For expert use only. If you do not understand homogeneous coordinates, then
        /// use an override that accepts world 3-D, or Euclidean, coordinates as input.
        /// </remarks>
        /// <since>5.0</since>
        public bool SetPoint(int index, Point4d point)
        {
            ValidateIndex(index);
            IntPtr ptr_curve = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_SetCV4(ptr_curve, index, ref point);
        }

        /// <summary>
        /// Sets a world 3-D, or Euclidean, control point and weight at a given index.
        /// The 4-D representation is (x*w, y*w, z*w, w).
        /// </summary>
        /// <param name="index">Index of control point to set.</param>
        /// <param name="point">Coordinates of the control point.</param>
        /// <param name="weight">Weight of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>6.0</since>
        public bool SetPoint(int index, Point3d point, double weight)
        {
            double w = weight != 1.0 && weight != 0.0 ? weight : 1.0;
            return SetPoint(index, new Point4d(point.m_x * w, point.m_y * w, point.m_z * w, w));
        }

        /// <summary>
        /// Gets a world 3-D, or Euclidean, control point at the given index.
        /// The 4-D representation is (x, y, z, 1.0).
        /// </summary>
        /// <param name="index">Index of control point to get.</param>
        /// <param name="point">Coordinate of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>6.0</since>
        public bool GetPoint(int index, out Point3d point)
        {
            point = new Point3d();
            ValidateIndex(index);
            IntPtr ptr_curve = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_GetCV3(ptr_curve, index, ref point);
        }

        /// <summary>
        /// Gets a homogeneous control point at the given index, where the 4-D representation is (x, y, z, w).
        /// The world 3-D, or Euclidean, representation is (x/w, y/w, z/w).
        /// </summary>
        /// <param name="index">Index of control point to get.</param>
        /// <param name="point">Coordinate and weight of control point.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <remarks>
        /// For expert use only. If you do not understand homogeneous coordinates, then
        /// use the override that returns world 3-D, or Euclidean, coordinates.
        /// </remarks>
        /// <since>6.0</since>
        public bool GetPoint(int index, out Point4d point)
        {
            point = new Point4d();
            ValidateIndex(index);
            IntPtr ptr_curve = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_GetCV4(ptr_curve, index, ref point);
        }

        /// <summary>
        /// Sets the weight of a control point at the given index
        /// Note, if the curve is non-rational, it will be converted to rational.
        /// </summary>
        /// <param name="index">Index of control point to set.</param>
        /// <param name="weight">The control point weight.</param>
        /// <returns>true if successful, false otherwise.</returns>
        /// <since>6.0</since>
        public bool SetWeight(int index, double weight)
        {
            ValidateIndex(index);
            IntPtr ptr_curve = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_SetWeight(ptr_curve, index, weight);
        }

        /// <summary>
        /// Gets the weight of a control point at the given index.
        /// Note, if the curve is non-rational, the weight will be 1.0.
        /// </summary>
        /// <param name="index">Index of control point to get.</param>
        /// <returns>The control point weight if successful, Pixel.Rhino.Math.UnsetValue otherwise.</returns>
        /// <since>6.0</since>
        public double GetWeight(int index)
        {
            ValidateIndex(index);
            IntPtr ptr_curve = m_curve.NonConstPointer();
            return UnsafeNativeMethods.ON_NurbsCurve_Weight(ptr_curve, index);
        }

        /// <summary>
        /// Returns the control point size, or the number of doubles per control point. 
        /// For rational curves, PointSize = Curve.Dimension + 1. 
        /// For non-rational curves, PointSize = Curve.Dimension.
        /// </summary>
        /// <since>6.9</since>
        public int PointSize
        {
            get
            {
                IntPtr ptr_curve = m_curve.NonConstPointer();
                return UnsafeNativeMethods.ON_NurbsCurve_CVSize(ptr_curve);
            }
        }

#if RHINO_SDK
    /// <summary>
    /// Simple check of distance between adjacent control points
    /// </summary>
    /// <param name="closeTolerance">tolerance to use for determining if control points are 'close'</param>
    /// <param name="stackTolerance">tolerance to use for determining if control points are 'stacked'</param>
    /// <param name="closeIndices">indices of 'close' points are returned in this array</param>
    /// <param name="stackedIndices">indices of 'stacked' points are returned in this array</param>
    /// <returns>true if close or stacked indices are found</returns>
    /// <since>6.0</since>
    public bool ValidateSpacing(double closeTolerance, double stackTolerance, out int[] closeIndices, out int[] stackedIndices)
    {
      bool rc;
      using (var close_indices = new Runtime.InteropWrappers.SimpleArrayInt())
      using (var stack_indices = new Runtime.InteropWrappers.SimpleArrayInt())
      {
        IntPtr const_ptr_curve = m_curve.ConstPointer();
        IntPtr ptr_close = close_indices.NonConstPointer();
        IntPtr ptr_stack = stack_indices.NonConstPointer();
        rc = UnsafeNativeMethods.ONC_ValidateCurveCVSpacing(const_ptr_curve, closeTolerance, stackTolerance, ptr_close, ptr_stack);
        closeIndices = close_indices.ToArray();
        stackedIndices = stack_indices.ToArray();
      }
      return rc;
    }

    /// <summary>
    /// Calculates the U, V, and N directions of a NURBS curve control point similar to the method used by Rhino's MoveUVN command.
    /// </summary>
    /// <param name="index">Index of control point.</param>
    /// <param name="uDir">The U direction.</param>
    /// <param name="vDir">The V direction.</param>
    /// <param name="nDir">The N direction.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <since>7.0</since>
    public bool UVNDirectionsAt(int index, out Vector3d uDir, out Vector3d vDir, out Vector3d nDir)
    {
      uDir = vDir = nDir = Vector3d.Unset;
      bool rc = false;
      if (index >= 0 && index < Count)
      {
        double t = m_curve.GrevilleParameter(index);
        if (RhinoMath.IsValidDouble(t))
        {
          // For periodic curves, t may be less than Domain.Min, 
          // and in that case the evaluated tangent and normal are completely bogus.
          // Need to shuffle t back into region where the evaluation is kosher.
          if (t < m_curve.Domain.Min)
            t += m_curve.Domain.Length;

          rc = m_curve.UVNDirectionsAt(t, out uDir, out vDir, out nDir);
        }
      }
      return rc;
    }

#endif

        #endregion

        #region IEnumerable<ControlPoint> Members
        IEnumerator<ControlPoint> IEnumerable<ControlPoint>.GetEnumerator()
        {
            return new TableEnumerator<NurbsCurvePointList, ControlPoint>(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TableEnumerator<NurbsCurvePointList, ControlPoint>(this);
        }
        #endregion

        /// <summary>
        /// Check that all values in other are within epsilon of the values in this
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        /// <since>5.4</since>
        public bool EpsilonEquals(NurbsCurvePointList other, double epsilon)
        {
            if (null == other) throw new ArgumentNullException("other");

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count)
                return false;

            if (!RhinoMath.EpsilonEquals(ControlPolygonLength, other.ControlPolygonLength, epsilon))
                return false;

            for (int i = 0; i < Count; ++i)
            {
                ControlPoint mine = this[i];
                ControlPoint theirs = other[i];
                if (!mine.EpsilonEquals(theirs, epsilon))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        bool ICollection<ControlPoint>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the index of a control point, or -1.
        /// </summary>
        /// <param name="item">The exact item to search for.</param>
        /// <returns>The index.</returns>
        /// <since>6.0</since>
        public int IndexOf(ControlPoint item)
        {
            return GenericIListImplementation.IndexOf<ControlPoint>(this, item);
        }

        void IList<ControlPoint>.Insert(int index, ControlPoint item)
        {
            throw new NotSupportedException("NurbsCurvePointList.Insert is not supported.");
        }

        void IList<ControlPoint>.RemoveAt(int index)
        {
            throw new NotSupportedException("NurbsCurvePointList.RemoveAt is not supported.");
        }

        void ICollection<ControlPoint>.Add(ControlPoint item)
        {
            throw new NotSupportedException("NurbsCurvePointList.Add is not supported.");
        }

        void ICollection<ControlPoint>.Clear()
        {
            throw new NotSupportedException("NurbsCurvePointList.Clear is not supported.");
        }

        /// <summary>
        /// Determines if this list contains an item.
        /// </summary>
        /// <param name="item">The exact item to search for.</param>
        /// <returns>A boolean value.</returns>
        /// <since>6.0</since>
        public bool Contains(ControlPoint item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary>
        /// Copied the list to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index in which the copy will begin.</param>
        /// <since>6.0</since>
        public void CopyTo(ControlPoint[] array, int arrayIndex)
        {
            GenericIListImplementation.CopyTo<ControlPoint>(this, array, arrayIndex);
        }

        bool ICollection<ControlPoint>.Remove(ControlPoint item)
        {
            throw new NotSupportedException("NurbsCurvePointList.Remove is not supported.");
        }
    }
}

//public:
//  // NOTE: These members are left "public" so that expert users may efficiently
//  //       create NURBS curves using the default constructor and borrow the
//  //       knot and CV arrays from their native NURBS representation.
//  //       No technical support will be provided for users who access these
//  //       members directly.  If you can't get your stuff to work, then use
//  //       the constructor with the arguments and the SetKnot() and SetCV()
//  //       functions to fill in the arrays.

//  int     m_dim;            // (>=1)

//  int     m_is_rat;         // 1 for rational B-splines. (Rational control
//                            // vertices use homogeneous form.)
//                            // 0 for non-rational B-splines. (Control
//                            // vertices do not have a weight coordinate.)

//  int     m_order;          // order = degree+1 (>=2)

//  int     m_cv_count;       // number of control vertices ( >= order )

//  // knot vector memory

//  int     m_knot_capacity;  // If m_knot_capacity > 0, then m_knot[]
//                            // is an array of at least m_knot_capacity
//                            // doubles whose memory is managed by the
//                            // ON_NurbsCurve class using rhmalloc(),
//                            // onrealloc(), and rhfree().
//                            // If m_knot_capacity is 0 and m_knot is
//                            // not NULL, then  m_knot[] is assumed to
//                            // be big enough for any requested operation
//                            // and m_knot[] is not deleted by the
//                            // destructor.

//  double* m_knot;           // Knot vector. ( The knot vector has length
//                            // m_order+m_cv_count-2. )

//  // control vertex net memory

//  int     m_cv_stride;      // The pointer to start of "CV[i]" is
//                            //   m_cv + i*m_cv_stride.

//  int     m_cv_capacity;    // If m_cv_capacity > 0, then m_cv[] is an array
//                            // of at least m_cv_capacity doubles whose
//                            // memory is managed by the ON_NurbsCurve
//                            // class using rhmalloc(), onrealloc(), and rhfree().
//                            // If m_cv_capacity is 0 and m_cv is not
//                            // NULL, then m_cv[] is assumed to be big enough
//                            // for any requested operation and m_cv[] is not
//                            // deleted by the destructor.

//  double* m_cv;             // Control points.
//                            // If m_is_rat is FALSE, then control point is
//                            //
//                            //          ( CV(i)[0], ..., CV(i)[m_dim-1] ).
//                            //
//                            // If m_is_rat is TRUE, then the control point
//                            // is stored in HOMOGENEOUS form and is
//                            //
//                            //           [ CV(i)[0], ..., CV(i)[m_dim] ].
//                            //
