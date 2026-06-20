using UnityEngine;

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

        // --- Follow camera ---
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
        }
        var follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        cam.transform.position = new Vector3(0f, 14f, -12f);
        cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }

    // Builds a simple Mario-style figure from primitives: red cap & shirt,
    // blue overalls, with swinging arm/leg pivots the animator drives.
    // The root carries the physics (Rigidbody + collider), the "Player"
    // tag, the controller and the animator; the visuals are children.
    static GameObject BuildCharacter(Vector3 position)
    {
        var root = new GameObject("Player");
        root.tag = "Player";
        root.transform.position = position;

        var rb = root.AddComponent<Rigidbody>();
        rb.linearDamping = 0.5f;

        var col = root.AddComponent<CapsuleCollider>();
        col.height = 1.6f;
        col.radius = 0.35f;
        col.center = new Vector3(0f, 0.8f, 0f);

        Color red = new Color(0.85f, 0.15f, 0.12f);
        Color blue = new Color(0.15f, 0.25f, 0.8f);
        Color skin = new Color(1f, 0.8f, 0.6f);

        // Body pivot so the animator can bob the torso/head together.
        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);

        MakePart(PrimitiveType.Capsule, body.transform, Vector3.zero,
                 new Vector3(0.45f, 0.35f, 0.45f), blue);                 // torso/overalls
        var head = MakePart(PrimitiveType.Sphere, body.transform, new Vector3(0f, 0.55f, 0f),
                 new Vector3(0.5f, 0.5f, 0.5f), skin);                    // head
        MakePart(PrimitiveType.Cylinder, head.transform, new Vector3(0f, 0.35f, 0f),
                 new Vector3(1.1f, 0.1f, 1.1f), red);                     // cap brim
        MakePart(PrimitiveType.Sphere, head.transform, new Vector3(0f, 0.3f, 0f),
                 new Vector3(1f, 0.7f, 1f), red);                         // cap top

        var leftLeg  = MakeLimb(root.transform, new Vector3(-0.18f, 0.7f, 0f), blue);
        var rightLeg = MakeLimb(root.transform, new Vector3( 0.18f, 0.7f, 0f), blue);
        var leftArm  = MakeLimb(root.transform, new Vector3(-0.45f, 1.4f, 0f), red);
        var rightArm = MakeLimb(root.transform, new Vector3( 0.45f, 1.4f, 0f), red);

        root.AddComponent<PlayerController>();
        var anim = root.AddComponent<CharacterAnimator>();
        anim.body = body.transform;
        anim.leftLeg = leftLeg;   anim.rightLeg = rightLeg;
        anim.leftArm = leftArm;   anim.rightArm = rightArm;

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

    // A limb is an empty pivot at the shoulder/hip with a capsule hanging
    // below it, so rotating the pivot swings the limb from the top.
    static Transform MakeLimb(Transform parent, Vector3 pivotPos, Color color)
    {
        var pivot = new GameObject("Limb");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = pivotPos;
        MakePart(PrimitiveType.Capsule, pivot.transform, new Vector3(0f, -0.3f, 0f),
                 new Vector3(0.18f, 0.3f, 0.18f), color);
        return pivot.transform;
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

// Smoothly trails the player character, keeping a fixed offset.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 offset;

    void Start()
    {
        if (target != null)
            offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, 0.1f);
    }
}
