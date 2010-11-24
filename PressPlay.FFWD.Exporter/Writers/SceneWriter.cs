﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEditor;
using System.IO;
using PressPlay.FFWD.Exporter.Interfaces;

namespace PressPlay.FFWD.Exporter.Writers
{
    public class SceneWriter
    {
        public SceneWriter(TypeResolver resolver)
        {
            this.resolver = resolver;
        }

        private TypeResolver resolver;
        private List<string> exportedTextures = new List<string>();

        public string ExportDir { get; set; }
        public string TextureDir { get; set; }

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
                writer.WriteAttributeString("ID", "#go" + go.GetInstanceID());
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
            writer.WriteElementString("localPosition", ToString(transform.localPosition));
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
            if (resolver.SkipComponent(component))
            {
                return;
            }

            writer.WriteStartElement("component");
            writer.WriteAttributeString("Type", resolver.ResolveTypeName(component));

            System.Type type = component.GetType();
            IComponentWriter componentWriter = resolver.GetComponentWriter(type);
            if (componentWriter != null)
            {
                componentWriter.Write(this, component);
            }
            writer.WriteEndElement();
        }

        internal void WriteTexture(Texture texture)
        {
            writer.WriteElementString("Texture", texture.name);
            ExportTexture(texture as Texture2D);
        }

        internal void WriteElement(string name, object obj)
        {
            if (obj == null)
            {
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
            if (obj is Vector3[])
            {
                writer.WriteElementString(name, ToString(obj as Vector3[]));
                return;
            }
            writer.WriteElementString(name, obj.ToString());
        }

        private void ExportTexture(Texture2D tex)
        {
            if (tex == null) return;
            if (exportedTextures.Contains(tex.name)) return;

            string path = Path.Combine(TextureDir, tex.name + ".png");
            try
            {
                Color[] texPixels = tex.GetPixels();
                Texture2D tex2 = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
                tex2.SetPixels(texPixels);
                byte[] texBytes = tex2.EncodeToPNG();
                FileStream writeStream;
                writeStream = new FileStream(path, FileMode.Create);
                BinaryWriter writeBinay = new BinaryWriter(writeStream);
                for (int i = 0; i < texBytes.Length; i++) writeBinay.Write(texBytes[i]);
                writeBinay.Close();
                exportedTextures.Add(tex.name);
            }
            catch (UnityException ue)
            {
                Debug.Log(ue.ToString());
            }
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
            return vector3.x.ToString("0.#####") + " " + vector3.y.ToString("0.#####") + " " + vector3.z.ToString("0.#####");
        }

        private string ToString(Quaternion quaternion)
        {
            return quaternion.x.ToString("0.#####") + " " + quaternion.y.ToString("0.#####") + " " + quaternion.z.ToString("0.#####") + " " + quaternion.w.ToString("0.#####");
        }

        private string ToString(bool b)
        {
            return b.ToString().ToLower();
        }
        #endregion
    }
}
