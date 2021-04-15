using Common.TopologyService.ExtendedClassess;
using Common.TopologyService.InternalModel.Circuts;
using Common.TopologyService.InternalModel.GraphElems;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Common.TopologyService.InternalModelBuilder
{
	public class CInternalModelBuilder
    {
        private String file_path;
        private CModelFramework internalModel;

        public CInternalModelBuilder(CModelFramework internalModel, String path)
        {
            this.file_path = path;
            this.internalModel = internalModel;
        }

        public void ReadSchemaFromFile()
        {
            ReadRootsFromFile();
            ReadNodesFromFile();
            ReadBranchesFromFile();
            ReadSwitchesFromFile();
        }

        public void ReadNodesFromFile()
        {
            Dictionary<long, MPNode> listOfNodes = internalModel.Nodes;
            String file_name = "Nodes.xml";

            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(file_path + file_name);

            XmlNodeList xnList = xml_doc.SelectNodes("/Nodes/BusNode");

            foreach (XmlElement xml_elem in xnList)
            {
                long nodeLID = UInt32.Parse(xml_elem.GetAttribute("LID"));
                EPhaseCode phases = (EPhaseCode)Byte.Parse(xml_elem["Phases"].InnerText);

                if (!listOfNodes.ContainsKey(nodeLID))
                {
                    MPBusNode node = new MPBusNode(nodeLID, phases);


                    listOfNodes.Add(nodeLID, node);
                }
                else
                {
                    throw new Exception("Bad input file. There is already node with same key " + nodeLID + ".");
                }


            }
        }

        public void ReadBranchesFromFile()
        {
            Dictionary<long, MPNode> listOfNodes = internalModel.Nodes;
            Dictionary<long, MPBranch> listOfBranches = internalModel.Branches;
            String file_name = "Branches.xml";

            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(file_path + file_name);


            XmlNodeList xnList = xml_doc.SelectNodes("/Branches/Line");

            foreach (XmlElement xml_elem in xnList)
            {
                long branchLID = UInt32.Parse(xml_elem.GetAttribute("LID"));
                long node1_lid = UInt32.Parse(xml_elem["End1_Node"].InnerText);
                long node2_lid = UInt32.Parse(xml_elem["End2_Node"].InnerText);
                EPhaseCode phases = (EPhaseCode)Byte.Parse(xml_elem["Phases"].InnerText);

                if (listOfBranches.ContainsKey(branchLID))
                {
                    throw new Exception("Bad input file. There is already branch with same key " + branchLID + ".");
                }
                else
                {
                    if (!listOfNodes.ContainsKey(node1_lid) || !listOfNodes.ContainsKey(node2_lid))
                    {
                        throw new Exception("Bad input file. There is(are) not node(s) with specific lid(s).");
                    }
                    else
                    {
                        MPLine branch = new MPLine(branchLID, phases, node1_lid, node2_lid);


                        listOfBranches.Add(branchLID, branch);
                    }
                }

            }
        }

        public void ReadRootsFromFile()
        {
            String file_name = "Roots.xml";

            Dictionary<long, MPNode> listOfNodes = internalModel.Nodes;

            Dictionary<long, MPBranch> listOfBranches = internalModel.Branches;
            Dictionary<long, MPRoot> roots = internalModel.Roots;

            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(file_path + file_name);

            XmlNodeList xnList = xml_doc.SelectNodes("/Roots/Root");

            foreach (XmlElement xml_elem in xnList)
            {
                long root_lid = UInt32.Parse(xml_elem.GetAttribute("LID"));
                long node_lid = UInt32.Parse(xml_elem["Node"].InnerText);
                EPhaseCode phases = (EPhaseCode)Byte.Parse(xml_elem["Phases"].InnerText);

                if (listOfNodes.ContainsKey(node_lid))
                {
                    throw new Exception("Bad input file. There is already root node with specific lid.");
                }

                if (!roots.ContainsKey(root_lid))
                {
                    MPRootNode rootNode = new MPRootNode(node_lid, root_lid, EPhaseCode.ABC);
                    listOfNodes.Add(node_lid, rootNode);

                    MPRoot root = new MPRoot(root_lid, node_lid);
                    roots.Add(root_lid, root);
                }
            }
        }

        public void ReadSwitchesFromFile()
        {
            Dictionary<long, MPNode> listOfNodes = internalModel.Nodes;
            Dictionary<long, MPBranch> listOfBranches = internalModel.Branches;
            Dictionary<long, MPSwitchDevice> listOfSwitchDevices = internalModel.SwitchDevices;

            String file_name = "Switches.xml";

            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(file_path + file_name);

            XmlNodeList xnList = xml_doc.SelectNodes("/Switches/Switch");

            foreach (XmlElement xml_elem in xnList)
            {
                ESwitchStatus[] active = new ESwitchStatus[3];

                long switch_lid = UInt32.Parse(xml_elem.GetAttribute("LID"));
                long branchLid = UInt32.Parse(xml_elem["Branch"].InnerText);
                long nodeLid = UInt32.Parse(xml_elem["Node"].InnerText);
                EPhaseCode state = (EPhaseCode)Byte.Parse(xml_elem["SwitchState"].InnerText);
                EPhaseCode phases = (EPhaseCode)Byte.Parse(xml_elem["Phases"].InnerText);

                if (!listOfNodes.ContainsKey(nodeLid) && !listOfBranches.ContainsKey(nodeLid))
                {
                    throw new Exception("Bad input file. There is(are) not graph elem(s) with specific lid(s).");
                }

                MPSwitchDevice switchDevice = new MPSwitchDevice(switch_lid, branchLid, nodeLid, phases, state);

                listOfSwitchDevices.Add(switch_lid, switchDevice);
            }
        }

    }
}
