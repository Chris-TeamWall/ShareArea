using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareArea.UI
{
  public partial class ShareForm : Form
  {
    private bool fMouseDown;
    private Point fLastLocation;
    private bool fShareAreaShown;
    public Task CursorTask { get; private set; }
    private bool fInWindowFinderMode;
    private Rectangle fInitialBounds;


    public ShareForm()
    {
      InitializeComponent();
      SetStyle(ControlStyles.ResizeRedraw, true);
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      fShareAreaShown = true;
      var s = Screen.FromPoint(Cursor.Position);
      SetDesktopLocation(s.WorkingArea.X + 50, s.WorkingArea.Y + 50);
      StartCursorTask();
    }

    private void StartCursorTask()
    {
      CursorTask = Task.Run(() =>
      {
        while (fShareAreaShown)
        {
          Invoke(() =>
          {
            //Keep Window On Top -> ContextMenus will place over the current top most window, so bring our window again to the top
            NativeFunctions.SetWindowPos(this.Handle, NativeFunctions.HWND_TOPMOST, 0, 0, 0, 0, NativeFunctions.TOPMOST_FLAGS);
          });

          if (fInWindowFinderMode)
          {
            Invoke(() =>
            {
              SetShareBounds(Cursor.Position);
            });
          }
          Application.DoEvents();
        }
      });
    }

    private void SetShareBounds(Point p)
    {
      IntPtr hWnd = NativeFunctions.WindowFromPoint(p);
      if (hWnd != IntPtr.Zero)
      {
        var res = NativeFunctions.GetWindowThreadProcessId(hWnd, out uint processId);
        if ((processId > 0) && (processId != Process.GetCurrentProcess().Id))
        {
          IntPtr parent;
          while ((parent = NativeFunctions.GetParent(hWnd)) != IntPtr.Zero)
          {
            hWnd = parent;
          }
          var wHandle = hWnd;
          var taskBarHandle = NativeFunctions.FindWindow("Shell_traywnd", "");
          var secondTaskBarHandle = NativeFunctions.FindWindow("Shell_SecondaryTrayWnd", "");

          if ((wHandle != taskBarHandle) && (wHandle != secondTaskBarHandle))
          {
            if (NativeFunctions.GetWindowRect(wHandle, out var rect))
            {
              var placement = NativeFunctions.GetPlacement(wHandle);
              if (placement.showCmd == NativeFunctions.ShowWindowCommands.Maximized)
              {
                SetDesktopBounds(rect.Left + 8, rect.Top + 8, rect.Right - rect.Left - 16, rect.Bottom - rect.Top - 14);
              }
              else
              {
                SetDesktopBounds(rect.Left + 2, rect.Top, rect.Right - rect.Left - 4, rect.Bottom - rect.Top - 4);
              }
            }
          }
        }
      }
    }


    protected override void OnClosed(EventArgs e)
    {
      fShareAreaShown = false;
      base.OnClosed(e);
    }

    private void Form_Shown(object sender, EventArgs e)
    {
      AnchorPanel.BackColor = Color.FromArgb(25, Color.Black); ;
      NativeFunctions.SetWindowPos(this.Handle, NativeFunctions.HWND_TOPMOST, 0, 0, 0, 0, NativeFunctions.TOPMOST_FLAGS);
      NativeFunctions.SetWindowText(this.Handle, "Share Area");
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      //Draw Border
      var borderColor = Color.Red;
      var borderStyle = ButtonBorderStyle.Solid;
      var borderWidth = 3;

      ControlPaint.DrawBorder(
                          e.Graphics,
                          ClientRectangle,
                          borderColor,
                          borderWidth,
                          borderStyle,
                          borderColor,
                          borderWidth,
                          borderStyle,
                          borderColor,
                          borderWidth,
                          borderStyle,
                          borderColor,
                          borderWidth,
                          borderStyle);

    }


    #region custom resize handling
    protected override void WndProc(ref Message m)
    {
      const int htLeft = 10;
      const int htRight = 11;
      const int htTop = 12;
      const int htTopLeft = 13;
      const int htTopRight = 14;
      const int htBottom = 15;
      const int htBottomLeft = 16;
      const int htBottomRight = 17;
      const int WM_NCHITTEST = 0x84;
      //Debug.WriteLine(m.Msg);
      if (m.Msg == WM_NCHITTEST)
      {
        int x = (int)(m.LParam.ToInt64() & 0xFFFF);
        int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);
        Point pt = PointToClient(new Point(x, y));
        Size clientSize = ClientSize;
        ///allow resize on the lower right corner
        if (pt.X >= clientSize.Width - 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
          return;
        }
        ///allow resize on the lower left corner
        if (pt.X <= 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
          return;
        }
        ///allow resize on the upper right corner
        if (pt.X <= 16 && pt.Y <= 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
          return;
        }
        ///allow resize on the upper left corner
        if (pt.X >= clientSize.Width - 16 && pt.Y <= 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
          return;
        }
        ///allow resize on the top border
        if (pt.Y <= 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(htTop);
          return;
        }
        ///allow resize on the bottom border
        if (pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(htBottom);
          return;
        }
        ///allow resize on the left border
        if (pt.X <= 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(htLeft);
          return;
        }
        ///allow resize on the right border
        if (pt.X >= clientSize.Width - 16 && clientSize.Height >= 16)
        {
          m.Result = (IntPtr)(htRight);
          return;
        }
        m.Result = (IntPtr)(-1);
        return;
      }
      base.WndProc(ref m);
    }

    #endregion custom resize handling

    #region Anchor handling

    private void Anchor_MouseDown(object sender, MouseEventArgs e)
    {
      fInitialBounds = Bounds;
      fMouseDown = true;
      fLastLocation = e.Location;
      if (ModifierKeys == Keys.Control)
      {
        fInWindowFinderMode = true;
        AnchorPanel.Visible = false;
      }
    }

    private void Anchor_MouseMove(object sender, MouseEventArgs e)
    {
      if (fMouseDown)
      {
        if (ModifierKeys == Keys.Control)
        {
          fInWindowFinderMode = true;
          AnchorPanel.Visible = false;
        }
        else
        {
          fInWindowFinderMode = false;
          AnchorPanel.Visible = true;
        }
        if (fInWindowFinderMode == false)
        {
          Width = fInitialBounds.Width;
          Height = fInitialBounds.Height;
          Location = new Point(
              (Location.X - fLastLocation.X) + e.X, (Location.Y - fLastLocation.Y) + e.Y);

          Update();
        }
      }
    }

    private void Anchor_MouseUp(object sender, MouseEventArgs e)
    {
      fInWindowFinderMode = false;
      fMouseDown = false;
      AnchorPanel.Visible = true;
    }

    private void ShareForm_MouseMove(object sender, MouseEventArgs e)
    {
      Debug.WriteLine("Moving");
    }

    private void Anchor_MouseCaptureChanged(object sender, EventArgs e)
    {
      if (fInWindowFinderMode || fMouseDown)
      {
        fInWindowFinderMode = false;
        fMouseDown = false;
        AnchorPanel.Visible = true;
      }
    }

    private void Anchor_DoubleClick(object sender, EventArgs e)
    {
      var po = new Point(Cursor.Position.X, Cursor.Position.Y + 15);
      SetShareBounds(po);
    }
    #endregion
  }
}