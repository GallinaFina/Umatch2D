using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MovementUI : MonoBehaviour
{
    private Action<MonoBehaviour> onUnitSelected;
    private List<MonoBehaviour> selectableUnits = new List<MonoBehaviour>();
    private List<MonoBehaviour> movedUnits = new List<MonoBehaviour>();
    private bool isSelectingUnit = false;
    public bool IsSelectingUnit => isSelectingUnit;
    public bool IsMovementComplete { get; private set; } = true;
    private MonoBehaviour currentlySelectedUnit;
    public MonoBehaviour CurrentlySelectedUnit => currentlySelectedUnit;
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material movedMaterial;
    private Dictionary<MonoBehaviour, Material> originalMaterials = new Dictionary<MonoBehaviour, Material>();

    public void StartUnitSelection(List<MonoBehaviour> units, Action<MonoBehaviour> callback)
    {
        Debug.Log($"Starting unit selection with {units.Count} units");
        IsMovementComplete = false;
        selectableUnits = units.Where(u => !movedUnits.Contains(u)).ToList();
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

        foreach (var unit in movedUnits)
        {
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = movedMaterial;
            }
        }
    }

    void Update()
    {
        if (!isSelectingUnit) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                MonoBehaviour clickedUnit = hit.collider.GetComponent<MonoBehaviour>();
                if (selectableUnits.Contains(clickedUnit))
                {
                    EndSelection(clickedUnit);
                }
            }
        }
    }

    private void EndSelection(MonoBehaviour selected)
    {
        Debug.Log($"Ending selection with unit: {selected.gameObject.name}");
        isSelectingUnit = false;
        currentlySelectedUnit = selected;

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

    public void MarkUnitMoved(MonoBehaviour unit)
    {
        if (!movedUnits.Contains(unit))
        {
            movedUnits.Add(unit);
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = movedMaterial;
            }
        }
    }

    public void ResetMovedUnits()
    {
        movedUnits.Clear();
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
        else if (unit is Sidekick sidekick)
        {
            while (sidekick.movement > 0)
            {
                yield return null;
            }
        }

        MarkUnitMoved(unit);
        IsMovementComplete = true;

        // Check if any units still have movement
        var remainingUnits = selectableUnits.Where(u => !movedUnits.Contains(u)).ToList();
        if (remainingUnits.Any())
        {
            StartUnitSelection(remainingUnits, onUnitSelected);
        }
    }
}
