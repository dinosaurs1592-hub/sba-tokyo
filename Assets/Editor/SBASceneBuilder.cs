using SBATokyo.Core;
using SBATokyo.Gameplay;
using SBATokyo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SBATokyo.Editor
{
    public static class SBASceneBuilder
    {
        private const float ChunkLen = 36f;
        private const float ChunkHalf = ChunkLen * 0.5f;

        [MenuItem("SBA Tokyo/Build Game Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Chunks");

            // ── chunk prefabs ──────────────────────────────────────────────
            var chunkPrefabs = new GameObject[]
            {
                SaveChunkPrefab(BuildChunkFlat(),            "ChunkFlat"),
                SaveChunkPrefab(BuildChunkObstacleRun(),     "ChunkObstacleRun"),
                SaveChunkPrefab(BuildChunkRailSection(),     "ChunkRailSection"),
                SaveChunkPrefab(BuildChunkObjectPlatform(),  "ChunkObjectPlatform"),
                SaveChunkPrefab(BuildChunkMixed(),           "ChunkMixed"),
            };

            // ── new scene ──────────────────────────────────────────────────
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "GameScene";

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.8f, 0.88f, 0.96f);
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 250f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.78f, 0.82f, 0.9f);

            // ── GameManager ────────────────────────────────────────────────
            var gmObj = new GameObject("GameManager");
            var gm = gmObj.AddComponent<SBAGameManager>();

            // ── InputManager ───────────────────────────────────────────────
            var imObj = new GameObject("InputManager");
            var im = imObj.AddComponent<SBAInputManager>();

            // ── Runner ─────────────────────────────────────────────────────
            var runner = new GameObject("Runner");
            runner.transform.position = new Vector3(0f, 1.1f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(runner.transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.1f, 0f);

            var core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            core.name = "Core";
            core.transform.SetParent(visual.transform, false);
            core.transform.localScale = new Vector3(0.58f, 1f, 0.58f);
            core.GetComponent<Renderer>().sharedMaterial =
                CreateTransparentMat("Runner_Core_Mat", new Color(0.3f, 0.9f, 1f, 0.28f));
            Object.DestroyImmediate(core.GetComponent<Collider>());

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(visual.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.93f, 0f);
            ring.transform.localScale = new Vector3(0.66f, 0.04f, 0.66f);
            ring.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Runner_Outline_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(ring.GetComponent<Collider>());

            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Stripe";
            stripe.transform.SetParent(visual.transform, false);
            stripe.transform.localPosition = new Vector3(0f, 0f, 0.57f);
            stripe.transform.localScale = new Vector3(0.12f, 2.02f, 0.06f);
            stripe.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Runner_Outline_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(stripe.GetComponent<Collider>());

            var cc = runner.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2f;
            cc.radius = 0.4f;

            var rc = runner.AddComponent<RunnerController>();
            SetRef(rc, "gameManager", gm);
            SetRef(rc, "inputManager", im);

            // ── Camera ─────────────────────────────────────────────────────
            var camObj = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
            camObj.name = "Main Camera";
            camObj.tag = "MainCamera";
            if (camObj.GetComponent<Camera>() == null) camObj.AddComponent<Camera>();
            if (camObj.GetComponent<AudioListener>() == null) camObj.AddComponent<AudioListener>();

            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.82f, 0.91f, 0.98f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 350f;
            cam.fieldOfView = 62f;
            camObj.transform.position = new Vector3(0f, 2.35f, -5.8f);
            camObj.transform.rotation = Quaternion.Euler(8f, 0f, 0f);

            var cf = camObj.GetComponent<CameraFollow>() ?? camObj.AddComponent<CameraFollow>();
            SetRef(cf, "target", runner.transform);
            SetRef(cf, "gameManager", gm);

            // ── Scenery (static, decorative only) ─────────────────────────
            CreateRoadsidePosts();
            CreateCityBackdrop();
            CreateDirectionalLight();

            // ── ChunkSpawner ───────────────────────────────────────────────
            var spawnerObj = new GameObject("ChunkSpawner");
            var spawner = spawnerObj.AddComponent<ChunkSpawner>();
            SetRef(spawner, "gameManager", gm);
            SetRef(spawner, "runner", runner.transform);
            SetRefArray(spawner, "chunkPrefabs", chunkPrefabs);

            // ── HUD ────────────────────────────────────────────────────────
            BuildHUD(gm);

            // ── Save ───────────────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
            };
            AssetDatabase.SaveAssets();
            Selection.activeObject = gmObj;
        }

        // ── chunk builders ─────────────────────────────────────────────────

        private static GameObject BuildChunkFlat()
        {
            var chunk = new GameObject("ChunkFlat");
            chunk.AddComponent<ChunkController>();
            AddRoad(chunk.transform);
            return chunk;
        }

        private static GameObject BuildChunkObstacleRun()
        {
            var chunk = new GameObject("ChunkObstacleRun");
            chunk.AddComponent<ChunkController>();
            AddRoad(chunk.transform);
            AddObstacle(chunk.transform, -3f, 10f);
            AddObstacle(chunk.transform, 3f,  22f);
            AddObstacle(chunk.transform, 0f,  30f);
            return chunk;
        }

        private static GameObject BuildChunkRailSection()
        {
            var chunk = new GameObject("ChunkRailSection");
            chunk.AddComponent<ChunkController>();
            AddRoad(chunk.transform);

            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(chunk.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.4f, ChunkHalf);
            rail.transform.localScale = new Vector3(2.5f, 0.08f, 30f);
            var rs = rail.AddComponent<RoadSurface>();
            SetEnum(rs, "surfaceType", (int)SurfaceType.Rail);
            rail.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Rail_Mat", new Color(0.58f, 0.85f, 0.95f));
            return chunk;
        }

        private static GameObject BuildChunkObjectPlatform()
        {
            var chunk = new GameObject("ChunkObjectPlatform");
            chunk.AddComponent<ChunkController>();
            AddRoad(chunk.transform);

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetParent(chunk.transform, false);
            platform.transform.localPosition = new Vector3(0f, 0.55f, ChunkHalf);
            platform.transform.localScale = new Vector3(2.3f, 0.9f, 6f);
            var ps = platform.AddComponent<RoadSurface>();
            SetEnum(ps, "surfaceType", (int)SurfaceType.Object);
            platform.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Platform_Mat", new Color(0.86f, 0.46f, 0.18f));

            AddObstacle(chunk.transform, -3f, 12f);
            AddObstacle(chunk.transform, 3f,  12f);
            return chunk;
        }

        private static GameObject BuildChunkMixed()
        {
            var chunk = new GameObject("ChunkMixed");
            chunk.AddComponent<ChunkController>();
            AddRoad(chunk.transform);

            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(chunk.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.4f, ChunkHalf);
            rail.transform.localScale = new Vector3(2.5f, 0.08f, 20f);
            var rs = rail.AddComponent<RoadSurface>();
            SetEnum(rs, "surfaceType", (int)SurfaceType.Rail);
            rail.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Rail_Mat", new Color(0.58f, 0.85f, 0.95f));

            AddObstacle(chunk.transform, -3f, 8f);
            AddObstacle(chunk.transform, 3f,  26f);
            return chunk;
        }

        // ── road geometry (per chunk) ──────────────────────────────────────

        private static void AddRoad(Transform parent)
        {
            // Ground (with RoadSurface for runner detection)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = new Vector3(0f, -0.1f, ChunkHalf);
            ground.transform.localScale = new Vector3(10.5f, 0.2f, ChunkLen);
            ground.AddComponent<RoadSurface>();
            ground.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Ground_Mat", new Color(0.17f, 0.19f, 0.23f));

            // Lane markers (visual only)
            for (int lane = -1; lane <= 1; lane++)
                AddDecor(parent, new Vector3(lane * 3f, 0.025f, ChunkHalf),
                    new Vector3(0.12f, 0.05f, ChunkLen),
                    lane == 0 ? "LaneCenter_Mat" : "LaneSide_Mat",
                    lane == 0 ? new Color(0.32f, 0.82f, 0.95f) : new Color(0.28f, 0.3f, 0.35f));

            // Lane dividers
            for (int s = -1; s <= 1; s += 2)
                AddDecor(parent, new Vector3(s * 1.5f, 0.03f, ChunkHalf),
                    new Vector3(0.2f, 0.03f, ChunkLen),
                    "LaneDash_Mat", new Color(0.94f, 0.94f, 0.9f));

            // Road edge lines
            for (int s = -1; s <= 1; s += 2)
                AddDecor(parent, new Vector3(s * 4.2f, 0.03f, ChunkHalf),
                    new Vector3(0.18f, 0.04f, ChunkLen),
                    "RoadEdge_Mat", new Color(0.97f, 0.97f, 0.94f));

            // Road borders (keep collider to block lateral escape)
            for (int s = -1; s <= 1; s += 2)
            {
                var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
                border.name = s < 0 ? "Border_L" : "Border_R";
                border.transform.SetParent(parent, false);
                border.transform.localPosition = new Vector3(s * 5f, 0.55f, ChunkHalf);
                border.transform.localScale = new Vector3(0.45f, 1.1f, ChunkLen);
                border.GetComponent<Renderer>().sharedMaterial =
                    CreateMat("RoadBorder_Mat", new Color(0.85f, 0.31f, 0.24f));
            }
        }

        private static void AddDecor(Transform parent, Vector3 pos, Vector3 scale, string matName, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = CreateMat(matName, color);
            Object.DestroyImmediate(obj.GetComponent<Collider>());
        }

        private static void AddObstacle(Transform parent, float laneX, float localZ)
        {
            var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obs.name = "Obstacle";
            obs.transform.SetParent(parent, false);
            obs.transform.localPosition = new Vector3(laneX, 0.65f, localZ);
            obs.transform.localScale = new Vector3(1.8f, 1.3f, 2.8f);
            obs.AddComponent<BoxCollider>();
            obs.AddComponent<Obstacle>();
            obs.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Obstacle_Mat", new Color(0.96f, 0.3f, 0.2f));
        }

        // ── prefab save ────────────────────────────────────────────────────

        private static GameObject SaveChunkPrefab(GameObject go, string name)
        {
            string path = $"Assets/Prefabs/Chunks/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ── static scenery ─────────────────────────────────────────────────

        private static void CreateRoadsidePosts()
        {
            float startZ = -72f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 30; i++)
                {
                    float z = startZ + i * 15f;
                    var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pole.name = $"Post_{(side < 0 ? "L" : "R")}_{i}";
                    pole.transform.position = new Vector3(side * 6.2f, 1.15f, z);
                    pole.transform.localScale = new Vector3(0.12f, 2.3f, 0.12f);
                    pole.GetComponent<Renderer>().sharedMaterial =
                        CreateMat("Post_Mat", new Color(0.72f, 0.75f, 0.8f));

                    var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    lamp.name = $"Lamp_{(side < 0 ? "L" : "R")}_{i}";
                    lamp.transform.position = new Vector3(side * 6.2f, 2.35f, z);
                    lamp.transform.localScale = new Vector3(0.3f, 0.16f, 0.3f);
                    lamp.GetComponent<Renderer>().sharedMaterial =
                        CreateMat("Lamp_Mat", new Color(0.98f, 0.88f, 0.58f));
                }
            }
        }

        private static void CreateCityBackdrop()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 8; i++)
                {
                    float z = 135f + i * 34f;
                    float x = side * (10f + (i % 3) * 3.5f);
                    float h = 8f + (i % 4) * 4f;
                    float w = 3.5f + (i % 2) * 1.5f;

                    var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    b.name = $"Building_{(side < 0 ? "L" : "R")}_{i}";
                    b.transform.position = new Vector3(x, h * 0.5f, z);
                    b.transform.localScale = new Vector3(w, h, w);
                    b.GetComponent<Renderer>().sharedMaterial =
                        CreateMat($"Bldg_{i}", i % 2 == 0
                            ? new Color(0.36f, 0.43f, 0.5f)
                            : new Color(0.46f, 0.52f, 0.58f));

                    var e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    e.name = $"BldgEdge_{(side < 0 ? "L" : "R")}_{i}";
                    e.transform.position = new Vector3(x, h + 0.08f, z);
                    e.transform.localScale = new Vector3(w + 0.18f, 0.16f, w + 0.18f);
                    e.GetComponent<Renderer>().sharedMaterial =
                        CreateMat($"BldgEdge_{i}", new Color(0.88f, 0.92f, 0.98f));
                }
            }

            var horizon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horizon.name = "HorizonWall";
            horizon.transform.position = new Vector3(0f, 12f, 420f);
            horizon.transform.localScale = new Vector3(72f, 24f, 1f);
            horizon.GetComponent<Renderer>().sharedMaterial =
                CreateMat("Horizon_Mat", new Color(0.74f, 0.79f, 0.86f));
        }

        private static void CreateDirectionalLight()
        {
            var go = GameObject.Find("Directional Light") ?? new GameObject("Directional Light");
            var light = go.GetComponent<Light>() ?? go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.96f, 0.9f);
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // ── HUD ───────────────────────────────────────────────────────────

        private static void BuildHUD(SBAGameManager gm)
        {
            var canvas = new GameObject("Canvas");
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            var hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(canvas.transform, false);
            var hud = hudObj.AddComponent<GameHUD>();
            SetRef(hud, "gameManager", gm);
            SetRef(hud, "speedText",    MakeText("SpeedText",    hudObj.transform, new Vector2(120f, -30f)));
            SetRef(hud, "distanceText", MakeText("DistanceText", hudObj.transform, new Vector2(140f, -70f)));
            SetRef(hud, "stateText",    MakeText("StateText",    hudObj.transform, new Vector2(240f, -110f)));
            SetRef(hud, "hintText",     MakeText("HintText",     hudObj.transform, new Vector2(240f, -150f)));
            SetRef(hud, "diagnosticsText", MakeText("DiagText",  hudObj.transform, new Vector2(240f, -190f)));
            SetRef(hud, "speedSlider",  MakeSlider(hudObj.transform));
        }

        private static Text MakeText(string name, Transform parent, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = pos;
            r.sizeDelta = new Vector2(520f, 30f);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 24;
            t.color = new Color(0.06f, 0.08f, 0.1f);
            t.text = name;
            return t;
        }

        private static Slider MakeSlider(Transform parent)
        {
            var root = new GameObject("SpeedSlider");
            root.transform.SetParent(parent, false);
            var r = root.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(20f, -230f);
            r.sizeDelta = new Vector2(320f, 20f);
            var slider = root.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;

            var bg = new GameObject("Background");
            bg.transform.SetParent(root.transform, false);
            var bgR = bg.AddComponent<RectTransform>();
            bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one;
            bgR.offsetMin = bgR.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.15f, 0.19f, 0.85f);

            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(root.transform, false);
            var faR = fillArea.AddComponent<RectTransform>();
            faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one;
            faR.offsetMin = new Vector2(5f, 5f); faR.offsetMax = new Vector2(-5f, -5f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillR = fill.AddComponent<RectTransform>();
            fillR.anchorMin = Vector2.zero; fillR.anchorMax = Vector2.one;
            fillR.offsetMin = fillR.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.98f, 0.47f, 0.18f);

            slider.fillRect = fillR;
            slider.targetGraphic = fillImg;
            slider.handleRect = null;
            return slider;
        }

        // ── helpers ───────────────────────────────────────────────────────

        private static Material CreateMat(string name, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
            if (existing != null) { existing.color = color; return existing; }
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
            AssetDatabase.CreateAsset(mat, $"Assets/Materials/{name}.mat");
            return mat;
        }

        private static Material CreateTransparentMat(string name, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
            if (existing != null) { existing.color = color; return existing; }
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.color = color;
            AssetDatabase.CreateAsset(mat, $"Assets/Materials/{name}.mat");
            return mat;
        }

        private static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRefArray(Object target, string field, GameObject[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
