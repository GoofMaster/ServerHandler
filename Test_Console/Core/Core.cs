using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*Additionals*/
using System.ConfigFile;
using System.IO;
using System.Net;
using System.Net.Security;
using FluentFTP;
using System.Security.Cryptography;
using System.Globalization;
using T = System.Threading.Thread;

namespace Test_Console.Core
{
/*<!---------------- Namespace Enumerables ----------------!>*/
	public enum FolderType
	{ TempFailsFolder, SearchFolder }
	public enum TestStatus
	{ Pass, Fail, None }

	public partial class EngineCore
	{
/*<!---------------- Internal variables ----------------!>*/
		private string _serverAddress, _user, _password, _searchFolder, _tempFailUploads, _stationName, _searchImgExtension;
		private bool _useIntegrityFileCheck, _setUserAsStationName, _LastConnectionSucess, _awaitKeyPressOnEnd;
		private string appRoute = AppDomain.CurrentDomain.BaseDirectory;
		private string endl = Environment.NewLine;
		//FTP Server loaders
		private FtpClient ftpConnection;
		//File Handlers
		IEnumerable<string> searchFolder_FilesList, tempFoder_FileList;
		//Integrity check handlers
		SHA1 shaMan;

/*<!---------------- Property Nodes ----------------!>*/
		/// <summary>Ask to engine if Local Directory has files to upload</summary>
		public bool LocalDirHasFiles
		{ get => searchFolder_FilesList.Count() > 0 ? true : false; }

		/// <summary>Ask to engine if Temporal Upload Fails has files to upload</summary>
		public bool TempFailDirHasFiles
		{ get => tempFoder_FileList.Count() > 0 ? true : false; }

		/// <summary>Get las FTP connection status</summary>
		public bool LastConnectionStatus
		{ get => _LastConnectionSucess; }

		public bool AwaitKeyPressOnEnd
		{ get => _awaitKeyPressOnEnd; }

		/// <summary>Create instance of Engine core and configure FTP esscencial parameters</summary>
		/// <param name="configName">Config File name (Relative path)</param>
		public EngineCore(string configName)
		{
			//Locals
			ConfigFileAdmin clientCfg = new ConfigFileAdmin(Path.Combine(appRoute, configName), false);

			//Load Server parameters
			clientCfg.ReadKeyFromConfig("Server_Parameters", "Server_Address", out _serverAddress);
			clientCfg.ReadKeyFromConfig("Server_Parameters", "Server_User", out _user);
			clientCfg.ReadKeyFromConfig("Server_Parameters", "Server_Password", out _password);
			//Load Engine parameters
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_SearchFolder", out _searchFolder);
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_TempFailUploadsFolder", out _tempFailUploads);
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_StationName", out _stationName);
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_ImageFileExtension", out _searchImgExtension);
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_UseIntegrityCheck", out _useIntegrityFileCheck);
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_SetFtpUserAsStationName", out _setUserAsStationName);
			clientCfg.ReadKeyFromConfig("Engine_Options", "Engine_AwaitKeyPressOnEnd", out _awaitKeyPressOnEnd);
			clientCfg.Close(false);

			//Configure FTP Client
			ftpConnection = new FtpClient(_serverAddress, new NetworkCredential(_user, _password));
			ftpConnection.EncryptionMode = FtpEncryptionMode.Explicit;
			ftpConnection.DownloadDataType = FtpDataType.Binary;
			ftpConnection.ValidateCertificate += new FtpSslValidation(ftpClient_OnCertificationValidation);
			Console.WriteLine("Initializing Image backup Engine..." + endl + "   > Server Address: " + _serverAddress + endl + "   > User: " + _user + endl + "   > Search Folder: " + _searchFolder + endl + "   > Temporal Fail Uploads Folder: " + _tempFailUploads);

			//Configure localFile Allocations
			if(!Directory.Exists(_searchFolder))
			{ Directory.CreateDirectory(_searchFolder); }
			searchFolder_FilesList = Directory.EnumerateFiles(_searchFolder, _searchImgExtension);

			//Configure Temp Directory folder allocation
			if (!Directory.Exists(_tempFailUploads))
			{ Directory.CreateDirectory(_tempFailUploads); }
			tempFoder_FileList = Directory.EnumerateFiles(_tempFailUploads, _searchImgExtension);

			//Configure SHA1 to generate file integrity checksum
			shaMan = SHA1.Create();
		}

		/// <summary>Move remaining images to Temporal Fails folder.</summary>
		public void MoveFailedUploadsToTempFolder(string serialNumber)
		{
			//Locals
			string tempFailedFullName;

			foreach(string imgFile in searchFolder_FilesList.Where(e => e.Contains(serialNumber)))
			{
				try
				{
					tempFailedFullName = Path.Combine(_tempFailUploads, Path.GetFileName(imgFile));
					File.Move(imgFile, tempFailedFullName);
					Console.WriteLine("Image " + Path.GetFileName(imgFile) + " moved sucessfull.");
				}
				catch
				{ }
			}
			return;
		}

		/// <summary>Purge search folder in case of existent files</summary>
		public void PurgeSearchFolder()
		{
			//Locals
			IEnumerable<string> remainingFiles = Directory.EnumerateFiles(_searchFolder, "*.*", SearchOption.TopDirectoryOnly);

			//If remaining files, purge directory
			if(remainingFiles.Count() > 0)
			{
				Console.WriteLine("There are " + remainingFiles.Count().ToString() + " remaining in search directory");

				foreach (string fileName in Directory.EnumerateFiles(_searchFolder, "*.*", SearchOption.TopDirectoryOnly))
				{
					try
					{
						File.Delete(fileName);
						Console.WriteLine("File " + fileName + " purged sucessfull.");
					}
					catch
					{ }
				}
			}
			else
			{ Console.WriteLine("Search directory are clear and ready."); }
		}

		/// <summary>Rename file in Search directory</summary>
		/// <param name="testDate">Timespan of actual date and time</param>
		/// <param name="serialNumber">UUT Serial number to attach to images.</param>
		/// <returns>True if all files where renamed sucessfull</returns>
		public bool RenameFilesInSearchFolder(DateTime testDate, string serialNumber, TestStatus testStatus)
		{
			//Locals
			string fullRenPattern, newFileNamePattern;
			string[] renFileArray = searchFolder_FilesList.ToArray();
			int imgNumber = 1;

			//Load directory tree
			newFileNamePattern = serialNumber + "_" + testDate.ToString("dd'-'MM'-'yyyy'_'HH'-'mm'-'ss", CultureInfo.InvariantCulture) + "_" + _stationName.Replace(' ', '-') + "_";

			switch(testStatus)
			{
				case TestStatus.Pass:
					newFileNamePattern += "Pass";
					break;
				case TestStatus.Fail:
					newFileNamePattern += "Fail";
					break;
				default:
					newFileNamePattern += "NoStatus";
					break;
			}

			//Generate full renaming pattern
			for (int Acc=0; Acc<renFileArray.Length; Acc++)
			{
				//Complete pattern image name
				for(int retries=5; retries>0; retries--)
				{
					fullRenPattern = Path.Combine(_searchFolder, newFileNamePattern + "_" + imgNumber.ToString() + Path.GetExtension(renFileArray[Acc]));
					if (!File.Exists(fullRenPattern))
					{
						try
						{
							File.Move(renFileArray[Acc], fullRenPattern);
							imgNumber++;
							break;
						}
						catch
						{ }
					}
					else
					{ imgNumber++; }
				}
			}

			return false;
		}

		/// <summary>Send Image files to remote FTP Server.</summary>
		/// <param name="remoteFolder">Remote folder path (can be in unix mode).</param>
		/// <returns>Returns true if all files where sucessfull transfered to the server.</returns>
		public bool SendImagesToServer(DateTime makeRemoteFolder, FolderType selectionFolder)
		{
			//Locals
			string unixRemoteFolder = makeRemoteFolder.ToString("yyyy'/" + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(makeRemoteFolder.Month) + "/'dd");
			List<string> tempFileToErase = new List<string>();
			IEnumerable<string> tempFolderList_Generic;
			string ftpFullName, server_SHA1Value, local_SHA1Value;
			int fileNum, ofNum;

			//Selection between Search folder and temporal fails folder
			fileNum = 1;
			switch (selectionFolder)
			{
				case FolderType.TempFailsFolder:
					tempFolderList_Generic = tempFoder_FileList;
					ofNum = tempFoder_FileList.Count();
					break;
				case FolderType.SearchFolder:
					tempFolderList_Generic = searchFolder_FilesList;
					ofNum = searchFolder_FilesList.Count();
					break;
				default:
					tempFolderList_Generic = searchFolder_FilesList;
					ofNum = searchFolder_FilesList.Count();
					break;
			}

			//Validate if available files to upload to server
			if (tempFolderList_Generic.Count() == 0)
			{
				Console.WriteLine(" Folder " + selectionFolder.ToString() + " has 0 files to proccesate.");
				return false;
			}
			else
			{ Console.WriteLine("There are " + tempFolderList_Generic.Count().ToString() + " files..."); }

			//Connect to FTP Server
			try
			{
				ftpConnection.Connect();
				_LastConnectionSucess = true;
			}
			catch
			{
				Console.WriteLine("Server " + _serverAddress + " does not respond, maybe are at offline state");
				return false;
			}

			//Validate if directory does not exist in server
			if (!ftpConnection.DirectoryExists(unixRemoteFolder))
			{
				if (!ftpConnection.CreateDirectory(unixRemoteFolder, true))
				{
					Console.WriteLine("Directory " + unixRemoteFolder + "cannot be created, check Server folder creation permissions...");
					ftpConnection.Disconnect();
					return false;
				}
				else
				{ Console.WriteLine("Direcorty " + unixRemoteFolder + " created sucessfull on the server..."); }
			}

			//Warn to user if IntegrityCheck is disabled
			if(!_useIntegrityFileCheck)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("WARNING!!! File Integrity Check is disabled, files are copied but file corruption is not guaranted.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}

			//Execute file copy to server.
			foreach (string tmpImgName in tempFolderList_Generic)
			{
				ftpFullName = unixRemoteFolder + "/" + Path.GetFileName(tmpImgName);
				Console.WriteLine("File " + fileNum.ToString() + " of " + ofNum.ToString() + ": " + Path.GetFileName(tmpImgName));

				try
				{ ftpConnection.UploadFile(tmpImgName, ftpFullName, FtpRemoteExists.Overwrite, false, FtpVerify.OnlyChecksum | FtpVerify.Retry); }
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("   > Failed to Upload file to server...");
					Console.ForegroundColor = ConsoleColor.Gray;
					continue;
				}

				//If IntegrityFlag is false, integrity check is disabled
				if(_useIntegrityFileCheck)
				{
					//Create SHA1 of local file
					using (FileStream fileDrv = new FileStream(tmpImgName, FileMode.Open))
					{
						server_SHA1Value = ftpConnection.GetHash(ftpFullName).Value.ToUpper();
						Console.WriteLine("   > Server Hash SHA1 [" + server_SHA1Value + "]");
						local_SHA1Value = shaMan.ComputeHash(fileDrv).Select(e => e.ToString("X2")).Aggregate((prev, next) => prev + next);
						Console.WriteLine("   > Local Hash SHA1 [" + local_SHA1Value + "]");

						//If SHA1 checksum match, add picture to delete queue
						if (server_SHA1Value.Equals(local_SHA1Value, StringComparison.OrdinalIgnoreCase))
						{
							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine("   > File integrity OK...");
							Console.ForegroundColor = ConsoleColor.Gray;
							tempFileToErase.Add(tmpImgName);    //Add picture to delete queue
						}
						else
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("   > File integrity INVALID...");
							Console.ForegroundColor = ConsoleColor.Gray;
						}
						fileNum++;
					}
				}
			}

			//Disconnect from ftp server and delete temporal files
			ftpConnection.Disconnect();

			//Erase only sucessfull files
			foreach (string fileElement in tempFileToErase)
			{
				File.Delete(fileElement);
				Console.WriteLine("Temporal image " + Path.GetFileName(fileElement) + " deleted sucessfull.");
			}

			tempFileToErase.Clear();
			return false;
		}

/*<!---------------- Event Callback Handlers ----------------!>*/
		private static void ftpClient_OnCertificationValidation(FtpClient control, FtpSslValidationEventArgs e)
		{
			if (e.PolicyErrors != SslPolicyErrors.None)
			{
				//Console.WriteLine(e.PolicyErrors.ToString());
				//Console.WriteLine("El certificado ha expirado: " + e.Certificate.ToString());
				e.Accept = true;
			}
			else
			{ e.Accept = true; }
			return;
		}
	}
}
