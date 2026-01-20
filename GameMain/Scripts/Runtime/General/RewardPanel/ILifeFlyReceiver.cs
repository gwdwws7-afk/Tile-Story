using UnityEngine;

public interface ILifeFlyReceiver
{
    Vector3 LifeFlyTargetPos { get; }

    void Show();

    void OnLifeFlyHit();

    void OnLifeFlyEnd();
}
