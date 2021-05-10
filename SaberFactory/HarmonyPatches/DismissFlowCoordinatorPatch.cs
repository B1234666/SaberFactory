﻿using HarmonyLib;
using HMUI;

namespace SaberFactory.HarmonyPatches
{
    // If state of flow coordinators changes always close saber factory

    [HarmonyPatch(typeof(FlowCoordinator), "Activate")]
    public class ActivateFlowCoordinatorPatch
    {
        public static void Prefix()
        {
            Editor.Editor.Instance?.Close(true);
        }
    }

    [HarmonyPatch(typeof(FlowCoordinator), "Deactivate")]
    public class DeactivateFlowCoordinatorPatch
    {
        public static void Prefix()
        {
            Editor.Editor.Instance?.Close(true);
        }
    }
}