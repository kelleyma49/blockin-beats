using System;
using UnityEditor;
using UnityEngine;

public class LevelCreator : EditorWindow
{
    string _levelName = "";
    private string _shaderPath = "";
    private string _blackImagePath = "";
    private string _whiteImagePath = "";

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Level Creator")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(LevelCreator));
    }

    void GuiFileEditor(string  name,ref string path,string fileType)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField(name, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button(".."))
            {
                path = EditorUtility.OpenFilePanel(name, "", fileType);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnGUI()
    {
        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        _levelName = EditorGUILayout.TextField("Level Name", _levelName);
        GuiFileEditor("shader path",ref _shaderPath,"glsl");
        GuiFileEditor("black image path",ref _blackImagePath,"png");
        GuiFileEditor("white image path", ref _whiteImagePath, "png");

        if (GUILayout.Button("Create fragment shader level..."))
        {
            CreateLevel(true);
        }
        else if (GUILayout.Button("Create vertex shader level..."))
        {
            CreateLevel(false);
        }
    }

    string CopyItem(string file,string outputFileName = null)
    {
        var templatePath = file;
        if (!System.IO.Path.IsPathRooted(file))
        {
            templatePath = $"Assets\\Editor\\LevelTemplate\\{file}";
        }
        if (outputFileName == null)
            outputFileName = file;
        var outputPath = $"Assets\\Resources\\Levels\\{_levelName}\\" + outputFileName.Replace("Template", _levelName);
        FileUtil.CopyFileOrDirectory(templatePath, outputPath);
        return outputPath;
    }
    
    string SaveTemplateFile(string templateFileContent,string outputFileName)
    {
        var tempFile = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(tempFile, templateFileContent);
        var outputPath = CopyItem(tempFile, outputFileName);
        System.IO.File.Delete(tempFile);
        return outputPath;
    }

    void CreateLevel(bool fragmentBased)
    {
        if (string.IsNullOrWhiteSpace(_levelName))
        {
            EditorUtility.DisplayDialog("Level name required", "Please fill in the 'Level Name' field before creating a level", "OK");
            return;
        }

        AssetDatabase.CreateFolder("Assets/Resources/Levels", _levelName);

        var blackPath = CopyItem(!string.IsNullOrWhiteSpace(_blackImagePath) ? _blackImagePath : "Black.png","Black.png");
        CopyItem("Black.prefab");
        var whitePath = CopyItem(!string.IsNullOrWhiteSpace(_whiteImagePath) ? _whiteImagePath : "White.png","White.png");
        CopyItem("White.prefab");

        var shaderNameOnly = "";
        if (!string.IsNullOrEmpty(_shaderPath))
        {
            shaderNameOnly = System.IO.Path.GetFileName(_shaderPath);
            CopyItem(_shaderPath,shaderNameOnly);
        }
        var shaderPath = SaveTemplateFile(new T4Generator.ShaderTemplate(_levelName,shaderNameOnly).TransformText(), "LevelTemplate.shader");
      
        // import copied assets:
        Debug.Log("updating resources...");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // create material from shader:
        var shaderImporter = (ShaderImporter)AssetImporter.GetAtPath(shaderPath);
        var newMaterial = new Material(shaderImporter.GetShader());
        AssetDatabase.CreateAsset(newMaterial,shaderPath.Replace(".shader",".mat"));

        // reset level name:
        _levelName = "";

       
        var go = new GameObject();
        go.AddComponent<AudioSource>();
        AbstractLevel al = null;
        if (fragmentBased)
        {

            var level = go.AddComponent<FragmentShaderLevel>();
            level.timelineRate = 0.4f;
            al = level;
        }
        else
        {
            //go.AddComponent<AbstractLevel>();
        }

        foreach (var p in new string[] { blackPath, whitePath })
        {
            // set texture/sprite properties:
            var importer = (TextureImporter)AssetImporter.GetAtPath(p);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureType = TextureImporterType.Sprite;

            var texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int)SpriteAlignment.TopLeft;
            texSettings.spriteMode = (int) SpriteImportMode.Single;
            importer.SetTextureSettings(texSettings);
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();

            // create prefab
            var prefabPath = System.IO.Path.ChangeExtension(p, "prefab").Replace("\\", "/");
            var ph = PrefabUtility.CreateEmptyPrefab(prefabPath);

            var spriteGo = new GameObject();
            var sr = spriteGo.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Playfield";
            sr.sortingOrder = 1;
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);

            PrefabUtility.ReplacePrefab(spriteGo, ph, ReplacePrefabOptions.ConnectToPrefab);
            GameObject.DestroyImmediate(spriteGo);

            var prefabLoaded = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (p.Contains("Black"))
            {
                al.prefabBlack = prefabLoaded;
                al.colorBlack = GetDominantColor(p);
            }
            else
            {
                al.prefabWhite = prefabLoaded;
                al.colorWhite = GetDominantColor(p);
            }
        }
        al.backgroundMaterial = newMaterial;
 
        // setup level prefab:       
        var placeholder = PrefabUtility.CreateEmptyPrefab(shaderPath.Replace("\\", "/").Replace(".shader", ".prefab"));
        PrefabUtility.ReplacePrefab(go, placeholder, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(go);
    }

    public Color GetDominantColor(string filePath)
    {
        var fileData = System.IO.File.ReadAllBytes(filePath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        float totalR = 0.0f, totalG = 0.0f, totalB = 0.0f;
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                var c = tex.GetPixel(x, y);
                totalR += c.r;
                totalG += c.g;
                totalB += c.b;
            }
        }
        var numPixels = tex.width * tex.height;
        var avgR = totalR / numPixels;
        var avgG = totalG / numPixels;
        var avgB = totalB / numPixels;

        return new Color(avgR, avgG, avgB);
    }
}
