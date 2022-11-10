using Btm.Shared.WindowsUtils;

namespace ShareArea.Service
{
  internal class Program
  {
    static void Main(string[] args)
    {
      SelfInstallService.HandleStartup<ShareAreaSvc>("ShareAreaService", args);
    }
  }
}