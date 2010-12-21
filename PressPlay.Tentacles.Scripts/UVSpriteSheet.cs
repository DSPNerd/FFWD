using System;
using System.Collections.Generic;
using System.Text;
using PressPlay.FFWD;
using PressPlay.FFWD.Components;

namespace PressPlay.Tentacles.Scripts {
	public class UVSpriteSheet : MonoBehaviour {

        public SkinnedMeshRenderer skinnedMeshRenderer;
        public MeshFilter meshFilter;

        protected Mesh mesh;
        protected Microsoft.Xna.Framework.Vector2[] baseUVs;
        protected Microsoft.Xna.Framework.Vector2[] tmpUVs;

        public int xCount;
        public int yCount;

        private int currentX = -1;
        private int currentY = -1;

        protected Microsoft.Xna.Framework.Vector2 tileSize;

        //protected SpriteSheetFrame[] frames;

        public bool scaleMeshUVsOnInitialize = true;

        protected Microsoft.Xna.Framework.Vector2 offset;

        protected bool isInitialized = false;

        public bool autoInitializeOnStart = true;

        public virtual void Start()
        {
            if (autoInitializeOnStart)
            {
                Initialize();
            }
        }

        // Use this for initialization
        public virtual void Initialize()
        {
            //Debug.Log("INITIALIZING UV SPRITE SHEET");
            if (isInitialized) { return; }
            isInitialized = true;

            Mesh tmpMesh = new Mesh();

            if (skinnedMeshRenderer != null)
            {
                tmpMesh = (Mesh)Instantiate(skinnedMeshRenderer.sharedMesh);
                skinnedMeshRenderer.sharedMesh = tmpMesh;
                mesh = skinnedMeshRenderer.sharedMesh;
            }
            else if (meshFilter != null)
            {
                tmpMesh.vertices = meshFilter.sharedMesh.vertices;
                tmpMesh.uv = meshFilter.sharedMesh.uv;
                tmpMesh.normals = meshFilter.sharedMesh.vertices;
                tmpMesh.triangles = meshFilter.sharedMesh.triangles;
                meshFilter.sharedMesh = tmpMesh;
                mesh = meshFilter.sharedMesh;
            }

            baseUVs = mesh.uv;

            tmpUVs = new Microsoft.Xna.Framework.Vector2[baseUVs.Length];

            for (int i = 0; i < baseUVs.Length; i++)
            {

                if (scaleMeshUVsOnInitialize)
                {
                    baseUVs[i].X = baseUVs[i].X / xCount;
                    baseUVs[i].Y = baseUVs[i].Y / yCount;
                }

                tmpUVs[i] = new Microsoft.Xna.Framework.Vector2(0, 0);
            }

            tileSize = new Microsoft.Xna.Framework.Vector2(1f / (float)xCount, 1f / (float)yCount);

            /*frames = new SpriteSheetFrame[xCount*yCount];
            for (int i = 0; i < frames.Length; i++) {
                frames[i] = new SpriteSheetFrame();
			
                frames[i]._xPos = XPosFromIndex(i);
                frames[i]._yPos = YPosFromIndex(i);
            }*/

            currentX = -1;
            currentY = -1;
            UpdateUVs(0, 0);


        }

        // Update is called once per frame
        protected void UpdateUVs(int _x, int _y)
        {

            if (_x == currentX && _y == currentY) { return; }

            currentX = _x;
            currentY = _y;

            /*offset = new Vector2(_x * tileSize.X, 1 - tileSize.Y - _y * tileSize.Y);
		
            Debug.Log("UPDATING UVs : "+_x+ ", "+_y + " offset : "+offset + " baseUVs.Length "+baseUVs.Length);
		
            for (int i = 0; i < baseUVs.Length; i++) 
            {
                tmpUVs[i].X = baseUVs[i].X + offset.X;
                tmpUVs[i].Y = baseUVs[i].Y + offset.Y;
            }*/

            mesh.uv = CreateUVs(currentX, currentY);

            //Debug.Log(mesh.uv);
        }

        protected Microsoft.Xna.Framework.Vector2[] CreateUVs(int _x, int _y)
        {
            offset = new Microsoft.Xna.Framework.Vector2(_x * tileSize.X, 1 - ((_y + 1) * tileSize.Y));

            //Debug.Log("Creating UVs : "+_x+ ", "+_y + " offset : "+offset + " baseUVs.Length "+baseUVs.Length);

            for (int i = 0; i < baseUVs.Length; i++)
            {
                tmpUVs[i].X = baseUVs[i].X + offset.X;
                tmpUVs[i].Y = baseUVs[i].Y + offset.Y;
            }

            return tmpUVs;
        }


        public int XPosFromIndex(int _index)
        {
            return _index % xCount;
        }

        public int YPosFromIndex(int _index)
        {
            return (int)(_index / xCount);
        }

        /*public int IndexFromFrame(SpriteSheetFrame _frame)
        {
            return _frame.xPos + _frame.yPos*xCount;
        }*/
    }


    /*[System.Serializable]
    public class SpriteSheetFrame{
        public SpriteSheetFrame(int _xPos, int _yPos)
        {
            xPos = _xPos;
            yPos = _yPos;
        }
	
        public int xPos;
        public int yPos;

    }*/
}