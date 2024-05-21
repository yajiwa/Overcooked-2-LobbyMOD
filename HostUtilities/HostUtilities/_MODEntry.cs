﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using Team17.Online;
using UnityEngine;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace HostUtilities
{
    [BepInPlugin("com.ch3ngyz.plugin.HostUtilities", "[HostUtilities] By.yc阿哲 Q群860480677 点击下方“‧‧‧”展开", "1.0.64")]
    [BepInProcess("Overcooked2.exe")]
    public class _MODEntry : BaseUnityPlugin
    {
        public static string Version = "1.0.64";
        public static Harmony HarmonyInstance { get; set; }
        public static List<string> AllHarmonyName = new List<string>();
        public static List<Harmony> AllHarmony = new List<Harmony>();
        public static string modName;
        public static _MODEntry Instance;
        public static bool IsInLobby = false;
        public static bool IsHost = false;
        public static bool IsInParty = false;
        public static float dpiScaleFactor = 1f;
        private float baseScreenWidth = 1920f;
        private float baseScreenHeight = 1080f;
        public static ConfigEntry<int> defaultFontSize;
        public static ConfigEntry<Color> defaultFontColor;
        public static bool IsSelectedAndPlay = false;

        //当前玩家的steamid
        public static CSteamID CurrentSteamID;
        public static bool IsAuthor = false;

        //自动更新相关
        public static bool IsUpdateNeded = false;
        public static string ReleaseNote = "";
        public void Awake()
        {
            try
            {
                UI_DisplayModName.Awake();
                defaultFontSize = Config.Bind<int>("00-UI", "MOD的UI字体大小", 20, new ConfigDescription("MOD的UI字体大小", new AcceptableValueRange<int>(5, 40)));
                defaultFontColor = Config.Bind<Color>("00-UI", "MOD的UI字体颜色", new Color(1, 1, 1, 1));


                modName = "HostUtilities";
                Instance = this;
                UI_DisplayModsOnResultsScreen.Awake();
                SkipLevel.Awake();
                KickUser.Awake();
                LevelEdit.Awake();
                QuitInLoadingScreen.Awake();
                UI_DisplayKickedUser.Awake();
                ReplaceOneShotAudio.Awake();
                ForceHost.Awake();
                UI_DisplayLatency.Awake();
                FixDoubleServing.Awake();
                RestartLevel.Awake();
                ChangeDisplayName.Awake();
                AlwaysServeOldestOrder.Awake();
                LevelSelector.Awake();
                AddDirtyDishes.Awake();
                FixBrokenWashingStation.Awake();

                HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
                AllHarmony.Add(HarmonyInstance);
                AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
                foreach (string harmony in AllHarmonyName)
                {
                    LogError($"Patched {harmony}!");
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            try
            {
                Instance = null;
                for (int i = 0; i < AllHarmony.Count; i++)
                {
                    AllHarmony[i].UnpatchAll();
                    LogWarning($"Unpatched {AllHarmonyName[i]}!");
                }
                AllHarmony.Clear();
                AllHarmonyName.Clear();
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        public void Update()
        {
            try
            {
                UI_DisplayModName.Update();
                UI_DisplayModsOnResultsScreen.Update();
                SkipLevel.Update();
                KickUser.Update();
                LevelEdit.Update();
                QuitInLoadingScreen.Update();
                UI_DisplayKickedUser.Update();
                UI_DisplayLatency.Update();
                ForceHost.Update();
                RestartLevel.Update();
                LevelSelector.Update();
                AddDirtyDishes.Update();
                if (IsAuthor)
                {
                    Recipe.Update();
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        public void OnGUI()
        {
            try
            {
                UI_DisplayModName.OnGUI();
                UI_DisplayModsOnResultsScreen.OnGUI();
                UI_DisplayKickedUser.OnGUI();
                UI_DisplayLatency.OnGUI();
                if (IsAuthor)
                {
                    Recipe.OnGUI();
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }


        public static bool isInLobby()
        {
            ServerLobbyFlowController instance = ServerLobbyFlowController.Instance;
            ClientLobbyFlowController instance2 = ClientLobbyFlowController.Instance;
            bool flag = false;
            flag |= (instance2 != null && (LobbyFlowController.LobbyState.OnlineThemeSelection.Equals(instance2.m_state) || LobbyFlowController.LobbyState.LocalThemeSelection.Equals(instance2.m_state)));
            flag |= (instance != null && (LobbyFlowController.LobbyState.OnlineThemeSelection.Equals(instance.m_state) || LobbyFlowController.LobbyState.LocalThemeSelection.Equals(instance.m_state)));
            bool flag2 = flag && instance != null;
            if (flag != IsInLobby)
            {
                if (!flag)
                {
                    IsInLobby = false;
                    LogInfo("Exit Lobby");
                    return false;
                }
                else
                {
                    IsInLobby = true;
                    IsInParty = true;
                    LogInfo("Enter Lobby");
                    return true;
                }
            }
            return false;
        }
        private void UpdateGUIDpi()
        {
            float ratioWidth = (float)Screen.width / baseScreenWidth;
            float ratioHeight = (float)Screen.height / baseScreenHeight;
            dpiScaleFactor = Mathf.Min(ratioWidth, ratioHeight);
        }
        public static void LogWarning(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogWarning(message);
        public static void LogInfo(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogInfo(message);
        public static void LogError(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError(message);
        public static void LogHarmony(string classname, MethodBase methodBase) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError($"{classname}: {methodBase.Name}");

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPatch(typeof(DisconnectionHandler), "HandleSessionConnectionLost")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireSessionConnectionLostEvent")]
        [HarmonyPatch(typeof(DisconnectionHandler), "OnlineMultiplayerConnectionModeErrorCallback")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireConnectionModeErrorEvent")]
        [HarmonyPatch(typeof(DisconnectionHandler), "HandleLocalDisconnection")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireLocalDisconnectionEvent")]
        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireKickedFromSessionEvent")]
        [HarmonyPatch(typeof(ClientLobbyFlowController), "Leave")]
        [HarmonyPatch(typeof(LoadingScreenFlow), "RequestReturnToStartScreen")]
        [HarmonyPostfix]
        public static void ExitFromParty()
        {
            IsInParty = false;
        }


        public static bool isHost()
        {
            IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
            if (onlinePlatformManager == null)
            {
                return true;
            }

            IOnlineMultiplayerSessionCoordinator coordinator = onlinePlatformManager.OnlineMultiplayerSessionCoordinator();
            if (coordinator == null)
            {
                return true;
            }

            if (coordinator.IsIdle())
            {
                return true;
            }

            return coordinator.IsHost();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch()
        {
            try
            {
                IsHost = isHost();
                isInLobby();
                if (Screen.width != Mathf.RoundToInt(_MODEntry.Instance.baseScreenWidth * dpiScaleFactor) || Screen.height != Mathf.RoundToInt(_MODEntry.Instance.baseScreenHeight * dpiScaleFactor)) { Instance.UpdateGUIDpi(); }
                //LogInfo($"IsHost  {IsHost}  IsInParty  {IsInParty}");

                if (CurrentSteamID == null || CurrentSteamID.m_SteamID == 0)
                {
                    CurrentSteamID = SteamUser.GetSteamID();
                    LogError("CurrentSteamID: " + CurrentSteamID.m_SteamID);
                    if (_MODEntry.CurrentSteamID.m_SteamID.Equals(76561199191224186) && !IsAuthor)
                    {
                        IsAuthor = true;
                        Recipe.Awake();
                        ModifyMaxActiveOrders.Awake();
                        FixHeatedPosition.Awake();
                    }
                    VersionChecker versionChecker = new VersionChecker();
                    versionChecker.Init();
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }

        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch2()
        {

        }
        public static void ShowWarningDialog(string message)
        {
            T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
            if (dialog != null)
            {
                dialog.Initialize("Text.Warning", "\"" + message.Replace("\n\n", "\n") + "\"", "Text.Button.Continue", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, true, true, false);
                dialog.Show();
            }
        }

        //进入主界面后，自动检查更新
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MetaGameProgress), "ByteLoad")]
        public static void MetaGameProgressByteLoadPatch(MetaGameProgress __instance)
        {
            if (IsUpdateNeded)
            {
                ShowWarningDialog("HostUtilities有新版本可用\n" + ReleaseNote + "\n请使用MOD安装器下载更新");
            }
        }
    }

    [System.Serializable]
    public class ReleaseInfo
    {
        public string tag_name;
        public string body;
    }
    [Serializable]
    public class VersionChecker : MonoBehaviour
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static void logerr(string mes) => _MODEntry.LogError(mes);
        private static readonly string currentVersion = _MODEntry.Version; // 当前MOD版本
        private static readonly string versionInfoUrl = "https://api.github.com/repos/CH3NGYZ/Overcooked-2-HostUtilities/releases/latest"; // GitHub API的版本信息URL
        public void Init()
        {
            logerr("init");
            GetWebContent(versionInfoUrl);
            logerr("init out");
        }

        public static string GetWebContent(string url)
        {
            //throw new Exception("uihfuhfhsuifhuioshfgiuush");
            try
            {
                // 创建 WebClient 实例
                using (WebClient client = new WebClient())
                {
                    // 使用 DownloadString 方法下载指定 URL 的内容
                    string content = client.DownloadString(url);
                    _MODEntry.LogInfo($"{content}");
                    // 返回下载的内容
                    return content;
                }
            }
            catch (WebException e)
            {
                // 如果请求失败，捕获异常并打印错误信息
                _MODEntry.LogError($"Error: {e.Message}");
                return null;
            }
        }

        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            System.Version current = new System.Version(currentVersion);
            System.Version latest = new System.Version(latestVersion);
            return latest > current;
        }
    }

    public static class WebContentFetcher
    {
        public static string GetWebContent(string url)
        {
            try
            {
                // 创建 WebClient 实例
                using (WebClient client = new WebClient())
                {
                    // 使用 DownloadString 方法下载指定 URL 的内容
                    string content = client.DownloadString(url);

                    // 返回下载的内容
                    return content;
                }
            }
            catch (WebException e)
            {
                // 如果请求失败，捕获异常并打印错误信息
                _MODEntry.LogError($"Error: {e.Message}");
                return null;
            }
        }
    }
}
