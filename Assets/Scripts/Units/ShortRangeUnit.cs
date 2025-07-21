using UnityEngine;

namespace ZShopping.Units
{
    public class ShortRangeUnit : UnitBase
    {
        protected override void Awake()
        {
            base.Awake();
            moveRange = 6;
            attackRange = 2;
            health = 6;
        }
    }
} 
