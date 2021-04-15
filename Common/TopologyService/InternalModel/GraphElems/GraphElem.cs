using Common.TopologyService.ExtendedClassess;
using System;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModel.GraphElems
{
	[Serializable]
    [DataContract]
    public abstract class MPGraphElem : MPIdentifiedObject
    {
        protected EPhaseCode phases;			// Originalna faznost
        private EPhaseCode phaseMarker;		    // Aktivna faznost
        private EEnergizationStatus marker;	    // Energizovanost
        private long[] ownerCircuit;            // Celina (koren ili ostrvo) kojoj elemenat pripada

        public MPGraphElem(long lid, EPhaseCode phaseCode) : base(lid)
        {

            this.phases = phaseCode;
            this.phaseMarker = EPhaseCode.NONE;
            this.marker = EEnergizationStatus.TA_UNENERGIZED;
            this.ownerCircuit = new long[3];

        }

        [DataMember]
        public EPhaseCode ActivePhases
        {
            get { return phaseMarker; }
            set { phaseMarker = value; }
        }

        [DataMember]
        public EPhaseCode OriginalPhases
        {
            get { return phases; }
            set { phases = value; }
        }


        [DataMember]
        public long[] OwnerCircuit
        {
            get { return ownerCircuit; }
            set { ownerCircuit = value; }
        }

        [DataMember]
        public EEnergizationStatus Marker
        {
            get { return marker; }
            set { marker = value; }
        }


    }
}
