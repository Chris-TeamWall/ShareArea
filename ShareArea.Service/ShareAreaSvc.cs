using ShareArea.WindowsUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using static Btm.Shared.WindowsUtils.SelfInstallService;

namespace ShareArea.Service
{
  partial class ShareAreaSvc : ServiceBase
  {

    private Process fUIProcess;
    private static object fLock = new object();
    private string fUserInteractionSystemFileName;

    private string UserInteractionSystemFileName
    {
      get
      {
        if (string.IsNullOrEmpty(fUserInteractionSystemFileName))
        {
          Assembly assembly = Assembly.GetExecutingAssembly();
          var appPath = Path.GetDirectoryName(assembly.Location);
          fUserInteractionSystemFileName = Path.Combine(appPath, "ShareArea.UI.exe");
        }
        return fUserInteractionSystemFileName;
      }
    }

    public ShareAreaSvc()
    {
      CanHandleSessionChangeEvent = true;
      InitializeComponent();
    }

    protected override void OnSessionChange(SessionChangeDescription changeDescription)
    {
      base.OnSessionChange(changeDescription);
      if (changeDescription.Reason == SessionChangeReason.ConsoleConnect)
      {
        RestartUI();
      }
    }

    private void RestartUI()
    {
      lock (fLock)
      {
        StopUI();
        StartUI();
      }
    }

    protected override void OnStart(string[] args)
    {
      // TODO: Add code here to start your service.
      Task.Run(StartUI);
    }

    protected override void OnStop()
    {
      StopUI();
    }

    public void StopUI()
    {
      if (fUIProcess != null)
      {
        try
        {
          fUIProcess.Kill();
        }
        catch
        {
          ProcessUtils.TerminateProcess(UserInteractionSystemFileName);
        }
      }
    }

    private void TerminateExistingUserInteractionProcess()
    {
      ProcessUtils.TerminateProcess(UserInteractionSystemFileName);
    }
    public void StartUI()
    {
      TerminateExistingUserInteractionProcess();

      UACUtils.StartProcessAndBypassUAC((int)UACUtils.WTSGetActiveConsoleSessionId(), UserInteractionSystemFileName, out UACUtils.PROCESS_INFORMATION procInfo);

      fUIProcess = System.Diagnostics.Process.GetProcessById((int)procInfo.dwProcessId);
      fUIProcess.Exited += UserInteractionServiceProcess_Exited;
      Task.Delay(500).Wait();
    }

    private void UserInteractionServiceProcess_Exited(object sender, EventArgs e)
    {
      fUIProcess.Exited -= UserInteractionServiceProcess_Exited;
      fUIProcess = null;
      StartUI();
    }

  }
}
