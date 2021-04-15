namespace Common.TopologyService.ExtendedClassess
{
	public enum EBranchEnd : sbyte { UNKNOWN = -1, END_1 = 0, END_2 = 1 }

	public enum ESwitchStatus : sbyte { UNKNOWN = -1, OPEN = 0, CLOSE = 1 }

	public enum EPhaseIndex : uint { A = 0, B = 1, C = 2 }

	public enum EPhaseCode : byte
	{
		ABCN = 0xF,
		ABC = 0xE,
		ABN = 0xD,
		ACN = 0xB,
		BCN = 0x7,
		AB = 0xC,
		AC = 0xA,
		BC = 0x6,
		AN = 0x9,
		BN = 0x5,
		CN = 0x3,
		A = 0x8,
		B = 0x4,
		C = 0x2,
		N = 0x1,
		NONE = 0x0,
		UNKNOWN = 0x0
	}

	public enum EEnergizationStatus : sbyte
	{
		//!Unknown or uncertain
		TA_UNKNOWN = -2,
		//!Unenergized and grounded
		TA_GROUNDED = -1,
		//!Unenergized
		TA_UNENERGIZED = 0,
		//!The element is energized, but not in all phases.
		TA_PARTIAL = 1,
		//!Energized and radial
		TA_ENERGIZED = 2,
		//!Energized from generator-supplied source
		TA_GENERATOR = 3,
		//!In loop
		TA_MESHED = 4

	}
}
