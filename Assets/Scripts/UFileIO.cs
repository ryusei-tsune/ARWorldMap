using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using NavMeshExtension;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation.Samples;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

/// <summary>
/// ユーザの環境マップロードのプログラム
/// </summary>
public class UFileIO : MonoBehaviour
{
    [Tooltip("The ARSession component controlling the session from which to generate ARWorldMaps.")]
    [SerializeField]
    ARSession m_ARSession;
    /// <summary>
    /// The ARSession component controlling the session from which to generate ARWorldMaps.
    /// </summary>
    public ARSession arSession
    {
        get { return m_ARSession; }
        set { m_ARSession = value; }
    }

    public ARSessionOrigin arsessionOrigin;
    public Text statusMessage;
    public GameObject landmarkPrefab;  //コピー対象;

    //   ARWorldMappingStatus mappingStatus;
    public GameObject mapInstantiate;
    public Material mapMaterial;
    //   public Toggle visual;
    //   public Text mapindicator;
    //   public Text ARCameraStatusIndicator;
    //   public Text saveStatusIndicator;




    //画像認識プログラムから環境マッププログラムへの切り替え
    void Start()
    { 
        UpdateDropdown();

        arsessionOrigin.GetComponent<ARPlaneManager>().enabled = true;    //環境マップに関するプログラムを起動
        arsessionOrigin.GetComponent<ARPointCloudManager>().enabled = true;    //環境マップに関するプログラムを起動

        TrackedImageInfoManager trackedImageInfoManager = arsessionOrigin.GetComponent<TrackedImageInfoManager>();
        string filename = trackedImageInfoManager.Filename;     //画像認識で決定したファイル名を取得

        arsessionOrigin.GetComponent<TrackedImageInfoManager>().enabled = false;    //画像認識のプログラムを停止
        arsessionOrigin.GetComponent<ARTrackedImageManager>().enabled = false;    //画像認識のプログラムを停止

        LoadStart(filename);
    }
    


    // 環境マップのロードを行う
    public void LoadStart(string filename)
    {
        string sWpathstr;
        string sApathstr;

#if UNITY_EDITOR
        sApathstr = Application.dataPath + "/" + filename;
        sWpathstr = filename;
        sWpathstr = Path.Combine(Application.dataPath, sWpathstr.Replace("ARMAP", "RoWMap"));
#elif UNITY_IPHONE
        sApathstr = Application.persistentDataPath + "/" + filename;
        sWpathstr = filename;
        sWpathstr = Path.Combine(Application.persistentDataPath, sWpathstr.Replace( "ARMAP", "RoWMap"));
#endif

        if (File.Exists(sApathstr))
        {
            if (File.Exists(sWpathstr))
            {
                Debug.Log(sWpathstr);
                try
                {
#if UNITY_IOS
                    StartCoroutine(Load(sWpathstr, sApathstr));
#endif
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex);
                }

            }
            else
            {
                statusMessage.text = "load RoWMap failed.";
                arsessionOrigin.GetComponent<ARPlaneManager>().enabled = false;
                arsessionOrigin.GetComponent<ARPointCloudManager>().enabled = false;
                arsessionOrigin.GetComponent<ARTrackedImageManager>().enabled = true;
                arsessionOrigin.GetComponent<TrackedImageInfoManager>().enabled = true;

            }
        }
        else
        {
            statusMessage.text = "load ARMap failed.";
            arsessionOrigin.GetComponent<ARPlaneManager>().enabled = false;
            arsessionOrigin.GetComponent<ARPointCloudManager>().enabled = false;
            arsessionOrigin.GetComponent<ARTrackedImageManager>().enabled = true;
            arsessionOrigin.GetComponent<TrackedImageInfoManager>().enabled = true;
        }
    }



    //ファイルパスから環境マップ（平面検知の情報）をロード（基本的にGitのコードのまま）
#if UNITY_IOS
    IEnumerator Load(string sWpathstr, string sApathstr)
    {

        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            yield break;
        }

        var file = File.Open(sWpathstr, FileMode.Open);
        if (file == null)
        {
            yield break;
        }


        int bytesPerFrame = 1024 * 10;
        var bytesRemaining = file.Length;
        var binaryReader = new BinaryReader(file);
        var allBytes = new List<byte>();
        while (bytesRemaining > 0)
        {
            var bytes = binaryReader.ReadBytes(bytesPerFrame);
            allBytes.AddRange(bytes);
            bytesRemaining -= bytesPerFrame;
            yield return null;
        }

        var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
        data.CopyFrom(allBytes.ToArray());

        Debug.Log(string.Format("Deserializing to ARWorldMap...", sWpathstr));

        if (ARWorldMap.TryDeserialize(data, out ARWorldMap worldMap))
            data.Dispose();

        if (worldMap.valid)
        {
            statusMessage.text = "Deserialized successfully.";          //確認用
        }
        else
        {
            Debug.LogError("Data is not a valid ARWorldMap.");
            yield break;
        }

        sessionSubsystem.ApplyWorldMap(worldMap);
        LoadMap(sApathstr);
        statusMessage.text = "Load完了";          //確認用
    }
#endif



    //李さんのプログラムの一部を使用．目印となるオブジェクトの生成や座標をXMLファイルから読み出す
    public void LoadMap(string sApathstr)
    {
        VariableCP.landMarks.Clear();
        VariableCP.navMeshes.Clear();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(sApathstr);
        statusMessage.text = "Load ARMap";

        XmlNodeList nodeList = xmlDoc.SelectSingleNode("gameObjects").ChildNodes;
        GameObject arMap = Instantiate(mapInstantiate);
        foreach (XmlElement gmobject in nodeList)
        {
            if(gmobject.Name == "maps")
            {
                foreach(XmlElement map in gmobject.ChildNodes)
                {
                    List<Vector3> verticsList = new List<Vector3>();
                    List<int> trianglesList = new List<int>();
                    Vector3 pos = Vector3.zero;
                    Vector3 rot = Vector3.zero;
                    Vector3 sca = Vector3.zero;

                    foreach (XmlElement prs in map.ChildNodes)
                    {
                        if (prs.Name == "position")
                        {
                            foreach (XmlElement pposition in prs.ChildNodes)
                            {
                                switch (pposition.Name)
                                {
                                    case "x":
                                        pos.x = float.Parse(pposition.InnerText);
                                        break;
                                    case "y":
                                        pos.y = float.Parse(pposition.InnerText);
                                        break;
                                    case "z":
                                        pos.z = float.Parse(pposition.InnerText);
                                        break;
                                }
                            }
                        }
                        else if(prs.Name == "rotation")
                        {
                            foreach (XmlElement rrotation in prs.ChildNodes)
                            {
                                switch (rrotation.Name)
                                {
                                    case "x":
                                        rot.x = float.Parse(rrotation.InnerText);
                                        break;
                                    case "y":
                                        rot.y = float.Parse(rrotation.InnerText);
                                        break;
                                    case "z":
                                        rot.z = float.Parse(rrotation.InnerText);
                                        break;
                                }
                            }
                        }
                        else if(prs.Name == "scale")
                        {
                            foreach (XmlElement sscale in prs.ChildNodes)
                            {
                                switch (sscale.Name)
                                {
                                    case "x":
                                        sca.x = float.Parse(sscale.InnerText);
                                        break;
                                    case "y":
                                        sca.y = float.Parse(sscale.InnerText);
                                        break;
                                    case "z":
                                        sca.z = float.Parse(sscale.InnerText);
                                        break;
                                }
                            }
                        }
                        else if(prs.Name == "MeshData")
                        {
                            foreach (XmlElement mdItem in prs.ChildNodes)
                            {
                                Vector3 temp = new Vector3(0, 0, 0);
                                if(mdItem.Name == "Vertics")
                                {
                                    foreach (XmlElement meshVertex in mdItem.ChildNodes)
                                    {
                                        foreach(XmlElement Pvalue in meshVertex.ChildNodes)
                                        {
                                            switch (Pvalue.Name)
                                            {
                                                case "Vt_x":
                                                    temp.x = float.Parse(Pvalue.InnerText);
                                                    break;
                                                case "Vt_y":
                                                    temp.y = float.Parse(Pvalue.InnerText);
                                                    break;
                                                case "Vt_z":
                                                    temp.z = float.Parse(Pvalue.InnerText);
                                                    break;
                                            }
                                        }
                                        verticsList.Add(temp);
                                    }
                                }
                                else if(mdItem.Name == "Triangles")
                                {
                                    foreach (XmlElement item in mdItem.ChildNodes)
                                    {
                                        trianglesList.Add(int.Parse(item.InnerText));
                                    }
                                }
                            }
                        }
                    }

                    GameObject ob = new GameObject("New NavMesh");
                    ob.transform.parent = arMap.transform;
                    ob.AddComponent<NavMeshModifier>();
                    ob.AddComponent<NavMeshObject>();
                    ob.AddComponent<MeshFilter>();
                    ob.AddComponent<MeshRenderer>();
                    
                    ob.GetComponent<MeshRenderer>().material = mapMaterial; //test1
                    ob.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 0.0f, 1.0f, 0.1f);//test2
                    /*
                    NavMeshManager mapmesh = arMap.GetComponent<NavMeshManager>();
                    MeshRenderer mRenderer = ob.GetComponent<MeshRenderer>();
                    mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mRenderer.receiveShadows = false;
                    if (mapmesh.meshMaterial)
                    {
                        mapmesh.meshMaterial.color = new Color(0.0f, 0.0f, 1.0f, 0.3f);
                        mRenderer.sharedMaterial = mapmesh.meshMaterial;
                    }
                    else
                    {
                        mRenderer.enabled = false;
                    }
                    */
                    ob.transform.position = pos;
                    ob.transform.rotation = Quaternion.Euler(rot);
                    ob.transform.localScale = sca;


                    VariableCP.navMeshes.Add(ob);

                    ob.GetComponent<MeshFilter>().mesh.vertices = verticsList.ToArray();
                    ob.GetComponent<MeshFilter>().mesh.triangles = trianglesList.ToArray();
                }

            }
            else if (gmobject.Name == "landMarks")
            {
                foreach (XmlElement landmark in gmobject.ChildNodes)
                {
                    Vector3 pos = Vector3.zero;
                    Vector3 rot = Vector3.zero;
                    Vector3 sca = Vector3.zero;
                    foreach (XmlElement prs in landmark.ChildNodes)
                    {
                        if (prs.Name == "position")
                        {
                            foreach (XmlElement pposition in prs.ChildNodes)
                            {
                                switch (pposition.Name)
                                {
                                    case "x":
                                        pos.x = float.Parse(pposition.InnerText);
                                        break;
                                    case "y":
                                        pos.y = float.Parse(pposition.InnerText);
                                        break;
                                    case "z":
                                        pos.z = float.Parse(pposition.InnerText);
                                        break;
                                }

                            }
                        }
                        else if (prs.Name == "rotation")
                        {
                            foreach (XmlElement rrotation in prs.ChildNodes)
                            {
                                switch (rrotation.Name)
                                {
                                    case "x":
                                        rot.x = float.Parse(rrotation.InnerText);
                                        break;
                                    case "y":
                                        rot.y = float.Parse(rrotation.InnerText);
                                        break;
                                    case "z":
                                        rot.z = float.Parse(rrotation.InnerText);
                                        break;
                                }

                            }
                        }
                        else if (prs.Name == "scale")
                        {
                            foreach (XmlElement sscale in prs.ChildNodes)
                            {
                                switch (sscale.Name)
                                {
                                    case "x":
                                        sca.x = float.Parse(sscale.InnerText);
                                        break;
                                    case "y":
                                        sca.y = float.Parse(sscale.InnerText);
                                        break;
                                    case "z":
                                        sca.z = float.Parse(sscale.InnerText);
                                        break;
                                }

                            }
                        }


                        /*
                        else if (prs.Name == "dataContainer")
                        {
                            foreach (XmlElement ddata in prs.ChildNodes)
                            {
                                switch (ddata.Name)
                                {
                                    case "NewText":
                                        tmpstring[0] = ddata.InnerText;
                                        break;

                                    case "NewText1":
                                        tmpstring[1] = ddata.InnerText;
                                        break;
                                    case "NewText2":
                                        tmpstring[2] = ddata.InnerText;
                                        break;
                                    case "NewText3":
                                        tmpstring[3] = ddata.InnerText;
                                        break;
                                    case "NewText4":
                                        tmpstring[4] = ddata.InnerText;
                                        break;
                                    case "NewText5":
                                        tmpstring[5] = ddata.InnerText;
                                        break;
                                    case "NewText6":
                                        tmpstring[6] = ddata.InnerText;
                                        break;
                                }
                            }
                        }
                        */

                    }

                    GameObject ob = Instantiate(landmarkPrefab, pos, Quaternion.Euler(rot));
                    ob.transform.localScale = sca;
                    VariableCP.landMarks.Add(ob);

                    /*                   
                                       Component[] items = ob.GetComponentsInChildren<TextMesh>();
                                       int i = 0;
                   　　　　　　　　　　foreach (TextMesh item in items)
                                       {
                                           item.text = tmpstring[i];
                                           i++;
                                       }

                                       if (!visual.isOn)
                                       {
                                           ob.SetActive(false);
                                       }
                   */
                }
            }
        }
    }



    
    // 保存した環境マップを選択できるように選択肢を更新
    // Updates the dropdown
    public void UpdateDropdown()
    {
        //Pathに関する作業（ファイルを/dataPath/の下に作成）
        try
        {
#if UNITY_EDITOR
            if (Directory.Exists(Application.dataPath + "/Inbox/"))
            {
                Debug.Log("Inbox accesse");
                string[] fileList = Directory.GetFiles(Application.dataPath + "/Inbox/");
#elif UNITY_IPHONE
            if(Directory.Exists(Application.persistentDataPath + "/Inbox/"))
            {
                Debug.Log("Inbox accesse");
                string[] fileList = Directory.GetFiles(Application.persistentDataPath + "/Inbox/");
#endif

                // dataPath/Inbox/ファイル名　→　dataPath/ファイル名
                if (fileList != null)
                {
                    Debug.Log("File list");
                    string replaceFile;
                    foreach (var item in fileList)
                    {
                        replaceFile = item.Replace("/Inbox/", "/");
                        if (File.Exists(replaceFile)) File.Delete(replaceFile);     //File.Copyは同じ名前のファイルを上書きできないため

                        File.Copy(item, replaceFile);            //itemの内容をreplacefileにコピー
                        File.Delete(item);
                    }
                }
            }
        }
        catch (System.Exception ex)  //例外処理
        {
            Debug.Log(ex);
        }

        // /dataPath/に存在するファイル名をfileSelectorに追加
/*
#if UNITY_EDITOR
        string[] temp = Directory.GetFiles(Application.dataPath + "/");
#elif UNITY_IPHONE
        string[] temp = Directory.GetFiles(Application.persistentDataPath + "/");
#endif

        List<string> m_dropOptions = new List<string>();

        // dataPath/ファイル名　→　ファイル名
        foreach (var go in temp)
        {
            string t;
#if UNITY_EDITOR
            t = go.Replace(Application.dataPath + "/", "");
#elif UNITY_IPHONE
            t = go.Replace(Application.persistentDataPath + "/", "");
#endif
            m_dropOptions.Add(t);   //ファイル名を一時的に格納
        }

        fileSelector.ClearOptions();
        fileSelector.AddOptions(m_dropOptions);
 */
    }
    
    /// <summary>
    /// Reset the <c>ARSession</c>, destroying any existing trackables,
    /// such as planes. Upon loading a saved <c>ARWorldMap</c>, saved
    /// trackables will be restored.
    /// </summary>
    public void OnResetButton()
    {
        m_ARSession.Reset();
    }



    private void Update()
    {
#if UNITY_IOS
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
#else
        XRSessionSubsystem sessionSubsystem = null;
#endif
        if (sessionSubsystem == null)
            return;

    }
}