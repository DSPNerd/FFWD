﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEditor;
using System.IO;
using PressPlay.FFWD.Exporter.Interfaces;
using System.Globalization;

namespace PressPlay.FFWD.Exporter.Writers
{
    public class SceneWriter
    {
        public SceneWriter(TypeResolver resolver, AssetHelper assets)
        {
            this.resolver = resolver;
            assetHelper = assets;
        }

        private TypeResolver resolver;
        private AssetHelper assetHelper;

        public string ExportDir { get; set; }
        public bool FlipYInTransforms { get; set; }

        private XmlWriter writer = null;

        public void Write(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            using (writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("XnaContent");
                writer.WriteStartElement("Asset");
                writer.WriteAttributeString("Type", resolver.DefaultNamespace + ".Scene");
                WriteGOs();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private void WriteGOs()
        {
            UnityEngine.Object[] objs = GameObject.FindObjectsOfType(typeof(GameObject));

            for (int i = 0; i < objs.Length; i++)
            {
                GameObject go = objs[i] as GameObject;
                if (go.transform.parent != null)
                {
                    continue;
                }
                writer.WriteStartElement("gameObject");
                WriteGameObject(go);
                writer.WriteEndElement();
            }
        }

        private void WriteGameObject(GameObject go)
        {
            writer.WriteElementString("id", go.GetInstanceID().ToString());
            writer.WriteElementString("name", go.name);
            writer.WriteStartElement("transform");
            WriteTransform(go.transform);
            writer.WriteEndElement();
            UnityEngine.Object prefab = EditorUtility.GetPrefabParent(go);
            if (prefab != null)
            {
                writer.WriteElementString("prefab", prefab.name);
            }
            else
            {
                writer.WriteStartElement("prefab");
                writer.WriteAttributeString("Null", ToString(true));
                writer.WriteEndElement();
            }
            writer.WriteStartElement("components");
            Component[] comps = go.GetComponents(typeof(Component));
            for (int i = 0; i < comps.Length; i++)
            {
                WriteComponent(comps[i]);
            }
            writer.WriteEndElement();
        }

        private void WriteTransform(Transform transform)
        {
            Vector3 pos = transform.localPosition;
            if (FlipYInTransforms)
            {
                pos.y = -pos.y;
            }
            writer.WriteElementString("localPosition", ToString(pos));
            writer.WriteElementString("localScale", ToString(transform.localScale));
            writer.WriteElementString("localRotation", ToString(transform.localRotation));
            if (transform.childCount > 0)
            {
                writer.WriteStartElement("children");
                for (int i = 0; i < transform.childCount; i++)
                {
                    writer.WriteStartElement("child");
                    WriteGameObject(transform.GetChild(i).gameObject);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

        }

        private void WriteComponent(Component component)
        {
            if (resolver == null)
            {
                return;
            }
            if (component == null)
            {
                return;
            }
            if (resolver.SkipComponent(component))
            {
                return;
            }

            System.Type type = component.GetType();
            IComponentWriter componentWriter = resolver.GetComponentWriter(type);
            if (componentWriter != null)
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("Type", resolver.ResolveTypeName(component));
                componentWriter.Write(this, component);
                writer.WriteEndElement();
            }
        }

        internal void WriteTexture(Texture texture)
        {
            writer.WriteElementString("texture", texture.name);
            assetHelper.ExportTexture(texture as Texture2D);
        }

        internal void WriteScript(MonoBehaviour component)
        {
            assetHelper.ExportScript(component, false);
            // Check for base classes
            Type tp = component.GetType().BaseType;
            if (tp != typeof(MonoBehaviour))
            {
                WriteScript(component.gameObject.AddComponent(tp) as MonoBehaviour);
            }
        }

        internal void WriteScriptStub(MonoBehaviour component)
        {
            assetHelper.ExportScript(component, true);
            // Check for base classes
            Type tp = component.GetType().BaseType;
            if (tp != typeof(MonoBehaviour))
            {
                WriteScriptStub(component.gameObject.AddComponent(tp) as MonoBehaviour);
            }
        }

        internal void WriteMesh(Mesh mesh)
        {
            string asset = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(mesh.GetInstanceID()));
            WriteElement("asset", asset);
            WriteElement("mesh", mesh.name);
            assetHelper.ExportMesh(mesh);
        }

        internal void WriteElement(string name, object obj)
        {
            if (obj == null)
            {
                writer.WriteStartElement(name);
                writer.WriteAttributeString("Null", ToString(true));
                writer.WriteEndElement();
                return;
            }
            if (obj is float)
            {
                writer.WriteElementString(name, ToString((float)obj));
                return;
            }
            if (obj is Boolean)
            {
                writer.WriteElementString(name, ToString((Boolean)obj));
                return;
            }
            if (obj is int[])
            {
                writer.WriteElementString(name, ToString(obj as int[]));
                return;
            }
            if (obj is Vector3)
            {
                writer.WriteElementString(name, ToString((Vector3)obj));
                return;
            }
            if (obj is Vector3[])
            {
                writer.WriteElementString(name, ToString(obj as Vector3[]));
                return;
            }
            writer.WriteElementString(name, obj.ToString());
        }


        #region ToString methods
        private string ToString(int[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int item in array)
            {
                sb.Append(item);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        private string ToString(Vector3[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Vector3 item in array)
            {
                sb.Append(ToString(item));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        private string ToString(Vector3 vector3)
        {
            return vector3.x.ToString("0.#####", CultureInfo.InvariantCulture) + " " + vector3.y.ToString("0.#####", CultureInfo.InvariantCulture) + " " + vector3.z.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        private string ToString(Quaternion quaternion)
        {
            return quaternion.x.ToString("0.#####", CultureInfo.InvariantCulture) + " " + quaternion.y.ToString("0.#####", CultureInfo.InvariantCulture) + " " + quaternion.z.ToString("0.#####", CultureInfo.InvariantCulture) + " " + quaternion.w.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        private string ToString(bool b)
        {
            return b.ToString().ToLower();
        }

        private string ToString(float f)
        {
            return f.ToString("0.#####", CultureInfo.InvariantCulture);
        }
        #endregion

    }
}
