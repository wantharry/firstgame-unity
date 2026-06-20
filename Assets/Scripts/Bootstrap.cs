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
        // --- Ground ---
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f); // 30x30 units
        var groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.material.color = new Color(0.20f, 0.45f, 0.30f);

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
        Color dirt = new Color(0.48f, 0.34f, 0.19f);
        Color stone = new Color(0.42f, 0.43f, 0.40f);
        Color wood = new Color(0.42f, 0.23f, 0.10f);
        Color leaves = new Color(0.08f, 0.38f, 0.14f);
        Color darkLeaves = new Color(0.04f, 0.26f, 0.10f);
        Color roofRed = new Color(0.55f, 0.08f, 0.06f);
        Color plaster = new Color(0.78f, 0.67f, 0.48f);

        var world = new GameObject("Environment");

        // A simple cross path gives the player a readable route through the field.
        MakeWorldPart(PrimitiveType.Cube, world.transform, new Vector3(0f, 0.015f, 0f),
                      new Vector3(24f, 0.03f, 2.2f), dirt, "East-West Dirt Path");
        MakeWorldPart(PrimitiveType.Cube, world.transform, new Vector3(0f, 0.02f, 0f),
                      new Vector3(2.2f, 0.03f, 24f), dirt, "North-South Dirt Path");

        Vector3[] treePositions =
        {
            new Vector3(-12f, 0f, -11f), new Vector3(-8f, 0f, -13f), new Vector3(-13f, 0f, -5f),
            new Vector3(10f, 0f, -12f), new Vector3(13f, 0f, -7f), new Vector3(12f, 0f, 4f),
            new Vector3(-12f, 0f, 8f), new Vector3(-7f, 0f, 12f), new Vector3(7f, 0f, 13f),
            new Vector3(13f, 0f, 11f)
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            float scale = 0.85f + (i % 4) * 0.12f;
            CreateTree(world.transform, treePositions[i], scale, i % 2 == 0 ? leaves : darkLeaves, wood);
        }

        CreateHouse(world.transform, new Vector3(-10f, 0f, 2.5f), 25f, plaster, roofRed, wood);
        CreateHouse(world.transform, new Vector3(9.5f, 0f, -3.5f), -18f, new Color(0.68f, 0.74f, 0.62f), new Color(0.22f, 0.23f, 0.28f), wood);

        Vector3[] rockPositions =
        {
            new Vector3(-5f, 0.18f, -9f), new Vector3(4.5f, 0.16f, -10f), new Vector3(10f, 0.14f, 7f),
            new Vector3(-9f, 0.12f, 6f), new Vector3(2f, 0.12f, 11f), new Vector3(-13f, 0.13f, 0f)
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            var rock = MakeWorldPart(PrimitiveType.Sphere, world.transform, rockPositions[i],
                                     new Vector3(0.7f + i * 0.04f, 0.28f, 0.55f), stone, "Rock");
            rock.transform.rotation = Quaternion.Euler(0f, i * 33f, 0f);
        }

        CreateFenceLine(world.transform, new Vector3(-6f, 0f, -14f), 8, 1.4f, true, wood);
        CreateFenceLine(world.transform, new Vector3(4f, 0f, 14f), 7, 1.4f, true, wood);
        CreateFenceLine(world.transform, new Vector3(-14f, 0f, -3f), 6, 1.4f, false, wood);
    }

    static void CreateTree(Transform parent, Vector3 position, float scale, Color leafColor, Color trunkColor)
    {
        var tree = new GameObject("Tree");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;

        MakeWorldPart(PrimitiveType.Cylinder, tree.transform, new Vector3(0f, 0.75f, 0f),
                      new Vector3(0.32f, 0.8f, 0.32f), trunkColor, "Trunk");
        MakeWorldPart(PrimitiveType.Sphere, tree.transform, new Vector3(0f, 1.8f, 0f),
                      new Vector3(1.25f, 1.05f, 1.25f), leafColor, "Leaf Crown");
        MakeWorldPart(PrimitiveType.Sphere, tree.transform, new Vector3(-0.45f, 1.55f, 0.12f),
                      new Vector3(0.8f, 0.65f, 0.8f), leafColor, "Leaf Crown");
        MakeWorldPart(PrimitiveType.Sphere, tree.transform, new Vector3(0.45f, 1.55f, -0.1f),
                      new Vector3(0.8f, 0.65f, 0.8f), leafColor, "Leaf Crown");
    }

    static void CreateHouse(Transform parent, Vector3 position, float yRotation, Color wallColor, Color roofColor, Color trimColor)
    {
        var house = new GameObject("Small House");
        house.transform.SetParent(parent, false);
        house.transform.position = position;
        house.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        MakeWorldPart(PrimitiveType.Cube, house.transform, new Vector3(0f, 1f, 0f),
                      new Vector3(3.2f, 2f, 2.8f), wallColor, "House Body");
        MakeWorldPart(PrimitiveType.Cube, house.transform, new Vector3(0f, 2.15f, 0f),
                      new Vector3(3.8f, 0.45f, 3.3f), roofColor, "Roof");
        MakeWorldPart(PrimitiveType.Cube, house.transform, new Vector3(0f, 2.45f, 0f),
                      new Vector3(3.35f, 0.3f, 2.75f), roofColor, "Roof Ridge");
        MakeWorldPart(PrimitiveType.Cube, house.transform, new Vector3(0f, 0.55f, 1.43f),
                      new Vector3(0.65f, 1.1f, 0.08f), trimColor, "Door");
        MakeWorldPart(PrimitiveType.Cube, house.transform, new Vector3(-0.95f, 1.2f, 1.44f),
                      new Vector3(0.55f, 0.45f, 0.08f), new Color(0.65f, 0.9f, 1f), "Window");
        MakeWorldPart(PrimitiveType.Cube, house.transform, new Vector3(0.95f, 1.2f, 1.44f),
                      new Vector3(0.55f, 0.45f, 0.08f), new Color(0.65f, 0.9f, 1f), "Window");
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






