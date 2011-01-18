﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace PressPlay.FFWD
{
    public class AssetHelper
    {
        private Dictionary<string, ContentManager> contentManagers = new Dictionary<string, ContentManager>();
        private Dictionary<string, List<string>> managerContent = new Dictionary<string, List<string>>();
        private Dictionary<string, object> content = new Dictionary<string, object>();

        public Func<ContentManager> CreateContentManager;
        private static List<string> staticAssets = new List<string>();

        public T Load<T>(string contentPath)
        {
            return Load<T>(Application.loadedLevelName, contentPath);
        }

        public T Load<T>(string category, string contentPath)
        {
            if (!content.ContainsKey(contentPath))
            {
                if (staticAssets.Contains(contentPath) || staticAssets.Contains(category))
                {
                    category = "Static";
                }
                ContentManager manager = GetContentManager(category);
                try
                {
                    content.Add(contentPath, manager.Load<T>(contentPath));
                    managerContent[category].Add(contentPath);
                }
                catch
                {
                    Debug.Log("Asset not found. " + typeof(T).Name + " at " + contentPath);
                    return default(T);
                }
            }
            else
            {
                if (!(content[contentPath] is T))
                {
                    return default(T);
                }
            }
            return (T)content[contentPath];
        }

        public void Unload(string category)
        {
            if (contentManagers.ContainsKey(category))
            {
                ContentManager manager = GetContentManager(category);
                contentManagers.Remove(category);
                List<string> contentInManager = managerContent[category];
                for (int i = 0; i < contentInManager.Count; i++)
                {
                    content.Remove(contentInManager[i]);
                }
                managerContent.Remove(category);
                manager.Unload();
            }
        }

        private ContentManager GetContentManager(string category)
        {
            if (!contentManagers.ContainsKey(category))
            {
                contentManagers.Add(category, CreateContentManager());
                managerContent.Add(category, new List<string>());
            }
            return contentManagers[category];
        }

        public static void AddStaticAsset(string name)
        {
            staticAssets.Add(name);
        }
    }
}
