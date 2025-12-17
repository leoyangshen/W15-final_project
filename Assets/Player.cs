/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
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

using UnityEngine;

public class Player : MonoBehaviour
{
    // Stats
    public int CurrentPosition { get; private set; } = 1; // Starts on space 1 [cite: 13]
    public int ExperiencePoints { get; private set; } = 0; // Starts at 0 
    public Weapon CurrentWeapon { get; private set; } = new Weapon("Knife", 2); // Starts with Knife (+2) 
    public GameManager gameManager;

    // Required Display Function (You'll call this to update the UI)
    /* public void DisplayStats()
    {
        Debug.Log($"Player Stats: XP: {ExperiencePoints}, Weapon: {CurrentWeapon.Name} (+{CurrentWeapon.AttackModifier})"); // Requirement: Display XP and Weapon [cite: 62]
    } */
    // Inside Player.cs
    // Update this function:
    public void DisplayStats()
    {
        // Format the stats string
        string stats = $"Current Space: {CurrentPosition}\n" +
                       $"XP: {ExperiencePoints} | Weapon: {CurrentWeapon.Name} (+{CurrentWeapon.AttackModifier})";

        // Call the GameManager to update the UI
        gameManager.UpdateStatsUI(stats);
    }

    // Core Logic: Equipping a new weapon
    public void CheckAndEquipWeapon(Weapon newWeapon)
    {
        if (newWeapon.AttackModifier > CurrentWeapon.AttackModifier)
        {
            CurrentWeapon = newWeapon;
            Debug.Log($"You found a {newWeapon.Name}! It is stronger and is now equipped.");
        }
        else
        {
            Debug.Log($"You found a {newWeapon.Name}, but your {CurrentWeapon.Name} is stronger or equal. You keep your current weapon.");
        }
        DisplayStats();
    }
    /* private int RollDie()
    {
        // Add this helper function inside your Player class or a Utility class
        // Generates a random integer between 1 (inclusive) and 7 (exclusive), resulting in 1 through 6.
        return Random.Range(1, 7);
    } */
    // Inside Player.cs
    private int RollDie()
    {
        // FIX: Specify UnityEngine.Random to resolve ambiguity
        return UnityEngine.Random.Range(1, 7);
    }

    // Add this function inside your Player class
    // Inside Player.cs
    public void MovePlayer()
    {
        // Block movement if the player is already at the Dragon's lair
        if (CurrentPosition == 28)
        {
            Debug.Log("You are at the Dragon's dungeon. Choose 'Dismount and explore' to face the Dragon.");
            return;
        }

        int dieRoll = RollDie();
        Debug.Log($"You chose to travel. You rolled a **{dieRoll}**."); // Requirement: Display the result of the die roll [cite: 63]

        // Calculate the new position
        int newPosition = CurrentPosition + dieRoll;
        CurrentPosition = newPosition;

        // Check for the End Game trigger (Space 28 or past)
        if (CurrentPosition >= 28)
        {
            CurrentPosition = 28; // Cap position at 28
            Debug.Log("You have arrived at the Dragon's dungeon (Space 28)!");

            // The game manager handles the fight initiation
            gameManager.DragonEncounter();
        }
        else
        {
            Debug.Log($"You are now on space **{CurrentPosition}**.");
            DisplayStats(); // Always display stats after a move
        }
        gameManager.DrawMap();
    }
    // Add this method to your Player class
    public void IncreaseXP(int amount)
    {
        // Increases the player's XP by the specified amount
        ExperiencePoints += amount;

        // Display the updated stats (a core requirement)
        DisplayStats();
    }
}




