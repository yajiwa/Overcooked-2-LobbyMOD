﻿using UnityEngine;
using BepInEx.Configuration;
using GameModes.Horde;


namespace HostUtilities
{
    public class SkipLevel
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> stopLevel;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            stopLevel = _MODEntry.Instance.Config.Bind("02-按键绑定", "10-直接跳过关卡", KeyCode.Delete, "跳过关卡");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(stopLevel.Value))
            {
                if (!_MODEntry.IsHost)
                {
                    _MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                if (!cooling)
                {
                    log("跳过关卡");
                    EndLevel();
                }
                if (System.Environment.TickCount - startTime > 5000)
                {
                    cooling = false;
                    log("跳过关卡");
                    EndLevel();
                }
            }
        }


        public static void EndLevel()
        {
            ServerCampaignFlowController flowController = GameObject.FindObjectOfType<ServerCampaignFlowController>();
            ServerHordeFlowController ServerHordeFlowController = GameObject.FindObjectOfType<ServerHordeFlowController>();
            if (flowController == null)
            {
                log("flowController为空");
                if (ServerHordeFlowController == null)
                {
                    log("ServerHordeFlowController为空");
                    return;
                }
                else
                {
                    ServerHordeFlowController.Damage(100);
                    log("跳过敌群关卡");
                    startTime = System.Environment.TickCount;
                    cooling = true;
                }
            }
            else
            {
                flowController.SkipToEnd();
                log("跳过普通关卡");
                startTime = System.Environment.TickCount;
                cooling = true;
            }

        }
    }
}
