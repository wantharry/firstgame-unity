using UnityEngine;
using UnityEngine.InputSystem;

// Builds the entire playable level from scratch when the game starts,
// so you can just press Play with no manual scene setup.
//
// It spawns: a ground plane, a Mario-style runner character (runs with
// WASD/arrows), a ring of spinning coins, a GameManager (score HUD), a
// light, and a follow camera. Everything is wired up automatically.
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BuildLevel()
    {
        // --- World lighting setup ---
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.64f, 0.74f, 0.78f);
        RenderSettings.fogDensity = 0.015f;

        // --- Environment ---
        CreateEnvironment();

        // --- Light ---
        var lightGO = new GameObject("Sun");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // --- Player character (Mario-style runner) ---
        var player = BuildCharacter(new Vector3(0f, 0f, 0f));

        // --- Coins in a ring around the player ---
        int coinCount = 8;
        float radius = 8f;
        for (int i = 0; i < coinCount; i++)
        {
            float angle = (i / (float)coinCount) * Mathf.PI * 2f;
            var pos = new Vector3(Mathf.Cos(angle) * radius, 0.6f, Mathf.Sin(angle) * radius);
            CreateCoin(pos);
        }

        // --- Game manager (builds the score HUD) ---
        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        // --- First-person camera ---
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
        }
        cam.nearClipPlane = 0.03f;
        cam.fieldOfView = 72f;
        var firstPerson = cam.gameObject.AddComponent<FirstPersonCamera>();
        firstPerson.target = player.transform;

        var anim = player.GetComponent<CharacterAnimator>();
        if (anim != null)
            anim.rotateToMovement = false;
    }

    // Builds a simple platform-hero figure from primitives: red cap/shirt,
    // blue overalls, gloves and shoes. The root carries physics and the
    // visual child pivots are animated by CharacterAnimator.
    static GameObject BuildCharacter(Vector3 position)
    {
        var root = new GameObject("Player");
        root.tag = "Player";
        root.transform.position = position;

        var rb = root.AddComponent<Rigidbody>();
        rb.linearDamping = 0.5f;

        var col = root.AddComponent<CapsuleCollider>();
        col.height = 1.75f;
        col.radius = 0.42f;
        col.center = new Vector3(0f, 0.88f, 0f);

        Color red = new Color(0.85f, 0.12f, 0.10f);
        Color blue = new Color(0.10f, 0.22f, 0.78f);
        Color skin = new Color(1f, 0.78f, 0.58f);
        Color brown = new Color(0.34f, 0.16f, 0.06f);
        Color white = new Color(0.96f, 0.94f, 0.88f);
        Color black = new Color(0.04f, 0.03f, 0.02f);

        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 1.02f, 0f);

        MakePart(PrimitiveType.Capsule, body.transform, new Vector3(0f, -0.05f, 0f),
                 new Vector3(0.52f, 0.42f, 0.42f), blue);                 // rounded overalls
        MakePart(PrimitiveType.Capsule, body.transform, new Vector3(0f, 0.22f, 0f),
                 new Vector3(0.58f, 0.25f, 0.44f), red);                  // shirt/chest
        MakePart(PrimitiveType.Cube, body.transform, new Vector3(-0.18f, 0.17f, 0.33f),
                 new Vector3(0.09f, 0.36f, 0.06f), blue);                 // left overall strap
        MakePart(PrimitiveType.Cube, body.transform, new Vector3(0.18f, 0.17f, 0.33f),
                 new Vector3(0.09f, 0.36f, 0.06f), blue);                 // right overall strap
        MakePart(PrimitiveType.Sphere, body.transform, new Vector3(-0.11f, 0.29f, 0.37f),
                 new Vector3(0.08f, 0.08f, 0.08f), new Color(1f, 0.85f, 0.1f));
        MakePart(PrimitiveType.Sphere, body.transform, new Vector3(0.11f, 0.29f, 0.37f),
                 new Vector3(0.08f, 0.08f, 0.08f), new Color(1f, 0.85f, 0.1f));

        var head = MakePart(PrimitiveType.Sphere, body.transform, new Vector3(0f, 0.72f, 0f),
                 new Vector3(0.48f, 0.48f, 0.48f), skin);
        MakePart(PrimitiveType.Sphere, head.transform, new Vector3(0f, 0.27f, 0f),
                 new Vector3(1.03f, 0.62f, 1.03f), red);                  // cap top
        MakePart(PrimitiveType.Cube, head.transform, new Vector3(0f, 0.16f, 0.42f),
                 new Vector3(0.75f, 0.12f, 0.38f), red);                  // forward cap brim
        MakePart(PrimitiveType.Sphere, head.transform, new Vector3(-0.14f, 0.03f, 0.42f),
                 new Vector3(0.06f, 0.08f, 0.035f), black);               // left eye
        MakePart(PrimitiveType.Sphere, head.transform, new Vector3(0.14f, 0.03f, 0.42f),
                 new Vector3(0.06f, 0.08f, 0.035f), black);               // right eye
        MakePart(PrimitiveType.Sphere, head.transform, new Vector3(0f, -0.05f, 0.47f),
                 new Vector3(0.16f, 0.11f, 0.11f), skin);                 // nose
        MakePart(PrimitiveType.Capsule, head.transform, new Vector3(0f, -0.16f, 0.44f),
                 new Vector3(0.25f, 0.04f, 0.055f), brown);               // moustache

        var leftLeg = MakeLeg(root.transform, new Vector3(-0.18f, 0.78f, 0f), blue, brown, out var leftFoot);
        var rightLeg = MakeLeg(root.transform, new Vector3(0.18f, 0.78f, 0f), blue, brown, out var rightFoot);
        var leftArm = MakeArm(root.transform, new Vector3(-0.47f, 1.42f, 0f), red, white, out var leftHand);
        var rightArm = MakeArm(root.transform, new Vector3(0.47f, 1.42f, 0f), red, white, out var rightHand);

        root.AddComponent<PlayerController>();
        var anim = root.AddComponent<CharacterAnimator>();
        anim.body = body.transform;
        anim.leftLeg = leftLeg;
        anim.rightLeg = rightLeg;
        anim.leftArm = leftArm;
        anim.rightArm = rightArm;
        anim.leftFoot = leftFoot;
        anim.rightFoot = rightFoot;
        anim.leftHand = leftHand;
        anim.rightHand = rightHand;

        return root;
    }

    // Creates a visual-only primitive (collider stripped) parented under `parent`.
    static GameObject MakePart(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = color;
        return go;
    }

    static Transform MakeLeg(Transform parent, Vector3 pivotPos, Color pantsColor, Color shoeColor, out Transform foot)
    {
        var pivot = new GameObject("Leg Pivot");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = pivotPos;

        MakePart(PrimitiveType.Capsule, pivot.transform, new Vector3(0f, -0.33f, 0f),
                 new Vector3(0.20f, 0.34f, 0.20f), pantsColor);
        var footGO = MakePart(PrimitiveType.Cube, pivot.transform, new Vector3(0f, -0.68f, 0.10f),
                 new Vector3(0.30f, 0.13f, 0.46f), shoeColor);
        foot = footGO.transform;
        return pivot.transform;
    }

    static Transform MakeArm(Transform parent, Vector3 pivotPos, Color sleeveColor, Color gloveColor, out Transform hand)
    {
        var pivot = new GameObject("Arm Pivot");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = pivotPos;

        MakePart(PrimitiveType.Capsule, pivot.transform, new Vector3(0f, -0.28f, 0f),
                 new Vector3(0.17f, 0.30f, 0.17f), sleeveColor);
        var handGO = MakePart(PrimitiveType.Sphere, pivot.transform, new Vector3(0f, -0.58f, 0.03f),
                 new Vector3(0.22f, 0.18f, 0.22f), gloveColor);
        hand = handGO.transform;
        return pivot.transform;
    }

    static void CreateEnvironment()
    {
        Color dirt = new Color(0.42f, 0.30f, 0.18f);
        Color stone = new Color(0.38f, 0.39f, 0.36f);
        Color wood = new Color(0.34f, 0.18f, 0.08f);
        Color bark = new Color(0.24f, 0.13f, 0.06f);
        Color pine = new Color(0.03f, 0.23f, 0.09f);
        Color leaf = new Color(0.08f, 0.34f, 0.12f);
        Color roofRed = new Color(0.42f, 0.05f, 0.035f);
        Color plaster = new Color(0.62f, 0.55f, 0.42f);

        var world = new GameObject("Environment");
        CreateTerrain(world.transform);


        Vector3[] treePositions =
        {
            new Vector3(-15f, 0f, -14f), new Vector3(-11f, 0f, -16f), new Vector3(-16f, 0f, -7f),
            new Vector3(12f, 0f, -15f), new Vector3(16f, 0f, -9f), new Vector3(15f, 0f, 4f),
            new Vector3(-16f, 0f, 9f), new Vector3(-9f, 0f, 16f), new Vector3(8f, 0f, 16f),
            new Vector3(16f, 0f, 13f), new Vector3(-3.5f, 0f, -15f), new Vector3(4f, 0f, -16f)
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            float scale = 0.85f + (i % 5) * 0.12f;
            bool pineTree = i % 3 != 1;
            CreateTexturedTree(world.transform, treePositions[i], scale, pineTree ? pine : leaf, bark, pineTree, i);
        }

        CreateCabin(world.transform, new Vector3(-10.5f, 0f, 3.3f), 25f, plaster, roofRed, wood);
        CreateCabin(world.transform, new Vector3(10.5f, 0f, -4.8f), -18f, new Color(0.52f, 0.58f, 0.48f), new Color(0.16f, 0.17f, 0.18f), wood);

        Vector3[] rockPositions =
        {
            new Vector3(-5f, 0.18f, -9f), new Vector3(4.5f, 0.16f, -10f), new Vector3(10f, 0.14f, 7f),
            new Vector3(-9f, 0.12f, 7f), new Vector3(2f, 0.12f, 11f), new Vector3(-14f, 0.13f, 0f),
            new Vector3(14f, 0.14f, -1f), new Vector3(-2f, 0.12f, -13f)
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            var rock = MakeWorldPart(PrimitiveType.Sphere, world.transform, rockPositions[i],
                                     new Vector3(0.75f + i * 0.03f, 0.28f, 0.55f), stone, "Mossy Rock");
            rock.transform.rotation = Quaternion.Euler(0f, i * 33f, 0f);
            MakeWorldPart(PrimitiveType.Sphere, rock.transform, new Vector3(0.15f, 0.28f, 0.05f),
                          new Vector3(0.45f, 0.06f, 0.35f), new Color(0.10f, 0.28f, 0.10f), "Moss Patch");
        }

        CreateFenceLine(world.transform, new Vector3(-7f, 0f, -15.5f), 9, 1.35f, true, wood);
        CreateFenceLine(world.transform, new Vector3(3f, 0f, 15.5f), 8, 1.35f, true, wood);
        CreateFenceLine(world.transform, new Vector3(-15.5f, 0f, -3.5f), 7, 1.35f, false, wood);

        // Grass and flowers now come from the imported GrassFlowersFREE textures.
        CreateAssetGrassFlowers(world.transform);
        CreateSmallProps(world.transform, wood, stone);
    }

    static void CreateTerrain(Transform parent)
    {
        var terrainData = new TerrainData();
        terrainData.heightmapResolution = 129;
        terrainData.size = new Vector3(48f, 1.8f, 48f);

        float[,] heights = new float[129, 129];
        for (int y = 0; y < 129; y++)
        {
            for (int x = 0; x < 129; x++)
            {
                float nx = x / 128f;
                float ny = y / 128f;
                float roll = Mathf.PerlinNoise(nx * 4.2f + 2.1f, ny * 4.2f + 8.3f) * 0.028f;
                float fine = Mathf.PerlinNoise(nx * 14f, ny * 14f) * 0.007f;
                heights[y, x] = roll + fine;
            }
        }
        terrainData.SetHeights(0, 0, heights);

        var grassTexture = LoadTexture("Assets/ALP_Assets/GrassFlowersFREE/Textures/Ground/Grass01_BigUV.png");
        if (grassTexture == null)
            grassTexture = MakeSolidTexture(new Color(0.17f, 0.36f, 0.16f), new Color(0.24f, 0.45f, 0.20f));
        var terrainLayer = new TerrainLayer();
        terrainLayer.diffuseTexture = grassTexture;
        terrainLayer.tileSize = new Vector2(20f, 20f);
        terrainData.terrainLayers = new[] { terrainLayer };

        DetailPrototype[] prototypes =
        {
            CreateGrassDetail("Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass01.tga", 0.55f, 1.15f, 0.45f, 0.95f),
            CreateGrassDetail("Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass02.tga", 0.5f, 1.05f, 0.4f, 0.85f),
            CreateGrassDetail("Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower01.tga", 0.45f, 0.95f, 0.35f, 0.8f),
            CreateGrassDetail("Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower04.tga", 0.45f, 0.95f, 0.35f, 0.8f),
            CreateGrassDetail("Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower08.tga", 0.45f, 0.95f, 0.35f, 0.8f)
        };
        terrainData.detailPrototypes = prototypes;
        terrainData.SetDetailResolution(128, 8);

        for (int layer = 0; layer < prototypes.Length; layer++)
        {
            int[,] details = new int[128, 128];
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.13f + layer * 7.1f, y * 0.13f + layer * 3.4f);
                    int density = layer < 2 ? 3 : 1;
                    details[y, x] = noise > (layer < 2 ? 0.24f : 0.68f) ? density : 0;
                }
            }
            terrainData.SetDetailLayer(0, 0, layer, details);
        }

        var terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        terrainGO.name = "Grass Terrain";
        terrainGO.transform.SetParent(parent, false);
        terrainGO.transform.position = new Vector3(-24f, -0.08f, -24f);
        var terrain = terrainGO.GetComponent<Terrain>();
        terrain.drawInstanced = true;
        terrain.detailObjectDensity = 0.65f;
        terrain.detailObjectDistance = 45f;
    }

    static Texture2D MakeSolidTexture(Color a, Color b)
    {
        var texture = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.35f, y * 0.35f);
                texture.SetPixel(x, y, Color.Lerp(a, b, n));
            }
        }
        texture.Apply();
        return texture;
    }

    static DetailPrototype CreateGrassDetail(string assetPath, float minHeight, float maxHeight, float minWidth, float maxWidth)
    {
        var prototype = new DetailPrototype();
        prototype.prototypeTexture = LoadTexture(assetPath);
        if (prototype.prototypeTexture == null)
            prototype.prototypeTexture = MakeGrassBladeTexture();
        prototype.renderMode = DetailRenderMode.GrassBillboard;
        prototype.usePrototypeMesh = false;
        prototype.healthyColor = Color.white;
        prototype.dryColor = new Color(0.76f, 0.70f, 0.48f);
        prototype.minHeight = minHeight;
        prototype.maxHeight = maxHeight;
        prototype.minWidth = minWidth;
        prototype.maxWidth = maxWidth;
        return prototype;
    }

    static Texture2D LoadTexture(string assetPath)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#else
        return null;
#endif
    }
    static Texture2D MakeGrassBladeTexture()
    {
        var texture = new Texture2D(16, 32, TextureFormat.ARGB32, false);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float center = Mathf.Abs(x - 7.5f) / 7.5f;
                float taper = y / 31f;
                float alpha = center < Mathf.Lerp(0.55f, 0.08f, taper) ? 1f : 0f;
                texture.SetPixel(x, y, new Color(0.24f, 0.62f, 0.19f, alpha));
            }
        }
        texture.Apply();
        return texture;
    }

    static void CreateTexturedTree(Transform parent, Vector3 position, float scale, Color leafColor, Color trunkColor, bool pineTree, int seed)
    {
        var tree = new GameObject(pineTree ? "Textured Pine Tree" : "Textured Broadleaf Tree");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;

        CreateCylinderBetween(tree.transform, new Vector3(0f, 0f, 0f), new Vector3(0f, pineTree ? 2.9f : 2.35f, 0f), pineTree ? 0.14f : 0.18f, trunkColor, "Textured Bark Trunk");
        CreateCylinderBetween(tree.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.65f, 1.85f, 0.18f), 0.045f, trunkColor, "Textured Branch");
        CreateCylinderBetween(tree.transform, new Vector3(0f, 1.25f, 0f), new Vector3(-0.58f, 1.95f, -0.2f), 0.04f, trunkColor, "Textured Branch");

        Material canopyMaterial = CreateCutoutGrassMaterial(pineTree ? MakePineCanopyTexture(leafColor, seed) : MakeLeafCanopyTexture(leafColor, seed));
        canopyMaterial.name = pineTree ? "Runtime Pine Canopy" : "Runtime Leaf Canopy";

        if (pineTree)
        {
            CreateTreeBillboard(tree.transform, new Vector3(0f, 2.15f, 0f), 2.2f, 3.0f, 0f, canopyMaterial);
            CreateTreeBillboard(tree.transform, new Vector3(0f, 2.05f, 0f), 2.0f, 2.8f, 60f, canopyMaterial);
        }
        else
        {
            CreateTreeBillboard(tree.transform, new Vector3(0f, 2.25f, 0f), 2.6f, 2.25f, 0f, canopyMaterial);
            CreateTreeBillboard(tree.transform, new Vector3(0.34f, 2.05f, 0.1f), 1.8f, 1.65f, 65f, canopyMaterial);
            CreateTreeBillboard(tree.transform, new Vector3(-0.36f, 2.0f, -0.08f), 1.75f, 1.6f, 125f, canopyMaterial);
        }
    }

    static void CreateTreeBillboard(Transform parent, Vector3 localPosition, float width, float height, float yaw, Material material)
    {
        var billboard = GameObject.CreatePrimitive(PrimitiveType.Quad);
        billboard.name = "Textured Tree Canopy";
        Object.Destroy(billboard.GetComponent<Collider>());
        billboard.transform.SetParent(parent, false);
        billboard.transform.localPosition = localPosition;
        billboard.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        billboard.transform.localScale = new Vector3(width, height, 1f);
        billboard.GetComponent<Renderer>().sharedMaterial = material;
    }

    static Texture2D MakeLeafCanopyTexture(Color baseColor, int seed)
    {
        var texture = new Texture2D(96, 96, TextureFormat.ARGB32, false);
        Color dark = Color.Lerp(baseColor, Color.black, 0.35f);
        Color light = Color.Lerp(baseColor, new Color(0.5f, 0.8f, 0.35f), 0.35f);
        for (int y = 0; y < 96; y++)
        {
            for (int x = 0; x < 96; x++)
            {
                float nx = (x - 47.5f) / 47.5f;
                float ny = (y - 47.5f) / 47.5f;
                float blob = 1.05f - (nx * nx * 0.82f + ny * ny * 1.15f);
                float noise = Mathf.PerlinNoise(x * 0.095f + seed, y * 0.095f + seed * 0.37f);
                float alpha = blob + (noise - 0.5f) * 0.42f > 0.12f ? 1f : 0f;
                texture.SetPixel(x, y, alpha > 0f ? Color.Lerp(dark, light, noise) : new Color(0f, 0f, 0f, 0f));
            }
        }
        texture.Apply();
        return texture;
    }

    static Texture2D MakePineCanopyTexture(Color baseColor, int seed)
    {
        var texture = new Texture2D(96, 128, TextureFormat.ARGB32, false);
        Color dark = Color.Lerp(baseColor, Color.black, 0.28f);
        Color light = Color.Lerp(baseColor, new Color(0.18f, 0.52f, 0.16f), 0.42f);
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 96; x++)
            {
                float height = y / 127f;
                float center = Mathf.Abs(x - 47.5f) / 47.5f;
                float tier = Mathf.Sin(height * Mathf.PI * 7f) * 0.08f;
                float width = Mathf.Lerp(0.08f, 0.82f, 1f - height) + tier;
                float noise = Mathf.PerlinNoise(x * 0.11f + seed, y * 0.08f + seed * 0.29f);
                float alpha = center < width + (noise - 0.5f) * 0.16f && height > 0.03f ? 1f : 0f;
                texture.SetPixel(x, y, alpha > 0f ? Color.Lerp(dark, light, noise) : new Color(0f, 0f, 0f, 0f));
            }
        }
        texture.Apply();
        return texture;
    }
    static void CreateDetailedTree(Transform parent, Vector3 position, float scale, Color leafColor, Color trunkColor, bool pineTree, int seed)
    {
        var tree = new GameObject(pineTree ? "Pine Tree" : "Broadleaf Tree");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;

        CreateCylinderBetween(tree.transform, new Vector3(0f, 0f, 0f), new Vector3(0f, 2.6f, 0f), 0.16f, trunkColor, "Textured Trunk");
        CreateCylinderBetween(tree.transform, new Vector3(0f, 1.35f, 0f), new Vector3(0.75f, 2.05f, 0.28f), 0.055f, trunkColor, "Branch");
        CreateCylinderBetween(tree.transform, new Vector3(0f, 1.65f, 0f), new Vector3(-0.7f, 2.25f, -0.15f), 0.05f, trunkColor, "Branch");
        CreateCylinderBetween(tree.transform, new Vector3(0f, 1.95f, 0f), new Vector3(0.15f, 2.55f, -0.65f), 0.045f, trunkColor, "Branch");

        if (pineTree)
        {
            for (int i = 0; i < 4; i++)
            {
                MakeWorldPart(PrimitiveType.Capsule, tree.transform, new Vector3(0f, 1.4f + i * 0.42f, 0f),
                              new Vector3(1.35f - i * 0.22f, 0.38f, 1.35f - i * 0.22f), leafColor, "Pine Foliage");
            }
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                float angle = (i / 9f) * Mathf.PI * 2f + seed * 0.7f;
                float radius = 0.25f + (i % 3) * 0.28f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 2.25f + Mathf.Sin(i * 1.7f) * 0.18f, Mathf.Sin(angle) * radius);
                MakeWorldPart(PrimitiveType.Sphere, tree.transform, pos,
                              new Vector3(0.95f, 0.72f, 0.95f), leafColor, "Leaf Cluster");
            }
        }
    }

    static void CreateCabin(Transform parent, Vector3 position, float yRotation, Color wallColor, Color roofColor, Color trimColor)
    {
        var cabin = new GameObject("Detailed Cabin");
        cabin.transform.SetParent(parent, false);
        cabin.transform.position = position;
        cabin.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0f, 1f, 0f),
                      new Vector3(3.8f, 2f, 3f), wallColor, "Cabin Walls");
        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0f, 2.18f, 0f),
                      new Vector3(4.35f, 0.42f, 3.55f), roofColor, "Broad Roof");
        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0f, 2.52f, 0f),
                      new Vector3(3.75f, 0.26f, 2.9f), roofColor, "Roof Cap");
        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(1.15f, 2.95f, -0.6f),
                      new Vector3(0.42f, 0.9f, 0.42f), new Color(0.20f, 0.20f, 0.18f), "Chimney");

        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0f, 0.62f, 1.54f),
                      new Vector3(0.72f, 1.22f, 0.08f), trimColor, "Wood Door");
        MakeWorldPart(PrimitiveType.Sphere, cabin.transform, new Vector3(0.24f, 0.62f, 1.60f),
                      new Vector3(0.06f, 0.06f, 0.025f), new Color(0.95f, 0.75f, 0.18f), "Door Knob");

        CreateWindow(cabin.transform, new Vector3(-1.1f, 1.25f, 1.56f));
        CreateWindow(cabin.transform, new Vector3(1.1f, 1.25f, 1.56f));
        CreateWindow(cabin.transform, new Vector3(-1.94f, 1.22f, -0.4f), true);

        for (int i = 0; i < 5; i++)
        {
            MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0f, 0.35f + i * 0.35f, 1.585f),
                          new Vector3(3.9f, 0.035f, 0.035f), trimColor, "Wall Plank");
        }

        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0f, 0.08f, 1.85f),
                      new Vector3(2.4f, 0.16f, 1.1f), trimColor, "Front Porch");
        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(-0.9f, 0.35f, 2.08f),
                      new Vector3(0.16f, 0.7f, 0.16f), trimColor, "Porch Post");
        MakeWorldPart(PrimitiveType.Cube, cabin.transform, new Vector3(0.9f, 0.35f, 2.08f),
                      new Vector3(0.16f, 0.7f, 0.16f), trimColor, "Porch Post");
    }

    static void CreateWindow(Transform parent, Vector3 pos, bool side = false)
    {
        var window = MakeWorldPart(PrimitiveType.Cube, parent, pos,
                                   new Vector3(0.72f, 0.5f, 0.07f), new Color(0.56f, 0.80f, 0.95f), "Glass Window");
        if (side) window.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        MakeWorldPart(PrimitiveType.Cube, window.transform, Vector3.zero,
                      new Vector3(1.14f, 0.08f, 1.18f), new Color(0.16f, 0.09f, 0.04f), "Window Frame");
    }

    static void CreateFenceLine(Transform parent, Vector3 start, int count, float spacing, bool horizontal, Color color)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = start + (horizontal ? new Vector3(i * spacing, 0f, 0f) : new Vector3(0f, 0f, i * spacing));
            MakeWorldPart(PrimitiveType.Cube, parent, pos + new Vector3(0f, 0.45f, 0f),
                          new Vector3(0.16f, 0.9f, 0.16f), color, "Fence Post");
        }

        Vector3 railCenter = start + (horizontal ? new Vector3((count - 1) * spacing * 0.5f, 0.7f, 0f) : new Vector3(0f, 0.7f, (count - 1) * spacing * 0.5f));
        Vector3 railScale = horizontal ? new Vector3(count * spacing, 0.13f, 0.13f) : new Vector3(0.13f, 0.13f, count * spacing);
        MakeWorldPart(PrimitiveType.Cube, parent, railCenter, railScale, color, "Fence Rail");
        MakeWorldPart(PrimitiveType.Cube, parent, railCenter + new Vector3(0f, -0.32f, 0f), railScale, color, "Fence Rail");
    }

    static void CreateAssetGrassFlowers(Transform parent)
    {
        string[] texturePaths =
        {
            "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass01.tga",
            "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass02.tga",
            "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower01.tga",
            "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower04.tga",
            "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower08.tga"
        };

        Material[] materials = new Material[texturePaths.Length];
        for (int i = 0; i < texturePaths.Length; i++)
            materials[i] = CreateCutoutGrassMaterial(LoadTexture(texturePaths[i]));

        var group = new GameObject("Grass Flowers Pack Free Clusters");
        group.transform.SetParent(parent, false);

        for (int i = 0; i < 180; i++)
        {
            float x = Mathf.Sin(i * 12.989f) * 21.5f;
            float z = Mathf.Cos(i * 8.233f) * 21.5f;            float height = 0.45f + Mathf.PerlinNoise(i * 0.19f, 2.7f) * 0.55f;
            float width = height * (0.55f + Mathf.PerlinNoise(5.1f, i * 0.17f) * 0.35f);
            var material = materials[i % materials.Length];
            CreateGrassBillboard(group.transform, new Vector3(x, 0.08f, z), width, height, (i * 37f) % 360f, material);
        }
    }

    static Material CreateCutoutGrassMaterial(Texture2D texture)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Transparent Cutout");
        if (shader == null)
            shader = Shader.Find("Unlit/Transparent");
        if (shader == null)
            shader = Shader.Find("Standard");

        var material = new Material(shader);
        material.name = texture != null ? texture.name + " Runtime Grass" : "Runtime Grass Fallback";
        material.mainTexture = texture != null ? texture : MakeGrassBladeTexture();
        material.color = Color.white;
        material.SetFloat("_Cutoff", 0.35f);
        material.SetFloat("_AlphaClip", 1f);
        material.SetFloat("_Surface", 1f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        material.EnableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = 2450;
        return material;
    }

    static void CreateGrassBillboard(Transform parent, Vector3 position, float width, float height, float yaw, Material material)
    {
        var clump = new GameObject("Grass Flower Asset Clump");
        clump.transform.SetParent(parent, false);
        clump.transform.localPosition = position;
        clump.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        CreateGrassQuad(clump.transform, Quaternion.identity, width, height, material);
        CreateGrassQuad(clump.transform, Quaternion.Euler(0f, 90f, 0f), width, height, material);
    }

    static void CreateGrassQuad(Transform parent, Quaternion rotation, float width, float height, Material material)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Grass Flower Billboard";
        Object.Destroy(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        quad.transform.localRotation = rotation;
        quad.transform.localScale = new Vector3(width, height, 1f);
        quad.GetComponent<Renderer>().sharedMaterial = material;
    }

    static void CreateSmallProps(Transform parent, Color wood, Color stone)
    {
        CreateBarrel(parent, new Vector3(-8.2f, 0f, 5.8f), wood);
        CreateBarrel(parent, new Vector3(8.2f, 0f, -6.2f), wood);
        MakeWorldPart(PrimitiveType.Cube, parent, new Vector3(-7.4f, 0.25f, 6.8f), new Vector3(0.75f, 0.5f, 0.75f), wood, "Supply Crate");
        MakeWorldPart(PrimitiveType.Cube, parent, new Vector3(7.8f, 0.22f, -7.2f), new Vector3(0.65f, 0.44f, 0.65f), wood, "Supply Crate");
        MakeWorldPart(PrimitiveType.Sphere, parent, new Vector3(5.7f, 0.18f, 8.9f), new Vector3(0.55f, 0.24f, 0.42f), stone, "Pebble Cluster");
    }

    static void CreateBarrel(Transform parent, Vector3 position, Color color)
    {
        var barrel = MakeWorldPart(PrimitiveType.Cylinder, parent, position + new Vector3(0f, 0.45f, 0f),
                                   new Vector3(0.42f, 0.45f, 0.42f), color, "Barrel");
        MakeWorldPart(PrimitiveType.Cylinder, barrel.transform, new Vector3(0f, 0.53f, 0f),
                      new Vector3(1.05f, 0.035f, 1.05f), new Color(0.12f, 0.12f, 0.11f), "Metal Band");
        MakeWorldPart(PrimitiveType.Cylinder, barrel.transform, new Vector3(0f, -0.53f, 0f),
                      new Vector3(1.05f, 0.035f, 1.05f), new Color(0.12f, 0.12f, 0.11f), "Metal Band");
    }

    static void CreateCylinderBetween(Transform parent, Vector3 start, Vector3 end, float radius, Color color, string name)
    {
        Vector3 delta = end - start;
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = start + delta * 0.5f;
        cylinder.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        cylinder.transform.localScale = new Vector3(radius, delta.magnitude * 0.5f, radius);
        cylinder.GetComponent<Renderer>().material.color = color;
    }

    static GameObject MakeWorldPart(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color color, string name)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = color;
        return go;
    }
    static void CreateCoin(Vector3 position)
    {
        var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.position = position;
        coin.transform.localScale = new Vector3(0.8f, 0.08f, 0.8f);
        coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // face up like a coin
        coin.GetComponent<Renderer>().material.color = new Color(1f, 0.85f, 0.1f);

        var col = coin.GetComponent<Collider>();
        col.isTrigger = true;

        coin.AddComponent<Coin>();
    }
}

// Mouse-look first-person camera mounted on the player.
public class FirstPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 eyeOffset = new Vector3(0f, 1.55f, 0.22f);
    public float mouseSensitivity = 0.12f;
    public float pitchLimit = 78f;

    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 delta = mouse.delta.ReadValue();
            target.Rotate(Vector3.up, delta.x * mouseSensitivity, Space.World);
            pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -pitchLimit, pitchLimit);
        }

        transform.position = target.TransformPoint(eyeOffset);
        transform.rotation = target.rotation * Quaternion.Euler(pitch, 0f, 0f);
    }
}










