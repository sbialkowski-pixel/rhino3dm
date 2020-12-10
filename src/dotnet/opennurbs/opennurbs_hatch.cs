using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Pixel.Rhino.DocObjects;
using Pixel.Rhino.Runtime;

namespace Pixel.Rhino.Geometry
{
  /// <summary>
  /// Represents a hatch in planar boundary loop or loops.
  /// This is a 2d entity with a plane defining a local coordinate system.
  /// The loops, patterns, angles, etc are all in this local coordinate system.
  /// The Hatch object manages the plane and loop array
  /// Fill definitions are in the HatchPattern or class derived from HatchPattern
  /// Hatch has an index to get the pattern definition from the pattern table.
  /// </summary>
  [Serializable]
  public class Hatch : GeometryBase
  {
    internal Hatch(IntPtr nativePtr, object parent)
      : base(nativePtr, parent, -1)
    { }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected Hatch(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Hatch(IntPtr.Zero, null);
    }

#if RHINO_SDK
    /// <summary>
    /// Constructs an array of <see cref="Hatch">hatches</see> from a set of curves.
    /// </summary>
    /// <param name="curves">An array, a list or any enumerable set of <see cref="Curve"/>.</param>
    /// <param name="hatchPatternIndex">The index of the hatch pattern in the document hatch pattern table.</param>
    /// <param name="rotationRadians">The relative rotation of the pattern.</param>
    /// <param name="scale">A scaling factor.</param>
    /// <returns>An array of hatches. The array might be empty on error.</returns>
    /// <exception cref="ArgumentNullException">If curves is null.</exception>
    /// <since>5.0</since>
    [Obsolete("Use version that takes a tolerance parameter instead")]
    public static Hatch[] Create(IEnumerable<Curve> curves, int hatchPatternIndex, double rotationRadians, double scale)
    {
      double tol, angle_tol;
      RhinoDoc.ActiveDocTolerances(out tol, out angle_tol);
      return Create(curves, hatchPatternIndex, rotationRadians, scale, tol);
    }

    /// <summary>
    /// Constructs an array of <see cref="Hatch">hatches</see> from a set of curves.
    /// </summary>
    /// <param name="curves">An array, a list or any enumerable set of <see cref="Curve"/>.</param>
    /// <param name="hatchPatternIndex">The index of the hatch pattern in the document hatch pattern table.</param>
    /// <param name="rotationRadians">The relative rotation of the pattern.</param>
    /// <param name="scale">A scaling factor.</param>
    /// <param name="tolerance"></param>
    /// <returns>An array of hatches. The array might be empty on error.</returns>
    /// <exception cref="ArgumentNullException">If curves is null.</exception>
    /// <since>6.0</since>
    public static Hatch[] Create(IEnumerable<Curve> curves, int hatchPatternIndex, double rotationRadians, double scale, double tolerance)
    {
      if (curves == null) throw new ArgumentNullException("curves");

      var curvearray = new Runtime.InteropWrappers.SimpleArrayCurvePointer(curves);
      IntPtr ptr_curve_array = curvearray.NonConstPointer();
      var hatcharray = new Runtime.InteropWrappers.SimpleArrayGeometryPointer();
      IntPtr ptr_hatch_array = hatcharray.NonConstPointer();
      UnsafeNativeMethods.RHC_RhinoCreateHatches(ptr_curve_array, tolerance, hatchPatternIndex, rotationRadians, scale, ptr_hatch_array);
      GeometryBase[] g = hatcharray.ToNonConstArray();
      if (g == null)
        return new Hatch[0];
      List<Hatch> hatches = new List<Hatch>();
      for (int i = 0; i < g.Length; i++)
      {
        Hatch hatch = g[i] as Hatch;
        if (hatch != null)
          hatches.Add(hatch);
      }
      GC.KeepAlive(curves);
      return hatches.ToArray();
    }

    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    /// <summary>
    /// Constructs an array of <see cref="Hatch">hatches</see> from one curve.
    /// </summary>
    /// <param name="curve">A <see cref="Curve"/>.</param>
    /// <param name="hatchPatternIndex">The index of the hatch pattern in the document hatch pattern table.</param>
    /// <param name="rotationRadians">The relative rotation of the pattern.</param>
    /// <param name="scale">A scaling factor.</param>
    /// <returns>An array of hatches. The array might be empty on error.</returns>
    /// <since>5.0</since>
    [Obsolete("Use version that takes a tolerance parameter instead")]
    public static Hatch[] Create(Curve curve, int hatchPatternIndex, double rotationRadians, double scale)
    {
      double tol, angle_tol;
      RhinoDoc.ActiveDocTolerances(out tol, out angle_tol);
      return Create(curve, hatchPatternIndex, rotationRadians, scale, tol);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    /// <summary>
    /// Constructs an array of <see cref="Hatch">hatches</see> from one curve.
    /// </summary>
    /// <param name="curve">A <see cref="Curve"/>.</param>
    /// <param name="hatchPatternIndex">The index of the hatch pattern in the document hatch pattern table.</param>
    /// <param name="rotationRadians">The relative rotation of the pattern.</param>
    /// <param name="scale">A scaling factor.</param>
    /// <param name="tolerance"></param>
    /// <returns>An array of hatches. The array might be empty on error.</returns>
    /// <since>6.0</since>
    public static Hatch[] Create(Curve curve, int hatchPatternIndex, double rotationRadians, double scale, double tolerance)
    {
      return Create(new Curve[] { curve }, hatchPatternIndex, rotationRadians, scale, tolerance);
    }

    /// <summary>
    /// Generate geometry that would be used to draw the hatch with a given hatch pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="patternScale"></param>
    /// <param name="bounds"></param>
    /// <param name="lines"></param>
    /// <param name="solidBrep"></param>
    /// <since>5.6</since>
    [ConstOperation]
    public void CreateDisplayGeometry(DocObjects.HatchPattern pattern, double patternScale, out Curve[] bounds, out Line[] lines, out Brep solidBrep)
    {
      IntPtr const_ptr_this = ConstPointer();
      IntPtr const_ptr_pattern = pattern.ConstPointer();
      using(var curve_array = new Runtime.InteropWrappers.SimpleArrayCurvePointer())
      using(var line_array = new Runtime.InteropWrappers.SimpleArrayLine())
      {
        IntPtr ptr_curves = curve_array.NonConstPointer();
        IntPtr ptr_lines = line_array.NonConstPointer();
        IntPtr ptr_brep = UnsafeNativeMethods.CRhinoHatchPattern_CreateDisplay(const_ptr_this, const_ptr_pattern, patternScale, ptr_curves, ptr_lines);
        solidBrep = (ptr_brep==IntPtr.Zero) ? null : new Brep(ptr_brep, null);
        bounds = curve_array.ToNonConstArray();
        lines = line_array.ToArray();
      }
      GC.KeepAlive(pattern);
    }

    /// <summary>
    /// Decomposes the hatch pattern into an array of geometry.
    /// </summary>
    /// <returns>An array of geometry that formed the appearance of the original elements.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_explodehatch.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_explodehatch.cs' lang='cs'/>
    /// <code source='examples\py\ex_explodehatch.py' lang='py'/>
    /// </example>
    /// <since>5.0</since>
    [ConstOperation]
    public GeometryBase[] Explode()
    {
      using (Runtime.InteropWrappers.SimpleArrayGeometryPointer geometry = new Runtime.InteropWrappers.SimpleArrayGeometryPointer())
      {
        IntPtr ptr_parent_rhinoobject = IntPtr.Zero;

        if (IsDocumentControlled)
        {
          DocObjects.RhinoObject rhobj = ParentRhinoObject();
          if (rhobj != null)
            ptr_parent_rhinoobject = rhobj.ConstPointer();
        }
        IntPtr ptr_geometry_array = geometry.NonConstPointer();
        IntPtr const_ptr_this = ConstPointer();

        UnsafeNativeMethods.ON_Hatch_Explode(const_ptr_this, ptr_parent_rhinoobject, ptr_geometry_array);
        GeometryBase[] rc = geometry.ToNonConstArray();
        return rc;
      }
    }
#endif

    /// <summary>
    /// Gets 3d curves that define the boundaries of the hatch
    /// </summary>
    /// <param name="outer">true to get the outer curves, false to get the inner curves</param>
    /// <returns></returns>
    /// <since>5.0</since>
    [ConstOperation]
    public Curve[] Get3dCurves(bool outer)
    {
      using (Runtime.InteropWrappers.SimpleArrayCurvePointer curves = new Runtime.InteropWrappers.SimpleArrayCurvePointer())
      {
        IntPtr ptr_curve_array = curves.NonConstPointer();
        IntPtr const_ptr_this = ConstPointer();
        UnsafeNativeMethods.ON_Hatch_LoopCurve3d(const_ptr_this, ptr_curve_array, outer);
        return curves.ToNonConstArray();
      }
    }

    /// <summary>
    /// Gets or sets the index of the pattern in the document hatch pattern table.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_replacehatchpattern.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_replacehatchpattern.cs' lang='cs'/>
    /// <code source='examples\py\ex_replacehatchpattern.py' lang='py'/>
    /// </example>
    /// <since>5.0</since>
    public int PatternIndex
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.ON_Hatch_PatternIndex(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetPatternIndex(ptr_this, value);
      }
    }

    /// <summary>
    /// Gets or sets the relative rotation of the pattern.
    /// </summary>
    /// <since>5.0</since>
    public double PatternRotation
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.ON_Hatch_GetRotation(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetRotation(ptr_this, value);
      }
    }

    /// <summary>
    /// Gets or sets the hatch pattern base point
    /// </summary>
    /// <since>6.11</since>
    public Point3d BasePoint
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        Point3d p = new Point3d();
        UnsafeNativeMethods.ON_Hatch_GetBasePoint(const_ptr_this, ref p);
        return p;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetBasePoint(ptr_this, value);
      }
    }

    /// <summary>
    /// Gets or sets the hatch plane
    /// </summary>
    /// <since>6.11</since>
    public Plane Plane
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        Plane p = new Plane();
        UnsafeNativeMethods.ON_Hatch_GetPlane(const_ptr_this, ref p);
        return p;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetPlane(ptr_this, value);
      }
    }

    /// <summary>
    /// Gets or sets the scaling factor of the pattern.
    /// </summary>
    /// <since>5.0</since>
    public double PatternScale
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.ON_Hatch_GetScale(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetScale(ptr_this, value);
      }
    }

    /// <summary>
    /// Scale the hatch's pattern
    /// </summary>
    /// <param name="xform"></param>
    /// <since>6.11</since>
    public void ScalePattern(Transform xform)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_Hatch_ScalePattern(ptr_this, ref xform);
    }

    /// <summary>
    /// Get gradient fill information for this hatch. If the "GradientType" for
    /// the fill is None, then this hatch doesn't have any gradient fill.
    /// </summary>
    /// <returns></returns>
    /// <since>7.0</since>
    public Pixel.Rhino.Display.ColorGradient GetGradientFill()
    {
      IntPtr const_ptr_this = ConstPointer();
      IntPtr ptrColorStopArray = UnsafeNativeMethods.ON_ColorStopArray_New();
      Point3d startPoint = new Point3d();
      Point3d endPoint = new Point3d();
      int gradientType = 0;
      double repeat = 0;
      int stopCount = UnsafeNativeMethods.ON_Hatch_GetGradientData(const_ptr_this, ref startPoint, ref endPoint, ref gradientType, ref repeat, ptrColorStopArray);
      var rc = new Pixel.Rhino.Display.ColorGradient();
      rc.StartPoint = startPoint;
      rc.EndPoint = endPoint;
      rc.GradientType = (Display.GradientType)gradientType;
      rc.Repeat = repeat;
      Display.ColorStop[] stops = new Display.ColorStop[stopCount];
      for( int i=0; i<stopCount; i++ )
      {
        int argb =0;
        double t = 0;
        UnsafeNativeMethods.ON_ColorStopArray_Get(ptrColorStopArray, i, ref argb, ref t);
        var color = System.Drawing.Color.FromArgb(argb);
        stops[i] = new Display.ColorStop(color, t);
      }
      rc.SetColorStops(stops);
      UnsafeNativeMethods.ON_ColorStopArray_Delete(ptrColorStopArray);
      return rc;
    }

    /// <summary>
    /// </summary>
    /// <param name="fill"></param>
    /// <since>7.0</since>
    public void SetGradientFill(Pixel.Rhino.Display.ColorGradient fill)
    {
      IntPtr ptr_this = NonConstPointer();
      if( null==fill )
      {
        UnsafeNativeMethods.ON_Hatch_SetGradientData(ptr_this, Point3d.Unset, Point3d.Unset, (int)Pixel.Rhino.Display.GradientType.None, 0, IntPtr.Zero);
        return;
      }
      var stops = fill.GetColorStops();
      IntPtr ptrColorStopArray = UnsafeNativeMethods.ON_ColorStopArray_New();
      foreach(var stop in stops)
      {
        int argb = stop.Color.ToArgb();
        UnsafeNativeMethods.ON_ColorStopArray_Append(ptrColorStopArray, argb, stop.Position);
      }
      UnsafeNativeMethods.ON_Hatch_SetGradientData(ptr_this, fill.StartPoint, fill.EndPoint, (int)fill.GradientType, fill.Repeat, ptrColorStopArray);
      UnsafeNativeMethods.ON_ColorStopArray_Delete(ptrColorStopArray);
    }
  }
}

namespace Pixel.Rhino.Display
{
  /// <summary>
  /// Combination of a color and position. Used in defining gradient fills
  /// </summary>
  public struct ColorStop
  {
    /// <summary>
    /// Create color stop from a color and position
    /// </summary>
    /// <param name="color"></param>
    /// <param name="t"></param>
    /// <since>7.0</since>
    public ColorStop(System.Drawing.Color color, double t)
    {
      Color = color;
      Position = t;
    }
    /// <summary>
    /// </summary>
    /// <since>7.0</since>
    public System.Drawing.Color Color { get; set; }

    /// <summary> Parameter that Color is defined at </summary>
    /// <since>7.0</since>
    public double Position { get; set; }
  }

  /// <summary>
  /// </summary>
  public class ColorGradient
  {
    List<ColorStop> _stops = new List<ColorStop>();

    /// <summary>
    /// Gradient fill type associated with this hatch
    /// </summary>
    /// <since>7.0</since>
    public Pixel.Rhino.Display.GradientType GradientType
    {
      get;
      set;
    }

    /// <summary>
    /// Get sorted list of colors / positions that a gradient is defined over
    /// </summary>
    /// <returns></returns>
    /// <since>7.0</since>
    public ColorStop[] GetColorStops()
    {
      return _stops.ToArray();
    }

    /// <summary>
    /// Set color stops for the gradient
    /// </summary>
    /// <param name="stops"></param>
    /// <since>7.0</since>
    public void SetColorStops(IEnumerable<ColorStop> stops)
    {
      _stops = new List<ColorStop>(stops);
    }

    /// <summary>
    /// Repeat factor for gradient. Factors greater than 1 define a reflected
    /// repeat factor while values less than -1 define a wrapped repeat factor.
    /// </summary>
    /// <since>7.0</since>
    public double Repeat
    {
      get;
      set;
    }

    /// <summary>
    /// Start point of gradient
    /// </summary>
    /// <since>7.0</since>
    public Pixel.Rhino.Geometry.Point3d StartPoint
    {
      get;
      set;
    }

    /// <summary>
    /// End point of gradient
    /// </summary>
    /// <since>7.0</since>
    public Pixel.Rhino.Geometry.Point3d EndPoint
    {
      get;
      set;
    }
  }
}
