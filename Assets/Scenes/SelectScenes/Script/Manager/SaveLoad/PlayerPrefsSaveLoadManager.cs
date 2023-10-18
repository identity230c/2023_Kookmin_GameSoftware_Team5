using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace deck
{ 
    public class PlayerPrefsSaveLoadManager : SaveLoadManager
    {

        public override void load()
        {
            if (PlayerPrefs.HasKey("PlayerManager"))
            {
                PlayerManager playerManager = new PlayerManager();
                /*
                // 캐릭터 목록 
                string loadPlayerCharacters = PlayerPrefs.GetString("playerCharacters");
                Debug.Log($"playerJson = {loadPlayerCharacters}");
                List<PixelHumanoid> playerCharacters = JsonConvert.DeserializeObject<List<PixelHumanoid>>(loadPlayerCharacters);

                // 아이템 목록
                string loadPlayerEquipItems = PlayerPrefs.GetString("playerEquipItems");
                Debug.Log($"itemJson = {loadPlayerEquipItems}");
                List<EquipItem> playerEquipItems = JsonConvert.DeserializeObject<List<EquipItem>>(loadPlayerEquipItems);

                foreach (PixelCharacter character in playerCharacters)
                {
                    character.loadForJson();
                }
                if (playerEquipItems != null)
                {
                    foreach (EquipItem item in playerEquipItems)
                    {
                        item.loadForJson();
                    }
                }
                else
                {
                    playerEquipItems = new List<EquipItem>();
                }
                // 플레이어 돈
                int playerGold = PlayerPrefs.GetInt("playerGold");
                int playerLife = PlayerPrefs.GetInt("playerLife");

                playerManager.Initialize(playerGold, playerLife, playerCharacters, playerEquipItems);
                PlayerManager.Initialize(playerManager);
                */

                // 아래부터 신버전
                JObject jplayerManager = JObject.Parse(PlayerPrefs.GetString("PlayerManager"));
                int playerGold = (int)jplayerManager["playerGold"];
                int playerLife = (int)jplayerManager["playerLife"];

                // 장착처리를 위한 map 
                Dictionary<string, EquipItem> itemMap = new Dictionary<string, EquipItem>(); 

                // 아이템 
                JArray jitems = (JArray) jplayerManager["items"];

                List<EquipItem> equipItems = new List<EquipItem>();
                foreach(JObject jitem in jitems)
                {
                    EquipItem equipItem = new EquipItem();
                    string ownerID = equipItem.fromJson(jitem);
                    equipItems.Add(equipItem);
                    if(ownerID != null)
                    {
                        itemMap[equipItem.id] = equipItem;
                    }
                }

                // 캐릭터  
                JArray jcharacters = (JArray)jplayerManager["characters"];
                List<PixelCharacter> characters = new List<PixelCharacter>();
                foreach (JObject jcharacter in jcharacters)
                {
                    PixelHumanoid character = new PixelHumanoid();
                    character.fromJson(jcharacter, itemMap);
                    characters.Add(character);
                }
                playerManager.Initialize(playerGold, playerLife, characters, equipItems);
                PlayerManager.Initialize(playerManager);


            }
            else
            {
                PlayerManager playerManager = new PlayerManager();
                playerManager.Initialize();
                PlayerManager.Initialize(playerManager);
                Debug.Log($"new Manager");
            }
        }

        public override void save()
        {
            PlayerManager playerManager = PlayerManager.Instance();
            // 구버전
            foreach (PixelCharacter character in playerManager.playerCharacters)
            {
                character.saveForJson();
            }
            PlayerPrefs.SetInt("playerGold", playerManager.playerGold);
            PlayerPrefs.SetInt("playerLife", playerManager.playerLife);
            PlayerPrefs.SetString("playerCharacters", JsonConvert.SerializeObject(playerManager.playerCharacters));
            PlayerPrefs.SetString("playerEquipItems", JsonConvert.SerializeObject(playerManager.playerEquipItems));


            // item save
            JArray itemArray = new JArray();
            foreach (EquipItem item in playerManager.playerEquipItems)
            {
                itemArray.Add(item.toJson());
            }

            // character save
            JArray characterArray = new JArray();
            foreach(PixelCharacter character in playerManager.playerCharacters)
            {
                characterArray.Add(character.toJson());
            }

            // 통합
            JObject jplayerManager = new JObject();
            jplayerManager["characters"] = characterArray;
            jplayerManager["items"] = itemArray;
            jplayerManager["playerGold"] = playerManager.playerGold;
            jplayerManager["playerLife"] = playerManager.playerLife;
            Debug.Log(jplayerManager);
            PlayerPrefs.SetString("PlayerManager", jplayerManager.ToString());
        }

        public override void delete()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
