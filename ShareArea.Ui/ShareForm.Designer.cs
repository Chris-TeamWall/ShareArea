namespace ShareArea.UI
{
  partial class ShareForm
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.AnchorPanel = new ShareArea.UI.TransparentPanel();
      this.SuspendLayout();
      // 
      // AnchorPanel
      // 
      this.AnchorPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.AnchorPanel.Location = new System.Drawing.Point(411, 5);
      this.AnchorPanel.Name = "AnchorPanel";
      this.AnchorPanel.Size = new System.Drawing.Size(71, 10);
      this.AnchorPanel.TabIndex = 0;
      this.AnchorPanel.DoubleClick += new System.EventHandler(this.Anchor_DoubleClick);
      this.AnchorPanel.MouseCaptureChanged += new System.EventHandler(this.Anchor_MouseCaptureChanged);
      this.AnchorPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Anchor_MouseDown);
      this.AnchorPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Anchor_MouseMove);
      this.AnchorPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Anchor_MouseUp);
      // 
      // ShareForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(892, 520);
      this.Controls.Add(this.AnchorPanel);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.Name = "ShareForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = " Share Area";
      this.TransparencyKey = System.Drawing.SystemColors.Control;
      this.Shown += new System.EventHandler(this.Form_Shown);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ShareForm_MouseMove);
      this.ResumeLayout(false);

    }

    #endregion

    private TransparentPanel AnchorPanel;
  }
}