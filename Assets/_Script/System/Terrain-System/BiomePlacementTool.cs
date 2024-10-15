using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

[Serializable]
public class BiomeResourceContainer
{
    public float percent;
    public BiomeResource resource;
}

public class BiomePlacementTool : MonoBehaviour
{
    [ScriptableCreator]
    public BiomeResource focusBiomeResource;
    public List<BiomeResourceContainer> biomeResources = new();

    public Vector3 size;
    public float fadeDistance = 1.0f;

    // ������ ����
    public enum NoiseType { Perlin, Simplex }
    public NoiseType selectedNoiseType = NoiseType.Perlin;
    public int noiseSeed = 0; // ������ �õ� ����
    public float noiseScale = 1.0f; // ������ ������ ����

    [TextureBox]
    public Texture2D noiseTexture; // ������ �ؽ�ó

    [SerializeField]
    public List<Vector3> addedTreesPosition = new List<Vector3>();


    private void OnValidate()
    {
        GenerateNoiseTexture();
    }

    private void GenerateNoiseTexture()
    {
        int textureSize = 256;
        noiseTexture = new Texture2D(textureSize, textureSize);

        System.Random random = new System.Random(noiseSeed);
        float offsetX = random.Next(0, 10000);
        float offsetY = random.Next(0, 10000);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float xCoord = (float)x / textureSize * noiseScale + offsetX;
                float yCoord = (float)y / textureSize * noiseScale + offsetY;

                float sample = 0f;
                if (selectedNoiseType == NoiseType.Perlin)
                {
                    sample = Mathf.PerlinNoise(xCoord, yCoord);
                }
                else if (selectedNoiseType == NoiseType.Simplex)
                {
                    // Simplex ������ ���� �ʿ� (�ܺ� ���̺귯�� ��� ����)
                    sample = Mathf.PerlinNoise(xCoord, yCoord); // �ӽ� ��ü
                }

                noiseTexture.SetPixel(x, y, new Color(sample, sample, sample));
            }
        }

        noiseTexture.Apply();
    }


#if UNITY_EDITOR
    [Button("Place")]
    public void _Editor_Place()
    {
        _Editor_Remove();

        Place(focusBiomeResource, 1);

        biomeResources.ForEach(e =>
        {
            Place(e.resource, e.percent);
        });
    }

    private void Place(BiomeResource resource, float percent)
    {
        if (resource == null || resource.prefabs.Count == 0)
        {
            Debug.LogWarning("��Ŀ���� BiomeResource�� ���ų� prefab ����Ʈ�� ��� �ֽ��ϴ�.");
            return;
        }

        // ��Ŀ���� �Ĺ��� ��ġ�� ������
        Vector2 plantSize = resource.plantSize;

        // ���μ��� �� �� �� �κ��� �������� ������ �ؼ�
        float longerAxis = Mathf.Max(size.x, size.z);
        float scaleRatio = longerAxis / noiseTexture.width;

        // ���� Ȱ��ȭ�� Terrain ��������
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("Ȱ��ȭ�� Terrain�� �����ϴ�.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        // ���� ���� �ν��Ͻ��� ��������
        List<TreeInstance> newTreeInstances = new List<TreeInstance>(terrainData.treeInstances);

        // ����� �̿��� �Ļ� ��ġ
        for (float z = 0; z < size.z; z += plantSize.y)
        {
            for (float x = 0; x < size.x; x += plantSize.x)
            {
                // ������ �� ��������
                float noiseValue = noiseTexture.GetPixel(
                    Mathf.RoundToInt(x / scaleRatio),
                    Mathf.RoundToInt(z / scaleRatio)
                ).grayscale;

                // ������ ���� ���� ���� �̻��� �� �Ļ� ��ġ
                if (noiseValue > 0.35f && Random.Range(0, 1) < percent)
                {
                    // ���� ������ ��ġ�� ��ġ ����
                    Vector3 position = new Vector3(
                        x + Random.Range(-plantSize.x / 2, plantSize.x / 2),
                        0,
                        z + Random.Range(-plantSize.y / 2, plantSize.y / 2)
                    );

                    var worldPosition = transform.position + transform.rotation * position - (transform.rotation * (size * 0.5f));
                    worldPosition.y = 1000;

                    // ����ĳ��Ʈ�� ���� �Ǵ� �׶��� ���̾ �浹�ϴ��� Ȯ��
                    RaycastHit hit;
                    if (Physics.Raycast(worldPosition, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) ||
                        Physics.Raycast(worldPosition, Vector3.down, out hit, Mathf.Infinity, terrain.gameObject.layer))
                    {
                        // �ͷ��� ��ǥ�� ��ȯ
                        Vector3 localPos = hit.point - terrain.transform.position;
                        float normalizedX = localPos.x / terrainData.size.x;
                        float normalizedZ = localPos.z / terrainData.size.z;
                        float normalizedY = (hit.point.y - terrain.transform.position.y) / terrainData.size.y;

                        // ���� ������ ��������
                        var targetPrefab = resource.prefabs.GetRandomElement();
                        // ������ �ͷ��ο� ��ϵǾ� �ִ��� Ȯ��
                        int prototypeIndex = GetOrRegisterTreePrototype(terrainData, targetPrefab.prefab);
                        var targetTreePosition = new Vector3(normalizedX, normalizedY, normalizedZ);

                        // ���� �ν��Ͻ� ����
                        TreeInstance treeInstance = new TreeInstance
                        {
                            position = targetTreePosition,
                            widthScale = Random.Range(resource.minScale.x, resource.maxScale.x),
                            heightScale = Random.Range(resource.minScale.y, resource.maxScale.y),
                            rotation = Random.Range(0, 360),
                            color = Color.white,                // �⺻ ����
                            lightmapColor = Color.white,        // �⺻ ����Ʈ�� ����
                            prototypeIndex = prototypeIndex     // �ͷ��ο� �̸� ��ϵ� ���� ������Ÿ�� �ε���
                        };

                        // ���� ����Ʈ�� �߰�
                        newTreeInstances.Add(treeInstance);
                        addedTreesPosition.Add(targetTreePosition);
                    }
                }
            }
        }

        // ���� �ν��Ͻ� ����Ʈ�� �ٽ� �ͷ��� �����Ϳ� �Ҵ�
        terrainData.treeInstances = newTreeInstances.ToArray();
        terrainData.RefreshPrototypes();
    }

    private int GetOrRegisterTreePrototype(TerrainData terrainData, GameObject treePrefab)
    {
        TreePrototype[] prototypes = terrainData.treePrototypes;

        // ������ �̸��� �������� ������ �̹� ��ϵǾ� �ִ��� Ȯ��
        for (int i = 0; i < prototypes.Length; i++)
        {
            if (prototypes[i].prefab.name == treePrefab.name)
            {
                return i; // �̹� ��ϵ� ��� �ش� �ε����� ��ȯ
            }
        }

        // ������ ��ϵ��� ���� ��� ���Ӱ� ���
        TreePrototype newPrototype = new TreePrototype
        {
            prefab = treePrefab
        };

        List<TreePrototype> prototypeList = new List<TreePrototype>(prototypes)
        {
            newPrototype
        };

        // ���� ������Ÿ�� ����Ʈ�� �ٽ� ����
        terrainData.treePrototypes = prototypeList.ToArray();
        // ���� �߰��� ������Ÿ�� �ε����� ��ȯ
        return prototypeList.Count - 1;
    }

    [Button("Remove")]
    public void _Editor_Remove()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            return;
        }

        if (addedTreesPosition.Count <= 0)
            return;

        TerrainData terrainData = terrain.terrainData;

        // ���� �ͷ��ο� �ִ� ���� �ν��Ͻ��� ����Ʈ�� ������
        List<TreeInstance> currentTrees = new List<TreeInstance>(terrainData.treeInstances);

        for (int i = 0; i < currentTrees.Count; ++i)
        {
            if (currentTrees[i].position.Eqaul_XZ(addedTreesPosition[0]))
            {
                currentTrees.RemoveRange(i, addedTreesPosition.Count);
                addedTreesPosition.Clear();
                break;
            }
        }

        // ���� �ν��Ͻ� �迭�� ������Ʈ (���� ���ŵ� ����)
        terrainData.treeInstances = currentTrees.ToArray();
    }
#endif

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 0, 1, 0.35f);

        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.color = new Color(1, 1, 1, 1f);
        Gizmos.DrawWireCube(Vector3.zero, size + Vector3.one * fadeDistance);
    }
#endif
}