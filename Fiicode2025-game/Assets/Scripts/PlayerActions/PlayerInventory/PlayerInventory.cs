using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;
    public Inventory inventory;

    private const string SaveKey = "PlayerInventoryData"; // Key for saving data

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        LoadInventory(); // Load saved inventory at start
    }

    public void AddItem(string itemName, int amount)
    {
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory: Inventory ScriptableObject is not assigned!");
            return;
        }

        if (ModifyVariable(inventory, itemName, amount))
        {
            Debug.Log($"Added {amount} {itemName} to inventory.");
            SaveInventory(); // Save after adding
        }
        else
        {
            Debug.LogWarning($"Item '{itemName}' not found in inventory.");
        }
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory: Inventory ScriptableObject is not assigned!");
            return;
        }

        if (CanRemoveItem(inventory, itemName, amount))
        {
            if (ModifyVariable(inventory, itemName, -amount))
            {
                Debug.Log($"Removed {amount} {itemName} from inventory.");
                SaveInventory(); // Save after removing
            }
        }
        else
        {
            NotEnoughItems(itemName, amount);
        }
    }

    private bool ModifyVariable(ScriptableObject scriptableObject, string variableName, int amount)
    {
        System.Type type = scriptableObject.GetType();
        FieldInfo field = type.GetField(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null && field.FieldType == typeof(int))
        {
            int currentValue = (int)field.GetValue(scriptableObject);
            int newValue = Mathf.Max(0, currentValue + amount); // Prevent negative values
            field.SetValue(scriptableObject, newValue);
            return true;
        }

        return false;
    }

    private bool CanRemoveItem(ScriptableObject scriptableObject, string variableName, int amount)
    {
        System.Type type = scriptableObject.GetType();
        FieldInfo field = type.GetField(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null && field.FieldType == typeof(int))
        {
            int currentValue = (int)field.GetValue(scriptableObject);
            return currentValue >= amount;
        }

        return false;
    }

    private void NotEnoughItems(string itemName, int amount)
    {
        Debug.LogWarning($"Not enough {itemName} in inventory to remove {amount}.");
    }

    /// <summary>
    /// Saves the inventory data to PlayerPrefs in JSON format.
    /// </summary>
    public void SaveInventory()
    {
        string json = JsonUtility.ToJson(inventory);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("Inventory saved: " + json);
    }

    /// <summary>
    /// Loads the inventory data from PlayerPrefs.
    /// </summary>
    public void LoadInventory()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            JsonUtility.FromJsonOverwrite(json, inventory);
            Debug.Log("Inventory loaded: " + json);
        }
        else
        {
            Debug.Log("No saved inventory found, starting fresh.");
            ResetInventory();
        }
    }

    /// <summary>
    /// Resets the inventory to its default values.
    /// </summary>
    public void ResetInventory()
    {
        inventory.wood = 0;
        inventory.stone = 0;
        inventory.gold = 0;
        inventory.silver = 0;
        SaveInventory(); // Save the reset values to avoid re-resetting
    }
}
