using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using CecilMerge;
using UnityEngine;

namespace TestPluginA
{
    [BepInPlugin("CecilMerge.UnitTesters.A", "CecilMerge UnitTester Alpha", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Hello!");
            
            Logger.LogInfo(MergedEnum<CauseOfDeath>.ValueOrDefault(nameof(CauseOfDeathPatch.DivideByZero)));
        }
    }

    [CecilPatch(typeof(CauseOfDeath))]
    [CecilBefore("some.other.guid")]
    [CecilAppend]
    public enum CauseOfDeathPatch
    {
        DivideByZero,
        GrogledTooMuch
    }
}