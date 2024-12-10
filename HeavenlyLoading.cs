using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.IO;
using System;

namespace HeavenlyLoading
{
    public class HeavenLoading : MelonMod
    {
        private static DateTime lastLoggedTime = DateTime.MinValue;
        private static readonly TimeSpan logInterval = TimeSpan.FromSeconds(1);

        private static Material cache;

        private static MelonPreferences_Category loadingScreenCategory;
        private static MelonPreferences_Entry<string> customImage;
        private static MelonPreferences_Entry<bool> presetImages;
        private static MelonPreferences_Entry<Color> imageColor;
        private static MelonPreferences_Entry<bool> logs;

        public override void OnLateInitializeMelon()
        {
            loadingScreenCategory = MelonPreferences.CreateCategory("Heavenly Loading");
            customImage = loadingScreenCategory.CreateEntry<string>("Load Custom Image", default_value: "", description: "Sets the loading screen to a custom image.\nEnter the file path for the desired image. (Remember to remove quotes!)\nSupports JPG and PNG image files.");
            presetImages = loadingScreenCategory.CreateEntry<bool>("Load Preset Images", default_value: false, description: "Enable this to set the loading screen to a preset image that matches your location. This overrides the custom loading screen image.");
            imageColor = loadingScreenCategory.CreateEntry<Color>("Recolor Background Screen", default_value: Color.black, description: "Use this to recolor the loading screen.\nThis stacks with other options! Set as white to load images without any discoloring.");
            logs = loadingScreenCategory.CreateEntry<bool>("Enable Extra Console Logging", default_value: false, description: "Enable this to have the console log extra messages. Keep disabled unless it is for troubleshooting. Warning messages will still appear even when disabled.");
        }
        public override void OnPreferencesSaved()
        {
            SetColor();
            if (!string.IsNullOrEmpty(customImage.Value))
            {
                if (logs.Value) { MelonLogger.Msg("Custom Image Path: " + customImage.Value); }
                SetCustomLoading();
            }
            if (presetImages.Value == true)
            {
                if (logs.Value) { MelonLogger.Msg("Preset Images Enabled"); }
                SetPresetLoading();
                if (logs.Value) { MelonLogger.Msg("Preset Loading Set! This overrides the Custom Loading."); }
            }
        }

        public override void OnLateUpdate()
        {
            if (presetImages.Value == true)
            {
                DateTime now = DateTime.Now;
                if (now - lastLoggedTime > logInterval)
                {
                    SetColor();
                    SetPresetLoading();
                    if (logs.Value) { MelonLogger.Msg("Preset Loading Set!"); }
                    lastLoggedTime = now;
                }
            }
        }

        private static void SetLoadingScreen(Material mat)
        {
            Graphic screen = MainMenu._instance._screenLoading.background.GetComponent<Graphic>();
            Material newMat = mat;
            cache = newMat;
            screen.material = cache;
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
                screen.color = Color.black;
                screen.material = screen.defaultMaterial;
                MelonLogger.Warning("Path not valid: " + path);
                return;
            }

            var file = File.ReadAllBytes(path);
            Texture tex = LoadTexture(file);

            newMat.mainTexture = tex;
            cache = newMat;
            SetLoadingScreen(cache);
            MelonLogger.Warning("Custom Loading Set!");
        }
        private static void SetPresetLoading()
        {
            Graphic screen = null;

            if (MainMenu._instance != null)
            {
                if (MainMenu._instance._screenLoading != null)
                {
                    screen = MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>();
                }
                else
                {
                    MelonLogger.Warning("MainMenu._instance._screenLoading is null");
                }
            }
            else
            {
                MelonLogger.Warning("MainMenu._instance is null");
            }

            if (screen == null)
            {
                MelonLogger.Warning("screen is null");
                return;
            }

            Material mat = screen.material != null ? new Material(screen.material) : null;

            if (mat == null)
            {
                MelonLogger.Warning("screen material is null");
                return;
            }

            Sprite sprLvl = Singleton<Game>.Instance?.GetCurrentLevel()?.GetPreviewImage();
            Sprite sprLoc = Singleton<MainMenu>.Instance?.GetCurrentLocation()?.background;
            if (sprLvl == null || sprLoc == null)
            {
                if (sprLvl == null)
                {
                    MelonLogger.Warning("Current level preview image is null");
                }
                if (sprLoc == null)
                {
                    MelonLogger.Warning("Current location background is null");
                }
            }

            Texture tex = null;
            if (sprLvl != null)
            {
                tex = sprLvl.texture;
            }
            else if (sprLoc != null)
            {
                tex = sprLoc.texture;
            }

            if (tex == null)
            {
                screen.color = Color.black;
                screen.material = screen.defaultMaterial;
                MelonLogger.Warning("tex is null, moving to default color");
                return;
            }

            if (logs.Value) { MelonLogger.Msg(tex.ToString()); }
            mat.mainTexture = tex;
            cache = mat;
            SetLoadingScreen(mat);
        }
        private static void SetColor()
        {
            Graphic screen = null;
            if (MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>() != null)
            {
                screen = MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>();
            }
            screen.color = imageColor.Value;
            screen.material = screen.defaultMaterial;
            if (logs.Value) { MelonLogger.Msg("Color Set!"); }

        }

    }

}
