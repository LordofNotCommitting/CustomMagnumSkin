using HarmonyLib;
using MGSC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using static System.Net.Mime.MediaTypeNames;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace CustomMagnumSkin
{
    [HarmonyPatch(typeof(SpaceshipSkin), nameof(SpaceshipSkin.RefreshSkin))]
    public class ReplaceShipTexture
    {

        public static void Postfix(ref SpaceshipSkin __instance)
        {


            bool flag = SteamWrapper.SUPPORTER_PACK_AVAILABLE && SingletonMonoBehaviour<GameSettings>.Instance.ShowSupporterPackSkin;

            if (__instance._shipStdMat.GetTexture(Shader.PropertyToID("_MainTex")).name == "MagnumMain") {
                //Plugin.Logger.Log("--- applying override only once");
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Texture2D new_ship_texture = LoadPNG(Path.Combine(assemblyPath, "new_MagnumMain.png"), Path.Combine(assemblyPath, "default_MagnumMain.png"));
                Texture2D new_antenna_texture = LoadPNG(Path.Combine(assemblyPath, "new_ship_devises.png"), Path.Combine(assemblyPath, "default_ship_devises.png"));
                //load new texture?

                if (new_ship_texture != null)
                {

                    /*
                    // I need to see old texture before doing this.
                    Texture2D old_ship_texture = (Texture2D)__instance._shipStdMat.GetTexture("_MainTex");
                    old_ship_texture.isReadable = true;
                    byte[] bytes = old_ship_texture.EncodeToPNG();
                    System.IO.File.WriteAllBytes(Path.Combine(assemblyPath, "old_MagnumMain.png"), bytes);
                    Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + Path.Combine(assemblyPath, "old_MagnumMain.png"));
                    */
                    new_ship_texture.name = "New_MagnumMain";

                    //Material temp_material = new Material(__instance._shipStdMat);

                    /*
                    Texture old_ship_texture = __instance._shipStdMat.GetTexture("_MainTex");

                    //for debugging this pos
                    for (int i = 0; i < temp_material.shader.GetPropertyCount(); i++)
                    {
                        if (temp_material.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                        {
                            string propName = temp_material.shader.GetPropertyName(i);
                            Texture tex = temp_material.GetTexture(propName);
                            Plugin.Logger.Log($"{propName} = {tex}");
                        }
                    }
                    */

                    //Plugin.Logger.Log("--- old textur " + __instance._shipStdMat.GetTexture("_MainTex").GetType());
                    //Plugin.Logger.Log("--- old textur deets " + __instance._shipStdMat.GetTexture("_MainTex").GetNativeTexturePtr());

                    //Plugin.Logger.Log("--- new textur  " + new_ship_texture.GetType());
                    // Plugin.Logger.Log("--- new textur deets " + new_ship_texture.GetNativeTexturePtr());



                    __instance._shipStdMat.SetTexture(Shader.PropertyToID("_MainTex"), new_ship_texture);
                    __instance._shipStdMat.SetTexture(Shader.PropertyToID("_BaseMap"), new_ship_texture);

                    MeshRenderer[] array = __instance._shipRenderers;

                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].sharedMaterial = (flag ? __instance._shipPremiumMat : __instance._shipStdMat); ;
                    }
                }

                if (new_antenna_texture != null)
                {
                    __instance._detailsStdMat.SetTexture(Shader.PropertyToID("_MainTex"), new_antenna_texture);
                    __instance._detailsStdMat.SetTexture(Shader.PropertyToID("_BaseMap"), new_antenna_texture);
                    MeshRenderer[] array = __instance._detailsRenderers;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].sharedMaterial = (flag ? __instance._detailsPremiumMat : __instance._detailsStdMat);
                    }
                }

            }

            if (__instance._shipStdMat.GetTexture(Shader.PropertyToID("_MainTex")).name == "New_MagnumMain")
            {
                //I think the original version of ship light is blinding the hell out of me in realspace. tone it down.

                //original intensity is 12 for all
                float light_adjusted_intensity = 10;
                if (!__instance._isInBramfatura)
                {
                    light_adjusted_intensity = 3;
                }
                foreach (Light light in __instance._additionalLights)
                {
                    light.intensity = light_adjusted_intensity;
                    //Plugin.Logger.Log("--- light intensity?  " + light.intensity);
                }
            }
            

        }


        private static Texture2D LoadPNG(string expected_filepath, string default_filepath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(expected_filepath))
            {
                fileData = File.ReadAllBytes(expected_filepath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //This will auto-resize the texture dimensions.
            }
            else if (File.Exists(default_filepath))
            {
                fileData = File.ReadAllBytes(default_filepath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); 
            }
            else {
                throw new FileNotFoundException($"Unable to find {expected_filepath} or {default_filepath}");
            }
                return tex;
        }




    }


    




}
