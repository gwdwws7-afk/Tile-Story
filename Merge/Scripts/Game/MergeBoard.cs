using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// ∫œ≥…∆Â≈Ã
    /// </summary>
    public class MergeBoard : MonoBehaviour
    {
        public Transform m_PropsRoot;
        public List<Square> m_Squares = new List<Square>();

        public virtual Square GetSquare(int index)
        {
            return m_Squares[index];
        }

        public virtual void Clear()
        {
        }

        public virtual Square GetClosestSquare(Vector3 worldPos)
        {
            Square result = null;
            float minDis = 100000000;
            for (int i = 0; i < m_Squares.Count; i++)
            {
                float dis = Mathf.Pow(m_Squares[i].transform.position.x - worldPos.x, 2) + Mathf.Pow(m_Squares[i].transform.position.y - worldPos.y, 2);
                if (dis < minDis)
                {
                    minDis = dis;
                    result = m_Squares[i];
                }
            }

            return result;
        }
    }
}
