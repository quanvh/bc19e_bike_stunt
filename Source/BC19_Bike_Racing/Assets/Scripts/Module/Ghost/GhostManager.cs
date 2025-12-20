using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Bacon
{
    public class GhostManager : MonoBehaviour
    {
        private List<GhostShot> framesList;
        private float recordTime = 0.0f;
        public Transform target = null;

        private bool recording = false;

        public bool gamePlaying = false;

        private List<GhostShot> lastReplayList = null;
        private readonly float replayTimescale = 1f;
        private int replayIndex = 0;
        private bool ghostPlaying = false;
        private float replayTime = 0.0f;

        private GameObject theGhost;
        private PlayerModel _player;


        public void ClearGhost()
        {
            if (theGhost)
            {
                Destroy(theGhost);
                theGhost = null;
            }
        }

        public void StartReplay()
        {
            _player = DataManager.Instance._player;
            LoadFromFile();
            PlayRecorded();
        }

        public void PlayRecorded()
        {
            if (lastReplayList != null && lastReplayList.Count > 0)
            {
                ClearGhost();
                theGhost = Instantiate(DataManager.Instance.CurrentCar.CarGhost, transform);
                replayIndex = 0;
                ghostPlaying = true;
            }
            else ghostPlaying = false;
        }

        void StopReplay()
        {
            ghostPlaying = false;
            theGhost.SetActive(false);
        }

        public void StartRecord(Transform _target = null)
        {
            _player = DataManager.Instance._player;
            target = _target;

            framesList = new List<GhostShot>();
            recordTime = Time.time * 1000;
            recording = true;
        }

        public void StopRecord()
        {
            LevelModel CurrentLevel = DataManager.Instance.CurrentLevel;
            recording = false;
            gamePlaying = false;
            if (LevelController.Instance.isComplete &&
                (LevelController.Instance.timeLevel < CurrentLevel.Time || CurrentLevel.Time <= 0))
            {
                SaveGhostToFile();
            }
        }

        private void Update()
        {
            if (!gamePlaying) return;
            RecordFrame();
            MoveGhost();
        }

        public void LoadFromJson(string a_Json)
        {
            GhostData ghostData = new GhostData(new List<GhostShot>());
            JsonUtility.FromJsonOverwrite(a_Json, ghostData);
            lastReplayList = ghostData.ghostShots;
        }

        public void LoadFromFile()
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath + "/Level" + _player.currentLevel + ".txt")))
            {
                string saveString = File.ReadAllText(Path.Combine(Application.persistentDataPath + "/Level" + _player.currentLevel + ".txt"));
                LoadFromJson(saveString);
            }
            else
            {
                //Debug.Log("No Data Found, read from resource");
                string filename = "Ghost/Level" + _player.currentLevel;
                TextAsset theTextFile = Resources.Load<TextAsset>(filename);
                if (theTextFile != null)
                {
                    string saveString = theTextFile.text;
                    LoadFromJson(saveString);
                }
            }
        }

        public void MoveGhost()
        {
            if (lastReplayList == null || !ghostPlaying)
            {
                return;
            }
            replayIndex++;

            if (replayIndex < lastReplayList.Count)
            {
                GhostShot frame = lastReplayList[replayIndex];
                DoLerp(lastReplayList[replayIndex - 1], frame);
                replayTime += Time.smoothDeltaTime * 1000 * replayTimescale;
            }
            else
            {
                StopReplay();
            }
        }

        private void DoLerp(GhostShot a, GhostShot b)
        {
            {
                theGhost.transform.SetPositionAndRotation(Vector3.Slerp(a.posMark, b.posMark, Mathf.Clamp(replayTime, a.timeMark, b.timeMark)),
                    Quaternion.Slerp(a.rotMark, b.rotMark, Mathf.Clamp(replayTime, a.timeMark, b.timeMark)));
            }
        }

        public void RecordFrame()
        {
            if (!recording) return;
            recordTime += Time.smoothDeltaTime * 1000;
            if (target == null)
            {
                GhostShot newFrame = new GhostShot()
                {
                    timeMark = recordTime,

                    posMark = this.transform.position,
                    rotMark = this.transform.rotation
                };

                framesList.Add(newFrame);
            }
            else
            {
                GhostShot newFrame = new GhostShot()
                {
                    timeMark = recordTime,

                    posMark = target.position,
                    rotMark = target.rotation
                };

                framesList.Add(newFrame);
            }
        }

        public void SaveGhostToFile()
        {
            GhostData ghostData = new GhostData(framesList);

            File.WriteAllText(Path.Combine(Application.persistentDataPath + "/Level" + _player.currentLevel + ".txt"), JsonUtility.ToJson(ghostData));
        }

    }


    [System.Serializable]
    public class GhostShot
    {
        public float timeMark = 0.0f;
        public Vector3 posMark = Vector3.zero;
        public Quaternion rotMark = Quaternion.identity;
    }

    public class GhostData
    {
        public List<GhostShot> ghostShots = new List<GhostShot>();
        public GhostData(List<GhostShot> _ghostShots)
        {
            ghostShots = _ghostShots;
        }
    }
}