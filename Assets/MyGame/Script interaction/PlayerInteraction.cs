using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask interactableLayer = 1;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("UI")]
    public GameObject interactionPrompt;
    
    private Camera playerCamera;
    private IInteractable currentInteractable;

    void Start()
    {
        playerCamera = Camera.main;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    void CheckForInteractable()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                SetCurrentInteractable(interactable);
                return;
            }
        }
        
        SetCurrentInteractable(null);
    }

    void SetCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable != interactable)
        {
            currentInteractable = interactable;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(currentInteractable != null);
            }
        }
    }

    void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactionRange);
    }
}