﻿using Microsoft.Xna.Framework;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile;
using xTile.ObjectModel;
using SObject = StardewValley.Object;


namespace PyTK.Lua
{
    public class LuaUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static void log(string text)
        {
            Monitor.Log(text, LogLevel.Info);
        }

        public static int setCounter(string id, int value)
        {
            return counters(id, value, true);
        }

        public static int counters(string id, int value = 0, bool set = false)
        {
            if (!PyTKMod.saveData.Counters.ContainsKey(id))
                PyTKMod.saveData.Counters.Add(id, 0);

            int before = PyTKMod.saveData.Counters[id];

            if (!set)
                PyTKMod.saveData.Counters[id] += value;
            else
                PyTKMod.saveData.Counters[id] = value;

            int after = PyTKMod.saveData.Counters[id];

            int dif = after - before;
            if (dif != 0 && Game1.IsMultiplayer)
                PyTKMod.syncCounter(id, dif);

            return PyTKMod.saveData.Counters[id];
        }

        public static bool invertSwitch(string id)
        {
            id = "switch_" + id;
            return setCounter(id, counters(id) == 1 ? 0 : 1) == 1;
        }

        public static bool switches(string id, bool? value = null)
        {
            id = "switch_" + id;
            if (value.HasValue)
                setCounter(id, value.Value ? 1 : 0);

            return counters(id) == 1;
        }

        public static bool setMapProperty(Map map, string property, string value)
        {
                map.Properties[property] = value;
                return true;
        }

        public static bool setMapProperty(string locationName, string property, string value)
        {
            return setMapProperty(Game1.getLocationFromName(locationName).Map, property, value);
        }

        public static string getMapProperty(Map map, string property)
        {
            PropertyValue p = "";
            if (map.Properties.TryGetValue(property, out p))
            {
                return p.ToString();
            }
            return "";
        }

        public static GameLocation getLocation(string locationName)
        {
            return Game1.getLocationFromName(locationName);
        }

        public static string getMapProperty(string locationName, string property)
        {
            return getMapProperty(Game1.getLocationFromName(locationName).Map, property);
        }

        public static void updateWarps(string locationName)
        {
            updateWarps(Game1.getLocationFromName(locationName));
        }

        public static void updateWarps(GameLocation location)
        {
            location.warps.Clear();
            PropertyValue p = "";
            if (location.Map.Properties.TryGetValue("Warp", out p) && p != "")
                Helper.Reflection.GetMethod(location, "updateWarps").Invoke();
        }

        public static bool setGameValue(string field, object value, int delay = 0, object root = null)
        {
            List<string> tree = new List<string>(field.Split('.'));
            FieldInfo fieldInfo = null;
                
            object currentBranch = root == null ? Game1.game1 : root;

            fieldInfo = typeof(Game1).GetField(tree[0], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            tree.Remove(tree[0]);

            if (tree.Count > 0)
                foreach (string branch in tree)
                {
                    currentBranch = fieldInfo.GetValue(currentBranch);
                    fieldInfo = currentBranch.GetType().GetField(branch, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }

            if (delay > 0)
                PyUtils.setDelayedAction(delay, () => fieldInfo.SetValue(fieldInfo.IsStatic ? null : currentBranch, value));
            else
                fieldInfo.SetValue(fieldInfo.IsStatic ? null : currentBranch, value);

            return true;
        }

        public static double getDistance(Vector2 p1, Vector2 p2)
        {
            float distX = Math.Abs(p1.X - p2.X);
            float distY = Math.Abs(p1.Y - p2.Y);
            double dist = (distX * distX) + (distY * distY);
            return dist;
        }
        
        public static double getTileDistance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt(getDistance(p1, p2));
        }

        public static string getObjectType(object o)
        {
            return o.GetType().ToString().Split(',')[0];
        }

        public static string getFullObjectType(object o)
        {
            return o.GetType().AssemblyQualifiedName;
        }

        public static void registerTypeFromObject(object obj, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            PyLua.registerTypeFromObject(obj, showErrors, registerAssembly, predicate);
        }

        public static void registerTypeFromString(string fullTypeName, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            PyLua.registerTypeFromString(fullTypeName, showErrors, registerAssembly, predicate);
        }

        public static void loadGlobals()
        {
            PyLua.loadGlobals();
        }

        public static void addGlobal(string name, object obj)
        {
            PyLua.addGlobal(name, obj);
        }

        public static object getObjectByName(string name, bool bigCraftable = false)
        {
            int index = Game1.objectInformation.getIndexByName(name);
            return getObjectByIndex(index, bigCraftable);
        }

        public static object getObjectByIndex(int index, bool bigCraftable = false)
        {
            if (bigCraftable)
                return new SObject(Vector2.Zero, index);
            return new SObject(index, 1);
        }
    }
}
