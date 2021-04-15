using Common.TopologyService.ExtendedClassess;
using Common.TopologyService.InternalModel.Circuts;
using Common.TopologyService.InternalModel.GraphElems;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModelBuilder
{
	[Serializable]
    [KnownType(typeof(MPLine))]
    [KnownType(typeof(MPConnectivityBranch))]
    [KnownType(typeof(MPRootNode))]
    [KnownType(typeof(MPBusNode))]
    [KnownType(typeof(MPConnectivityNode))]
    public class CModelFramework
    {
        #region Fields

        //Cvorovi
        private Dictionary<long, MPNode> nodesInMem;

        //Grane
        private Dictionary<long, MPBranch> branchesInMem;

        //Koreni
        private Dictionary<long, MPRoot> rootsInMem;

        //Polja
        private Dictionary<long, MPSwitchDevice> baysInMem;

        //Privremeni elementi
        private Dictionary<long, MPGraphElem> tempsInMem;

        private LidManager lidManager;

        #endregion

        #region Constructor

        public CModelFramework()
        {
            this.nodesInMem = new Dictionary<long, MPNode>(100);
            this.branchesInMem = new Dictionary<long, MPBranch>(100);
            this.rootsInMem = new Dictionary<long, MPRoot>(10);
            this.baysInMem = new Dictionary<long, MPSwitchDevice>(10);
            this.tempsInMem = new Dictionary<long, MPGraphElem>(10);
            //this.shuntsInMem = new Dictionary<long,MPShunt>();

            this.lidManager = new LidManager();

            DefineTypes();
        }

        #endregion 

        #region Properties

        public Dictionary<long, MPNode> Nodes
        {
            get { return nodesInMem; }
        }

        public Dictionary<long, MPBranch> Branches
        {
            get { return branchesInMem; }
        }

        public Dictionary<long, MPRoot> Roots
        {
            get { return rootsInMem; }
        }

        public Dictionary<long, MPSwitchDevice> SwitchDevices
        {
            get { return baysInMem; }
        }

        public Dictionary<long, MPGraphElem> Temps
        {
            get { return tempsInMem; }
        }

        public LidManager LidManager
        {
            get { return lidManager; }
        }

        #endregion

        #region Public methods

        public void DefineTypes()
        {
            //Nodes
            lidManager.AddType("MPRootNode", 1000000, 1099999);
            lidManager.AddType("MPBusNode", 1100000, 1199999);
            lidManager.AddType("MPConnectivityNode", 1200000, 1299999);


            //Branches
            lidManager.AddType("MPLine", 2100000, 2199999);
            lidManager.AddType("MPSupplyLine", 2200000, 2299999);
            lidManager.AddType("MPConnectivityBranch", 2300000, 2399999);

            //Bays
            lidManager.AddType("MPSwitch", 3000000, 3099999);
            lidManager.AddType("MPCompositeSwitch", 3100000, 3199999);

            //Shunts
            //lidManager.AddType("MPShunt",               4000000,    4099999);
            //lidManager.AddType("MPGround",              4100000,    4199999);
            //lidManager.AddType("MPConsumer",            4200000,    4299999);

            //Temporary elements
            lidManager.AddType("MPTempCut", 6100000, 6199999);
            lidManager.AddType("MPTempJumper", 6200000, 6299999);
            lidManager.AddType("MPTempSwitch", 6300000, 6399999);

            //Circuits
            lidManager.AddType("MPRoot", 9100000, 9199999);
            lidManager.AddType("MPIsland", 9200000, 9299999);
        }


        #endregion
    }
}
