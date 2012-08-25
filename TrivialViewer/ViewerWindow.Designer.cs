namespace TrivialViewer
{
	partial class ViewerWindow
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ViewerWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(185, 68);
			this.Name = "ViewerWindow";
			this.Text = "Form1";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.onPaint);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.onKeyDown);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.onMouseClick);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.onMouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.onMouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.onMouseUp);
			this.ResumeLayout(false);

		}

		#endregion

	}
}

