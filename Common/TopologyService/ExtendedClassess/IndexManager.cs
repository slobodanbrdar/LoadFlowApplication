using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.TopologyService.ExtendedClassess
{
	[Serializable]
    public class IndexManager
    {
        private List<long> freeIndices;
        private long maxUsedIndex;
        private long stepSize;

        public IndexManager(long step, int size)
        {
            this.freeIndices = new List<long>(size);
            this.maxUsedIndex = 0;                  //0 indeks se ne koristi!
            this.stepSize = step;
        }

        public bool FindFreeIndex(out long index)
        {
            if (freeIndices.Count == 0)
            {
                maxUsedIndex += stepSize;
                index = maxUsedIndex;
                return true;
            }
            else
            {
                index = freeIndices.First();
                freeIndices.RemoveAt(0);
                return false;
            }

        }

        public void RemoveIndex(long index)
        {
            freeIndices.Add(index);
        }
    }
}
