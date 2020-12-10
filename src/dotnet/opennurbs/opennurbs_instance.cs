using System;
using Pixel.Rhino.Runtime.InteropWrappers;
using Pixel.Rhino.DocObjects;
using Pixel.Rhino.FileIO;
using Pixel.Rhino.Runtime;
//don't make serializable yet.

namespace Pixel.Rhino.Geometry
{
  /// <summary>
  /// Represents a block definition in a File3dm. This is the same as
  /// Pixel.Rhino.DocObjects.InstanceDefinition, but not associated with a RhinoDoc.
  /// </summary>
  public class InstanceDefinitionGeometry : ModelComponent //was derived from GeometryBase but this can no longer be.
  {
    internal readonly Guid m_id;

    #region internals
#if RHINO_SDK
    /// <summary> This is here if we make InstanceDefinition derive from InstanceDefinitionGeometry.
    /// DO NOT USE unless that becomes true. </summary>
    internal InstanceDefinitionGeometry(Guid id, RhinoDoc parent)
      : base()
    {
      m_id = id;
      m__parent = parent;
    }

    internal InstanceDefinitionGeometry(int index, RhinoDoc parent)
      : base()
    {
      m_id = UnsafeNativeMethods.CRhinoInstanceDefinition_IdFromIndex(parent.RuntimeSerialNumber, index);
      m__parent = parent;
    }

    internal InstanceDefinitionGeometry(Pixel.Rhino.DocObjects.Tables.InstanceDefinitionTableEventArgs parent)
      : base()
    {
      m__parent = parent;
    }

#endif

    internal InstanceDefinitionGeometry(Guid id, File3dm parent)
      : base()
    {
      m_id = id;
      m__parent = parent;

      // 20 Nov 2018 S. Baer (RH-49605)
      // Instance definition geometry that is a child of a File3dm should not hold
      // onto it's pointer.
      //IntPtr parent_ptr = parent.ConstPointer();
      //IntPtr idf_ptr = UnsafeNativeMethods.ONX_Model_GetModelComponentPointer(parent_ptr, id);

      //ConstructNonConstObject(idf_ptr);
    }
#endregion

    /// <summary>
    /// Initializes a new block definition.
    /// </summary>
    /// <since>5.0</since>
    public InstanceDefinitionGeometry()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_InstanceDefinition_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    const int IDX_NAME = 0;
    const int IDX_DESCRIPTION = 1;

    /// <summary>
    /// Gets or sets the description of the definition.
    /// </summary>
    /// <since>5.0</since>
    public string Description
    {
      get
      {
        IntPtr ptr = ConstPointer();
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, IDX_DESCRIPTION, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, IDX_DESCRIPTION, value);
      }
    }

    /// <summary>
    /// Returns <see cref="ModelComponentType.InstanceDefinition"/>.
    /// </summary>
    /// <since>6.0</since>
    public override ModelComponentType ComponentType
    {
      get
      {
        return ModelComponentType.InstanceDefinition;
      }
    }

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      //constructed in table callback
      DocObjects.Tables.InstanceDefinitionTableEventArgs ide = m__parent as DocObjects.Tables.InstanceDefinitionTableEventArgs;
      if (ide != null)
        return ide.ConstLightPointer();

      //derived from doc
      RhinoDoc parent_doc = m__parent as RhinoDoc;
      if (parent_doc != null)
      {
        IntPtr idf_ptr = UnsafeNativeMethods.CRhinoInstanceDefinition_PtrFromId(
          parent_doc.RuntimeSerialNumber, m_id);
      }
#endif
      FileIO.File3dm parent_file = m__parent as FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr ptr_model = parent_file.NonConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetInstanceDefinitionPointer(ptr_model, m_id);
      }
      return IntPtr.Zero;
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is FileIO.File3dm)
        return _InternalGetConstPointer();

      return base.NonConstPointer();
    }

    /// <summary>
    /// list of object ids in the instance geometry table
    /// </summary>
    /// <returns></returns>
    /// <since>5.6</since>
    [ConstOperation]
    public Guid[] GetObjectIds()
    {
      using (Runtime.InteropWrappers.SimpleArrayGuid ids = new Runtime.InteropWrappers.SimpleArrayGuid())
      {
        IntPtr ptr_const_this = ConstPointer();
        IntPtr ptr_id_array = ids.NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_GetObjectIds(ptr_const_this, ptr_id_array);
        return ids.ToArray();
      }
    }
  }

  /// <summary>
  /// Represents a reference to the geometry in a block definition.
  /// </summary>
  public class InstanceReferenceGeometry : GeometryBase
  {
    /// <summary>
    /// Constructor used when creating nested instance references.
    /// </summary>
    /// <param name="instanceDefinitionId"></param>
    /// <param name="transform"></param>
    /// <example>
    /// <code source='examples\cs\ex_nestedblock.cs' lang='cs'/>
    /// </example>
    /// <since>5.1</since>
    public InstanceReferenceGeometry(Guid instanceDefinitionId, Transform transform)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_InstanceRef_New(instanceDefinitionId, ref transform);
      ConstructNonConstObject(ptr);
    }

    internal InstanceReferenceGeometry(IntPtr nativePointer, object parent)
      : base(nativePointer, parent, -1)
    { }

    /// <summary>
    /// The unique id for the parent instance definition of this instance reference.
    /// </summary>
    /// <since>5.6</since>
    public Guid ParentIdefId
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.ON_InstanceRef_IDefId (ptr_const_this);
      }
    }

    /// <summary>Transformation for this reference.</summary>
    /// <since>5.6</since>
    public Transform Xform
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        Transform rc = new Transform();
        UnsafeNativeMethods.ON_InstanceRef_GetTransform (ptr_const_this, ref rc);
        return rc;
      }
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceReferenceGeometry(IntPtr.Zero, null);
    }
  }
}
