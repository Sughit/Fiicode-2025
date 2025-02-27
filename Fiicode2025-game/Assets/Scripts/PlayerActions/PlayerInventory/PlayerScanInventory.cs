using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class PlayerScanInventory : MonoBehaviour
{
    public static PlayerScanInventory instance;
    public ScanInventory inventory;

    private const string SaveKey = "PlayerScanInventoryData"; // Key for saving data

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        LoadInventory(); // Load saved inventory at start
    }

    public void Unlock(string itemName)
    {
        if (inventory == null)
        {
            Debug.LogError("PlayerScanInventory: ScanInventory ScriptableObject is not assigned!");
            return;
        }

        if (ModifyVariable(inventory, itemName, true))
        {
            Debug.Log($"Unlocked {itemName}.");
            SaveInventory(); // Save after adding
        }
        else
        {
            Debug.LogWarning($"Discovery '{itemName}' not found in scanning inventory.");
        }
    }

    public void Lock(string itemName)
    {
        if (inventory == null)
        {
            Debug.LogError("PlayerScanInventory: ScanInventory ScriptableObject is not assigned!");
            return;
        }

        if (ModifyVariable(inventory, itemName, false))
        {
            Debug.Log($"Locked discovery {itemName}.");
            SaveInventory(); // Save after removing
        }
    }

    private bool ModifyVariable(ScriptableObject scriptableObject, string variableName, bool state)
    {
        System.Type type = scriptableObject.GetType();
        FieldInfo field = type.GetField(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null && field.FieldType == typeof(bool))
        {
            bool currentValue = (bool)field.GetValue(scriptableObject);
            bool newValue = state;
            field.SetValue(scriptableObject, newValue);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Saves the inventory data to PlayerPrefs in JSON format.
    /// </summary>
    public void SaveInventory()
    {
        string json = JsonUtility.ToJson(inventory);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("ScanInventory saved: " + json);
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
            Debug.Log("ScanInventory loaded: " + json);
        }
        else
        {
            Debug.Log("No saved scan inventory found, starting fresh.");
            ResetInventory();
        }
    }

    /// <summary>
    /// Resets the inventory to its default values.
    /// </summary>
    public void ResetInventory()
    {
        inventory.ironIngot = false;
        inventory.goldIngot = false;
        inventory.plank = false;
        inventory.paper = false;
        SaveInventory(); // Save the reset values to avoid re-resetting
    }
}
