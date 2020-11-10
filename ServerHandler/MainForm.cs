using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*Additonals*/
using ServerHandler.SaverCore;
using System.ConfigFile;
using System.Diagnostics.Eventing.Reader;

namespace ServerHandler
{
	public partial class MainForm : Form
	{
/*<!--------------- Class Locals ---------------!>*/
		private string appLocal = AppDomain.CurrentDomain.BaseDirectory;		//Application local directory
		private string endl = Environment.NewLine;      //New line variable
		private Dictionary<string, Core> serversList;
		private FileStream fileDrv;
		private StreamWriter textFile;
		private Timer clockTimer;

		public MainForm()
		{
			InitializeComponent();

			//Initialize server list
			serversList = new Dictionary<string, Core>();
			initializeServiceList();

			//Start clock system
			clockTimer = new Timer()
			{ Interval = 1000 };
			clockTimer.Tick += new EventHandler(clockTimer_OnTick);
			clockTimer.Enabled = true;
			clockTimer.Start();

			//Application general Event handlers
			FormClosing += new FormClosingEventHandler(MainForm_OnFormClosing);
			FormClosed += new FormClosedEventHandler(MainForm_OnFormClosed);
		}

		private void initializeServiceList()
		{
			//Locals
			Core serverConfig;
			TableLayoutPanel tablePan;
			PictureBox listIcon;
			int nameCounter = 1;

			//Recover list of servers from ini file
			ConfigFileAdmin cfgReader = new ConfigFileAdmin(Path.Combine(appLocal, "ServerHandler_config.ini"), false);
			string[] sectionNames = cfgReader.GetSectionNames();
			string Temporal;

			//Configure Log file
			string logFileRoute = Path.Combine(appLocal, "Logs", DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".log");

			fileDrv = new FileStream(logFileRoute, File.Exists(logFileRoute) ? FileMode.Append : FileMode.OpenOrCreate, FileAccess.Write);
			textFile = new StreamWriter(fileDrv, Encoding.ASCII)
			{ AutoFlush = true, NewLine = endl, };
			textFile.WriteLine("***************************** Server Handler LOG *****************************" + endl + "Application start at: " + DateTime.Now.ToString() + endl);

			foreach (string sectionName in sectionNames)
			{
				//Get data from config position
				cfgReader.ReadKeyFromConfig(sectionName, "Folder_TempDir", out string searchDir);       //Search directory
				cfgReader.ReadKeyFromConfig(sectionName, "Folder_TargetDir", out string targetDir);       //Target directory
				cfgReader.ReadKeyFromConfig(sectionName, "Extension_TriggerFileExtension", out string targetFileExtension);       //Trigger file extension
				cfgReader.ReadKeyFromConfig(sectionName, "Extension_ImageFileExtension", out string imgFileExtension);       //Image file extension to search.

				//Create base table panel
				tablePan = new TableLayoutPanel()
				{
					Name = "paneContainer_" + sectionName + "_" + nameCounter.ToString(),
					Size = new Size(500, 74),
					AutoSizeMode = AutoSizeMode.GrowOnly,
					RowCount = 1,
					ColumnCount = 2,
					AutoSize = false,
					Location = new Point(10, (nameCounter - 1)*74),
					CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset
				};
				Temporal = "Station Name: " + sectionName + endl + "Search Directory: " + searchDir + endl + "Target Directory: " + targetDir + endl + "Trigger file Extension: " + targetFileExtension + endl +
					"Image Extension: " + imgFileExtension;

				//Create image from icon file.
				listIcon = new PictureBox()
				{ Name = "iconList_" + sectionName + "_" + nameCounter.ToString(), SizeMode = PictureBoxSizeMode.AutoSize };
				listIcon.Image = Image.FromFile(Path.Combine(appLocal, "data", "folder_64px.png"), true);
				//Add control to Server list
				tablePan.Controls.Add(listIcon);
				tablePan.Controls.Add(new Label() { Name = "Label_" + sectionName + "_" + nameCounter.ToString(), Text = Temporal, AutoSize = true });
				serverList_cont.Controls.Add(tablePan);

				//Initialize File Server instance
				serverConfig = new Core(sectionName, searchDir, targetDir, targetFileExtension, imgFileExtension, true);
				serverConfig.TriggerFileFound += new TriggerFileFoundHandler(onReport);
				serverConfig.OnServerError += new ErrorOnServerHandler(onServerError);
				serverConfig.InitializeFileServer();
				serversList.Add(sectionName, serverConfig);

				nameCounter++;
			}
			return;
		}

/*<!--------------- Event Handlers ---------------!>*/
		/// <summary>Trigger event when form is about to close.</summary>
		/// <param name="sender"></param>
		/// <param name="ex"></param>
		private void MainForm_OnFormClosing(object sender, FormClosingEventArgs ex)
		{
			textFile.WriteLine(DateTime.Now.ToString() + ": an attempt is made to close the application...");
			if(MessageBox.Show("Se va a cerrar la aplicacion de servidor, desea cerrarla?", "Cierre de aplicacion...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{ textFile.WriteLine("Application is shutting down..."); }
			else
			{ ex.Cancel = true; }
		}

		/// <summary>Trigger when application closes.</summary>
		/// <param name="sender"></param>
		/// <param name="ex"></param>
		private void MainForm_OnFormClosed(object sender, FormClosedEventArgs ex)
		{
			textFile.WriteLine(DateTime.Now.ToString() + ": Application is closing..." + endl + "Unloading server resources...");
			textFile.Close();
			fileDrv.Close();
		}

		/// <summary>Event used to show time</summary>
		/// <param name="sender"></param>
		/// <param name="ex"></param>
		private void clockTimer_OnTick(object sender, EventArgs ex )
		{
			timeLabel.Text = DateTime.Now.ToString();
		}

		/// <summary>Catch when File system reports data to Main HMI.</summary>
		/// <param name="valType">Return value type to catch</param>
		/// <param name="retObj">Object returned from filesystem</param>
		private void onReport(returnValueType valType, object retObj)
		{
			if (notifier_textbox.InvokeRequired)
				switch(valType)
				{
					case returnValueType.getString:
						notifier_textbox.BeginInvoke(new Action(() =>
						{
							notifier_textbox.AppendText(retObj as string);
							notifier_textbox.SelectionStart = notifier_textbox.TextLength;
							notifier_textbox.ScrollToCaret();
						}));
						break;
				}
			else
			{
				notifier_textbox.AppendText(retObj as string);
				notifier_textbox.SelectionStart = notifier_textbox.TextLength;
				notifier_textbox.ScrollToCaret();
			}

			//Store data to log file
			textFile.Write(retObj as string);
		}

		/// <summary>Catch when File system reports error to Main HMI.</summary>
		/// <param name="valType">Return value type to catch</param>
		/// <param name="retObj">Object returned from filesystem</param>
		private void onServerError(returnValueType valType, object retObj)
		{
			switch (valType)
			{
				case returnValueType.getString:
					notifier_textbox.BeginInvoke(new Action(() =>
					{
						notifier_textbox.SelectionStart = notifier_textbox.TextLength;
						notifier_textbox.SelectionLength = 0;
						notifier_textbox.SelectionColor = Color.Red;
						notifier_textbox.AppendText(retObj as string);
						notifier_textbox.SelectionStart = notifier_textbox.TextLength;
						notifier_textbox.SelectionColor = Color.Black;
						notifier_textbox.ScrollToCaret();
					}));
					break;
			}

			//Store data to log file
			textFile.Write(retObj as string);
		}
	}
}
