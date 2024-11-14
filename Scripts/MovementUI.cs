using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MovementUI : MonoBehaviour
{
    private Action<MonoBehaviour> onUnitSelected;
    private MonoBehaviour[] selectableUnits;
    private bool isSelectingUnit = false;
    public bool IsSelectingUnit => isSelectingUnit;
    public bool IsMovementComplete { get; private set; } = true;
    public Material defaultMaterial;
    public Material highlightMaterial;
    private Dictionary<MonoBehaviour, Material> originalMaterials = new Dictionary<MonoBehaviour, Material>();

    public void StartUnitSelection(MonoBehaviour[] units, Action<MonoBehaviour> callback)
    {
        Debug.Log("Starting unit selection");
        IsMovementComplete = false;
        selectableUnits = units;
        onUnitSelected = (selected) => {
            callback(selected);
            StartCoroutine(WaitForMovement(selected));
        };
        isSelectingUnit = true;

        foreach (var unit in selectableUnits)
        {
            Debug.Log($"Making unit selectable: {unit.gameObject.name}");
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials[unit] = renderer.material;
                renderer.material = highlightMaterial;
            }
        }
    }

    void Update()
    {
        if (!isSelectingUnit) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click detected while selecting unit");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"Attempting raycast at position: {mousePosition}");
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} on layer: {hit.collider.gameObject.layer}");
                MonoBehaviour clickedUnit = hit.collider.GetComponent<MonoBehaviour>();
                if (selectableUnits.Contains(clickedUnit))
                {
                    Debug.Log($"Valid unit selected: {clickedUnit.gameObject.name}");
                    EndSelection(clickedUnit);
                }
            }
            else
            {
                Debug.Log("No object hit by raycast");
            }
        }
    }

    private void EndSelection(MonoBehaviour selected)
    {
        Debug.Log($"Ending selection with unit: {selected.gameObject.name}");
        isSelectingUnit = false;

        foreach (var unit in selectableUnits)
        {
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null && originalMaterials.ContainsKey(unit))
            {
                renderer.material = originalMaterials[unit];
            }
        }
        originalMaterials.Clear();

        onUnitSelected?.Invoke(selected);
    }

    private IEnumerator WaitForMovement(MonoBehaviour unit)
    {
        Debug.Log($"Waiting for movement completion of: {unit.gameObject.name}");
        if (unit is Player player)
        {
            while (player.movement > 0)
            {
                yield return null;
            }
        }
        else if (unit is Enemy enemy)
        {
            while (enemy.movement > 0)
            {
                yield return null;
            }
        }
        IsMovementComplete = true;
    }
}
