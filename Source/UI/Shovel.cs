using UnityEngine;
using UnityEngine.InputSystem;


public class Shovel : MonoBehaviour
{
    Vector3 home;
    Camera cam;
    [SerializeField] GameObject slot;

    private void Start()
    {
        home = transform.localPosition;
        cam = LevelUI.Instance.cam;
    }
    bool followingMouse = false;
    private void OnMouseDown()
    {
        //first click
        if (!followingMouse)
        {
            followingMouse = true;
            Hotbar.SelectPlant((int)ActionState.Shovel);
        }
        //second click
        else
        {
            transform.localPosition = home;
            followingMouse = false;
        }
    }

    private void Update()
    {
        if(followingMouse)
        {
            Vector3 mousePosition = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePosition.z = transform.position.z;

            transform.position = mousePosition;
        }
    }
}
