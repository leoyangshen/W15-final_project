/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
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

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // You must add this line for TextMeshPro
using UnityEngine.UI; // <--- ADD THIS LINE

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text statsText;
    public TMP_Text mapText;
    public TMP_Text logText;
    // Add this line with your other variable declarations at the top of the GameManager class
    public ScrollRect logScrollRect; // <--- ADD THIS VARIABLE

    // Enum to easily define the possible content of a space
    public enum SpaceType { Monster, Weapon, Empty, Dragon, Start }

    // Dictionary to hold the content of each space (Space Number -> SpaceType)
    private Dictionary<int, SpaceType> board = new Dictionary<int, SpaceType>();

    // Reference to the Player script
    public Player player;

    // List of all 5 discoverable weapons
    private List<Weapon> availableWeapons = new List<Weapon>
    {
        new Weapon("Crossbow", 3), // +3 on attack [cite: 36]
        new Weapon("Flail", 4),      // +4 on attack [cite: 37]
        new Weapon("Broad Sword", 5), // +5 on attack [cite: 38]
        new Weapon("Dragon Slayer", 6), // +6 on attack [cite: 39]
        new Weapon("Spell of The Gods", 7) // +7 on attack [cite: 40]
    };

    // List of weapons not yet found
    private List<Weapon> weaponsToPlace;

    // Dictionary to store specific monster HP (Space Number -> HP)
    private Dictionary<int, int> monsterHPs = new Dictionary<int, int>();
    // Add this line with your other variable declarations at the top of the GameManager class
    private Dictionary<int, Weapon> weaponPlacements = new Dictionary<int, Weapon>();
    void Awake()
    {
        InitializeBoard();
    }
    public void LogMessage(string message)
    {
        // Append the new message to the existing log and show it on screen
        logText.text += "\n" + message;

        // --- NEW CODE ADDED FOR SCROLLING ---
        // Make sure the scroll view automatically jumps to the bottom (newest message)
        if (logScrollRect != null)
        {
            // Set the vertical normalized position to 0 (bottom of the scroll view)
            // This must be done after a small delay (yield return null) or at the end of the frame
            // to give the Content Size Fitter time to calculate the new height.
            // For simplicity in a single-frame operation, this often works:
            logScrollRect.verticalNormalizedPosition = 0f;
        }
        // -------------------------------------

        // Optional: Keep the log from getting too long
        if (logText.text.Length > 2000) logText.text = logText.text.Substring(logText.text.Length - 1000); 
    }
    // Inside GameManager.cs
    private void InitializeBoard()
    {
        // 1. Set fixed spots
        board[1] = SpaceType.Start;
        board[28] = SpaceType.Dragon;

        // 2. Identify all available spaces for random placement (2 through 27)
        List<int> availableSpaces = new List<int>();
        for (int i = 2; i <= 27; i++)
        {
            availableSpaces.Add(i);
        }

        // 3. Helper function to choose a unique random space and remove it from the available pool
        Func<int> GetRandomSpace = () =>
        {
            int index = UnityEngine.Random.Range(0, availableSpaces.Count);
            int space = availableSpaces[index];
            availableSpaces.RemoveAt(index);
            return space;
        };

        // --- Allocate 5 Weapons ---
        // Make a copy of availableWeapons to place so the original list remains intact
        weaponsToPlace = new List<Weapon>(availableWeapons);
        for (int i = 0; i < 5; i++)
        {
            int space = GetRandomSpace();
            board[space] = SpaceType.Weapon;

            // Map the space to a specific weapon from the list
            weaponPlacements[space] = weaponsToPlace[i];
        }

        // --- Allocate 14 Monsters ---
        for (int i = 0; i < 14; i++)
        {
            int space = GetRandomSpace();
            board[space] = SpaceType.Monster;

            // Assign a random HP between 3 (inclusive) and 7 (inclusive) [cite: 29]
            monsterHPs[space] = UnityEngine.Random.Range(3, 8);
        }

        // --- Allocate the remaining 7 Empty Spaces ---
        // The remaining 7 spaces in availableSpaces are all Empty
        foreach (int space in availableSpaces)
        {
            board[space] = SpaceType.Empty;
        }

        Debug.Log("Board initialized with random encounters!");
        LogMessage("Board initialized with random encounters!");
        player.DisplayStats(); // Display initial stats
        DrawMap();
    }

    // Add this helper function inside your GameManager class
    /* public void LogMessage(string message)
    {
        // Append the new message to the existing log and show it on screen
        logText.text += "\n" + message;

        // Optional: Keep the log from getting too long
        // if (logText.text.Length > 2000) logText.text = logText.text.Substring(logText.text.Length - 1000); 
    }
    */
    // Add this function inside your GameManager class
    public void DragonEncounter()
    {
        // 1. Requirement: Check for the XP prerequisite
        if (player.ExperiencePoints < 8)
        {
            // FAIL condition (lack of XP)
            Debug.Log("Alas, the dragon’s eyes stare at you and places you under his spell. You try to move but fail to do so and find yourself torched by the dragon’s fire. If only you had more experience, you could have seen it coming.");
            LogMessage("Alas, the dragon’s eyes stare at you and places you under his spell. You try to move but fail to do so and find yourself torched by the dragon’s fire. If only you had more experience, you could have seen it coming.");
            // End Game
            GameOver(false);
            return;
        }

        // Player meets the XP requirement
        Debug.Log("Due to your cunning and experience, you are worthy to face the deadly dragon!");
        LogMessage("Due to your cunning and experience, you are worthy to face the deadly dragon!");
        // 2. Perform Combat Check
        const int DRAGON_HP = 10;
        int dieRoll = RollDie(); // Reuses your existing RollDie helper
        int weaponModifier = player.CurrentWeapon.AttackModifier;

        // Calculate Total Attack
        int totalAttack = dieRoll + weaponModifier;

        // Requirements: Display the die roll and total attack
        Debug.Log($"The Dragon has **{DRAGON_HP} HP**.");
        LogMessage($"The Dragon has **{DRAGON_HP} HP**.");
        Debug.Log($"You roll a **{dieRoll}**. Your {player.CurrentWeapon.Name} (+{weaponModifier}) gives a total attack of **{totalAttack}**.");
        LogMessage($"You roll a **{dieRoll}**. Your {player.CurrentWeapon.Name} (+{weaponModifier}) gives a total attack of **{totalAttack}**.");
        // 3. Determine Outcome
        if (totalAttack >= DRAGON_HP)
        {
            // WIN condition
            Debug.Log("Due to your cunning and experience, you have defeated the deadly dragon. Your quest has ended good sir. You’ve obtained the Chalice of knowledge and all of earth’s mysteries are revealed.");
            LogMessage("Due to your cunning and experience, you have defeated the deadly dragon. Your quest has ended good sir. You’ve obtained the Chalice of knowledge and all of earth’s mysteries are revealed.");
            // End Game
            GameOver(true);
        }
        else
        {
            // FAIL condition (insufficient damage)
            Debug.Log("Your attack fails to defeat the Dragon! It roars in defiance and torches you with its fiery breath.");


            LogMessage("Your attack fails to defeat the Dragon! It roars in defiance and torches you with its fiery breath.");
            // End Game
            GameOver(false);
        }
    }

    // Add this function inside your GameManager class
    private void FindWeapon(int spaceNumber)
    {
        // 1. Retrieve the specific weapon from the placement dictionary
        if (weaponPlacements.TryGetValue(spaceNumber, out Weapon foundWeapon))
        {
            Debug.Log($"You dismount and explore the area, finding a powerful new weapon: the **{foundWeapon.Name} (+{foundWeapon.AttackModifier})**!");
            LogMessage($"You dismount and explore the area, finding a powerful new weapon: the **{foundWeapon.Name} (+{foundWeapon.AttackModifier})**!");
            // 2. Use the Player script method to check and potentially equip the new weapon
            player.CheckAndEquipWeapon(foundWeapon);
        }
        else
        {
            Debug.Log("Error: The space was marked as a weapon space but no weapon data was found.");
            LogMessage("Error: The space was marked as a weapon space but no weapon data was found.");
        }

        // OPTIONAL: Prevent finding the same weapon multiple times
        // Since the weapon is found, you might want to remove it from the board
        // to prevent the player from finding it again, and set the space to Empty.
        board[spaceNumber] = SpaceType.Empty;
    }
    // This is where we link the Player's decision to the board data
    public void DismountAndExplore()
    {
        if (player.CurrentPosition == 28)
        {
            // Call a function to handle the Dragon fight (not implemented yet)
            Debug.Log("You must face the Dragon!");
            LogMessage("You must face the Dragon!");
            return;
        }

        // Safety check: ensure the space has content
        if (!board.ContainsKey(player.CurrentPosition))
        {
            Debug.Log("Error: Current space has no defined encounter.");
            LogMessage("Error: Current space has no defined encounter.");
            return;
        }

        // Get the type of encounter at the player's current position
        SpaceType encounter = board[player.CurrentPosition];

        switch (encounter)
        {
            case SpaceType.Monster:
                // Start a battle! We need a Combat function.
                Debug.Log("You encounter a Monster!");
                LogMessage("You encounter a Monster!");
                StartCombat(player.CurrentPosition);
                break;

            case SpaceType.Weapon:
                // Find a Weapon! We need logic to assign which weapon was found.
                Debug.Log("You find a new weapon!");
                LogMessage("You find a new weapon!");
                FindWeapon(player.CurrentPosition);
                break;

            case SpaceType.Empty:
                // Empty area! Grant 1 XP.
                AreaIsEmpty();
                break;

            default:
                // Start or Dragon spaces shouldn't be explorable this way, but included for completeness.
                Debug.Log("This area holds no secrets.");
                LogMessage("This area holds no secrets.");
                break;
        }

        // After exploration, the space should often become 'Empty' or 'Explored'
        // For simplicity, we can set it to Empty so you don't fight the same monster twice.
        if (encounter == SpaceType.Monster)
        {
            board[player.CurrentPosition] = SpaceType.Empty;
        }
    }

    // Placeholder function for Empty Area logic [cite: 45]
    private void AreaIsEmpty()
    {
        // Display the required message [cite: 46, 47]
        Debug.Log("There is nothing for you to do, so you reflect upon your adventures thus far. You take the time to train and enhance your reflexes");
        LogMessage("There is nothing for you to do, so you reflect upon your adventures thus far. You take the time to train and enhance your reflexes");

        // Increase XP by 1 [cite: 48]
        player.IncreaseXP(1); // Assuming we add this method to the Player class
    }

    // The FindWeapon and StartCombat functions will be implemented next!
    // ...
    // Assuming you have access to the Player's RollDie() method or redefine it here.
    /* private int RollDie()
    {
        // Generates a random integer between 1 (inclusive) and 7 (exclusive), resulting in 1 through 6.
        return Random.Range(1, 7);
    } */
    // Inside GameManager.cs
    private int RollDie()
    {
        // FIX: Specify UnityEngine.Random to resolve ambiguity
        return UnityEngine.Random.Range(1, 7);
    }

    // Function to handle a monster encounter
    private void StartCombat(int spaceNumber)
    {
        // 1. Get Monster HP
        int monsterHP = monsterHPs[spaceNumber]; // Assuming monsterHPs has been populated

        // Requirement: Display the HP of the monster
        Debug.Log($"You encounter a fearsome creature! It has **{monsterHP} HP**.");
        LogMessage($"You encounter a fearsome creature! It has **{monsterHP} HP**.");

        // 2. Player Attack Roll
        int dieRoll = RollDie();
        int weaponModifier = player.CurrentWeapon.AttackModifier;

        // Calculate Total Attack
        int totalAttack = dieRoll + weaponModifier;

        // Requirement: Display the die roll and total attack
        Debug.Log($"You roll a **{dieRoll}**. Your {player.CurrentWeapon.Name} (+{weaponModifier}) gives a total attack of **{totalAttack}**.");
        LogMessage($"You roll a **{dieRoll}**. Your {player.CurrentWeapon.Name} (+{weaponModifier}) gives a total attack of **{totalAttack}**.");
        // 3. Determine Outcome (Single-turn battle)
        if (totalAttack >= monsterHP)
        {
            // WIN condition
            Debug.Log("The attack destroys the monster!");
            LogMessage("The attack destroys the monster!");

            // Requirement: Gain 2 XP for defeating a monster
            player.IncreaseXP(2);
        }
        else
        {
            // LOSS condition (Instant Death)
            Debug.Log("Your attack fails to defeat the creature. The monster destroys you.");
            LogMessage("Your attack fails to defeat the creature. The monster destroys you.");

            // **End Game: The game should stop here.**
            GameOver(false); // Assuming a function that handles game over state
        }
    }

    // Placeholder for the Game Over function
    private void GameOver(bool isWin)
    {
        if (isWin)
        {
            Debug.Log("🎉 YOU WIN! Quest completed.");
            Debug.Log("🎉 YOU WIN! Quest completed.");
            Debug.Log("🎉 YOU WIN! Quest completed.");
            LogMessage("🎉 YOU WIN! Quest completed.");
        }
        else
        {
            Debug.Log("💀 GAME OVER! Yarra has fallen.");
            LogMessage("💀 GAME OVER! Yarra has fallen.");
        }
        // In a real Unity game, you would disable controls or load a new scene here.
        Time.timeScale = 0; // Pauses the game loop for a dramatic stop
    }
    // Inside GameManager.cs
    public void UpdateStatsUI(string stats)
    {
        statsText.text = stats;
    }
    // Add this function inside your GameManager class
    public void DrawMap()
    {
        char[] map = new char[28]; // Board spaces 1 through 28

        // Initialize the entire map with the '*' character
        for (int i = 0; i < 28; i++)
        {
            map[i] = '*';
        }

        // Set the Dragon space 'D' at the end (index 27)
        map[27] = 'D';

        // Set the Player position 'P' (CurrentPosition is 1-based, array is 0-based)
        int playerIndex = player.CurrentPosition - 1;
        map[playerIndex] = 'P';

        // Convert the array of characters into a single string with spaces for display
        string mapString = string.Join(" ", map);

        // Update the UI
        mapText.text = "Map:\n" + mapString; // Requirement: Display the map on screen [cite: 9, 10]
    }
}