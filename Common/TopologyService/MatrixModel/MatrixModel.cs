using Common.TopologyService.ExtendedClassess;
using System;
using System.Collections.Generic;

namespace Common.TopologyService.MatrixModel
{
	//TEHNIKA RETKIH MATRICA
	[Serializable]
    public class CSparseMatrix
    {
        // Osnovni vektori
        private long[] sum;                 // Vektor sumiranog broja nenultih vandijagonalnih elemenata matrice po vrstama 
        private long[] firstInRow;          // Vektor kojim se ukazuje na redni broj prvog nenultog vandijagonalnog elementa u svakoj vrsti u vektoru EROW
        private long[] position;            // Vektor indeksa kolona(pozicija u matrici) nenultih vandijagonalnih elemenata iz vektora EROW
        private long[] nextInRow;			// Vektor ulancavanja kojim se ukazuju na redni broj sledeceg u nizu nenultog elementa iz vektora EROW (INXR)
        private long[] values;        // Vektor vrednosti nenultih vandijagonalnih matrice koji su poredjani po proizvoljnom redosledu (EROW)

        // Dodatni vektori
        private Dictionary<string, long> valInMatrix;   // Vektor omogucava da se pomocu reda i kolone pristupi elementu matrice

        // Ostalo
        private IndexManager verticesIndexManager;      // Vodi racuna o indeksiranju elemenata u matrici

        /// <summary>
        /// Konstruktor tehnike retkih matrica
        /// </summary>
        /// <param name="internalModel">Interni model podataka</param>
        public CSparseMatrix()
        {
        }


        #region Adding and removing rows


        public void AddSpaceForRows(int capacity)
        {
            sum = new long[capacity];
            firstInRow = new long[capacity];
        }

        public void AddSpaceForElems(int capacity)
        {
            this.verticesIndexManager = new IndexManager(1, capacity);
            position = new long[capacity];
            nextInRow = new long[capacity];
            values = new long[capacity];
            valInMatrix = new Dictionary<string, long>(capacity);
        }
        #endregion;

        #region Adding and removing elements
        public void AddElement(long rowIndex, long columnIndex, long matrixValue)
        {
            long iterIndex, prevIndex = 0, numOfNeigh;
            long elemIndex;

            numOfNeigh = sum[(int)rowIndex]++;

            verticesIndexManager.FindFreeIndex(out elemIndex);
            //if (verticesIndexManager.FindFreeIndex(out elemIndex))
            //{
            //    Position.Add(0);
            //    NextInRow.Add(0);
            //    Values.Add(default(ObjectType));
            //}

            //IFIRR & RED
            if (numOfNeigh == 0)                        //u koliko prvi cvor grane do tog trenutka nema prijavljenih suseda
            {
                firstInRow[(int)rowIndex] = elemIndex;
                nextInRow[(int)elemIndex] = 0;
            }
            else//ako vec postoji prijavljeni sused za dati cvor
            {
                long iterCount = 0;
                iterIndex = firstInRow[(int)rowIndex];

                for (iterCount = 0; iterCount < numOfNeigh; iterCount++)
                {
                    if (columnIndex < position[(int)iterIndex])
                    {
                        break;
                    }
                    prevIndex = iterIndex;
                    iterIndex = nextInRow[(int)iterIndex];
                }

                if (iterCount == 0)                          //element is first in row
                {
                    firstInRow[(int)rowIndex] = elemIndex;
                    nextInRow[(int)elemIndex] = iterIndex;
                }
                else if (iterCount == numOfNeigh)           //element is last in row
                {
                    nextInRow[(int)prevIndex] = elemIndex;
                    nextInRow[(int)elemIndex] = 0;
                }
                else
                {
                    nextInRow[(int)elemIndex] = nextInRow[(int)prevIndex];
                    nextInRow[(int)prevIndex] = elemIndex;
                }

            }
            position[(int)elemIndex] = columnIndex;
            values[(int)elemIndex] = matrixValue;
            valInMatrix[rowIndex + ":" + columnIndex] = elemIndex;

            return;
        }

        public bool RemoveElement(long row, long column)
        {
            long previous = 0;
            long ind, noOfElemsInRow;

            noOfElemsInRow = sum[(int)row];

            ind = firstInRow[(int)row];

            int iterFound = 0;

            for (int i = 1; i <= noOfElemsInRow; i++)
            {
                if (position[(int)ind] == column)
                {
                    iterFound = i;
                    break;
                }
                previous = ind;
                ind = nextInRow[(int)ind];
            }

            if (iterFound == 0)
            {
                return false;
            }
            else if (iterFound == 1)
            {
                firstInRow[(int)row] = nextInRow[(int)ind];
                if (sum[(int)row] > 0)
                {
                    sum[(int)row]--;
                }
            }
            else
            {
                sum[(int)row]--;
                nextInRow[(int)previous] = nextInRow[(int)ind];
            }

            valInMatrix.Remove(row + ":" + column);
            values[(int)ind] = default(long);
            nextInRow[(int)ind] = default(long);
            position[(int)ind] = default(long);

            verticesIndexManager.RemoveIndex(ind);

            return true;

        }

        #endregion;

        #region Some usefull methods


        public long GetElemByRowAndColumn(long row, long column)
        {
            return values[(int)valInMatrix[row + ":" + column]];
        }

        public long GetValue(long index)
        {
            return values[(int)index];
        }

        public bool Contains(long row, long column)
        {
            return valInMatrix.ContainsKey(row + ":" + column);
        }

        public long GetIndexOfElem(long row, long column)
        {
            return valInMatrix[row + ":" + column];
        }
        #endregion

        #region Getters and Setters

        public long GetFirstInRow(long index)
        {
            return firstInRow[(int)index];
        }

        public long GetNextInRow(long index)
        {
            return nextInRow[(int)index];
        }


        public long GetColumnIndex(long index)
        {
            return position[(int)index];
        }

        public long GetSumByRow(long row)
        {
            return sum[(int)row];

        }

        #endregion

    }
}
