using System;
using BepInEx;
using BepInEx.Logging;
using CecilMerge;
using UnityEngine;

namespace TestPluginA
{
    [BepInPlugin("CecilMerge.UnitTesters.A", "CecilMerge UnitTester Alpha", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public class BehaviourObject : MonoBehaviour
        {
            public ManualLogSource Log;
            public void Start()
            {
                Log.LogInfo("Hello!");
            }
        }
        public void Awake()
        {
        }
    }

    [CecilPatch]
    public class ClipboardItemPatch : ClipboardItem
    {
        
    }
}