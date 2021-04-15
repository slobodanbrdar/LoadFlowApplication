using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.TopologyService.ExtendedClassess
{
	[Serializable]
    public class CTypeInfo
    {
        #region Fields


        private long nextFree;
        private List<long> freeIndexes;

        public long BaseIndex { get; private set; }
        public long MaxIndex { get; private set; }

        #endregion

        #region Constructor

        public CTypeInfo(long baseIndex, long maxIndex, int capacity = 20)
        {
            this.BaseIndex = baseIndex;
            this.MaxIndex = maxIndex;

            this.nextFree = baseIndex + 1;

            this.freeIndexes = new List<long>(capacity);
        }

        #endregion



        public void AddFreeIndex(long index)
        {
            freeIndexes.Add(index);
        }


        public long GetFirstFreeIndex()
        {
            long index;

            if (freeIndexes.Count != 0)
            {
                index = freeIndexes.First();
                freeIndexes.RemoveAt(0);
            }
            else
            {
                index = nextFree++;
            }
            return index;
        }
    }
}
