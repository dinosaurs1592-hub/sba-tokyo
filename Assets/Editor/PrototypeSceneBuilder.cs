using SBATokyo.Prototype.Core;
using SBATokyo.Prototype.Gameplay;
using SBATokyo.Prototype.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SBATokyo.Prototype.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const float RoadCenterZ = 170f;
        private const float RoadLength = 520f;
        private const float RoadStartZ = RoadCenterZ - RoadLength * 0.5f;
        private const float RoadEndZ = RoadCenterZ + RoadLength * 0.5f;

        [MenuItem("SBA Tokyo/Build Prototype Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "RunnerPrototype";
            EnsureFolder("Assets/Materials");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.8f, 0.88f, 0.96f);
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 250f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.78f, 0.82f, 0.9f);

            var gameManagerObject = new GameObject("GameManager");
            var gameManager = gameManagerObject.AddComponent<PrototypeGameManager>();

            var inputManagerObject = new GameObject("InputManager");
            var inputManager = inputManagerObject.AddComponent<PrototypeInputManager>();

            var player = new GameObject("Runner");
            player.transform.position = new Vector3(0f, 1.1f, 0f);

            var visualRoot = new GameObject("RunnerVisual");
            visualRoot.transform.SetParent(player.transform, false);
            visualRoot.transform.localPosition = new Vector3(0f, -0.1f, 0f);

            var core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            core.name = "Core";
            core.transform.SetParent(visualRoot.transform, false);
            core.transform.localScale = new Vector3(0.58f, 1f, 0.58f);
            core.GetComponent<Renderer>().sharedMaterial = CreateTransparentMaterial("Runner_Core_Mat", new Color(0.3f, 0.9f, 1f, 0.28f));

            var topRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topRing.name = "TopRing";
            topRing.transform.SetParent(visualRoot.transform, false);
            topRing.transform.localPosition = new Vector3(0f, 0.93f, 0f);
            topRing.transform.localScale = new Vector3(0.66f, 0.04f, 0.66f);
            topRing.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Runner_Outline_Mat", new Color(0.05f, 0.05f, 0.05f));

            var bottomRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottomRing.name = "BottomRing";
            bottomRing.transform.SetParent(visualRoot.transform, false);
            bottomRing.transform.localPosition = new Vector3(0f, -0.93f, 0f);
            bottomRing.transform.localScale = new Vector3(0.66f, 0.04f, 0.66f);
            bottomRing.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Runner_Outline_Mat", new Color(0.05f, 0.05f, 0.05f));

            var frontStripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontStripe.name = "FrontStripe";
            frontStripe.transform.SetParent(visualRoot.transform, false);
            frontStripe.transform.localPosition = new Vector3(0f, 0f, 0.57f);
            frontStripe.transform.localScale = new Vector3(0.12f, 2.02f, 0.06f);
            frontStripe.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Runner_Outline_Mat", new Color(0.05f, 0.05f, 0.05f));

            Object.DestroyImmediate(core.GetComponent<Collider>());
            Object.DestroyImmediate(topRing.GetComponent<Collider>());
            Object.DestroyImmediate(bottomRing.GetComponent<Collider>());
            Object.DestroyImmediate(frontStripe.GetComponent<Collider>());

            var controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.height = 2f;
            controller.radius = 0.4f;

            var runner = player.AddComponent<PrototypeRunnerController>();
            SetObjectReference(runner, "gameManager", gameManager);
            SetObjectReference(runner, "inputManager", inputManager);

            var cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
            if (cameraObject.GetComponent<Camera>() == null)
            {
                cameraObject.AddComponent<Camera>();
            }

            cameraObject.name = "Main Camera";
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 2.35f, -5.8f);
            cameraObject.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
            if (cameraObject.GetComponent<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }

            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.82f, 0.91f, 0.98f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 350f;
            camera.fieldOfView = 62f;

            var cameraFollow = cameraObject.GetComponent<PrototypeCameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = cameraObject.AddComponent<PrototypeCameraFollow>();
            }

            SetObjectReference(cameraFollow, "target", player.transform);
            SetObjectReference(cameraFollow, "gameManager", gameManager);

            CreateRoad();
            CreateLaneMarkers();
            CreateLaneDashLines();
            CreateRoadEdgeLines();
            CreateRoadBorders();
            CreateRoadsidePosts();
            CreateCityBackdrop();
            CreateDirectionalLight();

            var chunkTemplates = CreateChunkTemplates();
            var spawnerObject = new GameObject("ChunkSpawner");
            var spawner = spawnerObject.AddComponent<ChunkSpawner>();
            SetObjectReference(spawner, "gameManager", gameManager);
            SetObjectReference(spawner, "runner", player.transform);
            SetObjectReferenceArray(spawner, "chunkPrefabs", chunkTemplates);

            CreateHud(gameManager);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/RunnerPrototype.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/RunnerPrototype.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false),
            };
            AssetDatabase.SaveAssets();
            Selection.activeObject = gameManagerObject;
        }

        private static void CreateHud(PrototypeGameManager gameManager)
        {
            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            var hudObject = new GameObject("PrototypeHUD");
            hudObject.transform.SetParent(canvasObject.transform, false);
            var hud = hudObject.AddComponent<PrototypeHUD>();
            SetObjectReference(hud, "gameManager", gameManager);
            SetObjectReference(hud, "speedText", CreateText("SpeedText", hudObject.transform, new Vector2(120f, -30f)));
            SetObjectReference(hud, "distanceText", CreateText("DistanceText", hudObject.transform, new Vector2(140f, -70f)));
            SetObjectReference(hud, "stateText", CreateText("StateText", hudObject.transform, new Vector2(240f, -110f)));
            SetObjectReference(hud, "comboHintText", CreateText("HintText", hudObject.transform, new Vector2(240f, -150f)));
            SetObjectReference(hud, "diagnosticsText", CreateText("DiagnosticsText", hudObject.transform, new Vector2(240f, -190f)));
            SetObjectReference(hud, "speedSlider", CreateSpeedSlider(hudObject.transform));
        }

        private static Text CreateText(string objectName, Transform parent, Vector2 anchoredPosition)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(520f, 30f);

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = new Color(0.06f, 0.08f, 0.1f);
            text.text = objectName;
            return text;
        }

        private static Slider CreateSpeedSlider(Transform parent)
        {
            var root = new GameObject("SpeedSlider");
            root.transform.SetParent(parent, false);

            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -230f);
            rect.sizeDelta = new Vector2(320f, 20f);

            var slider = root.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;

            var background = CreateImage("Background", root.transform, Vector2.zero, new Vector2(320f, 20f), new Color(0.12f, 0.15f, 0.19f, 0.85f));
            var fillArea = CreateRect("Fill Area", root.transform, new Vector2(5f, -5f), new Vector2(-5f, -5f));
            fillArea.anchorMin = new Vector2(0f, 0f);
            fillArea.anchorMax = new Vector2(1f, 1f);
            fillArea.offsetMin = new Vector2(5f, 5f);
            fillArea.offsetMax = new Vector2(-5f, -5f);
            var fill = CreateImage("Fill", fillArea, Vector2.zero, Vector2.zero, new Color(0.98f, 0.47f, 0.18f));
            fill.rectTransform.anchorMin = new Vector2(0f, 0f);
            fill.rectTransform.anchorMax = new Vector2(1f, 1f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = fill;

            var handleArea = CreateRect("Handle Slide Area", root.transform, Vector2.zero, Vector2.zero);
            handleArea.anchorMin = new Vector2(0f, 0f);
            handleArea.anchorMax = new Vector2(1f, 1f);
            handleArea.offsetMin = Vector2.zero;
            handleArea.offsetMax = Vector2.zero;
            slider.handleRect = null;

            return slider;
        }

        private static Image CreateImage(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
        {
            var imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(parent, false);
            var rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            var image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RectTransform CreateRect(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var rectObject = new GameObject(objectName);
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        private static void CreateLaneMarkers()
        {
            for (int lane = -1; lane <= 1; lane++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = $"LaneMarker_{lane}";
                marker.transform.position = new Vector3(lane * 3f, 0.025f, RoadCenterZ);
                marker.transform.localScale = new Vector3(0.12f, 0.05f, RoadLength);
                marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                    $"LaneMarker_{lane}_Mat",
                    lane == 0 ? new Color(0.32f, 0.82f, 0.95f) : new Color(0.28f, 0.3f, 0.35f));
            }
        }

        private static void CreateLaneDashLines()
        {
            for (int lane = -1; lane <= 1; lane += 2)
            {
                for (int i = 0; i < 34; i++)
                {
                    var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    dash.name = $"LaneDash_{lane}_{i}";
                    dash.transform.position = new Vector3(lane * 1.5f, 0.03f, RoadStartZ + 18f + i * 11f);
                    dash.transform.localScale = new Vector3(0.2f, 0.03f, 5.2f);
                    dash.GetComponent<Renderer>().sharedMaterial = CreateMaterial("LaneDash_Mat", new Color(0.94f, 0.94f, 0.9f));
                }
            }
        }

        private static void CreateRoad()
        {
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Ground";
            road.transform.position = new Vector3(0f, -0.1f, RoadCenterZ);
            road.transform.localScale = new Vector3(10.5f, 0.2f, RoadLength);
            if (road.GetComponent<PrototypeSurface>() == null)
            {
                road.AddComponent<PrototypeSurface>();
            }

            var roadRenderer = road.GetComponent<Renderer>();
            if (roadRenderer != null)
            {
                roadRenderer.sharedMaterial = CreateMaterial("Ground_Mat", new Color(0.17f, 0.19f, 0.23f));
            }
        }

        private static void CreateRoadEdgeLines()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var edgeLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                edgeLine.name = side < 0 ? "RoadEdgeLine_Left" : "RoadEdgeLine_Right";
                edgeLine.transform.position = new Vector3(side * 4.2f, 0.03f, RoadCenterZ);
                edgeLine.transform.localScale = new Vector3(0.18f, 0.04f, RoadLength);
                edgeLine.GetComponent<Renderer>().sharedMaterial = CreateMaterial("RoadEdgeLine_Mat", new Color(0.97f, 0.97f, 0.94f));
            }
        }

        private static void CreateRoadBorders()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
                border.name = side < 0 ? "RoadBorder_Left" : "RoadBorder_Right";
                border.transform.position = new Vector3(side * 5f, 0.55f, RoadCenterZ);
                border.transform.localScale = new Vector3(0.45f, 1.1f, RoadLength);
                border.GetComponent<Renderer>().sharedMaterial = CreateMaterial("RoadBorder_Mat", new Color(0.85f, 0.31f, 0.24f));
            }
        }

        private static void CreateRoadsidePosts()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 30; i++)
                {
                    float z = RoadStartZ + 18f + i * 15f;

                    var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pole.name = $"RoadsidePost_{(side < 0 ? "L" : "R")}_{i}";
                    pole.transform.position = new Vector3(side * 6.2f, 1.15f, z);
                    pole.transform.localScale = new Vector3(0.12f, 2.3f, 0.12f);
                    pole.GetComponent<Renderer>().sharedMaterial = CreateMaterial("RoadsidePost_Mat", new Color(0.72f, 0.75f, 0.8f));

                    var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    lamp.name = $"RoadsideLamp_{(side < 0 ? "L" : "R")}_{i}";
                    lamp.transform.position = new Vector3(side * 6.2f, 2.35f, z);
                    lamp.transform.localScale = new Vector3(0.3f, 0.16f, 0.3f);
                    lamp.GetComponent<Renderer>().sharedMaterial = CreateMaterial("RoadsideLamp_Mat", new Color(0.98f, 0.88f, 0.58f));
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
                    float height = 8f + (i % 4) * 4f;
                    float width = 3.5f + (i % 2) * 1.5f;

                    var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    building.name = $"Building_{(side < 0 ? "L" : "R")}_{i}";
                    building.transform.position = new Vector3(x, height * 0.5f, z);
                    building.transform.localScale = new Vector3(width, height, width);
                    building.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                        $"Building_Mat_{(side < 0 ? "L" : "R")}_{i}",
                        i % 2 == 0 ? new Color(0.36f, 0.43f, 0.5f) : new Color(0.46f, 0.52f, 0.58f));

                    var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    edge.name = $"BuildingEdge_{(side < 0 ? "L" : "R")}_{i}";
                    edge.transform.position = new Vector3(x, height + 0.08f, z);
                    edge.transform.localScale = new Vector3(width + 0.18f, 0.16f, width + 0.18f);
                    edge.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                        $"BuildingEdge_Mat_{(side < 0 ? "L" : "R")}_{i}",
                        new Color(0.88f, 0.92f, 0.98f));
                }
            }

            var horizon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horizon.name = "HorizonWall";
            horizon.transform.position = new Vector3(0f, 12f, 420f);
            horizon.transform.localScale = new Vector3(72f, 24f, 1f);
            horizon.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Horizon_Mat", new Color(0.74f, 0.79f, 0.86f));
        }

        private static GameObject[] CreateChunkTemplates()
        {
            var root = new GameObject("ChunkTemplates");
            var chunks = new GameObject[]
            {
                CreateChunkFlat(root.transform),
                CreateChunkObstacleRun(root.transform),
                CreateChunkRailSection(root.transform),
                CreateChunkObjectPlatform(root.transform),
                CreateChunkMixed(root.transform),
            };

            foreach (var c in chunks)
            {
                c.SetActive(false);
            }

            return chunks;
        }

        private static GameObject CreateChunkFlat(Transform parent)
        {
            var chunk = new GameObject("ChunkFlat");
            chunk.transform.SetParent(parent, false);
            var controller = chunk.AddComponent<ChunkController>();
            SetFloat(controller, "chunkLength", 36f);
            return chunk;
        }

        private static GameObject CreateChunkObstacleRun(Transform parent)
        {
            var chunk = new GameObject("ChunkObstacleRun");
            chunk.transform.SetParent(parent, false);
            var controller = chunk.AddComponent<ChunkController>();
            SetFloat(controller, "chunkLength", 36f);

            AddObstacleToChunk(chunk.transform, -3f, 10f);
            AddObstacleToChunk(chunk.transform, 3f, 22f);
            AddObstacleToChunk(chunk.transform, 0f, 30f);
            return chunk;
        }

        private static GameObject CreateChunkRailSection(Transform parent)
        {
            var chunk = new GameObject("ChunkRailSection");
            chunk.transform.SetParent(parent, false);
            var controller = chunk.AddComponent<ChunkController>();
            SetFloat(controller, "chunkLength", 36f);

            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(chunk.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.4f, 18f);
            rail.transform.localScale = new Vector3(2.5f, 0.08f, 30f);
            var railSurface = rail.AddComponent<PrototypeSurface>();
            SetEnum(railSurface, "surfaceType", 2);
            rail.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Rail_Mat", new Color(0.58f, 0.85f, 0.95f));
            return chunk;
        }

        private static GameObject CreateChunkObjectPlatform(Transform parent)
        {
            var chunk = new GameObject("ChunkObjectPlatform");
            chunk.transform.SetParent(parent, false);
            var controller = chunk.AddComponent<ChunkController>();
            SetFloat(controller, "chunkLength", 36f);

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetParent(chunk.transform, false);
            platform.transform.localPosition = new Vector3(0f, 0.55f, 18f);
            platform.transform.localScale = new Vector3(2.3f, 0.9f, 6f);
            var platformSurface = platform.AddComponent<PrototypeSurface>();
            SetEnum(platformSurface, "surfaceType", 1);
            platform.GetComponent<Renderer>().sharedMaterial = CreateMaterial("ObjectPlatform_Mat", new Color(0.86f, 0.46f, 0.18f));

            AddObstacleToChunk(chunk.transform, -3f, 12f);
            AddObstacleToChunk(chunk.transform, 3f, 12f);
            return chunk;
        }

        private static GameObject CreateChunkMixed(Transform parent)
        {
            var chunk = new GameObject("ChunkMixed");
            chunk.transform.SetParent(parent, false);
            var controller = chunk.AddComponent<ChunkController>();
            SetFloat(controller, "chunkLength", 36f);

            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(chunk.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.4f, 18f);
            rail.transform.localScale = new Vector3(2.5f, 0.08f, 20f);
            var railSurface = rail.AddComponent<PrototypeSurface>();
            SetEnum(railSurface, "surfaceType", 2);
            rail.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Rail_Mat", new Color(0.58f, 0.85f, 0.95f));

            AddObstacleToChunk(chunk.transform, -3f, 8f);
            AddObstacleToChunk(chunk.transform, 3f, 26f);
            return chunk;
        }

        private static void AddObstacleToChunk(Transform chunkTransform, float laneX, float localZ)
        {
            var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obs.name = "Obstacle";
            obs.transform.SetParent(chunkTransform, false);
            obs.transform.localPosition = new Vector3(laneX, 0.65f, localZ);
            obs.transform.localScale = new Vector3(1.8f, 1.3f, 2.8f);
            obs.AddComponent<PrototypeObstacle>();
            var col = obs.GetComponent<BoxCollider>();
            col.center = Vector3.zero;
            col.size = Vector3.one;
            obs.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Obstacle_Mat", new Color(0.96f, 0.3f, 0.2f));
        }

        private static void SetFloat(Object target, string fieldName, float value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectReferenceArray(Object target, string fieldName, GameObject[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateCarObstaclePrefab()
        {
            var root = new GameObject("ObstaclePrefab");
            var obstacle = root.AddComponent<PrototypeObstacle>();
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.65f, 0f);
            collider.size = new Vector3(1.8f, 1.3f, 3f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(1.65f, 0.7f, 2.9f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarBody_Mat", new Color(0.96f, 0.78f, 0.12f));
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 0.98f, -0.1f);
            cabin.transform.localScale = new Vector3(1.3f, 0.55f, 1.45f);
            cabin.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarCabin_Mat", new Color(0.15f, 0.16f, 0.19f));
            Object.DestroyImmediate(cabin.GetComponent<Collider>());

            var windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windshield.name = "Windshield";
            windshield.transform.SetParent(root.transform, false);
            windshield.transform.localPosition = new Vector3(0f, 0.98f, 0.62f);
            windshield.transform.localScale = new Vector3(1.18f, 0.46f, 0.16f);
            windshield.GetComponent<Renderer>().sharedMaterial = CreateTransparentMaterial("CarGlass_Mat", new Color(0.58f, 0.86f, 0.96f, 0.45f));
            Object.DestroyImmediate(windshield.GetComponent<Collider>());

            var rearGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearGlass.name = "RearGlass";
            rearGlass.transform.SetParent(root.transform, false);
            rearGlass.transform.localPosition = new Vector3(0f, 0.98f, -0.78f);
            rearGlass.transform.localScale = new Vector3(1.18f, 0.4f, 0.14f);
            rearGlass.GetComponent<Renderer>().sharedMaterial = CreateTransparentMaterial("CarRearGlass_Mat", new Color(0.58f, 0.86f, 0.96f, 0.4f));
            Object.DestroyImmediate(rearGlass.GetComponent<Collider>());

            var bumperFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bumperFront.name = "FrontBumper";
            bumperFront.transform.SetParent(root.transform, false);
            bumperFront.transform.localPosition = new Vector3(0f, 0.3f, 1.46f);
            bumperFront.transform.localScale = new Vector3(1.72f, 0.16f, 0.1f);
            bumperFront.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarOutline_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(bumperFront.GetComponent<Collider>());

            var bumperRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bumperRear.name = "RearBumper";
            bumperRear.transform.SetParent(root.transform, false);
            bumperRear.transform.localPosition = new Vector3(0f, 0.3f, -1.46f);
            bumperRear.transform.localScale = new Vector3(1.72f, 0.16f, 0.1f);
            bumperRear.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarOutline_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(bumperRear.GetComponent<Collider>());

            var sideStripeLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideStripeLeft.name = "SideStripeLeft";
            sideStripeLeft.transform.SetParent(root.transform, false);
            sideStripeLeft.transform.localPosition = new Vector3(-0.84f, 0.45f, 0f);
            sideStripeLeft.transform.localScale = new Vector3(0.08f, 0.72f, 2.92f);
            sideStripeLeft.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarOutline_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(sideStripeLeft.GetComponent<Collider>());

            var sideStripeRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideStripeRight.name = "SideStripeRight";
            sideStripeRight.transform.SetParent(root.transform, false);
            sideStripeRight.transform.localPosition = new Vector3(0.84f, 0.45f, 0f);
            sideStripeRight.transform.localScale = new Vector3(0.08f, 0.72f, 2.92f);
            sideStripeRight.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarOutline_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(sideStripeRight.GetComponent<Collider>());

            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = $"Wheel_{side}_{axle}";
                    wheel.transform.SetParent(root.transform, false);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    wheel.transform.localPosition = new Vector3(side * 0.92f, 0.3f, axle * 0.95f);
                    wheel.transform.localScale = new Vector3(0.25f, 0.12f, 0.25f);
                    wheel.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarWheel_Mat", new Color(0.08f, 0.08f, 0.09f));
                    Object.DestroyImmediate(wheel.GetComponent<Collider>());
                }
            }

            _ = obstacle;
            return root;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightObject = GameObject.Find("Directional Light") ?? new GameObject("Directional Light");
            var light = lightObject.GetComponent<Light>();
            if (light == null)
            {
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.96f, 0.9f);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
            if (existing != null)
            {
                existing.color = color;
                return existing;
            }

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = name;
            material.color = color;
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            return material;
        }

        private static Material CreateTransparentMaterial(string name, Color color)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
            if (existing != null)
            {
                existing.shader = Shader.Find("Universal Render Pipeline/Unlit");
                existing.color = color;
                existing.SetFloat("_Surface", 1f);
                existing.renderQueue = (int)RenderQueue.Transparent;
                return existing;
            }

            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.name = name;
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.color = color;
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetObjectReference(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Object target, string fieldName, int enumValue)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.enumValueIndex = enumValue;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
