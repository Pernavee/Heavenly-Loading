using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.IO;

// figuring out loading mod
namespace HeavenlyLoading
{
    
    [HarmonyPatch]
    public class HeavenLoading : MelonMod
    {

        private static Material cache;


        private static MelonPreferences_Category loadingScreenCategory;

        private static MelonPreferences_Entry<string> customImage;
        private static MelonPreferences_Entry<bool> presetImages;
        private static MelonPreferences_Entry<Color> imageColor;


        public override void OnLateInitializeMelon()
        {
            loadingScreenCategory = MelonPreferences.CreateCategory("Heavenly Loading");
            customImage = loadingScreenCategory.CreateEntry<string>("Custom Image File Path", default_value: "", description: "Sets the black loading screen to a custom image.\nEnter the file path for the desired image. (remember to remove quotes!)\nSupports BMP, TIF, TGA, JPG, and PSD image files.");
            presetImages = loadingScreenCategory.CreateEntry<bool>("Load Preset Images", default_value: false, description: "Enable this to set the loading screen to match your location. This overrides the custom loading screen image.");
            imageColor = loadingScreenCategory.CreateEntry<Color>("Recolor Background Screen", default_value: Color.white, description: "Use this to recolor the loading screen.\nThis stacks with other options! Set as white to load images without any discoloring.");
        }
        public override void OnPreferencesSaved()
        {

            if (customImage.Value!=null)
                {
                    SetCustomLoading();
                }
            if (presetImages.Value)
                {
                    SetPresetLoading();
                }
            
            
        }
        public override void OnLateUpdate()
        {
            if (!presetImages.Value)
            {
                return;
            }
            else
            {
                SetPresetLoading();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenu), "Start")]
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
                MelonLogger.Warning("Custom image file not found: " + path);
                return;
            }
            var file = File.ReadAllBytes(path);
            Texture tex = LoadTexture(file);


            screen.color = imageColor.Value;


            newMat.mainTexture = tex;
            cache = newMat;

            SetLoadingScreen(cache);
        }
        private static void SetPresetLoading()
        {
            Graphic screen = null;
            if (MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>() != null)
            {
                screen = MainMenu._instance._screenLoading.GetComponentInChildren<Graphic>();
            }
            Material mat = new Material(screen.material);

            Sprite sprLvl = Singleton<Game>.Instance.GetCurrentLevel().GetPreviewImage();
            Sprite sprLoc = Singleton<MainMenu>.Instance.GetCurrentLocation().background;

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
                MelonLogger.Warning("tex was null");
                return;
            }

            mat.mainTexture = tex;
            cache = mat;
            screen.color = imageColor.Value;
            SetLoadingScreen(mat);

        }

    }

}
