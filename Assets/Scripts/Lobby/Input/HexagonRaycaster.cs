using MapSpase.Hexagon;
using MapSpase.Hexagon.Backlight;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class HexagonRaycaster : MonoBehaviour
{
    public HexagonModel TargetHexagon { get; private set; }
    public HandlerHexagonBacklight Backlight { get; private set; }

    public bool TryRaycast()
    {
        ResetCurrentCell();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hit.collider.TryGetComponent(out HexagonModel currentCell))
            {
                Backlight = currentCell.HandlerHexagonBacklight;
                TargetHexagon = currentCell;
                OnRaycast();
                return true;
            }
        }
        return false;
    }

    public void ResetCurrentCell()
    {
        if (TargetHexagon != null)
        {
            OnResetCurrentCell();
            Backlight = null;
            TargetHexagon = null;
        }
    }

    public abstract void OnResetCurrentCell();
    public abstract void OnRaycast();
}
