using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.NetworkModelService
{
	public enum PhaseCode : short
	{
		Unknown = 0x0,
		N = 0x1,
		C = 0x2,
		CN = 0x3,
		B = 0x4,
		BN = 0x5,
		BC = 0x6,
		BCN = 0x7,
		A = 0x8,
		AN = 0x9,
		AC = 0xA,
		ACN = 0xB,
		AB = 0xC,
		ABN = 0xD,
		ABC = 0xE,
		ABCN = 0xF
	}

	public enum PhaseShuntConnectionKind : short
	{
		D = 1,      //Delta connection
		I = 2,      //Independent winding, for single-phase connections
		Y = 3,      //Wye connection
		Yn = 4,     //Wye, with neutral brought out for grounding
	}


	public enum WindingConnection : short
	{
		Y = 1,      // Wye
		D = 2,      // Delta
		Z = 3,      // ZigZag
		I = 4,      // Single-phase connection. Phase-to-phase or phase-to-ground is determined by elements' phase attribute.
		Scott = 5,   // Scott T-connection. The primary winding is 2-phase, split in 8.66:1 ratio
		OY = 6,     // 2-phase open wye. Not used in Network Model, only as result of Topology Analysis.
		OD = 7      // 2-phase open delta. Not used in Network Model, only as result of Topology Analysis.
	}

	public enum WindingType : short
	{
		None = 0,
		Primary = 1,
		Secondary = 2,
		Tertiary = 3
	}
}
