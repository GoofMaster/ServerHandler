using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*Additionals*/
using System.IO;
using System.Security.Permissions;
using System.Runtime.Remoting.Messaging;
using System.Threading;

/// <summary>File renamer and saver Class Controller</summary>
namespace ServerHandler.SaverCore
{
	public enum returnValueType
	{ getString, getInteger, getBool, getDouble };

/*<!-------------------- Public delegates (For use with Events) --------------------!>*/
	public delegate void TriggerFileFoundHandler(returnValueType valType, object sender);

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public partial class Core
	{
/*<!-------------------- Internal class variables --------------------!>*/
		/// <summary>Set to true when Server is configured.</summary>
		private bool _configured;
		/// <summary>Target and destiny route strings.</summary>
		private string _targetRoute, _destinyRoute, _triggerFileExtension, _imgFileExtension, endl;
		/// <summary>Dinamic enumerator for file assignation.</summary>
		IEnumerable<string> fileList;

		/// <summary>Directory Watcher control classes.</summary>
		FileSystemWatcher fileWatcher;

/*<!-------------------- Event Handlers --------------------!>*/
		public event TriggerFileFoundHandler TriggerFileFound;

		public Core(string targetRoute, string destinyRoute, string triggerFileExtension, string imgFileExtension, bool useTextFileForSerials)
		{
			//Assign locals
			_targetRoute = targetRoute;
			_destinyRoute = destinyRoute;
			_triggerFileExtension = triggerFileExtension;
			_imgFileExtension = imgFileExtension;
			endl = Environment.NewLine;

			//Try to open monitor port
			try
			{
				//Initialize UUT dictionary locals
				uutParameters = new Dictionary<string, string>();

				//Initialize target route monitor system
				fileWatcher = new FileSystemWatcher(_targetRoute, _triggerFileExtension);
				fileWatcher.Filter = _triggerFileExtension;
				fileWatcher.Path = _targetRoute;
				fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
				fileWatcher.Created+= new FileSystemEventHandler(fSystem_OnChanged);   //OnCreated Event handler
				fileWatcher.EnableRaisingEvents = true;
				//Initialize image monitoring variables
				fileList = Directory.EnumerateFiles(_targetRoute, _imgFileExtension);
			}
			catch(IOException ex)
			{ }
		}

/*<------ FileSystemWatcher Events ------>*/
		/// <summary>Call this event each time that FileMonitor detects trigger file.</summary>
		/// <param name="sender"></param>
		/// <param name="Ex"></param>
		private void fSystem_OnChanged(object sender, FileSystemEventArgs Ex)
		{
			//Locals
			string Verbose, destDir, tmpFullName, tmpImgName;
			string serialNum, testDate, stationName;
			int imgCounter = 1;

			Thread.Sleep(200);
			GetParamsFromFile(Ex.FullPath);     //Get serial and UUT info
			destDir = Path.Combine(new string[] { _destinyRoute, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString() });

			//Create folder if does not exist
			if (!Directory.Exists(destDir))
				Directory.CreateDirectory(destDir);

			//Get station and serial parameters
			uutParameters.TryGetValue("Serial", out serialNum);
			uutParameters.TryGetValue("Test_Date", out testDate);
			uutParameters.TryGetValue("Station", out stationName);
			testDate = testDate.Trim().Replace('/', '-').Replace(':', '-').Replace(' ', '_').Replace('.', '-');
			stationName = stationName.Trim().Replace(' ', '-');

			//Proccesate each picture in folder
			foreach (string imgFile in fileList)
			{
				//Get station and serial parameters
				tmpImgName = imgFile;

				//Compose full file name
				tmpFullName = Path.Combine(new string[] { destDir, serialNum + "_" + testDate + "_" + stationName + "_img_" + imgCounter.ToString() + Path.GetExtension(tmpImgName) });
				File.Copy(tmpImgName, tmpFullName, true);	//Copy photo to backup directory
				File.Delete(tmpImgName);	//Delete photo from target directory
				imgCounter++;
			}

			//TriggerFileFound?.BeginInvoke(returnValueType.getString, tmpData, EndAsyncEvent, null);
			uutParameters.Clear();		//Clear parameters from dictionary
			File.Delete(Ex.FullPath);	//Delete info file
			//
		}

		/// <summary>Async Callback designed to call TriggerFileDound event.</summary>
		/// <param name="IAr"></param>
		private void EndAsyncEvent(IAsyncResult IAr)
		{
			TriggerFileFoundHandler eventHandler = (TriggerFileFoundHandler)((AsyncResult)IAr).AsyncDelegate;

			try
			{ eventHandler.EndInvoke(IAr); }
			catch
			{ }
		}
	}
}
