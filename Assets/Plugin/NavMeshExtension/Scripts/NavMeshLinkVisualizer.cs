/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace NavMeshExtension
{
    /// <summary>
    /// 
    /// <summary>
	[RequireComponent(typeof(NavMeshLink))]
    public class NavMeshLinkVisualizer : MonoBehaviour
    {	
		/// <summary>
		/// The visual object.
		/// </summary>
		public Transform[] visualObj;

		[HideInInspector]
		public NavMeshLink navLink;

		
		void Start()
		{
			navLink = GetComponent<NavMeshLink>();
			UpdatePositions();
		}


		void Update()
		{
			if(navLink.autoUpdate) 
			{
				UpdatePositions();
			}
		}

		
		public void UpdatePositions()
		{
			if(visualObj == null || visualObj.Length != 2) return;
			if(visualObj[0] != null) visualObj[0].localPosition = navLink.startPoint;
			if(visualObj[1] != null) visualObj[1].localPosition = navLink.endPoint;
		}
    }
}
