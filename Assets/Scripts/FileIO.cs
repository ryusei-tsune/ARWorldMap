using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif


/// <summary>
/// 管理者の環境マップ保存プログラム
/// </summary>
public class FileIO : MonoBehaviour
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

    public InputField fileName;         //保存ファイルの名前;
    public Dropdown fileSelector;       //選択されたファイルの名前;

    public Text statusMessage;

    static string Apath = null;
    static string Wpath = null;

    public GameObject landmarkPrefab;  //コピー対象;

    //   static string loadpath = null;     //ファイル名の変更に使用する予定
    //   ARWorldMappingStatus mappingStatus;
    //   public GameObject mapInstantiate;
    //   public Material mapMaterial;
    //   public Toggle visual;
    //   public Text mapindicator;
    //   public Text ARCameraStatusIndicator;
    //   public Text saveStatusIndicator;


    [Tooltip("A UI button component which will generate an ARWorldMap and save it to disk.")]
    [SerializeField]
    Button m_SaveButton;

    /// <summary>
    /// A UI button component which will generate an ARWorldMap and save it to disk.
    /// </summary>
    public Button SaveButton
    {
        get { return m_SaveButton; }
        set { m_SaveButton = value; }
    }

    //既に存在するファイルを読み込み
    void Start()
    {
        UpdateDropdown();
        fileSelector.value = PlayerPrefs.GetInt("MapName");
    }


    /// <summary>
    /// Create an <c>ARWorldMap</c> and save it to disk.
    /// </summary>
    public void OnSaveButton()
    {

        if (fileName.text == "") //file name 
        {
            fileName.placeholder.GetComponent<Text>().color = Color.red;
            fileName.placeholder.GetComponent<Text>().text = "!!!Please input the filename";
        }
        else
        {
            fileName.placeholder.GetComponent<Text>().color = Color.grey;
            fileName.placeholder.GetComponent<Text>().text = "Filename";
#if UNITY_EDITOR
            Apath = Application.dataPath + "/" + fileName.text + ".ARMAP";
            Wpath = Path.Combine(Application.dataPath, fileName.text + ".RoWMap");
#elif UNITY_IPHONE
            Apath = Application.persistentDataPath + "/" + fileName.text + ".ARMAP";
            Wpath = Path.Combine(Application.persistentDataPath, fileName.text+".RoWMap");
#endif

            SaveMap();

#if UNITY_IOS
            StartCoroutine(Save());
#endif
        }
    }


    //李さんのプログラムの一部を使用．目印となるオブジェクトの座標をXMLファイルに保存
    public void SaveMap()
    {
        XmlDocument xmlMap = new XmlDocument();
        XmlElement root = xmlMap.CreateElement("gameObjects");
        XmlElement landMarks = xmlMap.CreateElement("landMarks");
        XmlElement maps = xmlMap.CreateElement("maps");
        GameObject mapHolder = GameObject.FindGameObjectWithTag("MapGameObject");
        Transform[] mapMeshs = mapHolder.GetComponentsInChildren<Transform>();

        if(mapMeshs.Length > 1)
        {
            for(int i = 1; i < mapMeshs.Length; i++)
            {
                XmlElement map = xmlMap.CreateElement("map" + i.ToString());

                XmlElement position = xmlMap.CreateElement("position");
                XmlElement position_x = xmlMap.CreateElement("x");
                position_x.InnerText = mapMeshs[i].transform.position.x + "";
                XmlElement position_y = xmlMap.CreateElement("y");
                position_y.InnerText = mapMeshs[i].transform.position.y + "";
                XmlElement position_z = xmlMap.CreateElement("z");
                position_z.InnerText = mapMeshs[i].transform.position.z + "";
                position.AppendChild(position_x);
                position.AppendChild(position_y);
                position.AppendChild(position_z);

                XmlElement rotation = xmlMap.CreateElement("rotation");
                XmlElement rotation_x = xmlMap.CreateElement("x");
                rotation_x.InnerText = mapMeshs[i].transform.rotation.eulerAngles.x + "";
                XmlElement rotation_y = xmlMap.CreateElement("y");
                rotation_y.InnerText = mapMeshs[i].transform.rotation.eulerAngles.y + "";
                XmlElement rotation_z = xmlMap.CreateElement("z");
                rotation_z.InnerText = mapMeshs[i].transform.rotation.eulerAngles.z + "";
                rotation.AppendChild(rotation_x);
                rotation.AppendChild(rotation_y);
                rotation.AppendChild(rotation_z);

                XmlElement scale = xmlMap.CreateElement("scale");
                XmlElement scale_x = xmlMap.CreateElement("x");
                scale_x.InnerText = mapMeshs[i].transform.localScale.x + "";
                XmlElement scale_y = xmlMap.CreateElement("y");
                scale_y.InnerText = mapMeshs[i].transform.localScale.y + "";
                XmlElement scale_z = xmlMap.CreateElement("z");
                scale_z.InnerText = mapMeshs[i].transform.localScale.z + "";
                scale.AppendChild(scale_x);
                scale.AppendChild(scale_y);
                scale.AppendChild(scale_z);

                Mesh mapMeshFilter = mapMeshs[i].GetComponent<MeshFilter>().mesh;
                Vector3[] vertics = mapMeshFilter.vertices;
                int[] triangles = mapMeshFilter.triangles;
                XmlElement meshdata = xmlMap.CreateElement("MeshData");

                XmlElement meshVertics = xmlMap.CreateElement("Vertics");
                for(int iVertics = 0; iVertics <vertics.Length; iVertics++)
                {
                    XmlElement meshVer = xmlMap.CreateElement("Vertex" + iVertics.ToString());
                    XmlElement vertics_x = xmlMap.CreateElement("Vt_x");
                    vertics_x.InnerText = vertics[iVertics].x.ToString();
                    XmlElement vertics_y = xmlMap.CreateElement("Vt_y");
                    vertics_y.InnerText = vertics[iVertics].y.ToString();
                    XmlElement vertics_z = xmlMap.CreateElement("Vt_z");
                    vertics_z.InnerText = vertics[iVertics].z.ToString();
                    meshVer.AppendChild(vertics_x);
                    meshVer.AppendChild(vertics_y);
                    meshVer.AppendChild(vertics_z);

                    meshVertics.AppendChild(meshVer);
                }

                XmlElement meshTriangles = xmlMap.CreateElement("Triangles");
                for(int iTriangle = 0; iTriangle < triangles.Length; iTriangle++)
                {
                    XmlElement idx = xmlMap.CreateElement("Index" + iTriangle.ToString());
                    XmlElement num = xmlMap.CreateElement("Num");
                    num.InnerText = triangles[iTriangle].ToString();
                    idx.AppendChild(num);

                    meshTriangles.AppendChild(idx);
                }

                meshdata.AppendChild(meshVertics);
                meshdata.AppendChild(meshTriangles);

                map.AppendChild(position);
                map.AppendChild(rotation);
                map.AppendChild(scale);
                map.AppendChild(meshdata);

                maps.AppendChild(map);

            }
        }
        foreach (var item in GameObject.FindGameObjectsWithTag("LocationMark"))
        {
            //XmlElement TransformXml = xmlMap.CreateElement(item.name);
            XmlElement TransformXml = xmlMap.CreateElement("FloorObject");      ///////////////////////////////改良の余地あり
            XmlElement position = xmlMap.CreateElement("position");
            XmlElement position_x = xmlMap.CreateElement("x");
            position_x.InnerText = item.transform.position.x + "";     
            XmlElement position_y = xmlMap.CreateElement("y");
            position_y.InnerText = item.transform.position.y + "";
            XmlElement position_z = xmlMap.CreateElement("z");
            position_z.InnerText = item.transform.position.z + "";
            position.AppendChild(position_x);
            position.AppendChild(position_y);
            position.AppendChild(position_z);
            
            XmlElement rotation = xmlMap.CreateElement("rotation");
            XmlElement rotation_x = xmlMap.CreateElement("x");
            rotation_x.InnerText = item.transform.rotation.eulerAngles.x + "";
            XmlElement rotation_y = xmlMap.CreateElement("y");
            rotation_y.InnerText = item.transform.rotation.eulerAngles.y + "";
            XmlElement rotation_z = xmlMap.CreateElement("z");
            rotation_z.InnerText = item.transform.rotation.eulerAngles.z + "";
            rotation.AppendChild(rotation_x);
            rotation.AppendChild(rotation_y);
            rotation.AppendChild(rotation_z);

            XmlElement scale = xmlMap.CreateElement("scale");
            XmlElement scale_x = xmlMap.CreateElement("x");
            scale_x.InnerText = item.transform.localScale.x + "";
            XmlElement scale_y = xmlMap.CreateElement("y");
            scale_y.InnerText = item.transform.localScale.y + "";
            XmlElement scale_z = xmlMap.CreateElement("z");
            scale_z.InnerText = item.transform.localScale.z + "";
            scale.AppendChild(scale_x);
            scale.AppendChild(scale_y);
            scale.AppendChild(scale_z);


            /*
            XmlElement dataContainer = xmlMap.CreateElement("dataContainer");
            XmlElement[] t = new XmlElement[7];
            int iz = 0;
            foreach (var iitem in item.GetComponentsInChildren<TextMesh>())
            {
                t[iz] = xmlMap.CreateElement(iitem.name.Replace(" ", "").Replace("(", "").Replace(")", ""));
                t[iz].InnerText = iitem.text;
                iz++;

            }
            dataContainer.AppendChild(t[0]);

            dataContainer.AppendChild(t[1]);
            dataContainer.AppendChild(t[2]);
            dataContainer.AppendChild(t[3]);
            dataContainer.AppendChild(t[4]);
            dataContainer.AppendChild(t[5]);
            dataContainer.AppendChild(t[6]);
            */

            TransformXml.AppendChild(position);
            TransformXml.AppendChild(rotation);
            TransformXml.AppendChild(scale);
            //TransformXml.AppendChild(dataContainer);

            landMarks.AppendChild(TransformXml);
        }

        root.AppendChild(landMarks);
        root.AppendChild(maps);
        xmlMap.AppendChild(root);

        if (File.Exists(Apath))
        {
            File.Delete(Apath);
        }
        xmlMap.Save(Apath);

        fileName.text = null;
    }



    //ファイルパスに環境マップ（平面検知の情報）を保存（基本的にGitのコードのまま)
#if UNITY_IOS
    IEnumerator Save()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            yield break;
        }

        var request = sessionSubsystem.GetARWorldMapAsync();

        while (!request.status.IsDone())
            yield return null;

        if (request.status.IsError())
        {
            yield break;
        }


        var worldMap = request.GetWorldMap();
        request.Dispose();

        SaveAndDisposeWorldMap(worldMap);

        UpdateDropdown();
    }

    void SaveAndDisposeWorldMap(ARWorldMap worldMap)
    {

        var data = worldMap.Serialize(Allocator.Temp);
        //Debug.Log(string.Format("ARWorldMap has {0} bytes.", data.Length));

        var file = File.Open(Wpath, FileMode.Create);
        var writer = new BinaryWriter(file);
        writer.Write(data.ToArray());
        writer.Close();
        data.Dispose();
        worldMap.Dispose();
        statusMessage.enabled = true;
        statusMessage.text = "Save完了";          //確認用
        //Debug.Log(string.Format("ARWorldMap written to {0}", Wpath));

    }
#endif



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

    public void MapDelete()
    {
#if UNITY_EDITOR
        string pathstr = Application.dataPath + "/" + fileSelector.captionText.text;
#elif UNITY_IPHONE
        string pathstr = Application.persistentDataPath + "/" + fileSelector.captionText.text;
#endif 
        if (File.Exists(pathstr))
        {
            File.Delete(pathstr);
            statusMessage.text = "Map Delete";
        }
        UpdateDropdown();
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