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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.notifier_textbox = new System.Windows.Forms.RichTextBox();
			this.serverList_cont = new System.Windows.Forms.ContainerControl();
			this.timeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// notifier_textbox
			// 
			this.notifier_textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.notifier_textbox.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			this.notifier_textbox.Location = new System.Drawing.Point(559, 12);
			this.notifier_textbox.Name = "notifier_textbox";
			this.notifier_textbox.ReadOnly = true;
			this.notifier_textbox.Size = new System.Drawing.Size(927, 778);
			this.notifier_textbox.TabIndex = 0;
			this.notifier_textbox.Text = "";
			this.notifier_textbox.UseWaitCursor = true;
			// 
			// serverList_cont
			// 
			this.serverList_cont.AutoScroll = true;
			this.serverList_cont.Location = new System.Drawing.Point(12, 12);
			this.serverList_cont.Name = "serverList_cont";
			this.serverList_cont.Size = new System.Drawing.Size(541, 778);
			this.serverList_cont.TabIndex = 3;
			this.serverList_cont.Text = "containerControl1";
			// 
			// timeLabel
			// 
			this.timeLabel.AutoSize = true;
			this.timeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.timeLabel.Location = new System.Drawing.Point(12, 801);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(73, 15);
			this.timeLabel.TabIndex = 4;
			this.timeLabel.Text = "Starting....";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1498, 825);
			this.Controls.Add(this.timeLabel);
			this.Controls.Add(this.serverList_cont);
			this.Controls.Add(this.notifier_textbox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "Servidor de organizacion de imagenes para AOI\'s";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox notifier_textbox;
		private System.Windows.Forms.ContainerControl serverList_cont;
		private System.Windows.Forms.Label timeLabel;
	}
}

