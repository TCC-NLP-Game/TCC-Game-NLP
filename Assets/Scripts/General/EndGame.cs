using UnityEngine;

public class EndGame : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
    }
}
