namespace ShareArea.Service
{
  partial class ShareServiceInstaller
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.shareAreaSvcInstaller = new System.ServiceProcess.ServiceInstaller();
      this.shareAreaSvcProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
      // 
      // soviaManagementSvcInstaller
      // 
      this.shareAreaSvcInstaller.Description = "ShareArea Service";
      this.shareAreaSvcInstaller.DisplayName = "ShareAreaService";
      this.shareAreaSvcInstaller.ServiceName = "ShareAreaService";
      this.shareAreaSvcInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
      this.shareAreaSvcInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.shareAreaSvcInstaller_AfterInstall);
      // 
      // soviaManagementServiceProcessInstaller
      // 
      this.shareAreaSvcProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
      this.shareAreaSvcProcessInstaller.Password = null;
      this.shareAreaSvcProcessInstaller.Username = null;
      this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.shareAreaSvcInstaller,
            this.shareAreaSvcProcessInstaller});
    }

    #endregion

    private System.ServiceProcess.ServiceInstaller shareAreaSvcInstaller;
    private System.ServiceProcess.ServiceProcessInstaller shareAreaSvcProcessInstaller;

  }
}