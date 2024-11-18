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

    void Start()
    {
        ServiceLocator.Instance.RegisterService(this);
    }

    public void StartUnitSelection(List<MonoBehaviour> units, Action<MonoBehaviour> callback)
    {
        Debug.Log($"Starting unit selection with {units.Count} units");
        IsMovementComplete = false;
        selectableUnits = units;
        onUnitSelected = callback;
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
        currentlySelectedUnit = selected;

        foreach (var unit in selectableUnits)
        {
            if (unit != selected)
            {
                var renderer = unit.GetComponent<Renderer>();
                if (renderer != null && originalMaterials.ContainsKey(unit))
                {
                    renderer.material = originalMaterials[unit];
                }
            }
        }

        onUnitSelected?.Invoke(selected);
    }

    public void MarkUnitMoved(MonoBehaviour unit)
    {
        var baseUnit = unit.GetComponent<BaseUnit>();
        if (baseUnit.movement <= 0)
        {
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = movedMaterial;
            }

            if (!movedUnits.Contains(unit))
            {
                movedUnits.Add(unit);
            }
        }

        var player = ServiceLocator.Instance.GameManager.player;
        bool canStillBoost = player.CanBoost &&
            (player.actionManager.currentAction == ActionState.Maneuvering ||
             player.actionManager.currentAction == ActionState.BoostedManeuvering);

        if (!canStillBoost && selectableUnits.All(u =>
            u.GetComponent<BaseUnit>().movement <= 0 && movedUnits.Contains(u)))
        {
            IsMovementComplete = true;
            isSelectingUnit = false;
        }
        else
        {
            currentlySelectedUnit = null;
            isSelectingUnit = true;
        }
    }

    public void ResetMovedUnits()
    {
        movedUnits.Clear();
        foreach (var material in originalMaterials)
        {
            if (material.Key != null)
            {
                var renderer = material.Key.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material.Value;
                }
            }
        }
        originalMaterials.Clear();
        IsMovementComplete = true;
        currentlySelectedUnit = null;
        isSelectingUnit = false;
    }
}
