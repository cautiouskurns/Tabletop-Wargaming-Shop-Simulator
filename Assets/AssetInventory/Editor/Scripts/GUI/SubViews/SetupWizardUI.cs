﻿using UnityEditor;
#if !USE_VECTOR_GRAPHICS
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace AssetInventory
{
    public sealed class SetupWizardUI : EditorWindow
    {
        public static SetupWizardUI ShowWindow()
        {
            SetupWizardUI window = GetWindow<SetupWizardUI>("Setup Wizard");
            window.minSize = new Vector2(400, 450);

            return window;
        }

        public void OnGUI()
        {
            int labelWidth = 220;
            int cbWidth = 20;

            EditorGUILayout.HelpBox("Welcome to Asset Inventory! This wizard will help you set up the plugin. All settings are set to meaningful defaults and all can be adjusted later. Some settings do have more influence though on the performance so it makes sense to consider these up-front.", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Downloading assets which are not cached yet ensures all your purchased assets are indeed indexed.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content("Download Assets for Indexing", "Download Assets for Indexing\", \"Automatically download uncached items from the Asset Store for indexing. Will delete them again afterwards if not selected otherwise below. Attention: downloading an item will revoke the right to easily return it through the Asset Store."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            AI.Actions.DownloadAssets = EditorGUILayout.Toggle(AI.Actions.DownloadAssets, GUILayout.MaxWidth(cbWidth));
            GUILayout.EndHorizontal();

            if (AI.Actions.DownloadAssets)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content("Keep Downloaded Assets", "Will not delete automatically downloaded assets after indexing but keep them in the cache instead."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                AI.Config.keepAutoDownloads = EditorGUILayout.Toggle(AI.Config.keepAutoDownloads, GUILayout.MaxWidth(cbWidth));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("These are optional indices that can be useful but are typically used less often.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content("Extract Color Information", "Determines the hue of an image which will enable search by color. Increases indexing time. Can be turned on & off as needed."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            AI.Actions.ExtractColors = EditorGUILayout.Toggle(AI.Actions.ExtractColors, GUILayout.MaxWidth(cbWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content("Index Registry Package Contents", "Will index packages (from a registry) and make contents searchable. Can result in a lot of indexed files, depending on how many versions of a package there are."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            AI.Actions.IndexPackageCache = EditorGUILayout.Toggle(AI.Actions.IndexPackageCache, GUILayout.MaxWidth(cbWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content("Calculate FBX Dependencies", "Will scan FBX files for embedded texture references. This is recommended for maximum compatibility but can reduce performance of dependency calculation and preview generation."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            AI.Config.scanFBXDependencies = EditorGUILayout.Toggle(AI.Config.scanFBXDependencies, GUILayout.MaxWidth(cbWidth));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Previews are usually extracted from the packages. These are limited to 128x128 dimensions though. Activating the upscaling will provide larger previews for image files at the cost of additional storage space.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content("Upscale Preview Images", "Resize preview images to make them fill a bigger area of the tiles."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            AI.Config.upscalePreviews = EditorGUILayout.Toggle(AI.Config.upscalePreviews, GUILayout.MaxWidth(cbWidth));
            GUILayout.EndHorizontal();

            if (AI.Config.upscalePreviews)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content(AI.Config.upscaleLossless ? $"{UIStyles.INDENT}Target Size" : $"{UIStyles.INDENT}Minimum Size", "Minimum size the preview image should have. Bigger images are not changed."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                AI.Config.upscaleSize = EditorGUILayout.DelayedIntField(AI.Config.upscaleSize, GUILayout.Width(50));
                EditorGUILayout.LabelField("px", EditorStyles.miniLabel);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content("Verify Previews", "Will check preview images if they indeed contain a preview or just Unity default icons. Highly recommended but will slow down indexing and preview recreation."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            AI.Config.verifyPreviews = EditorGUILayout.Toggle(AI.Config.verifyPreviews, GUILayout.MaxWidth(cbWidth));
            GUILayout.EndHorizontal();

#if !USE_VECTOR_GRAPHICS
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("In order to see previews for SVG graphics, the 'com.unity.vectorgraphics' needs to be installed.", MessageType.Warning);
            if (GUILayout.Button("Install Package"))
            {
                Client.Add("com.unity.vectorgraphics");
            }
#endif

            if (EditorGUI.EndChangeCheck())
            {
                AI.SaveConfig();
            }
        }
    }
}