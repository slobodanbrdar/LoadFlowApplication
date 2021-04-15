using System;

namespace Common.TopologyService.ExtendedClassess
{
	public static class PhaseHelper
    {
        public static bool PhaseExistInPhaseCode(EPhaseCode ePhaseCode, EPhaseIndex phaseIndex)
        {
            if (((uint)ePhaseCode & (uint)Math.Pow(2, 3 - (byte)phaseIndex)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static EPhaseCode PhaseIndexToPhaseCode(EPhaseIndex phaseIndex)
        {
            switch (phaseIndex)
            {
                case EPhaseIndex.A:
                    return EPhaseCode.A;

                case EPhaseIndex.B:
                    return EPhaseCode.B;

                case EPhaseIndex.C:
                    return EPhaseCode.C;

                default:
                    return EPhaseCode.UNKNOWN;
            }
        }


        public static EPhaseIndex PhaseCodeToIndex(EPhaseCode phaseCode)
        {
            switch (phaseCode)
            {
                case EPhaseCode.A:
                    return EPhaseIndex.A;

                case EPhaseCode.B:
                    return EPhaseIndex.B;

                case EPhaseCode.C:
                    return EPhaseIndex.C;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
