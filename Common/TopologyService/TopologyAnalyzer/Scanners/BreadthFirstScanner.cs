using System.Collections.Generic;
using System.Linq;

namespace Common.TopologyService.TopologyAnalyzer.Scanners
{
	public class BreadthFirstScanner<T>
    {
        private List<T> nodes;

        public BreadthFirstScanner()
        {
            nodes = new List<T>();
        }

        public void Init()
        {
            nodes.Clear();
        }

        public void Add(T index)
        {
            nodes.Add(index);
        }

        public void AddRange(IEnumerable<T> indices)
        {
            nodes.AddRange(indices);
        }

        public bool HasNext()
        {
            return nodes.Count != 0;
        }

        public T GetNext()
        {
            var node = nodes.First();         // uzima se novi cvor za procesiranje
            nodes.RemoveAt(0);                // i brise iz liste neobradjenih cvorova
            return node;
        }

    }
}
