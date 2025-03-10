using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using CustomAvatar.Avatar;

namespace MultiplayerAvatars.Patches
{
    [HarmonyPatch(typeof(CustomAvatar.Player.PlayerAvatarManager), "SwitchToAvatar")]
    internal static class FilePathPatch
    {
    	static readonly ConditionalWeakTable<AvatarPrefab, string> pathMap = new();

    	public static string Lookup(AvatarPrefab prefab) {
    		lock(pathMap) {
	    		return pathMap.TryGetValue(prefab, out string fullPath) ? fullPath : string.Empty;
    		}
    	}

    	private static void Prefix(AvatarPrefab avatar, string fullPath) {
    		if(avatar == null)
	    		return;
    		lock(pathMap) {
	    		pathMap.Remove(avatar);
	    		pathMap.Add(avatar, fullPath);
	    	}
    	}
    }
}
