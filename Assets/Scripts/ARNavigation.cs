using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


/// <summary>
/// ユーザの経路案内プログラム
/// </summary>
public class ARNavigation : MonoBehaviour
{
    public Text statusMessage;

    [SerializeField]
    private LineRenderer line = null;

    private GameObject agent;
    private GameObject[] maps;
    private GameObject target;
    private NavMeshAgent navAgent;
    private NavMeshPath path;

    public void NavigationButton()
    {
        agent = GameObject.FindWithTag("MainCamera");

        target = GameObject.FindGameObjectWithTag("LocationMark");

        if (target)
        {
            maps = GameObject.FindGameObjectsWithTag("MapGameObject");
            foreach (var map in maps)
            {
                map.GetComponent<NavMeshSurface>().BuildNavMesh();
            }

            agent.AddComponent<NavMeshAgent>();
            agent.GetComponent<LineRenderer>().enabled = true;
            navAgent = agent.GetComponent<NavMeshAgent>();


            navAgent.radius = 0.2f;
            if (target != null)
            {
                navAgent.SetDestination(target.transform.position);
            }
            navAgent.speed = 0.0f;

            path = new NavMeshPath();
            navAgent.CalculatePath(target.transform.position, path);

            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
            statusMessage.text = "Navi";
        }
        else
        {
            statusMessage.text = "Navi失敗";
        }

    }
}
