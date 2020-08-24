using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Rhino.Runtime.InteropWrappers;
using System.Linq;
using Rhino.FileIO;
using System.Threading.Tasks;


namespace Rhino.Geometry.Intersect
{
    /// <summary>
    /// Provides static methods for the computation of intersections, projections, sections and similar.
    /// </summary>
    public static class Intersection
    {
        #region analytic
        /// <summary>
        /// Intersects two lines.
        /// </summary>
        /// <param name="lineA">First line for intersection.</param>
        /// <param name="lineB">Second line for intersection.</param>
        /// <param name="a">
        /// Parameter on lineA that is closest to LineB. 
        /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
        /// </param>
        /// <param name="b">
        /// Parameter on lineB that is closest to LineA. 
        /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
        /// </param>
        /// <param name="tolerance">
        /// If tolerance > 0.0, then an intersection is reported only if the distance between the points is &lt;= tolerance. 
        /// If tolerance &lt;= 0.0, then the closest point between the lines is reported.
        /// </param>
        /// <param name="finiteSegments">
        /// If true, the input lines are treated as finite segments. 
        /// If false, the input lines are treated as infinite lines.
        /// </param>
        /// <returns>
        /// true if a closest point can be calculated and the result passes the tolerance parameter test; otherwise false.
        /// </returns>
        /// <remarks>
        /// If the lines are exactly parallel, meaning the system of equations used to find a and b 
        /// has no numerical solution, then false is returned. If the lines are nearly parallel, which 
        /// is often numerically true even if you think the lines look exactly parallel, then the 
        /// closest points are found and true is returned. So, if you care about weeding out "parallel" 
        /// lines, then you need to do something like the following:
        /// <code lang="cs">
        /// bool rc = Intersect.LineLine(lineA, lineB, out a, out b, tolerance, segments);
        /// if (rc)
        /// {
        ///   double angle_tol = RhinoMath.ToRadians(1.0); // or whatever
        ///   double parallel_tol = Math.Cos(angle_tol);
        ///   if ( Math.Abs(lineA.UnitTangent * lineB.UnitTangent) >= parallel_tol )
        ///   {
        ///     ... do whatever you think is appropriate
        ///   }
        /// }
        /// </code>
        /// <code lang="vb">
        /// Dim rc As Boolean = Intersect.LineLine(lineA, lineB, a, b, tolerance, segments)
        /// If (rc) Then
        ///   Dim angle_tol As Double = RhinoMath.ToRadians(1.0) 'or whatever
        ///   Dim parallel_tolerance As Double = Math.Cos(angle_tol)
        ///   If (Math.Abs(lineA.UnitTangent * lineB.UnitTangent) >= parallel_tolerance) Then
        ///     ... do whatever you think is appropriate
        ///   End If
        /// End If
        /// </code>
        /// </remarks>
        /// <since>5.0</since>
        public static bool LineLine(Line lineA, Line lineB, out double a, out double b, double tolerance, bool finiteSegments)
        {
            bool rc = LineLine(lineA, lineB, out a, out b);
            if (rc)
            {
                if (finiteSegments)
                {
                    if (a < 0.0)
                        a = 0.0;
                    else if (a > 1.0)
                        a = 1.0;
                    if (b < 0.0)
                        b = 0.0;
                    else if (b > 1.0)
                        b = 1.0;
                }
                if (tolerance > 0.0)
                {
                    rc = (lineA.PointAt(a).DistanceTo(lineB.PointAt(b)) <= tolerance);
                }
            }
            return rc;
        }
        /// <summary>
        /// Finds the closest point between two infinite lines.
        /// </summary>
        /// <param name="lineA">First line.</param>
        /// <param name="lineB">Second line.</param>
        /// <param name="a">
        /// Parameter on lineA that is closest to lineB. 
        /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
        /// </param>
        /// <param name="b">
        /// Parameter on lineB that is closest to lineA. 
        /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
        /// </param>
        /// <returns>
        /// true if points are found and false if the lines are numerically parallel. 
        /// Numerically parallel means the 2x2 matrix:
        /// <para>+AoA  -AoB</para>
        /// <para>-AoB  +BoB</para>
        /// is numerically singular, where A = (lineA.To - lineA.From) and B = (lineB.To-lineB.From)
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_intersectlines.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_intersectlines.cs' lang='cs'/>
        /// <code source='examples\py\ex_intersectlines.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public static bool LineLine(Line lineA, Line lineB, out double a, out double b)
        {
            a = 0; b = 0;
            return UnsafeNativeMethods.ON_Intersect_LineLine(ref lineA, ref lineB, ref a, ref b);
        }
        /// <summary>
        /// Intersects a line and a plane. This function only returns true if the 
        /// intersection result is a single point (i.e. if the line is coincident with 
        /// the plane then no intersection is assumed).
        /// </summary>
        /// <param name="line">Line for intersection.</param>
        /// <param name="plane">Plane to intersect.</param>
        /// <param name="lineParameter">Parameter on line where intersection occurs. 
        /// If the parameter is not within the {0, 1} Interval then the finite segment 
        /// does not intersect the plane.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public static bool LinePlane(Line line, Plane plane, out double lineParameter)
        {
            lineParameter = 0.0;
            return UnsafeNativeMethods.ON_Intersect_LinePlane(ref line, ref plane, ref lineParameter);
        }
        /// <summary>
        /// Intersects two planes and return the intersection line. If the planes are 
        /// parallel or coincident, no intersection is assumed.
        /// </summary>
        /// <param name="planeA">First plane for intersection.</param>
        /// <param name="planeB">Second plane for intersection.</param>
        /// <param name="intersectionLine">If this function returns true, 
        /// the intersectionLine parameter will return the line where the planes intersect.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <since>5.0</since>
        public static bool PlanePlane(Plane planeA, Plane planeB, out Line intersectionLine)
        {
            intersectionLine = new Line();
            return UnsafeNativeMethods.ON_Intersect_PlanePlane(ref planeA, ref planeB, ref intersectionLine);
        }
        /// <summary>
        /// Intersects three planes to find the single point they all share.
        /// </summary>
        /// <param name="planeA">First plane for intersection.</param>
        /// <param name="planeB">Second plane for intersection.</param>
        /// <param name="planeC">Third plane for intersection.</param>
        /// <param name="intersectionPoint">Point where all three planes converge.</param>
        /// <returns>true on success, false on failure. If at least two out of the three planes 
        /// are parallel or coincident, failure is assumed.</returns>
        /// <since>5.0</since>
        public static bool PlanePlanePlane(Plane planeA, Plane planeB, Plane planeC, out Point3d intersectionPoint)
        {
            intersectionPoint = new Point3d();
            return UnsafeNativeMethods.ON_Intersect_PlanePlanePlane(ref planeA, ref planeB, ref planeC, ref intersectionPoint);
        }

        /// <summary>
        /// Intersects a plane with a circle using exact calculations.
        /// </summary>
        /// <param name="plane">Plane to intersect.</param>
        /// <param name="circle">Circe to intersect.</param>
        /// <param name="firstCircleParameter">First intersection parameter on circle if successful or RhinoMath.UnsetValue if not.</param>
        /// <param name="secondCircleParameter">Second intersection parameter on circle if successful or RhinoMath.UnsetValue if not.</param>
        /// <returns>The type of intersection that occurred.</returns>
        /// <since>5.0</since>
        public static PlaneCircleIntersection PlaneCircle(Plane plane, Circle circle, out double firstCircleParameter, out double secondCircleParameter)
        {
            firstCircleParameter = RhinoMath.UnsetValue;
            secondCircleParameter = RhinoMath.UnsetValue;

            if (plane.ZAxis.IsParallelTo(circle.Plane.ZAxis, RhinoMath.ZeroTolerance * Math.PI) != 0)
            {
                if (Math.Abs(plane.DistanceTo(circle.Center)) < RhinoMath.ZeroTolerance)
                    return PlaneCircleIntersection.Coincident;
                return PlaneCircleIntersection.Parallel;
            }

            Line L;

            //At this point, the PlanePlane should never fail since I already checked for parallellillity.
            if (!PlanePlane(plane, circle.Plane, out L)) { return PlaneCircleIntersection.Parallel; }

            double Lt = L.ClosestParameter(circle.Center);
            Point3d Lp = L.PointAt(Lt);

            double d = circle.Center.DistanceTo(Lp);

            //If circle radius equals the projection distance, we have a tangent intersection.
            if (Math.Abs(d - circle.Radius) < RhinoMath.ZeroTolerance)
            {
                circle.ClosestParameter(Lp, out firstCircleParameter);
                secondCircleParameter = firstCircleParameter;
                return PlaneCircleIntersection.Tangent;
            }

            //If circle radius too small to get an intersection, then abort.
            if (d > circle.Radius) { return PlaneCircleIntersection.None; }

            double offset = Math.Sqrt((circle.Radius * circle.Radius) - (d * d));
            Vector3d dir = offset * L.UnitTangent;

            if (!circle.ClosestParameter(Lp + dir, out firstCircleParameter)) { return PlaneCircleIntersection.None; }
            if (!circle.ClosestParameter(Lp - dir, out secondCircleParameter)) { return PlaneCircleIntersection.None; }

            return PlaneCircleIntersection.Secant;
        }
        /// <summary>
        /// Intersects a plane with a sphere using exact calculations.
        /// </summary>
        /// <param name="plane">Plane to intersect.</param>
        /// <param name="sphere">Sphere to intersect.</param>
        /// <param name="intersectionCircle">Intersection result.</param>
        /// <returns>If <see cref="PlaneSphereIntersection.None"/> is returned, the intersectionCircle has a radius of zero and the center point 
        /// is the point on the plane closest to the sphere.</returns>
        /// <since>5.0</since>
        public static PlaneSphereIntersection PlaneSphere(Plane plane, Sphere sphere, out Circle intersectionCircle)
        {
            intersectionCircle = new Circle();
            int rc = UnsafeNativeMethods.ON_Intersect_PlaneSphere(ref plane, ref sphere, ref intersectionCircle);

            return (PlaneSphereIntersection)rc;
        }
        /// <summary>
        /// Intersects a line with a circle using exact calculations.
        /// </summary>
        /// <param name="line">Line for intersection.</param>
        /// <param name="circle">Circle for intersection.</param>
        /// <param name="t1">Parameter on line for first intersection.</param>
        /// <param name="point1">Point on circle closest to first intersection.</param>
        /// <param name="t2">Parameter on line for second intersection.</param>
        /// <param name="point2">Point on circle closest to second intersection.</param>
        /// <returns>
        /// If <see cref="LineCircleIntersection.Single"/> is returned, only t1 and point1 will have valid values. 
        /// If <see cref="LineCircleIntersection.Multiple"/> is returned, t2 and point2 will also be filled out.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_intersectlinecircle.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_intersectlinecircle.cs' lang='cs'/>
        /// <code source='examples\py\ex_intersectlinecircle.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public static LineCircleIntersection LineCircle(Line line, Circle circle, out double t1, out Point3d point1, out double t2, out Point3d point2)
        {
            t1 = 0.0;
            t2 = 0.0;
            point1 = new Point3d();
            point2 = new Point3d();

            if (!line.IsValid || !circle.IsValid) { return LineCircleIntersection.None; }

            int rc = UnsafeNativeMethods.ON_Intersect_LineCircle(ref line, ref circle, ref t1, ref point1, ref t2, ref point2);
            return (LineCircleIntersection)rc;
        }

        /// <summary>
        /// Intersects a line with a circle using exact calculations.
        /// </summary>
        /// <param name="line">Line for intersection.</param>
        /// <param name="circle">Circle for intersection.</param>
        /// <param name="t1">Parameter on line for first intersection.</param>
        /// <param name="point1">Point on circle closest to first intersection.</param>
        /// <param name="t2">Parameter on line for second intersection.</param>
        /// <param name="point2">Point on circle closest to second intersection.</param>
        /// <returns>
        /// If <see cref="LineCircleIntersection.Single"/> is returned, only t1 and point1 will have valid values. 
        /// If <see cref="LineCircleIntersection.Multiple"/> is returned, t2 and point2 will also be filled out.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_intersectlinecircle.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_intersectlinecircle.cs' lang='cs'/>
        /// <code source='examples\py\ex_intersectlinecircle.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public static LineCircleIntersection LineCircle(Line line, Circle circle, out double t1, out Point3d point1, out double t2, out Point3d point2, bool finiteSegments)
        {
            t1 = 0.0;
            t2 = 0.0;
            point1 = new Point3d();
            point2 = new Point3d();

            if (!line.IsValid || !circle.IsValid) { return LineCircleIntersection.None; }

            int rc = UnsafeNativeMethods.ON_Intersect_LineCircle(ref line, ref circle, ref t1, ref point1, ref t2, ref point2);

            if (rc > 0 && finiteSegments)
            {
                if (t1 < 0.0)
                {
                    t1 = 0.0; point1 = new Point3d();
                }
                else if (t1 > 1.0)
                {
                    t1 = 1.0; point1 = new Point3d();
                }

                if (t2 < 0.0)
                {
                    t2 = 0.0; point2 = new Point3d();
                }
                else if (t2 > 1.0)
                {
                    t2 = 1.0; point2 = new Point3d();
                }
            }
            return (LineCircleIntersection)rc;
        }


        /// <summary>
        /// Intersects a line with a sphere using exact calculations.
        /// </summary>
        /// <param name="line">Line for intersection.</param>
        /// <param name="sphere">Sphere for intersection.</param>
        /// <param name="intersectionPoint1">First intersection point.</param>
        /// <param name="intersectionPoint2">Second intersection point.</param>
        /// <returns>If <see cref="LineSphereIntersection.None"/> is returned, the first point is the point on the line closest to the sphere and 
        /// the second point is the point on the sphere closest to the line. 
        /// If <see cref="LineSphereIntersection.Single"/> is returned, the first point is the point on the line and the second point is the 
        /// same point on the sphere.</returns>
        /// <since>5.0</since>
        public static LineSphereIntersection LineSphere(Line line, Sphere sphere, out Point3d intersectionPoint1, out Point3d intersectionPoint2)
        {
            intersectionPoint1 = new Point3d();
            intersectionPoint2 = new Point3d();
            int rc = UnsafeNativeMethods.ON_Intersect_LineSphere(ref line, ref sphere, ref intersectionPoint1, ref intersectionPoint2);

            return (LineSphereIntersection)rc;
        }
        /// <summary>
        /// Intersects a line with a cylinder using exact calculations.
        /// </summary>
        /// <param name="line">Line for intersection.</param>
        /// <param name="cylinder">Cylinder for intersection.</param>
        /// <param name="intersectionPoint1">First intersection point.</param>
        /// <param name="intersectionPoint2">Second intersection point.</param>
        /// <returns>If None is returned, the first point is the point on the line closest
        /// to the cylinder and the second point is the point on the cylinder closest to
        /// the line. 
        /// <para>If <see cref="LineCylinderIntersection.Single"/> is returned, the first point
        /// is the point on the line and the second point is the  same point on the
        /// cylinder.</para></returns>
        /// <since>5.0</since>
        public static LineCylinderIntersection LineCylinder(Line line, Cylinder cylinder, out Point3d intersectionPoint1, out Point3d intersectionPoint2)
        {
            intersectionPoint1 = new Point3d();
            intersectionPoint2 = new Point3d();
            int rc = UnsafeNativeMethods.ON_Intersect_LineCylinder(ref line, ref cylinder, ref intersectionPoint1, ref intersectionPoint2);

            return (LineCylinderIntersection)rc;
        }
        /// <summary>
        /// Intersects two spheres using exact calculations.
        /// </summary>
        /// <param name="sphereA">First sphere to intersect.</param>
        /// <param name="sphereB">Second sphere to intersect.</param>
        /// <param name="intersectionCircle">
        /// If intersection is a point, then that point will be the center, radius 0.
        /// </param>
        /// <returns>
        /// The intersection type.
        /// </returns>
        /// <since>5.0</since>
        public static SphereSphereIntersection SphereSphere(Sphere sphereA, Sphere sphereB, out Circle intersectionCircle)
        {
            intersectionCircle = new Circle();
            int rc = UnsafeNativeMethods.ON_Intersect_SphereSphere(ref sphereA, ref sphereB, ref intersectionCircle);

            if (rc <= 0 || rc > 3)
            {
                return SphereSphereIntersection.None;
            }

            return (SphereSphereIntersection)rc;
        }
        /// <summary>
        /// Intersects an infinite line and an axis aligned bounding box.
        /// </summary>
        /// <param name="box">BoundingBox to intersect.</param>
        /// <param name="line">Line for intersection.</param>
        /// <param name="tolerance">
        /// If tolerance &gt; 0.0, then the intersection is performed against a box 
        /// that has each side moved out by tolerance.
        /// </param>
        /// <param name="lineParameters">
        /// The chord from line.PointAt(lineParameters.T0) to line.PointAt(lineParameters.T1) is the intersection.
        /// </param>
        /// <returns>true if the line intersects the box, false if no intersection occurs.</returns>
        /// <since>5.0</since>
        public static bool LineBox(Line line, BoundingBox box, double tolerance, out Interval lineParameters)
        {
            lineParameters = new Interval();
            return UnsafeNativeMethods.ON_Intersect_BoundingBoxLine(ref box, ref line, tolerance, ref lineParameters);
        }
        /// <summary>
        /// Intersects an infinite line with a box volume.
        /// </summary>
        /// <param name="box">Box to intersect.</param>
        /// <param name="line">Line for intersection.</param>
        /// <param name="tolerance">
        /// If tolerance &gt; 0.0, then the intersection is performed against a box 
        /// that has each side moved out by tolerance.
        /// </param>
        /// <param name="lineParameters">
        /// The chord from line.PointAt(lineParameters.T0) to line.PointAt(lineParameters.T1) is the intersection.
        /// </param>
        /// <returns>true if the line intersects the box, false if no intersection occurs.</returns>
        /// <since>5.0</since>
        public static bool LineBox(Line line, Box box, double tolerance, out Interval lineParameters)
        {
            //David: test this!
            BoundingBox bbox = new BoundingBox(new Point3d(box.X.Min, box.Y.Min, box.Z.Min),
                                               new Point3d(box.X.Max, box.Y.Max, box.Z.Max));
            Transform xform = Transform.ChangeBasis(Plane.WorldXY, box.Plane);
            line.Transform(xform);

            return LineBox(line, bbox, tolerance, out lineParameters);
        }
        #endregion

        #region sections
#if RHINO_SDK
    /// <summary>
    /// Intersects a curve with an (infinite) plane.
    /// </summary>
    /// <param name="curve">Curve to intersect.</param>
    /// <param name="plane">Plane to intersect with.</param>
    /// <param name="tolerance">Tolerance to use during intersection.</param>
    /// <returns>A list of intersection events or null if no intersections were recorded.</returns>
    /// <since>5.0</since>
    public static CurveIntersections CurvePlane(Curve curve, Plane plane, double tolerance)
    {
      if (!plane.IsValid)
        return null;

      // Use dedicated plane intersector in Rhino5
      IntPtr pConstCurve = curve.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Curve_IntersectPlane(pConstCurve, ref plane, tolerance);
      GC.KeepAlive(curve);
      return CurveIntersections.Create(pIntersectArray);
    }

    /// <summary>
    /// Intersects a mesh with an (infinite) plane.
    /// </summary>
    /// <param name="mesh">Mesh to intersect.</param>
    /// <param name="plane">Plane to intersect with.</param>
    /// <returns>An array of polylines describing the intersection loops or null (Nothing in Visual Basic) if no intersections could be found.</returns>
    /// <since>5.0</since>
    public static Polyline[] MeshPlane(Mesh mesh, Plane plane)
    {
      Rhino.Collections.RhinoList<Plane> planes = new Rhino.Collections.RhinoList<Plane>(1, plane);
      return MeshPlane(mesh, planes);
    }
    /// <summary>
    /// Intersects a mesh with a collection of (infinite) planes.
    /// </summary>
    /// <param name="mesh">Mesh to intersect.</param>
    /// <param name="planes">Planes to intersect with.</param>
    /// <returns>An array of polylines describing the intersection loops or null (Nothing in Visual Basic) if no intersections could be found.</returns>
    /// <exception cref="ArgumentNullException">If planes is null.</exception>
    /// <since>5.0</since>
    public static Polyline[] MeshPlane(Mesh mesh, IEnumerable<Plane> planes)
    {
      if (planes == null) throw new ArgumentNullException("planes");

      Rhino.Collections.RhinoList<Plane> list = planes as Rhino.Collections.RhinoList<Plane> ??
                                                new Rhino.Collections.RhinoList<Plane>(planes);
      if (list.Count < 1)
        return null;

      IntPtr pMesh = mesh.ConstPointer();
      int polylines_created = 0;
      IntPtr pPolys = UnsafeNativeMethods.TL_Intersect_MeshPlanes1(pMesh, list.Count, list.m_items, ref polylines_created);
      GC.KeepAlive(mesh);
      if (polylines_created < 1 || IntPtr.Zero == pPolys)
        return null;

      // convert the C++ polylines created into .NET polylines
      Polyline[] rc = new Polyline[polylines_created];
      for (int i = 0; i < polylines_created; i++)
      {
        int point_count = UnsafeNativeMethods.ON_Intersect_MeshPlanes2(pPolys, i);
        Polyline pl = new Polyline(point_count);
        if (point_count > 0)
        {
          pl.m_size = point_count;
          UnsafeNativeMethods.ON_Intersect_MeshPlanes3(pPolys, i, point_count, pl.m_items);
        }
        rc[i] = pl;
      }
      UnsafeNativeMethods.ON_Intersect_MeshPlanes4(pPolys);

      return rc;
    }

    /// <summary>
    /// Intersects a Brep with an (infinite) plane.
    /// </summary>
    /// <param name="brep">Brep to intersect.</param>
    /// <param name="plane">Plane to intersect with.</param>
    /// <param name="tolerance">Tolerance to use for intersections.</param>
    /// <param name="intersectionCurves">The intersection curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <since>5.0</since>
    public static bool BrepPlane(Brep brep, Plane plane, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionPoints = null;
      intersectionCurves = null;

      //David sez: replace this logic with the dedicated Geometry/Plane intersector methods in Rhino5.
      if (!plane.IsValid)
        return false;

      PlaneSurface section = ExtendThroughBox(plane, brep.GetBoundingBox(false), 1.0); //should this be 1.0 or 100.0*tolerance?
      bool rc = false;
      if (section != null)
      {
        rc = BrepSurface(brep, section, tolerance, out intersectionCurves, out intersectionPoints);
        section.Dispose();
      }
      return rc;
    }
#endif

        /// <summary>
        /// Utility function for creating a PlaneSurface through a Box.
        /// </summary>
        /// <param name="plane">Plane to extend.</param>
        /// <param name="box">Box to extend through.</param>
        /// <param name="fuzzyness">Box will be inflated by this amount.</param>
        /// <returns>A Plane surface through the box or null.</returns>
        internal static PlaneSurface ExtendThroughBox(Plane plane, BoundingBox box, double fuzzyness)
        {
            if (fuzzyness != 0.0) { box.Inflate(fuzzyness); }

            Point3d[] corners = box.GetCorners();
            int side = 0;
            bool valid = false;

            for (int i = 0; i < corners.Length; i++)
            {
                double d = plane.DistanceTo(corners[i]);
                if (d == 0.0) { continue; }

                if (d < 0.0)
                {
                    if (side > 0) { valid = true; break; }
                    side = -1;
                }
                else
                {
                    if (side < 0) { valid = true; break; }
                    side = +1;
                }
            }

            if (!valid) { return null; }

            Interval s, t;
            if (!plane.ExtendThroughBox(box, out s, out t)) { return null; }

            if (s.IsSingleton || t.IsSingleton)
                return null;

            return new PlaneSurface(plane, s, t);
        }
        #endregion

        #region geometric

        /// <summary>
        /// Finds the places where a Polyline intersects itself. 
        /// </summary>
        /// <param name="curve">Curve for self-intersections.</param>
        /// <param name="tolerance">Intersection tolerance. If the curve approaches itself to within tolerance, 
        /// an intersection is assumed.</param>
        /// <returns>A collection of Tuples where Item1 and Item2 are parameters of intersection</returns>
        /// <since>5.0</since>
        public static List<IntersectionEvent> PolylineSelf(Polyline pLine, double tolerance)
        {
            int roudInt = 12;
            BlockingCollection<IntersectionEvent> result = new BlockingCollection<IntersectionEvent>();

            Parallel.For(0, pLine.SegmentCount, i =>
            {
                Line LineA1 = pLine.SegmentAt(i);
                for (int j = i + 2; j < pLine.SegmentCount; j++)
                {
                    Line LineA2 = pLine.SegmentAt(j);
                    BoundingBox inter = BoundingBox.Intersection(LineA1.BoundingBox, LineA2.BoundingBox);
                    if (inter.IsValid)
                    {
                        double a;
                        double b;
                        bool rc = Intersection.LineLine(LineA1, LineA2, out a, out b, tolerance, true);
                        if (rc)
                        {
                            double paramA = Math.Round(a + i, roudInt);
                            double paramB = Math.Round(b + j, roudInt);
                            result.Add(new IntersectionEvent(pLine.PointAt(paramA), paramA, pLine.PointAt(paramB), paramB));
                        }
                    }
                }
            });
            result.CompleteAdding();
            return result.ToHashSet().ToList();
        }

        /// <summary>
        /// Intersect two PolyLines
        /// </summary>
        /// <param name="A">Polyline A</param>
        /// <param name="B">Polyline B</param>
        /// <param name="tolerance">
        /// If tolerance > 0.0, then an intersection is reported only if the distance between the points is &lt;= tolerance. 
        /// If tolerance &lt;= 0.0, then the closest point between the lines is reported.
        /// </param>
        /// <returns>List of Tuples where Item1 = Parameter on A, Item2 = Parameter on B</returns>
        public static List<IntersectionEvent> PolyLinePolyLine(Polyline A, Polyline B, double tolerance)
        {
            int roudInt = 12;
            BlockingCollection<IntersectionEvent> result = new BlockingCollection<IntersectionEvent>();

            Parallel.For(0, A.SegmentCount, i =>
            {
                Line LineA = A.SegmentAt(i);
                for (int j = 0; j < B.SegmentCount; j++)
                {
                    Line LineB = B.SegmentAt(j);

                    BoundingBox inter = BoundingBox.Intersection(LineA.BoundingBox, LineB.BoundingBox);
                    if (inter.IsValid)
                    {
                        double a;
                        double b;
                        bool rc = Intersection.LineLine(LineA, LineB, out a, out b, tolerance, true);
                        if (rc)
                        {
                            double paramA = Math.Round(a + i, roudInt);
                            double paramB = Math.Round(b + j, roudInt);
                            result.Add(new IntersectionEvent(A.PointAt(paramA), paramA, B.PointAt(paramB), paramB));
                        }
                    }
                }
            });
            result.CompleteAdding();
            return result.ToHashSet().ToList();
        }


        /// <summary>
        /// Finds the places where a curve intersects itself. 
        /// </summary>
        /// <param name="curve">Curve for self-intersections.</param>
        /// <param name="tolerance">Intersection tolerance. If the curve approaches itself to within tolerance, 
        /// an intersection is assumed.</param>
        /// <returns>A collection of intersection events.</returns>
        /// <since>5.0</since>
        public static List<IntersectionEvent> CurveSelf(Curve curve, double tolerance)
        {
            // 1e-7 -> trie show thah below this tolerance line from Trim is return  as null. soooo...
            tolerance = Math.Max(1e-7, tolerance);

            double[] paramsOnA;
            Polyline pLineA = curve.TryGetPolyline(out paramsOnA);

            List<IntersectionEvent> inter = PolylineSelf(pLineA, tolerance);
            List<IntersectionEvent> realInter = new List<IntersectionEvent>(inter.Count);
            for (int i = 0; i < inter.Count; i++)
            {
                IntersectionEvent localInter = inter[i];

                double valA1 = Interval.Map(localInter.ParameterA, new Interval(0, pLineA.Count - 1), new Interval(paramsOnA.First(), paramsOnA.Last()));
                double valA2 = Interval.Map(localInter.ParameterB, new Interval(0, pLineA.Count - 1), new Interval(paramsOnA.First(), paramsOnA.Last()));
                double distance = curve.PointAt(valA1).DistanceTo(curve.PointAt(valA2));
                if (distance > tolerance)
                {
                    int segIdA1 = (int)Math.Floor(localInter.ParameterA);
                    int segIdA2 = (int)Math.Floor(localInter.ParameterA);
                    Interval trimA1 = new Interval(0, 0);
                    trimA1.T0 = paramsOnA[(int)Math.Max(0, (segIdA1 - 1))];
                    trimA1.T1 = paramsOnA[(int)Math.Min(paramsOnA.Length - 1, (segIdA1 + 2))];
                    // }
                    Interval trimA2 = new Interval(0, 0);
                    trimA2.T0 = paramsOnA[(int)Math.Max(0, (segIdA2 - 1))];
                    trimA2.T1 = paramsOnA[(int)Math.Min(paramsOnA.Length - 1, (segIdA2 + 2))];

                    Curve trimedA1 = curve.Trim(trimA1);
                    Curve trimedA2 = curve.Trim(trimA2);
                    if (trimedA1 != null && trimedA2 != null) realInter.AddRange(CurveCurve(trimedA1, trimedA2, tolerance));
                }
                else
                {
                    realInter.Add(new IntersectionEvent(curve.PointAt(valA1), valA1, curve.PointAt(valA2), valA2));
                }
            }

            return realInter;
        }

        /// <summary>
        /// Finds the intersections between two curves. 
        /// </summary>
        /// <param name="curveA">First curve for intersection.</param>
        /// <param name="curveB">Second curve for intersection.</param>
        /// <param name="tolerance">Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.</param>
        /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
        /// <returns>A collection of intersection events.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_intersectcurves.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_intersectcurves.cs' lang='cs'/>
        /// <code source='examples\py\ex_intersectcurves.py' lang='py'/>
        /// </example>
        /// <since>5.0</since>
        public static List<IntersectionEvent> CurveCurve(Curve curveA, Curve curveB, double tolerance)
        {
            // 1e-7 -> trie show thah below this tolerance line from Trim is return  as null. soooo...
            tolerance = Math.Max(1e-7, tolerance);

            // Create approximation of curve with params on real curve domain
            double[] paramsOnA;
            Polyline pLineA = curveA.TryGetPolyline(out paramsOnA);
            double[] paramsOnB;
            Polyline pLineB = curveB.TryGetPolyline(out paramsOnB);

            // Get PolyPoly intersection
            List<IntersectionEvent> inter = PolyLinePolyLine(pLineA, pLineB, tolerance);
            List<IntersectionEvent> realInter = new List<IntersectionEvent>(inter.Count);
            // Check all intersection occured
            for (int i = 0; i < inter.Count; i++)
            {
                IntersectionEvent localInter = inter[i];

                // Convert pline Params to Curve params
                double valA = Interval.Map(localInter.ParameterA, new Interval(0, pLineA.Count - 1), new Interval(paramsOnA.First(), paramsOnA.Last()));
                double valB = Interval.Map(localInter.ParameterB, new Interval(0, pLineB.Count - 1), new Interval(paramsOnB.First(), paramsOnB.Last()));
                // Chceck distance between intersection points
                double distance = curveA.PointAt(valA).DistanceTo(curveB.PointAt(valB));
                //if not in tolerance, get smaller part of curve and intersect one more time.
                if (distance > tolerance)
                {
                    // Get segments Id per curve
                    int segIdA = (int)Math.Floor(localInter.ParameterA);
                    int segIdB = (int)Math.Floor(localInter.ParameterB);

                    // Get Intervals or trim
                    Interval trimA = new Interval(0, 0);
                    trimA.T0 = paramsOnA[(int)Math.Max(0, (segIdA - 1))];
                    trimA.T1 = paramsOnA[(int)Math.Min(paramsOnA.Length - 1, (segIdA + 2))];
    
                    Interval trimB = new Interval(0, 0);
                    trimB.T0 = paramsOnB[(int)Math.Max(0, (segIdB - 1))];
                    trimB.T1 = paramsOnB[(int)Math.Min(paramsOnB.Length - 1, (segIdB + 2))];

                    // Trim Curves
                    Curve trimedA = curveA.Trim(trimA);
                    Curve trimedB = curveB.Trim(trimB);
                    // IF both trimed and exists, Intersect
                    if (trimedA != null && trimedB != null) realInter.AddRange(CurveCurve(trimedA, trimedB, tolerance));
                    // Not sure about that...adding intersetion out of tolerance...
                   // else realInterParams.Add(Tuple.Create(valA, valB));
                }
                //if in tolerance, just addd to final list
                else
                {
                    realInter.Add(new IntersectionEvent(curveA.PointAt(valA), valA, curveB.PointAt(valB), valB));
                }
            }

            return realInter;
        }


#if MUSTHAVE
    /// <summary>
    /// Finds the places where a curve intersects itself. 
    /// </summary>
    /// <param name="curve">Curve for self-intersections.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches itself to within tolerance, 
    /// an intersection is assumed.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <since>5.0</since>
    public static CurveIntersections CurveSelf(Curve curve, double tolerance)
    {
      IntPtr pCurve = curve.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveSelf(pCurve, tolerance);
      GC.KeepAlive(curve);
      return CurveIntersections.Create(pIntersectArray);
    }

    /// <summary>
    /// Finds the intersections between two curves. 
    /// </summary>
    /// <param name="curveA">First curve for intersection.</param>
    /// <param name="curveB">Second curve for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_intersectcurves.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_intersectcurves.cs' lang='cs'/>
    /// <code source='examples\py\ex_intersectcurves.py' lang='py'/>
    /// </example>
    /// <since>5.0</since>
    public static CurveIntersections CurveCurve(Curve curveA, Curve curveB, double tolerance, double overlapTolerance)
    {
      IntPtr pCurveA = curveA.ConstPointer();
      IntPtr pCurveB = curveB.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveCurve(pCurveA, pCurveB, tolerance, overlapTolerance, IntPtr.Zero, IntPtr.Zero);
      Runtime.CommonObject.GcProtect(curveA, curveB);
      return CurveIntersections.Create(pIntersectArray);
    }

    /// <summary>
    /// Finds the intersections between two curves.
    /// </summary>
    /// <param name="curveA">First curve for intersection.</param>
    /// <param name="curveB">Second curve for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <param name="invalidIndices">The indices in the resulting CurveIntersections collection that are invalid.</param>
    /// <param name="textLog">A text log that contains tails about the invalid intersection events.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <since>7.0</since>
    public static CurveIntersections CurveCurveValidate(Curve curveA, Curve curveB, double tolerance, double overlapTolerance, out int[] invalidIndices, out TextLog textLog)
    {
      invalidIndices = new int[0];
      textLog = new TextLog();
      using (SimpleArrayInt output_indices = new SimpleArrayInt())
      {
        IntPtr ptr_curve_a = curveA.ConstPointer();
        IntPtr ptr_curve_b = curveB.ConstPointer();
        IntPtr ptr_indices = output_indices.NonConstPointer();
        IntPtr ptr_text_log = textLog.NonConstPointer();
        IntPtr ptr_intersect_array = UnsafeNativeMethods.ON_Intersect_CurveCurve(ptr_curve_a, ptr_curve_b, tolerance, overlapTolerance, ptr_indices, ptr_text_log);
        if (ptr_intersect_array != IntPtr.Zero)
          invalidIndices = output_indices.ToArray();
        Runtime.CommonObject.GcProtect(curveA, curveB);
        return CurveIntersections.Create(ptr_intersect_array);
      }
    }

    /// <summary>
    /// Intersects a curve and an infinite line. 
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="line">Infinite line to intersect.</param>
    /// <param name="tolerance">Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <since>6.0</since>
    public static CurveIntersections CurveLine(Curve curve, Line line, double tolerance, double overlapTolerance)
    {
      // Extend the line through the curve's bounding box
      var bbox = curve.GetBoundingBox(false);
      if (!bbox.IsValid)
        return null;

      line.ExtendThroughBox(bbox);
      var line_curve = new LineCurve(line);

      IntPtr pCurveA = curve.ConstPointer();
      IntPtr pCurveB = line_curve.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveCurve(pCurveA, pCurveB, tolerance, overlapTolerance, IntPtr.Zero, IntPtr.Zero);
      Runtime.CommonObject.GcProtect(curve, line_curve);
      return CurveIntersections.Create(pIntersectArray);
    }
#endif
#if RHINO_SDK

    /// <summary>
    /// Intersects a curve and a surface.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="surface">Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_curvesurfaceintersect.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_curvesurfaceintersect.cs' lang='cs'/>
    /// <code source='examples\py\ex_curvesurfaceintersect.py' lang='py'/>
    /// </example>
    /// <since>5.0</since>
    public static CurveIntersections CurveSurface(Curve curve, Surface surface, double tolerance, double overlapTolerance)
    {
      IntPtr pCurve = curve.ConstPointer();
      IntPtr pSurface = surface.ConstPointer();
      if (overlapTolerance > 0.0 && overlapTolerance < tolerance)
        overlapTolerance = tolerance;

      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveSurface(pCurve, pSurface, tolerance, overlapTolerance, IntPtr.Zero, IntPtr.Zero);
      Runtime.CommonObject.GcProtect(curve, surface);
      return CurveIntersections.Create(pIntersectArray);
    }

    /// <summary>
    /// Intersects a curve and a surface.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="surface">Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <param name="invalidIndices">The indices in the resulting CurveIntersections collection that are invalid.</param>
    /// <param name="textLog">A text log that contains tails about the invalid intersection events.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <since>7.0</since>
    public static CurveIntersections CurveSurfaceValidate(Curve curve, Surface surface, double tolerance, double overlapTolerance, out int[] invalidIndices, out TextLog textLog)
    {
      invalidIndices = new int[0];
      textLog = new TextLog();

      if (overlapTolerance > 0.0 && overlapTolerance < tolerance)
        overlapTolerance = tolerance;

      using (SimpleArrayInt output_indices = new SimpleArrayInt())
      {
        IntPtr ptr_curve = curve.ConstPointer();
        IntPtr ptr_surface = surface.ConstPointer();
        IntPtr ptr_indices = output_indices.NonConstPointer();
        IntPtr ptr_text_log = textLog.NonConstPointer();
        IntPtr ptr_intersect_array = UnsafeNativeMethods.ON_Intersect_CurveSurface(ptr_curve, ptr_surface, tolerance, overlapTolerance, ptr_indices, ptr_text_log);
        if (ptr_intersect_array != IntPtr.Zero)
          invalidIndices = output_indices.ToArray();
        Runtime.CommonObject.GcProtect(curve, surface);
        return CurveIntersections.Create(ptr_intersect_array);
      }
    }

    /// <summary>
    /// Intersects a sub-curve and a surface.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="curveDomain">Domain of sub-curve to take into consideration for Intersections.</param>
    /// <param name="surface">Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <since>5.0</since>
    public static CurveIntersections CurveSurface(Curve curve, Interval curveDomain, Surface surface, double tolerance, double overlapTolerance)
    {
      Interval domain = curve.Domain;
      double t0 = Math.Max(domain.Min, curveDomain.Min);
      double t1 = Math.Min(domain.Max, curveDomain.Max);
      if (t0 >= t1) { return null; }

      IntPtr pCurve = curve.ConstPointer();
      IntPtr pSurface = surface.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveSurface2(pCurve, pSurface, t0, t1, tolerance, overlapTolerance, IntPtr.Zero, IntPtr.Zero);
      Runtime.CommonObject.GcProtect(curve, surface);
      return CurveIntersections.Create(pIntersectArray);
    }

    /// <summary>
    /// Intersects a sub-curve and a surface.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="curveDomain">Domain of sub-curve to take into consideration for Intersections.</param>
    /// <param name="surface">Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <param name="invalidIndices">The indices in the resulting CurveIntersections collection that are invalid.</param>
    /// <param name="textLog">A text log that contains tails about the invalid intersection events.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <since>7.0</since>
    public static CurveIntersections CurveSurfaceValidate(Curve curve, Interval curveDomain, Surface surface, double tolerance, double overlapTolerance, out int[] invalidIndices, out TextLog textLog)
    {
      invalidIndices = new int[0];
      textLog = new TextLog();

      Interval domain = curve.Domain;
      double t0 = Math.Max(domain.Min, curveDomain.Min);
      double t1 = Math.Min(domain.Max, curveDomain.Max);
      if (t0 >= t1) { return null; }

      if (overlapTolerance > 0.0 && overlapTolerance < tolerance)
        overlapTolerance = tolerance;

      using (SimpleArrayInt output_indices = new SimpleArrayInt())
      {
        IntPtr ptr_curve = curve.ConstPointer();
        IntPtr ptr_surface = surface.ConstPointer();
        IntPtr ptr_indices = output_indices.NonConstPointer();
        IntPtr ptr_text_log = textLog.NonConstPointer();
        IntPtr ptr_intersect_array = UnsafeNativeMethods.ON_Intersect_CurveSurface2(ptr_curve, ptr_surface, t0, t1, tolerance, overlapTolerance, ptr_indices, ptr_text_log);
        if (ptr_intersect_array != IntPtr.Zero)
          invalidIndices = output_indices.ToArray();
        Runtime.CommonObject.GcProtect(curve, surface);
        return CurveIntersections.Create(ptr_intersect_array);
      }
    }

    /// <summary>
    /// Intersects a curve with a Brep. This function returns the 3D points of intersection
    /// and 3D overlap curves. If an error occurs while processing overlap curves, this function 
    /// will return false, but it will still provide partial results.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="brep">Brep for intersection.</param>
    /// <param name="tolerance">Fitting and near miss tolerance.</param>
    /// <param name="overlapCurves">The overlap curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_elevation.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_elevation.cs' lang='cs'/>
    /// <code source='examples\py\ex_elevation.py' lang='py'/>
    /// </example>
    /// <since>5.0</since>
    public static bool CurveBrep(Curve curve, Brep brep, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints)
    {
      overlapCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr curvePtr = curve.ConstPointer();
      IntPtr brepPtr = brep.ConstPointer();

      bool rc = false;

      rc = UnsafeNativeMethods.ON_Intersect_CurveBrep(curvePtr, brepPtr, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        overlapCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();
      Runtime.CommonObject.GcProtect(curve, brep);
      return rc;
    }

    /// <summary>
    /// Intersects a curve with a Brep. This function returns the 3D points of intersection, curve parameters at the intersection locations,
    /// and 3D overlap curves. If an error occurs while processing overlap curves, this function 
    /// will return false, but it will still provide partial results.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="brep">Brep for intersection.</param>
    /// <param name="tolerance">Fitting and near miss tolerance.</param>
    /// <param name="overlapCurves">The overlap curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <param name="curveParameters">The intersection curve parameters will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <since>6.0</since>
    public static bool CurveBrep(Curve curve, Brep brep, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints, out double[] curveParameters)
    {
      overlapCurves = new Curve[0];
      intersectionPoints = new Point3d[0];
      curveParameters = new double[0];

      SimpleArrayPoint3d outputPoints = new SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      SimpleArrayCurvePointer outputCurves = new SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      SimpleArrayDouble outputParameters = new SimpleArrayDouble();
      IntPtr outputParametersPtr = outputParameters.NonConstPointer();

      IntPtr curvePtr = curve.ConstPointer();
      IntPtr brepPtr = brep.ConstPointer();


      bool rc = false;
      rc = UnsafeNativeMethods.ON_Intersect_CurveBrep2(curvePtr, brepPtr, tolerance, outputCurvesPtr, outputPointsPtr, outputParametersPtr);


      if (rc)
      {
        overlapCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
        curveParameters = outputParameters.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();
      outputParameters.Dispose();
      Runtime.CommonObject.GcProtect(curve, brep);

      return rc;
    }

    /// <summary>
    /// Intersect a curve with a Brep. This function returns the intersection parameters on the curve.
    /// </summary>
    /// <param name="curve">Curve.</param>
    /// <param name="brep">Brep.</param>
    /// <param name="tolerance">Absolute tolerance for intersections.</param>
    /// <param name="angleTolerance">Angle tolerance in radians.</param>
    /// <param name="t">Curve parameters at intersections.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <since>6.0</since>
    public static bool CurveBrep(Curve curve, Brep brep, double tolerance, double angleTolerance, out double[] t)
    {
      if (curve == null) throw new ArgumentNullException(nameof(curve));
      if (brep == null) throw new ArgumentNullException(nameof(brep));

      IntPtr curvePtr = curve.ConstPointer();
      IntPtr brepPtr = brep.ConstPointer();
      using (SimpleArrayDouble array = new SimpleArrayDouble())
      {
        IntPtr arrayPtr = array.NonConstPointer();

        bool rc = false;
        rc = UnsafeNativeMethods.ON_Intersect_CurveBrepParameters(curvePtr, brepPtr, tolerance, angleTolerance, arrayPtr);

        Runtime.CommonObject.GcProtect(curve, brep);
        if (rc)
          t = array.ToArray();
        else
          t = new double[0];
        return rc;
      }
    }

    /// <summary>
    /// Intersects a curve with a Brep face.
    /// </summary>
    /// <param name="curve">A curve.</param>
    /// <param name="face">A brep face.</param>
    /// <param name="tolerance">Fitting and near miss tolerance.</param>
    /// <param name="overlapCurves">A overlap curves array argument. This out reference is assigned during the call.</param>
    /// <param name="intersectionPoints">A points array argument. This out reference is assigned during the call.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <since>5.0</since>
    public static bool CurveBrepFace(Curve curve, BrepFace face, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints)
    {
      overlapCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr curvePtr = curve.ConstPointer();
      IntPtr facePtr = face.ConstPointer();

      bool rc = UnsafeNativeMethods.RHC_RhinoCurveFaceIntersect(curvePtr, facePtr, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        overlapCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();
      Runtime.CommonObject.GcProtect(curve, face);

      return rc;
    }

    /// <summary>
    /// Intersects two Surfaces.
    /// </summary>
    /// <param name="surfaceA">First Surface for intersection.</param>
    /// <param name="surfaceB">Second Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance.</param>
    /// <param name="intersectionCurves">The intersection curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <since>5.0</since>
    public static bool SurfaceSurface(Surface surfaceA, Surface surfaceB, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr srfPtrA = surfaceA.ConstPointer();
      IntPtr srfPtrB = surfaceB.ConstPointer();

      bool rc = UnsafeNativeMethods.RHC_RhinoIntersectSurfaces(srfPtrA, srfPtrB, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        intersectionCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();
      Runtime.CommonObject.GcProtect(surfaceA, surfaceB);

      return rc;
    }

    /// <summary>
    /// Intersects two Breps.
    /// </summary>
    /// <param name="brepA">First Brep for intersection.</param>
    /// <param name="brepB">Second Brep for intersection.</param>
    /// <param name="tolerance">Intersection tolerance.</param>
    /// <param name="intersectionCurves">The intersection curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success; false on failure.</returns>
    /// <since>5.0</since>
    public static bool BrepBrep(Brep brepA, Brep brepB, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr brepPtrA = brepA.ConstPointer();
      IntPtr brepPtrB = brepB.ConstPointer();

      bool rc = UnsafeNativeMethods.ON_Intersect_BrepBrep(brepPtrA, brepPtrB, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        intersectionCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();
      Runtime.CommonObject.GcProtect(brepA, brepB);

      return rc;
    }

    /// <summary>
    /// Intersects a Brep and a Surface.
    /// </summary>
    /// <param name="brep">A brep to be intersected.</param>
    /// <param name="surface">A surface to be intersected.</param>
    /// <param name="tolerance">A tolerance value.</param>
    /// <param name="intersectionCurves">The intersection curves array argument. This out reference is assigned during the call.</param>
    /// <param name="intersectionPoints">The intersection points array argument. This out reference is assigned during the call.</param>
    /// <returns>true on success; false on failure.</returns>
    /// <since>5.0</since>
    public static bool BrepSurface(Brep brep, Surface surface, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr brepPtr = brep.ConstPointer();
      IntPtr surfacePtr = surface.ConstPointer();

      bool rc = false;

      rc = UnsafeNativeMethods.ON_Intersect_BrepSurface(brepPtr, surfacePtr, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        intersectionCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();
      Runtime.CommonObject.GcProtect(brep, surface);

      return rc;
    }

    /// <summary>
    /// This is an old overload kept for compatibility. Overlaps and near misses are ignored.
    /// </summary>
    /// <param name="meshA">First mesh for intersection.</param>
    /// <param name="meshB">Second mesh for intersection.</param>
    /// <returns>An array of intersection line segments, or null if no intersections were found.</returns>
    /// <since>5.0</since>
    [Obsolete("Use the MeshMesh() method.")]
    public static Line[] MeshMeshFast(Mesh meshA, Mesh meshB)
    {
      if (UseNewMeshIntersections)
      {
        const double fixed_tolerance = RhinoMath.SqrtEpsilon * 10;

        var arr = new[] { meshA, meshB };
        var made_it = MeshMesh(arr, fixed_tolerance, out Polyline[] result, false, out Polyline[] _, false, out Mesh _, null, System.Threading.CancellationToken.None, null);
        if (!made_it) return null;
        if (result == null) return new Line[0];
        return result.SelectMany((pl) => pl.GetSegments()).ToArray();
      }
      else
      {
        IntPtr ptrA = meshA.ConstPointer();
        IntPtr ptrB = meshB.ConstPointer();
        Line[] intersectionLines = new Line[0];

        using (Runtime.InteropWrappers.SimpleArrayLine arr = new Runtime.InteropWrappers.SimpleArrayLine())
        {
          IntPtr pLines = arr.NonConstPointer();
          int rc = UnsafeNativeMethods.ON_Mesh_IntersectMesh(ptrA, ptrB, pLines);
          if (rc > 0)
            intersectionLines = arr.ToArray();
        }
        Runtime.CommonObject.GcProtect(meshA, meshB);

        return intersectionLines;
      }
    }

    /// <summary>
    /// Instructs Rhino to provide the new mesh-mesh intersector implementation. This affects also MeshSplit and other commands.
    /// </summary>
    internal static bool UseNewMeshIntersections
    {
      get
      {
        return UnsafeNativeMethods.RH_MX_UseNew(false, false);
      }
      set
      {
        UnsafeNativeMethods.RH_MX_UseNew(true, value);
      }
    }

    private const string DiminishMeshIntersectionsTolerancesRequest_CODE = "MeshIntersections.RequestedDiminishTolerancesCoefficient";
    private const double DiminishMeshIntersectionsTolerancesRequest_DEFAULT = 0.0001;

    /// <summary>
    /// <para>Offers a requested adjustment coefficient for mesh-mesh intersections tolerances.
    /// The value can be used to multiply the document absolute tolerance.</para>
    /// <para>This is only a UI value; it is up to developer to honor (or not) this request, depending on application needs.</para>
    /// </summary>
    /// <value><para>Setting the value to 1.0 results in the setting doing nothing.</para>
    /// <para>Setting the value to 0.0 results in the default value being reset.</para>
    /// <para>Setting negative values results in an exception.</para></value>
    /// <remarks>Generally, document tolerances are around 0.001 for objects sized about 100 units.
    /// However, good mesh triangles for these objects are often a few orders of magnitude smaller than these values.
    /// This coefficient is provided to translate absolute document tolerances to values more suitable for good mesh intersections.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">When the value is negative.</exception>
    /// <since>7.0</since>
    public static double MeshIntersectionsTolerancesCoefficient
    {
      get
      {
        return PersistentSettings.RhinoAppSettings.GetDouble(DiminishMeshIntersectionsTolerancesRequest_CODE, DiminishMeshIntersectionsTolerancesRequest_DEFAULT);
      }
      set
      {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");
        if (value == 0) value = DiminishMeshIntersectionsTolerancesRequest_DEFAULT;
        PersistentSettings.RhinoAppSettings.SetDouble(DiminishMeshIntersectionsTolerancesRequest_CODE, value);
      }
    }

    internal static bool MeshMesh_Helper(IEnumerable<Mesh> meshes, double tolerance,
      bool computeSelfIntersections, bool overlaps_with_intersections, out Polyline[] intersections, 
      bool overlapsPolylines, out Polyline[] overlapsPolylinesResult, bool overlapsMesh, out Mesh overlapsMeshResult,
      FileIO.TextLog textLog, System.Threading.CancellationToken cancel, IProgress<double> progress)
    {
      if (meshes == null) throw new ArgumentNullException(nameof(meshes));
      tolerance = Math.Abs(tolerance);

      intersections = null;
      overlapsPolylinesResult = null;
      overlapsMeshResult = null;

      Runtime.Interop.MarshalProgressAndCancelToken(cancel, progress,
        out IntPtr ptr_terminator, out int progress_report_serial_number, out var reporter, out var terminator);

      try
      {
        using (var input = new SimpleArrayMeshPointer())
        {
          foreach (var mesh in meshes)
          {
            if (mesh == null) continue;
            input.Add(mesh, true);
          }

          using (var intersections_native = new SimpleArrayArrayPoint3d())
          using (var overlaps_native = ((overlapsPolylines) ? (overlaps_with_intersections ? intersections_native : new SimpleArrayArrayPoint3d()) : null))
          {
            IntPtr mesh_overlaps_ptr = IntPtr.Zero;
            if (overlapsMesh)
            {
              overlapsMeshResult = new Mesh();
              mesh_overlaps_ptr = overlapsMeshResult.NonConstPointer();
            }
            IntPtr overlaps_native_ptr = IntPtr.Zero;
            if (overlaps_native != null)
            {
              overlaps_native_ptr = overlaps_native.NonConstPointer();
            }

            bool rc = UnsafeNativeMethods.RH_MX_MeshMeshIntersect(computeSelfIntersections,
              input.ConstPointer(), tolerance, intersections_native.NonConstPointer(), overlaps_native_ptr, mesh_overlaps_ptr,
              textLog != null ? textLog.NonConstPointer() : IntPtr.Zero, ptr_terminator, progress_report_serial_number);

            GC.KeepAlive(meshes);

            if (!rc)
            {
              return false;
            }

            if (!overlaps_with_intersections && overlaps_native != null && overlaps_native.Count > 0)
            {
              Polyline[] output_pls = new Polyline[overlaps_native.Count];
              for (int i = 0; i < output_pls.Length; i++)
              {
                int pca = overlaps_native.PointCountAt(i);
                var npl = new Polyline(pca);

                for (int j = 0; j < pca; j++)
                {
                  npl.Add(overlaps_native[i, j]);
                }

                output_pls[i] = npl;
              }

              overlapsPolylinesResult = output_pls;
            }

            if (intersections_native.Count > 0)
            {
              Polyline[] output_pls = new Polyline[intersections_native.Count];
              for (int i = 0; i < output_pls.Length; i++)
              {
                int pca = intersections_native.PointCountAt(i);
                var npl = new Polyline(pca);

                for (int j = 0; j < pca; j++)
                {
                  npl.Add(intersections_native[i, j]);
                }

                output_pls[i] = npl;
              }

              intersections = output_pls;
            }
          }
        }
      }
      finally
      {
        if (reporter != null) reporter.Disable();
        if (terminator != null) terminator.Dispose();
      }

      return true;
    }


    /// <summary>
    /// Intersects meshes. Overlaps and perforations are provided in the output list.
    /// </summary>
    /// <param name="meshes">The mesh input list. This cannot be null. Null entries are tolerated.</param>
    /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
    /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.</param>
    /// <param name="intersections">Returns the intersections.</param>
    /// <param name="overlapsPolylines">If true, overlaps are computed and returned.</param>
    /// <param name="overlapsPolylinesResult">If requested, overlaps are returned here.</param>
    /// <param name="overlapsMesh">If true, an overlaps mesh is computed and returned.</param>
    /// <param name="overlapsMeshResult">If requested, overlaps are returned here.</param>
    /// <param name="textLog">A text log, or null.</param>
    /// <param name="cancel">A cancellation token to stop the computation at a given point.</param>
    /// <param name="progress">A progress reporter to inform the user about progress, or null. The reported value is indicative.</param>
    /// <returns>True, if the operation succeeded, otherwise false.</returns>
    /// <since>7.0</since>
    public static bool MeshMesh(IEnumerable<Mesh> meshes, double tolerance,
      out Polyline[] intersections, bool overlapsPolylines, out Polyline[] overlapsPolylinesResult, bool overlapsMesh, out Mesh overlapsMeshResult,
      FileIO.TextLog textLog, System.Threading.CancellationToken cancel, IProgress<double> progress)
    {
      return MeshMesh_Helper(meshes, tolerance, false, false, out intersections,
        overlapsPolylines, out overlapsPolylinesResult,
        overlapsMesh, out overlapsMeshResult,
        textLog, cancel, progress);
    }

    /// <summary>
    /// Intersects two meshes. Overlaps and near misses are handled. This is an old method kept for compatibility.
    /// </summary>
    /// <param name="meshA">First mesh for intersection.</param>
    /// <param name="meshB">Second mesh for intersection.</param>
    /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
    /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.</param>
    /// <returns>An array of intersection and overlaps polylines.</returns>
    /// <since>5.0</since>
    public static Polyline[] MeshMeshAccurate(Mesh meshA, Mesh meshB, double tolerance)
    {
      if (UseNewMeshIntersections)
      {
        var arr = new[] { meshA, meshB };
        var rc = MeshMesh_Helper(arr, tolerance, false, true,
          out Polyline[] result, true, out Polyline[] _, false, out Mesh _, null, System.Threading.CancellationToken.None, null);
        if (!rc) return null;
        return result;
      }
      else
      {
        IntPtr pMeshA = meshA.ConstPointer();
        IntPtr pMeshB = meshB.ConstPointer();
        int polylines_created = 0;
        IntPtr pPolys = UnsafeNativeMethods.ON_Intersect_MeshMesh1(pMeshA, pMeshB, ref polylines_created, tolerance);
        if (polylines_created < 1 || IntPtr.Zero == pPolys)
          return null;

        // convert the C++ polylines created into .NET polylines. We can reuse the meshplane functions
        Polyline[] rc = new Polyline[polylines_created];
        for (int i = 0; i < polylines_created; i++)
        {
          int point_count = UnsafeNativeMethods.ON_Intersect_MeshPlanes2(pPolys, i);
          Polyline pl = new Polyline(point_count);
          if (point_count > 0)
          {
            pl.m_size = point_count;
            UnsafeNativeMethods.ON_Intersect_MeshPlanes3(pPolys, i, point_count, pl.m_items);
          }
          rc[i] = pl;
        }
        UnsafeNativeMethods.ON_Intersect_MeshPlanes4(pPolys);

        Runtime.CommonObject.GcProtect(meshA, meshB);
        return rc;
      }
    }

    /// <summary>Finds the first intersection of a ray with a mesh.</summary>
    /// <param name="mesh">A mesh to intersect.</param>
    /// <param name="ray">A ray to be casted.</param>
    /// <returns>
    /// >= 0.0 parameter along ray if successful.
    /// &lt; 0.0 if no intersection found.
    /// </returns>
    /// <since>5.0</since>
    public static double MeshRay(Mesh mesh, Ray3d ray)
    {
      IntPtr pConstMesh = mesh.ConstPointer();
      double rc = UnsafeNativeMethods.ON_Intersect_MeshRay1(pConstMesh, ref ray, IntPtr.Zero);
      GC.KeepAlive(mesh);

      return rc;
    }

    /// <summary>Finds the first intersection of a ray with a mesh.</summary>
    /// <param name="mesh">A mesh to intersect.</param>
    /// <param name="ray">A ray to be casted.</param>
    /// <param name="meshFaceIndices">faces on mesh that ray intersects.</param>
    /// <returns>
    /// >= 0.0 parameter along ray if successful.
    /// &lt; 0.0 if no intersection found.
    /// </returns>
    /// <remarks>
    /// The ray may intersect more than one face in cases where the ray hits
    /// the edge between two faces or the vertex corner shared by multiple faces.
    /// </remarks>
    /// <since>5.0</since>
    public static double MeshRay(Mesh mesh, Ray3d ray, out int[] meshFaceIndices)
    {
      meshFaceIndices = null;
      using (Runtime.InteropWrappers.SimpleArrayInt indices = new Rhino.Runtime.InteropWrappers.SimpleArrayInt())
      {
        IntPtr pConstMesh = mesh.ConstPointer();
        double rc = UnsafeNativeMethods.ON_Intersect_MeshRay1(pConstMesh, ref ray, indices.m_ptr);
        int[] vals = indices.ToArray();
        if (vals != null && vals.Length > 0)
          meshFaceIndices = vals;
        GC.KeepAlive(mesh);
        return rc;
      }
    }

    /// <summary>
    /// Finds the intersection of a mesh and a polyline.
    /// </summary>
    /// <param name="mesh">A mesh to intersect.</param>
    /// <param name="curve">A polyline curves to intersect.</param>
    /// <param name="faceIds">The indices of the intersecting faces. This out reference is assigned during the call.</param>
    /// <returns>An array of points: one for each face that was passed by the faceIds out reference.</returns>
    /// <since>5.0</since>
    public static Point3d[] MeshPolyline(Mesh mesh, PolylineCurve curve, out int[] faceIds)
    {
      faceIds = null;
      IntPtr pConstMesh = mesh.ConstPointer();
      IntPtr pConstCurve = curve.ConstPointer();
      int count = 0;
      IntPtr rc = UnsafeNativeMethods.ON_Intersect_MeshPolyline1(pConstMesh, pConstCurve, ref count);
      if (0 == count || IntPtr.Zero == rc)
        return new Point3d[0];

      Point3d[] points = new Point3d[count];
      faceIds = new int[count];
      UnsafeNativeMethods.ON_Intersect_MeshPolyline_Fill(rc, count, points, faceIds);
      Runtime.CommonObject.GcProtect(mesh, curve);
      return points;
    }

    /// <summary>
    /// Finds the intersection of a mesh and a line
    /// </summary>
    /// <param name="mesh">A mesh to intersect</param>
    /// <param name="line">The line to intersect with the mesh</param>
    /// <param name="faceIds">The indices of the intersecting faces. This out reference is assigned during the call.</param>
    /// <returns>An array of points: one for each face that was passed by the faceIds out reference.</returns>
    /// <since>5.0</since>
    public static Point3d[] MeshLine(Mesh mesh, Line line, out int[] faceIds)
    {
      faceIds = null;
      IntPtr pConstMesh = mesh.ConstPointer();
      int count = 0;
      IntPtr rc = UnsafeNativeMethods.ON_Intersect_MeshLine(pConstMesh, line.From, line.To, ref count);
      if (0 == count || IntPtr.Zero == rc)
        return new Point3d[0];

      Point3d[] points = new Point3d[count];
      faceIds = new int[count];
      UnsafeNativeMethods.ON_Intersect_MeshPolyline_Fill(rc, count, points, faceIds);
      GC.KeepAlive(mesh);
      return points;
    }

    /// <summary>
    /// Computes point intersections that occur when shooting a ray to a collection of surfaces and Breps.
    /// </summary>
    /// <param name="ray">A ray used in intersection.</param>
    /// <param name="geometry">Only Surface and Brep objects are currently supported. Trims are ignored on Breps.</param>
    /// <param name="maxReflections">The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.</param>
    /// <returns>An array of points: one for each surface or Brep face that was hit, or an empty array on failure.</returns>
    /// <exception cref="ArgumentNullException">geometry is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxReflections is strictly outside the [1-1000] range.</exception>
    /// <since>5.0</since>
    public static Point3d[] RayShoot(Ray3d ray, IEnumerable<GeometryBase> geometry, int maxReflections)
    {
      if (null == geometry) throw new ArgumentNullException(nameof(geometry));
      if (maxReflections < 1 || maxReflections > 1000)
        throw new ArgumentOutOfRangeException("maxReflections", "maxReflections must be between 1-1000");

      using (var in_geom = new SimpleArrayGeometryPointer(geometry))
      using (var out_points = new SimpleArrayPoint3d())
      {
        var ptr_const_geom = in_geom.ConstPointer();
        var ptr_out_points = out_points.NonConstPointer();
        int count = UnsafeNativeMethods.ON_RayShooter_ShootRay(ptr_const_geom, ray.Position, ray.Direction, maxReflections, ptr_out_points, IntPtr.Zero, IntPtr.Zero);
        if (count > 0) 
          return out_points.ToArray();
      }
      return new Point3d[0];
    }

    /// <summary>
    /// Computes point intersections that occur when shooting a ray to a collection of surfaces and Breps.
    /// </summary>
    /// <param name="geometry">The collection of surfaces and Breps to intersect. Trims are ignored on Breps.</param>
    /// <param name="ray">>A ray used in intersection.</param>
    /// <param name="maxReflections">The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.</param>
    /// <returns>An array of RayShootEvent structs if successful, or an empty array on failure.</returns>
    /// <exception cref="ArgumentNullException">geometry is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxReflections is strictly outside the [1-1000] range.</exception>
    /// <since>7.0</since>
    public static RayShootEvent[] RayShoot(IEnumerable<GeometryBase> geometry, Ray3d ray, int maxReflections)
    {
      if (null == geometry) throw new ArgumentNullException(nameof(geometry));
      if (maxReflections < 1 || maxReflections > 1000)
        throw new ArgumentOutOfRangeException("maxReflections", "maxReflections must be between 1-1000");

      using (var in_geom = new SimpleArrayGeometryPointer(geometry))
      using (var out_geom_idx = new SimpleArrayInt())
      using (var out_face_idx = new SimpleArrayInt())
      using (var out_points = new SimpleArrayPoint3d())
      {
        var ptr_const_geom = in_geom.ConstPointer();
        var ptr_out_geom_idx = out_geom_idx.NonConstPointer();
        var ptr_out_face_idx = out_face_idx.NonConstPointer();
        var ptr_out_points = out_points.NonConstPointer();
        var count = UnsafeNativeMethods.ON_RayShooter_ShootRay(ptr_const_geom, ray.Position, ray.Direction, maxReflections, ptr_out_points, ptr_out_geom_idx, ptr_out_face_idx);
        if (count > 0)
        {
          var points = out_points.ToArray();
          var geom_idx = out_geom_idx.ToArray();
          var face_idx = out_face_idx.ToArray();
          if (count == points.Length && 
            count == geom_idx.Length && 
            count == face_idx.Length
            )
          {
            var rc = new RayShootEvent[count];
            for (var i = 0; i < count; i++)
            {
              rc[i] = new RayShootEvent
              {
                Point = points[i],
                GeometryIndex = geom_idx[i],
                BrepFaceIndex = face_idx[i]
              };
            }
            return rc;
          }
        }
      }
      return new RayShootEvent[0];
    }

#endif

        #endregion

#if RHINO_SDK
    /// <summary>
    /// Projects points onto meshes.
    /// </summary>
    /// <param name="meshes">the meshes to project on to.</param>
    /// <param name="points">the points to project.</param>
    /// <param name="direction">the direction to project.</param>
    /// <param name="tolerance">
    /// Projection tolerances used for culling close points and for line-mesh intersection.
    /// </param>
    /// <returns>
    /// Array of projected points, or null in case of any error or invalid input.
    /// </returns>
    /// <since>5.0</since>
    public static Point3d[] ProjectPointsToMeshes(IEnumerable<Mesh> meshes, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
    {
      Point3d[] rc = null;
      if (meshes != null && points != null)
      {
        Runtime.InteropWrappers.SimpleArrayMeshPointer mesh_array = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
        foreach (Mesh mesh in meshes)
          mesh_array.Add(mesh, true);

        Rhino.Collections.Point3dList inputpoints = new Rhino.Collections.Point3dList(points);
        if (inputpoints.Count > 0)
        {
          IntPtr const_ptr_mesh_array = mesh_array.ConstPointer();

          using (Runtime.InteropWrappers.SimpleArrayPoint3d output = new Runtime.InteropWrappers.SimpleArrayPoint3d())
          {
            IntPtr ptr_output = output.NonConstPointer();
            if (UnsafeNativeMethods.RHC_RhinoProjectPointsToMeshes(const_ptr_mesh_array, direction, tolerance, inputpoints.Count, inputpoints.m_items, ptr_output, IntPtr.Zero))
              rc = output.ToArray();
          }
        }
      }
      GC.KeepAlive(meshes);

      return rc;
    }

    /// <summary>
    /// Projects points onto meshes.
    /// </summary>
    /// <param name="meshes">the meshes to project on to.</param>
    /// <param name="points">the points to project.</param>
    /// <param name="direction">the direction to project.</param>
    /// <param name="tolerance">
    /// Projection tolerances used for culling close points and for line-mesh intersection.
    /// </param>
    /// <param name="indices">Return points[i] is a projection of points[indices[i]]</param>
    /// <returns>
    /// Array of projected points, or null in case of any error or invalid input.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_projectpointstomeshesex.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_projectpointstomeshesex.cs' lang='cs'/>
    /// <code source='examples\py\ex_projectpointstomeshesex.py' lang='py'/>
    /// </example>
    /// <since>5.10</since>
    public static Point3d[] ProjectPointsToMeshesEx(IEnumerable<Mesh> meshes, IEnumerable<Point3d> points, Vector3d direction, double tolerance, out int[] indices)
    {
      Point3d[] rc = null;
      indices = new int[0];
      if (meshes != null && points != null)
      {
        Runtime.InteropWrappers.SimpleArrayMeshPointer mesh_array = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
        foreach (Mesh mesh in meshes)
          mesh_array.Add(mesh, true);

        Rhino.Collections.Point3dList inputpoints = new Rhino.Collections.Point3dList(points);
        if (inputpoints.Count > 0)
        {
          IntPtr const_ptr_mesh_array = mesh_array.ConstPointer();

          using (Runtime.InteropWrappers.SimpleArrayPoint3d output = new Runtime.InteropWrappers.SimpleArrayPoint3d())
          using (Runtime.InteropWrappers.SimpleArrayInt output_indices = new Runtime.InteropWrappers.SimpleArrayInt())
          {
            IntPtr ptr_output = output.NonConstPointer();
            IntPtr ptr_indices = output_indices.NonConstPointer();
            if (UnsafeNativeMethods.RHC_RhinoProjectPointsToMeshes(const_ptr_mesh_array, direction, tolerance, inputpoints.Count, inputpoints.m_items, ptr_output, ptr_indices))
            {
              rc = output.ToArray();
              indices = output_indices.ToArray();
            }
          }
        }
      }
      GC.KeepAlive(meshes);
      return rc;
    }


    /// <summary>
    /// Projects points onto breps.
    /// </summary>
    /// <param name="breps">The breps projection targets.</param>
    /// <param name="points">The points to project.</param>
    /// <param name="direction">The direction to project.</param>
    /// <param name="tolerance">The tolerance used for intersections.</param>
    /// <returns>
    /// Array of projected points, or null in case of any error or invalid input.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_projectpointstobreps.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_projectpointstobreps.cs' lang='cs'/>
    /// <code source='examples\py\ex_projectpointstobreps.py' lang='py'/>
    /// </example>
    /// <since>5.0</since>
    public static Point3d[] ProjectPointsToBreps(IEnumerable<Brep> breps, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
    {
      Point3d[] rc = null;
      if (breps != null && points != null)
      {
        Runtime.InteropWrappers.SimpleArrayBrepPointer brep_array = new Runtime.InteropWrappers.SimpleArrayBrepPointer();
        foreach (Brep brep in breps)
          brep_array.Add(brep, true);

        Rhino.Collections.Point3dList inputpoints = new Rhino.Collections.Point3dList(points);
        if (inputpoints.Count > 0)
        {
          IntPtr const_ptr_brep_array = brep_array.ConstPointer();

          using (Runtime.InteropWrappers.SimpleArrayPoint3d output = new Runtime.InteropWrappers.SimpleArrayPoint3d())
          {
            IntPtr ptr_output_points = output.NonConstPointer();
            if (UnsafeNativeMethods.RHC_RhinoProjectPointsToBreps(const_ptr_brep_array, direction, tolerance, inputpoints.Count, inputpoints.m_items, ptr_output_points, IntPtr.Zero))
              rc = output.ToArray();
          }
        }
      }
      GC.KeepAlive(breps);
      return rc;
    }

    /// <summary>
    /// Projects points onto breps.
    /// </summary>
    /// <param name="breps">The breps projection targets.</param>
    /// <param name="points">The points to project.</param>
    /// <param name="direction">The direction to project.</param>
    /// <param name="tolerance">The tolerance used for intersections.</param>
    /// <param name="indices">Return points[i] is a projection of points[indices[i]]</param>
    /// <returns>
    /// Array of projected points, or null in case of any error or invalid input.
    /// </returns>
    /// <since>5.10</since>
    public static Point3d[] ProjectPointsToBrepsEx(IEnumerable<Brep> breps, IEnumerable<Point3d> points, Vector3d direction, double tolerance, out int[] indices)
    {
      Point3d[] rc = null;
      indices = new int[0];
      if (breps != null && points != null)
      {
        Runtime.InteropWrappers.SimpleArrayBrepPointer brep_array = new Runtime.InteropWrappers.SimpleArrayBrepPointer();
        foreach (Brep brep in breps)
          brep_array.Add(brep, true);

        Rhino.Collections.Point3dList inputpoints = new Rhino.Collections.Point3dList(points);
        if (inputpoints.Count > 0)
        {
          IntPtr const_ptr_brep_array = brep_array.ConstPointer();

          using (Runtime.InteropWrappers.SimpleArrayPoint3d output = new Runtime.InteropWrappers.SimpleArrayPoint3d())
          using (Runtime.InteropWrappers.SimpleArrayInt output_indices = new Runtime.InteropWrappers.SimpleArrayInt())
          {
            IntPtr ptr_output_points = output.NonConstPointer();
            IntPtr ptr_indices = output_indices.NonConstPointer();
            if (UnsafeNativeMethods.RHC_RhinoProjectPointsToBreps(const_ptr_brep_array, direction, tolerance, inputpoints.Count, inputpoints.m_items, ptr_output_points, ptr_indices))
            {
              rc = output.ToArray();
              indices = output_indices.ToArray();
            }
          }
        }
      }
      GC.KeepAlive(breps);
      return rc;
    }

#endif
    }

    /// <summary>
    /// Represents all possible cases of a Plane|Circle intersection event.
    /// </summary>
    public enum PlaneCircleIntersection : int
    {
        /// <summary>
        /// No intersections. Either because radius is too small or because circle plane is parallel but not coincident with the intersection plane.
        /// </summary>
        None = 0,

        /// <summary>
        /// Tangent (one point) intersection.
        /// </summary>
        Tangent = 1,

        /// <summary>
        /// Secant (two point) intersection.
        /// </summary>
        Secant = 2,

        /// <summary>
        /// Circle and plane are planar but not coincident. 
        /// Parallel indicates no intersection took place.
        /// </summary>
        Parallel = 3,

        /// <summary>
        /// Circle and plane are co-planar, they intersect everywhere.
        /// </summary>
        Coincident = 4
    }

    /// <summary>
    /// Represents all possible cases of a Plane|Sphere intersection event.
    /// </summary>
    public enum PlaneSphereIntersection : int
    {
        /// <summary>
        /// No intersections.
        /// </summary>
        None = 0,

        /// <summary>
        /// Tangent intersection.
        /// </summary>
        Point = 1,

        /// <summary>
        /// Circular intersection.
        /// </summary>
        Circle = 2,
    }

    /// <summary>
    /// Represents all possible cases of a Line|Circle intersection event.
    /// </summary>
    public enum LineCircleIntersection : int
    {
        /// <summary>
        /// No intersections.
        /// </summary>
        None = 0,

        /// <summary>
        /// One intersection.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Two intersections.
        /// </summary>
        Multiple = 2,
    }

    /// <summary>
    /// Represents all possible cases of a Line|Sphere intersection event.
    /// </summary>
    public enum LineSphereIntersection : int
    {
        /// <summary>
        /// No intersections.
        /// </summary>
        None = 0,

        /// <summary>
        /// One intersection.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Two intersections.
        /// </summary>
        Multiple = 2,
    }

    /// <summary>
    /// Represents all possible cases of a Line|Cylinder intersection event.
    /// </summary>
    public enum LineCylinderIntersection : int
    {
        /// <summary>
        /// No intersections.
        /// </summary>
        None = 0,

        /// <summary>
        /// One intersection.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Two intersections.
        /// </summary>
        Multiple = 2,

        /// <summary>
        /// Line lies on cylinder.
        /// </summary>
        Overlap = 3
    }

    /// <summary>
    /// Represents all possible cases of a Sphere|Sphere intersection event.
    /// </summary>
    public enum SphereSphereIntersection : int
    {
        /// <summary>
        /// Spheres do not intersect.
        /// </summary>
        None = 0,

        /// <summary>
        /// Spheres touch at a single point.
        /// </summary>
        Point = 1,

        /// <summary>
        /// Spheres intersect at a circle.
        /// </summary>
        Circle = 2,

        /// <summary>
        /// Spheres are identical.
        /// </summary>
        Overlap = 3
    }
}
