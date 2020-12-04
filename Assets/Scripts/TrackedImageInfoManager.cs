using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// ユーザの画像認識プログラム
    ///ImageTracking(画像追跡)を画像認識に用いる
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab has a world space UI canvas,
        /// which requires a camera to function properly.
        /// </summary>
        [SerializeField]
        [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
        Camera m_WorldSpaceCanvasCamera;
        public Camera worldSpaceCanvasCamera
        {
            get { return m_WorldSpaceCanvasCamera; }
            set { m_WorldSpaceCanvasCamera = value; }
        }

        /// <summary>
        /// If an image is detected but no source texture can be found,
        /// this texture is used instead.
        /// </summary>
        [SerializeField]
        [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
        Texture2D m_DefaultTexture;
        public Texture2D defaultTexture
        {
            get { return m_DefaultTexture; }
            set { m_DefaultTexture = value; }
        }
        
        private string filename;                //UFileIO.cs内でファイル名を取得する際に使用
        public string Filename
        {
            get { return this.filename; }
            private set { this.filename = value; }
        }

        public Text statusMessage;              //状態をテキストで表示
        public GameObject FileController;       //UFileIO.csをアタッチしているゲームオブジェクト

        ARTrackedImageManager m_TrackedImageManager;        //画像追跡を行うクラス

        void Awake()
        {
            statusMessage.enabled = false;
            FileController.GetComponent<UFileIO>().enabled = false;
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            m_WorldSpaceCanvasCamera.GetComponent<LineRenderer>().enabled = false;
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            //画像を認識した際に実行
            if (trackedImage.trackingState != TrackingState.None)
            {
                statusMessage.enabled = true;
                statusMessage.text = "Recognize Image";

                //認識した画像の名前を文字列に
                statusMessage.text = "Start to load Map";
                filename = trackedImage.referenceImage.name;
                FileController.GetComponent<UFileIO>().enabled = true;         //UFileIO.csのプログラム起動
            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
                UpdateInfo(trackedImage);
        }
    }
}