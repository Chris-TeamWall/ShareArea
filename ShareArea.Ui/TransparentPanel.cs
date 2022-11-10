using System.Drawing;
using System.Windows.Forms;

namespace ShareArea.UI
{
  public class TransparentPanel : Panel
  {
    protected override void OnPaintBackground(PaintEventArgs e)
    {
      e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(5, Color.Gray)), ClientRectangle);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      var c = Color.FromArgb(245, 126, 130);
      e.Graphics.DrawLine(new Pen(c, 2), 0, 0, Width, 0);
      e.Graphics.DrawLine(new Pen(c,2), 0, (Height/2) -1, Width, (Height / 2) - 1);
      e.Graphics.DrawLine(new Pen(c, 2), 0, Height - 2, Width, Height - 2);
    }
  }
}
