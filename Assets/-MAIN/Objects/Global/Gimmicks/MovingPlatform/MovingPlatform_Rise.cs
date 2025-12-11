using UnityEngine;

public class MovingPlatform_Rise : BaseMovingPlatform
{
    protected override bool ShowMoveAtStart => false;
    protected override bool ShowLoop => false;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerExit(other);
        MoveToStart();
    }

    protected override void OnTriggerExit(Collider other)
    {

        base.OnTriggerEnter(other);
        MoveToEnd();
    }
}

