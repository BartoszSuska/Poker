using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class OfflineSceneMenu : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] TMP_InputField addressToConnect;
    [SerializeField] TMP_InputField nickname;
    [SerializeField] string[] skinNames;
    int skinNumber;
    [SerializeField] TMP_Text skinNameText;
    Camera mainCamera;
    public bool host;
    float waitForTumbleweed;
    [SerializeField] Transform tumbleweedSpawner;
    [SerializeField] GameObject tumbleweed;
    [SerializeField] Transform clouds;
    [SerializeField] Transform doors1;
    [SerializeField] Transform doors2;

    void Start()
    {
        mainCamera = Camera.main;
        waitForTumbleweed = Random.Range(5, 30);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
    }

    void Update()
    {
        skinNameText.text = skinNames[skinNumber];

        if(waitForTumbleweed <= 0)
        {
            GameObject _tumbleweed = Instantiate(tumbleweed, tumbleweedSpawner);
            Destroy(_tumbleweed, 15);
            waitForTumbleweed = Random.Range(10, 30);
        }
        else
        {
            waitForTumbleweed -= Time.deltaTime;
        }

        clouds.Rotate(Vector3.up * Time.deltaTime);
    }

    public void NextSkin(int sign)
    {
        skinNumber += sign;

        if(skinNumber >= skinNames.Length)
        {
            skinNumber = 0;
        }
        else if(skinNumber < 0)
        {
            skinNumber = skinNames.Length - 1;
        }
    }

    public void SetAddress()
    {

        if (addressToConnect.text != null)
        {
            networkManager.networkAddress = addressToConnect.text;

        }

        Debug.Log(networkManager.networkAddress);
    }

    public void SetPlayerInfo()
    {
        string nick = nickname.text;

        if(nick == "")
        {
            nick = "Player " + Random.Range(1, 10000);
        }

        PlayerPrefs.SetString("Nickname", nick.ToUpper());

        PlayerPrefs.SetInt("Model", skinNumber);
    }

    public void DisableCanvas(GameObject canvas)
    {
        canvas.SetActive(false);
    }

    public void ActivateCanvas(GameObject canvas)
    {
        StartCoroutine(CanvasIn(canvas));
    }

    public void MoveCamera(Transform location)
    {
        StartCoroutine(SlerpRot(mainCamera.transform.rotation, location.rotation, 2));
        StartCoroutine(SlerpPos(mainCamera.transform.position, location.position, 2));
    }

    public void SetHost(bool _host) //true = host || false = client
    {
        host = _host;
    }

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    public void MoveDoors(bool open)
    {
        Quaternion oneRotOpen;
        Quaternion twoRotOpen;

        if(open)
        {
            oneRotOpen = new Quaternion(0, -90, 0, 0);
            twoRotOpen = new Quaternion(0, 90, 0, 0);
            StartCoroutine(OpenDoors(Vector3.up * -90, Vector3.up * 90));
        }
        else
        {
            StartCoroutine(OpenDoors(Vector3.up * 90, Vector3.up * -90));
            oneRotOpen = new Quaternion(0, 0, 0, 0);
            twoRotOpen = new Quaternion(0, 0, 0, 0);
        }
    }

    IEnumerator CanvasIn(GameObject canvas)
    {
        yield return new WaitForSeconds(2f);
        canvas.SetActive(true);
    }

    IEnumerator OpenDoors(Vector3 byAnglesOne, Vector3 byAnglesTwo)
    {
        Quaternion oneRot = doors1.rotation;
        Quaternion twoRot = doors2.rotation;
        Quaternion oneRotTo = Quaternion.Euler(doors1.eulerAngles + byAnglesOne);
        Quaternion twoRotTo = Quaternion.Euler(doors2.eulerAngles + byAnglesTwo);

        for (float t = 0f; t < 1; t += Time.deltaTime / 2)
        {
            doors1.rotation = Quaternion.Lerp(oneRot, oneRotTo, t);
            doors2.rotation = Quaternion.Lerp(twoRot, twoRotTo, t);
            yield return null;
        }
    }

    IEnumerator SlerpRot(Quaternion startRot, Quaternion endRot, float slerpTime)
    {
        float elapsed = 0;
        while (elapsed < slerpTime)
        {
            elapsed += Time.deltaTime;

            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / slerpTime);

            yield return null;
        }
    }

    IEnumerator SlerpPos(Vector3 startRot, Vector3 endRot, float slerpTime)
    {
        float elapsed = 0;
        while (elapsed < slerpTime)
        {
            elapsed += Time.deltaTime;

            mainCamera.transform.position = Vector3.Slerp(startRot, endRot, elapsed / slerpTime);

            yield return null;
        }
    }

    IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(1.9f);

        if (host)
        {
            networkManager.StartHost();
        }
        else
        {
            networkManager.StartClient();
        }
    }
}
