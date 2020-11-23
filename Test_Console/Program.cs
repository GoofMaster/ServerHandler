using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*Additionals*/
using Test_Console.Core;
using T = System.Threading.Thread;

namespace Test_Console
{
	class Program
	{
		static void Main(string[] args)
		{
			//Locals
			string serialNumber = string.Empty;
			Dictionary<string, bool> validationKeys = new Dictionary<string, bool>();
			DateTime registeredDate = DateTime.Now;
			TestStatus testStatus = TestStatus.None;

			//Decode argument list
			for(int ptr=0; ptr<args.Length; ptr++)
			{
				switch(args[ptr])
				{
						//Catch Serial number
					case string pC when pC.Contains("/s") || pC.Contains("/serial") || pC.Contains("/sn"):
						ptr++;
						while(ptr < args.Length && !args[ptr].Contains("/"))
						{
							serialNumber += args[ptr] + " ";
							ptr++;
						}
						serialNumber = serialNumber.Trim(); //Clear final whitespaces
						validationKeys.Add("SerialNumber", serialNumber.Length > 0 ? true : false); //Set SerialNumber key
						ptr--;	//Back ptr position
						break;
						//Catch send to fail
					case string pC when pC.Contains("/f") || pC.Contains("/fail"):
						testStatus = TestStatus.Fail;
						validationKeys.Add("FailStatus", true);
						break;
						//Catch send to Pass
					case string pC when pC.Contains("/p") || pC.Contains("/pass"):
						testStatus = TestStatus.Pass;
						validationKeys.Add("PassStatus", true);
						break;
					default:
						break;
				}
			}

			//Initialize Core
			EngineCore eCore = new EngineCore("ClientConfig.ini");
			Console.WriteLine("---------------------------------------------------");
			//Fail and Pass arguments must not be present at argument list
			if (validationKeys.ContainsKey("PassStatus") && validationKeys.ContainsKey("FailStatus"))
			{
				Console.WriteLine("   > Argument /f and /p can not be at same time, must be one at time");
				if (eCore.AwaitKeyPressOnEnd)	//Await for key press flag is enabled
				{ Console.WriteLine("Press any key to continue..."); Console.ReadLine(); }
				return;
			}
			//Validate correct serial number
			else if(!validationKeys.ContainsKey("SerialNumber") || !validationKeys.TryGetValue("SerialNumber", out bool keyValid) || !keyValid)
			{
				Console.WriteLine("   > Serial number " + serialNumber + " is not valid");
				if (eCore.AwaitKeyPressOnEnd)   //Await for key press flag is enabled
				{ Console.WriteLine("Press any key to continue..."); Console.ReadLine(); }
				return;
			}

			//Check in local cache for previous failed files
			Console.WriteLine("Checking for available images at Offline cache direcory...");

			if(eCore.TempFailDirHasFiles)
			{
				Console.WriteLine("Offline cache has files to upload...");
				eCore.SendImagesToServer(registeredDate, FolderType.TempFailsFolder);
			}
			else
			{ Console.WriteLine("Offline cache is empty."); }

			//Check for files in local cache
			Console.WriteLine("Checking for available images at Local Directory...");

			if (eCore.LocalDirHasFiles)
			{
				//Rename files in Local directory according to Serial, test date and status.
				eCore.RenameFilesInSearchFolder(registeredDate, serialNumber, testStatus);
				T.Sleep(200);   //Wait time to liberate all files from memory
				eCore.SendImagesToServer(registeredDate, FolderType.SearchFolder);

				//Move remaining or failed files to Temporal cache directory
				if (eCore.LocalDirHasFiles)
				{ eCore.MoveFailedUploadsToTempFolder(serialNumber); }
			}
			else
			{ Console.WriteLine("Local Directory is emty"); }

			//Purge Directory
			eCore.PurgeSearchFolder();

			if (eCore.AwaitKeyPressOnEnd)   //Await for key press flag is enabled
			{ Console.WriteLine("Press any key to continue..."); Console.ReadLine(); }
			return;
		}
	}
}
