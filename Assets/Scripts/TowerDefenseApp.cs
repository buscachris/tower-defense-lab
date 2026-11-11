using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefenseLab
{
    public enum TowerKind
    {
        Barricade,
        Arrow,
        Cannon,
        HotPlate
    }

    public sealed class TowerDefenseApp : MonoBehaviour
    {
        private const int Width = 12;
        private const int Height = 20;
        private const float CellSize = 2f;

        private readonly List<TowerNode> towers = new List<TowerNode>();
        private readonly List<EnemyUnit> enemies = new List<EnemyUnit>();
        private readonly Vector2Int spawnCell = new Vector2Int(Width / 2, Height - 1);
        private readonly Vector2Int goalCell = new Vector2Int(Width / 2, 0);

        private Material floorMaterial;
        private Material borderMaterial;
        private Material arrowMaterial;
        private Material cannonMaterial;
        private Material hotPlateMaterial;
        private Material barricadeMaterial;
        private Material enemyMaterial;
        private Material goalMaterial;
        private Camera mainCamera;
        private Text hudText;
        private Text statusText;
        private Text selectionText;
        private TowerKind selectedKind = TowerKind.Arrow;
        private bool buildMode = true;
        private bool gameOver;
        private int round = 1;
        private int gold = 180;
        private int lives = 10;
        private int toSpawn;
        private float spawnTimer;
        private float waveDelay;

        private int Cost(TowerKind kind)
        {
            switch (kind)
            {
                case TowerKind.Barricade: return 20;
                case TowerKind.Arrow: return 50;
                case TowerKind.Cannon: return 75;
                case TowerKind.HotPlate: return 60;
                default: return 0;
            }
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            BuildMaterials();
            BuildWorld();
            BuildCamera();
            BuildUi();
            UpdateHud("Build mode: place towers, then press START WAVE.");
        }

        private void Update()
        {
            if (gameOver) return;
            if (buildMode)
            {
                ReadBuildInput();
                if (Input.GetKeyDown(KeyCode.Space)) StartWave();
            }
            else
            {
                UpdateWave(Time.deltaTime);
            }

            UpdateEnemies(Time.deltaTime);
            UpdateTowers(Time.deltaTime);
        }

        private void BuildMaterials()
        {
            floorMaterial = Material(new Color(0.07f, 0.10f, 0.16f));
            borderMaterial = Material(new Color(0.18f, 0.24f, 0.34f));
            arrowMaterial = Material(new Color(0.22f, 0.70f, 1f));
            cannonMaterial = Material(new Color(1f, 0.40f, 0.18f));
            hotPlateMaterial = Material(new Color(1f, 0.82f, 0.18f));
            barricadeMaterial = Material(new Color(0.46f, 0.52f, 0.62f));
            enemyMaterial = Material(new Color(0.95f, 0.16f, 0.24f));
            goalMaterial = Material(new Color(0.25f, 1f, 0.55f));
        }

        private Material Material(Color color)
        {
            Material material = new Material(Shader.Find("Standard")) { color = color };
            material.SetFloat("_Metallic", 0.15f);
            material.SetFloat("_Glossiness", 0.35f);
            return material;
        }

        private void BuildWorld()
        {
            Cube("Playing Field", new Vector3(0f, -0.15f, Height * CellSize * 0.5f), new Vector3(Width * CellSize, 0.25f, Height * CellSize), floorMaterial);
            float width = Width * CellSize;
            float height = Height * CellSize;
            Cube("Left Border", new Vector3(-width * 0.5f - 0.25f, 0.6f, height * 0.5f), new Vector3(0.5f, 1.5f, height + 1f), borderMaterial);
            Cube("Right Border", new Vector3(width * 0.5f + 0.25f, 0.6f, height * 0.5f), new Vector3(0.5f, 1.5f, height + 1f), borderMaterial);
            Cube("Spawn Marker", CellCenter(spawnCell) + Vector3.up * 0.05f, new Vector3(CellSize * 0.8f, 0.12f, CellSize * 0.8f), enemyMaterial);
            Cube("Leak Marker", CellCenter(goalCell) + Vector3.up * 0.05f, new Vector3(CellSize * 0.8f, 0.12f, CellSize * 0.8f), goalMaterial);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        Cube("Grid Tile", CellCenter(new Vector2Int(x, y)) + Vector3.down * 0.01f, new Vector3(CellSize - 0.04f, 0.03f, CellSize - 0.04f), Material(new Color(0.09f, 0.13f, 0.20f)));
                    }
                }
            }
        }

        private void BuildCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            mainCamera.transform.position = new Vector3(0f, 30f, Height * CellSize * 0.5f);
            mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 24f;
            mainCamera.backgroundColor = new Color(0.015f, 0.02f, 0.04f);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("Tower Defense UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            hudText = Label(canvasObject.transform, "HUD", new Vector2(0f, -18f), new Vector2(1f, 42f), 22, TextAnchor.MiddleLeft);
            statusText = Label(canvasObject.transform, "Status", new Vector2(0f, 26f), new Vector2(1f, 38f), 18, TextAnchor.MiddleCenter);
            selectionText = Label(canvasObject.transform, "Selection", new Vector2(0f, 68f), new Vector2(1f, 32f), 17, TextAnchor.MiddleCenter);

            CreateButton(canvasObject.transform, "BARRICADE  $20", new Vector2(20f, 20f), TowerKind.Barricade);
            CreateButton(canvasObject.transform, "ARROW  $50", new Vector2(180f, 20f), TowerKind.Arrow);
            CreateButton(canvasObject.transform, "CANNON  $75", new Vector2(320f, 20f), TowerKind.Cannon);
            CreateButton(canvasObject.transform, "HOT PLATE  $60", new Vector2(460f, 20f), TowerKind.HotPlate);
            CreateStartButton(canvasObject.transform);
            Label(canvasObject.transform, "Instructions", new Vector2(0f, 104f), new Vector2(1f, 34f), 15, TextAnchor.MiddleCenter).text = "Click a grid tile to place | B / A / C / H selects towers | SPACE starts a wave";
            UpdateSelectionText();
        }

        private Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject objectRoot = new GameObject(name);
            objectRoot.transform.SetParent(parent, false);
            RectTransform rect = objectRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text label = objectRoot.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = alignment;
            return label;
        }

        private void CreateButton(Transform parent, string caption, Vector2 position, TowerKind kind)
        {
            Button button = Button(parent, caption, position, new Vector2(130f, 42f));
            button.onClick.AddListener(() => SelectTower(kind));
        }

        private void CreateStartButton(Transform parent)
        {
            Button button = Button(parent, "START WAVE", new Vector2(-125f, 20f), new Vector2(220f, 42f));
            button.onClick.AddListener(StartWave);
        }

        private Button Button(Transform parent, string caption, Vector2 position, Vector2 size)
        {
            GameObject objectRoot = new GameObject(caption);
            objectRoot.transform.SetParent(parent, false);
            RectTransform rect = objectRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = objectRoot.AddComponent<Image>();
            image.color = new Color(0.08f, 0.15f, 0.25f, 0.95f);
            Button button = objectRoot.AddComponent<Button>();
            button.targetGraphic = image;

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(objectRoot.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = caption;
            return button;
        }

        private void ReadBuildInput()
        {
            if (Input.GetKeyDown(KeyCode.B)) SelectTower(TowerKind.Barricade);
            if (Input.GetKeyDown(KeyCode.A)) SelectTower(TowerKind.Arrow);
            if (Input.GetKeyDown(KeyCode.C)) SelectTower(TowerKind.Cannon);
            if (Input.GetKeyDown(KeyCode.H)) SelectTower(TowerKind.HotPlate);
            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(ray, out float distance)) return;
            Vector2Int cell = CellFromWorld(ray.GetPoint(distance));
            PlaceTower(cell);
        }

        private void SelectTower(TowerKind kind)
        {
            selectedKind = kind;
            UpdateSelectionText();
        }

        private void UpdateSelectionText()
        {
            if (selectionText != null) selectionText.text = "Selected: " + selectedKind.ToString().ToUpperInvariant() + " ($" + Cost(selectedKind) + ")";
        }

        private void PlaceTower(Vector2Int cell)
        {
            if (!buildMode || !Inside(cell) || cell == spawnCell || cell == goalCell) return;
            if (HasTower(cell) || gold < Cost(selectedKind)) return;
            if (FindPath(spawnCell, goalCell, true, cell).Count == 0)
            {
                UpdateHud("That placement would block the route. Choose another tile.");
                return;
            }

            gold -= Cost(selectedKind);
            towers.Add(new TowerNode(selectedKind, cell, this));
            UpdateHud(selectedKind + " tower placed. Keep the route open.");
        }

        private void StartWave()
        {
            if (!buildMode || gameOver) return;
            buildMode = false;
            toSpawn = 4 + round * 2;
            spawnTimer = 0f;
            waveDelay = 0f;
            UpdateHud("Wave " + round + " incoming. Defend the leak point.");
        }

        private void UpdateWave(float deltaTime)
        {
            if (toSpawn > 0)
            {
                spawnTimer -= deltaTime;
                if (spawnTimer <= 0f)
                {
                    SpawnEnemy();
                    toSpawn--;
                    spawnTimer = 0.8f;
                }
            }
            else if (enemies.Count == 0)
            {
                waveDelay += deltaTime;
                if (waveDelay > 0.4f)
                {
                    buildMode = true;
                    gold += 30 + round * 5;
                    round++;
                    UpdateHud("Wave cleared. Build phase restored with a round bonus.");
                }
            }
        }

        private void SpawnEnemy()
        {
            enemies.Add(new EnemyUnit(this, spawnCell, enemyMaterial));
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (int index = enemies.Count - 1; index >= 0; index--)
            {
                EnemyUnit enemy = enemies[index];
                if (enemy.Dead)
                {
                    enemies.RemoveAt(index);
                    continue;
                }

                if (enemy.Advance(deltaTime))
                {
                    lives--;
                    Destroy(enemy.Visual);
                    enemies.RemoveAt(index);
                    UpdateHud("An enemy leaked. Protect the green goal tile.");
                    if (lives <= 0)
                    {
                        gameOver = true;
                        buildMode = false;
                        UpdateHud("GAME OVER. Press Play again to restart the simulation.");
                    }
                }
            }
        }

        private void UpdateTowers(float deltaTime)
        {
            foreach (TowerNode tower in towers) tower.Tick(deltaTime);
        }

        public EnemyUnit FindTarget(Vector3 origin, float range)
        {
            EnemyUnit best = null;
            float bestDistance = range * range;
            foreach (EnemyUnit enemy in enemies)
            {
                if (enemy.Dead) continue;
                float distance = (enemy.Position - origin).sqrMagnitude;
                if (distance < bestDistance)
                {
                    best = enemy;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public void DamageArea(Vector3 center, float radius, int damage)
        {
            foreach (EnemyUnit enemy in enemies)
            {
                if (!enemy.Dead && (enemy.Position - center).sqrMagnitude <= radius * radius) enemy.TakeDamage(damage);
            }
        }

        public void Reward(int amount)
        {
            gold += amount;
            UpdateHud("Enemy destroyed. Earned $" + amount + ".");
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, bool useExtraBlock = false, Vector2Int extraBlock = default(Vector2Int))
        {
            Queue<Vector2Int> open = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> previous = new Dictionary<Vector2Int, Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            open.Enqueue(start);
            visited.Add(start);
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            while (open.Count > 0)
            {
                Vector2Int current = open.Dequeue();
                if (current == goal) break;
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = current + direction;
                    if (!Inside(next) || visited.Contains(next) || IsBlocked(next, useExtraBlock, extraBlock)) continue;
                    visited.Add(next);
                    previous[next] = current;
                    open.Enqueue(next);
                }
            }

            if (!visited.Contains(goal)) return new List<Vector2Int>();
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int cursor = goal;
            path.Add(cursor);
            while (cursor != start)
            {
                cursor = previous[cursor];
                path.Add(cursor);
            }
            path.Reverse();
            return path;
        }

        private bool IsBlocked(Vector2Int cell, bool useExtraBlock, Vector2Int extraBlock)
        {
            if (useExtraBlock && cell == extraBlock) return true;
            return HasTower(cell);
        }

        private bool HasTower(Vector2Int cell)
        {
            foreach (TowerNode tower in towers) if (tower.Cell == cell) return true;
            return false;
        }

        private bool Inside(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
        }

        public Vector3 CellCenter(Vector2Int cell)
        {
            return new Vector3((cell.x - Width * 0.5f + 0.5f) * CellSize, 0.7f, (cell.y + 0.5f) * CellSize);
        }

        private Vector2Int CellFromWorld(Vector3 world)
        {
            return new Vector2Int(Mathf.FloorToInt(world.x / CellSize + Width * 0.5f), Mathf.FloorToInt(world.z / CellSize));
        }

        private GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private void UpdateHud(string message)
        {
            if (statusText != null) statusText.text = message;
            if (hudText != null) hudText.text = "TOWER DEFENSE LAB    |    ROUND " + round + "    |    " + (buildMode ? "BUILD" : "PLAY") + "    |    GOLD $" + gold + "    |    LIVES " + lives;
            UpdateSelectionText();
        }
    }

    public sealed class TowerNode
    {
        public readonly TowerKind Kind;
        public readonly Vector2Int Cell;
        private readonly TowerDefenseApp app;
        private readonly GameObject visual;
        private float cooldown;

        public TowerNode(TowerKind kind, Vector2Int cell, TowerDefenseApp owner)
        {
            Kind = kind;
            Cell = cell;
            app = owner;
            visual = GameObject.CreatePrimitive(kind == TowerKind.HotPlate ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            visual.name = kind + " Tower";
            visual.transform.position = owner.CellCenter(cell);
            visual.transform.localScale = kind == TowerKind.HotPlate ? new Vector3(1.7f, 0.12f, 1.7f) : new Vector3(1.2f, 1.1f, 1.2f);
            visual.GetComponent<Renderer>().sharedMaterial = MaterialFor(kind);
        }

        public void Tick(float deltaTime)
        {
            if (Kind == TowerKind.Barricade) return;
            cooldown -= deltaTime;
            if (cooldown > 0f) return;
            Vector3 position = visual.transform.position + Vector3.up * 0.6f;

            if (Kind == TowerKind.HotPlate)
            {
                app.DamageArea(visual.transform.position, 1.2f, 5);
                cooldown = 0.35f;
                return;
            }

            EnemyUnit target = app.FindTarget(position, Kind == TowerKind.Cannon ? 7f : 6f);
            if (target == null) return;
            if (Kind == TowerKind.Arrow)
            {
                target.TakeDamage(16);
                Flash(target.Position, new Color(0.2f, 0.8f, 1f));
                cooldown = 0.45f;
            }
            else
            {
                app.DamageArea(target.Position, 2.2f, 26);
                Flash(target.Position, new Color(1f, 0.35f, 0.1f));
                cooldown = 1.25f;
            }
        }

        private void Flash(Vector3 target, Color color)
        {
            GameObject tracer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tracer.name = "Projectile Trace";
            tracer.transform.position = target;
            tracer.transform.localScale = Vector3.one * 0.18f;
            tracer.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
            Object.Destroy(tracer, 0.08f);
        }

        private Material MaterialFor(TowerKind kind)
        {
            Color color = kind == TowerKind.Arrow ? new Color(0.22f, 0.70f, 1f) : kind == TowerKind.Cannon ? new Color(1f, 0.40f, 0.18f) : kind == TowerKind.HotPlate ? new Color(1f, 0.82f, 0.18f) : new Color(0.46f, 0.52f, 0.62f);
            return new Material(Shader.Find("Standard")) { color = color };
        }
    }

    public sealed class EnemyUnit
    {
        private readonly TowerDefenseApp app;
        private readonly List<Vector2Int> path;
        private int pathIndex;
        private float health = 70f;
        private readonly float speed;

        public readonly GameObject Visual;
        public bool Dead { get; private set; }
        public Vector2Int Cell { get; private set; }
        public Vector3 Position => Visual == null ? Vector3.zero : Visual.transform.position;

        public EnemyUnit(TowerDefenseApp owner, Vector2Int start, Material material)
        {
            app = owner;
            Cell = start;
            speed = Random.Range(2.0f, 2.8f);
            path = owner.FindPath(start, new Vector2Int(6, 0));
            Visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Visual.name = "Ground Enemy";
            Visual.transform.position = owner.CellCenter(start);
            Visual.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            Visual.GetComponent<Renderer>().sharedMaterial = material;
        }

        public bool Advance(float deltaTime)
        {
            if (Dead || path.Count == 0) return false;
            if (pathIndex >= path.Count - 1) return true;
            Vector3 target = app.CellCenter(path[pathIndex + 1]);
            Visual.transform.position = Vector3.MoveTowards(Visual.transform.position, target, speed * deltaTime);
            if (Vector3.Distance(Visual.transform.position, target) < 0.02f)
            {
                pathIndex++;
                Cell = path[pathIndex];
                if (pathIndex >= path.Count - 1) return true;
            }
            return false;
        }

        public void TakeDamage(int damage)
        {
            if (Dead) return;
            health -= damage;
            if (health > 0f) return;
            Dead = true;
            app.Reward(15);
            Object.Destroy(Visual);
        }
    }
}
