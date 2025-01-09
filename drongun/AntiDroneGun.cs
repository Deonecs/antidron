using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AntiDroneGun : MonoBehaviour
{
    public float speed = 5f;
    public float batteryCapacity = 100f;
    public int score = 0;
    public Transform centerOfArea;
    public float maxDistanceFromCenter;
    [SerializeField] bool outOfAreaWarning;
    [SerializeField] bool isDefeat;
    public float maxTimeWhenLeave;
    [SerializeField] float tick_maxTimeWhenLeave;
    [Header("-- UI")]
    [SerializeField] GameObject statusPanel;
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI lengthText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI batteryText;

    public GameObject cam;

    Vector2 inputDir;
    [SerializeField] ConeMeshDrawer meshDrawer;
    [SerializeField] MeshCollider meshCollider;
    ServerManager server;

    void Start()
    {
        tick_maxTimeWhenLeave = maxTimeWhenLeave;
        server = UIManager.Instance.server;
    }

    void Update()
    {
        GetInput();
        Move();
        AdjustConeCollider();
        ActivateEW();
        ChargeBattery();
        CheckBoundary();
        SwitchStatusPanel();
        UpdateStatusPanel();

        //if (server.isConnected)
        //    server.SendMessage(PacketManager.SerializeTransformPacket(GameManager.Instance.GetHashCode(), this.transform.position, this.transform.rotation));
    }

    void GetInput()
    {
        inputDir = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Debug.Log(inputDir);
    }

    void Move()
    {
        this.transform.Translate(speed * Time.deltaTime * (this.transform.right * inputDir.x + this.transform.forward * inputDir.y).normalized);
        this.transform.localEulerAngles = new Vector3(0, cam.transform.localEulerAngles.y, 0);
        //SoundManager.Instance.Play(SoundChannel.SFX, 1, 7, true);
    }

    void AdjustConeCollider()
    {
        meshDrawer.Length += OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
    }

    void ActivateEW()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && batteryCapacity > 0)
        {
            SoundManager.Instance.Play(SoundChannel.SFX, 0, 4, loop: true);
        }
        else if (batteryCapacity < 0)
        {
            SoundManager.Instance.Stop(SoundChannel.SFX, 0);
            SoundManager.Instance.Play(SoundChannel.SFX, 0, 5);
            batteryCapacity = 0;
        }
        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            SoundManager.Instance.Stop(SoundChannel.SFX, 0);
            SoundManager.Instance.Play(SoundChannel.SFX, 0, 5);
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && batteryCapacity > 0)
        {
            meshCollider.enabled = true;
            batteryCapacity -= 7.5f * Time.deltaTime;
        }
        else
        {
            meshCollider.enabled = false;
            server.SendMessage(PacketManager.SerializeBoolPacket(this.GetHashCode(), 13, false));
        }
    }

    void ChargeBattery()
    {
        Physics.Raycast(meshDrawer.transform.position, meshDrawer.transform.forward * 3f, out RaycastHit hit);
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) && hit.collider.CompareTag("Battery"))
        {
            batteryCapacity = 150f;
            hit.collider.gameObject.SetActive(false);
            SoundManager.Instance.PlayOneShot(SoundChannel.SFX, 0, 6);
        }
    }

    void CheckBoundary()
    {
        outOfAreaWarning = Vector3.Distance(this.transform.position, centerOfArea.position) >= maxDistanceFromCenter;

        if (outOfAreaWarning)
        {
            tick_maxTimeWhenLeave -= Time.deltaTime;
            SoundManager.Instance.Play(SoundChannel.SFX, 1, 8, loop: true);
            isDefeat = tick_maxTimeWhenLeave <= 0;
        }
        else
        {
            SoundManager.Instance.Stop(SoundChannel.SFX, 1);
            tick_maxTimeWhenLeave = maxTimeWhenLeave;
        }
    }

    void SwitchStatusPanel()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            statusPanel.SetActive(!statusPanel.activeSelf);
        }
    }

    void UpdateStatusPanel()
    {
        strengthText.text = GameManager.Instance.DronegunStrength.ToString();
        lengthText.text = meshDrawer.Length.ToString("0.00");
        timerText.text = GameManager.Instance.SpentTime.ToString("0.00") + "s";
        scoreText.text = GameManager.Instance.ScoreOfAntiDroneGun.ToString();
        batteryText.text = (batteryCapacity / 150 * 100).ToString("00.0") + "%";
    }

    public void ActivateInteraction()
    {
        this.transform.GetChild(1).gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(this.transform.GetChild(0).position, this.transform.GetChild(0).forward * 5f);
    }
}
