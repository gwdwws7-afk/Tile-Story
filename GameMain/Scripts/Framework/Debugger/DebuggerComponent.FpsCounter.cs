

public sealed partial class DebuggerComponent : GameFrameworkComponent
{
    private sealed class FpsCounter
    {
        private float m_UpdateInterval;
        private float m_CurrentFps;
        private int m_Frames;
        private float m_Accumulator;
        private float m_LeftTime;

        public FpsCounter(float updateInterval)
        {
            if (updateInterval <= 0)
            {
                Log.Error("FpsCounter updateInterval is invalid");
            }
            this.m_UpdateInterval = updateInterval;
            OnReset();
        }

        public float UpdateInterval
        {
            get
            {
                return m_UpdateInterval;
            }
            set
            {
                if (value <= 0)
                {
                    Log.Error("FpsCounter updateInterval is invalid");
                }
                m_UpdateInterval = value;
                OnReset();
            }
        }

        public float CurrentFps
        {
            get
            {
                return m_CurrentFps;
            }
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            m_Frames++;
            m_Accumulator += realElapseSeconds;
            m_LeftTime -= realElapseSeconds;
            while (m_LeftTime <= 0)
            {
                m_CurrentFps = m_Accumulator > 0 ? m_Frames / m_Accumulator : 0;
                m_Frames = 0;
                m_Accumulator = 0;
                m_LeftTime += m_UpdateInterval;
            }
        }

        private void OnReset()
        {
            m_CurrentFps = 0;
            m_Frames = 0;
            m_Accumulator = 0;
            m_LeftTime = 0;
        }
    }
}
