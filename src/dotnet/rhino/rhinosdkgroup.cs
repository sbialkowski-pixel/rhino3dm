#pragma warning disable 1591
using Pixel.Rhino.FileIO;
using Pixel.Rhino.Runtime.InteropWrappers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

//[skipping] CRhinoGroup

namespace Pixel.Rhino.DocObjects
{
#if RHINO_SDK
  namespace Tables
  {
    /// <summary>
    /// Defines the types of group table events.
    /// </summary>
    public enum GroupTableEventType : int
    {
      /// <summary>
      /// A group was added.
      /// </summary>
      Added = 0,
      /// <summary>
      /// A group was deleted.
      /// </summary>
      Deleted = 1,
      /// <summary>
      /// A group was undeleted.
      /// </summary>
      Undeleted = 2,
      /// <summary>
      /// A group was modified.
      /// </summary>
      Modified = 3,
      /// <summary>
      /// The group table was sorted.
      /// </summary>
      Sorted = 4
    }

    /// <summary>
    /// Contains group table event data.
    /// </summary>
    public class GroupTableEventArgs : EventArgs
    {
      private readonly uint m_doc_serial_number;
      private readonly GroupTableEventType m_event_type;
      private readonly int m_group_index;
      private readonly IntPtr m_ptr_old_group;

      internal GroupTableEventArgs(uint docSerialNumber, int eventType,int index, IntPtr pConstOldGroup)
      {
        m_doc_serial_number = docSerialNumber;
        m_event_type = (GroupTableEventType)eventType;
        m_group_index = index;
        m_ptr_old_group = pConstOldGroup;
      }

      RhinoDoc m_doc;
      /// <summary>
      /// The document in which the event occurred.
      /// </summary>
      /// <since>5.0</since>
      public RhinoDoc Document
      {
        get { return m_doc ?? (m_doc = RhinoDoc.FromRuntimeSerialNumber(m_doc_serial_number)); }
      }

      /// <summary>
      /// The event type.
      /// </summary>
      /// <since>5.0</since>
      public GroupTableEventType EventType
      {
        get { return m_event_type; }
      }

      /// <summary>
      /// The index of the Group that has changed.
      /// </summary>
      /// <since>6.10</since>
      public int GroupIndex
      {
        get { return m_group_index; }
      }

      Group m_new_group;
      /// <summary>
      /// The Group that has changed.
      /// </summary>
      /// <since>6.10</since>
      public Group NewState
      {
        get { return m_new_group ?? (m_new_group = new Group(GroupIndex, Document)); }
      }

      Group m_old_group;
      /// <summary>
      /// If the event is GroupTableEventType.Modified, then the old Group.
      /// </summary>
      /// <since>6.10</since>
      public Group OldState
      {
        get
        {
          if (m_old_group == null && m_ptr_old_group != IntPtr.Zero)
          {
            m_old_group = new Group(m_ptr_old_group);
          }
          return m_old_group;
        }
      }
    }


    /// <summary>
    /// Group tables store the list of groups in a Rhino document.
    /// </summary>
    public sealed class GroupTable :
      RhinoDocCommonTable<Group>, ICollection<Group>
    {
      internal GroupTable(RhinoDoc doc) : base(doc)
      {
      }

      /// <since>6.0</since>
      public override ModelComponentType ComponentType
      {
        get
        {
          return ModelComponentType.Group;
        }
      }


      /// <summary>Finds a group with a given name.</summary>
      /// <param name="groupName">
      /// Name of group to search for. Ignores case.
      /// </param>
      /// <returns>
      /// &gt;=0 index of the group with the given name.
      /// <see cref="RhinoMath.UnsetIntIndex">UnsetIntIndex</see> no group found with the given name.
      /// </returns>
      /// <since>6.0</since>
      [System.ComponentModel.Browsable(false), System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
      public int Find(string groupName)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_FindGroup(m_doc.RuntimeSerialNumber, groupName);
      }

      /// <summary>Finds a group with a given name.</summary>
      /// <param name="groupName">
      /// Name of group to search for. Ignores case.
      /// </param>
      /// <param name="ignoreDeletedGroups">
      /// This parameter is ignored. Deleted groups are never searched.
      /// </param>
      /// <returns>
      /// &gt;=0 index of the group with the given name.
      /// -1 no group found with the given name.
      /// </returns>
      /// <since>5.0</since>
      [Obsolete("Use the overload without the ignoreDeletedGroups input. Note that the new method might return UnsetIntIndex.")]
      [System.ComponentModel.Browsable(false), System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
      public int Find(string groupName, bool ignoreDeletedGroups)
      {
        var rc = Find(groupName);
        return rc == RhinoMath.UnsetIntIndex ? -1 : rc;
      }

      /// <summary>
      /// Finds a Linetype given its name. Returns the instance, rather than the index.
      /// </summary>
      /// <param name="name">The name of the Linetype to be searched.</param>
      /// <returns>An Linetype, or null on error.</returns>
      /// <since>6.0</since>
      public Group FindName(string name)
      {
        return __FindNameInternal(name);
      }

      /// <summary>
      /// Finds a Linetype given its name hash.
      /// </summary>
      /// <param name="nameHash">The name hash of the Linetype to be searched.</param>
      /// <returns>An Linetype, or null on error.</returns>
      /// <since>6.0</since>
      public Group FindNameHash(NameHash nameHash)
      {
        return __FindNameHashInternal(nameHash);
      }

      /// <summary>
      /// Retrieves a Group object based on Index. This search type of search is discouraged.
      /// We are moving towards using only IDs for all tables.
      /// </summary>
      /// <param name="index">The index to search for.</param>
      /// <returns>A Group object, or null if none was found.</returns>
      /// <since>6.0</since>
      public Group FindIndex(int index)
      {
        return __FindIndexInternal(index);
      }

      /// <summary>Adds a new empty group to the group table.</summary>
      /// <param name="groupName">name of new group.</param>
      /// <returns>
      /// &gt;=0 index of new group. 
      /// -1 group not added because a group with that name already exists.
      /// </returns>
      /// <remarks>
      /// In some cases, calling Add() can cause the group indices to become invalid.
      /// </remarks>
      /// <since>5.0</since>
      public int Add(string groupName)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_Add(m_doc.RuntimeSerialNumber, groupName, 0, null);
      }

      void ICollection<Group>.Add(Group item)
      {
        bool mem;
        IntPtr dup = item._InternalDuplicate(out mem);
        UnsafeNativeMethods.CRhinoGroupTable_AddGroupPtr(m_doc.RuntimeSerialNumber, dup);
      }

      /// <summary>Adds a new empty group to the group table.</summary>
      /// <returns>
      /// &gt;=0 index of new group. 
      /// -1 group not added because a group with that name already exists.
      /// </returns>
      /// <remarks>
      /// In some cases, calling Add() can cause the group indices to become invalid.
      /// </remarks>
      /// <since>5.0</since>
      public int Add()
      {
        return UnsafeNativeMethods.CRhinoGroupTable_Add(m_doc.RuntimeSerialNumber, null, 0, null);
      }

      /// <summary>
      /// Adds a new group to the group table with a set of objects.
      /// </summary>
      /// <param name="groupName">Name of new group.</param>
      /// <param name="objectIds">An array, a list or any enumerable set of object IDs.</param>
      /// <returns>
      /// &gt;=0 index of new group. 
      /// <para>-1 group not added because a group with that name already exists.</para>
      /// </returns>
      /// <remarks>
      /// In some cases, calling Add() can cause the group indices to become invalid.
      /// </remarks>
      /// <since>5.0</since>
      public int Add(string groupName, System.Collections.Generic.IEnumerable<Guid> objectIds)
      {
        if (null == objectIds)
          return Add(groupName);
        Pixel.Rhino.Collections.RhinoList<Guid> ids = new Pixel.Rhino.Collections.RhinoList<Guid>();
        foreach (Guid id in objectIds)
        {
          ids.Add(id);
        }
        if (ids.Count < 1)
          return Add(groupName);
        return UnsafeNativeMethods.CRhinoGroupTable_Add(m_doc.RuntimeSerialNumber, groupName, ids.Count, ids.m_items);
      }

      /// <summary>
      /// Adds a new group to the group table with a set of objects.
      /// </summary>
      /// <param name="objectIds">An array, a list or any enumerable set of object IDs.</param>
      /// <returns>
      /// &gt;=0 index of new group.
      /// <para>-1 group not added because a group with that name already exists.</para>
      /// </returns>
      /// <remarks>
      /// In some cases, calling Add() can cause the group indices to become invalid.
      /// </remarks>
      /// <example>
      /// <code source='examples\vbnet\ex_addobjectstogroup.vb' lang='vbnet'/>
      /// <code source='examples\cs\ex_addobjectstogroup.cs' lang='cs'/>
      /// <code source='examples\py\ex_addobjectstogroup.py' lang='py'/>
      /// </example>
      /// <since>5.0</since>
      public int Add(System.Collections.Generic.IEnumerable<Guid> objectIds)
      {
        return Add(null, objectIds);
      }

      /// <summary>
      /// Adds an object to an existing group.
      /// </summary>
      /// <param name="groupIndex">The group index value.</param>
      /// <param name="objectId">An ID of an object.</param>
      /// <returns>true if the operation was successful.</returns>
      /// <since>5.0</since>
      public bool AddToGroup(int groupIndex, Guid objectId)
      {
        Guid[] ids = new Guid[] { objectId };
        return UnsafeNativeMethods.CRhinoGroupTable_AddToGroup(m_doc.RuntimeSerialNumber, groupIndex, 1, ids);
      }

      /// <summary>
      /// Adds a list of objects to an existing group.
      /// </summary>
      /// <param name="groupIndex">The group index value.</param>
      /// <param name="objectIds">An array, a list or any enumerable set of IDs to objects.</param>
      /// <returns>true if at least an operation was successful.</returns>
      /// <since>5.0</since>
      public bool AddToGroup(int groupIndex, System.Collections.Generic.IEnumerable<Guid> objectIds)
      {
        if (null == objectIds)
          return false;
        Pixel.Rhino.Collections.RhinoList<Guid> ids = new Pixel.Rhino.Collections.RhinoList<Guid>();
        foreach (Guid id in objectIds)
        {
          ids.Add(id);
        }
        if (ids.Count < 1)
          return false;
        return UnsafeNativeMethods.CRhinoGroupTable_AddToGroup(m_doc.RuntimeSerialNumber, groupIndex, ids.Count, ids.m_items);
      }

      /// <summary>
      /// Deletes a group from this table.
      /// <para>Deleted groups are kept in the runtime group table so that undo
      /// will work with groups.  Call IsDeleted() to determine if a group is deleted.</para>
      /// </summary>
      /// <param name="groupIndex">An group index to be deleted.</param>
      /// <returns>true if the operation was successful.</returns>
      /// <since>5.0</since>
      public bool Delete(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_DeleteGroup(m_doc.RuntimeSerialNumber, groupIndex, true);
      }

      /// <since>6.0</since>
      public override bool Delete(Group item)
      {
        if (item == null) return false;
        return Delete(item.Index);
      }

      /// <since>5.0</since>
      public bool Undelete(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_DeleteGroup(m_doc.RuntimeSerialNumber, groupIndex, false);
      }

      /// <since>5.0</since>
      public bool IsDeleted(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_IsDeleted(m_doc.RuntimeSerialNumber, groupIndex);
      }

      /// <since>5.0</since>
      public string GroupName(int groupIndex)
      {
        using (var sh = new StringHolder())
        {
          bool worked = UnsafeNativeMethods.CRhinoGroupTable_GroupName(m_doc.RuntimeSerialNumber, groupIndex, sh.NonConstPointer());
          if (!worked) throw new ArgumentOutOfRangeException("groupIndex");
          return sh.ToString();
        }
      }

      /// <since>5.0</since>
      public bool ChangeGroupName(int groupIndex, string newName)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_ChangeGroupName(m_doc.RuntimeSerialNumber, groupIndex, newName);
      }

      /// <since>5.0</since>
      public string[] GroupNames(bool ignoreDeletedGroups)
      {
        int count = Count;
        if (count < 1)
          return null;
        Pixel.Rhino.Collections.RhinoList<string> names = new Pixel.Rhino.Collections.RhinoList<string>(count);
        for (int i = 0; i < count; i++)
        {
          if (ignoreDeletedGroups && IsDeleted(i))
            continue;
          string name = GroupName(i);
          if (!string.IsNullOrEmpty(name))
            names.Add(name);
        }
        if (names.Count < 1)
          return null;
        return names.ToArray();
      }

      const int idxHideGroup = 0;
      const int idxShowGroup = 1;
      const int idxLockGroup = 2;
      const int idxUnlockGroup = 3;
      const int idxGroupObjectCount = 4;

      /// <since>5.0</since>
      public int Hide(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.RuntimeSerialNumber, groupIndex, idxHideGroup);
      }
      /// <since>5.0</since>
      public int Show(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.RuntimeSerialNumber, groupIndex, idxShowGroup);
      }
      /// <since>5.0</since>
      public int Lock(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.RuntimeSerialNumber, groupIndex, idxLockGroup);
      }
      /// <since>5.0</since>
      public int Unlock(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.RuntimeSerialNumber, groupIndex, idxUnlockGroup);
      }
      /// <since>5.0</since>
      public int GroupObjectCount(int groupIndex)
      {
        return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.RuntimeSerialNumber, groupIndex, idxGroupObjectCount);
      }

      /// <summary>
      /// Gets an array of all of the objects in a group.
      /// </summary>
      /// <param name="groupIndex">The index of the group in this table.</param>
      /// <returns>An array with all the objects in the specified group.</returns>
      /// <since>5.0</since>
      public RhinoObject[] GroupMembers(int groupIndex)
      {
        using (Pixel.Rhino.Runtime.InternalRhinoObjectArray rhobjs = new Runtime.InternalRhinoObjectArray())
        {
          IntPtr pRhinoObjects = rhobjs.NonConstPointer();
          UnsafeNativeMethods.CRhinoGroupTable_GroupMembers(m_doc.RuntimeSerialNumber, groupIndex, pRhinoObjects);
          return rhobjs.ToArray();
        }
      }
    }
  }
#endif

  public sealed class Group : ModelComponent
  {
    readonly Guid m_id;
#if RHINO_SDK
    readonly RhinoDoc m_doc;

    internal Group(int index, RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoGroupTable_IdFromIndex(doc.RuntimeSerialNumber, index);
      m_doc = doc;
      m__parent = m_doc;
    }
#endif

    internal Group(Guid id, FileIO.File3dm parent)
    {
      m_id = id;
      m__parent = parent;
    }

    class GroupHolder
    {
      readonly IntPtr m_const_ptr_group;
      public GroupHolder(IntPtr pConstGroup)
      {
        m_const_ptr_group = pConstGroup;
      }
      public IntPtr ConstPointer()
      {
        return m_const_ptr_group;
      }
    }

    internal Group(IntPtr pConstGroup)
    {
      GroupHolder holder = new GroupHolder(pConstGroup);
      ConstructConstObject(holder, -1);
    }


    /// <summary>
    /// Serialization constructor
    /// </summary>
    /// <param name="info">Info.</param>
    /// <param name="context">The context.</param>
    private Group(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      if (m_doc != null)
      {
        IntPtr rc = UnsafeNativeMethods.CRhinoGroupTable_FindGroupPtr(m_doc.RuntimeSerialNumber, m_id);
        if (rc == IntPtr.Zero)
          throw new Runtime.DocumentCollectedException($"Could not find Group with ID {m_id}");
        return rc;
      }
#endif

      FileIO.File3dm file_parent = m__parent as FileIO.File3dm;
      if (file_parent != null)
      {
        IntPtr pConstParent = file_parent.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetModelComponentPointer(pConstParent, m_id);
      }
      return IntPtr.Zero;
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is Pixel.Rhino.FileIO.File3dm)
        return _InternalGetConstPointer();

      return base.NonConstPointer();
    }

    /// <summary>
    /// Returns <see cref="ModelComponentType.Group"/>.
    /// </summary>
    /// <since>6.0</since>
    public override ModelComponentType ComponentType
    {
      get
      {
        return ModelComponentType.Group;
      }
    }


    #region user strings
    /// <summary>
    /// Attach a user string (key,value combination) to this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve this string.</param>
    /// <param name="value">string associated with key.</param>
    /// <returns>true on success.</returns>
    /// <since>6.4</since>
    public bool SetUserString(string key, string value)
    {
      return _SetUserString(key, value);
    }
    /// <summary>
    /// Gets user string from this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve the string.</param>
    /// <returns>string associated with the key if successful. null if no key was found.</returns>
    /// <since>6.4</since>
    public string GetUserString(string key)
    {
      return _GetUserString(key);
    }

    /// <summary>
    /// Gets the amount of user strings.
    /// </summary>
    /// <since>6.4</since>
    public int UserStringCount
    {
      get
      {
        return _UserStringCount;
      }
    }

    /// <summary>
    /// Gets a copy of all (user key string, user value string) pairs attached to this geometry.
    /// </summary>
    /// <returns>A new collection.</returns>
    /// <since>6.4</since>
    public System.Collections.Specialized.NameValueCollection GetUserStrings()
    {
      return _GetUserStrings();
    }
    #endregion


  }
}
