using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NavMeshExtension;


/// <summary>
/// 管理者の平面作成および目印作成プログラム
/// </summary>
public class Create : MonoBehaviour
{
    private GameObject mesh;
    private static NavMeshObject navmesh;
    private NavMeshManager mapmesh;
    public GameObject mapInstantiate;
    private GameObject NewNavMesh;

    public GameObject particle;

    private static Vector3 yValue;
    List<GameObject> particles = new List<GameObject>();
//    private GameObject showdrag;

    [SerializeField]
    private Vector3[] allPoints;
    private List<int> selected = new List<int>();

    private static bool placing = false;
//    private static bool drag = false;

    private static Vector3 touchPosition;

    private int dragIndex;

    private ARRaycastManager raycastManager;
    List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
    public GameObject Landmark;
    private GameObject landmarkObject;

    public Text statusMessage;


    private void Start()
    {
        statusMessage.enabled = false;
        yValue = Vector3.zero;
        raycastManager = GetComponent<ARRaycastManager>();
        mesh = new GameObject("mesh");
        navmesh = mesh.AddComponent<NavMeshObject>();
        mapmesh = mesh.AddComponent<NavMeshManager>();
    }

    private void Update()
    {
        if((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                return;
            }
            else
            {
                allPoints = ConvertAllPoints();

                var touch = Input.GetTouch(0);


                if (raycastManager.Raycast(touch.position, hitResults, TrackableType.Planes))
                {
                    touchPosition = hitResults[0].pose.position;
                    if (yValue == Vector3.zero)
                    {
                        yValue = touchPosition;
                    }
                    touchPosition.y = yValue.y;


                    dragIndex = FindClosest();


                    if (placing)
                    {
                        particles.Add(Instantiate(particle));
                        particles[particles.Count - 1].transform.position = touchPosition;

                        int currentCount = navmesh.current.Count;

                        if (navmesh.current.Contains(dragIndex))
                        {
                            navmesh.AddPoint(navmesh.transform.TransformPoint(navmesh.list[dragIndex]));
                        }
                        else if (dragIndex >= 0)
                        {
                            navmesh.AddPoint(dragIndex);
                        }
                        else
                        {
                            navmesh.AddPoint(touchPosition);
                        }
                        navmesh.CreateMesh();
                    }
                    if (!placing)
                    {
                        if (landmarkObject)
                        {
                            landmarkObject.transform.position = touchPosition;
                        }
                        else
                        {
                            landmarkObject = Instantiate(Landmark, touchPosition, Quaternion.identity);
                        }
                    }
                    /*
                    if (!placing)
                    {
                        if (drag)
                        {
                            if (dragIndex != -1)
                            {
                                // 同じ点をタッチすると点の選択を削除する
                                if (selected.Contains(dragIndex))
                                {
                                    Debug.Log("Remove selected dragIndex");
                                    selected.Remove(dragIndex);
                                    Destroy(showdrag);
                                    Debug.Log("Removed");
                                }
                                // まだ選択されてない場合、新しい選択した点とをselectedに追加する
                                //１個すでにされた場合、点を削除し、２回目位選択された点を書き換える
                                else if (selected.Count == 0 || selected.Count == 1)
                                {
                                    selected.Clear();
                                    selected.Count();
                                    selected.Add(dragIndex);
                                    showdrag = Instantiate(particle);
                                    //Instantiate(particle).transform.parent = showdrag.transform;
                                    showdrag.transform.position = navmesh.transform.TransformPoint(navmesh.list[dragIndex]);
                                    Debug.Log(touchPosition);
                                    Debug.Log(navmesh.transform.TransformPoint(navmesh.list[dragIndex]));
                                    foreach (var item in navmesh.list)
                                    {
                                        Debug.Log(item);
                                    }


                                }
                            }
                    
                            else
                            {
                                //selected内に点がある場合、点の位置を移動する
                                if (selected.Count == 1)
                                {
                                    Debug.Log("Point Reset.");
                                    //navmesh.GetComponent<MeshFilter>().mesh.vertices[selected[0]] = navmesh.transform.InverseTransformPoint(touchPosition);

                                    navmesh.list[selected[0]] = navmesh.transform.InverseTransformPoint(touchPosition);
                                    navmesh.UpdateMesh(ConvertAllPoints());
                                    showdrag.transform.position = touchPosition;
                                    Debug.Log("Point Reset successed.");
                                    foreach (var item in navmesh.list)
                                    {
                                        Debug.Log(item);
                                    }
                                }
                            }
                            navmesh.CreateMesh();
                        }
                    }
                    */
                }
            }
        }
    }


    
    public void EditMode()
    {
        if (!placing)
        {
            EnterEditMode();
            statusMessage.text = "Mesh";
        }
        else
        {
            LeaveEditMode();
            statusMessage.text = "LandMark";
        }
    }
    


    public void MeshCreate()
    {
        particles.Clear();
        mapmesh = (GameObject.FindGameObjectWithTag("MapGameObject") == null) ?
        Instantiate(mapInstantiate).GetComponent<NavMeshManager>() : GameObject.FindGameObjectWithTag("MapGameObject").GetComponent<NavMeshManager>();
        NewNavMesh = new GameObject("New NavMesh");
        //navmesh = navGO.GetComponent<NavMeshObject>();
        //navGO.transform.parent = mapmesh.transform;
        NewNavMesh.transform.parent = mapmesh.transform;
        NewNavMesh.AddComponent<NavMeshObject>();
        NewNavMesh.AddComponent<NavMeshModifier>();
        NewNavMesh.AddComponent<MeshFilter>();
        NewNavMesh.AddComponent<MeshRenderer>();
        NewNavMesh.tag = "mapNavMesh";
        navmesh = NewNavMesh.GetComponent<NavMeshObject>();
        //navGO.isStatic = true;
        //modify renderer to ignore shadows
        MeshRenderer mRenderer = NewNavMesh.GetComponent<MeshRenderer>();

        mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mRenderer.receiveShadows = false;
        if (mapmesh.meshMaterial)
        {
            mapmesh.meshMaterial.color = new Color(0.0f, 0.0f, 1.0f, 0.5f);
            mRenderer.sharedMaterial = mapmesh.meshMaterial;
            statusMessage.enabled = true;
            statusMessage.text = "Create";
        }
        else
        {
            mRenderer.enabled = false;
        }
    }

    
    private void EnterEditMode()
    {
        //if possible, create new submesh
        GameObject newObj = NewNavMesh.GetComponent<NavMeshObject>().CreateSubMesh();
        placing = true;
    }




    private void LeaveEditMode()
    {
        //if possible, combine submeshes
        CheckCombine();

        //get all mesh filters
        MeshFilter[] meshFilters = navmesh.GetComponentsInChildren<MeshFilter>();
        //let the navmesh combine them   
        navmesh.Combine();
        for (int i = 1; i < meshFilters.Length; i++)
        {
            Destroy(meshFilters[i].gameObject);
        }
        placing = false;
        GameObject[] ptcontainer = GameObject.FindGameObjectsWithTag("PT");
        foreach (var item in ptcontainer)
        {
            Destroy(item);
        }
    }




    private void CheckCombine()
    {

        //get count of all submeshes
        int subPointsCount = navmesh.subPoints.Count;

        if (navmesh.subPoints.Count == 0) return;
        NavMeshObject.SubPoints lastPoints = navmesh.subPoints[subPointsCount - 1];

        if (lastPoints.list.Count <= 2)
        {
            selected.Clear();
            for (int i = 0; i < lastPoints.list.Count; i++)
            {
                selected.Add(lastPoints.list[i]);
            }
            DeleteSelected();
        }
    }


    private void DeleteSelected(bool auto = false)
    {
        //get mesh references
        MeshFilter filter = navmesh.GetComponent<MeshFilter>();
        List<Vector3> vertices = null;
        if (filter.sharedMesh != null)
            vertices = new List<Vector3>(filter.sharedMesh.vertices);
        //filter selected list for unique entries
        selected = selected.Distinct().ToList();
        selected = selected.OrderByDescending(x => x).ToList();

        //loop over selected vertex indices
        for (int i = 0; i < selected.Count; i++)
        {
            //remove index from mesh vertices
            int index = selected[i];
            if (vertices != null)
            {
                if (vertices.Contains(allPoints[index])) vertices.Remove(allPoints[index]);
                else try { vertices.RemoveAt(index); } catch { };
            }
            navmesh.list.RemoveAt(index);

            //loop over submeshes and remove it there too
            for (int j = 0; j < navmesh.subPoints.Count; j++)
            {
                navmesh.subPoints[j].list.Remove(index);
                //decrease higher entries, as the array is smaller now
                for (int k = 0; k < navmesh.subPoints[j].list.Count; k++)
                {
                    if (navmesh.subPoints[j].list[k] >= index)
                        navmesh.subPoints[j].list[k] -= 1;
                }
            }
        }

        //clear selection
        selected.Clear();


        //loop over submeshes to remove obsolete indices,
        //e.g. if a submesh has only 2 vertices after removal
        for (int i = navmesh.subPoints.Count - 1; i >= 0; i--)
        {
            //check for vertex count
            if (navmesh.subPoints[i].list.Count <= 2)
            {
                //construct a combined list with all indices
                List<int> allIndices = new List<int>();
                for (int j = 0; j < navmesh.subPoints.Count; j++)
                    allIndices.AddRange(navmesh.subPoints[j].list);


                //check whether an index occurs more than once
                List<int> duplicates = allIndices.GroupBy(x => x)
                                       .Where(x => x.Count() > 1)
                                       .Select(x => x.Key)
                                       .ToList();

                //if an index in this submesh is not being used in other
                //submeshes anymore, this means that we can remove it too
                for (int j = 0; j < navmesh.subPoints[i].list.Count; j++)
                    if (!duplicates.Contains(navmesh.subPoints[i].list[j]))
                        selected.Add(navmesh.subPoints[i].list[j]);

                //delete this submesh entry
                navmesh.subPoints.RemoveAt(i);
            }
        }

        //recalculate triangles for complete mesh
        if (filter.sharedMesh == null) return;
        List<int> triangles = new List<int>();
        for (int i = 0; i < navmesh.subPoints.Count; i++)
            triangles.AddRange(navmesh.RecalculateTriangles(navmesh.subPoints[i].list));

        //assign triangles and update vertices
        filter.sharedMesh.triangles = triangles.ToArray();
        navmesh.list = vertices;
        navmesh.UpdateMesh(ConvertAllPoints());

        //recursively delete the remaining obsolete indices
        //which were found by looking through all submeshes
        if (selected.Count > 0)
        {
            DeleteSelected();
            return;
        }

        //deletion done - optimize mesh
        OptimizeMesh(filter.sharedMesh);
    }
    //rebuild mesh properties
    private void OptimizeMesh(Mesh Opmesh)
    {
        Opmesh.RecalculateNormals();
        Opmesh.RecalculateBounds();
    }
    

    //convert relative vertex positions to world positions
    private Vector3[] ConvertAllPoints()
    {
        int count = navmesh.list.Count;
        List<Vector3> all = new List<Vector3>();
        for (int i = 0; i < count; i++)
            all.Add(navmesh.transform.TransformPoint(navmesh.list[i]));

        return all.ToArray();
    }



    /// <summary>
    /// Finds the closest.
    /// </summary>
    /// <returns>The closest.</returns>
    private int FindClosest()
    {
        //initialize variables 
        List<int> closest = new List<int>();
        int near = -1;

        // loop over vertices to find the nearest
        for (int i = 0; i < allPoints.Length; i++)
        {
            Vector2 screenPoint = Camera.current.WorldToScreenPoint(allPoints[i]);
            if (Vector2.Distance(screenPoint, Input.GetTouch(0).position) < 100)
                closest.Add(i);
        }

        //don't do further calculation in some cases
        if (closest.Count == 0)
            return near;
        else if (closest.Count == 1)
            return closest[0];
        else
        {
            //there are more than a few vertices near the touch position,
            //here only the closest vertex to the camera should matter
            Vector3 camPos = Camera.current.transform.position;
            float nearDist = float.MaxValue;

            // loop over all vertices and get the one near the camera 
            for (int i = 0; i < closest.Count; i++)
            {
                float dist = Vector3.Distance(allPoints[i], camPos);
                if (dist < nearDist)
                {
                    nearDist = dist;
                    near = closest[i];

                }

            }
        }
        closest.Clear();
        return near;
    }

}








//-------------------------------------------------------------------------------------------------------

/*using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using TriangleNet.Smoothing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPointCloud))]
public class CreateMesh : MonoBehaviour
{
    public Mesh mesh { get; private set; }
    public ParticleSystem pointCloudParticlePrefab;
    public GameObject particle;
    public int maxPointsToShow;
    public float particleSize = 1.0f;
    public Transform chunk;

    private ARPointCloud m_PointCloud;

    private bool frameUpdated;
    private Polygon polygon;
    private List<Vector3> mapVertices;
    private List<Vector3> s_Vertices = new List<Vector3>();
    ParticleSystem currentPS;
    ParticleSystem.Particle[] particles;
    private TriangleNet.Mesh Trianglemesh = null;

    public float yValue;
    public int trianglesInChunk = 20000;
    void Awake()
    {
        polygon = new Polygon();
        mapVertices = new List<Vector3>();

        mesh = new Mesh();
        m_PointCloud = GetComponent<ARPointCloud>();
        currentPS = Instantiate(pointCloudParticlePrefab);
        frameUpdated = false;
    }

    void OnEnable()
    {
        m_PointCloud.updated += OnPointCloudChanged;
        frameUpdated = true;
    }

    void OnDisable()
    {
        m_PointCloud.updated -= OnPointCloudChanged;
        frameUpdated = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Stationary))
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                return;
            }
            else
            {
                if (frameUpdated)
                {
                    yValue = 0.0f;
                    polygon = new Polygon();
                    if ((s_Vertices != null) && (s_Vertices.Count > 0) && (maxPointsToShow > 0))
                    {
                        int numParticles = Mathf.Min(s_Vertices.Count, maxPointsToShow);
                        particles = new ParticleSystem.Particle[numParticles];
                        int index = 0;
                        foreach (Vector3 currentPoint in s_Vertices)
                        {
                            particles[index].position = currentPoint;
                            particles[index].startColor = new Color(1.0f, 1.0f, 1.0f);
                            particles[index].startSize = particleSize;

                            RaycastHit hit;
                            Ray meshdetector = new Ray(currentPoint, Vector3.down);

                            if (Physics.Raycast(meshdetector, out hit, 3))
                            {

                            }
                            else
                            {
                                mapVertices.Add(currentPoint);
                                yValue += currentPoint.y;
                                index++;
                            }

                            if (index >= numParticles) break;
                        }

                        foreach (var item in mapVertices)
                        {
                            polygon.Add(new Vertex(item.x, item.z));
                        }

                        yValue = yValue / index;

                        currentPS.SetParticles(particles, numParticles);

                        TriangleNet.Meshing.ConstraintOptions options =
                                            new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };

                        Trianglemesh = (TriangleNet.Mesh)polygon.Triangulate(options);
                        MakeMesh();
                    }
                    else
                    {
                        particles = new ParticleSystem.Particle[1];
                        particles[0].startSize = 0.0f;
                        currentPS.SetParticles(particles, 1);
                    }
                    frameUpdated = false;
                }
            }
        }
    }

    void OnPointCloudChanged(ARPointCloudUpdatedEventArgs eventArgs)
    {
        s_Vertices.Clear();
        foreach (var point in m_PointCloud.positions)
            s_Vertices.Add(point);

        mesh.Clear();
        mesh.SetVertices(s_Vertices);

    }

    public void MakeMesh()
    {
        IEnumerator<Triangle> triangleEnumerator = Trianglemesh.Triangles.GetEnumerator();

        for(int chunkStart = 0; chunkStart < Trianglemesh.Triangles.Count; chunkStart += trianglesInChunk)
        {
            List<Vector3> vertices = new List<Vector3>();

            List<Vector3> normals = new List<Vector3>();

            List<Vector2> uvs = new List<Vector2>();

            List<int> triangles = new List<int>();

            int chunkEnd = chunkStart + trianglesInChunk;
            for (int i = chunkStart; i<chunkEnd; i++)
            {
                if (!triangleEnumerator.MoveNext())
                {
                    break;
                }

                Triangle triangle = triangleEnumerator.Current;

                Vector3 v0 = new Vector3((float)triangle.vertices[2].x, yValue, (float)triangle.vertices[2].y);
                Vector3 v1 = new Vector3((float)triangle.vertices[1].x, yValue, (float)triangle.vertices[1].y);
                Vector3 v2 = new Vector3((float)triangle.vertices[0].x, yValue, (float)triangle.vertices[0].y);

                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
            }

            Mesh chunkMesh = new Mesh();
            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            chunkMesh.normals = normals.ToArray();

            chunk.GetComponent<MeshFilter>().mesh = chunkMesh;
            chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            chunk.transform.parent = transform;
        }
    }
}
*/
