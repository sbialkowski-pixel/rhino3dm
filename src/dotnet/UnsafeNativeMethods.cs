using System;
using Pixel.Rhino.Geometry;
using System.Runtime.InteropServices;
using Pixel.Rhino.Display;

// 19 Dec. 2010 S. Baer
// Giulio saw a significant performance increase by marking this class with the
// SuppressUnmanagedCodeSecurity attribute. See MSDN for details
[System.Security.SuppressUnmanagedCodeSecurity]
internal partial class UnsafeNativeMethods
{
  [StructLayout(LayoutKind.Sequential)]
  public struct Point
  {
    public int X;
    public int Y;

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }
  }

#if RHINO3DM_BUILD
  static UnsafeNativeMethods()
  {
    Init();
  }

  private static bool g_paths_set = false;
  public static void Init()
  {
    if (!g_paths_set)
    {
      var assembly_name = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string dir_name = System.IO.Path.GetDirectoryName(assembly_name);

      switch(Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          {
            string env_path = Environment.GetEnvironmentVariable("path");
            var sub_directory = Environment.Is64BitProcess ? "\\Win64" : "\\Win32";
            Environment.SetEnvironmentVariable("path", env_path + ";" + dir_name + sub_directory);
          }
          break;
        default:
          break; // This is solved on Mac by using a config file
      }
      g_paths_set = true;
    }
  }
#endif


#if RHINO_SDK
  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool GetCursorPos(out Point lpPoint);

  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  internal static extern bool DestroyIcon(IntPtr handle);

  [DllImport("Gdi32.dll", CharSet = CharSet.Auto)]
  internal static extern bool DeleteObject(IntPtr handle);

  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoUiHooks_SetLocalizationLocaleId(Pixel.Rhino.UI.Localization.SetCurrentLanguageIdDelegate hook);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_RhRegisterNamedCallbackProc([MarshalAs(UnmanagedType.LPWStr)]string name, IntPtr callback);


  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoCommonPlugInLoader_SetCallbacks(Pixel.Rhino.Runtime.HostUtils.LoadPluginCallback loadplugin,
    Pixel.Rhino.Runtime.HostUtils.LoadSkinCallback loadskin,
    Action buildlists,
    Pixel.Rhino.Runtime.HostUtils.GetAssemblyIdCallback getassemblyid);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern bool ON_SpaceMorph_MorphGeometry(IntPtr pConstGeometry, double tolerance, [MarshalAs(UnmanagedType.U1)]bool quickpreview, [MarshalAs(UnmanagedType.U1)]bool preserveStructure, IntPtr callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern bool ON_SpaceMorph_MorphPlane(ref Plane pPlane, double tolerance, [MarshalAs(UnmanagedType.U1)]bool quickpreview, [MarshalAs(UnmanagedType.U1)]bool preserveStructure, IntPtr callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetPythonEvaluateCallback(Pixel.Rhino.Runtime.HostUtils.EvaluateExpressionCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetTextFieldEvalCallback(Pixel.Rhino.Runtime.HostUtils.EvaluateTextFieldCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetGetNowProc(Pixel.Rhino.Runtime.HostUtils.GetNowCallback callback, Pixel.Rhino.Runtime.HostUtils.GetFormattedTimeCallback formattedTimCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetSendLogMessageToCloudProc(Pixel.Rhino.Runtime.HostUtils.SendLogMessageToCloudCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetSendToPdfProc(Pixel.Rhino.FileIO.FilePdf.SendToPdfCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetLicenseManagerCallbacks(Pixel.Rhino.Runtime.LicenseManager.InitializeCallback dinitLicenseManagerProc,
                                                             Pixel.Rhino.Runtime.LicenseManager.EchoCallback echoProc,
                                                             Pixel.Rhino.Runtime.LicenseManager.ShowValidationUiCallback showLicenseValidationProc,
                                                             Pixel.Rhino.Runtime.LicenseManager.UuidCallback licenseUuidProc,
                                                             Pixel.Rhino.Runtime.LicenseManager.GetLicenseCallback getLicense,
                                                             Pixel.Rhino.Runtime.LicenseManager.GetCustomLicenseCallback getCustomLicense,
                                                             Pixel.Rhino.Runtime.LicenseManager.AskUserForLicenseCallback askUserForLicense,
                                                             Pixel.Rhino.Runtime.LicenseManager.GetRegisteredOwnerInfoCallback getRegisteredOwnerInfo,
                                                             Pixel.Rhino.Runtime.LicenseManager.ShowExpiredMessageCallback showExpiredMessage,
                                                             Pixel.Rhino.Runtime.LicenseManager.GetInternetTimeCallback getInternetTime
                                                            );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool CRhMainFrame_Invoke(Pixel.Rhino.InvokeHelper.InvokeAction invokeProc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoVisualAnalysisMode_SetCallbacks(Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODEENABLEUIPROC enableuiProc,
    Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODEOBJECTSUPPORTSPROC objectSupportProc,
    Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODESHOWISOCURVESPROC showIsoCurvesProc,
    Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODESETDISPLAYATTRIBUTESPROC displayAttributesProc,
    Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODEUPDATEVERTEXCOLORSPROC updateVertexColorsProc,
    Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODEDRAWRHINOOBJECTPROC drawRhinoObjectProc,
    Pixel.Rhino.Display.VisualAnalysisMode.ANALYSISMODEDRAWGEOMETRYPROC drawGeometryProc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetCallbacks(int serialNumber,
    Pixel.Rhino.PlugIns.PlugIn.OnLoadDelegate onloadCallback,
    Pixel.Rhino.PlugIns.PlugIn.OnShutdownDelegate shutdownCallback,
    Pixel.Rhino.PlugIns.PlugIn.OnGetPlugInObjectDelegate getpluginobjectCallback,
    Pixel.Rhino.PlugIns.PlugIn.CallWriteDocumentDelegate callwriteCallback,
    Pixel.Rhino.PlugIns.PlugIn.WriteDocumentDelegate writedocumentCallback,
    Pixel.Rhino.PlugIns.PlugIn.ReadDocumentDelegate readdocumentCallback,
    Pixel.Rhino.PlugIns.PlugIn.DisplayOptionsDialogDelegate displayOptionsDialog
    );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetCallbacks3(Pixel.Rhino.PlugIns.PlugIn.OnAddPagesToOptionsDelegate addoptionpagesCallback,
                                                         Pixel.Rhino.PlugIns.PlugIn.OnAddPagesToObjectPropertiesDelegate addobjectpropertiespagesCallback,
                                                         Pixel.Rhino.PlugIns.PlugIn.OnPlugInProcDelegate plugInProcCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetCallbacks4(Pixel.Rhino.PlugIns.PlugIn.ResetMessageBoxesDelegate resetMessageBoxesCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetLegacyCallbacks(Pixel.Rhino.PlugIns.PlugIn.LoadSaveProfileDelegate profilefunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoFileImportPlugIn_SetCallbacks(Pixel.Rhino.PlugIns.FileImportPlugIn.AddFileType addfiletype,
    Pixel.Rhino.PlugIns.FileImportPlugIn.ReadFileFunc readfile);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoFileExportPlugIn_SetCallbacks(Pixel.Rhino.PlugIns.FileExportPlugIn.AddFileType addfiletype,
    Pixel.Rhino.PlugIns.FileExportPlugIn.WriteFileFunc writefile);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoRenderPlugIn_SetCallbacks(
    Pixel.Rhino.PlugIns.RenderPlugIn.RenderFunc render,
    Pixel.Rhino.PlugIns.RenderPlugIn.RenderWindowFunc renderwindow,
    Pixel.Rhino.PlugIns.RenderPlugIn.OnSetCurrrentRenderPlugInFunc onsetcurrent,
    Pixel.Rhino.PlugIns.RenderPlugIn.OnRenderDialogPageFunc onRenderDialogPage,
    Pixel.Rhino.PlugIns.RenderPlugIn.RenderMaterialUiEventHandler materialUi
    );

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoRenderPlugIn_SetRdkCallbacks(
    Pixel.Rhino.PlugIns.RenderPlugIn.SupportsFeatureCallback supportsFeatureCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.PreferBasicContentCallback preferBasicContentCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.AbortRenderCallback abortRenderCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.AllowChooseContentCallback allowChooseContentCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.CreateDefaultContentCallback createDefaultContentCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.OutputTypesCallback outputTypesCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.PreviewRenderTypeCallback previewRenderTypeCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.CreateTexturePreviewCallback texturePreviewCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.CreatePreviewCallback previewCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.DecalCallback decalCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.PlugInQuestionCallback plugInQuestionCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.RegisterContentIoCallback registerContentIoCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.RegisterCustomPlugInsCallback registerCustomPlugInsCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.GetCustomRenderSaveFileTypesCallback getCustomRenderSaveFileTypesCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.UiContentTypesCallback uiContentTypesCallback,
    Pixel.Rhino.PlugIns.RenderPlugIn.SaveCusomtomRenderFileCallback saveCustomRenderFile,
    Pixel.Rhino.PlugIns.RenderPlugIn.RenderSettingsSectionsCallback renderSettingsSections
    );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetRdkInitializationCallbacks(Pixel.Rhino.Runtime.HostUtils.InitializeRDKCallback init, Pixel.Rhino.Runtime.HostUtils.ShutdownRDKCallback shut);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoDigitizerPlugIn_SetCallbacks(Pixel.Rhino.PlugIns.DigitizerPlugIn.EnableDigitizerFunc enablefunc,
    Pixel.Rhino.PlugIns.DigitizerPlugIn.UnitSystemFunc unitsystemfunc,
    Pixel.Rhino.PlugIns.DigitizerPlugIn.PointToleranceFunc pointtolfunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern IntPtr CRhinoSkin_New(Pixel.Rhino.Runtime.Skin.ShowSplashCallback cb, [MarshalAs(UnmanagedType.LPWStr)]string name, IntPtr hicon);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoCommand_SetCallbacks(int commandSerialNumber,
    Pixel.Rhino.Commands.Command.RunCommandCallback cb,
    Pixel.Rhino.Commands.Command.DoHelpCallback dohelpCb,
    Pixel.Rhino.Commands.Command.ContextHelpCallback contexthelpCb,
    Pixel.Rhino.Commands.Command.ReplayHistoryCallback replayhistoryCb,
    Pixel.Rhino.Commands.SelCommand.SelFilterCallback selCb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoDisplayConduit_SetCallback(int which, Pixel.Rhino.Display.DisplayPipeline.ConduitCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportcb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetReplaceColorDialogCallback(Pixel.Rhino.UI.Dialogs.ColorDialogCallback cb);

  //In RhinoApp
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetEscapeKeyCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetKeyboardCallback(Pixel.Rhino.RhinoApp.KeyboardHookEvent cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetInitAppCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetCloseAppCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetAppSettingsChangeCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  //In Command
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetBeginCommandCallback(Pixel.Rhino.Commands.Command.CommandCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetEndCommandCallback(Pixel.Rhino.Commands.Command.CommandCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetUndoEventCallback(Pixel.Rhino.Commands.Command.UndoCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  //In RhinoDoc
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetCloseDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetNewDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetActiveDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);
  
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CNetModelessUserInterfaceDocChanged_SetCallback(Pixel.Rhino.RhinoDoc.DocumentCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDocPropChangeCallback(Pixel.Rhino.RhinoDoc.DocumentCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetBeginOpenDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentIoCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetEndOpenDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentIoCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetOnAfterPostReadViewUpdateCallback(Pixel.Rhino.RhinoDoc.DocumentIoCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetViewModifiedEventCallback(Pixel.Rhino.Display.RhinoView.ViewCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetBeginSaveDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentIoCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetEndSaveDocumentCallback(Pixel.Rhino.RhinoDoc.DocumentIoCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetAddObjectCallback(Pixel.Rhino.RhinoDoc.RhinoObjectCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDeleteObjectCallback(Pixel.Rhino.RhinoDoc.RhinoObjectCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetReplaceObjectCallback(Pixel.Rhino.RhinoDoc.RhinoObjectCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetUnDeleteObjectCallback(Pixel.Rhino.RhinoDoc.RhinoObjectCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetPurgeObjectCallback(Pixel.Rhino.RhinoDoc.RhinoObjectCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetSelectObjectCallback(Pixel.Rhino.RhinoDoc.RhinoObjectSelectionCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDeselectAllObjectsCallback(Pixel.Rhino.RhinoDoc.RhinoDeselectAllObjectsCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetModifyObjectAttributesCallback(Pixel.Rhino.RhinoDoc.RhinoModifyObjectAttributesCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetLayerTableEventCallback(Pixel.Rhino.RhinoDoc.RhinoTableCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDimStyleTableEventCallback(Pixel.Rhino.RhinoDoc.RhinoTableCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetTextureMappingEventCallback(Pixel.Rhino.RhinoDoc.TextureMappingEventCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetIdefTableEventCallback(Pixel.Rhino.RhinoDoc.RhinoTableCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetLightTableEventCallback(Pixel.Rhino.RhinoDoc.RhinoTableCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetMaterialTableEventCallback(Pixel.Rhino.RhinoDoc.RhinoTableCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetGroupTableEventCallback(Pixel.Rhino.RhinoDoc.RhinoTableCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoDoc_AddCustomUndoEvent(uint docSerialNumber, [MarshalAs(UnmanagedType.LPWStr)]string description,
                                                           Pixel.Rhino.RhinoDoc.RhinoUndoEventHandlerCallback undoCb,
                                                           Pixel.Rhino.RhinoDoc.RhinoDeleteUndoEventHandlerCallback deleteCb);

  //In RhinoView
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetCreateViewCallback(Pixel.Rhino.Display.RhinoView.ViewCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDestroyViewCallback(Pixel.Rhino.Display.RhinoView.ViewCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetActiveViewCallback(Pixel.Rhino.Display.RhinoView.ViewCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetRenameViewCallback(Pixel.Rhino.Display.RhinoView.ViewCallback cb, Pixel.Rhino.Runtime.HostUtils.ReportCallback reportCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDetailEventCallback(Pixel.Rhino.Display.RhinoPageView.PageViewCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDisplayModeChangedEventCallback(Pixel.Rhino.Display.DisplayPipeline.DisplayModeChangedCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetOnIdleCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetTransformObjectsCallback(Pixel.Rhino.RhinoDoc.RhinoTransformObjectsCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetOnMainLoopCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetObject_GetObjects(IntPtr ptr, int min, int max, Pixel.Rhino.Input.Custom.GetObject.GeometryFilterCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetPoint_GetPoint(
    IntPtr ptr,
    [MarshalAs(UnmanagedType.U1)]bool onMouseUp,
    [MarshalAs(UnmanagedType.U1)]bool getPoint2D,
    Pixel.Rhino.Input.Custom.GetPoint.MouseCallback mouseCb,
    Pixel.Rhino.Input.Custom.GetPoint.DrawCallback drawCb,
    Pixel.Rhino.Display.DisplayPipeline.ConduitCallback postDrawCb,
    Pixel.Rhino.Input.Custom.GetTransform.CalculateXformCallack calcXformCb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetXform_GetXform(IntPtr ptr, Pixel.Rhino.Input.Custom.GetPoint.MouseCallback mouseCb,
                                                      Pixel.Rhino.Input.Custom.GetPoint.DrawCallback drawCb,
                                                      Pixel.Rhino.Display.DisplayPipeline.ConduitCallback postDrawCb,
                                                      Pixel.Rhino.Input.Custom.GetTransform.CalculateXformCallack calcXformCb);

  //In RhinoObject
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObject_SetCallbacks(Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectDuplicateCallback duplicate,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectDocNotifyCallback docNotify,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectActiveInViewportCallback activeInViewport,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectSelectionCallback selectionChange,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectTransformCallback transform,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectSpaceMorphCallback morph,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectDeletedCallback deleted);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObject_SetPerObjectCallbacks(IntPtr ptrObject, IntPtr drawCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObject_SetPickCallbacks(Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectPickCallback pick,
                                                        Pixel.Rhino.DocObjects.RhinoObject.RhinoObjectPickedCallback pickedCallback);

  //RH_C_FUNCTION void CRhCmnMouseCallback_SetCallback(enum MouseCallbackType which, RHMOUSEEVENTCALLBACK_PROC func)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnMouseCallback_SetCallback(MouseCallbackType which, Pixel.Rhino.Display.RhinoView.MouseCallback callback);


  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoApp_RegisterGripsEnabler(Guid key, Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoGripsEnablerCallback turnonFunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObjectGrips_SetCallbacks(Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsResetCallback resetFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsResetCallback resetmeshFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsUpdateMeshCallback updatemeshFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNewGeometryCallback newgeomFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsDrawCallback drawFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNeighborGripCallback neighborgripFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNurbsSurfaceGripCallback nurbssurfacegripFunc,
    Pixel.Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNurbsSurfaceCallback nurbssurfaceFunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnGripObject_SetCallbacks(Pixel.Rhino.DocObjects.Custom.CustomGripObject.CRhinoObjectDestructorCallback destructorFunc,
    Pixel.Rhino.DocObjects.Custom.CustomGripObject.CRhinoGripObjectWeightCallback getweightFunc,
    Pixel.Rhino.DocObjects.Custom.CustomGripObject.CRhinoGripObjectSetWeightCallback setweightFunc);

  #region RDK Functions
  //int Rdk_Globals_ShowColorPicker(HWND hWnd, ON_4FVECTOR_STRUCT v, bool bUseAlpha, ON_4fPoint* pColor)
  // Z:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\c_rdk\rdk_plugin.cpp line 1430
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern int Rdk_Globals_ShowColorPickerNewEx(
    IntPtr hWnd,
    Pixel.Rhino.Display.Color4f vColor,
    [MarshalAs(UnmanagedType.U1)]bool bUseAlpha,
    ref Pixel.Rhino.Display.Color4f pColor,
    Pixel.Rhino.UI.Dialogs.OnColorChangedEvent colorCallback,
    IntPtr named_color_list);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern int Rdk_Globals_ShowColorPickerEx(
    IntPtr hWnd,
    Pixel.Rhino.Display.Color4f vColor,
    [MarshalAs(UnmanagedType.U1)]bool bUseAlpha,
    ref Pixel.Rhino.Display.Color4f pColor,
    IntPtr pointerToNamedArgs,
    Pixel.Rhino.UI.Dialogs.OnColorChangedEvent colorCallback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureEvaluatorCallbacks(Pixel.Rhino.Render.TextureEvaluator.GetColorCallback callbackFunc,
    Pixel.Rhino.Render.TextureEvaluator.OnDeleteThisCallback ondeletethisCallback, Pixel.Rhino.Render.TextureEvaluator.InitializeCallback initCallbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureCallback(Pixel.Rhino.Render.RenderContent.NewRenderContentCallbackEvent callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewMaterialCallback(Pixel.Rhino.Render.RenderContent.NewRenderContentCallbackEvent callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewEnvironmentCallback(Pixel.Rhino.Render.RenderContent.NewRenderContentCallbackEvent callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureEvaluatorCallback(Pixel.Rhino.Render.RenderTexture.GetNewTextureEvaluatorCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateTextureCallback(Pixel.Rhino.Render.RenderTexture.SimulateTextureCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureGetVirtualIntCallback(Pixel.Rhino.Render.RenderTexture.GetVirtualIntCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureSetVirtualIntCallback(Pixel.Rhino.Render.RenderTexture.SetVirtualIntCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureGetVirtualVector3dCallback(Pixel.Rhino.Render.RenderTexture.GetVirtual3DVectorCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureSetVirtualVector3dCallback(Pixel.Rhino.Render.RenderTexture.SetVirtual3DVectorCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateMaterialCallback(Pixel.Rhino.Render.RenderMaterial.SimulateMaterialCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateEnvironmentCallback(Pixel.Rhino.Render.RenderEnvironment.SimulateEnvironmentCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoDeleteThisCallback(Pixel.Rhino.Render.RenderContentSerializer.DeleteThisCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoLoadCallback(Pixel.Rhino.Render.RenderContentSerializer.LoadCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoSaveCallback(Pixel.Rhino.Render.RenderContentSerializer.SaveCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoStringCallback(Pixel.Rhino.Render.RenderContentSerializer.GetRenderContentIoStringCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureChildSlotNameCallback(Pixel.Rhino.Render.RenderMaterial.TextureChildSlotNameCallback callbackFunc);

#pragma warning disable 612
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_DeleteThis(Pixel.Rhino.Render.CustomRenderMeshProvider.CrmProviderDeleteThisCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_WillBuild(Pixel.Rhino.Render.CustomRenderMeshProvider.CrmProviderWillBuildCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_BBox(Pixel.Rhino.Render.CustomRenderMeshProvider.CrmProviderBBoxCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_Build(Pixel.Rhino.Render.CustomRenderMeshProvider.CrmProviderBuildCallback callbackFunc);
#pragma warning restore 612

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetSunChangedEventCallback(Pixel.Rhino.Render.Sun.RdkSunSettingsChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetUndoRedoEventCallback(Pixel.Rhino.Render.UndoRedo.RdkUndoRedoCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetUndoRedoEndedEventCallback(Pixel.Rhino.Render.UndoRedo.RdkUndoRedoEndedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetGroundPlaneChangedEventCallback(Pixel.Rhino.Render.GroundPlane.RdkGroundPlaneSettingsChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetSafeFrameChangedEventCallback(Pixel.Rhino.Render.SafeFrame.RdkSafeFrameChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetSkylightChangedEventCallback(Pixel.Rhino.Render.Skylight.RdkSkylightSettingsChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetOnRenderImageEventCallback(Pixel.Rhino.Render.ImageFile.OnRenderImageCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCallback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetOnRenderWindowClonedEventCallback(Pixel.Rhino.Render.RenderWindow.ClonedEventCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCallback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetOnCustomEventCallback(Pixel.Rhino.Render.CustomEvent.RdkCustomEventCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCallback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListClearingEventCallback(Pixel.Rhino.Render.RenderContentTableEventForwarder.ContentListClearingCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListClearedEventCallback(Pixel.Rhino.Render.RenderContentTableEventForwarder.ContentListClearedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListLoadedEventCallback(Pixel.Rhino.Render.RenderContentTableEventForwarder.ContentListLoadedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentOnCppDtorCallback(Pixel.Rhino.Render.RenderContent.RenderContentOnCppDtorCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentBitFlagsCallback(Pixel.Rhino.Render.RenderContent.RenderContentBitFlagsCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetContentStringCallback(Pixel.Rhino.Render.RenderContent.GetRenderContentStringCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetAddUISectionsCallback(Pixel.Rhino.Render.RenderContent.AddUiSectionsCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetDefaultsFromUserCallback(Pixel.Rhino.Render.RenderContent.GetDefaultsFromUserCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_IsFactoryProductAcceptableAsChildCallback(Pixel.Rhino.Render.RenderContent.IsFactoryProductAcceptableAsChildCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetIsContentTypeAcceptableAsChildCallback(Pixel.Rhino.Render.RenderContent.IsContentTypeAcceptableAsChildCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentRenderCrcCallback(Pixel.Rhino.Render.RenderContent.RenderCrcCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSetParameterCallback(Pixel.Rhino.Render.RenderContent.SetParameterCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetEmbeddedFilesCallback(Pixel.Rhino.Render.RenderContent.SetEmbeddedFilesCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetOnContentFieldChangeCallback(Pixel.Rhino.Render.RenderContent.OnContentFieldChangedCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetParameterCallback(Pixel.Rhino.Render.RenderContent.GetParameterCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetExtraRequirementParameterCallback(Pixel.Rhino.Render.RenderContent.GetExtraRequirementParameterCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSetExtraRequirementParameterCallback(Pixel.Rhino.Render.RenderContent.SetExtraRequirementParameterCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetContentIconCallback(Pixel.Rhino.Render.RenderContent.SetContentIconCallback callbackFunction);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetHarvestDataCallback(Pixel.Rhino.Render.RenderContent.HarvestDataCallback callbackFunc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetShaderCallback(Pixel.Rhino.Render.RenderContent.GetShaderCallback callbackFunc);

  //Pixel.Rhino.Render.UI.UserInterfaceSection is obsolete
#pragma warning disable 0612
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_ContentUiSectionSetCallbacks(Pixel.Rhino.Render.UI.UserInterfaceSection.SerialNumberCallback deleteThisCallback,
                                                               Pixel.Rhino.Render.UI.UserInterfaceSection.SerialNumberCallback displayDataCallback,
                                                               Pixel.Rhino.Render.UI.UserInterfaceSection.SerialNumberBoolCallback onExpandCallback,
                                                               Pixel.Rhino.Render.UI.UserInterfaceSection.SerialNumberCallback isHiddenCallback
                                                              );
#pragma warning restore 0612

  // UiSection
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_RhinoUiSectionImpl_SetCallbacks(Pixel.Rhino.UI.Controls.Delegates.MOVEPROC move,
                                                              Pixel.Rhino.RDK.Delegates.SETBOOLPROC show,
                                                              Pixel.Rhino.RDK.Delegates.VOIDPROC deletethis,
                                                              Pixel.Rhino.RDK.Delegates.GETBOOLPROC isShown,
                                                              Pixel.Rhino.RDK.Delegates.GETBOOLPROC isEnabled,
                                                              Pixel.Rhino.RDK.Delegates.SETBOOLPROC enable,
                                                              Pixel.Rhino.RDK.Delegates.GETINTPROC getHeight,
                                                              Pixel.Rhino.RDK.Delegates.GETINTPROC getInitialState,
                                                              Pixel.Rhino.RDK.Delegates.GETBOOLPROC isHidden,
                                                              Pixel.Rhino.RDK.Delegates.FACTORYPROC getSection,
                                                              Pixel.Rhino.RDK.Delegates.SETINTPTRPROC setParent,
                                                              Pixel.Rhino.RDK.Delegates.GETGUIDPROC id,
                                                              Pixel.Rhino.RDK.Delegates.GETSTRINGPROC englishCaption,
                                                              Pixel.Rhino.RDK.Delegates.GETSTRINGPROC localCaption,
                                                              Pixel.Rhino.RDK.Delegates.GETBOOLPROC collapsible,
                                                              Pixel.Rhino.RDK.Delegates.GETINTPROC backgroundColor,
                                                              Pixel.Rhino.RDK.Delegates.SETINTPROC setBackgroundColor,
                                                              Pixel.Rhino.RDK.Delegates.GETINTPTRPROC getWindowPtr,
                                                              Pixel.Rhino.RDK.Delegates.SETGUIDPROC onEvent,
                                                              Pixel.Rhino.RDK.Delegates.VOIDPROC onViewModelActivatedEvent,
                                                              Pixel.Rhino.RDK.Delegates.GETGUIDPROC PlugInId,
                                                              Pixel.Rhino.RDK.Delegates.GETSTRINGPROC CommandOptName,
                                                              Pixel.Rhino.RDK.Delegates.GETINTPROC RunScript,
                                                              Pixel.Rhino.RDK.Delegates.GETGUIDPROC cid);


  // UiSection
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_RhinoUiViewModelImpl_SetCallbacks(Pixel.Rhino.RDK.Delegates.FACTORYPROC getViewModel,
                                                                    Pixel.Rhino.RDK.Delegates.SETGUIDEVENTINFOPROC onEvent,
                                                                    Pixel.Rhino.RDK.Delegates.VOIDPROC deleteThis,
                                                                    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC requireddatasources);

  // GenericController
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_RhinoGenericController_SetCallbacks(Pixel.Rhino.RDK.Delegates.SETGUIDEVENTINFOPROC onEvent,
                                                                      Pixel.Rhino.RDK.Delegates.SETINTPTRPROC requireddatasources);

  // UiWithController
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_UiWithController_SetCallbacks(Pixel.Rhino.RDK.Delegates.VOIDPROC deletethis,
                                                                Pixel.Rhino.RDK.Delegates.SETINTPTRPROC setcontroller,
                                                                Pixel.Rhino.RDK.Delegates.SETGUIDPROC onevent,
                                                                Pixel.Rhino.RDK.Delegates.GETGUIDPROC controllerid);
  // RdkMenu
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_Menu_SetCallbacks(Pixel.Rhino.RDK.Delegates.ADDSUBMENUPROC addsubmenu,
                                                    Pixel.Rhino.RDK.Delegates.ADDITEMPROC additem,
                                                    Pixel.Rhino.RDK.Delegates.ADDSEPARATORPROC addseparator);

  // RdkDataSource
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_DataSource_SetCallbacks(Pixel.Rhino.RDK.Delegates.SUPPORTEDUUIDDATAPROC supporteduuiddata);

  // RdkColorDataSource
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_ColorDataSource_SetCallbacks(Pixel.Rhino.RDK.Delegates.GETCOLORPROC getcolor,
                                                               Pixel.Rhino.RDK.Delegates.SETCOLORPROC setcolor,
                                                               Pixel.Rhino.RDK.Delegates.USESALPHAPROC usesalpha);

  // UiDynamicAcces
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_UiDynamicAccess_SetCallbacks(Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addsectionspre,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addsectionspost,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRREFINTPTRPROC newcontentcreatorusingtypebrowser,
                                                               Pixel.Rhino.RDK.Delegates.GETGUIDINTPROC contenteditortabid,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addtextureadjustmentsection,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addtwocolorsection,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addlocalmapping2dsection,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addlocalmapping3dsection,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addgraphsection,
                                                               Pixel.Rhino.RDK.Delegates.GETINTPTRPROC newfloatingpreviewmanager,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRGETBOOLPROC openfloatingcontentedtirodlg,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRINTPTRBOOLPROC gettagswindow,
                                                               Pixel.Rhino.RDK.Delegates.SETREFINTREFINTREFBOOLBOOLPROC showdecalsmappingstyledialog,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRINTPTRUINTUINTBOOLBOOLPROC showmodalsundialog,
                                                               Pixel.Rhino.RDK.Delegates.SETINTPTRGETBOOLPROC showmodalcontenteditordialog,
                                                               Pixel.Rhino.RDK.Delegates.SETUINTGUIDBOOLPROC showchooselayersdlg,
                                                               Pixel.Rhino.RDK.Delegates.NEWRENDERFRAMEPROC newrenderframe,
                                                               Pixel.Rhino.RDK.Delegates.SETBOOLUINT showttmappingmesheditordockbar,
                                                               Pixel.Rhino.RDK.Delegates.SHOWRENDERINGOPENFILEDLGPROC ShowRenderOpenFileDlg,
                                                               Pixel.Rhino.RDK.Delegates.SHOWRENDERINGSAVEFILEDLGPROC ShowRenderSaveFileDlg,
                                                               Pixel.Rhino.RDK.Delegates.SHOWCONTENTTYPEBROWSERPROC ShowContentTypeBrowser,
                                                               Pixel.Rhino.RDK.Delegates.PROMPTFORIMAGEFILEPARAMSPROC PromptForImageFileParams,
                                                               Pixel.Rhino.RDK.Delegates.SHOWNAMEDITEMEDITDLGPROC ShowNamedItemEditDlg,
                                                               Pixel.Rhino.RDK.Delegates.SHOWSMARTMERGENAMECOLLISIONDLGPROC ShowSmartMergeNameCollisionDialog,
                                                               Pixel.Rhino.RDK.Delegates.SHOWPREVIEWPROPERTIESDLGPROC ShowPreviewPropertiesDlg,
                                                               Pixel.Rhino.RDK.Delegates.CHOOSECONTENTPROC ChooseContent,
                                                               Pixel.Rhino.RDK.Delegates.PEPPICKPOINTONIMAGEPROC PepPickPointOnImage,
                                                               Pixel.Rhino.RDK.Delegates.SHOWLAYERMATERIALDIALOGPROC ShowLayerMaterialDialog,
                                                               Pixel.Rhino.RDK.Delegates.PROMPTFORIMAGEDRAGOPTIONSDLGPROC PromptForImageDragOptionsDlg,
                                                               Pixel.Rhino.RDK.Delegates.PROMPTFOROPENACTIONSDLGPROC PromptForOpenActionsDlg,
                                                               Pixel.Rhino.RDK.Delegates.DISPLAYMISSINGTEXTURESDIALOG DisplayMissingTexturesDialog,
                                                               Pixel.Rhino.RDK.Delegates.OPENNAMEDVIEWANIMATIONSETTINGSDLG OpenNamedViewAnimationSettingsDlg,
                                                               Pixel.Rhino.RDK.Delegates.PEPPICKRECTANGLEONIMAGEPROC PepPickRectangleOnImage,
                                                               Pixel.Rhino.RDK.Delegates.SHOWCONTENTCTRLPROPDLGPROC ShowContentCtrlPropDlg,
                                                               Pixel.Rhino.RDK.Delegates.CREATEINPLACERENDERVIEWPROC CreateInPlaceRenderView,
                                                               Pixel.Rhino.RDK.Delegates.ADDCONTENTAUTOMATICUISECTIONPROC AddContentAutomaticUISection,
                                                               Pixel.Rhino.RDK.Delegates.SHOWNAMEDVIEWPROPERTIESDLG ShowNamedItemPropertiesDlg,
                                                               Pixel.Rhino.RDK.Delegates.ONPLUGINLOADEDPROC OnPlugInLoaded,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsFog,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionGlow,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsGlare,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsDOF,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsGamma,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsToneMappingNone,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsToneMappingBlackWhitePoint,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsToneMappingLogarithmic,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsToneMappingFilmic,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsToneMappingFilmicAdvanced,
                                                               Pixel.Rhino.RDK.Delegates.NEWRENDERSETTINGPAGEPROC NewRenderSettingsPage,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsDithering,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsWatermark,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsHueSatLum,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsBriCon,
                                                               Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC AddPostEffectUISectionsMultiplier,
                                                               Pixel.Rhino.RDK.Delegates.PEPRENDERSETTINGSPAGEPROC AttachRenderPostEffectsPage);

  // EarlyPostEffectPlugIn
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_CRdkCmnEarlyPostEffectPlugIn_SetCallbacks(Pixel.Rhino.RDK.Delegates.SHOWNPROC shown,
                                                                            Pixel.Rhino.RDK.Delegates.SETSHOWNPROC setshown,
                                                                            Pixel.Rhino.RDK.Delegates.ONPROC on,
                                                                            Pixel.Rhino.RDK.Delegates.SETONPROC seton,
                                                                            Pixel.Rhino.RDK.Delegates.FIXEDPROC fixedd,
                                                                            Pixel.Rhino.RDK.Delegates.CANEXECUTEPROC canexecute,
                                                                            Pixel.Rhino.RDK.Delegates.REQUIREDCHANNELSPROC requiredchannels,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEWHILERENDERINGDELAYMSPROC executewhilerenderingdelaysms,
                                                                            Pixel.Rhino.RDK.Delegates.SUPPORTSHDRDATAPROC supportshdrdata,
                                                                            Pixel.Rhino.RDK.Delegates.READFROMDOCUMENTDEFAULTSPROC readfromdocumentdefaults,
                                                                            Pixel.Rhino.RDK.Delegates.WRITETODOCUMENTDEFAULTSPROC writetodocumentdefaults,
                                                                            Pixel.Rhino.RDK.Delegates.CRCPROC crc,
                                                                            Pixel.Rhino.RDK.Delegates.UUIDPROC uuid,
                                                                            Pixel.Rhino.RDK.Delegates.LOCALNAMEPROC localname,
                                                                            Pixel.Rhino.RDK.Delegates.USAGEFLAGSPROC usageflags,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEWHILERENDERINGOPTIONSPROC executewhilerenderingoptions,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEPROC execute,
                                                                            Pixel.Rhino.RDK.Delegates.GETPARAMPROC getparam,
                                                                            Pixel.Rhino.RDK.Delegates.SETPARAMPROC setparam,
                                                                            Pixel.Rhino.RDK.Delegates.READSTATEPROC readstate,
                                                                            Pixel.Rhino.RDK.Delegates.WRITESTATEPROC writestate,
                                                                            Pixel.Rhino.RDK.Delegates.RESETTOFACTORYDEFAULTSPROC resettofactorydefaults,
                                                                            Pixel.Rhino.RDK.Delegates.ADDUISECTIONSPROC adduiseections,
                                                                            Pixel.Rhino.RDK.Delegates.DISPLAYHELPPROC displayhelp,
                                                                            Pixel.Rhino.RDK.Delegates.CANDISPLAYHELPPROC candisplayhelp);

  // LatePostEffectPlugIn
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_CRdkCmnLatePostEffectPlugIn_SetCallbacks(Pixel.Rhino.RDK.Delegates.SHOWNPROC shown,
                                                                            Pixel.Rhino.RDK.Delegates.SETSHOWNPROC setshown,
                                                                            Pixel.Rhino.RDK.Delegates.ONPROC on,
                                                                            Pixel.Rhino.RDK.Delegates.SETONPROC seton,
                                                                            Pixel.Rhino.RDK.Delegates.FIXEDPROC fixedd,
                                                                            Pixel.Rhino.RDK.Delegates.CANEXECUTEPROC canexecute,
                                                                            Pixel.Rhino.RDK.Delegates.REQUIREDCHANNELSPROC requiredchannels,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEWHILERENDERINGDELAYMSPROC executewhilerenderingdelaysms,
                                                                            Pixel.Rhino.RDK.Delegates.SUPPORTSHDRDATAPROC supportshdrdata,
                                                                            Pixel.Rhino.RDK.Delegates.READFROMDOCUMENTDEFAULTSPROC readfromdocumentdefaults,
                                                                            Pixel.Rhino.RDK.Delegates.WRITETODOCUMENTDEFAULTSPROC writetodocumentdefaults,
                                                                            Pixel.Rhino.RDK.Delegates.CRCPROC crc,
                                                                            Pixel.Rhino.RDK.Delegates.UUIDPROC uuid,
                                                                            Pixel.Rhino.RDK.Delegates.LOCALNAMEPROC localname,
                                                                            Pixel.Rhino.RDK.Delegates.USAGEFLAGSPROC usageflags,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEWHILERENDERINGOPTIONSPROC executewhilerenderingoptions,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEPROC execute,
                                                                            Pixel.Rhino.RDK.Delegates.GETPARAMPROC getparam,
                                                                            Pixel.Rhino.RDK.Delegates.SETPARAMPROC setparam,
                                                                            Pixel.Rhino.RDK.Delegates.READSTATEPROC readstate,
                                                                            Pixel.Rhino.RDK.Delegates.WRITESTATEPROC writestate,
                                                                            Pixel.Rhino.RDK.Delegates.RESETTOFACTORYDEFAULTSPROC resettofactorydefaults,
                                                                            Pixel.Rhino.RDK.Delegates.ADDUISECTIONSPROC adduiseections,
                                                                            Pixel.Rhino.RDK.Delegates.DISPLAYHELPPROC displayhelp,
                                                                            Pixel.Rhino.RDK.Delegates.CANDISPLAYHELPPROC candisplayhelp);

  // ToneMappingPostEffectPlugIn
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_CRdkCmnToneMappingPostEffectPlugIn_SetCallbacks(
                                                                            Pixel.Rhino.RDK.Delegates.CANEXECUTEPROC canexecute,
                                                                            Pixel.Rhino.RDK.Delegates.REQUIREDCHANNELSPROC requiredchannels,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEWHILERENDERINGDELAYMSPROC executewhilerenderingdelaysms,
                                                                            Pixel.Rhino.RDK.Delegates.SUPPORTSHDRDATAPROC supportshdrdata,
                                                                            Pixel.Rhino.RDK.Delegates.READFROMDOCUMENTDEFAULTSPROC readfromdocumentdefaults,
                                                                            Pixel.Rhino.RDK.Delegates.WRITETODOCUMENTDEFAULTSPROC writetodocumentdefaults,
                                                                            Pixel.Rhino.RDK.Delegates.CRCPROC crc,
                                                                            Pixel.Rhino.RDK.Delegates.UUIDPROC uuid,
                                                                            Pixel.Rhino.RDK.Delegates.LOCALNAMEPROC localname,
                                                                            Pixel.Rhino.RDK.Delegates.USAGEFLAGSPROC usageflags,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEWHILERENDERINGOPTIONSPROC executewhilerenderingoptions,
                                                                            Pixel.Rhino.RDK.Delegates.EXECUTEPROC execute,
                                                                            Pixel.Rhino.RDK.Delegates.GETPARAMPROC getparam,
                                                                            Pixel.Rhino.RDK.Delegates.SETPARAMPROC setparam,
                                                                            Pixel.Rhino.RDK.Delegates.READSTATEPROC readstate,
                                                                            Pixel.Rhino.RDK.Delegates.WRITESTATEPROC writestate,
                                                                            Pixel.Rhino.RDK.Delegates.RESETTOFACTORYDEFAULTSPROC resettofactorydefaults,
                                                                            Pixel.Rhino.RDK.Delegates.ADDUISECTIONSPROC adduiseections,
                                                                            Pixel.Rhino.RDK.Delegates.DISPLAYHELPPROC displayhelp,
                                                                            Pixel.Rhino.RDK.Delegates.CANDISPLAYHELPPROC candisplayhelp,
                                                                            Pixel.Rhino.RDK.Delegates.SETMANAGERPROC setmanager);
  // CmnPostEffectFactory
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_CRdkCmnPostEffectFactory_SetCallbacks(Pixel.Rhino.RDK.Delegates.NEWPOSTEFFECTPROC newposteffect,
                                                                        Pixel.Rhino.RDK.Delegates.PEFUUIDPROC pluginid);

  // CmnPostEffectJob
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_CRdkCmnPostEffectJob_SetCallbacks(Pixel.Rhino.RDK.Delegates.CLONEPOSTEFFECTJOBPROC clone,
                                                                    Pixel.Rhino.RDK.Delegates.DELETETHISPOSTEFFECTJOB delete,
                                                                    Pixel.Rhino.RDK.Delegates.EXECUTEPOSTEFFECTJOB execute);

  // FloatingPreviewManager
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_FloatingPreviewManager_SetCallbacks(Pixel.Rhino.RDK.Delegates.GETGUIDINTPTRINTINTPROC openfloatingpreview,
                                                                      Pixel.Rhino.RDK.Delegates.GETGUIDINTPTRINTINTPROC openfloatingpreviewatmouse,
                                                                      Pixel.Rhino.RDK.Delegates.SETGUIDINTINTBOOLPROC resizefloatingpreview,
                                                                      Pixel.Rhino.RDK.Delegates.SETGUIDBOOLPROC closefloatingpreview);

  // ContentUIAgent
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_ContentUIAgent_SetCallbacks(Pixel.Rhino.RDK.Delegates.SETINTPTRPROC adduisections_proc,
                                                              Pixel.Rhino.RDK.Delegates.GETGUIDPROC contenttypeid_proc);
  
  // SnapShotsClient
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhinoSnapShotsClient_SetCallbacks(Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETSTRINGPROC category,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETSTRINGPROC name,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLPROC supportsdocument,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCBUFFERPROC savedocument,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCBUFFERPROC restoredocument,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLPROC supportsobjects,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLOBJECTPROC supportsobject,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCOBJECTTRANSFORMBUFFERPROC saveobject,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCOBJECTTRANSFORMBUFFERPROC restoreobject,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLPROC supportsanimation,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.SETINTINTPROC animationstart,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCBUFFERBUFFERPROC preparefordocumentanimation,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.SETDOCBUFFERBUFFERBBOXPROC extendboundingboxfordocumentanimation,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETDOCDOUBLEBUFFERBUFFERPROC animatedocument,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCOBJTRANSFORMBUFFERBUFFERPROC prepareforobjectanimation,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.SETBOOLDOCOBJTRANSFORMBUFFERBUFFERBBOXPROC extendboundingboxforobjectanimation,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCOBJTRANSFORMDOUBLEBUFFERBUFFERPROC animateobject,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.SETINTINT animationstop,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCOBJECTTRANSFORMBUFFERPROC objecttransformnotification,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETGUIDPROC pluginid,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETGUIDPROC clientid,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.SETINTINT snapshotrestored,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCBUFFERBUFFERARRAYTEXTLOGPROC iscurrentmodelstateinanysnapshot,
                                                                    Pixel.Rhino.DocObjects.SnapShots.SnapShotsClient.GETBOOLDOCOBJBUFFERBUFFERARRAYTEXTLOGPROC iscurrentobjmodelstateinanysnapshot);

  // ThumbnailList
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_UiHolder_SetCallbacks(
      Pixel.Rhino.UI.Controls.Delegates.MOVEPROC m,
      Pixel.Rhino.RDK.Delegates.SETBOOLPROC s,
      Pixel.Rhino.RDK.Delegates.VOIDPROC dt,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC ish,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC ien,
      Pixel.Rhino.RDK.Delegates.SETBOOLPROC e,
      Pixel.Rhino.RDK.Delegates.VOIDPROC u,
      Pixel.Rhino.UI.Controls.CollapsibleSectionHolderImpl.ATTACHSECTIONPROC a,
      Pixel.Rhino.UI.Controls.CollapsibleSectionHolderImpl.ATTACHSECTIONPROC de,
      Pixel.Rhino.RDK.Delegates.GETINTPROC sc,
      Pixel.Rhino.RDK.Delegates.ATINDEXPROC sa,
      Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sp);


  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_Thumbnaillist_SetCallbacks(
      Pixel.Rhino.UI.Controls.Delegates.MOVEPROC m,
      Pixel.Rhino.RDK.Delegates.SETBOOLPROC s,
      Pixel.Rhino.RDK.Delegates.VOIDPROC dt,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC ish,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC ien,
      Pixel.Rhino.RDK.Delegates.SETBOOLPROC e,
      Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sp,
      Pixel.Rhino.RDK.Delegates.GETGUIDPROC id,
      Pixel.Rhino.RDK.Delegates.SETMODEPROC a,
      Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sc,
      Pixel.Rhino.RDK.Delegates.SETGUIDPROC sid,
      Pixel.Rhino.RDK.Delegates.VOIDSETSELECTIONPROC ssp,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC gm,
      Pixel.Rhino.RDK.Delegates.GETINTPTRPROC getWindowPtr,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC ic,
      Pixel.Rhino.RDK.Delegates.SETBOOLPROC su,
      Pixel.Rhino.RDK.Delegates.GETBOOLPROC gu,
      Pixel.Rhino.RDK.Delegates.SETINTPTRPROC addThumb,
      Pixel.Rhino.RDK.Delegates.GETGUIDPROC cid);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_BreadCrumb_SetCallbacks(
    Pixel.Rhino.UI.Controls.Delegates.MOVEPROC m,
    Pixel.Rhino.RDK.Delegates.SETBOOLPROC s,
    Pixel.Rhino.RDK.Delegates.VOIDPROC dt,
    Pixel.Rhino.RDK.Delegates.GETBOOLPROC ish,
    Pixel.Rhino.RDK.Delegates.GETBOOLPROC ien,
    Pixel.Rhino.RDK.Delegates.SETBOOLPROC e,
    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sp,
    Pixel.Rhino.RDK.Delegates.GETGUIDPROC id,
    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sc,
    Pixel.Rhino.RDK.Delegates.SETGUIDPROC sid,
    Pixel.Rhino.RDK.Delegates.GETINTPTRPROC getWindowPtr,
    Pixel.Rhino.RDK.Delegates.GETBOOLPROC ic,
    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC scec,
    Pixel.Rhino.RDK.Delegates.GETGUIDPROC cid);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_NewContentCtrl_SetCallbacks(
    Pixel.Rhino.UI.Controls.Delegates.MOVEPROC m,
    Pixel.Rhino.RDK.Delegates.SETBOOLPROC s,
    Pixel.Rhino.RDK.Delegates.VOIDPROC dt,
    Pixel.Rhino.RDK.Delegates.GETBOOLPROC ish,
    Pixel.Rhino.RDK.Delegates.GETBOOLPROC ien,
    Pixel.Rhino.RDK.Delegates.SETBOOLPROC e,
    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sp,
    Pixel.Rhino.RDK.Delegates.GETGUIDPROC id,
    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC sc,
    Pixel.Rhino.RDK.Delegates.SETGUIDPROC sid,
    Pixel.Rhino.RDK.Delegates.GETINTPTRPROC getWindowPtr,
    Pixel.Rhino.RDK.Delegates.GETBOOLPROC ic,
    Pixel.Rhino.RDK.Delegates.SETINTPTRPROC scec,
    Pixel.Rhino.RDK.Delegates.GETGUIDPROC cid,
    Pixel.Rhino.RDK.Delegates.SETSELECTIONPROC ssp);

  // LightManagerSupport
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_LightManagerSupport_SetCallbacks(
    Pixel.Rhino.RDK.Delegates.GETGUIDPROC RenderEngineId,
    Pixel.Rhino.RDK.Delegates.GETGUIDPROC PluginId,
    Pixel.Rhino.Render.LightManagerSupport.MODIFYLIGHTPROC ModifyLight,
    Pixel.Rhino.Render.LightManagerSupport.DELETELIGHTPROC DeleteLight,
    Pixel.Rhino.Render.LightManagerSupport.GETLIGHTSPROC GetLights,
    Pixel.Rhino.Render.LightManagerSupport.LIGHTFROMIDPROC LightFromId,
    Pixel.Rhino.Render.LightManagerSupport.OBJECTSERIALNUMBERFROMLIGHTPROC ObjectSerialNumberFromLight,
    Pixel.Rhino.Render.LightManagerSupport.ONEDITLIGHTPROC OnEditLight,
    Pixel.Rhino.Render.LightManagerSupport.GROUPLIGHTSPROC GroupLights,
    Pixel.Rhino.Render.LightManagerSupport.UNGROUPPROC UnGroup,
    Pixel.Rhino.Render.LightManagerSupport.LIGHTDESCRIPTIONPROC LightDescription,
    Pixel.Rhino.Render.LightManagerSupport.SETLIGHTSOLO SetLightSolo,
    Pixel.Rhino.Render.LightManagerSupport.GETLIGHTSOLO GeTLightSolo,
    Pixel.Rhino.Render.LightManagerSupport.LIGHTSINSOLOSTORAGE LightInSoloStorage
  );

  // ChangeQueue >>
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_ChangeQueue_SetCallbacks(
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.ViewCallback viewcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.MeshChangesCallback meshcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.MaterialChangesCallback matcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.BeginUpdatesCallback beginnotifcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.EndUpdatesCallback endnotifcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.DynamicUpdatesAreAvailableCallback dynupdatesavailablecb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.BakeForCallback bakeforcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.BakingSizeCallback bakingsizecb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.ApplyDisplayPipelineAttributesChangesCallback displayattrcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.MeshInstanceChangesCallback meshinstancecb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.GroundplaneChangesCallback gpcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.DynamicObjectChangesCallback dynobjchangescb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.LightChangesCallback lightcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.DynamicLightChangesCallback dynlightscb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.SunChangesCallback suncb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.SkylightChangesCallback skylightcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.EnvironmentChangesCallback envcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.RenderSettingsChangesCallback bgcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.LinearWorkflowChangesCallback lwfcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.ProvideOriginalObjectCallback origcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.ClippingPlaneChangesCallback clippingcb,
    Pixel.Rhino.Render.ChangeQueue.ChangeQueue.DynamicClippingPlaneChangesCallback dynclippingcb
    );
  // << ChangeQueue

  // >> RenderedDisplayMode
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_RtVpImpl_SetCallbacks(Pixel.Rhino.Render.RealtimeDisplayMode.CreateWorldCallback createWorld,
    Pixel.Rhino.Render.RealtimeDisplayMode.StartRenderCallback startRender,
    Pixel.Rhino.Render.RealtimeDisplayMode.RestartRenderCallback restartRender,
    Pixel.Rhino.Render.RealtimeDisplayMode.ShutdownRenderCallback shutdownRender,
    Pixel.Rhino.Render.RealtimeDisplayMode.LastRenderedPassCallback lastRenderedPass,
    Pixel.Rhino.Render.RealtimeDisplayMode.IsRendererStartedCallback isRendererStarted,
    Pixel.Rhino.Render.RealtimeDisplayMode.IsRenderframeAvailableCallback isRenderframeAvailable,
    Pixel.Rhino.Render.RealtimeDisplayMode.IsFrameBufferAvailableCallback isFramebufferAvailable,
    Pixel.Rhino.Render.RealtimeDisplayMode.ShowCaptureProgressCallback showCaptureProgress,
    Pixel.Rhino.Render.RealtimeDisplayMode.CaptureProgressCallback captureProgress,

    Pixel.Rhino.Render.RealtimeDisplayMode.OnDisplayPipelineSettingsChangedCallback onDisplayPipelineSettingsChanged,
    Pixel.Rhino.Render.RealtimeDisplayMode.OnDrawMiddlegroundCallback onDrawMiddleGround,
    Pixel.Rhino.Render.RealtimeDisplayMode.OnInitFramebufferCallback onInitFramebuffer,

    Pixel.Rhino.Render.RealtimeDisplayMode.DrawOpenGlCallback drawOpenGl,
    Pixel.Rhino.Render.RealtimeDisplayMode.UseFastDrawCallback useFastDraw,

    Pixel.Rhino.Render.RealtimeDisplayMode.HudProductNameCallback hudProductName,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudCustomStatusTextCallback hudCustomStatusText,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudMaximumPassesCallback hudMaximumPasses,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudLastRenderedPassCallback hudLastRenderedPass,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudRendererPausedCallback hudRendererPaused,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudRendererLockedCallback hudRendererLocked,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudShowMaxPassesCallback hudShowMaxPasses,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudShowPassesCallback hudShowPasses,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudShowCustomStatusTextCallback hudShowCustomStatusText,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudShowControlsCallback hudShowControls,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudShowCallback hudShow,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudStartTimeCallback hudStartTime,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudStartTimeMSCallback hudStartTimeMS,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudPlayButtonPressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudPauseButtonPressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudLockButtonPressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudUnlockButtonPressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudProductNamePressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudStatusTextPressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudButtonPressed hudTimePressed,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudMaxPassesChanged hudMaxPassesChanged,
    Pixel.Rhino.Render.RealtimeDisplayMode.HudAllowEditMaxPassesCallback hudAllowEditMaxPasses
    );
  // << RenderedDisplayMode

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSunAndRenderSettingsCallbacks(Pixel.Rhino.PlugIns.PlugIn.OnAddSectionsToSunPanelDelegate s,
                                                    Pixel.Rhino.PlugIns.PlugIn.OnAddSectionsToRenderSettingsPanelDelegate rs);

  //Events
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetObjectMaterialAssignmentChangedEventCallback(Pixel.Rhino.Render.RenderContentTableEventForwarder.MaterialAssigmentChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetLayerMaterialAssignmentChangedEventCallback(Pixel.Rhino.Render.RenderContentTableEventForwarder.MaterialAssigmentChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentAddedEventCallback(Pixel.Rhino.Render.RenderContent.ContentAddedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentRenamedEventCallback(Pixel.Rhino.Render.RenderContent.ContentRenamedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentDeletingEventCallback(Pixel.Rhino.Render.RenderContent.ContentDeletingCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentDeletedEventCallback(Pixel.Rhino.Render.RenderContent.ContentDeletingCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentReplacingEventCallback(Pixel.Rhino.Render.RenderContent.ContentReplacingCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentReplacedEventCallback(Pixel.Rhino.Render.RenderContent.ContentReplacedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentChangedEventCallback(Pixel.Rhino.Render.RenderContent.ContentChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentUpdatePreviewEventCallback(Pixel.Rhino.Render.RenderContent.ContentUpdatePreviewCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentCurrencyChangedEventCallback(Pixel.Rhino.Render.RenderContent.CurrentContentChangedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);



  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetNewRdkDocumentEventCallback(Pixel.Rhino.RhinoApp.RhCmnOneUintCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetGlobalSettingsChangedEventCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetUpdateAllPreviewsEventCallback(Pixel.Rhino.RhinoApp.RhCmnOneUintCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetCacheImageChangedEventCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetRendererChangedEventCallback(Pixel.Rhino.RhinoApp.RhCmnEmptyCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);


  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryAddedEventCallback(Pixel.Rhino.Render.RenderContent.ContentTypeAddedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryDeletingEventCallback(Pixel.Rhino.Render.RenderContent.ContentTypeDeletingCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryDeletedEventCallback(Pixel.Rhino.Render.RenderContent.ContentTypeDeletedCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetClientPlugInUnloadingEventCallback(Pixel.Rhino.RhinoApp.ClientPlugInUnloadingCallback cb, Pixel.Rhino.Runtime.HostUtils.RdkReportCallback reportCb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSdkRenderCallback(Pixel.Rhino.Render.RenderPipeline.ReturnBoolGeneralCallback callbackFunc);

  // Docking Tabs in rh_utilities.cpp
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnRdkRenderPlugIn_RegisterCustomPlugInUi(
    RhRdkCustomUiType panelType,
    [MarshalAs(UnmanagedType.LPWStr)] string caption,
    Guid tabId,
    Guid pluginId,
    Guid renderEngineId,
    bool initialShow,
    bool alwaysShow,
    Pixel.Rhino.Render.RenderPanels.CreatePanelCallback createProc,
    Pixel.Rhino.Render.RenderPanels.VisiblePanelCallback visibleProc,
    Pixel.Rhino.Render.RenderPanels.DestroyPanelCallback destroyProc,
    Pixel.Rhino.Render.RenderPanels.SetControllerPanelCallback setcontrollerProc);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetAsyncRenderContextCallbacks(
    Pixel.Rhino.Render.AsyncRenderContext.DeleteThisCallback deleteThis,
    Pixel.Rhino.Render.AsyncRenderContext.StopRenderingCallback stopRendering);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_RenderFrame_SetCallbacks(
    Pixel.Rhino.RDK.Delegates.VOIDPROC delete,
    Pixel.Rhino.RDK.Delegates.BOOL_INTINPTRINTINT_PROC create,
    Pixel.Rhino.RDK.Delegates.BOOL_INT_PROC destroy,
    Pixel.Rhino.RDK.Delegates.VOID_INTBOOL_PROC SetVisibility,
    Pixel.Rhino.RDK.Delegates.BOOL_INT_PROC StartRendering,
    Pixel.Rhino.RDK.Delegates.BOOL_INT_PROC IsRendering,
    Pixel.Rhino.RDK.Delegates.VOIDPROC StopRendering,
    Pixel.Rhino.RDK.Delegates.VOID_GETINTINTINT_PROC Size,
    Pixel.Rhino.RDK.Delegates.VOID_SETINTINTINT_PROC SetImageSize,
    Pixel.Rhino.RDK.Delegates.VOID_INTBOOL_PROC Refresh,
    Pixel.Rhino.RDK.Delegates.SAVERENDERIMAGEASPROC SaveAs,
    Pixel.Rhino.RDK.Delegates.BOOL_INT_PROC CopyToClipboard,
    Pixel.Rhino.RDK.Delegates.PICKPOINTONRENDERIMAGEPROC PickPoint,
    Pixel.Rhino.RDK.Delegates.PICKRECTANGLEONRENDERIMAGEPROC PickRectangle);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnRdkRenderPlugIn_RegisterCustomDockBarTab(
    RhRdkCustomUiType panelType,
    [MarshalAs(UnmanagedType.LPWStr)] string caption,
    Guid tabId,
    Guid pluginId,
    Guid renderEngineId,
    IntPtr icon,
    Pixel.Rhino.Render.RenderPanels.CreatePanelCallback createProc,
    Pixel.Rhino.Render.RenderPanels.VisiblePanelCallback visibleProc,
    Pixel.Rhino.Render.RenderPanels.DestroyPanelCallback destroyProc,
    Pixel.Rhino.Render.RenderPanels.VisiblePanelCallback doHelpProc,
    Pixel.Rhino.Render.RenderPanels.SetControllerPanelCallback setcontrollerProc);

  #endregion

  // Docking Tabs in rh_utilities.cpp
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_RegisterTabbedDockBar(
    [MarshalAs(UnmanagedType.LPWStr)] string caption,
    Guid tabId,
    Guid plugInId,
    IntPtr image,
    bool hasDocContext,
    Pixel.Rhino.UI.PanelSystem.CreatePanelCallback createProc,
    Pixel.Rhino.UI.PanelSystem.StatePanelCallback visibleProc,
    Pixel.Rhino.UI.PanelSystem.StatePanelCallback onShowPanelProc);
#endif

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnUserData_SetCallbacks(Pixel.Rhino.DocObjects.Custom.UserData.TransformUserDataCallback xformFunc,
    Pixel.Rhino.DocObjects.Custom.UserData.ArchiveUserDataCallback archiveFunc,
    Pixel.Rhino.DocObjects.Custom.UserData.ReadWriteUserDataCallback readwriteFunc,
    Pixel.Rhino.DocObjects.Custom.UserData.DuplicateUserDataCallback duplicateFunc,
    Pixel.Rhino.DocObjects.Custom.UserData.CreateUserDataCallback createFunc,
    Pixel.Rhino.DocObjects.Custom.UserData.DeleteUserDataCallback deleteFunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_RTree_Search(IntPtr pConstRtree, Point3d pt0, Point3d pt1, int serialNumber, RTree.SearchCallback searchCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_RTree_SearchSphere(IntPtr pConstRtree, Point3d center, double radius, int serialNumber, RTree.SearchCallback searchCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_RTree_Search2(IntPtr pConstRtreeA, IntPtr pConstRtreeB, double tolerance, int serialNumber, RTree.SearchCallback searchCallback);

  //bool ON_Arc_Copy(ON_Arc* pRdnArc, ON_Arc* pRhCmnArc, bool rdnToRhc)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_Arc_Copy(IntPtr pRdnArc, ref Arc pRhCmnArc, [MarshalAs(UnmanagedType.U1)]bool rdnToRhc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void ON_ProgressReporter_SetReportCallback(Pixel.Rhino.ProgressReporter.ProgressReportCallback progressReportCallback);

#if RHINO_SDK
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnPersistentSettingHooks_SetHooks(Pixel.Rhino.PersistentSettingsHooks.CreateDelegate createDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SaveDelegate saveDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetKeysDelegate getKeysDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetPlugInPersistentSettingsPointerProc getPlugInPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetPlugInPersistentSettingsPointerProc getManagedPlugInPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetCommandPersistentSettingsPointerProc getCommandPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetChildPersistentSettingsPointerProc addCommandPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetChildPersistentSettingsPointerProc deleteCommandPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetChildPersistentSettingsPointerProc getChildPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetChildPersistentSettingsPointerProc deleteItemPersistentSettings,
                                                                    Pixel.Rhino.PersistentSettingsHooks.ReleasePlugInSettingsPointerProc releasePointerHook,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetStringDelegate setStringDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetIntegerDelegate setIntDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetUnsignedIntegerDelegate setUnsignedIntDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetDoubleDelegate setDoubleDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetIntegerDelegate setBoolDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetIntegerDelegate setHideDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.PersistentSettingsHiddenProc setPersistentSettingsHiddenDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetIntegerDelegate setColorDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetRectDelegate setRectDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetPointDelegate setPointDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetPointDelegate setSizeDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetStringListDelegate setStringListDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetStringDictionaryDelegate setStringDictionaryDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetGuidDelegate setGuidDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.SetPoint3DDelegate setPoint3DDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetStringDelegate getStringDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetIntegerDelegate getIntDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetUnsignedIntegerDelegate getUnsignedIntDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetDoubleDelegate getDoubleDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetIntegerDelegate getBoolDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetIntegerDelegate getHideDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.PersistentSettingsHiddenProc getPersistentSettingsHiddenDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetIntegerDelegate getColorDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetRectDelegate getRectDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetPointDelegate getPointDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetPointDelegate getSizeDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetStringListDelegate getStringListDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetStringDictionaryDelegate getStringDictionaryDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetGuidDelegate getGuidDelegate,
                                                                    Pixel.Rhino.PersistentSettingsHooks.GetPoint3DDelegate getPoint3DDelegate
                                                                   );

  #region rh_menu.cpp

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRuiOnUpdateMenuItems_SetHooks(Pixel.Rhino.UI.RuiOnUpdateMenuItems.OnUpdateMenuItemCallback onUpdateCallback);

  #endregion rh_menu.cpp

  #region rh_pages.cpp
  // IRhino...Page classes in rh_pages.cpp
  // Z:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\c\rh_pages.cpp line 108
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnPageBase_SetHooks(
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageGetStringDelegate getString,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageGetIconDelegate getIcon,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageWindowDelegate getWindow,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageMinimumSizeDelegate getMinSize,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageReleaseDelegate release,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageRefreshDelegate refresh,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageIntPtrDelegate hostCreated,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageIntIntDelegate sizeChanged,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageIntDelegate show,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageIntReturnsIntDelegate showHelp,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageIntReturnsIntDelegate activated,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPageIntReturnsHwndIntDelegate overrideSupressEnterEscape
  );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void IRhinoOptionsPage_SetHooks(
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageUintDelegate runScript,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageProcDelegate updatePage,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageButtonsToDisplayDelegate buttonToDisplay,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageIsButtonEnabled isButonEnabled,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageProcDelegate apply,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageProcDelegate cancel,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageProcDelegate restoreDefaults,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoOptionsPageProcDelegate attachedToUi
  );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void IRhinoPropertiesPanelPage_SetHooks(
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate includeInNavigation,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate runScript,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate updatePage,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate onModifyPage,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelPageTypeDelegate pageType,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate index,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate pageEvent,
    Pixel.Rhino.UI.RhinoPageHooks.RhinoPropertiesPanelDelegate supportsSubObjectSelection
  );

  #endregion rh_pages.cpp

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool CRhinoPrintInfo_VectorCapture(IntPtr constPrintInfo,
    Pixel.Rhino.Runtime.ViewCaptureWriter.SetClipRectProc clipRectProc,
    Pixel.Rhino.Runtime.ViewCaptureWriter.FillProc fillProc,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorPolylineProc polylineCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorArcProc arcCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorStringProc stringCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorPolylineProc bezCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorFillPolygonProc fillPolyCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorPathProc pathCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorPointProc pointCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorBitmapProc bitmapCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorRoundedRectProc roundedRectCallback,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorClipPathProc pathProc,
    Pixel.Rhino.Runtime.ViewCaptureWriter.VectorGradientProc gradientProc,
    uint docSerialNumber);


  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void SetRhCsInternetFunctionalityCallback(Pixel.Rhino.Render.InternalUtilities.RhCsDownloadFileProc downloadFileCallback, Pixel.Rhino.Render.InternalUtilities.RhCsUrlResponseProc urlResponseCallback);

  [DllImport (Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhCmn_PropertiesEditor_SetDisplayPageHook(Guid id, IntPtr proc);

  [DllImport (Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhCmn_PropertiesEditor_SetRemovePageHook(Guid id, IntPtr proc);

  [DllImport (Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhCmn_PropertiesEditor_SetLoadPagesHook (Guid id, IntPtr proc);

  [DllImport (Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhCmn_PropertiesEditor_SetLoadPlugInPagesHook(Guid id, IntPtr proc);

  [DllImport (Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhCmn_PropertiesEditor_SeIncludeInNavHook (Guid id, IntPtr proc);

  [DllImport (Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RhCmn_PropertiesEditor_SetActivePageHook (Guid id, IntPtr proc);

  #region rh_fileeventwatcher.cpp
  //void CRhCmnFileEventWatcherInterop_SetHooks(CREATEFILEWATCHERPROC createProc)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnFileEventWatcherInterop_SetHooks(
    Pixel.Rhino.RhinoFileEventWatcherHooks.AttachFileWatcherDelegate attach,
    Pixel.Rhino.RhinoFileEventWatcherHooks.AttachFileWatcherDelegate detach,
    Pixel.Rhino.RhinoFileEventWatcherHooks.FileWatcherWatchDelegate watch,
    Pixel.Rhino.RhinoFileEventWatcherHooks.FileWatcherEnableDelegate enable
  );
  #endregion

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_PostEffectUI_SetCallbacks(
    Pixel.Rhino.RDK.Delegates.PEP_UI_ADDSECTIONS_PROC addsection
  );
#endif
}
