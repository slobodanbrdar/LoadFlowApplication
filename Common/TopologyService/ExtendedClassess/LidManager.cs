using Common.NetworkModelService;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.TopologyService.ExtendedClassess
{
	[Serializable]
    [KnownType(typeof(CTypeInfo))]
    public class LidManager
    {
        private Dictionary<string, CTypeInfo> types;

        public LidManager()
        {
            this.types = new Dictionary<string, CTypeInfo>(20);
        }

        public long FindFreeTemporaryIndex(string typeName)
        {
            return types[typeName].GetFirstFreeIndex();
        }

        public void RemoveTemporaryIndex(string key, long index)
        {
            types[key].AddFreeIndex(index);
        }

        public void AddType(string type, long baseIndex, long maxIndex)
        {
            if (!types.ContainsKey(type))
            {
                types.Add(type, new CTypeInfo(baseIndex, maxIndex));
            }
        }

        public void RemoveType(string type)
        {
            if (types.ContainsKey(type))
            {
                types.Remove(type);
            }
        }

        public bool CheckType(string type, long lid)
        {
            switch (type)
            {
                case "MPRootNode":
                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(lid) == DMSType.ENERGYSOURCE)
					{
                        return true;
					}
                    else
					{
                        break;
					}
                    //if (types[type].BaseIndex < lid && lid < types[type].MaxIndex)
                    //{
                    //    return true;
                    //}
                    //break;
            }

            return false;
        }
    }
}
