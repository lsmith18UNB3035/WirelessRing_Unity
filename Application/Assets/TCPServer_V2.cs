using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System;
using TMPro;

#if !UNITY_EDITOR
using System.Threading.Tasks;
#else
using System.Threading;
#endif

public class TCPServer_V2 : MonoBehaviour
{

#if !UNITY_EDITOR
    private Windows.Networking.Sockets.StreamSocket socket;
    private Task socketListenTask;
#endif

    public int connectionPort = 23456;
    private string lensPort = "23456";
    private string host = "127.0.0.1";
    private string address = "10.0.0.117";
    TcpListener server;
    private StreamReader reader;
    bool connected = false;
    Vector3 newPos = new Vector3(0.0f, 0.0f, 10.0f);
    private float offset = 5.0f;
    public float currentX = 0.0f;
    public float currentY = 0.0f;

    public Spawner spawner;
    public process pc;
    private StreamWriter writer;
    public int clickFlag = 0;

#if UNITY_EDITOR
    TcpClient client;
#endif

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        spawner = FindObjectOfType<Spawner>();
        pc = GameObject.FindWithTag("cursor").GetComponent<process>();

        ConnectSocketUWP();
#else             
        spawner = FindObjectOfType<Spawner>();
        ConnectSocketUnity();
        ThreadStart ts = new ThreadStart(ListenForDataUnity);
        var thread = new Thread(ts);
        thread.IsBackground = true;
        thread.Start();
#endif

    }
#if !UNITY_EDITOR
    private async void ConnectSocketUWP()
    {
        try
        {
            socket = new Windows.Networking.Sockets.StreamSocket();
            Windows.Networking.HostName serverHost = new Windows.Networking.HostName(address);
            await socket.ConnectAsync(serverHost, lensPort);
            Stream streamIn = socket.InputStream.AsStreamForRead();
            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            reader = new StreamReader(streamIn, Encoding.UTF8);
            writer = new StreamWriter(streamOut, Encoding.UTF8);
            connected = true;
            Debug.Log("Connected");

            /**for (int i = 0; i < 16; i++)
            {
                Debug.Log(spawner.clickLocations[i]);
            }**/
        }
        catch (Exception e)
        {
            Debug.Log("Connection Error");
        }
    }
#else
    void ConnectSocketUnity()
    {
        IPAddress ipAddress = IPAddress.Parse(host);
        Debug.Log("ip result: " + ipAddress);
        Debug.Log("port being sent " + connectionPort);
        client = new TcpClient();
        try
        {
            client.Connect(ipAddress, connectionPort);
        }
        catch
        {
            Debug.Log("error connecting to socket servers");
        }
    }
#endif

    public void SendClickData()
    {
#if !UNITY_EDITOR
        for (int i = 0; i < 16; i++)
        {
            writer.WriteLine(spawner.clickLocations[i]);
            writer.Flush();
        }
        for(int i = 0; i < 3; i++)
        {
            writer.WriteLine(spawner.otherData[i]);
            writer.Flush();
        }
#else
        NetworkStream stream2 = client.GetStream();
        writer = new StreamWriter(stream2);
        for (int i = 0; i < 16; i++)
        {
            Debug.Log(spawner.clickLocations[i]);
            //writer.flush();
            byte[] bytesToSend = (ASCIIEncoding.ASCII.GetBytes(spawner.clickLocations[i])) ;
            Debug.Log("Sending this size: " + bytesToSend.Length);
            stream2.Write(bytesToSend, 0, bytesToSend.Length);
        }
        for(int i = 0; i < 3; i++)
        {
            writer.WriteLine(spawner.otherData[i]);
            writer.Flush();
        }

#endif
    }

#if !UNITY_EDITOR
    private void ListenForDataUWP()
    {
        try
        {
            string lensData;
            lensData = reader.ReadLine();
            //spawner.updateDisplay(lensData, false);
            if (String.Equals(lensData, "done"))
            {
                SendClickData();
            }
            string[] coords = lensData.Split(",");
     
            if (coords[0] == "XX")
            {
                Debug.Log("Reading click");
                clickFlag = 1;

            }
            else
            {
                float differenceX = ((float.Parse(coords[0])) / 120.0f);
                float differenceY = ((float.Parse(coords[1])) / 15.0f);
                if ((currentX + differenceX < -5.0f) || (currentX + differenceX > 5.0f))
                {
                    differenceX = 0.0f;
                }
                if ((currentY + differenceY < -5.0f) || (currentY + differenceY > 5.0f))
                {
                    differenceY = 0.0f;
                }
                newPos = new Vector3((currentX + differenceX), (currentY + differenceY), -10.0f);
                newPos.z = offset;
            }
            

        }
        catch (Exception e)
        {
            Debug.Log("Do nothing");
        }
    }
#else
        void ListenForDataUnity()
        {
            int data = 1;
            float scaleFactor = 7.0f;
            string translated = "start";
            while (!(String.Equals(translated, "done\n")))
            {
                byte[] bytes = new byte[client.ReceiveBufferSize];
                NetworkStream stream = client.GetStream();
                data = stream.Read(bytes, 0, client.ReceiveBufferSize);
                Debug.Log("data: " + data);
                translated = Encoding.UTF8.GetString(bytes, 0, data);
                if (!(String.Equals(translated, "done\n")))
                {
                    string[] coords = translated.Split(",");
                    if(coords[0] == "XX"){
                        Debug.Log("Reading click");
                        clickFlag = 1;
                                                
                    }
                    else{
                        float differenceX = ((float.Parse(coords[0])) / 110.0f);
                        float differenceY = ((float.Parse(coords[1])) / 110.0f);
                        if((currentX + differenceX < -10.0f) || (currentX + differenceX > 10.0f))
                        {
                            differenceX = 0.0f;
                        }
                        if ((currentY + differenceY < -16.0f) || (currentY + differenceY > 5.0f))
                        {
                            differenceY = 0.0f;
                        }

                        newPos = new Vector3((currentX + differenceX), (currentY + differenceY), -10.0f);
                        newPos.z = offset;
                        Debug.Log("new position: " + newPos);
                    }
                    
                }
                else{
                    SendClickData();
                }
            }
        }
#endif
    /*
    void GetData()
    {
        Debug.Log("Confirm in here");
        server = new TcpListener(IPAddress.Any, connectionPort);
        server.Start();

        client = server.AcceptTcpClient();

        Debug.Log(client.GetType());

        running = true;
        while (running)
        {
            Connection();
        }
        server.Stop();
        thread.Abort();
    }
    
    void Connection()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

        string dataRecieved = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Debug.Log("Read this: " + dataRecieved);
        string response = "Coords recieved";
        nwStream.Write(buffer, 0, bytesRead);


    }
    */

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR
        if (socketListenTask == null || socketListenTask.IsCompleted)
        {
            socketListenTask = new Task(async () => { ListenForDataUWP(); });
            socketListenTask.Start();
        }
#endif
        currentX = transform.position.x;
        currentY = transform.position.y;
        transform.position = newPos;
    }
}
