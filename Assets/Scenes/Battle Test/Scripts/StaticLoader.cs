using Assets.PixelHeroes.Scripts.CollectionScripts;
using System.Collections.Generic;
using UnityEngine;
using data;

namespace battle
{
    public class StaticLoader : StaticComponentGetter<StaticLoader>
    {
        [SerializeField] private SpriteCollection collection;
        public SpriteCollection GetCollection() { return collection; }

        [Header("Prefap")]
        [SerializeField] private GameObject pixelHumanoidPrefap;
        public GameObject GetPixelCharacterPrefap() {  return pixelHumanoidPrefap; }

        [SerializeField] private GameObject defaultArrowPrefap;
        public GameObject GetDefaultArrowPrefab() { return defaultArrowPrefap; }

        [SerializeField] private GameObject pixelCharacterHeadBarPrefap;
        public GameObject GetPixelCharacterHeadBarPrefap() { return pixelCharacterHeadBarPrefap; }

        [SerializeField] private GameObject floatingTextPrefap;
        public GameObject GetFlatingTextPrefap() { return floatingTextPrefap; }

        [SerializeField] private GameObject lightningPillar;
        public GameObject GetLightningPillar() {  return lightningPillar; }

        [SerializeField] private GameObject fireOrbit;
        public GameObject GetFireOrbit() { return fireOrbit; }

        [Header("Humanoid")]
        [SerializeField] private List<PixelHumanoidData> pixelHumanoidDatas = new List<PixelHumanoidData>();
        public int GetPixelHumanoidCount() { return pixelHumanoidDatas.Count; }
        public List<PixelHumanoidData> GetPixelHumanoidDatas() { return pixelHumanoidDatas;  }
        public PixelHumanoidData GetPixelHumanoidData(int index) 
        {
            if (index >= pixelHumanoidDatas.Count)
            {
                Debug.LogError("invalid index: over the length");
                return null;
            }
            else if (index < 0)
            {
                Debug.LogError("invalid index: minus value");
                return null;
            }
                
            return pixelHumanoidDatas[index]; 
        }
    }
}
