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
        private static float wait;
        private static float interval = 1.0f;

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
            logs = loadingScreenCategory.CreateEntry<bool>("Enable Extra Console Logging", default_value: false, description: "Enable this to log extra messages to the console. Keep disabled if you are not troubleshooting.");
        }
        public override void OnPreferencesSaved()
        {
            SetColor();

            if (presetImages.Value == true)
            {
                if (logs.Value) MelonLogger.Msg("Preset Images Enabled");
                SetPresetLoading();
                if (logs.Value) MelonLogger.Msg("Preset Loading Set! This overrides the Custom Loading.");
            }
            else if (!string.IsNullOrEmpty(customImage.Value))
            {
                if (logs.Value) MelonLogger.Msg("Custom Image Path: " + customImage.Value);
                SetCustomLoading();
            }
        }

        public override void OnLateUpdate()
        {
            wait += Time.deltaTime;
            if (wait >= interval)
            {
                wait -= interval;

                try
                {
                    if (presetImages.Value == true)
                    {
                        SetColor();
                        SetPresetLoading();
                    }
                }
                catch (NullReferenceException e)
                {
                }
            }
            base.OnLateUpdate();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                SetColor();
                if (presetImages.Value == true)
                {
                    SetPresetLoading();
                }
                else if (!string.IsNullOrEmpty(customImage.Value)) 
                {
                    SetCustomLoading();
                }
            }
            catch (NullReferenceException e)
            {
            }
            base.OnSceneWasLoaded(buildIndex, sceneName);
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

            if (newMat.mainTexture == tex) return;

            newMat.mainTexture = tex;
            cache = newMat;
            SetLoadingScreen(cache);
            if (logs.Value) MelonLogger.Msg("Custom Loading Set!");
        }
        private static void SetPresetLoading()
        {
            Graphic screen;
            try
            {
                screen = MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>();
            }
            catch (NullReferenceException e) 
            {
                if (logs.Value) MelonLogger.Warning("MainMenu._instance._screenLoading is null");
                return;
            }

            Material mat;
            try
            {
                mat = new Material(screen.material);
            }
            catch (NullReferenceException e)
            {
                if (logs.Value) MelonLogger.Warning("screen material is null");
                return;
            }

            Sprite sprLvl = null, sprLoc = null;
            try
            {
                 sprLvl = Singleton<Game>.Instance.GetCurrentLevel().GetPreviewImage();
                 sprLoc = Singleton<MainMenu>.Instance.GetCurrentLocation().background;
            }
            catch (NullReferenceException e) 
            {
                if (logs.Value) MelonLogger.Warning("Current location image is null");
                if (!sprLoc && !sprLvl) {
                    screen.color = Color.black;
                    screen.material = screen.defaultMaterial;
                }
            }

            Texture tex = null;
            try
            {
                if (sprLvl != null)
                    tex = sprLvl.texture;
                else if (sprLoc != null)
                    tex = sprLoc.texture;
                else
                {
                    screen.color = Color.black;
                    tex = screen.defaultMaterial.mainTexture;
                    if (logs.Value) MelonLogger.Msg(tex.ToString());
                }
                mat.mainTexture = tex;
                cache = mat;
                SetLoadingScreen(mat);
                if (logs.Value) MelonLogger.Msg("Preset Loading Set!");
            }
            catch (NullReferenceException e)
            {
                screen.color = Color.black;
                screen.material = screen.defaultMaterial;
                if (logs.Value) MelonLogger.Warning("Setting screen material failed, moving to default color. (Error: " + e + " )");
            }

            return;
        }
        
        private static void SetColor()
        {
            Graphic screen = null;
            try
            {
                screen = MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>();
                screen.color = imageColor.Value;
                screen.material = screen.defaultMaterial;
                if (logs.Value) MelonLogger.Msg("Color Set!");
            }
            catch (NullReferenceException e)
            {
            }
            return;
        }

    }

}
