using System;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using TownOfHost.Extensions;

namespace TownOfHost.Patches
{
    [HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles.PlacePlayer))]
    public static class PlayerParticlesPlacePlayerPatch
    {
        public static void Postfix([HarmonyArgument(0)] PlayerParticle part)
        {
            if (!Constants.ShouldHorseAround())
            {
                return;
            }

            part.angularVelocity = new FloatRange[2] { new(600f, 800f), new(-800f, -600f) }.PickRandom().Next();
        }
    }

    [HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles.Update))]
    public static class PlayerParticlesUpdatePatch
    {
        public static void Postfix(PlayerParticles __instance)
        {
            if (!Constants.ShouldHorseAround())
            {
                return;
            }

            var particles = __instance.GetComponentsInChildren<PlayerParticle>();
            for (int i = 0; i < particles.Length; i++)
            {
                var particle = particles[i];
                var currentColor = particle.myRend.material.GetColor(PlayerMaterial.BodyColor);
                Color.RGBToHSV(currentColor, out var currentHue, out var currentSat, out var currentVal);
                var hue = (DateTime.Now.Millisecond / 20000f % 1) + currentHue;
                var roundedHue = hue > 1 ? hue - 1 : hue;
                var newColor = Color.HSVToRGB(roundedHue, currentSat, currentVal);
                PlayerMaterial.SetColors(newColor, particle.myRend);
            }
        }
    }
}
