﻿using BuilderCore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
namespace ChampionsOfForest.Res
{
    public class ResourceLoader : MonoBehaviour
    {
        public static ResourceLoader instance = null;
        public static Texture2D GetTexture(int i)
        {
            if (ModSettings.IsDedicated) return null;
                if (instance.LoadedTextures.ContainsKey(i))
            {
                return instance.LoadedTextures[i];
            }
            else if (instance.unloadedResources.ContainsKey(i))
            {
                LoadTexture(instance.unloadedResources[i]);
                return instance.LoadedTextures[i];
            }
            return null;

        }
        public static void UnloadTexture(int i)
        {
            Destroy(instance.LoadedTextures[i]);
            instance.LoadedTextures.Remove(i);
        }
        public static void LoadTexture(Resource r)
        {
            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(Resource.path + r.fileName));
            t.Apply();
            t.Compress(true);
            instance.LoadedTextures.Add(r.ID, t);

        }


        public Dictionary<int, Resource> unloadedResources;
        public List<Resource> FailedLoadResources;
        public List<Resource> toDownload;
        public Dictionary<int, Mesh> LoadedMeshes;
        public Dictionary<int, Texture2D> LoadedTextures;
        public Dictionary<int, AudioClip> LoadedAudio;
        private string LabelText;
        private enum VersionCheckStatus { Unchecked, UpToDate, OutDated, Fail, NewerThanOnline }
        private enum LoadingState { CheckingFiles, Downloading, Loading, Done}
        private LoadingState loadingState;
        private VersionCheckStatus checkStatus = VersionCheckStatus.Unchecked;
        private string OnlineVersion;
        private string MOTD;
        private Vector2 MOTDoffset;
        public static bool InMainMenu;
        public bool FinishedLoading = false;
        private WWW download;
        private bool IgnoreErrors;
        private bool ShowMOTD=true;
        private void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                if (string.IsNullOrEmpty(Resource.path))
                {
                    Resource.path = Application.dataPath + "/COTF Files/";
                }

            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
            IgnoreErrors = false;
            FinishedLoading = false;
            unloadedResources = new Dictionary<int, Resource>();
            // unloadedResources = new List<Resource>();
            FillResources();
            LoadedMeshes = new Dictionary<int, Mesh>();
            LoadedTextures = new Dictionary<int, Texture2D>();
            LoadedAudio = new Dictionary<int, AudioClip>();
            toDownload = new List<Resource>();
            FailedLoadResources = new List<Resource>();
            StartCoroutine(FileVerification());
            StartCoroutine(VersionCheck());
            StartCoroutine(DownloadMotd());

        }

        private int DownloadCount;

        private IEnumerator VersionCheck()
        {
            WWW ModapiWebsite = new WWW("https://modapi.survivetheforest.net/mod/101/champions-of-the-forest");
            yield return ModapiWebsite;
            if (string.IsNullOrEmpty(ModapiWebsite.error) && !string.IsNullOrEmpty(ModapiWebsite.text))
            {
                Regex regex1 = new Regex(@"Version+\W+([0-9.]+)");
                Match match1 = regex1.Match(ModapiWebsite.text);
                if (match1.Success)
                {
                    Regex regex2 = new Regex(@"([0-9.]+)");
                    Match match2 = regex2.Match(match1.Value);
                    if (match2.Success)
                    {
                        OnlineVersion = match2.Value;
                        if (ModSettings.Version == OnlineVersion)
                        {
                            checkStatus = VersionCheckStatus.UpToDate;
                        }
                        else if (CompareVersion(OnlineVersion) == Status.Outdated)
                        {
                            checkStatus = VersionCheckStatus.OutDated;
                        }
                        else
                        {
                            checkStatus = VersionCheckStatus.NewerThanOnline;
                        }
                        yield break;
                    }
                }
            }
            if (checkStatus == VersionCheckStatus.Unchecked)
            {
                checkStatus = VersionCheckStatus.Fail;
            }

        }

        private IEnumerator DownloadMotd()
        {
            ///WWW motdWebsite = new WWW("https://textuploader.com/1avou/raw");
            WWW motdWebsite = new WWW("https://docs.google.com/document/export?format=txt&id=1tq7scNmg0_CAzg0TfOhfq737ugaoCw3Idr-0esbKlhE&token=AC4w5Vgy9AG6mRMGIoA_NgkcxmFpPmmVUA%3A1548265532959&ouid=105695979354176851391&includes_info_params=true");
            yield return motdWebsite;
            MOTD = motdWebsite.text;
        }

        public enum Status { TheSame, Outdated, Newer }

        public static Status CompareVersion(string s1)
        {
            int i = 0;
            int a = 0;
            string val = "";
            int[] values1 = new int[4];
            int[] values2 = new int[4];

            //filling values1
            while (i < s1.Length)
            {
                if (s1[i] != '.')
                {
                    val += s1[i];
                }
                else
                {
                    values1[a] = int.Parse(val);
                    val = "";
                    a++;

                }
                i++;
            }
            if (val != "")
            {
                values1[a] = int.Parse(val);
            }
            val = "";
            a = 0;
            i = 0;


            while (i < ModSettings.Version.Length)
            {
                if (ModSettings.Version[i] != '.')
                {
                    val += ModSettings.Version[i];
                }
                else
                {
                    values2[a] = int.Parse(val);
                    val = "";
                    a++;

                }
                i++;
            }
            if (val != "")
            {
                values2[a] = int.Parse(val);
            }
            for (i = 0; i < 4; i++)
            {
                if (values1[i] > values2[i])
                {
                    return Status.Outdated;
                }
                else if (values1[i] < values2[i])
                {
                    return Status.Newer;
                }
            }
            return Status.TheSame;
        }

        private int CheckedFileNumber;
        private int DownloadedFileNumber;
        private int LoadedFileNumber;
        private IEnumerator FileVerification()
        {
            LabelText = "";
            loadingState = LoadingState.CheckingFiles;
            CheckedFileNumber = 0;
            DownloadedFileNumber = 0;
            LoadedFileNumber = 0;

            if (DirExists())
            {
                bool DeleteCurrentFiles = false;
                if (ModSettings.RequiresNewFiles)
                {
                    if (File.Exists(Resource.path + "VERSION.txt"))
                    {
                        string versiontext = File.ReadAllText(Resource.path + "VERSION.txt");
                        if (CompareVersion(versiontext) == Status.Outdated)
                        {
                            DeleteCurrentFiles = true;
                        }
                    }
                    else
                    {
                        DeleteCurrentFiles = true;
                    }
                }
                File.WriteAllText(Resource.path + "VERSION.txt", ModSettings.Version);
                foreach (Resource resource in unloadedResources.Values)
                {
                    if (File.Exists(Resource.path + resource.fileName))
                    {
                        if (DeleteCurrentFiles && ModSettings.outdatedFiles.Contains(resource.ID))
                        {
                            LabelText = "File " + resource.fileName + " is mared as outdated, deleting and redownloading.";
                            File.Delete(Resource.path + resource.fileName);
                            toDownload.Add(resource);
                            yield return new WaitForEndOfFrame();

                        }
                    }
                    else
                    {
                        LabelText = "File " + resource.fileName + " is missing, downloading.";
                        toDownload.Add(resource);
                        yield return new WaitForEndOfFrame();

                    }
                    CheckedFileNumber++;
                }
            }


            loadingState = LoadingState.Downloading;

            DownloadCount = toDownload.Count;

            foreach (Resource resource in toDownload)
            {
                LabelText = "Downloading " + resource.fileName;

                WWW www = new WWW(Resource.url + resource.fileName);
                download = www;
                yield return www;
                if (www.isDone)
                {
                    File.WriteAllBytes(Resource.path + resource.fileName, www.bytes);
                }
                else
                {
                    ModAPI.Log.Write(resource.fileName + " - Error with downloading a file " + www.error);
                }
                download = null;
                DownloadedFileNumber++;
                yield return null;
            }
            loadingState = LoadingState.Loading;
            yield return null;
            yield return null;
            foreach (Resource resource in unloadedResources.Values)
            {

                LabelText = "Loading " + resource.fileName;

                switch (resource.type)
                {
                    case Resource.ResourceType.Texture:

                        byte[] data = File.ReadAllBytes(Resource.path + resource.fileName);

                        if ((data[0] == 137 && data[1] == 80 && data[2] == 78 && data[3] == 71 && data[4] == 13 && data[5] == 10 && data[6] == 26 && data[7] == 10) ||
                            (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF))
                        {
                            Texture2D t = new Texture2D(1, 1, TextureFormat.RGBA32, true, true);
                            t.LoadImage(data);
                            t.Apply();
                            if (resource.CompressTexture)
                            {
                                t.Compress(true);
                            }
                            LoadedTextures.Add(resource.ID, t);
                        }
                        else
                        {
                            ModAPI.Log.Write("Missing texture " + resource.fileName);
                            FailedLoadResources.Add(resource);
                        }
                        break;
                    case Resource.ResourceType.Mesh:
                        Mesh mesh = Core.ReadMeshFromOBJ(Resource.path + resource.fileName);
                        if (mesh == null)
                        {
                            ModAPI.Log.Write("Missing mesh " + resource.fileName);
                            FailedLoadResources.Add(resource);
                        }
                        else
                        {
                            LoadedMeshes.Add(resource.ID, mesh);
                        }
                        break;
                    case Resource.ResourceType.Audio:
                        WWW audioWWW = new WWW("file://" + Resource.path + resource.fileName);
                        yield return audioWWW;

                        AudioClip clip = audioWWW.GetAudioClip(false, false);
                        if(clip != null)
                        {
                            LoadedAudio.Add(resource.ID, clip);
                        }
                        else
                        {
                            ModAPI.Log.Write("Missing audio " + resource.fileName);
                            FailedLoadResources.Add(resource);
                        }
                        break;
                    case Resource.ResourceType.Text:
                        
                        break;
                }
                LoadedFileNumber++;

                yield return null;
            }
            loadingState = LoadingState.Done;
            toDownload.Clear();

            yield return new WaitForSeconds(1f);
            FinishedLoading = true;
        }

        private void AttemptRedownload()
        {
            IgnoreErrors = false;
            FinishedLoading = false;
            toDownload.Clear();
            LoadedMeshes.Clear();
            LoadedTextures.Clear();

            foreach (Resource resource in FailedLoadResources)
            {
                if (File.Exists(Resource.path + resource.fileName))
                {
                    File.Delete(Resource.path + resource.fileName);
                    //toDownload.Add(resource);
                }

            }
            FailedLoadResources.Clear();
            StartCoroutine(FileVerification());

        }

        
        private void OnGUI()
        {
          



            if (!InMainMenu)
            {
                return;
            }

            if (!FinishedLoading)
            {
                float rr = (float)Screen.height / 1080;
                GUI.color = Color.black;
                Rect BGR = new Rect(0, 0, Screen.width, Screen.height);
                GUI.DrawTexture(BGR, Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(new Rect(0, 30 * rr, Screen.width, 60 * rr), "Please wait while Champions of the Forest is loading.", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic, fontSize = (int)(30 * rr), alignment = TextAnchor.UpperCenter });
                GUIStyle skin = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = (int)(30 * rr),
                    alignment = TextAnchor.MiddleCenter
                };
                switch (loadingState)
                {
                    case LoadingState.CheckingFiles:
                        GUI.Label(new Rect(0, 100 * rr, Screen.width, 300 * rr), "Step (1 of 3)\nChecking for existing files.", new GUIStyle(GUI.skin.label) { fontSize = (int)(55 * rr), alignment = TextAnchor.UpperCenter });
                        Rect pgBar = new Rect(Screen.width / 2 - 300 * rr, 600 * rr, 600 * rr, 50 * rr);
                        Rect prog = new Rect(pgBar);
                        pgBar.width *= (float)CheckedFileNumber / unloadedResources.Count;
                        GUI.color = Color.gray;
                        GUI.DrawTexture(prog, Texture2D.whiteTexture);
                        GUI.color = Color.white;
                        GUI.DrawTexture(pgBar, Texture2D.whiteTexture);
                        GUI.color = Color.black;
                        GUI.Label(prog, CheckedFileNumber + "/" + unloadedResources.Count, skin);
                        break;

                    case LoadingState.Downloading:
                        GUI.Label(new Rect(0, 100 * rr, Screen.width, 300 * rr), "Step (2 of 3)\nDownloading missing files.", new GUIStyle(GUI.skin.label) { fontSize = (int)(55 * rr), alignment = TextAnchor.UpperCenter });
                        Rect pgBar1 = new Rect(Screen.width / 2 - 300 * rr, 600 * rr, 600 * rr, 50 * rr);
                        Rect prog1 = new Rect(pgBar1);
                        pgBar1.width *= (float)DownloadedFileNumber / DownloadCount;
                        GUI.color = Color.gray;
                        GUI.DrawTexture(prog1, Texture2D.whiteTexture);
                        GUI.color = Color.white;
                        GUI.DrawTexture(pgBar1, Texture2D.whiteTexture);
                        GUI.color = Color.black;
                        GUI.Label(prog1, DownloadedFileNumber + "/" + DownloadCount, skin);
                        if (download != null)
                        {
                            Rect downloadRectBG = new Rect(prog1);
                            downloadRectBG.y += 100 * rr;
                            Rect downloadRect = new Rect(downloadRectBG);
                            downloadRect.width *= download.progress;
                            GUI.color = Color.gray;
                            GUI.DrawTexture(downloadRectBG, Texture2D.whiteTexture);
                            GUI.color = Color.white;
                            GUI.DrawTexture(downloadRect, Texture2D.whiteTexture);
                            GUI.color = Color.black;
                            GUI.Label(prog1, download.progress * 100 + "%\tDownloaded " + (float)download.bytesDownloaded / 1000 + " KB", skin);
                        }
                        GUI.color = Color.white;
                        break;

                    case LoadingState.Loading:
                        GUI.Label(new Rect(0, 100 * rr, Screen.width, 300 * rr), "Step (3 of 3)\nLoading assets.", new GUIStyle(GUI.skin.label) { fontSize = (int)(55 * rr), alignment = TextAnchor.UpperCenter });
                        Rect pgBar2 = new Rect(Screen.width / 2 - 300 * rr, 600 * rr, 600 * rr, 50 * rr);
                        Rect prog2 = new Rect(pgBar2);
                        pgBar2.width *= (float)LoadedFileNumber / unloadedResources.Count;
                        GUI.color = Color.gray;
                        GUI.DrawTexture(prog2, Texture2D.whiteTexture);
                        GUI.color = Color.white;
                        GUI.DrawTexture(pgBar2, Texture2D.whiteTexture);
                        GUI.color = Color.black;
                        GUI.Label(prog2, LoadedFileNumber + "/" + unloadedResources.Count, skin);
                        break;

                    case LoadingState.Done:
                        GUI.Label(new Rect(0, 100 * rr, Screen.width, 300 * rr), "Done!\n Enjoy", new GUIStyle(GUI.skin.label) { fontSize = (int)(55 * rr), alignment = TextAnchor.UpperCenter });
                        break;
                }
                GUI.color = Color.white;
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = (int)(25 * rr),
                    alignment = TextAnchor.LowerLeft,
                };
                GUI.Label(new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height), LabelText, style);
            }
            else
            {
                if (FailedLoadResources.Count > 0 && !IgnoreErrors)
                {
                    float rr = (float)Screen.height / 1080;
                    GUI.color = Color.black;
                    Rect BGR = new Rect(0, 0, Screen.width, Screen.height);
                    GUI.DrawTexture(BGR, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    string text = "OH NO!\nThere were errors with loading resources for COTF!\n\nUnable to load those assets:\n";
                    foreach (Resource item in FailedLoadResources)
                    {
                        text += item.fileName + ",\t";
                    }
                    text += "\n\nWhat would you like to do now?";
                    GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = (int)(30 * rr), alignment = TextAnchor.UpperCenter, wordWrap = true };
                    Rect labelRect = new Rect(0, 100*rr, Screen.width, style.CalcHeight(new GUIContent(text),Screen.width));
                    GUI.Label(labelRect, text, style);
                    float y = labelRect.yMax;
                    y = Mathf.Clamp(y, 0, Screen.height - 100 * rr);
                    Rect bt1 = new Rect(Screen.width - 300 * rr, y, 300 * rr, 100 * rr);
                    Rect bt2 = new Rect(Screen.width - 600 * rr, y, 300 * rr, 100 * rr);
                    GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = (int)(30 * rr), wordWrap = true, fontStyle = FontStyle.BoldAndItalic };
                    if (GUI.Button(bt1, "IGNORE ERRORS", btnStyle))
                    {
                        IgnoreErrors = true;
                        FailedLoadResources = null;
                    }
                    if (GUI.Button(bt2, "ATTEMPT REDOWNLOAD", btnStyle))
                    {
                        AttemptRedownload();
                    }

                }



                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

                GUIStyle versionStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight, fontSize = 34, fontStyle = FontStyle.Italic };
                switch (checkStatus)
                {
                    case VersionCheckStatus.Unchecked:
                        GUI.color = Color.gray;
                        GUILayout.Label("Checking for updated version...", versionStyle);
                        break;
                    case VersionCheckStatus.UpToDate:
                        GUI.color = Color.green;
                        GUILayout.Label("Champions of The Forest is up to date.", versionStyle);

                        break;
                    case VersionCheckStatus.OutDated:
                        GUI.color = Color.red;
                        GUILayout.Label("Champions of The Forest is outdated! \n Installed " + ModSettings.Version + ";  Newest " + OnlineVersion, versionStyle);

                        break;
                    case VersionCheckStatus.Fail:
                        GUI.color = Color.gray;
                        GUILayout.Label("Failed to get update info", versionStyle);
                        break;
                    case VersionCheckStatus.NewerThanOnline:
                        GUI.color = Color.yellow;
                        GUILayout.Label("You're using a version newer than uploaded to ModAPI", versionStyle);
                        break;
                }
                GUI.color = Color.white;
GUILayout.EndArea();

                if (MOTD != "")
                {
                    GUIStyle title = new GUIStyle(GUI.skin.button) { fontSize = 34, fontStyle = FontStyle.Bold, richText = true };
                    GUIStyle motdstyle = new GUIStyle(GUI.skin.box) { fontSize = 22, fontStyle = FontStyle.Normal, alignment = TextAnchor.UpperCenter, wordWrap = true, richText = true };

                    Rect r1 = new Rect(new Rect(Screen.width * 2 / 3, 120, Screen.width / 3, 50));
                    if (GUI.Button(r1, "◄NEWS►", title)) ShowMOTD = !ShowMOTD;
                    if (ShowMOTD)
                    {
                        float height = motdstyle.CalcHeight(new GUIContent(MOTD), r1.width);
                        Rect r2 = new Rect(r1.x, r1.y + 40, r1.width, Screen.height - 200);
                        GUILayout.BeginArea(r2);
                        MOTDoffset = GUILayout.BeginScrollView(MOTDoffset);

                        GUILayout.Label(MOTD, motdstyle);

                        GUILayout.EndScrollView();
                        GUILayout.EndArea();
                    }
                }
            }
        }

        private bool DirExists()
        {
            if (!Directory.Exists(Resource.path))
            {
                LabelText = LabelText + " \n NO DIRECTORY FOUND, DOWNLOADING \n Please wait... ";
                Directory.CreateDirectory(Resource.path);
                foreach (Resource resource in unloadedResources.Values)
                {
                    toDownload.Add(resource);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void FillResources()
        {
            new Resource(1, "wheel.png");
            new Resource(2, "wheelOn.png");
            new Resource(5, "SpellBG.png");
            new Resource(6, "SpellFrame.png");
            new Resource(8, "CoolDownFill.png");
            new Resource(12, "Item1SlotBg.png");
            new Resource(13, "Item1Frame.png");
            new Resource(15, "ProgressBack.png");
            new Resource(16, "ProgressFill.png");
            new Resource(17, "ProgressFront.png");
            new Resource(18, "CombatTimer.png");
            new Resource(20, "BlackHoleTex.png");
            new Resource(21, "BlackHole.obj");
            new Resource(22, "Leaf.png");
            new Resource(24, "SmallCircle.png");
            new Resource(25, "Row.png");
            new Resource(26, "snowflake.png");
            new Resource(27, "Background.png");
            new Resource(28, "HorizontalListItem.png");
            new Resource(30, "Space.png");
            new Resource(40, "amulet.obj");
            new Resource(41, "Glove.obj");
            new Resource(42, "jacket.obj");
            new Resource(43, "ring.obj");
            new Resource(44, "shoe.obj");
            new Resource(45, "Bracer.obj");
            //new Resource(46, "Boots1.obj");
            //new Resource(47, "Boots2.obj");
            new Resource(48, "helmet_armet_2.obj");
            new Resource(49, "Shield.obj");
            new Resource(50, "Pants.obj");
            new Resource(51, "Sword.obj");
            new Resource(52, "HeavySword.obj");
            new Resource(53, "Shoulder.obj");
            //new Resource(59, "SwordMetalic.png");
            new Resource(60, "SwordTexture.png");
            new Resource(61, "SwordColor.png");
            new Resource(62, "SwordEmissive.png");
            new Resource(64, "SwordNormal.png");
            new Resource(65, "SwordRoughness.png");
            new Resource(66, "SwordAmbientOcculusion.png");
            new Resource(67, "ManyParticles.png");
            new Resource(68, "InverseSphere.obj");
            new Resource(69, "ChainPart.obj");
            new Resource(70, "Spike.obj");
            new Resource(71, "particle.png");
            new Resource(72, "Melee.jpg");
            new Resource(73, "Magic.jpg");
            new Resource(74, "Ranged.jpg");
            new Resource(75, "Defensive.jpg");
            new Resource(76, "Utility.jpg");
            new Resource(77, "Support.jpg");
            new Resource(78, "Background1.jpg");
            new Resource(82, "PerkNode1.png");
            new Resource(81, "PerkNode2.png");
            new Resource(83, "PerkNode3.png");
            new Resource(84, "PerkNode4.png");
            new Resource(85, "ItemBoots.png");
            new Resource(86, "ItemGloves.png");
            new Resource(87, "ItemPants.png");
            new Resource(88, "ItemGreatSword.png");
            new Resource(89, "ItemLongSword.png");
            new Resource(90, "ItemRing.png");
            new Resource(91, "ItemHelmet.png");
            new Resource(92, "ItemBoots1.png");
            new Resource(93, "ItemBracer.png");
            new Resource(94, "ItemBracer2.png");
            new Resource(95, "ItemShoulder.png");
            new Resource(96, "ItemChest.png");
            new Resource(98, "ItemQuiver.png");
            new Resource(99, "ItemShield.png");
            new Resource(100, "ItemScarf.png");
            new Resource(101, "ItemAmulet.png");
            new Resource(102, "Heart.obj");
            new Resource(103, "HeartTexture.png");
            new Resource(104, "HeartNormal.png");
            new Resource(105, "ItemHeart.png");
            new Resource(106, "Corner.png");
            new Resource(107, "BeamParticleHorizontal.png");
            new Resource(108, "Hammer.obj");
            new Resource(109, "ItemHammer.png");
            new Resource(110, "ItemScroll.png");



            }
    }

}
