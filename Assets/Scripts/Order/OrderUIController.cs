using UnityEngine;
using System.Collections.Generic;

public class OrderUIController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer dishRenderer;
    [SerializeField] private Transform ingredientContainer; // A child transform where ingredient icons will be spawned
    [SerializeField] private GameObject spritePrefab; // A simple prefab with a SpriteRenderer, used for ingredient icons

    public void SetupOrder(DishTypeMapping dishData, List<IngredientMapping> ingredientDatas)
    {
        // 1. Set the main dish image
        if (dishRenderer != null && dishData.sprite != null)
        {
            dishRenderer.sprite = dishData.sprite;
            dishRenderer.transform.localScale = Vector3.one * dishData.iconScale;

            // auto-rotate the dish to face the player (assuming the order UI is a 3D object in the world)
            dishRenderer.transform.localRotation = Quaternion.Euler(0, 90f, 0);
        }

        // 2. Clear the old ingredients
        foreach (Transform child in ingredientContainer) Destroy(child.gameObject);

        // 3. generate new ingredient icons with proper spacing and random rotation for a "messy" look
        float spacing = 0.25f;
        for (int i = 0; i < ingredientDatas.Count; i++)
        {
            GameObject iconObj = Instantiate(spritePrefab, ingredientContainer);
            iconObj.GetComponent<SpriteRenderer>().sprite = ingredientDatas[i].sprite;

            // Calculate the Z-axis offset for centering
            float zOffset = (i - (ingredientDatas.Count - 1) / 2f) * spacing;

            // 1. Set the base orientation (Y-axis 90 degrees)
            // This will make the image face forward on the X-axis
            iconObj.transform.localRotation = Quaternion.Euler(0, 90f, 0);

            // 2. Deal with the "tilt" on the paper (random rotation)
            // Since you rotated the Y-axis by 90 degrees, now you want it to look "slanted" on the paper
            // Usually, this should be around the image's own Z-axis (Local Z)
            iconObj.transform.Rotate(Vector3.forward, Random.Range(-10f, 10f));

            // 3. Make sure the position is correct (X-axis raised, Z-axis translated based on index)
            iconObj.transform.localPosition = new Vector3(0.005f, 0, zOffset);

            // 4. Set the scale
            float s = ingredientDatas[i].iconScale;
            iconObj.transform.localScale = Vector3.one * s;
        }
    }
}