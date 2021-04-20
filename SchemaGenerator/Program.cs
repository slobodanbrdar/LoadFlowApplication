using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaGenerator
{
	class Program
	{
		private static int baseVoltageCounter = 1;
		private static Random RANDOM = new Random();

		private static Dictionary<int, List<int>> HangingNodes = new Dictionary<int, List<int>>();

		private static Dictionary<int, List<int>> UnconnectedNodes = new Dictionary<int, List<int>>();

		private static int terminalCounter = 1;
		private static int transformerCounter = 1;
		private static int acLineSegmentCounter = 1;
		private static int energyConsumerCounter = 1;
		private static int nodeCounter = 1;


		static void Main(string[] args)
		{
			int numberOfRoots = -1;

			while (numberOfRoots < 1)
			{
				Console.WriteLine("Enter number of roots: ");
				numberOfRoots = Convert.ToInt32(Console.ReadLine());

				if (numberOfRoots < 1)
				{
					Console.WriteLine("Number of roots must be greater then 0.");
				}
			}

			int minNodes = 1;
			int maxNodes = 0;
			
			while (minNodes > maxNodes)
			{
				Console.WriteLine("Enter minimum number of nodes: ");
				minNodes = Convert.ToInt32(Console.ReadLine());

				Console.WriteLine("Enter maximum number of nodes: ");
				maxNodes = Convert.ToInt32(Console.ReadLine());

				if (minNodes > maxNodes)
				{
					Console.WriteLine("Minimum number of nodes must be less then maximum.");
				}
			}

			GenerateSchema(numberOfRoots, minNodes, maxNodes);

			Console.WriteLine("Done generating schema.");
			Console.ReadLine();

		}

		private static void GenerateSchema(int numberOfRoots, int minNodes, int maxNodes)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			builder.AppendLine("<rdf:RDF	xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"");
			builder.AppendLine("\t\t\txmlns:cim=\"http://iec.ch/TC57/2010/CIM-schema-cim15#\"");
			builder.AppendLine("\t\t\txmlns:ftn=\"http://www.ftnydro.com/CIM15/2010/extension#\">");

			builder.AppendLine(GenerateBaseVoltages());

			builder.AppendLine(GenerateRoots(numberOfRoots));

			builder.AppendLine(GenerateNodes(numberOfRoots, minNodes, maxNodes));

			builder.AppendLine(GenerateBranches(numberOfRoots));

			builder.AppendLine(GenerateLowVoltageLoads(numberOfRoots));

			builder.AppendLine("</rdf:RDF>");
			File.WriteAllText($"..\\..\\..\\Resources\\LoadFlow_{numberOfRoots}Roots.xml", builder.ToString());
		}

		private static string GenerateBaseVoltages()
		{
			StringBuilder bvsBuilder = new StringBuilder();

			bvsBuilder.AppendLine("<!--BaseVoltages-->");
			bvsBuilder.AppendLine(GenerateBaseVoltage(115000)); //VN
			bvsBuilder.AppendLine(GenerateBaseVoltage(12470));  //SN
			bvsBuilder.AppendLine(GenerateBaseVoltage(4500));	//NN
			bvsBuilder.AppendLine();
			bvsBuilder.AppendLine("<!--END BaseVoltages-->");

			return bvsBuilder.ToString();
		}

		private static string GenerateBaseVoltage(int voltage)
		{
			StringBuilder bvBuilder = new StringBuilder();

			string baseString = "\t\t";
			bvBuilder.AppendLine($"{baseString}<cim:BaseVoltage rdf:ID=\"BV_{baseVoltageCounter}\">");
			bvBuilder.AppendLine($"{baseString}\t<cim:BaseVoltage.nominalVoltage>{voltage}</cim:BaseVoltage.nominalVoltage>");
			bvBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>BaseVoltage{baseVoltageCounter}</cim:IdentifiedObject.aliasName>");
			bvBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>BV_{baseVoltageCounter}</cim:IdentifiedObject.mRID>");
			bvBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>BV_{baseVoltageCounter}</cim:IdentifiedObject.name>");
			bvBuilder.AppendLine($"{baseString}</cim:BaseVoltage>");

			baseVoltageCounter++;

			return bvBuilder.ToString();
		}

		private static string GenerateRoots(int numberOfRoots)
		{
			StringBuilder rootsBuilder = new StringBuilder();

			rootsBuilder.AppendLine("<!--EnergySources-->");

			for (int i = 1; i <= numberOfRoots; i++)
			{
				rootsBuilder.AppendLine(GenerateEnergySource(i));
			}

			rootsBuilder.AppendLine("<!--END EnergySources-->");

			return rootsBuilder.ToString();
		}

		private static string GenerateEnergySource(int i)
		{
			StringBuilder energySourceBuilder = new StringBuilder();
			int activePower = RANDOM.Next(10000000, 40000000); 

			string baseString = "\t\t";
			energySourceBuilder.AppendLine($"{baseString}<cim:EnergySource rdf:ID=\"ES_{i}\">");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.nominalVoltage>{activePower}</cim:EnergySource.nominalVoltage>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.r>{RANDOM.NextDouble() * 10}</cim:EnergySource.r>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.r0>{RANDOM.NextDouble() * 10}</cim:EnergySource.r0>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.rn>{RANDOM.NextDouble() * 10}</cim:EnergySource.rn>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.x>{RANDOM.NextDouble() * 10}</cim:EnergySource.x>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.x0>{RANDOM.NextDouble() * 10}</cim:EnergySource.x0>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.xn>{RANDOM.NextDouble() * 10}</cim:EnergySource.xn>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.voltageAngle>0</cim:EnergySource.voltageAngle>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:EnergySource.voltageMagnitude>115000</cim:EnergySource.voltageMagnitude>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:ConductingEquipment.BaseVoltage rdf:resource=\"#BV_1\"/>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>EnergySource{i}</cim:IdentifiedObject.aliasName>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>ES_{i}</cim:IdentifiedObject.mRID>");
			energySourceBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>ES_{i}</cim:IdentifiedObject.name>");
			energySourceBuilder.AppendLine($"{baseString}</cim:EnergySource>");


			return energySourceBuilder.ToString();
		}

		private static string GenerateNodes(int numberOfRoots, int minNodes, int maxNodes)
		{
			StringBuilder nodeBuilder = new StringBuilder();

			nodeBuilder.AppendLine("<!--ConnectivityNodes-->");
			
			for (int i = 1; i <= numberOfRoots; i++)
			{
				int nodeCount = RANDOM.Next(minNodes, maxNodes);
				List<int> nodes = new List<int>(nodeCount);

				for (int j = 1; j <= nodeCount; j++)
				{
					nodes.Add(nodeCounter);
					nodeBuilder.AppendLine(GenerateNode(nodeCounter));
					nodeCounter++;
				}

				HangingNodes.Add(i, nodes);
			}

			nodeBuilder.AppendLine("<!--END ConnectivityNodes-->");
			return nodeBuilder.ToString();
		}

		private static string GenerateNode(int nodeCount)
		{
			StringBuilder nodeBuilder = new StringBuilder();

			string baseString = "\t\t";
			nodeBuilder.AppendLine($"{baseString}<cim:ConnectivityNode rdf:ID=\"CN_{nodeCount}\">");
			nodeBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>ConnectivityNode{nodeCount}</cim:IdentifiedObject.aliasName>");
			nodeBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>CN_{nodeCount}</cim:IdentifiedObject.mRID>");
			nodeBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>CN_{nodeCount}</cim:IdentifiedObject.name>");
			nodeBuilder.AppendLine($"{baseString}</cim:ConnectivityNode>");

			
			return nodeBuilder.ToString();
		}

		private static string GenerateBranches(int numberOfRoots)
		{
			StringBuilder branchBuilder = new StringBuilder();


			StringBuilder terminalsBuilder = new StringBuilder();
			StringBuilder transformerBuiler = new StringBuilder();
			StringBuilder transformerEndBuiler = new StringBuilder();
			StringBuilder acLineSegmentBuilder = new StringBuilder();
			StringBuilder energyConsumerBuilder = new StringBuilder();

			

			for (int i = 1; i <= numberOfRoots; i++)
			{
				List<int> hangingNodes = HangingNodes[i];
				List<int> connectedNodes = new List<int>(hangingNodes.Count);

				int hanhingNode = hangingNodes[0];
				hangingNodes.RemoveAt(0);
				GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"ES_{i}", hanhingNode);

				//Svaki source na pocetku ima transformator (VN/SN)
				GenerateTransformer(transformerBuiler, "BV_1", transformerCounter);
				GenerateTransformerEnd(transformerEndBuiler, transformerCounter, 115000, 1);
				GenerateTransformerEnd(transformerEndBuiler, transformerCounter, 12470, 2);

				GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"PT_{transformerCounter}", hanhingNode);
				int endNode = hangingNodes[0];
				hangingNodes.RemoveAt(0);

				GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"PT_{transformerCounter}", endNode);
				transformerCounter++;

				connectedNodes.Add(endNode);
				List<int> unconnectedNodes = new List<int>();
				while (connectedNodes.Count > 0)
				{
					int nodeIndex = RANDOM.Next(connectedNodes.Count);
					int startNode = connectedNodes[nodeIndex];
					connectedNodes.RemoveAt(nodeIndex);

					if (hangingNodes.Count == 0)
					{
						unconnectedNodes.Add(startNode);
					}

					int nodeBranchesCount = GetNodeBranchesCount();

					bool energyConsumerExists = false;

					for (int j = 1; j <= nodeBranchesCount && hangingNodes.Count > 0; j++)
					{
						endNode = hangingNodes[0];
						hangingNodes.RemoveAt(0);

						if (nodeBranchesCount > 1 && RANDOM.Next(0, 100) < 20 && !energyConsumerExists) //20% sanse za ec na srednjem naponu
						{
							GenerateEnergyConsumer(energyConsumerBuilder, "BV_2", energyConsumerCounter);
							GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"EC_{energyConsumerCounter}", startNode);
							GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"EC_{energyConsumerCounter}", endNode);

							energyConsumerCounter++;
							energyConsumerExists = true;
							continue;
						}

						GenerateACLineSegment(acLineSegmentBuilder, acLineSegmentCounter);
						GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"ACL_{acLineSegmentCounter}", startNode);
						GenerateElementTerminal(terminalsBuilder, terminalCounter++, $"ACL_{acLineSegmentCounter}", endNode);

						acLineSegmentCounter++;
						connectedNodes.Add(endNode);
					}
				}

				UnconnectedNodes.Add(i, unconnectedNodes);

			}

			branchBuilder.AppendLine("<!--Transformers-->");
			branchBuilder.AppendLine(transformerBuiler.ToString());
			branchBuilder.AppendLine("<!--END Transformers-->");

			branchBuilder.AppendLine("<!--TransformerEnds-->");
			branchBuilder.AppendLine(transformerEndBuiler.ToString());
			branchBuilder.AppendLine("<!--END TransformerEnds-->");

			branchBuilder.AppendLine("<!--ACLineSegments-->");
			branchBuilder.AppendLine(acLineSegmentBuilder.ToString());
			branchBuilder.AppendLine("<!--END ACLineSegments-->");

			branchBuilder.AppendLine("<!--EnergyConsumers-->");
			branchBuilder.AppendLine(energyConsumerBuilder.ToString());
			branchBuilder.AppendLine("<!--END EnergyConsumers-->");

			branchBuilder.AppendLine("<!--Terminals-->");
			branchBuilder.AppendLine(terminalsBuilder.ToString());
			branchBuilder.AppendLine("<!--END Terminals-->");

			return branchBuilder.ToString();
		}


		private static string GenerateLowVoltageLoads(int numberOfRoots)
		{

			StringBuilder loadBuilder = new StringBuilder();
			StringBuilder energyConsumerBuilder = new StringBuilder();
			StringBuilder nodeBuilder = new StringBuilder();
			StringBuilder transformerBuilder = new StringBuilder();
			StringBuilder transformerEndBuilder = new StringBuilder();
			StringBuilder terminalBuilder = new StringBuilder();

			for (int i = 1; i <= numberOfRoots; i++)
			{
				List<int> unconnectedNodes = UnconnectedNodes[i];

				while (unconnectedNodes.Count > 0)
				{
					int startNode = unconnectedNodes[0];
					unconnectedNodes.RemoveAt(0);

					int endNode = nodeCounter;
					nodeBuilder.AppendLine(GenerateNode(endNode));

					GenerateTransformer(transformerBuilder, "BV_2", transformerCounter);
					GenerateTransformerEnd(transformerEndBuilder, transformerCounter, 12470, 1);
					GenerateTransformerEnd(transformerEndBuilder, transformerCounter, 4500, 2);

					GenerateElementTerminal(terminalBuilder, terminalCounter++, $"PT_{transformerCounter}", startNode);
					GenerateElementTerminal(terminalBuilder, terminalCounter++, $"PT_{transformerCounter}", endNode);

					nodeCounter++;
					transformerCounter++;
					int numberOfLoads = GetLowVoltageLoadsCount();

					for (int j = 1; j <= numberOfLoads; j++)
					{
						GenerateEnergyConsumer(energyConsumerBuilder, "BV_3", energyConsumerCounter);
						int loadEndNode = nodeCounter;
						nodeBuilder.AppendLine(GenerateNode(loadEndNode));

						GenerateElementTerminal(terminalBuilder, terminalCounter++, $"EC_{energyConsumerCounter}", endNode);
						GenerateElementTerminal(terminalBuilder, terminalCounter++, $"EC_{energyConsumerCounter}", loadEndNode);

						nodeCounter++;
						energyConsumerCounter++;
					}

				}
			}

			loadBuilder.AppendLine("<!--AdditionalTransformers-->");
			loadBuilder.AppendLine(transformerBuilder.ToString());
			loadBuilder.AppendLine("<!--END AdditionalTransformers-->");
			loadBuilder.AppendLine("<!--AdditionalTransformerEnds-->");
			loadBuilder.AppendLine(transformerEndBuilder.ToString());
			loadBuilder.AppendLine("<!--END AdditionalTransformerEnds-->");
			loadBuilder.AppendLine("<!--AdditionalEnergyConsumers-->");
			loadBuilder.AppendLine(energyConsumerBuilder.ToString());
			loadBuilder.AppendLine("<!--END AdditionalEnergyConsumers-->");
			loadBuilder.AppendLine("<!--AdditionalConnectivityNodes-->");
			loadBuilder.AppendLine(nodeBuilder.ToString());
			loadBuilder.AppendLine("<!--END AdditionalConnectivityNodes-->");
			loadBuilder.AppendLine("<!--AdditionalTerminals-->");
			loadBuilder.AppendLine(terminalBuilder.ToString());
			loadBuilder.AppendLine("<!--END AdditionalTerminals-->");

			return loadBuilder.ToString();
		}

		private static void GenerateEnergyConsumer(StringBuilder energyConsumerBuilder, string baseVoltageId, int energyConsumerCounter)
		{
			string connKind = RANDOM.Next(0, 100) > 50 ? "Y" : "D";
			string baseString = "\t\t";

			int powerMultiplier = 1000000; //za srednji napon (BV_2)
			if (baseVoltageId == "BV_3") //za niski napon
			{
				powerMultiplier = 1000;
			}

			energyConsumerBuilder.AppendLine($"{baseString}<cim:EnergyConsumer rdf:ID=\"EC_{energyConsumerCounter}\">");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:EnergyConsumer.grounded>{connKind}</cim:EnergyConsumer.grounded>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:EnergyConsumer.pfixed>{RANDOM.Next(1, 50) * powerMultiplier}</cim:EnergyConsumer.pfixed>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:EnergyConsumer.pfixedPct>{RANDOM.Next(80, 100)}</cim:EnergyConsumer.pfixedPct>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:EnergyConsumer.phaseConnection>{connKind}</cim:EnergyConsumer.phaseConnection>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:ConductingEquipment.BaseVoltage rdf:resource=\"#{baseVoltageId}\"/>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>EnergyConsumer{energyConsumerCounter}</cim:IdentifiedObject.aliasName>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>EC_{energyConsumerCounter}</cim:IdentifiedObject.mRID>");
			energyConsumerBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>EC_{energyConsumerCounter}</cim:IdentifiedObject.name>");
			energyConsumerBuilder.AppendLine($"{baseString}</cim:EnergyConsumer>");
		}

		private static void GenerateACLineSegment(StringBuilder acLineSegmentBuilder, int acLineSegmentCounter)
		{
			string baseString = "\t\t";
			acLineSegmentBuilder.AppendLine($"{baseString}<cim:ACLineSegment rdf:ID=\"ACL_{acLineSegmentCounter}\">");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.b0ch>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.b0ch>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.bch>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.bch>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.g0ch>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.g0ch>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.gch>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.gch>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.r>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.r>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.r0>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.r0>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.x>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.x>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ACLineSegment.x0>{RANDOM.NextDouble() * 10}</cim:ACLineSegment.x0>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:Conductor.length>{RANDOM.Next(1, 50) * 1000}</cim:Conductor.length>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:ConductingEquipment.BaseVoltage rdf:resource=\"#BV_2\"/>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>ACLineSegment{acLineSegmentCounter}</cim:IdentifiedObject.aliasName>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>ACL_{acLineSegmentCounter}</cim:IdentifiedObject.mRID>");
			acLineSegmentBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>ACL_{acLineSegmentCounter}</cim:IdentifiedObject.name>");
			acLineSegmentBuilder.AppendLine($"{baseString}</cim:ACLineSegment>");


		}

		private static void GenerateElementTerminal(StringBuilder terminalBuilder, int terminalCounter, string elementId, int nodeNumber)
		{
			string baseString = "\t\t";
			terminalBuilder.AppendLine($"{baseString}<cim:Terminal rdf:ID=\"TM_{terminalCounter}\">");
			terminalBuilder.AppendLine($"{baseString}\t<cim:Terminal.ConductingEquipment rdf:resource=\"#{elementId}\"/>");
			terminalBuilder.AppendLine($"{baseString}\t<cim:Terminal.ConnectivityNode rdf:resource=\"#CN_{nodeNumber}\"/>");
			terminalBuilder.AppendLine($"{baseString}\t<cim:Terminal.phases>ABC</cim:Terminal.phases>");
			terminalBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>Terminal{terminalCounter}</cim:IdentifiedObject.aliasName>");
			terminalBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>TM_{terminalCounter}</cim:IdentifiedObject.mRID>");
			terminalBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>TM_{terminalCounter}</cim:IdentifiedObject.name>");
			terminalBuilder.AppendLine($"{baseString}</cim:Terminal>");
			terminalBuilder.AppendLine();
		}


		private static void GenerateTransformer(StringBuilder transformerBuilder, string baseVoltageId, int transformerCounter)
		{
			string baseString = "\t\t";
			transformerBuilder.AppendLine($"{baseString}<cim:PowerTransformer rdf:ID=\"PT_{transformerCounter}\">");
			transformerBuilder.AppendLine($"{baseString}\t<cim:ConductingEquipment.BaseVoltage rdf:resource=\"#{baseVoltageId}\"/>");
			transformerBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>PowerTransformer{transformerCounter}</cim:IdentifiedObject.aliasName>");
			transformerBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>PT_{transformerCounter}</cim:IdentifiedObject.mRID>");
			transformerBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>PT_{transformerCounter}</cim:IdentifiedObject.name>");
			transformerBuilder.AppendLine($"{baseString}</cim:PowerTransformer>");
			transformerBuilder.AppendLine();
		}

		private static void GenerateTransformerEnd(StringBuilder transformerEndBuilder, int transformerCounter, int ratedU, int endNumber)
		{

			string connKind = RANDOM.Next(0, 100) > 50 ? "Y" : "D"; //50:50

			string baseString = "\t\t";
			transformerEndBuilder.AppendLine($"{baseString}<cim:PowerTransformerEnd rdf:ID=\"PTE_{transformerCounter}{endNumber}\">");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.b>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.b>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.b0>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.b0>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.g>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.g>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.g0>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.g0>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.x>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.x>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.x0>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.x0>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.r>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.r>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.r0>{RANDOM.NextDouble() * 10}</cim:PowerTransformerEnd.r0>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.ratedS>{RANDOM.Next(1, 20) * 1000000}</cim:PowerTransformerEnd.ratedS>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.ratedU>{ratedU}</cim:PowerTransformerEnd.ratedU>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.PowerTransformer rdf:resource=\"#PT_{transformerCounter}\"/>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:PowerTransformerEnd.connectionKind>{connKind}</cim:PowerTransformerEnd.connectionKind>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.aliasName>PowerTransformerEnd{transformerCounter}{endNumber}</cim:IdentifiedObject.aliasName>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.mRID>PTE_{transformerCounter}{endNumber}</cim:IdentifiedObject.mRID>");
			transformerEndBuilder.AppendLine($"{baseString}\t<cim:IdentifiedObject.name>PTE_{transformerCounter}{endNumber}</cim:IdentifiedObject.name>");
			transformerEndBuilder.AppendLine($"{baseString}</cim:PowerTransformerEnd>");
			transformerEndBuilder.AppendLine();

		}

		

		private static int GetLowVoltageLoadsCount()
		{
			int rnd = RANDOM.Next(100);
			if (rnd < 10) //10%
			{
				return 1;
			}
			else if (rnd < 40) //30%
			{
				return 2;
			}
			else if (rnd < 80) //40%
			{
				return 3;
			}
			else //20%
			{
				return 4;
			}
		}

		private static int GetNodeBranchesCount()
		{
			int rnd = RANDOM.Next(100);
			if (rnd < 30) // 30%
			{
				return 1;
			}
			else if (rnd < 80) // 50%
			{
				return 2;
			}
			else // 20%
			{
				return 3;
			}
		}
	}
}
