using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*Additonals*/
using ServerHandler.SaverCore;

namespace ServerHandler
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			//Create File handler and init
			Core fileCore = new Core(@"D:\TmpFTP\Itron_1", @"D:\TmpFTP\Itron_1_local", "*.txt", "*.jpg", true);
			fileCore.TriggerFileFound += new TriggerFileFoundHandler(onReport);
		}

		private void onReport(returnValueType valType, object retObj)
		{
			if (notifier_textbox.InvokeRequired)
				notifier_textbox.BeginInvoke(new Action(() => { notifier_textbox.AppendText(retObj as string); }));
			else
				notifier_textbox.AppendText(retObj as string);
		}
	}
}
