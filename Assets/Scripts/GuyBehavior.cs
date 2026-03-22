using UnityEngine;

public class GuyBehavior : MonoBehaviour
{
    enum AnimationState { INIT, WALKING, FALLING, DIGDOWN, DIGFORWARD, DIGQUER, BLOCKER, UMBRELLA, BUILDER, EXPLODE }

    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float maxFallSpeedUmbrella = 10f;
    [SerializeField] private float maxSafeFallDistance = 50f;
    [SerializeField] private GameObject deathAnimationPrefabDefault;
    [SerializeField] private GameObject deathAnimationPrefabSplash;
    [SerializeField] private GameObject deathAnimationPrefabExplode;
    [SerializeField] private GameObject deathAnimationPrefabBurn;
    [SerializeField] private GameObject deathAnimationPrefabShock;
    [SerializeField] private GameObject deathAnimationPrefabDrown;
    [SerializeField] private TextMesh countdownText;
    [SerializeField] private Sprite[] walkAnimation;
    [SerializeField] private Sprite[] fallAnimation;
    [SerializeField] private Sprite[] digDownAnimation;
    [SerializeField] private Sprite[] digForwardAnimation;
    [SerializeField] private Sprite[] blockerAnimation;
    [SerializeField] private Sprite[] umbrellaAnimation;
    [SerializeField] private Sprite[] builderAnimation;

    private Skill currentSkill = Skill.NONE;
    private AnimationState currentAnimation = AnimationState.INIT;
    private int dir = 1, skillUses = 0;
    private float fallenDistance = 0;
    private float fallSpeed = 0;
    private TerrainManager terrain;
    private float explodeTimer = 5.0f;
    private BoxCollider2D guyCollider;
    private SpriteRenderer spriteRenderer;
    private AnimatedImage anim;
    private bool umbrellaEquiped, explosionTriggered, skillReady = false, dead, initialized = false;
    private Vector2 size;
    public bool IsDead { get => dead; }

    // Für das Debugging speichern wir das letzte Check-Areal
    private Rect lastCheckArea;
    private Rect lastDigArea;

    void Start()
    {
        guyCollider = GetComponent<BoxCollider2D>();
        size = new Vector2(
            guyCollider.size.x * transform.localScale.x,
            guyCollider.size.y * transform.localScale.y
            );
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimatedImage>();
        anim.Init();
        anim.OnAnimationFinished += SetSkillReady;
        dead = false;
        initialized = true;
    }

    public void SetTerrain(TerrainManager terrainManager)
    {
        terrain = terrainManager;
    }

    public void SetSkillReady()
    {
        skillReady = true;
    }

    public bool GiveSkill(Skill s)
    {
        // Explosion cannot be stopped
        if (explosionTriggered)
            return false;
        else if (s == Skill.EXPLODER)
        {
            explosionTriggered = true;
            return true;
        }

        // Equip Umbrella if not already
        if (s == Skill.UMBRELLA)
        {
            if (umbrellaEquiped) return false;
            umbrellaEquiped = true;
            return true;
        }

        // Other skills
        if (currentSkill != Skill.BLOCKER
            && currentSkill != s)
        {
            currentSkill = s;
            skillUses = Core.DEFAULT_SKILL_USES;
            Debug.Log("Assigned Skill: " + s);
            return true;
        }
        return false;
    }

    private void SetAnimationState(AnimationState state)
    {
        if (currentAnimation != state)
        {
            //Debug.Log("SetAnimationState: " + state);
            currentAnimation = state;
            switch (state)
            {
                case AnimationState.WALKING:
                    anim.sprites = walkAnimation;
                    break;
                case AnimationState.FALLING:
                    anim.sprites = fallAnimation;
                    break;
                case AnimationState.DIGDOWN:
                    anim.sprites = digDownAnimation;
                    break;
                case AnimationState.DIGFORWARD:
                    anim.sprites = digForwardAnimation;
                    break;
                case AnimationState.DIGQUER:
                    anim.sprites = digForwardAnimation;
                    break;
                case AnimationState.UMBRELLA:
                    anim.sprites = umbrellaAnimation;
                    break;
                case AnimationState.BUILDER:
                    anim.sprites = builderAnimation;
                    break;
                case AnimationState.BLOCKER:
                    anim.sprites = blockerAnimation;
                    break;
                case AnimationState.EXPLODE:
                    break;
            }
            anim.Restart();
        }
    }

    public void Act(float dT)
    {
        if (!initialized) return;
        anim.Tick(dT);
        switch (currentSkill)
        {
            case Skill.DIGGER_DOWN:
                SetAnimationState(AnimationState.DIGDOWN);
                if (skillReady)
                {
                    skillReady = false;
                    skillUses--;
                    // Gräbt exakt unter dem Collider in der Breite des Guys
                    Vector2 pos = transform.localPosition;
                    Rect area = new Rect(pos.x - size.x / 2, pos.y - size.y / 2 - Core.DIG_RANGE, size.x, Core.DIG_RANGE + size.y);
                    if (!DigRect(area) || skillUses <= 0)
                        currentSkill = Skill.NONE;
                }
                break;

            case Skill.DIGGER_FORWARD:
                SetAnimationState(AnimationState.DIGFORWARD);
                if (skillReady)
                {
                    skillReady = false;
                    skillUses--;
                    // Gräbt eine Box vor dem Guy in der Höhe des Guys
                    Vector2 pos = transform.localPosition;
                    float width = Core.DIG_RANGE + size.x / 2;
                    float x = dir < 0 ? pos.x - width : pos.x;
                    float y = pos.y - size.y / 2 + Core.MIN_COLLISION_OFFSET;
                    Rect area = new Rect(x, y, width, size.y);
                    if (!DigRect(area) || skillUses <= 0)
                        currentSkill = Skill.NONE;
                    else
                        transform.localPosition += new Vector3(Core.DIG_RANGE * dir, 0, 0);
                }
                break;

            case Skill.DIGGER_QUER:
                SetAnimationState(AnimationState.DIGQUER);
                if (skillReady)
                {
                    skillReady = false;
                    skillUses--;
                    // Gräbt eine Box vor dem Guy in der Höhe des Guys
                    Vector2 pos = transform.localPosition;
                    float width = Core.DIG_RANGE + size.x / 2;
                    float x = dir < 0 ? pos.x - width : pos.x;
                    float y = pos.y - size.y / 2 - Core.DIG_RANGE;
                    Rect area = new Rect(x, y, width, size.y + Core.DIG_RANGE * 2);
                    if (!DigRect(area) || skillUses <= 0)
                        currentSkill = Skill.NONE;
                    else
                        transform.localPosition += new Vector3(Core.DIG_RANGE * dir, 0, 0);
                }
                break;
            case Skill.BUILDER:
                SetAnimationState(AnimationState.BUILDER);
                if (skillReady)
                {
                    skillReady = false;
                    skillUses--;
                    if (!BuildStep() || skillUses <= 0)
                        currentSkill = Skill.NONE;
                }
                break;
            case Skill.BLOCKER:
                SetAnimationState(AnimationState.BLOCKER);
                break;
        }

        if (explosionTriggered)
        {
            explodeTimer -= dT;
            countdownText.text = explodeTimer.ToString("F0");
            //Debug.Log("Countdown=" + explodeTimer);
            if (explodeTimer <= 0) Explode();
        }
        else
        {
            countdownText.text = "";
        }
    }

    public float GetWalkSpeed()
    {
        return 0;
    }

    public void Move(float dT)
    {
        if (!initialized) return;

        // Get current Position
        Vector3 pos = transform.localPosition;
        float dx = 0;
        // Only move when doing nothing
        if (currentSkill == Skill.NONE)
        {
            dx = dir * walkSpeed * dT;
        }
        float nextX = pos.x + dx;
        float yMin = pos.y - (size.y / 2);
        // Stick to ground while walking (climb down)
        if (Mathf.Abs(fallSpeed) < 0.1f)
            yMin -= Core.MAX_CLIMB_HEIGHT;

        float yMax = pos.y + (size.y / 2);
        float newY;
        lastCheckArea = new Rect(nextX, yMin, Core.MIN_COLLISION_OFFSET, yMax - yMin);
        TerrainType highestTerrain = terrain.GetHighestSolidPixelInColumn(nextX, yMin, yMax, out newY);
        if (highestTerrain != TerrainType.Empty)
        {
            // First check if in death zone
            if (TerrainManager.IsTerrainHazard(highestTerrain))
            {
                Die(highestTerrain);
                return;
            }
            // Wir haben Boden gefunden!
            if (fallenDistance > maxSafeFallDistance && !umbrellaEquiped)
            {
                Die(TerrainType.Dirt);
            }
            fallenDistance = 0;
            fallSpeed = 0;

            // Only switch animation when doing nothing
            if (dx != 0)
            {
                SetAnimationState(AnimationState.WALKING);
            }
            newY += (size.y / 2);
            float heightDiff = newY - transform.localPosition.y;

            // Check, ob es eine Wand ist
            if (heightDiff > Core.MAX_CLIMB_HEIGHT)
            {
                if (TerrainManager.IsTerrainPassable(highestTerrain))
                {
                    // Walking through stairs
                    // Climb on next highest ground
                    yMax = pos.y - (size.y / 2) + Core.MAX_CLIMB_HEIGHT;
                    highestTerrain = terrain.GetHighestSolidPixelInColumn(nextX, yMin, yMax, out newY);
                    if (highestTerrain != TerrainType.Empty)
                    {
                        newY += (size.y / 2);
                    }
                    else
                    {
                        // no solid ground, don't climb
                        newY = pos.y;
                    }
                }
                else
                {
                    ToggleDirection();
                    return;
                }
            }
            // Erfolg: Wir bewegen uns auf die neue Position (X und das angepasste Y)
            float dY = Mathf.Abs(pos.y - newY);
            if (dY < Core.MIN_COLLISION_OFFSET // Kleine Unebenheiten ignorieren - precision error
                || dY > Core.MAX_CLIMB_HEIGHT) // Zu große Schritte auch verbieten
                newY = pos.y;
            transform.localPosition = new Vector3(nextX, newY, 0);

        }
        else
        {
            // Loose skill when falling
            currentSkill = Skill.NONE;
            if (umbrellaEquiped)
                SetAnimationState(AnimationState.UMBRELLA);
            else
                SetAnimationState(AnimationState.FALLING);

            // Accelerate
            fallSpeed += Physics2D.gravity.y * dT;
            if (umbrellaEquiped && Mathf.Abs(fallSpeed) > maxFallSpeedUmbrella)
                fallSpeed = fallSpeed < 0 ? -maxFallSpeedUmbrella : maxFallSpeedUmbrella;
            else if (Mathf.Abs(fallSpeed) > maxFallSpeed)
                fallSpeed = fallSpeed < 0 ? -maxFallSpeed : maxFallSpeed;

            // Move
            float step = fallSpeed * dT;
            transform.localPosition += new Vector3(0, step, 0);
            fallenDistance += Mathf.Abs(step);
            // Reached bottom?
            if (terrain.IsOutOfBounds(pos.x, pos.y))
                Die(TerrainType.Empty);
        }
    }

    private void ToggleDirection()
    {
        dir *= -1;
        spriteRenderer.flipX = (dir < 0);
    }

    private bool BuildStep()
    {
        // Nutzt die Konstanten für die Treppen-Dimensionen
        Vector2 stepPos = (Vector2)transform.localPosition + new Vector2(dir * Core.MIN_COLLISION_OFFSET, (size.y / -2));
        int x = Mathf.RoundToInt(stepPos.x);
        int y = Mathf.RoundToInt(stepPos.y);
        bool allfree = true;
        lastDigArea = new Rect(stepPos.x, stepPos.y, Core.STAIR_WIDTH * dir, Core.STAIR_HEIGHT);
        for (int i = 0; i < Core.STAIR_WIDTH; i++)
        {
            for (int j = 0; j < Core.STAIR_HEIGHT; j++)
            {
                allfree &= terrain.BuildStair(x + (i * dir), y + j);
            }
        }
        // Play Sound
        Core.Instance.AM.PlaySound(SoundEffect.BUILD);
        // Try climb step
        Vector2 nextpos = new Vector2(transform.localPosition.x, transform.localPosition.y);
        nextpos.x += dir * (Core.STAIR_WIDTH - Core.MIN_COLLISION_OFFSET);
        nextpos.y += Core.STAIR_HEIGHT - Core.MIN_COLLISION_OFFSET;
        // Check collision
        float highestY;
        TerrainType highestTerrain = terrain.GetHighestSolidPixelInColumn(nextpos.x, nextpos.y - size.y / 2, nextpos.y + size.y / 2, out highestY);
        if (highestTerrain != TerrainType.Empty)
        {
            float d = Mathf.Abs(highestY - nextpos.y + size.y / 2);
            Debug.Log("BuildStep(): Wall height = " + d);
            if (d >= Core.MAX_CLIMB_HEIGHT) return false;
            //            else nextpos.y = highestY + size.y / 2;
        }
        //Debug.Log("Set Position: " + nextpos);
        transform.localPosition = nextpos;
        return true;
    }

    private void Die(TerrainType cause)
    {
        switch (cause)
        {
            case TerrainType.Empty:
                Debug.Log("Tod durch endlos Fallen");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.ENDLESS_FALL_DEATH);
                // Spawn VFX
                if (deathAnimationPrefabSplash)
                    Destroy(Instantiate(deathAnimationPrefabSplash, transform.position, Quaternion.identity), 2);
                break;
            case TerrainType.Dirt:
                Debug.Log("Tod durch Fallschaden");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.FALL_DEATH);
                // Spawn VFX
                if (deathAnimationPrefabSplash)
                    Destroy(Instantiate(deathAnimationPrefabSplash, transform.position, Quaternion.identity), 2);
                break;
            case TerrainType.Steel:
                Debug.Log("Tod durch Explosion");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.EXPLODE);
                // Spawn VFX
                if (deathAnimationPrefabExplode)
                    Destroy(Instantiate(deathAnimationPrefabExplode, transform.position, Quaternion.identity), 2);
                break;
            case TerrainType.Fire:
                Debug.Log("Tod durch Verbrennen");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.BURN);
                // Spawn VFX
                if (deathAnimationPrefabBurn)
                    Destroy(Instantiate(deathAnimationPrefabBurn, transform.position, Quaternion.identity), 2);
                break;
            case TerrainType.Water:
                Debug.Log("Tod durch Ertrinken");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.DROWN);
                // Spawn VFX
                if (deathAnimationPrefabDrown)
                    Destroy(Instantiate(deathAnimationPrefabDrown, transform.position, Quaternion.identity), 2);
                break;
            case TerrainType.Bolt:
                Debug.Log("Tod durch Stromschlag");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.SHOCK);
                // Spawn VFX
                if (deathAnimationPrefabShock)
                    Destroy(Instantiate(deathAnimationPrefabShock, transform.position, Quaternion.identity), 2);
                break;
            case TerrainType.Goal:
                Debug.Log("Gerettet!"); Core.Instance.AddSavedCount();
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.SAVED);
                // Spawn VFX
                if (deathAnimationPrefabDefault)
                    Destroy(Instantiate(deathAnimationPrefabDefault, transform.position, Quaternion.identity), 2);
                break;
            default:
                Debug.Log("Tod durch Unbekannt");
                // Play Sound
                Core.Instance.AM.PlaySound(SoundEffect.FALL_DEATH);
                // Spawn VFX
                if (deathAnimationPrefabDefault)
                    Destroy(Instantiate(deathAnimationPrefabDefault, transform.position, Quaternion.identity), 2);
                break;
        }
        dead = true;
        Destroy(gameObject);
    }

    private void Explode()
    {
        int centerX = Mathf.RoundToInt(transform.localPosition.x);
        int centerY = Mathf.RoundToInt(transform.localPosition.y);
        int radius = Mathf.RoundToInt(size.y * Core.EXPLOSION_RADIUS);
        Debug.Log("Explode Radius=" + radius);
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                    terrain.DestroyTerrain(centerX + i, centerY + j);
            }
        }
        Die(TerrainType.Steel);
    }

    private bool DigRect(Rect area)
    {
        // Umrechnung in Pixel-Koordinaten
        int startX = Mathf.RoundToInt(area.x);
        int startY = Mathf.RoundToInt(area.y);
        int width = Mathf.RoundToInt(area.width);
        int height = Mathf.RoundToInt(area.height);
        lastDigArea = area;
        bool candig = true;
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                TerrainType t = terrain.GetTypeAt(x, y);
                if (t != TerrainType.Empty && !terrain.DestroyTerrain(x, y, dir))
                    candig = false;
            }
        }
        // Play Sound
        Core.Instance.AM.PlaySound(SoundEffect.DIG);
        return candig;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Finish"))
        {
            Die(TerrainType.Goal);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            GuyBehavior other = collision.GetComponent<GuyBehavior>();
            if (other != null && other.currentSkill == Skill.BLOCKER)
            {
                // Hit blocker, turn around
                ToggleDirection();
                // get a little bit distance
                transform.localPosition += new Vector3(Core.MIN_COLLISION_OFFSET * dir, 0, 0);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            // Zeichne das Areal, das gerade auf Kollision geprüft wurde
            Gizmos.DrawWireCube(new Vector3(lastCheckArea.center.x, lastCheckArea.center.y + 500, 0),
                                new Vector3(lastCheckArea.width, lastCheckArea.height, 0.1f));
            if (lastDigArea != null)
            {
                Gizmos.color = Color.blue;
                // Zeichne das Areal, das zuletzt gebuddelt wurde
                Gizmos.DrawWireCube(new Vector3(lastDigArea.center.x, lastDigArea.center.y + 500, 0),
                                    new Vector3(lastDigArea.width, lastDigArea.height, 0.1f));
            }
        }
    }
}