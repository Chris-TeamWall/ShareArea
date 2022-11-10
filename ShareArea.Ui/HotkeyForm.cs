using System.Drawing;
using System.Windows.Forms;

namespace ShareArea.UI
{
  public partial class HotkeyForm : Form
  {
    private HotKeyManager fHk;
    private ShareForm fShareForm;
    public HotkeyForm()
    {
      InitializeComponent();
      FormBorderStyle = FormBorderStyle.FixedToolWindow;
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.Manual;
      Location = new Point(-2000, -2000);
      Size = new Size(1, 1);
      fHk = new HotKeyManager(this);
      fHk.OwnerForm = this;
      fHk.HotKeyPressed += HotKeyPressed;
      fHk.AddHotKey(Keys.A, HotKeyManager.MODKEY.MOD_WIN | HotKeyManager.MODKEY.MOD_ALT, "ShowForm");
    }

    private void HotKeyPressed(string hotKeyID)
    {
      if (string.Compare(hotKeyID, "ShowForm", true) == 0)
      {
        if (fShareForm != null)
        {
          fShareForm.Close();
          return;
        }
        else
        {
          using (fShareForm = new ShareForm())
          {
            fShareForm.Location = Cursor.Position;
            fShareForm.ShowDialog();
          }
          fShareForm = null;
        }
      }
    }
  }
}
