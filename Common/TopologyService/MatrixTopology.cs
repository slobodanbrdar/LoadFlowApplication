using Common.TopologyService.InternalModelBuilder;
using Common.TopologyService.MatrixModel;
using Common.TopologyService.TopologyAnalyzer;

namespace Common.TopologyService
{
	public class CMatrixTopology
    {
        #region Fields

        private CModelFramework internalModel;
        private CInternalModelBuilder internalModelBuilder;
        private CSparseMatrix matrixModel;
        private CMatrixModelBuilder matrixModelBuilder;
        private CTopologyAnalyzer topologyAnalyser;

        #endregion

        #region Properties

        public CModelFramework InternalModel
        {
            get { return internalModel; }
        }

        public CTopologyAnalyzer TopologyAnalyzer
        {
            get { return topologyAnalyser; }
        }

        public CMatrixModelBuilder MatrixModelBuilder
        {
            get { return matrixModelBuilder; }
        }
        #endregion


        public void RunSparseMatrixApplication(string path)
        {

            internalModel = new CModelFramework();

            // Read data from files
            internalModelBuilder = new CInternalModelBuilder(internalModel, path);
            internalModelBuilder.ReadSchemaFromFile();

            // Create matrix model
            matrixModel = new CSparseMatrix();
            matrixModelBuilder = new CMatrixModelBuilder(matrixModel, internalModel);
            matrixModelBuilder.CreateInitialLidDictionary();
            matrixModelBuilder.CreateInitialMatrixModel();
            matrixModelBuilder.InsertSwitchDeviceFromInternalToMatrixModel();

            // Analyze topology
            topologyAnalyser = new CTopologyAnalyzer(matrixModel, matrixModelBuilder.LIDtoIND, matrixModelBuilder.INDtoLID, internalModel);
            //topologyAnalyser.AnalyzeTopology(internalModel.Roots);
            foreach (var root in internalModel.Roots.Values)
            {
                if (root.UpToDate == false)     //obradjuju se samo neazurni koreni
                {
                    topologyAnalyser.UpdateRootTopology(root);
                }
            }

            return;
        }
    }
}
