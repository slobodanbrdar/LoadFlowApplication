using Common.TopologyService.ExtendedClassess;
using Common.TopologyService.ExtendedClassess.Exceptions;
using Common.TopologyService.InternalModel.Circuts;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using Common.TopologyService.InternalModelBuilder;
using Common.TopologyService.MatrixModel;
using Common.TopologyService.TopologyAnalyzer.AuxiliaryStructures;
using Common.TopologyService.TopologyAnalyzer.Scanners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.TopologyService.TopologyAnalyzer
{
    [Serializable]
    [KnownType(typeof(MPLine))]
    [KnownType(typeof(MPConnectivityBranch))]
    [KnownType(typeof(MPRootNode))]
    [KnownType(typeof(MPBusNode))]
    [KnownType(typeof(MPConnectivityNode))]
    public class CTopologyAnalyzer
    {
        #region Fields

        private Dictionary<long, long> LIDtoIND;
        private List<long> INDtoLID;
        private CModelFramework internalModel;
        private CSparseMatrix matrixModel;
        private BreadthFirstScanner<ActiveNodeInfo> bfScanner;
        private Dictionary<long, CVisitedNode> visitedNodes;
        private Dictionary<long, CVisitedBranch> visitedBranches;
        private CVisitedBranch[] previousBranch;
        private Dictionary<long, bool[]> loops;
        private Dictionary<long, MPRoot> rootsInParallel;

        #endregion

        #region Properties

        public CModelFramework InternalModel
        {
            get { return internalModel; }
            set { internalModel = value; }
        }

        #endregion

        #region Constructor
        public CTopologyAnalyzer(CSparseMatrix matrixModel, Dictionary<long, long> LIDtoIND, List<long> INDtoLID, CModelFramework internalModel)
        {
            this.matrixModel = matrixModel;
            this.LIDtoIND = LIDtoIND;
            this.INDtoLID = INDtoLID;
            this.bfScanner = new BreadthFirstScanner<ActiveNodeInfo>();
            this.visitedBranches = new Dictionary<long, CVisitedBranch>(100);
            this.visitedNodes = new Dictionary<long, CVisitedNode>(100);
            this.loops = new Dictionary<long, bool[]>();
            this.rootsInParallel = new Dictionary<long, MPRoot>();
            this.internalModel = internalModel;
        }

        #endregion

        #region Process Roots

        public void AnalyzeTopology(Dictionary<long, MPRoot> roots)
        {
            //OBRADA KORENA
            foreach (var root in roots.Values)
            {
                if (root.UpToDate == false)     //obradjuju se samo neazurni koreni
                {
                    UpdateRootTopology(root);
                }
            }
        }

        public void UpdateRootTopology(MPRoot root)
        {

            previousBranch = new CVisitedBranch[3];

            bool oneMoreTime = true;

            rootsInParallel.Clear();
            rootsInParallel.Add(root.Lid, root);

            do
            {
                SetUpProcessOfRoot(root);

                try
                {
                    ProcessRoot(root);

                    foreach (var processedRoot in rootsInParallel.Values)
                    {
                        processedRoot.UpToDate = true;
                    }
                    oneMoreTime = false;
                }
                catch (ParallelRootFoundException)
                {
                    ;// Ponavlja se obrada korena zbog uocenog dodatnog paralelnog korena
                }
            } while (oneMoreTime == true);  // Dok svi koreni ne budu uspesno procesuirani

        }

        private void SetUpProcessOfRoot(MPRoot currRoot)
        {
            // Init
            bfScanner.Init();
            visitedNodes.Clear();
            visitedBranches.Clear();
            loops.Clear();

            foreach (MPRoot parallelRoot in rootsInParallel.Values)
            {
                long rootNodeLid = parallelRoot.SourceObject.Value;
                CVisitedNode rootNode = new CVisitedNode(rootNodeLid, true);
                rootNode.OwnerCircuit = new long[] { rootNodeLid, rootNodeLid, rootNodeLid };
                visitedNodes.Add(rootNodeLid, rootNode);

                long nodeIndex = LIDtoIND[rootNodeLid];
                bfScanner.AddRange(new List<ActiveNodeInfo>(3)
                {
                    new ActiveNodeInfo(nodeIndex + (long)EPhaseIndex.A, (long)EPhaseIndex.A),
                    new ActiveNodeInfo(nodeIndex + (long)EPhaseIndex.B, (long)EPhaseIndex.B),
                    new ActiveNodeInfo(nodeIndex + (long)EPhaseIndex.C, (long)EPhaseIndex.C)
                });

                parallelRoot.InitTopology();
            }

            long invalidIndex = 0;
            previousBranch = new CVisitedBranch[3]
            {
                new CVisitedBranch(invalidIndex),
                new CVisitedBranch(invalidIndex),
                new CVisitedBranch(invalidIndex)
            };
        }

        private void ProcessRoot(MPRoot currRoot)
        {
            // Pretrazivanje grafa korena
            TraverseRoot(currRoot);

            // Bojenje pronadjenih petlji
            ProcessLoops();

            // Cuvanje rezultata topologije
            SaveNodeResultsForRoot(visitedNodes, currRoot);
            SaveBranchResultsForRoot(visitedBranches, currRoot);

            return;

        }

        private void TraverseRoot(MPRoot currRoot)
        {
            while (bfScanner.HasNext())							// Da li su procesirani svi cvorovi koji bi trebali biti
            {
                ActiveNodeInfo nextNode = bfScanner.GetNext();

                List<long> neigbours = ProcessNeighboursInRoot(nextNode.NodeIndex, nextNode.PhaseIndex, currRoot.Lid);      // Uzima se novi cvor za procesiranje

                bfScanner.AddRange(neigbours.Select(neighbour => new ActiveNodeInfo(neighbour, nextNode.PhaseIndex)).ToList()); // Dodaju se pronadjeni susedi
            }
        }

        private List<long> ProcessNeighboursInRoot(long upNodeInd, long phaseIndex, long rootLid)
        {
            // Dobavljaju se susedne grane i cvorovi iz odgovarajuceg reda matrice
            List<Neighbour> neighbours = GetNeighboursFromMatrix(upNodeInd);

            List<long> unvisitedNeigbours = new List<long>(neighbours.Count); // Neposeceni susedi datog cvora i faznost koju nose

            foreach (var neighbour in neighbours)
            {
                long upNodeLid = INDtoLID[(int)(upNodeInd - phaseIndex)];               // Polazni cvor
                long branchLid = neighbour.Branch;                                      // Susedni cvor
                long downNodeLid = INDtoLID[(int)(neighbour.NodeIndex - phaseIndex)];   // Grana koja spaja ta dva cvora

                CVisitedBranch visitedBranch;
                if (!visitedBranches.TryGetValue(branchLid, out visitedBranch)) // Ukoliko grana ne postoji u evidenciji
                {
                    visitedBranch = new CVisitedBranch(branchLid);
                    visitedBranches.Add(branchLid, visitedBranch);      // tada ju je potrebno dodati
                }

                if (visitedBranch.Visited[phaseIndex])       // Provera da li je grana prethodno posecena
                {
                    continue;
                }

                // Proverava se da sused nije korenski cvor
                CheckAndReportRootsInParallel(downNodeLid, rootLid);

                // Obrada susedne grane
                visitedBranch.Visited[phaseIndex] = true;                                   // Poseti granu za tu fazu

                visitedBranch.OwnerCircuit[phaseIndex] = rootLid;                           // Obelezi koren kome pripada

                visitedBranch.UpNode[phaseIndex] = upNodeLid;                               // Odredi gornji i donji cvor grane
                visitedBranch.DownNode[phaseIndex] = downNodeLid;

                visitedBranch.PrevInRoot[phaseIndex] = previousBranch[phaseIndex].Lid;      // Povezi prethodnu i narednu granu u sloju
                previousBranch[phaseIndex].NextInRoot[phaseIndex] = branchLid;

                previousBranch[phaseIndex] = visitedBranch;                         // Trenutna grana ce biti prethodna grana narednoj grani

                CVisitedNode visitedNode;
                if (!visitedNodes.TryGetValue(downNodeLid, out visitedNode)) // Ukoliko cvor ne postoji u evidenciji...
                {
                    visitedNode = new CVisitedNode(downNodeLid);
                    visitedNodes.Add(downNodeLid, visitedNode);     // ...tada ga je potrebno dodati
                }

                //DODAVANJE NEOBRADJENOG CVORA U GRUPU
                if (!visitedNode.Visited[phaseIndex])	// Ukoliko susedni cvor do sada nije posecivan u fazi po kojoj se obradjuje
                {
                    visitedNode.Visited[phaseIndex] = true;		// Poseti cvor
                    visitedNode.OwnerCircuit[phaseIndex] = rootLid; // Obelezi koren kojem  pripada

                    unvisitedNeigbours.Add(neighbour.NodeIndex); //susedi koji jos nisu do sada procesuirani se dodaju u listu za obradu              

                }
                //OBELEZAVANJE PETLJI
                //Ukoliko je cvor vec ranije posecen preko iste faze  to je jasan pokazatelj postojanja petlje (cvor smo vec prethodno posetili preko neke druge grane)
                else
                {
                    ReportLoop(branchLid, phaseIndex);
                }

                // Zabelezi susedne grane za gornji i donji cvor
                visitedNode.Parents[phaseIndex].Add(branchLid);
                visitedNodes[upNodeLid].Children[phaseIndex].Add(branchLid);
            }
            return unvisitedNeigbours;
        }

        List<Neighbour> GetNeighboursFromMatrix(long upNodeInd)
        {
            List<Neighbour> neighbours = new List<Neighbour>((int)matrixModel.GetSumByRow(upNodeInd));

            // Tehnika retkih matrica
            for (long index = matrixModel.GetFirstInRow(upNodeInd);     // indeks prvog suseda u redu datog cvora
                 index > 0;                                             // indeks = 0 ako nema vise elemenata u redu
                 index = matrixModel.GetNextInRow(index))				// indeks sledeceg suseda u redu datog cvora		
            {
                neighbours.Add(new Neighbour()
                {
                    Branch = matrixModel.GetValue(index),
                    NodeIndex = matrixModel.GetColumnIndex(index)
                });
            }

            return neighbours;
        }

        private void CheckAndReportRootsInParallel(long downLid, long masterRoot)
        {
            if (internalModel.LidManager.CheckType("MPRootNode", downLid))
            {
                long rootLid = internalModel.Nodes[downLid].OwnerCircuit[0];
                MPRoot root = internalModel.Roots[rootLid];

                rootsInParallel[root.Lid] = root;
                root.MasterRoot = masterRoot;

                rootsInParallel[masterRoot].SlaveRoots.Add(rootLid);

                throw new ParallelRootFoundException("Parallel root is founded. Process will be repeated.");

            }
        }

        private void ReportLoop(long branchLid, long phaseIndex)
        {
            bool[] phases;
            if (!loops.TryGetValue(branchLid, out phases))
            {
                phases = new bool[3];
                loops[branchLid] = phases;
            }
            phases[phaseIndex] = true;
        }

        private void SaveBranchResultsForRoot(Dictionary<long, CVisitedBranch> visitedBranches, MPRoot root)
        {
            foreach (CVisitedBranch visitedbranch in visitedBranches.Values)
            {
                MPBranch branch = internalModel.Branches[visitedbranch.Lid];
                branch.NextInRoot = visitedbranch.NextInRoot;
                branch.PrevInRoot = visitedbranch.PrevInRoot;
                branch.UpNode = visitedbranch.UpNode;
                branch.DownNode = visitedbranch.DownNode;
                branch.OwnerCircuit = visitedbranch.OwnerCircuit;
                branch.ActivePhases = CalculateActivePhases(visitedbranch.Visited);
                branch.Marker = CalculateEnergization(branch.OriginalPhases, branch.ActivePhases, visitedbranch.Loop);

                root.GraphElems.Add(visitedbranch.Lid);
            }
        }

        private void SaveNodeResultsForRoot(Dictionary<long, CVisitedNode> visitedNodes, MPRoot root)
        {
            foreach (CVisitedNode visitedNode in visitedNodes.Values)
            {
                MPNode node = internalModel.Nodes[visitedNode.Lid];
                node.Children = visitedNode.Children;
                node.Parents = visitedNode.Parents;
                node.OwnerCircuit = visitedNode.OwnerCircuit;
                node.ActivePhases = CalculateActivePhases(visitedNode.Visited);
                node.Marker = CalculateEnergization(node.OriginalPhases, node.ActivePhases, visitedNode.Loop);

                root.GraphElems.Add(visitedNode.Lid);
            }
        }

        private EPhaseCode CalculateActivePhases(bool[] visitedPhases)
        {
            byte activePhases;

            activePhases = visitedPhases[0] == true ? (byte)EPhaseCode.A : (byte)EPhaseCode.NONE;
            activePhases |= visitedPhases[1] == true ? (byte)EPhaseCode.B : (byte)EPhaseCode.NONE;
            activePhases |= visitedPhases[2] == true ? (byte)EPhaseCode.C : (byte)EPhaseCode.NONE;

            return (EPhaseCode)activePhases;
        }

        private EEnergizationStatus CalculateEnergization(EPhaseCode originalPhases, EPhaseCode activePhases, bool loop)
        {
            if (loop)
            {
                return EEnergizationStatus.TA_MESHED;
            }
            if (activePhases == originalPhases)
            {
                return EEnergizationStatus.TA_ENERGIZED;
            }
            if ((activePhases & originalPhases) == activePhases)
            {
                return EEnergizationStatus.TA_PARTIAL;
            }
            if (activePhases == EPhaseCode.NONE)
            {
                return EEnergizationStatus.TA_UNENERGIZED;
            }
            return EEnergizationStatus.TA_UNKNOWN;
        }

        private void ProcessLoops()
        {
            while (loops.Count != 0)
            {
                ProccessLoopByPhase(loops.First());
                loops.Remove(loops.First().Key);
            }

        }

        private void ProccessLoopByPhase(KeyValuePair<long, bool[]> loopInfo)
        {
            for (byte phaseIndex = 0; phaseIndex < 3; phaseIndex++)
            {
                if (loopInfo.Value[phaseIndex])
                {
                    long rightBranchLid = loopInfo.Key;
                    CVisitedBranch rightBranch = visitedBranches[rightBranchLid];

                    long downNodeLid = (long)rightBranch.DownNode[phaseIndex];
                    CVisitedNode downNode = visitedNodes[downNodeLid];
                    downNode.Loop = true;

                    long leftBranchLid = downNode.Parents[phaseIndex].First();
                    CVisitedBranch leftBranch = visitedBranches[leftBranchLid];


                    //Pronadji konturu
                    CContourScanner contourIterator = new CContourScanner();
                    contourIterator.Begin();
                    contourIterator.MarkContour(phaseIndex, leftBranch, rightBranch, visitedBranches, visitedNodes);

                    //Markiranje leve i desne strane petlje
                    CVisitedBranch branch; CVisitedNode node;
                    while (contourIterator.Next(out branch, out node, visitedNodes, phaseIndex) == true)
                    {
                        branch.Loop = true;
                        node.Loop = true;

                    }
                }

            }

        }

        #endregion

        #region Process Islands
        private void ProcessIsland()
        {
            // Pretrazivanje nenapojenog dela grafa
            TraverseIsland();

            // Cuvanje rezultata
            SaveNodeResultsForIsland(visitedNodes);
            SaveBranchResultsForIsland(visitedBranches);
        }

        private void TraverseIsland()   // Analogno obilasku grafa korena
        {
            while (bfScanner.HasNext())
            {
                ActiveNodeInfo nextNode = bfScanner.GetNext();

                List<long> neigbours = ProcessNodeInIsland(nextNode.NodeIndex, nextNode.PhaseIndex);

                bfScanner.AddRange(neigbours.Select(neighbour => new ActiveNodeInfo(neighbour, nextNode.PhaseIndex)).ToList());

            }
        }

        private List<long> ProcessNodeInIsland(long upNodeInd, long phaseIndex)
        {
            // Dobavljaju se susedne grane i cvorovi iz odgovarajuceg reda matrice
            List<Neighbour> neighbours = GetNeighboursFromMatrix(upNodeInd);

            List<long> unvisitedNeighbours = new List<long>(neighbours.Count);

            foreach (var neighbour in neighbours)
            {
                long branchLid = neighbour.Branch;            //susedni cvor //grana koja spaja ta dva cvora
                long downNodeLid = INDtoLID[(int)(neighbour.NodeIndex - phaseIndex)];          //susedni cvor

                CVisitedBranch visitedBranch;
                if (!visitedBranches.TryGetValue(branchLid, out visitedBranch))    // kreiraj ukoliko ne postoji
                {
                    visitedBranch = new CVisitedBranch(branchLid);
                    visitedBranches.Add(branchLid, visitedBranch);
                }
                if (visitedBranch.Visited[phaseIndex])       // proveri da li je grana prethodno posecena
                {
                    continue;
                }

                visitedBranch.Visited[phaseIndex] = true;                              // poseti granu u toj fazi

                CVisitedNode visitedNode;

                if (!visitedNodes.TryGetValue(downNodeLid, out visitedNode))	// u koliko cvor do sada nije uopste posecivan
                {
                    visitedNode = new CVisitedNode(downNodeLid);
                    visitedNodes.Add(downNodeLid, visitedNode);
                }

                if (!visitedNode.Visited[phaseIndex])	//u koliko cvor do sada nije posecivan u fazi po kojoj se obradjuje
                {
                    visitedNode.Visited[phaseIndex] = true;		//poseti cvor

                    unvisitedNeighbours.Add(neighbour.NodeIndex); //susedi koji jos nisu procesuirani dodaju se u listu za obradu              

                }
            }

            return unvisitedNeighbours;
        }

        private void SaveNodeResultsForIsland(Dictionary<long, CVisitedNode> visitedNodes)
        {
            foreach (CVisitedNode visitedNode in visitedNodes.Values)
            {
                MPNode node = internalModel.Nodes[visitedNode.Lid];
                node.Parents = new List<long>[3] { new List<long>(3), new List<long>(3), new List<long>(3) };
                node.Children = new List<long>[3] { new List<long>(5), new List<long>(5), new List<long>(5) };
                node.OwnerCircuit = new long[3];
                node.ActivePhases = EPhaseCode.NONE;
                node.Marker = EEnergizationStatus.TA_UNENERGIZED;
            }
        }

        private void SaveBranchResultsForIsland(Dictionary<long, CVisitedBranch> visitedBranches)
        {
            foreach (CVisitedBranch visitedbranch in visitedBranches.Values)
            {
                MPBranch branch = internalModel.Branches[visitedbranch.Lid];
                branch.NextInRoot = new long[3];
                branch.PrevInRoot = new long[3];
                branch.UpNode = new long[3];
                branch.DownNode = new long[3];
                branch.OwnerCircuit = new long[3];
                branch.ActivePhases = EPhaseCode.NONE;
                branch.Marker = EEnergizationStatus.TA_UNENERGIZED;
            }

        }
        #endregion

        #region Update Topology

        private void FindRootsToUpdate(long branch)
        {
            long downNode = internalModel.Branches[branch].EndNodes[0];
            long upNode = internalModel.Branches[branch].EndNodes[1];

            List<long> rootsToUpdate = internalModel.Nodes[downNode].OwnerCircuit.Union(
                internalModel.Nodes[upNode].OwnerCircuit).Distinct().Where(x => x != 0).ToList();

            rootsToUpdate.ForEach(rootToUpdate => internalModel.Roots[rootToUpdate].UpToDate = false);
        }

        public void UpdateTopologyAfterRemovingBranch(long branch)
        {
            MPBranch stateBranch = internalModel.Branches[branch];

            if (stateBranch.Marker != EEnergizationStatus.TA_UNENERGIZED)
            {
                FindRootsToUpdate(branch);

                bfScanner.Init();

                if ((stateBranch.OriginalPhases & EPhaseCode.A) != EPhaseCode.NONE)
                {
                    long nodeLid = stateBranch.DownNode[(long)EPhaseIndex.A];
                    bfScanner.Add(new ActiveNodeInfo(LIDtoIND[nodeLid] + (long)EPhaseIndex.A, (long)EPhaseIndex.A));
                }

                if ((stateBranch.OriginalPhases & EPhaseCode.B) != EPhaseCode.NONE)
                {
                    long nodeLid = stateBranch.DownNode[(long)EPhaseIndex.B];
                    bfScanner.Add(new ActiveNodeInfo(LIDtoIND[nodeLid] + (long)EPhaseIndex.B, (long)EPhaseIndex.B));
                }

                if ((stateBranch.OriginalPhases & EPhaseCode.C) != EPhaseCode.NONE)
                {
                    long nodeLid = stateBranch.DownNode[(long)EPhaseIndex.C];
                    bfScanner.Add(new ActiveNodeInfo(LIDtoIND[nodeLid] + (long)EPhaseIndex.C, (long)EPhaseIndex.C));
                }

                ProcessIsland();

                AnalyzeTopology(internalModel.Roots);
            }
        }

        public void UpdateTopologyAfterAddingBranch(long branch)
        {
            FindRootsToUpdate(branch);

            AnalyzeTopology(internalModel.Roots);
        }

        public void UpdateTopologyAfterClosingPhaseOfSwitch(long branch, EPhaseCode phaseToClose)
        {
            FindRootsToUpdate(branch);

            AnalyzeTopology(internalModel.Roots);
        }

        public void UpdateTopologyAfterOpeningPhaseOfSwitch(long branch, EPhaseCode phaseToClose)
        {
            MPBranch stateBranch = internalModel.Branches[branch];

            if (stateBranch.Marker != EEnergizationStatus.TA_UNENERGIZED)
            {
                FindRootsToUpdate(branch);

                EPhaseIndex phaseIndex = PhaseHelper.PhaseCodeToIndex(phaseToClose);

                long nodeLid = stateBranch.DownNode[(long)phaseIndex];
                bfScanner.Init();
                bfScanner.Add(new ActiveNodeInfo(LIDtoIND[nodeLid] + (long)phaseIndex, (long)phaseIndex));

                ProcessIsland();

                AnalyzeTopology(internalModel.Roots);
            }
        }

        #endregion
    }
}
