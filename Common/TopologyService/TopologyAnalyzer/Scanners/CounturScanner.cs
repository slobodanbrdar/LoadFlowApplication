using Common.TopologyService.TopologyAnalyzer.AuxiliaryStructures;
using System.Collections.Generic;
using System.Linq;

namespace Common.TopologyService.TopologyAnalyzer.Scanners
{
	public class CContourScanner
    {
        private enum EContourSide : int { LEFT = 0, RIGHT = 1, UNKNOWN = 2 };
        private List<CVisitedBranch>[] branchInContour;
        private EContourSide sideOfContour;
        private int index;

        public CContourScanner()
        {
            this.branchInContour = new List<CVisitedBranch>[2];
        }

        public void Begin()
        {
            branchInContour[0] = new List<CVisitedBranch>();
            branchInContour[1] = new List<CVisitedBranch>();
            sideOfContour = EContourSide.LEFT;
            index = 0;
        }

        public void MarkContour(byte phaseIndex, CVisitedBranch leftBranch, CVisitedBranch rightBranch, Dictionary<long, CVisitedBranch> visitedBranches, Dictionary<long, CVisitedNode> visitedNodes)
        {
            //LEVI KRAJ PETLJE
            CVisitedBranch tempBranch = leftBranch;
            branchInContour[0].Add(tempBranch);

            CVisitedNode tempNode = visitedNodes[tempBranch.UpNode[phaseIndex]];
            while (!(tempNode.Parents[phaseIndex].Count == 0))
            {
                tempBranch = visitedBranches[tempNode.Parents[phaseIndex].First()];
                branchInContour[0].Add(tempBranch);
                tempNode = visitedNodes[tempBranch.UpNode[phaseIndex]];
            }

            //DESNI KRAJ PETLJE
            tempBranch = rightBranch;
            branchInContour[1].Add(tempBranch);

            tempNode = visitedNodes[tempBranch.UpNode[phaseIndex]];
            while (!(tempNode.Parents[phaseIndex].Count == 0))
            {
                tempBranch = visitedBranches[tempNode.Parents[phaseIndex].First()];
                tempNode = visitedNodes[tempBranch.UpNode[phaseIndex]];

                if (!branchInContour[0].Contains(tempBranch))
                {
                    branchInContour[1].Add(tempBranch);
                }
                else
                {
                    int firstIndex = branchInContour[0].LastIndexOf(tempBranch);
                    int count = branchInContour[0].Count - firstIndex;
                    branchInContour[0].RemoveRange(firstIndex, count);
                    break;
                }
            }
        }

        public bool Next(out CVisitedBranch branch, out CVisitedNode node, Dictionary<long, CVisitedNode> visitedNodes, byte phaseIndex)
        {

            switch (sideOfContour)
            {
                case (sbyte)EContourSide.LEFT:
                    {
                        branch = branchInContour[0][index];
                        node = visitedNodes[branch.UpNode[phaseIndex]];

                        if (index < branchInContour[0].Count - 1)
                        {
                            index++;
                        }
                        else
                        {
                            sideOfContour = EContourSide.RIGHT;
                            index = branchInContour[1].Count - 1;
                        }
                        return true;
                    }
                case EContourSide.RIGHT:
                    {
                        branch = branchInContour[1][index];
                        node = visitedNodes[branch.UpNode[phaseIndex]];

                        if (index > 0)
                        {
                            index--;
                        }
                        else
                        {
                            sideOfContour = EContourSide.UNKNOWN;
                        }
                        return true;
                    }

                default:
                    {
                        node = null;
                        branch = null;
                        return false;
                    }
            }
        }


    }
}
