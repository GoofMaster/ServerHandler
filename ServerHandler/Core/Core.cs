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
using System.Windows.Forms;

/// <summary>File renamer and saver Class Controller</summary>
namespace ServerHandler.SaverCore
{
	public enum returnValueType
	{ getString, getInteger, getBool, getDouble };

/*<!-------------------- Public delegates (For use with Events) --------------------!>*/
	public delegate void TriggerFileFoundHandler(returnValueType valType, object sender);
	public delegate void ErrorOnServerHandler(returnValueType valType, object sender);

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public partial class Core
	{
/*<!-------------------- Internal class variables --------------------!>*/
		/// <summary>Set to true when Server is configured.</summary>
		private bool _configured;
		/// <summary>Target and destiny route strings.</summary>
		private string _targetRoute, _destinyRoute, _triggerFileExtension, _imgFileExtension, _stationName, endl;
		/// <summary>Dinamic enumerator for file assignation.</summary>
		IEnumerable<string> fileList;

		/// <summary>Directory Watcher control classes.</summary>
		FileSystemWatcher fileWatcher;

/*<!-------------------- Event Handlers --------------------!>*/
		public event TriggerFileFoundHandler TriggerFileFound;
		public event ErrorOnServerHandler OnServerError;

		/// <summary>Setup File server</summary>
		/// <param name="stationName">Station Name</param>
		/// <param name="targetRoute">Temporal folder route</param>
		/// <param name="destinyRoute">Target folder route</param>
		/// <param name="triggerFileExtension">When file extension detected, trigger image store</param>
		/// <param name="imgFileExtension">Image extension to search</param>
		/// <param name="useTextFileForSerials">If true, use trigger file to store station parameters.</param>
		public Core(string stationName, string targetRoute, string destinyRoute, string triggerFileExtension, string imgFileExtension, bool useTextFileForSerials)
		{
			//Assign locals
			_stationName = stationName;
			_targetRoute = targetRoute;
			_destinyRoute = destinyRoute;
			_triggerFileExtension = triggerFileExtension;
			_imgFileExtension = imgFileExtension;
			endl = Environment.NewLine;
		}

		public void InitializeFileServer()
		{
			//Try to open monitor port
			try
			{
				ReportStringData("**************************************************************" + endl + "Inicializando servidor de archivos para " + _stationName + endl);
				ReportStringData("   Estacion: " + _stationName + endl + "   Ruta de imagenes: " + _targetRoute + endl + "   Ruta de destino: " + _destinyRoute + endl);
				//Initialize UUT dictionary locals
				uutParameters = new Dictionary<string, string>();

				//Initialize image monitoring variables
				fileList = Directory.EnumerateFiles(_targetRoute, _imgFileExtension, SearchOption.TopDirectoryOnly);
				//Initialize target route monitor system
				fileWatcher = new FileSystemWatcher(_targetRoute, _triggerFileExtension);
				fileWatcher.Filter = _triggerFileExtension;
				fileWatcher.Path = _targetRoute;
				fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
				fileWatcher.Created += new FileSystemEventHandler(fSystem_OnChanged);   //OnCreated Event handler

				fileWatcher.EnableRaisingEvents = true;
				_configured = true;
			}
			catch (IOException ex)
			{
				ReportErrorString("**************************************************************" + endl + "   Error en configuracion de estacion: " + _stationName + endl + "   " + ex.Message + endl +
					"**************************************************************" + endl);
			}
			return;
		}

/*<------ FileSystemWatcher Events ------>*/
		/// <summary>Call this event each time that FileMonitor detects trigger file.</summary>
		/// <param name="sender"></param>
		/// <param name="Ex"></param>
		private void fSystem_OnChanged(object sender, FileSystemEventArgs Ex)
		{
			if(!_configured)
			{ return; }

			//Locals
			string Verbose, destDir, tmpFullName, tmpImgName;
			string serialNum, testDate, stationName;
			int imgCounter = 1;

			//Reporter.
			ReportStringData("**************************************************" + endl + "   Archivo de informacion de estacion de [" + _stationName + "]" + endl +
				"   Hora: " + DateTime.Now.ToString() + endl);

			Thread.Sleep(200);
			GetParamsFromFile(Ex.FullPath);     //Get serial and UUT info
			destDir = Path.Combine(new string[] { _destinyRoute, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString() });

			//Create folder if does not exist
			if (!Directory.Exists(destDir))
			{
				ReportStringData("   El directorio \"" + destDir + "\" no existe, creando directorio..." + endl);
				Directory.CreateDirectory(destDir);
			}

			//Get station and serial parameters
			uutParameters.TryGetValue("Serial", out serialNum);
			uutParameters.TryGetValue("Test_Date", out testDate);
			uutParameters.TryGetValue("Station", out stationName);
			testDate = testDate.Trim().Replace('/', '-').Replace(':', '-').Replace(' ', '_').Replace('.', '-');
			stationName = stationName.Trim().Replace(' ', '-');
			ReportStringData("   Numero de serie: " + serialNum + endl + "   Hora de prueba de la estacion: " + testDate + endl + "   Imagenes encontradas: " + fileList.Count().ToString() + endl);

			//Proccesate each picture in folder
			foreach (string imgFile in fileList)
			{
				//Get station and serial parameters
				tmpImgName = imgFile;

				//Compose full file name
				tmpFullName = Path.Combine(new string[] { destDir, serialNum + "_" + testDate + "_" + stationName + "_img_" + imgCounter.ToString() + Path.GetExtension(tmpImgName) });
				ReportStringData("   Imagen: " + tmpFullName + endl);
				File.Copy(tmpImgName, tmpFullName, true);	//Copy photo to backup directory
				File.Delete(tmpImgName);	//Delete photo from target directory
				imgCounter++;
			}

			uutParameters.Clear();		//Clear parameters from dictionary
			File.Delete(Ex.FullPath);   //Delete info file
			ReportStringData("   Imagenes temporales copiadas..." + endl);
		}

		/// <summary>Generate Event sending string data to front panel</summary>
		/// <param name="dataToReport">String data to be reported.</param>
		private void ReportStringData(string dataToReport)
		{ TriggerFileFound?.Invoke(returnValueType.getString, dataToReport); }

		/// <summary>Generate Event when error is reported on File Server</summary>
		/// <param name="errorToReport">String error to be reported.</param>
		private void ReportErrorString(string errorToReport)
		{ OnServerError?.Invoke(returnValueType.getString, errorToReport); }
	}
}
