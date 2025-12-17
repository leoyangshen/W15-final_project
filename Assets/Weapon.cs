/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/

// Weapon.cs (Does NOT use MonoBehaviour)
public struct Weapon
{
    public string Name;
    public int AttackModifier;

    public Weapon(string name, int modifier)
    {
        Name = name;
        AttackModifier = modifier;
    }
}