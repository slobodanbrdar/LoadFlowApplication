using Common.TopologyService.ExtendedClassess;
using Common.TopologyService.InternalModel.GraphElems;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using Common.TopologyService.InternalModelBuilder;
using System.Collections.Generic;

namespace Common.TopologyService.MatrixModel
{
	public class CMatrixModelBuilder
    {
        #region Fields

        private CSparseMatrix matrixModel;
        public Dictionary<long, long> LIDtoIND { private set; get; }
        public List<long> INDtoLID { private set; get; }

        private IndexManager matrixIndexManager;

        private CModelFramework internalModel;

        #endregion

        public CMatrixModelBuilder(CSparseMatrix matrixModel, CModelFramework internalModel)
        {
            this.matrixModel = matrixModel;
            this.internalModel = internalModel;
        }

        #region LID-index dictionaries

        public void CreateInitialLidDictionary()
        {
            Dictionary<long, MPNode> listOfNodes = internalModel.Nodes;

            LIDtoIND = new Dictionary<long, long>(listOfNodes.Count * 2);         //foreach node
            INDtoLID = new List<long>((listOfNodes.Count) * 4);                 //foreach node and phase
            this.matrixIndexManager = new IndexManager(3, (listOfNodes.Count) * 4);

            LIDtoIND.Add(0, 0);     //pozicija nultog cvora odgovara korenu
            INDtoLID.AddRange(new List<long>(3) { 0, 0, 0 });

        }

        private long CreateLidIndexPair(long lid)
        {
            long index;

            if (matrixIndexManager.FindFreeIndex(out index))
            {
                INDtoLID.AddRange(new List<long>(3) { lid, lid, lid });
            }
            else
            {
                INDtoLID[(int)index + (int)EPhaseIndex.A] = lid;
                INDtoLID[(int)index + (int)EPhaseIndex.B] = lid;
                INDtoLID[(int)index + (int)EPhaseIndex.C] = lid;
            }

            LIDtoIND.Add(lid, index);

            return index;
        }

        public void RemoveLidIndexPair(long lid)
        {
            long baseIndex = LIDtoIND[lid];

            LIDtoIND.Remove(lid);

            INDtoLID[(int)baseIndex + (int)EPhaseIndex.A] = 0;
            INDtoLID[(int)baseIndex + (int)EPhaseIndex.B] = 0;
            INDtoLID[(int)baseIndex + (int)EPhaseIndex.C] = 0;

            matrixIndexManager.RemoveIndex(baseIndex);

        }

        #endregion

        #region Initial model
        public void CreateInitialMatrixModel()
        {
            Dictionary<long, MPNode> listOfNodes = internalModel.Nodes;
            Dictionary<long, MPBranch> listOfBranches = internalModel.Branches;

            matrixModel.AddSpaceForRows(listOfNodes.Count * 4);     // 3x za svaku fazu i 1x za dodatne privremene elemente u mrezi
            matrixModel.AddSpaceForElems(listOfBranches.Count * 8); // 2x u odnosu na broj cvorova

            foreach (long node in listOfNodes.Keys)
            {
                CreateLidIndexPair(node);
            }

            foreach (var branch in listOfBranches.Values)
            {
                AddBranchToModel(branch);
            }
        }

        private void AddNodeInModel(long node)
        {
            CreateLidIndexPair(node);
        }

        public void InsertSwitchDeviceFromInternalToMatrixModel()
        {
            Dictionary<long, MPSwitchDevice> listOfBays = internalModel.SwitchDevices;

            foreach (var bay in listOfBays.Values)
            {
                InsertSwitchDeviceInMatrixModel(bay);
            }
        }

        private void InsertSwitchDeviceInMatrixModel(MPSwitchDevice bay)
        {
            MPBranch oldBranch = internalModel.Branches[bay.EndBranch];

            // Krajevi grane na kojoj se nalazi prekidac
            long node2Lid = bay.EndNode;
            long node1Lid = oldBranch.EndNodes[0] == node2Lid ? oldBranch.EndNodes[1] : oldBranch.EndNodes[0];

            // Kreiranje novog cvora sa faznoscu stare grane
            long connNodeLid = internalModel.LidManager.FindFreeTemporaryIndex("MPConnectivityNode");
            MPConnectivityNode connNode = new MPConnectivityNode(connNodeLid, oldBranch.OriginalPhases);
            AddNodeInModel(connNodeLid);
            internalModel.Nodes.Add(connNodeLid, connNode);

            // Kreiranje prve grane sa faznoscu stare grane
            long connBranch1Lid = internalModel.LidManager.FindFreeTemporaryIndex("MPConnectivityBranch");
            MPConnectivityBranch connBranch1 = new MPConnectivityBranch(connBranch1Lid, oldBranch.Lid, oldBranch.OriginalPhases, node1Lid, connNodeLid);
            AddBranchToModel(connBranch1);
            internalModel.Branches.Add(connBranch1Lid, connBranch1);

            // Kreiranje druge grane sa faznoscu i stanjem prekidaca
            long connBranch2Lid = internalModel.LidManager.FindFreeTemporaryIndex("MPConnectivityBranch");
            MPConnectivityBranch connBranch2 = new MPConnectivityBranch(connBranch2Lid, oldBranch.Lid, bay.OriginalPhases, connNodeLid, node2Lid);
            AddBranchToModel(connBranch2, bay.State);
            internalModel.Branches.Add(connBranch2Lid, connBranch2);

            // Povezivanje prekidaca sa granom koja modelira njegovo stanje
            bay.StateBranch = connBranch2Lid;

            // Brisanje stare grane
            RemoveBranchFromModel(oldBranch);
            internalModel.Branches.Remove(oldBranch.Lid);

        }
        #endregion

        #region Add and remove branch

        private void AddBranchToModel(MPBranch branch, EPhaseCode? phasesToAdd = null)
        {
            long node1Lid, node2Lid;
            long[] node1Ind, node2Ind;

            phasesToAdd = phasesToAdd ?? branch.OriginalPhases & ~EPhaseCode.N;

            node1Lid = branch.EndNodes[(sbyte)EBranchEnd.END_1];
            node2Lid = branch.EndNodes[(sbyte)EBranchEnd.END_2];

            GetIndicesFromNode(node1Lid, out node1Ind);
            GetIndicesFromNode(node2Lid, out node2Ind);

            long branchLid = branch.Lid;

            //A FAZA
            if ((phasesToAdd & EPhaseCode.A) != 0)
            {
                AddBranchToMatrix(node1Ind[(long)EPhaseIndex.A], node2Ind[(long)EPhaseIndex.A], branchLid, node1Lid, node2Lid);
            }

            //B FAZA
            if ((phasesToAdd & EPhaseCode.B) != 0)
            {
                AddBranchToMatrix(node1Ind[(long)EPhaseIndex.B], node2Ind[(long)EPhaseIndex.B], branchLid, node1Lid, node2Lid);
            }

            //C FAZA
            if ((phasesToAdd & EPhaseCode.C) != 0)
            {
                AddBranchToMatrix(node1Ind[(long)EPhaseIndex.C], node2Ind[(long)EPhaseIndex.C], branchLid, node1Lid, node2Lid);
            }
        }

        private void RemoveBranchFromModel(MPBranch branch, EPhaseCode? phasesToRemove = null)
        {
            long node1, node2;
            long[] node1Ind, node2Ind;

            phasesToRemove = phasesToRemove ?? branch.OriginalPhases & ~EPhaseCode.N;

            node1 = branch.EndNodes[(sbyte)EBranchEnd.END_1];
            node2 = branch.EndNodes[(sbyte)EBranchEnd.END_2];

            GetIndicesFromNode(node1, out node1Ind);
            GetIndicesFromNode(node2, out node2Ind);

            //A FAZA
            if ((phasesToRemove & EPhaseCode.A) != 0)
            {
                RemoveBranchFromMatrix(node1Ind[(long)EPhaseIndex.A], node2Ind[(long)EPhaseIndex.A]);
            }

            //B FAZA
            if ((phasesToRemove & EPhaseCode.B) != 0)
            {
                RemoveBranchFromMatrix(node1Ind[(long)EPhaseIndex.B], node2Ind[(long)EPhaseIndex.B]);
            }

            //C FAZA
            if ((phasesToRemove & EPhaseCode.C) != 0)
            {
                RemoveBranchFromMatrix(node1Ind[(long)EPhaseIndex.C], node2Ind[(long)EPhaseIndex.C]);
            }
        }

        private void AddBranchToMatrix(long node1Ind, long node2Ind, long branchLid, long node1Lid, long node2Lid)
        {
            matrixModel.AddElement(node1Ind, node2Ind, branchLid);
            matrixModel.AddElement(node2Ind, node1Ind, branchLid);
        }

        private void RemoveBranchFromMatrix(long index1, long index2)
        {
            matrixModel.RemoveElement(index1, index2);
            matrixModel.RemoveElement(index2, index1);
        }

        private void GetIndicesFromNode(long lid, out long[] index)
        {
            var baseIndex = LIDtoIND[lid];
            index = new long[3] { baseIndex + (long)EPhaseIndex.A, baseIndex + (long)EPhaseIndex.B, baseIndex + (long)EPhaseIndex.C };

        }
        #endregion

        #region Switch status change
        public void OpenSwitch(MPSwitchDevice switchDevice, EPhaseCode phasesToRemove)
        {
            MPConnectivityBranch statebranch = (MPConnectivityBranch)internalModel.Branches[switchDevice.StateBranch];

            RemoveBranchFromModel(statebranch, phasesToRemove);

            switchDevice.State = EPhaseCode.NONE;

        }

        public void CloseSwitch(MPSwitchDevice switchDevice, EPhaseCode phasesToAdd)
        {
            MPConnectivityBranch statebranch = (MPConnectivityBranch)internalModel.Branches[switchDevice.StateBranch];

            AddBranchToModel(statebranch, phasesToAdd);

            switchDevice.State = switchDevice.OriginalPhases;
        }

        public void OpenPhaseOfSwitch(MPSwitchDevice switchDevice, EPhaseCode phaseToChange)
        {
            MPConnectivityBranch statebranch = (MPConnectivityBranch)internalModel.Branches[switchDevice.StateBranch];

            RemoveBranchFromModel(statebranch, phaseToChange);

            switchDevice.State = switchDevice.State & (~phaseToChange & EPhaseCode.ABC);
        }

        public void ClosePhaseOfSwitch(MPSwitchDevice switchDevice, EPhaseCode phaseToChange)
        {
            MPConnectivityBranch statebranch = (MPConnectivityBranch)internalModel.Branches[switchDevice.StateBranch];

            AddBranchToModel(statebranch, phaseToChange);

            switchDevice.State = switchDevice.State | phaseToChange;
        }


        //public void UpdateCompositeSwitch(MPSwitchDevice switchDevice, EPhaseCode newState)
        //{
        //    switchDevice.State = newState; 

        //    MPConnectivityBranch statebranch = (MPConnectivityBranch)internalModel.Branches[switchDevice.StateBranch];

        //    EPhaseCode removePhases = statebranch.OriginalPhases & ((~newState) & EPhaseCode.ABC);
        //    RemoveBranchFromModel(statebranch, removePhases);

        //    EPhaseCode addPhases = newState & ((~statebranch.OriginalPhases) & EPhaseCode.ABC);
        //    AddBranchToModel(statebranch, addPhases);            
        //}

        #endregion

        #region Temporary Jumper
        public long AddJumper(long node1, long node2, EPhaseCode phaseCode)
        {
            long jumperLid = internalModel.LidManager.FindFreeTemporaryIndex("MPTempJumper");
            MPJumper jumper = new MPJumper(jumperLid, phaseCode, node1, node2);
            internalModel.Branches.Add(jumperLid, jumper);
            AddBranchToModel(jumper);

            return jumperLid;
        }

        public void RemoveJumper(long jumperLid)
        {
            MPJumper jumper = (MPJumper)internalModel.Temps[jumperLid];
            RemoveBranchFromModel(jumper);
            internalModel.LidManager.RemoveTemporaryIndex("MPTempJumper", jumperLid);
        }
        #endregion
    }
}
