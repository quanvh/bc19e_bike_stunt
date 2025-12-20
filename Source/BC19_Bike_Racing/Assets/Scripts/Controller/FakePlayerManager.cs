using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FakePlayerManager : MonoBehaviour
{
    protected static FakePlayerManager Instance = null;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    protected static List<FakePlayerData> nameDatas = new List<FakePlayerData>();
    protected static List<FakePlayerData> NameDatas
    {
        get
        {
            if (nameDatas == null || nameDatas.Count == 0)
            {
                var asset = Resources.Load<TextAsset>("nickname");
                if (asset != null)
                {
                    string content = asset.ToString();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var loadedData = JsonUtility.FromJson<NameDatas>(content);
                        nameDatas = loadedData.items;
                    }
                }
                for (int i = 1; i < 20; i++)
                {
                    nameDatas.Add(new FakePlayerData { name = "Player " + (i * 1000 + i * i + UnityEngine.Random.Range(10000, 99999)) });
                }
                return nameDatas;
            }
            else
            {
                return nameDatas;
            }
        }
    }

    [SerializeField]
    protected List<Sprite> flagDefaults = null;

    [SerializeField]
    protected List<Sprite> flags = null;

    public List<Sprite> Flags
    {
        get
        {
            return flags;
        }
    }

    public static Sprite GetFlagCountry(string code)
    {
        if (Instance == null)
        {
            var prefab = Resources.Load<GameObject>("FakePlayerDataManager");
            var go = Instantiate(prefab);
            Instance = go.GetComponent<FakePlayerManager>();
        }

        if (Instance && Instance.flags != null)
        {
            var check = Instance.flags.FirstOrDefault(x => x.name == code);
            if (check != null)
                return check;
        }

        int indexFlag = UnityEngine.Random.Range(0, Instance.flagDefaults.Count);
        return Instance.flagDefaults[indexFlag];
    }

    protected static List<FakePlayerBase> tempUserFakes = new List<FakePlayerBase>();

    /// <summary>
    /// GetFakeUserData then fill data for FakeUserDataComponent
    /// </summary>
    /// <param name="take">number</param>
    /// <returns></returns>
    public List<FakePlayerBase> GetFakeUserData(int take = 5)
    {
        tempUserFakes.Clear();
        var rand = new System.Random();

        FakePlayerBase tempFakeData = null;

        var tempList = NameDatas.OrderBy(x => Guid.NewGuid()).Take(take).ToList();
        var tempFlag = Flags.OrderBy(x => Guid.NewGuid()).Take(take).ToList();

        for (int i = 0; i < take; i++)
        {
            try
            {
                var playerData = tempList[i];
                tempFakeData = new FakePlayerBase { name = playerData.name, countryCode = playerData.countryCode };

                if (string.IsNullOrEmpty(tempFakeData.countryCode))
                {
                    tempFakeData.avatar = tempFlag[i];
                }
                else
                {
                    tempFakeData.avatar = GetFlagCountry(tempFakeData.countryCode);
                }

                tempUserFakes.Add(tempFakeData);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        return tempUserFakes;
    }


    /// <summary>
    /// Get country from ip adress, please make sure if player.countyCode is NULL or EMPTY for the best performance
    /// </summary>
    /// <param name="onDone">Return PlayerData</param>
    /// <returns></returns>
    public static IEnumerator DOGetPlayerLocation(Action<FakePlayerData> onDone, int timeOut = 3)
    {
        if (Instance == null)
        {
            var prefab = Resources.Load<GameObject>("FakePlayerDataManager");
            var go = Instantiate(prefab);
            Instance = go.GetComponent<FakePlayerManager>();
        }

        var playerData = new FakePlayerData();

        string ipAddress = "";
        string urlIp = "https://api.ipify.org";

        Debug.Log("Start get ip: " + urlIp);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlIp))
        {
            webRequest.timeout = timeOut;
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                ipAddress = webRequest.downloadHandler.text;
            }
            else
            {
                Debug.LogError("ipToGeoLocation: Error: " + webRequest.error);
                onDone?.Invoke(playerData);
            }
        }

        if (!string.IsNullOrEmpty(ipAddress))
        {
            playerData.ipAddress = ipAddress;
            string urlGeolocation = string.Format("http://ip-api.com/json/{0}", ipAddress);
            Debug.Log("Start get data: " + urlGeolocation);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(urlGeolocation))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(": Error: " + webRequest.error);
                }
                else
                {
                    try
                    {
                        string stringData = webRequest.downloadHandler.text;
                        Debug.Log("GeoData:" + stringData);
                        var data = JsonUtility.FromJson<GeoData>(stringData);
                        if (data != null && !string.IsNullOrEmpty(data.country))
                        {
                            Debug.Log("User's Country: \"" + data.country + "\"; CountryCode: \"" + data.countryCode + "\"");

                            playerData.country = data.country.ToLower();
                            playerData.countryCode = data.countryCode.ToLower();
                        }
                        else
                        {
                            Debug.LogError("Unsuccessful GeoData request: " + stringData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);

                    }
                }
                onDone?.Invoke(playerData);
            }
        }
    }

}


#region Properties
[Serializable]
public class NameDatas
{
    public List<FakePlayerData> items;
}

[Serializable]
public class FakePlayerData
{
    public string name;
    public string country;
    public string countryCode;
    public string ipAddress;
}

[Serializable]
public class FakePlayerBase : FakePlayerData
{
    public bool isPlayer = false;
    public int order = -1;
    public Sprite avatar;

    public FakePlayerBase Clone()
    {
        return MemberwiseClone() as FakePlayerBase;
    }
}
#endregion

#region GeoData
/// <summary>
/// The Geo data for a user.
/// 
/// http://ip-api.com/docs/api:json
/// 
/// <code>
/// {
/// 	"status": "success",
/// 	"country": "COUNTRY",
/// 	"countryCode": "COUNTRY CODE",
/// 	"region": "REGION CODE",
/// 	"regionName": "REGION NAME",
/// 	"city": "CITY",
/// 	"zip": "ZIP CODE",
/// 	"lat": LATITUDE,
/// 	"lon": LONGITUDE,
/// 	"timezone": "TIME ZONE",
/// 	"isp": "ISP NAME",
/// 	"org": "ORGANIZATION NAME",
/// 	"as": "AS NUMBER / NAME",
/// 	"query": "IP ADDRESS USED FOR QUERY"
/// }
/// </code>
/// 
/// </summary>
[Serializable]
public class GeoData
{
    /// <summary>
    /// The status that is returned if the response was successful.
    /// </summary>
    public const string SuccessResult = "success";
    public string status;
    public string country;
    public string countryCode;
    public string region;
    public string regionName;
    public string city;
    public string zip;
    public double lat;
    public double lon;
    public string timezone;
    public string isp;
    public string org;
    public string query;
}
#endregion