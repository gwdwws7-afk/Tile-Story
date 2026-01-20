using UnityEngine;

public class InputComponent : GameFrameworkComponent
{
    public float clickDeltaAble = 50;
    private Vector2 m_FirstV;
    private bool m_IsPressing;
    private ISimpleClick m_Target;

    public void ActiveChooseRaycast()
    {
        ISimpleClick find = ScreenPositionUtil.RaycastFindClick<ISimpleClick>(Input.mousePosition);
        if (find != null)
        {
            m_FirstV = Input.mousePosition;
            m_Target = find;
        }
    }

    void Update()
    {
        CheckClickInput();
    }

    /// <summary>
    /// 检测鼠标按下
    /// </summary>
    void CheckClickInput()
    {
        if (Input.GetMouseButtonDown(0))//鼠标按下
        {
            ActiveChooseRaycast();
            m_IsPressing = true;
        }
        if (m_IsPressing)
        {
            Vector2 curMousePosition = Input.mousePosition;
            if (Vector2.Distance(curMousePosition, m_FirstV) > clickDeltaAble)
            {
                //离开某距离显示失败
                ClickEnd();
            }
            if (Input.GetMouseButtonUp(0))//鼠标抬起
            {
                if (m_Target != null)
                {
                    m_Target.OnClickComplete();
                }
                ClickEnd();
            }
        }
    }

    void ClickEnd()
    {
        m_Target = null;
        m_IsPressing = false;
    }
}

public interface ISimpleClick
{
    void OnClickComplete();
}