using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*Additionals*/
using System.ConfigFile;

namespace ServerHandler.SaverCore
{
	public partial class Core
	{
		private Dictionary<string, string> uutParameters;

		internal void GetParamsFromFile(string textFileName)
		{
			ConfigFileAdmin uutCfgParams = new ConfigFileAdmin(textFileName, false);

			uutCfgParams.ReadKeyFromConfig("UUT_Info", "Serial", out string tmpValue);
			uutParameters.Add("Serial", tmpValue);
			uutCfgParams.ReadKeyFromConfig("UUT_Info", "Test_Date", out tmpValue);
			uutParameters.Add("Test_Date", tmpValue);
			uutCfgParams.ReadKeyFromConfig("UUT_Info", "Station", out tmpValue);
			uutParameters.Add("Station", tmpValue);
			uutCfgParams.Close();
		}
	}
}
