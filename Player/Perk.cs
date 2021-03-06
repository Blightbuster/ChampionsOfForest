﻿using System.Collections.Generic;
using UnityEngine;

namespace ChampionsOfForest.Player
{
    public class Perk
    {
        public static List<Perk> AllPerks = new List<Perk>();
        public static void FillPerkList()
        {
            AllPerks.Clear();
            //Tier one basic upgrades that allow stats to take effect
            new Perk("Stronger Hits", "Every point of STRENGHT increases MEELE DAMAGE by 0.5%.", -1, 1.5f, 0, PerkCategory.MeleeOffense, 1, 1, () => ModdedPlayer.instance.DamagePerStrenght += 0.005f, () => ModdedPlayer.instance.DamagePerStrenght -= 0.005f);
            new Perk("Stronger Spells", "Every point of INTELLIGENCE increases SPELL DAMAGE by 0.5%.", -1, 1.5f, 0, PerkCategory.MagicOffense, 1, 1, () => ModdedPlayer.instance.SpellDamageperInt += 0.005f, () => ModdedPlayer.instance.SpellDamageperInt -= 0.005f);
            new Perk("Stronger Projectiles", "Every point of AGILITY increases RANGED DAMAGE by 0.5%.", -1, 1.5f, 0, PerkCategory.RangedOffense, 1, 1, () => ModdedPlayer.instance.SpellDamageperInt += 0.005f, () => ModdedPlayer.instance.SpellDamageperInt -= 0.005f);
            new Perk("Stamina Recovery", "Every point of INTELLIGENCE increases stamina recover by 0.75%.", -1, 1.5f, 0, PerkCategory.Utility, 1, 1, () => ModdedPlayer.instance.EnergyRegenPerInt += 0.0075f, () => ModdedPlayer.instance.EnergyRegenPerInt -= 0.0075f);
            new Perk("More Stamina", "Every point of AGILITY increases max stamina by 0.5", -1, -1.5f, 0, PerkCategory.Utility, 1, 1, () => ModdedPlayer.instance.EnergyPerAgility += 0.5f, () => ModdedPlayer.instance.EnergyPerAgility -= 0.5f);
            new Perk("More Health", "Every point of VITALITY increases max health by 1", -1, 1.5f, 0, PerkCategory.Defense, 1, 1, () => ModdedPlayer.instance.HealthPerVitality += 1, () => ModdedPlayer.instance.HealthPerVitality -= 1f);
            new Perk("More Healing", "Increases healing by 10%", -1, -1.5f, 0, PerkCategory.Support, 1, 1, () => ModdedPlayer.instance.HealingMultipier *= 1.10f, () => ModdedPlayer.instance.HealingMultipier /= 1.10f);
            //new Perk()
            //{
            //    ApplyMethods = () => ,
            //    DisableMethods = () => ,
            //    Category = PerkCategory.MeleeOffense,
            //    Icon = Texture2D.whiteTexture,
            //    InheritIDs = new int[] { -1 },
            //    LevelRequirement = 1,
            //    PointsToBuy = 1,
            //    Size = 1,
            //    PosOffsetX = 2f,
            //    PosOffsetY = 0.5f,
            //    Name = "NAME",
            //    Description = "DESCRIPTION", 
            //    TextureVariation = 0, //0 or 1
            //    Endless = false,
            //}; 
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.HungerRate *= 0.96f,
                DisableMethods = () => ModdedPlayer.instance.HungerRate /= 0.96f,
                Category = PerkCategory.Utility,
                Icon = null,
                InheritIDs = new int[] { 4 },
                LevelRequirement = 5,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = -2f,
                PosOffsetY = 0.75f,
                Name = "Metabolism",
                Description = "Decreases hunger rate by 4%.",
                TextureVariation = 1, //0 or 1
                Endless = true,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.ThirstRate *= 0.96f,
                DisableMethods = () => ModdedPlayer.instance.ThirstRate /= 0.96f,
                Category = PerkCategory.Utility,
                Icon = null,
                InheritIDs = new int[] { 4 },
                LevelRequirement = 5,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = -2f,
                PosOffsetY = -0.75f,
                Name = "Water Conservation",
                Description = "Decreases thirst rate by 4%.",
                TextureVariation = 1, //0 or 1
                Endless = true,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.MeleeDamageBonus += 5,
                DisableMethods = () => ModdedPlayer.instance.MeleeDamageBonus -= 5,
                Category = PerkCategory.MeleeOffense,
                Icon = null,
                InheritIDs = new int[] { 0 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2f,
                PosOffsetY = 0.75f,
                Name = "Damage",
                Description = "Increases melee damage by 5",
                TextureVariation = 0, //0 or 1
                Endless = true,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.MeleeDamageAmplifier += 0.02f,
                DisableMethods = () => ModdedPlayer.instance.MeleeDamageBonus -= 0.02f,
                Category = PerkCategory.MeleeOffense,
                Icon = null,
                InheritIDs = new int[] { 0 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2.5f,
                PosOffsetY = 0f,
                Name = "Damage",
                Description = "Increases melee damage by 2%",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.strenght += 10,
                DisableMethods = () => ModdedPlayer.instance.strenght -= 10,
                Category = PerkCategory.MeleeOffense,
                Icon = null,
                InheritIDs = new int[] { 0 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2f,
                PosOffsetY = -0.75f,
                Name = "Strenght",
                Description = "Increases strenght by 10",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.RangedDamageBonus += 5,
                DisableMethods = () => ModdedPlayer.instance.MeleeDamageBonus -= 5,
                Category = PerkCategory.RangedOffense,
                Icon = null,
                InheritIDs = new int[] { 2 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2f,
                PosOffsetY = -0.75f,
                Name = "Damage",
                Description = "Increases projectile damage by 5",
                TextureVariation = 0, //0 or 1
                Endless = true,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.ProjectileSizeRatio += 0.05f,
                DisableMethods = () => ModdedPlayer.instance.ProjectileSizeRatio -= 0.05f,
                Category = PerkCategory.RangedOffense,
                Icon = null,
                InheritIDs = new int[] { 2 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2f,
                PosOffsetY = 0.75f,
                Name = "Size",
                Description = "Increases projectile size by 5%",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };
            new Perk()
            {
                ApplyMethods = () => ModdedPlayer.instance.ProjectileSpeedRatio += 0.05f,
                DisableMethods = () => ModdedPlayer.instance.ProjectileSpeedRatio -= 0.05f,
                Category = PerkCategory.RangedOffense,
                Icon = null,
                InheritIDs = new int[] { 2 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2.5f,
                PosOffsetY = 0f,
                Name = "Speed",
                Description = "Increases projectile speed by 5%",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };
            new Perk()
            {
                ApplyMethods = () => ItemDataBase.AddPercentage(ref ModdedPlayer.instance.SpellCostToStamina, 0.05f),
                DisableMethods = () => ItemDataBase.RemovePercentage(ref ModdedPlayer.instance.SpellCostToStamina, 0.05f),
                Category = PerkCategory.MagicOffense,
                Icon = null,
                InheritIDs = new int[] { 1 },
                LevelRequirement = 4,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2.5f,
                PosOffsetY = 0f,
                Name = "Transmutation",
                Description = "5% of the spell cost is now taxed from stamina instead of energy.",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };
            new Perk()
            {
                ApplyMethods = () => ItemDataBase.AddPercentage(ref ModdedPlayer.instance.SpellCostRatio, 0.02f),
                DisableMethods = () => ItemDataBase.RemovePercentage(ref ModdedPlayer.instance.SpellCostRatio, 0.02f),
                Category = PerkCategory.MagicOffense,
                Icon = null,
                InheritIDs = new int[] { 15 },
                LevelRequirement = 7,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2.5f,
                PosOffsetY = 0.75f,
                Name = "Resource Cost Reduction",
                Description = "Spell costs are reduced by 5",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };
            //0.7 perks
            new Perk()
            {
                ApplyMethods = () => { ItemDataBase.AddPercentage(ref ModdedPlayer.instance.DamageReductionPerks, 0.20f); ItemDataBase.RemovePercentage(ref ModdedPlayer.instance.DamageOutputMultPerks, 0.20f); },
                DisableMethods = () =>{ ItemDataBase.RemovePercentage(ref ModdedPlayer.instance.DamageReductionPerks, 0.20f); ItemDataBase.AddPercentage(ref ModdedPlayer.instance.DamageOutputMultPerks, 0.20f); },
                Category = PerkCategory.Defense,
                Icon = Texture2D.whiteTexture,
                InheritIDs = new int[] { 5 },
                LevelRequirement = 8,
                PointsToBuy = 1,
                Size = 1,
                PosOffsetX = 2f,
                PosOffsetY = 0.75f,
                Name = "Undestructable",
                Description = "Decreases all damage taken and decreases all damage dealt by 20%",
                TextureVariation = 0, //0 or 1
                Endless = false,
            };































            foreach (Perk item in AllPerks)
            {
                ModAPI.Log.Write("[" + item.ID + "]" + item.Name);
            }
        }

        public int ID;
        public int[] InheritIDs;
        public int PointsToBuy = 1;
        public int LevelRequirement;

        public bool IsBought = false;
        public bool Applied = false;

        public delegate void OnApply();
        public delegate void OnDisable();
        public OnApply ApplyMethods;
        public OnDisable DisableMethods;

        public string Name;
        public string Description;

        public Texture2D Icon;

        public bool Endless = false;
        public int ApplyAmount;

        public int TextureVariation = 0;
        public float Size = 1;
        public float PosOffsetX;
        public float PosOffsetY;
        public enum PerkCategory { MeleeOffense, RangedOffense, MagicOffense, Defense, Support, Utility }
        public PerkCategory Category;

        public Perk(string name, string description, int[] inheritIDs, float x, float y, PerkCategory category, float size, int levelRequirement, OnApply applyMethods, OnDisable disableMethods)
        {
            Name = name;
            Description = description;
            InheritIDs = inheritIDs;
            Category = category;
            Size = size;
            Endless = false;
            PosOffsetX = x;
            PosOffsetY = y;
            LevelRequirement = levelRequirement;
            ApplyMethods = applyMethods;
            DisableMethods = disableMethods;
            ID = AllPerks.Count;
            Applied = false;
            AllPerks.Add(this);


        }
        public Perk()
        {
            Applied = false;

            ID = AllPerks.Count;
            AllPerks.Add(this);

        }
        public Perk(string name, string description, int inheritIDs, float x, float y, PerkCategory category, float size, int levelRequirement, OnApply applyMethods, OnDisable disableMethods)
        {
            Name = name;
            Description = description;
            InheritIDs = new int[] { inheritIDs };
            PosOffsetX = x;
            PosOffsetY = y;
            Category = category;
            Size = size;
            Endless = false;
            LevelRequirement = levelRequirement;
            ApplyMethods = applyMethods;
            DisableMethods = disableMethods;
            ID = AllPerks.Count;
            Applied = false;
            AllPerks.Add(this);

        }
    }


}
