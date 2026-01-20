using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeBoard_DigTreasure : MergeBoard
    {
        public Transform m_Body, m_ShakeBody, m_SquareBgRoot;
        public GameObject m_SquareBgPrefab;

        private int m_SquareWidth = 143;
        private int m_SquareHeight = 143;
        private int m_MovedLayer;

        public int SquareWidth => m_SquareWidth;

        public int SquareHeight => m_SquareHeight;

        public int MovedLayer => m_MovedLayer;

        public override Square GetSquare(int index)
        {
            return m_Squares[(index + m_MovedLayer * MergeManager.Instance.BoardCol) % m_Squares.Count];
        }

        public override void Clear()
        {
            m_MovedLayer = 0;
        }

        public void MoveMergeBoard(int moveLayer)
        {
            if (moveLayer <= 0)
                return;

            int totalCount = m_Squares.Count;
            int boardRow = MergeManager.Instance.BoardRow;
            int boardCol = MergeManager.Instance.BoardCol;
            int lastMovedLayer = m_MovedLayer;
            m_MovedLayer += moveLayer;
            MergeManager.PlayerData.SetDigTreasureCurDepth(MergeManager.PlayerData.GetDigTreasureCurDepth() + moveLayer);

            for (int i = 0; i < moveLayer; i++)
            {
                for (int j = 0; j < boardCol; j++)
                {
                    int index = ((lastMovedLayer + i) * boardCol + j) % totalCount;
                    m_Squares[index].transform.localPosition = new Vector3(m_Squares[index].transform.localPosition.x, m_Squares[index].transform.localPosition.y - boardRow * m_SquareHeight);
                }
            }

            Vector3 prefabLocalPos = m_SquareBgPrefab.transform.localPosition;
            for (int i = 0; i < moveLayer; i++)
            {
                for (int j = 0; j < boardCol; j++)
                {
                    var bg = Instantiate(m_SquareBgPrefab, m_SquareBgRoot);
                    bg.transform.localPosition = new Vector3(prefabLocalPos.x + j * m_SquareWidth, prefabLocalPos.y - (lastMovedLayer + 1 + i) * m_SquareHeight);
                    bg.SetActive(true);
                }
            }

            GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Pane_Move_Up.ToString());
        }
    }
}
