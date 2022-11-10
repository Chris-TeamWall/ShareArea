using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using Microsoft.Win32.TaskScheduler;
using System.ComponentModel;
using System.IO;

namespace Btm.Shared.WindowsUtils
{
  public static class SelfInstallService
  {

    public static void HandleStartup<T>(string serviceName, string[] args) where T : ServiceBase, new()
    {
      string par = string.Concat(args);
      if ((System.Environment.UserInteractive) || (string.Compare(par, "--delayedinstall", true) == 0))
      {
        Console.WriteLine($"Your want to: {par}");
        switch (par)
        {
          case "--run":
          {
            foreach (var svc in ServiceController.GetServices())
            {
              if (string.Compare(svc.ServiceName, serviceName, true) == 0)
              {
                svc.Start();
                break;
              }
            }
            break;
          }
          case "--stop":
          {
            foreach (var svc in ServiceController.GetServices())
            {
              if (string.Compare(svc.ServiceName, serviceName, true) == 0)
              {
                if (svc.Status == ServiceControllerStatus.Running)
                {
                  svc.Stop();
                }
                break;
              }
            }
            break;
          }
          case "--delayedinstall":
          {
            try
            {
              var localFilePath = Assembly.GetExecutingAssembly().Location;
              ManagedInstallerClass.InstallHelper(new string[] { localFilePath });
              DeleteInstallTask();
            }
            catch
            {

            }
            foreach (var svc in ServiceController.GetServices())
            {
              if (string.Compare(svc.ServiceName, serviceName, true) == 0)
              {
                svc.Start();
                break;
              }
            }
            break;
          }
          case "--install":
          {
            var localFilePath = Assembly.GetExecutingAssembly().Location;
            ManagedInstallerClass.InstallHelper(new string[] { localFilePath });
            break;
          }
          case "--uninstall":
          {
            var localFilePath = Assembly.GetExecutingAssembly().Location;
            try
            {
              foreach (var svc in ServiceController.GetServices())
              {
                if (string.Compare(svc.ServiceName, serviceName, true) == 0)
                {
                  svc.Stop();
                  svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                  break;
                }
              }
            }
            catch
            {

            }
            ProcessUtils.TerminateProcess(localFilePath);
            try
            {
              ManagedInstallerClass.InstallHelper(new string[] { "/u", localFilePath });
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error during uninstall: {ex.Message}");
            }
            break;
          }
          default:
          {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Invalid Parameter.");
            Console.Error.WriteLine("Please use:");
            Console.Error.WriteLine($"--install\tInstalls the {serviceName}");
            Console.Error.WriteLine($"--uninstall\tUninstalls the {serviceName}");
            Console.ForegroundColor = currentColor;
            break;
          }
        }
      }
      else
      {
        ServiceBase[] ServicesToRun;
        ServicesToRun = new ServiceBase[]
            {
                new T()
            };
        ServiceBase.Run(ServicesToRun);
      }
    }

    private static void DeleteInstallTask()
    {
      var temp = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
      using (var svc = new TaskService())
      {
        var tasks = svc.RootFolder.Tasks.Where((x) => x.Name == $"{temp}_Install");
        foreach (var task in tasks)
        {
          svc.RootFolder.DeleteTask(task.Name, false);
        }
      }
    }

    internal class ProcessInfo
    {

      /// <summary>
      /// The unique identifier of the process. Every process gets an PID
      /// assigned when started.
      /// </summary>
      public int ID { get; set; }

      /// <summary>
      /// The fully qualified path of the process image.
      /// </summary>
      public string Path { get; set; }

      /// <summary>
      /// The file the process was created from.
      /// </summary>
      public string File
      {
        get { return System.IO.Path.GetFileName(Path); }
      }

      /// <summary>
      /// The directory the file of the process resides in.
      /// </summary>
      public string Directory
      {
        get { return System.IO.Path.GetDirectoryName(Path); }
      }

    }


    /// <summary>
    /// Allows you to get information about running processes and offers
    /// you the ability to terminate them.
    /// </summary>
    internal static class ProcessUtils
    {
      #region API declarations



      #endregion

      #region Functionality

      /// <summary>
      /// Fetch all processes running on the device.
      /// </summary>
      /// <returns>The processes running on the device</returns>
      public static ProcessInfo[] GetProcesses()
      {
        var list = new List<ProcessInfo>();
        var currentProcess = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcesses())
        {
          try
          {
            if (currentProcess.Id == process.Id)
            {
              continue;
            }
            string fullPath = process.MainModule.FileName;

            list.Add(new ProcessInfo()
            {
              ID = process.Id,
              Path = fullPath
            });
          }
          catch
          {
            // access denied
          }
        }


        return list.ToArray();
      }

      /// <summary>
      /// Returns the processes which were started from the given file.
      /// </summary>
      /// <param name="path">The full path of the processes to receive</param>
      /// <returns>The processes started from the given path, if any</returns>
      public static ProcessInfo[] GetProcessesByPath(string path)
      {
        var list = new List<ProcessInfo>();

        foreach (var process in GetProcesses())
        {
          if (string.Compare(process.Path, path, true) == 0)
          {
            list.Add(process);
          }
        }

        return list.ToArray();
      }

      /// <summary>
      /// Checks, whether a process started from the given path does exist or not.
      /// </summary>
      /// <param name="path">The full path to the process</param>
      /// <returns><c>true</c>, if a process for the given file is currently running</returns>
      public static bool ProcessRunning(string path)
      {
        return (GetProcessesByPath(path).Length > 0);
      }

      /// <summary>
      /// Tries to terminate the process started from the given path.
      /// </summary>
      /// <param name="path">The fully qualified path to the process</param>
      public static void TerminateProcess(string path)
      {
        foreach (var process in GetProcessesByPath(path))
        {
          try
          {
            using (var systemProcess = Process.GetProcessById(process.ID))
            {
              try
              {
                systemProcess.Kill();
              }
              catch (Win32Exception)
              {
                // the process has already terminated itself (race condition)
              }

              // wait for the process to finally be terminated
              // required, because kill works asynchronously
              if (systemProcess.WaitForExit(10000) == false)
              {
                throw new InvalidOperationException(string.Format("Unable to terminate process <{0}>.", path));
              }
            }
          }
          catch (ArgumentException)
          {
            // the process is no longer running (race condition)
          }
        }
      }

      #endregion

    }

  }
}
