using UnityEngine;
using UnityEngine.UI;

public class VersionNumberTextDisplayController : MonoBehaviour
{

    void Start()
    {
        GetComponent<Text>().text = "v" + Application.version;
    }
}
