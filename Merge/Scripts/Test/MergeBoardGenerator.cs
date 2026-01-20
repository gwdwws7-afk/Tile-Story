using UnityEngine;

namespace Merge
{
    [ExecuteInEditMode]
    public class MergeBoardGenerator : MonoBehaviour
    {
        public GameObject oddSquare;
        public GameObject evenSquare;

        public Transform root;
        public MergeBoard mergeBoard;

        public float startX = -358;
        public float startY = 342;
        public float cellWidth = 180;
        public int space = 0;
        public int row = 5;
        public int col = 5;

        public bool startGenerate;

        private void Update()
        {
            if (startGenerate)
            {
                startGenerate = false;
                GenerateChestBoard();
            }
        }

        public void GenerateChestBoard()
        {
            if (mergeBoard != null)
                mergeBoard.m_Squares.Clear();

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    int index = i * col + j;
                    GameObject newGrid;
                    if (index % 2 == 0)
                    {
                        newGrid = Instantiate(evenSquare, root);
                    }
                    else
                    {
                        newGrid = Instantiate(oddSquare, root);
                    }
                    newGrid.name = "Square_" + index.ToString();
                    newGrid.transform.localPosition = new Vector3(startX + space * (j + 1) + j * cellWidth, startY - space * (i + 1) - i * cellWidth);
                    newGrid.GetComponent<Square>().m_Row = i;
                    newGrid.GetComponent<Square>().m_Col = j;

                    if (mergeBoard != null)
                        mergeBoard.m_Squares.Add(newGrid.GetComponent<Square>());
                }
            }
        }
    }
}