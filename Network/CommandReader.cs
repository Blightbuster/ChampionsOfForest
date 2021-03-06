﻿using ChampionsOfForest.Enemies.EnemyAbilities;
using ChampionsOfForest.Player;
using System;
using TheForest.Utils;
using UnityEngine;

namespace ChampionsOfForest.Network
{
    public class CommandReader
    {
        private static char[] ch;
        private static string parseval;
        private static int i;



        public static void OnCommand(string s)
        {
            try
            {
                if (s.StartsWith("AB"))     //ask the host to send the command to set the difficulty for clinet
                {
                    if (GameSetup.IsMpServer && ModSettings.DifficultyChoosen)
                    {
                        string answer = "AA" + (int)ModSettings.difficulty + ";";
                        if (ModSettings.FriendlyFire)
                        {
                            answer += "t;";
                        }
                        else
                        {
                            answer += "f;";
                        }

                        Network.NetworkManager.SendLine(answer, Network.NetworkManager.Target.Clinets);
                    }
                }
                else if (s.StartsWith("AA"))    //answer for the what is the difficulty query
                {
                    if (ModSettings.DifficultyChoosen || !GameSetup.IsMpClient || ModSettings.IsDedicated)
                    {
                        return;
                    }
                    i = 2;
                    ch = s.ToCharArray();
                    int index = int.Parse(Read());
                    Array values = Enum.GetValues(typeof(ModSettings.Difficulty));
                    ModSettings.difficulty = (ModSettings.Difficulty)values.GetValue(index);
                    ModSettings.DifficultyChoosen = true;
                    LocalPlayer.FpCharacter.UnLockView();
                    LocalPlayer.FpCharacter.MovementLocked = false;
                }
                else if (s.StartsWith("SC"))    //spell cast
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    int spellid = int.Parse(Read());
                    if (spellid == 1)
                    {
                        Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                        BlackHole.CreateBlackHole(pos, ReadBool(), float.Parse(Read()), float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    }
                    else if (spellid == 2)
                    {
                        Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                        HealingDome.CreateHealingDome(pos, float.Parse(Read()), float.Parse(Read()), ReadBool(), float.Parse(Read()));
                    }
                    else if (spellid == 3)
                    {
                        Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                        DarkBeam.Create(pos, ReadBool(), float.Parse(Read()), float.Parse(Read()), float.Parse(Read()), float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    }
                }
                else if (s.StartsWith("RI"))    //remove item
                {
                    i = 2;
                    ch = s.ToCharArray();
                    int id = int.Parse(Read());
                    PickUpManager.RemovePickup(id);

                }
                else if (s.StartsWith("CI"))    //create item
                {
                    i = 2;
                    ch = s.ToCharArray();
                    Item item = new Item(ItemDataBase.ItemBases[int.Parse(Read())], 1, 0, false);   //reading first value, id
                    int id = int.Parse(Read());
                    item.level = int.Parse(Read());
                    int amount = int.Parse(Read());
                    Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    while (i < ch.Length)
                    {
                        ItemStat stat = new ItemStat(ItemDataBase.Stats[int.Parse(Read())])
                        {
                            Amount = float.Parse(Read())
                        };
                        item.Stats.Add(stat);
                    }
                    PickUpManager.SpawnPickUp(item, pos, amount, id);
                }
                else if (s.StartsWith("EE"))       //host has been asked to share info on enemy
                {
                    if (!GameSetup.IsMpClient)
                    {
                        i = 2;
                        ch = s.ToCharArray();
                        ulong packed = ulong.Parse(Read());
                        if (EnemyManager.hostDictionary.ContainsKey(packed))
                        {
                            EnemyProgression ep = EnemyManager.hostDictionary[packed];
                            parseval = "EA" + packed + ";" + ep.EnemyName + ";" + ep.Level + ";" + ep.Health + ";" + ep.MaxHealth + ";" + ep.Bounty + ";" + ep.Armor + ";" + ep.ArmorReduction + ";" + ep.SteadFest + ";" + ep.abilities.Count + ";";
                            foreach (EnemyProgression.Abilities item in ep.abilities)
                            {
                                parseval += (int)item + ";";
                            }
                            Network.NetworkManager.SendLine(parseval, Network.NetworkManager.Target.Everyone);
                        }


                    }
                }
                else if (s.StartsWith("ES"))       //enemy spell
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    int id = int.Parse(Read());
                    switch (id)
                    {
                        case 1: //snow aura
                            ulong packed = ulong.Parse(Read());
                            SnowAura sa = new GameObject("Snow").AddComponent<SnowAura>();
                            if (!EnemyManager.allboltEntities.ContainsKey(packed))
                            {
                                EnemyManager.GetAllEntities();
                            }

                            sa.followTarget = EnemyManager.allboltEntities[packed].transform;
                            break;
                        default:
                            break;
                    }
                }
                else if (s.StartsWith("PO"))    //poison Player
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    ulong playerPacked = ulong.Parse(Read());
                    if (ModReferences.ThisPlayerPacked == playerPacked)
                    {
                        int source = int.Parse(Read());
                        float amount = float.Parse(Read());
                        float duration = float.Parse(Read());

                        BuffDB.AddBuff(3, source, amount, duration);
                    }


                }
                else if (s.StartsWith("KX"))    //kill experience
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    long exp = long.Parse(Read());
                    ModAPI.Console.Write("Gained exp " + exp);
                    ModdedPlayer.instance.AddKillExperience(exp);
                }
                else if (s.StartsWith("EA"))       //host answered info about a enemy and the info is processed
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    if (GameSetup.IsMpClient)
                    {
                        i = 2;
                        ch = s.ToCharArray();
                        ulong packed = ulong.Parse(Read());
                        if (!EnemyManager.allboltEntities.ContainsKey(packed))
                        {
                            EnemyManager.GetAllEntities();
                        }
                        if (EnemyManager.allboltEntities.ContainsKey(packed))
                        {
                            BoltEntity entity = EnemyManager.allboltEntities[packed];
                            string name = Read();
                            int v1 = int.Parse(Read());
                            int v2 = int.Parse(Read());
                            int v3 = int.Parse(Read());
                            int v4 = int.Parse(Read());
                            int v5 = int.Parse(Read());
                            int v6 = int.Parse(Read());
                            float v7 = float.Parse(Read());
                            int lenght = int.Parse(Read());
                            int[] affixes = new int[lenght];
                            int id = 0;
                            while (i < ch.Length)
                            {
                                affixes[id] = int.Parse(Read());
                                id++;
                            }
                            if (EnemyManager.clinetProgressions.ContainsKey(entity))
                            {
                                ClinetEnemyProgression cp = EnemyManager.clinetProgressions[entity];
                                cp.creationTime = Time.time;
                                cp.Entity = entity;
                                cp.Level = v1;
                                cp.Health = v2;
                                cp.MaxHealth = v3;
                                cp.Armor = v5;
                                cp.ArmorReduction = v6;
                                cp.EnemyName = name;
                                cp.ExpBounty = v4;
                                cp.SteadFest = v7;
                                cp.Affixes = affixes;
                            }
                            else
                            {
                                new ClinetEnemyProgression(entity, name, v1, v2, v3, v4, v5, v6, v7, affixes);
                            }
                        }
                    }
                }
                else if (s.StartsWith("RO"))    //root the player
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    if (!ModdedPlayer.instance.RootImmune && !ModdedPlayer.instance.StunImmune)
                    {
                        i = 2;
                        ch = s.ToCharArray();
                        Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                        if ((LocalPlayer.Transform.position - pos).sqrMagnitude < 1200)
                        {
                            float duration = float.Parse(Read());
                            NetworkManager.SendLine("RE" + LocalPlayer.Transform.position.x + ";" + LocalPlayer.Transform.position.y + ";" + LocalPlayer.Transform.position.z + ";" + duration + ";", NetworkManager.Target.Everyone);
                            ModdedPlayer.instance.Root(duration);
                        }
                    }
                }
                else if (s.StartsWith("ST"))    //stun the player
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    if (!ModdedPlayer.instance.StunImmune)
                    {
                        i = 2;
                        ch = s.ToCharArray();
                        ulong packed = ulong.Parse(Read());
                        if (ModReferences.ThisPlayerPacked == packed)
                        {
                            float duration = float.Parse(Read());
                            ModdedPlayer.instance.Stun(duration);
                        }
                    }
                }
                else if (s.StartsWith("RE"))
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    float duration = float.Parse(Read());
                    RootSpell.Create(pos, duration);
                }
                else if (s.StartsWith("TR"))
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    float duration = float.Parse(Read());
                    float radius = float.Parse(Read());
                    TrapSphereSpell.Create(pos, radius, duration);

                }
                else if (s.StartsWith("LA"))
                {
                    i = 2;
                    ch = s.ToCharArray();
                    Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    Vector3 dir = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));

                    EnemyLaser.CreateLaser(pos, dir);
                }
                else if (s.StartsWith("MT"))
                {
                    i = 2;
                    ch = s.ToCharArray();
                    Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    Meteor.CreateEnemy(pos, int.Parse(Read()));
                }
                else if (s.StartsWith("RL"))
                {
                    if (s.Length > 2)
                    {
                        ModReferences.PlayerLevels.Clear();
                    }
                    NetworkManager.SendLine("AL" + ModReferences.ThisPlayerPacked + ";" + ModdedPlayer.instance.Level + ";", NetworkManager.Target.Everyone);
                }
                else if (s.StartsWith("AL"))
                {
                    i = 2;
                    ch = s.ToCharArray();
                    ulong packed = ulong.Parse(Read());
                    int level = int.Parse(Read());
                    if (ModReferences.PlayerLevels.ContainsKey(packed))
                    {
                        ModReferences.PlayerLevels[packed] = level;
                    }
                    else
                    {
                        ModReferences.PlayerLevels.Add(packed, level);
                    }
                }
                else if (s.StartsWith("EH"))    //enemy hitmarker
                {
                    if (ModSettings.IsDedicated)
                    {
                        return;
                    }

                    i = 2;
                    ch = s.ToCharArray();
                    int amount = int.Parse(Read());
                    Vector3 pos = new Vector3(float.Parse(Read()), float.Parse(Read()), float.Parse(Read()));
                    new MainMenu.HitMarker(amount, pos);

                }
                else if (s.StartsWith("AC"))    //slow Enemy
                {
                    if (GameSetup.IsMpServer || GameSetup.IsSinglePlayer)
                    {
                        i = 2;
                        ch = s.ToCharArray();
                        ulong id = ulong.Parse(Read());
                        float amount = float.Parse(Read());
                        float time = float.Parse(Read());
                        int src = int.Parse(Read());
                        EnemyManager.hostDictionary[id].Slow(src, amount, time);
                    }
                }
                else if (s.StartsWith("AD"))    //slow Enemy
                {
                    if (GameSetup.IsMpServer)
                    {
                        if (ModSettings.IsDedicated)
                        {
                            ItemDataBase.MagicFind = 1;
                        }
                        else
                        {
                            ItemDataBase.MagicFind = ModdedPlayer.instance.MagicFindMultipier;
                        }
                    }
                    else
                    {
                        Network.NetworkManager.SendLine("AE" + ModdedPlayer.instance.MagicFindMultipier + ";", Network.NetworkManager.Target.OnlyServer);

                    }

                }
                else if (s.StartsWith("AE"))    //slow Enemy
                {
                    if (GameSetup.IsMpServer)
                    {
                        i = 2;
                        ch = s.ToCharArray();
                        ItemDataBase.MagicFind *= float.Parse(Read());
                    }
                }

            }
            catch (Exception e)
            {

                ModAPI.Log.Write(e.ToString());
            }
        }
        private static string Read()
        {
            parseval = string.Empty;
            while (ch[i] != ';' && i < ch.Length)
            {
                parseval += ch[i];
                i++;
            }
            i++;
            return parseval;
        }
        private static bool ReadBool()
        {
            parseval = string.Empty;
            while (ch[i] != ';' && i < ch.Length)
            {
                parseval += ch[i];
                i++;
            }
            i++;
            if (parseval[0] == '1' || parseval[0] == 't')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
