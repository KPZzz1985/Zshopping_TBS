using UnityEngine;

namespace ZShopping.Units
{
    public class LongRangeUnit : UnitBase
    {
        protected override void Awake()
        {
            base.Awake();
            moveRange = 4;
            attackRange = 10;
            health = 3;
        }
    }
} 
