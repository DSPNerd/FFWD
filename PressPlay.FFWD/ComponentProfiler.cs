﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace PressPlay.FFWD
{
    class ComponentProfiler
    {
        private List<ComponentUpdateProfile> componentUpdateProfiles = new List<ComponentUpdateProfile>();

        private ComponentUpdateProfile currentUpdateProfile;
        
        private Stopwatch stopwatch = new Stopwatch();

        public long totalTicks = 0;
        public float totalMilliseconds {
            get {
                return (totalTicks / Stopwatch.Frequency) * 1000f;
            }
        }

        private ComponentUpdateProfile GetComponentProfileFromList(Component _component)
        {
            ComponentUpdateProfile updateProfile = new ComponentUpdateProfile(_component);
            for (int i = 0; i < componentUpdateProfiles.Count; i++)
            {
                if (componentUpdateProfiles[i].component == _component)
                {
                    return componentUpdateProfiles[i];
                }
            }
            
            componentUpdateProfiles.Add(currentUpdateProfile);
            
            return updateProfile;
        }

        public void StartUpdateCall(Component _component)
        {
            currentUpdateProfile = GetComponentProfileFromList(_component);
            currentUpdateProfile.updateCalls++;
            
            stopwatch.Start();
        }

        public void StartFixedUpdateCall(Component _component)
        {
            currentUpdateProfile = GetComponentProfileFromList(_component);
            currentUpdateProfile.fixedUpdateCalls++;

            stopwatch.Start();
        }

        public void StartLateUpdateCall(Component _component)
        {
            currentUpdateProfile = GetComponentProfileFromList(_component);
            currentUpdateProfile.lateUpdateCalls++;

            stopwatch.Start();
        }

        public void EndUpdateCall()
        {
            stopwatch.Stop();
            currentUpdateProfile.updateTotalTicks += stopwatch.ElapsedTicks;
            totalTicks += stopwatch.ElapsedTicks;
            stopwatch.Reset();
        }

        public void EndFixedUpdateCall()
        {
            stopwatch.Stop();
            currentUpdateProfile.fixedUpdateTotalTicks += stopwatch.ElapsedTicks;
            totalTicks += stopwatch.ElapsedTicks;
            stopwatch.Reset();
        }

        public void EndLateUpdateCall()
        {
            stopwatch.Stop();
            currentUpdateProfile.lateUpdateTotalTicks += stopwatch.ElapsedTicks;
            totalTicks += stopwatch.ElapsedTicks;
            stopwatch.Reset();
        }

        public void FlushData()
        {
            totalTicks = 0;
            componentUpdateProfiles.Clear();
        }

        public List<ComponentUpdateProfile> Sort()
        {
            //componentUpdateProfiles.Sort();

            return componentUpdateProfiles;
        }

        public ComponentUpdateProfile GetWorst()
        {
            ComponentUpdateProfile worst = new ComponentUpdateProfile(null);

            /*for (int i = 0; i < componentUpdateProfiles.Count; i++)
            {
                if (componentUpdateProfiles[i].totalTicks > worst.totalTicks)
                {
                    worst = componentUpdateProfiles[i];
                }
            }*/

            //if (worst.name == null)
            //{
            //    Debug.Log("whuaa");
            //}
            if (componentUpdateProfiles.Count == 0) { return new ComponentUpdateProfile(); }
            return componentUpdateProfiles[0];
        }
    }

    struct ComponentUpdateProfile : IComparable<ComponentUpdateProfile>
    {
        public Component component;
        public string name
        {
            get 
            {
                if (component == null)
                { return null; }
                return component.name;
            }
        }
        public int updateCalls;
        public int lateUpdateCalls;
        public int fixedUpdateCalls;
        public long updateTotalTicks;
        public long lateUpdateTotalTicks;
        public long fixedUpdateTotalTicks;

        public float totalMilliseconds {
            get {
                return (totalTicks / Stopwatch.Frequency) * 1000f;
            }
        }

        public long totalTicks 
        {
            get { return updateTotalTicks + lateUpdateTotalTicks + fixedUpdateTotalTicks; }
        }

        public ComponentUpdateProfile(Component component)
        {
            this.component = component;
            updateCalls = 0;
            lateUpdateCalls = 0;
            fixedUpdateCalls = 0;
            updateTotalTicks = 0;
            lateUpdateTotalTicks = 0;
            fixedUpdateTotalTicks = 0;
        }

        public void Flush()
        {
            updateCalls = 0;
            lateUpdateCalls = 0;
            fixedUpdateCalls = 0;
            updateTotalTicks = 0;
            lateUpdateTotalTicks = 0;
            fixedUpdateTotalTicks = 0;
        }

        public int CompareTo(ComponentUpdateProfile other)
        {
            return (int)other.totalTicks - (int)totalTicks;
        }

        public override  string ToString()
        {
            if (name == null)
            {
                return "null " + totalMilliseconds.ToString();
            }

            return name + " " + totalMilliseconds.ToString() + " ms";
        }
    }
}
