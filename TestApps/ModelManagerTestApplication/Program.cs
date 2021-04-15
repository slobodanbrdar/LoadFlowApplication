using Common;
using Common.Clients;
using Common.LoadFlowInterfaces;
using Common.ModelManagerInterfaces;
using Common.TopologyService;
using OpenDSSengine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ModelManagerTestApplication
{
	public class Program
	{
		public static IModelAccessContract ModelAccessContract { get { return ModelAccessClient.CreateClient(); } }
		public static ITopologyModelAccessContract TopologyModelAccessContract { get { return TopologyModelAccessClient.CreateClient(); } }

		public static ILoadFlowSolver LoadFlowSolverContract { get { return LoadFlowSolverClient.CreateClient(); } }
		static async Task Main(string[] args)
		{
			string response = String.Empty;
			ExecutionReport executionReport = null;
			do
			{

				PrintMenu();
				response = Console.ReadLine();

				if (response == "1")
				{
					executionReport = await ModelAccessContract.GetCurrentModel();

					Console.WriteLine($"Execution status: {executionReport.Status}. Message: {executionReport.Message}");
				}
				else if (response == "2")
				{
					executionReport = await ModelAccessContract.InitializeTopology();

					Console.WriteLine($"Execution status: {executionReport.Status}. Message: {executionReport.Message}");
				}
				else if (response == "3")
				{
					TopologyResult topologyResult = await TopologyModelAccessContract.GetTopologyResult();
				}
				else if (response == "4")
				{
					executionReport = await ModelAccessContract.GetOpenDSSScript();

					Console.WriteLine($"Execution status: {executionReport.Status}. Message:\n{executionReport.Message}");
				}
				else if (response == "5")
				{
					await LoadFlowSolverContract.SolveLoadFlow();
				}

			} while (response != "q");
			

			Console.ReadLine();
		}

		private static void PrintMenu()
		{
			Console.WriteLine("\nChoose tests type:");
			Console.WriteLine("\t1) Get current model");
			Console.WriteLine("\t2) Initialize topology");
			Console.WriteLine("\t3) Get topology");
			Console.WriteLine("\t4) Get Open Dss Script");
			Console.WriteLine("\t5) Solve load flow");
			Console.WriteLine("\tq) Quit");
		}
	}
}
