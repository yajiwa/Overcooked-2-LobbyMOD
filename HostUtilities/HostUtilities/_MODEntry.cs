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
using UnityEngine.Networking;
using SimpleJSON;
using Version = System.Version;

namespace HostUtilities
{
    [BepInPlugin("com.ch3ngyz.plugin.HostUtilities", "[HostUtilities] By.yc阿哲 Q群860480677 点击下方“‧‧‧”展开", "1.0.80")]
    [BepInProcess("Overcooked2.exe")]
    public class _MODEntry : BaseUnityPlugin
    {
        public static string Version = "1.0.80";
        public static Harmony HarmonyInstance { get; set; }
        public static List<string> AllHarmonyName = new List<string>();
        public static List<Harmony> AllHarmony = new List<Harmony>();
        public static string modName;
        public static _MODEntry Instance;
        public static bool IsInLobby = false;
        public static bool IsHost = false;
        public static bool IsInParty = false;
        public static float dpiScaleFactor = 1f;
        private readonly float baseScreenWidth = 1920f;
        private readonly float baseScreenHeight = 1080f;
        public static ConfigEntry<int> defaultFontSize;
        public static ConfigEntry<Color> defaultFontColor;
        public static bool IsSelectedAndPlay = false;
        public static bool IsAuthor = false;
        public static CSteamID CurrentSteamID;

        public void Awake()
        {
            try
            {
                defaultFontSize = Config.Bind<int>("00-UI", "MOD的UI字体大小", 20, new ConfigDescription("MOD的UI字体大小", new AcceptableValueRange<int>(5, 40)));
                defaultFontColor = Config.Bind<Color>("00-UI", "MOD的UI字体颜色", new Color(1, 1, 1, 1));

                modName = "HostUtilities";
                Instance = this;
                //不需要Update
                ChangeDisplayName.Awake();
                FixDoubleServing.Awake();
                FixBrokenWashingStation.Awake();
                ModifyScoreScreenTimeout.Awake();
                ReplaceOneShotAudio.Awake();


                //需要Update
                AddDirtyDishes.Awake();
                ForceHost.Awake();
                KickUser.Awake();
                LevelEdit.Awake();
                LevelSelector.Awake();
                QuitInLoadingScreen.Awake();
                RestartLevel.Awake();
                ScaleObject.Awake();
                SkipLevel.Awake();
                UI_DisplayModName.Awake();
                UI_DisplayModsOnResultsScreen.Awake();
                UI_DisplayKickedUser.Awake();
                UI_DisplayLatency.Awake();

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


        public void Update()
        {
            try
            {
                AddDirtyDishes.Update();
                ForceHost.Update();
                KickUser.Update();
                LevelEdit.Update();
                LevelSelector.Update();
                QuitInLoadingScreen.Update();
                RestartLevel.Update();
                ScaleObject.Update();
                SkipLevel.Update();
                UI_DisplayKickedUser.Update();
                UI_DisplayLatency.Update();
                UI_DisplayModsOnResultsScreen.Update();
                UI_DisplayModName.Update();
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

        public void OnGUI()
        {
            try
            {
                UI_DisplayModName.OnGUI();
                UI_DisplayModsOnResultsScreen.OnGUI();
                UI_DisplayKickedUser.OnGUI();
                UI_DisplayLatency.OnGUI();
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

        private static int canChangePosition = 0;
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
                if (canChangePosition > 5)
                {
                    UI_DisplayModName.SetRandomPosition();
                    canChangePosition = 0;
                }
                else
                {
                    canChangePosition += 1;
                }

                if (CurrentSteamID == null || CurrentSteamID.m_SteamID == 0)
                {
                    CurrentSteamID = SteamUser.GetSteamID();
                    LogError("CurrentSteamID: " + CurrentSteamID.m_SteamID);
                    if (_MODEntry.CurrentSteamID.m_SteamID.Equals(76561199191224186) && !IsAuthor)
                    {
                        IsAuthor = true;
                        ModifyMaxActiveOrders.Awake();
                        FixHeatedPosition.Awake();
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MetaGameProgress), "ByteLoad")]
        public static void MetaGameProgressByteLoadPatch(MetaGameProgress __instance)
        {
            GameObject versionCheckObject = new GameObject("VersionCheck");
            versionCheckObject.AddComponent<VersionCheckClass>();
        }

        private static DateTime lastCheckTime = DateTime.Now;
        private static bool skipFirstCheck = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartScreenFlow), "Awake")]
        public static void StartScreenFlow_Awake_Postfix()
        {
            DateTime currentTime = DateTime.Now;
            if (currentTime.Subtract(lastCheckTime).TotalSeconds > 600 && skipFirstCheck)
            {
                lastCheckTime = currentTime;
                LogInfo($"Start Version Check. Now Time {currentTime}");
                GameObject versionCheckObject = new GameObject("VersionCheck");
                versionCheckObject.AddComponent<VersionCheckClass>();
            }
            else
            {
                if (skipFirstCheck)
                {
                    LogError($"Skip Version Check. Last Check Time: {lastCheckTime}");
                }
                else
                {
                    LogInfo($"First Time Skip Version Check.");
                }
                skipFirstCheck = true;
            }
        }
    }

    public class VersionCheckClass : MonoBehaviour
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static VersionCheckClass Instance;
        private string githubtoken = string.Empty;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            try
            {
                string versionInfoUrl = "https://api.github.com/repos/CH3NGYZ/Overcooked-2-HostUtilities/releases?per_page=100";
                UI_DisplayModName.cornerMessage = $"Host Utilities v{_MODEntry.Version} ";
                StartCoroutine(SendWebRequest(versionInfoUrl));
            }
            catch (Exception e)
            {
                _MODEntry.LogError(e.Message);
                _MODEntry.LogError(e.StackTrace);
            }
        }

        private IEnumerator SendWebRequest(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("User-Agent", "request");
                request.SetRequestHeader("accept", "application/vnd.github+json");

                if (githubtoken != string.Empty)
                {
                    request.SetRequestHeader("Authorization", $"token {githubtoken}");
                }

                // 设置请求超时时间为10秒
                request.timeout = 10;
                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    HandleWebResponse(request);
                }
                else if (request.responseCode == 403)
                {
                    HandleRateLimit(request);
                }
                else
                {
                    _MODEntry.LogError($"主 URL 请求失败: {request.error}");
                    UI_DisplayModName.cornerMessage = $"Host Utilities v{_MODEntry.Version} AFailed {request.error}";
                    // 请求超时，使用备用 URL 重新请求一次
                    _MODEntry.LogError("请求超时，尝试使用备用 URL");
                    using (UnityWebRequest backupRequest = UnityWebRequest.Get("https://github-api.azhe.chat/repos/CH3NGYZ/Overcooked-2-HostUtilities/releases?per_page=100"))
                    {
                        backupRequest.SetRequestHeader("User-Agent", "request");
                        backupRequest.SetRequestHeader("accept", "application/vnd.github+json");

                        if (githubtoken != string.Empty)
                        {
                            backupRequest.SetRequestHeader("Authorization", $"token {githubtoken}");
                        }

                        backupRequest.timeout = 10;

                        yield return backupRequest.SendWebRequest();

                        if (backupRequest.responseCode == 200)
                        {
                            HandleWebResponse(backupRequest);
                        }
                        else if (backupRequest.responseCode == 403)
                        {
                            HandleRateLimit(backupRequest);
                        }
                        else
                        {
                            _MODEntry.LogError($"备用 URL 请求失败: {backupRequest.error}");
                            UI_DisplayModName.cornerMessage = $"Host Utilities v{_MODEntry.Version} BFailed {backupRequest.error}";
                        }
                    }
                }
            }
            GameObject versionCheckObject = GameObject.Find("VersionCheck");
            Destroy(versionCheckObject);
        }

        private void HandleWebResponse(UnityWebRequest request)
        {
            JSONNode jsonArray = JSON.Parse(request.downloadHandler.text);
            Dictionary<Version, string> versionBodyDict = new Dictionary<Version, string>();
            foreach (JSONNode node in jsonArray)
            {
                string tagName = node["tag_name"].Value;
                if (tagName.Contains("BepInEx")) continue;
                string body = node["body"].Value;
                tagName = tagName.Replace("v", "");
                Version version = new Version(tagName);
                versionBodyDict.Add(version, body);
            }

            //读取当前版本号以及最新版本号
            Version latestVersion = new Version("1.0.0");
            foreach (var entry in versionBodyDict)
            {
                latestVersion = entry.Key;
                break;
            }
            Version currentVersion = new Version(_MODEntry.Version);

            // 输出从当前版本到最新版本之间的所有更新
            bool isUpdateAvailable = false;
            string updateLog = $"最新版本更新内容: ";
            for (Version ver = latestVersion; ver > currentVersion; ver = new Version(ver.Major, ver.Minor, ver.Build - 1))
            {
                if (versionBodyDict.ContainsKey(ver))
                {
                    isUpdateAvailable = true;
                    updateLog += versionBodyDict[ver] + "更多更新内容, 请打开安装器查看";
                    break;
                }
            }

            if (isUpdateAvailable)
            {
                T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
                if (dialog != null)
                {
                    dialog.Initialize($"MOD更新{currentVersion.Build}到{latestVersion.Build}", updateLog.EndsWith("\n") ? updateLog.Substring(0, updateLog.Length - 1) : updateLog, "Text.Button.Okay", string.Empty, "Text.Button.Cancel", T17DialogBox.Symbols.Spinner, false, false, false);
                    dialog.OnConfirm += () =>
                    {
                        T17DialogBox upddialog = T17DialogBoxManager.GetDialog(false);
                        upddialog.Initialize($"您必须手动打开安装器", $"以从v{currentVersion}更新到v{latestVersion}", "Text.Button.Okay", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, false, false, false);
                        upddialog.OnConfirm += () =>
                        {
                            _MODEntry.LogInfo($"Will Upd {latestVersion}");
                            Application.Quit();
                        };
                        upddialog.Show();
                    };
                    dialog.OnCancel += () =>
                    {
                        UI_DisplayModName.cornerMessage = $"Host Utilities v{_MODEntry.Version} Cancel Update {latestVersion}";
                        _MODEntry.LogInfo($"Cancel Update {latestVersion}");
                    };
                    dialog.Show();
                }
                _MODEntry.LogInfo("Update Log from " + currentVersion + " to " + latestVersion + ":");
                _MODEntry.LogError(updateLog);
            }
            else
            {
                UI_DisplayModName.cornerMessage = $"Host Utilities v{_MODEntry.Version} Latest";
                _MODEntry.LogInfo("无可用更新.");
            }
        }

        private void HandleRateLimit(UnityWebRequest request)
        {
            string ts = request.GetResponseHeader("X-RateLimit-Reset");
            if (long.TryParse(ts, out long number))
            {
                DateTime dateTime = UnixTimeStampToDateTime(number);
                DateTime utcPlus8Time = dateTime.ToUniversalTime().AddHours(8);
                string formattedDateTime = utcPlus8Time.ToString("yyyy/MM/dd hh:mm:ss tt");
                _MODEntry.LogError($"请求更新API访问达到限制, 恢复时间: {formattedDateTime}");
                UI_DisplayModName.cornerMessage += $"Forbidden {formattedDateTime}";
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}