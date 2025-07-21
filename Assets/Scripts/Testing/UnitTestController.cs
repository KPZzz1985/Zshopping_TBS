using UnityEngine;
using ZShopping.Units;

public class UnitTestController : MonoBehaviour
{
    [Tooltip("Unit that will perform actions")]
    public UnitBase unit1;
    [Tooltip("Target unit for attack tests")]
    public UnitBase unit2;
    [Tooltip("Destination for movement tests")]
    public Vector3 destination;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.M) && unit1 != null)
        {
            unit1.MoveTo(destination);
            Debug.Log($"[Test] Moving {unit1.name} to {destination}");
        }

        if (Input.GetKeyDown(KeyCode.N) && unit1 != null && unit2 != null)
        {
            unit1.Attack(unit2);
            Debug.Log($"[Test] {unit1.name} attacked {unit2.name}. Health now {unit2.health}");
        }
    }
} 
