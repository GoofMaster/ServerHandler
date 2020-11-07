namespace ServerHandler
{
	partial class MainForm
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
			this.notifier_textbox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// notifier_textbox
			// 
			this.notifier_textbox.Location = new System.Drawing.Point(12, 12);
			this.notifier_textbox.Name = "notifier_textbox";
			this.notifier_textbox.Size = new System.Drawing.Size(751, 536);
			this.notifier_textbox.TabIndex = 0;
			this.notifier_textbox.Text = "";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1004, 560);
			this.Controls.Add(this.notifier_textbox);
			this.Name = "MainForm";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox notifier_textbox;
	}
}

