using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.IO;
using System.Reflection;
using MelonLoader.TinyJSON;
using UnityEngine.Assertions.Must;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.Experimental.AssetBundlePatching;



// figuring out loading mod
namespace PernaModTestv1
{
    [HarmonyPatch]
    public class ModTestv1 : MelonMod
    {

        private static bool enabled;
        private static Texture cache;

        private static MelonPreferences_Category loadingScreenCategory;

        private static MelonPreferences_Entry<string> customImage;
        private static MelonPreferences_Entry<bool> presetImages;


        
       
        public override void OnLateInitializeMelon()
        {
            loadingScreenCategory = MelonPreferences.CreateCategory("Heavenly Loads");
            customImage = loadingScreenCategory.CreateEntry<string>("Path", default_value: "", description: "Enter the file path of an image to set the black loading screen to that image.");
            presetImages = loadingScreenCategory.CreateEntry<bool>("Load Preset Images", default_value: false, description: "Enable this to have the loading screen image correlate to the mission you are in.");
            
        }
        
        public override void OnPreferencesSaved()
        {
            if (presetImages.Value)
            {
                SetLevelLoading();
                return;
            }
            if (customImage.Value.Length > 0)
            {
                SetLoadingScreen();
                return;
            }
            
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenu), "Start")] 
       
        
        private static void SetLoadingScreen(Material mat) // check custom portrait code
        {
            Graphic screen = MainMenu._instance._screenLoading.background.GetComponent<Graphic>();
            Material newMat = mat;

            

            
            

            cache = mat.mainTexture;


            screen.color = Color.white;
            screen.material = mat;
        }
        private static Texture2D LoadTexture(byte[] image)
        {

            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture2D, image, true);
            texture2D.wrapMode = TextureWrapMode.Clamp;
            return texture2D;
        }

        private static void SetCustomLoading()
        {
            string path = customImage.Value;
            Graphic screen = MainMenu._instance._screenLoading.background.GetComponent<Graphic>();
            Material newMat = new Material(screen.material);
            if (!File.Exists(path))
            {

                screen.material = MainMenu._instance._screenLoading.background.GetComponent<Graphic>().defaultMaterial;
                screen.color = Color.black;

                return;
            }
            var file = File.ReadAllBytes(path);
            var tex = LoadTexture(file);
            return;
        }
        private static void SetLevelLoading()
        { //    ._levelPreviewImage.mainTexture;
            Graphic screen = MainMenu._instance._screenLoading.background.GetComponent<Graphic>();
            Texture tex;
           
            Material newMat = new Material(screen.material);
            if (!tex)
            {
                screen.color = Color.white;
                newMat.mainTexture = tex;
                screen.material = newMat;
                return;
            }


            return;
        }



    }
}
