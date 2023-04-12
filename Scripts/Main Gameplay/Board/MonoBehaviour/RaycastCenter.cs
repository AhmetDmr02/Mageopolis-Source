using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastCenter : MonoBehaviour
{
    public static RaycastCenter instance;
    public GameObject lookingObject { get; private set; }
    public static event Action<GameObject> lookingObjectChanged;
    public static event Action<GameObject> lookingObjectLeftClicked;
    public static event Action<GameObject> lookingObjectRightClicked;
    public static event Action<GameObject> lookingObjectMiddleClicked;
    [SerializeField] private Camera cam;
    private RaycastHit hit;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    void Update()
    {
        castRay();
    }
    private void castRay()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hit.transform.gameObject != lookingObject)
            {
                lookingObject = hit.transform.gameObject;
                changeLookedObject(lookingObject);
            }
            if (Input.GetMouseButtonDown(0))
            {
                lookingObjectLeftClicked?.Invoke(lookingObject);
            }
            if (Input.GetMouseButtonDown(1))
            {
                lookingObjectRightClicked?.Invoke(lookingObject);
            }
            if (Input.GetMouseButtonDown(2))
            {
                lookingObjectMiddleClicked?.Invoke(lookingObject);
            }
        }
    }
    private void changeLookedObject(GameObject go)
    {
        lookingObjectChanged?.Invoke(go);
    }
}
