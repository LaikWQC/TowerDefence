using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    public virtual bool CanBuy => true;
    public virtual bool CanWalk => true;
    public virtual bool CanShoot => true;

    public virtual void Select(Color color) { }
    public virtual void UnSelect() { }
}
