#if RHINO_SDK

namespace Pixel.Rhino.DocObjects
{
  /// <summary>
  /// Rhino Object that represents text geometry and attributes
  /// </summary>
  public class TextObject : AnnotationObjectBase
  {
    internal TextObject(uint serialNumber) : base(serialNumber)
    { }

    /// <summary> Get the text geometry for this object. </summary>
    /// <since>5.0</since>
    public Geometry.TextEntity TextGeometry => Geometry as Geometry.TextEntity;

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoRichTextObject_InternalCommitChanges;
    }
  }
}

#endif
