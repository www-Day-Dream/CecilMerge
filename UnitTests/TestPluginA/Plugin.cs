using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using CecilMerge;
using HarmonyLib;
using UnityEngine;

namespace TestPluginA
{
    [BepInPlugin("CecilMerge.UnitTesters.A", "CecilMerge UnitTester Alpha", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }
        public void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("Hello!");
            
            Logger.LogInfo(MergedEnum<CauseOfDeath>.ValueOrDefault(nameof(CauseOfDeathPatch.DivideByZero)));
            Logger.LogInfo(MergedEnum<CauseOfDeath>.ValueOrDefault(nameof(CauseOfDeathPatch.GrogledTooMuch)));
            
        }
    }

    [CecilPatch(typeof(CauseOfDeath))]
    public enum CauseOfDeathPatch
    {
        DivideByZero,
        GrogledTooMuch
    }

    [CecilPatch(typeof(ClipboardItem))]
    public class ClipboardPatch
    {
        private ClipboardItem @this;
        private GrabbableObject @base;
        
        [CecilMergeMethod(null, true)]
        public void Start()
        {
            @this.PocketItem();
            TestMethod();
        }

        [CecilMergeMethod(null, true)]
        public void TestMethod()
        {
            Plugin.Logger.LogInfo("It fuckin' worked with no issues! " + @this.gameObject.name);
        }
    }
}