//public class WireframeObject : RhinoObject { }
//public class CageObject : WireframeObject { }


#if RHINO_SDK
namespace Pixel.Rhino.DocObjects
{
  /// <summary>
  /// Represents a <see cref="Pixel.Rhino.Geometry.MorphControl">MorphControl</see> in a document.
  /// </summary>
  public class MorphControlObject : RhinoObject
  {
    internal MorphControlObject(uint serialNumber)
      : base(serialNumber) { }

    //internal override CommitGeometryChangesFunc GetCommitFunc()
    //{
    //  return UnsafeNativeMethods.CRhinoMorphControlObject_InternalCommitChanges;
    //}
  }
}
#endif