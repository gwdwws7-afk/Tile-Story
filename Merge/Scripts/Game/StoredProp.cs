using GameFramework;

namespace Merge
{
    public class StoredProp
    {
        private int m_PropId;
        private PropSavedData m_StoredData;

        public StoredProp(int id)
        {
            m_PropId = id;
            m_StoredData = null;
        }

        public StoredProp(int id, PropSavedData data)
        {
            m_PropId = id;
            m_StoredData = data;
        }

        public int PropId
        {
            get
            {
                return m_PropId;
            }
        }

        public PropSavedData SavedData
        {
            get
            {
                return m_StoredData;
            }
        }
    }
}
