using System;
using System.Collections.Generic;
using UnityEngine;

namespace lee
{
    public class BattleTest : MonoBehaviour
    {
        [Serializable]
        public class BuildTarget
        {
            public string name;
            public Vector3 position;
        }

        public BuildTarget[] team0Characters;
        public BuildTarget[] team1Characters;

        private void Start()
        {
            spawnCharacters();
        }

        List<PixelHumanoid> m_team0Humanoids = new List<PixelHumanoid>();
        List<PixelHumanoid> m_team1Humanoids = new List<PixelHumanoid>();
        private void spawnCharacters()
        {
            foreach (BuildTarget target in team0Characters)
            {
                if (target == null)
                    continue;

                PixelHumanoid humanoid = MyCharacterFactory.Instance().CreatePixelHumanoid(target.name, target.position, transform);
                humanoid.SetDirectionLeft(Utility.Direction2.Right);
                humanoid.teamIndex = 0;
                humanoid.bm = BattleManager.Instance();

                m_team0Humanoids.Add(humanoid);
            }

            foreach (BuildTarget target in team1Characters)
            {
                if (target == null)
                    continue;

                PixelHumanoid humanoid = MyCharacterFactory.Instance().CreatePixelHumanoid(target.name, target.position, transform);
                humanoid.SetDirectionLeft(Utility.Direction2.Left);
                humanoid.teamIndex = 1;
                humanoid.bm = BattleManager.Instance();

                m_team1Humanoids.Add(humanoid);
            }
        }

        public void StartBattle()
        {
            BattleManager.EStatus battleStatus = BattleManager.Instance().GetStatus();
            if (battleStatus == BattleManager.EStatus.Waiting)
            {
                BattleManager.Instance().StartBattle(m_team0Humanoids, m_team1Humanoids);
            }
            else if (battleStatus == BattleManager.EStatus.Fighting)
            {
                Debug.LogError("Battle already started");
            }
        }
    }
}