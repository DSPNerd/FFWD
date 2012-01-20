﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PressPlay.FFWD.Components
{
    public class Camera : Component, IComparer<Camera>, IComparer<Renderer>
    {
        public Camera()
        {
            fieldOfView = MathHelper.ToRadians(60);
            nearClipPlane = 0.3f;
            farClipPlane = 1000;
        }

        public enum ClearFlags
        {
            Skybox,
            Color,
            Depth,
            Nothing
        }

        public float fieldOfView { get; set; }
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public float orthographicSize { get; set; }
        public bool orthographic { get; set; }
        public int depth { get; set; }
        public float aspect { get; set; }
        public int cullingMask { get; set; }

        public int pixelWidth 
        { 
            get
            {
                return viewPort.Width;
            }
        }
        public int pixelHeight
        { 
            get
            {
                return viewPort.Height;
            }
        }

        [ContentSerializerIgnore]
        public BoundingFrustum frustum { get; private set; }

        public static bool wireframeRender = false;

        private static int estimatedDrawCalls = 0;

        private static DynamicBatchRenderer dynamicBatchRenderer;

        internal static BasicEffect basicEffect;
        [ContentSerializerIgnore]
        public BasicEffect BasicEffect
        {
            get
            {
                return basicEffect;
            }
        }

        private Color _backgroundColor = Color.black;
        public Color backgroundColor
        { 
            get 
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = new Color(value.r, value.g, value.b, 1);
            }
        }

        public Rect rect { get; set; } //public Rectangle rect { get; set; }
        public ClearFlags clearFlags { get; set; }

        public override void Awake()
        {
            frustum = new BoundingFrustum(view * projectionMatrix);
            RecalculateView();
            for (int i = nonAssignedRenderers.Count - 1; i >= 0; i--)
            {
                if (nonAssignedRenderers[i] == null || nonAssignedRenderers[i].gameObject == null || addRenderer(nonAssignedRenderers[i]))
                {
                    nonAssignedRenderers.RemoveAt(i);
                }
            }
            for (int i = 0; i < _allCameras.Count; i++)
            {
                for (int j = 0; j < _allCameras[i].renderQueue.Count; j++)
                {
                    addRenderer(_allCameras[i].renderQueue[j]);
                }
            }
            _allCameras.Add(this);
            _allCameras.Sort(this);

            if (gameObject.CompareTag("MainCamera") && (main == null))
            {
                main = this;
            }
        }

        public static void RemoveCamera(Camera cam)
        {
            _allCameras.Remove(cam);
            if (cam == main)
            {
                main = null;
                if (_allCameras.Count > 0)
                {
                    main = _allCameras[0];
                }
            }
        }

        private static List<Camera> _allCameras = new List<Camera>();
        public static IEnumerable<Camera> allCameras
        {
            get
            {
                return _allCameras;
            }
        }

        public static Camera main { get; private set; }
        public static Camera mainCamera { get { return main; } }

        public static Viewport FullScreen;

        public Matrix view { get; private set; }

        [ContentSerializerIgnore]
        public Viewport viewPort 
        { 
            get
            {
                return FullScreen;
            }
        }

        private Matrix _projectionMatrix = Matrix.Identity;
        [ContentSerializerIgnore]
        public Matrix projectionMatrix
        {
            get
            {
                if (_projectionMatrix == Matrix.Identity)
                {
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), FullScreen.AspectRatio, Mathf.Max(ApplicationSettings.DefaultValues.minimumNearClipPlane, nearClipPlane), farClipPlane, out _projectionMatrix);
                }
                return _projectionMatrix;
            }
        }

        public Ray ScreenPointToRay(Vector2 screen)
        {
            Vector3 near = viewPort.Unproject(new Vector3(screen.x, viewPort.Height - screen.y, 0), projectionMatrix, view, Matrix.Identity);
            Vector3 far = viewPort.Unproject(new Vector3(screen.x, viewPort.Height - screen.y, 1), projectionMatrix, view, Matrix.Identity);
            return new Ray(near, (far - near).normalized);
        }

        public Vector3 ScreenToWorldPoint(Vector3 vector3)
        {
            float normZ = (vector3.z - nearClipPlane) / (farClipPlane - nearClipPlane);
            vector3.z = normZ;
            vector3.y = pixelHeight - vector3.y;
            return viewPort.Unproject(vector3, projectionMatrix, view, Matrix.Identity);
        }

        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            Vector3 v = viewPort.Project(position, projectionMatrix, view, Matrix.Identity);
            v.y = pixelHeight - v.y;
            return v;
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            // TODO: If the viewport of the camera is not the same as the screen, this will give an issue.
            // I am not sue if it can happen at all at the moment...
            Vector3 v = viewPort.Project(position, projectionMatrix, view, Matrix.Identity);
            v.y = pixelHeight - v.y;
            return v;
        }

        public Vector3 ScreenToViewportPoint(Vector3 v)
        {
            Vector2 pt = v;
            pt.x /= viewPort.Width;
            pt.y /= viewPort.Height;
            return new Vector3(pt.x, pt.y, (float)v);
        }

        public Vector3 ViewportToScreenPoint(Vector3 v)
        {
            return new Vector3(v.x * viewPort.Width, v.y * viewPort.Height, v.z);
        }

        #region Keeping track of renderers
        internal static List<Renderer> nonAssignedRenderers = new List<Renderer>();
        internal static void AddRenderer(Renderer renderer)
        {
            bool isAdded = false;

            if (renderer.isPartOfStaticBatch)
            {
                return;
            }

            for (int i = 0; i < _allCameras.Count; i++)
            {
                isAdded |= _allCameras[i].addRenderer(renderer);
            }
            if (!isAdded && !nonAssignedRenderers.Contains(renderer))
            {
                nonAssignedRenderers.Add(renderer);
            }
        }

        private readonly List<Renderer> renderQueue = new List<Renderer>(50);

        private bool addRenderer(Renderer renderer)
        {
            if (renderQueue.Contains(renderer))
            {
                return true;
            }
            if ((cullingMask & (1 << renderer.gameObject.layer)) > 0)
            {
                int index = renderQueue.BinarySearch(renderer, this);
                if (index < 0)
                {
                    renderQueue.Insert(~index, renderer);
                }
                else
                {
                    renderQueue.Insert(index, renderer);
                }
                return true;
            }
            return false;
        }

        internal static void RemoveRenderer(Renderer renderer)
        {
            for (int i = 0; i < _allCameras.Count; i++)
            {
                _allCameras[i].removeRenderer(renderer);
            }
        }

        private void removeRenderer(Renderer renderer)
        {
            renderQueue.Remove(renderer);
        }
        #endregion

#if DEBUG
        internal static bool logRenderCalls = false;
#endif

        internal static void DoRender(GraphicsDevice device)
        {
#if DEBUG && WINDOWS
            if (Input.GetMouseButtonUp(1))
            {
                wireframeRender = !wireframeRender;
            }
            if (Input.GetMouseButtonUp(2))
            {
                logRenderCalls = true;
                Debug.Log("----------- Render log begin ---------------", Time.realtimeSinceStartup);
            }
#endif
            if (dynamicBatchRenderer == null)
            {
                dynamicBatchRenderer = new DynamicBatchRenderer(device);
            }

            estimatedDrawCalls = 0;
            if (device == null)
            {
                return;
            }

            device.BlendState = BlendState.Opaque;
            if (wireframeRender)
            {
                RasterizerState state = new RasterizerState();
                state.FillMode = FillMode.WireFrame;
                device.RasterizerState = state;
            }
            else
            {
                device.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            for (int i = 0; i < _allCameras.Count; i++)
            {
                if (_allCameras[i].gameObject.active)
                {
                    _allCameras[i].doRender(device);
                }
            }

            GUI.StartRendering();
            GUI.RenderComponents(Application.guiComponents);
            GUI.EndRendering();

#if DEBUG
            Debug.Display("Estimated Draw calls", estimatedDrawCalls);
            logRenderCalls = false;
#endif
        }

        private readonly Matrix inverter = new Matrix(-1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        internal void doRender(GraphicsDevice device)
        {
            Clear(device);
#if DEBUG
            if (logRenderCalls)
            {
                Debug.Log("**** Camera begin : ", name, "****");
            }
#endif
            // TODO: Do not recreate view matrix every frame. Only when camera is moved.
            RecalculateView();

            #region TextRenderer3D batching start
            // We are beginning the batching of TextRenderer3D calls
            // TODO: This code is particularly ugly and should be reworked...
            if (wireframeRender)
            {
                RasterizerState state = new RasterizerState();
                state.FillMode = FillMode.WireFrame;
                state.CullMode = CullMode.None;
                TextRenderer3D.batch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.DepthRead, state, TextRenderer3D.basicEffect);
            }
            else
            {
                TextRenderer3D.batch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, TextRenderer3D.basicEffect);
            }
            #endregion

            BasicEffect.View = view;
            BasicEffect.Projection = projectionMatrix;

            if (Light.HasLights)
            {
                Light.EnableLighting(BasicEffect);
            }

            int q = 0;
            for (int i = 0; i < renderQueue.Count; i++)
            {
                if (renderQueue[i].gameObject == null)
                {
                    // This will happen if the game object has been destroyed in update.
                    // It is acceptable behaviour.
                    continue;
                }

                if (renderQueue[i].material == null)
                {
                    // We have no material, so we will skip rendering
                    continue;
                }

                if (renderQueue[i].isPartOfStaticBatch)
                {
                    // The object is statically batched, so we will skip it
                    continue;
                }

                if (renderQueue[i].material.renderQueue != q)
                {
                    if (q > 0)
                    {
                        estimatedDrawCalls += dynamicBatchRenderer.DoDraw(device, this);
                    }
                    q = renderQueue[i].material.renderQueue;
                }

                if (renderQueue[i].gameObject.active && renderQueue[i].enabled)
                {
                    estimatedDrawCalls += renderQueue[i].Draw(device, this);
                }
            }
            
            // We are ending the batching of TextRenderer3D calls
            TextRenderer3D.batch.End(); 

            estimatedDrawCalls += dynamicBatchRenderer.DoDraw(device, this);
        }

        private void RecalculateView()
        {
            Matrix m = Matrix.CreateLookAt(
                transform.position,
                transform.position + transform.forward,
                transform.up);
            view = m * inverter;
            frustum.Matrix = view * projectionMatrix;
        }

        private void Clear(GraphicsDevice device)
        {
            switch (clearFlags)
            {
                case ClearFlags.Skybox:
                    device.Clear(backgroundColor);
                    break;
                case ClearFlags.Color:
                    device.Clear(backgroundColor);
                    break;
                case ClearFlags.Depth:
                    device.Clear(ClearOptions.DepthBuffer, backgroundColor, 1.0f, 0);
                    break;
            }
        }
            
        #region IComparer<Camera> Members
        public int Compare(Camera x, Camera y)
        {
            return x.depth.CompareTo(y.depth);
        }
        #endregion

        #region IComparer<IRenderable> Members
        public int Compare(Renderer x, Renderer y)
        {
            return x.renderQueue.CompareTo(y.renderQueue);
        }
        #endregion

        internal static Camera FindByName(string name)
        {
            for (int i = 0; i < _allCameras.Count; i++)
            {
                if (_allCameras[i].name == name)
                {
                    return _allCameras[i];
                }
            }
            return null;
        }

        internal int BatchRender(Mesh data, Material[] materials, Transform transform)
        {
#if DEBUG
            if (Camera.logRenderCalls)
            {
                Debug.LogFormat("Dyn batch: {0} on {1} at {2}", data, gameObject, (transform != null) ? transform.position : Vector3.zero);
            }
#endif
            int calls = 0;
            for (int i = 0; i < data.subMeshCount; i++)
            {
                Material mat = materials[0];
                if (i < materials.Length)
                {
                    mat = materials[i];
                }
                calls += dynamicBatchRenderer.Draw(this, mat, data, transform, i);
            }
            return calls;
        }

        internal bool DoFrustumCulling(ref BoundingSphere sphere)
        {
            if (sphere.Radius == 0)
            {
                return false;
            }

            ContainmentType contain;
            frustum.Contains(ref sphere, out contain);
            if (contain == ContainmentType.Disjoint)
            {
                return true;
            }
            return false;
        }
    }
}
