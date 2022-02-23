using HarmonyLib;

namespace CelesteWilds
{
	[HarmonyPatch(typeof(PlayerResources))]
	public static class ChangePlayerResources
    {
		private static bool changeValues = false;

		private static float maxFuel;
		private static float currentFuel;
		private static float lowFuel;
		private static float criticalFuel;

		private static float boostChargeFraction;

		private static bool alwaysAllowBooster = false;

		private static bool allowJetpack = true;

		public static void ChangeValues(bool changeValues = true)
		{
			ChangePlayerResources.changeValues = changeValues;
		}

		public static void SetMaxFuel(float maxFuel)
		{
			ChangePlayerResources.maxFuel = maxFuel;
		}

		public static void SetCurrentFuel(float currentFuel)
		{
			ChangePlayerResources.currentFuel = currentFuel;
		}

		public static void SetLowFuel(float lowFuel)
		{
			ChangePlayerResources.lowFuel = lowFuel;
		}

		public static void SetCriticalFuel(float criticalFuel)
		{
			ChangePlayerResources.criticalFuel = criticalFuel;
		}

		public static void SetBoostChargeFraction(float boostChargeFraction)
		{
			ChangePlayerResources.boostChargeFraction = boostChargeFraction;
		}

		public static void AllowJetpack(bool allowJetpack)
		{
			ChangePlayerResources.allowJetpack = allowJetpack;
		}

		public static void AlwaysAllowBooster(bool alwaysAllowBooster)
		{
			ChangePlayerResources.alwaysAllowBooster = alwaysAllowBooster;
		}

		[HarmonyPostfix]
		[HarmonyPatch("GetFuel")]
		private static void GetCurrentFuel(ref float __result)
		{
			if (changeValues)
				__result = currentFuel;
		}

		[HarmonyPostfix]
		[HarmonyPatch("GetLowFuel")]
		private static void GetLowFuel(ref float __result)
		{
			if (changeValues)
				__result = lowFuel;
		}

		[HarmonyPostfix]
		[HarmonyPatch("GetCriticalFuel")]
		private static void GetCriticalFuel(ref float __result)
		{
			if (changeValues)
				__result = criticalFuel;
		}

		[HarmonyPostfix]
		[HarmonyPatch("GetFuelFraction")]
		private static void GetFuelFraction(ref float __result)
		{
			if (changeValues)
				__result = currentFuel / maxFuel;
		}


		[HarmonyPostfix]
		[HarmonyPatch("GetBoostChargeFraction")]
		private static void GetBoostChargeFraction(ref float __result)
		{
			if (changeValues)
				__result = boostChargeFraction;
		}

		[HarmonyPostfix]
		[HarmonyPatch("IsJetpackUsable")]
		private static void IsJetpackUsablePostfix(ref bool __result)
		{
			__result &= allowJetpack;
		}

		[HarmonyPostfix]
		[HarmonyPatch("IsBoosterAllowed")]
		private static void IsBoosterAllowedPostfix(ref bool __result)
		{
			__result |= alwaysAllowBooster;
		}
	}
}
