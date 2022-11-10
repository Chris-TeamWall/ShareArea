using ShareArea.WindowsUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ShareArea.Service
{
  [RunInstaller(true)]
  public partial class ShareServiceInstaller : System.Configuration.Install.Installer
  {
    public ShareServiceInstaller()
    {
      InitializeComponent();
    }

    private void shareAreaSvcInstaller_AfterInstall(object sender, InstallEventArgs e)
    {
      List<SC_ACTION> recoveryActions = new List<SC_ACTION>();
      recoveryActions.Add(new SC_ACTION()
      {
        Delay = 60000,
        Type = (int)SC_ACTION_TYPE.RestartService,
      });
      recoveryActions.Add(new SC_ACTION()
      {
        Delay = 60000,
        Type = (int)SC_ACTION_TYPE.RestartService,
      });
      recoveryActions.Add(new SC_ACTION()
      {
        Delay = 120000,
        Type = (int)SC_ACTION_TYPE.RestartService,
      });
      ServiceRecoveryProperty.ChangeRecoveryProperty("ShareAreaService", recoveryActions, 60 * 60 * 24, null, false, "reboot requested by share area");
    }
  }
}
