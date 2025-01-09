using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ConeMeshDrawer : MonoBehaviour
{
    public float Width
    {
        get { return width; }
        set
        {
            width = value;
            length = Strength / width;
            SetMeshData(length, width);
        }
    }

    public float Length
    {
        get { return length; }
        set
        {
            length = value;
            width = Strength / length;
            SetMeshData(length, width);
        }
    }

    public float Strength
    {
        get { return GameManager.Instance.DronegunStrength; }
    }

    public float width, length;

    float preWidth, preLength;
    Mesh mesh;
    Vector3[] va;
    int[] ia;

    public ServerManager server;

    private void Start()
    {
        mesh = this.GetComponent<MeshFilter>().mesh;
        this.GetComponent<MeshRenderer>().enabled = GameManager.Instance.RayVisibilty;
        Length = 20;
        server = UIManager.Instance.server;
        //preWidth = width;
        //preLength = length;
        //SetMeshData(length, width);
        //DrawMesh();
        
    }

    private void Update()
    {
        /*
        if (preWidth != width)
        {
            length = ratio / width;
        }
        else if (preLength != length)
        {
            width = ratio / length;
        }

        preLength = length;
        preWidth = width;
        */
        //SetMeshData(length, width);
        DrawMesh();
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            if (other.TryGetComponent<Drone_Single>(out Drone_Single drone_single))
            {
                drone_single.hp -= 1000 * Time.deltaTime / Vector3.Distance(other.transform.position, this.transform.position);
                if (drone_single.hp < 0)
                {
                    drone_single.WhenDestroy();
                }
                Debug.Log("Drone HP: " + drone_single.hp);
            }
            else if (server.isConnected)
                server.SendMessage(PacketManager.SerializeBoolPacket(this.GetHashCode(), 13, true));
            /*
            else
            {
                if (other.TryGetComponent<Drone>(out Drone drone))
                {
                    Drone.curHealth -= 1000 * Time.deltaTime / Vector3.Distance(other.transform.position, this.transform.position);
                    /*
                    if(Drone.curHealth < 0)
                    {
                       drone.gameObject.SetActive(false);
                        GameManager.Instance.droneRespawnCount--;
                    }
                    Debug.Log("Drone HP: " + Drone.curHealth);
                }
            }
            */
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            if (server.isConnected)
                server.SendMessage(PacketManager.SerializeBoolPacket(this.GetHashCode(), 13, false));
        }
    }
    

    void SetMeshData(float length, float width)
    {
        //Set Vertex Array
        va = new Vector3[22];
        va[0] = new Vector3(0, 0, length);
        for(int i = 1; i < 21; i++)
        {
            float angle = i * Mathf.PI / 10f;
            va[i] = new Vector3(Mathf.Cos(angle) * width, Mathf.Sin(angle) * width, length);
        }
        va[21] = Vector3.zero;

        //Set Index Array
        ia = new int[120];
        for(int i = 0; i < 20; i++)
        {
            ia[3*i] = 0;
            ia[3 * i + 1] = i + 1;
            ia[3 * i + 2] = i + 2;

            ia[60 + 3 * i] = 21;
            ia[61 + 3 * i] = i + 2;
            ia[62 + 3 * i] = i + 1;
        }
        ia[59] = 1;
        ia[118] = 1;
    }

    void DrawMesh()
    {
        mesh.Clear();
        mesh.vertices = va;
        mesh.triangles = ia;
        mesh.RecalculateNormals();
        this.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
