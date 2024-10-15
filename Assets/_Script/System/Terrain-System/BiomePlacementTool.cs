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

    // 노이즈 설정
    public enum NoiseType { Perlin, Simplex }
    public NoiseType selectedNoiseType = NoiseType.Perlin;
    public int noiseSeed = 0; // 노이즈 시드 설정
    public float noiseScale = 1.0f; // 노이즈 스케일 설정

    [TextureBox]
    public Texture2D noiseTexture; // 노이즈 텍스처

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
                    // Simplex 노이즈 구현 필요 (외부 라이브러리 사용 가능)
                    sample = Mathf.PerlinNoise(xCoord, yCoord); // 임시 대체
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
            Debug.LogWarning("포커스된 BiomeResource가 없거나 prefab 리스트가 비어 있습니다.");
            return;
        }

        // 포커스된 식물의 덩치를 가져옴
        Vector2 plantSize = resource.plantSize;

        // 가로세로 중 더 긴 부분을 기준으로 노이즈 해석
        float longerAxis = Mathf.Max(size.x, size.z);
        float scaleRatio = longerAxis / noiseTexture.width;

        // 현재 활성화된 Terrain 가져오기
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("활성화된 Terrain이 없습니다.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        // 기존 나무 인스턴스를 가져오기
        List<TreeInstance> newTreeInstances = new List<TreeInstance>(terrainData.treeInstances);

        // 노이즈를 이용한 식생 배치
        for (float z = 0; z < size.z; z += plantSize.y)
        {
            for (float x = 0; x < size.x; x += plantSize.x)
            {
                // 노이즈 값 가져오기
                float noiseValue = noiseTexture.GetPixel(
                    Mathf.RoundToInt(x / scaleRatio),
                    Mathf.RoundToInt(z / scaleRatio)
                ).grayscale;

                // 노이즈 값이 일정 수준 이상일 때 식생 배치
                if (noiseValue > 0.35f && Random.Range(0, 1) < percent)
                {
                    // 랜덤 값으로 배치할 위치 설정
                    Vector3 position = new Vector3(
                        x + Random.Range(-plantSize.x / 2, plantSize.x / 2),
                        0,
                        z + Random.Range(-plantSize.y / 2, plantSize.y / 2)
                    );

                    var worldPosition = transform.position + transform.rotation * position - (transform.rotation * (size * 0.5f));
                    worldPosition.y = 1000;

                    // 레이캐스트로 지형 또는 그라운드 레이어에 충돌하는지 확인
                    RaycastHit hit;
                    if (Physics.Raycast(worldPosition, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) ||
                        Physics.Raycast(worldPosition, Vector3.down, out hit, Mathf.Infinity, terrain.gameObject.layer))
                    {
                        // 터레인 좌표로 변환
                        Vector3 localPos = hit.point - terrain.transform.position;
                        float normalizedX = localPos.x / terrainData.size.x;
                        float normalizedZ = localPos.z / terrainData.size.z;
                        float normalizedY = (hit.point.y - terrain.transform.position.y) / terrainData.size.y;

                        // 나무 프리팹 가져오기
                        var targetPrefab = resource.prefabs.GetRandomElement();
                        // 나무가 터레인에 등록되어 있는지 확인
                        int prototypeIndex = GetOrRegisterTreePrototype(terrainData, targetPrefab.prefab);
                        var targetTreePosition = new Vector3(normalizedX, normalizedY, normalizedZ);

                        // 나무 인스턴스 생성
                        TreeInstance treeInstance = new TreeInstance
                        {
                            position = targetTreePosition,
                            widthScale = Random.Range(resource.minScale.x, resource.maxScale.x),
                            heightScale = Random.Range(resource.minScale.y, resource.maxScale.y),
                            rotation = Random.Range(0, 360),
                            color = Color.white,                // 기본 색상
                            lightmapColor = Color.white,        // 기본 라이트맵 색상
                            prototypeIndex = prototypeIndex     // 터레인에 미리 등록된 나무 프로토타입 인덱스
                        };

                        // 나무 리스트에 추가
                        newTreeInstances.Add(treeInstance);
                        addedTreesPosition.Add(targetTreePosition);
                    }
                }
            }
        }

        // 나무 인스턴스 리스트를 다시 터레인 데이터에 할당
        terrainData.treeInstances = newTreeInstances.ToArray();
        terrainData.RefreshPrototypes();
    }

    private int GetOrRegisterTreePrototype(TerrainData terrainData, GameObject treePrefab)
    {
        TreePrototype[] prototypes = terrainData.treePrototypes;

        // 프리팹 이름을 기준으로 나무가 이미 등록되어 있는지 확인
        for (int i = 0; i < prototypes.Length; i++)
        {
            if (prototypes[i].prefab.name == treePrefab.name)
            {
                return i; // 이미 등록된 경우 해당 인덱스를 반환
            }
        }

        // 나무가 등록되지 않은 경우 새롭게 등록
        TreePrototype newPrototype = new TreePrototype
        {
            prefab = treePrefab
        };

        List<TreePrototype> prototypeList = new List<TreePrototype>(prototypes)
        {
            newPrototype
        };

        // 나무 프로토타입 리스트를 다시 설정
        terrainData.treePrototypes = prototypeList.ToArray();
        // 새로 추가된 프로토타입 인덱스를 반환
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

        // 현재 터레인에 있는 나무 인스턴스를 리스트로 가져옴
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

        // 나무 인스턴스 배열을 업데이트 (나무 제거된 상태)
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