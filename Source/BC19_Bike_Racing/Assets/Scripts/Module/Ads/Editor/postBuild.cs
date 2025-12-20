#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class postBuild
{
    private const string KEY_SK_ADNETWORK_ITEMS = "SKAdNetworkItems";
    private const string KEY_SK_ADNETWORK_ID = "SKAdNetworkIdentifier";
    const string TrackingDescription = "This game includes ads. To improve your experience and see ads that match your interests, allow tracking.";

    static List<string> skAdNetworkIds = new List<string>
    {
        "9g2aggbj52.skadnetwork",
        "su67r6k2v3.skadnetwork",
        "f7s53z58qe.skadnetwork",
        "2u9pt9hc89.skadnetwork",
        "hs6bdukanm.skadnetwork",
        "8s468mfl3y.skadnetwork",
        "c6k4g5qg8m.skadnetwork",
        "v72qych5uu.skadnetwork",
        "44jx6755aq.skadnetwork",
        "prcb7njmu6.skadnetwork",
        "m8dbw4sv7c.skadnetwork",
        "3rd42ekr43.skadnetwork",
        "4fzdc2evr5.skadnetwork",
        "t38b2kh725.skadnetwork",
        "f38h382jlk.skadnetwork",
        "424m5254lk.skadnetwork",
        "ppxm28t8ap.skadnetwork",
        "av6w8kgt66.skadnetwork",
        "4pfyvq9l8r.skadnetwork",
        "yclnxrl5pm.skadnetwork",
        "tl55sbb4fm.skadnetwork",
        "mlmmfzh3r3.skadnetwork",
        "klf5c3l5u5.skadnetwork",
        "9t245vhmpl.skadnetwork",
        "9rd848q2bz.skadnetwork",
        "7ug5zh24hu.skadnetwork",
        "4468km3ulz.skadnetwork",
        "7rz58n8ntl.skadnetwork",
        "ejvt5qm6ak.skadnetwork",
        "5lm9lj6jb7.skadnetwork",
        "mtkv5xtk9e.skadnetwork",
        "6g9af3uyq4.skadnetwork",
        "uw77j35x4d.skadnetwork",
        "u679fj5vs4.skadnetwork",
        "rx5hdcabgc.skadnetwork",
        "g28c52eehv.skadnetwork",
        "cg4yq2srnc.skadnetwork",
        "9nlqeag3gk.skadnetwork",
        "275upjj5gd.skadnetwork",
        "wg4vff78zm.skadnetwork",
        "qqp299437r.skadnetwork",
        "2fnua5tdw4.skadnetwork",
        "3qcr597p9d.skadnetwork",
        "3qy4746246.skadnetwork",
        "3sh42y64q3.skadnetwork",
        "5a6flpkh64.skadnetwork",
        "cstr6suwn9.skadnetwork",
        "e5fvkxwrpn.skadnetwork",
        "kbd757ywx3.skadnetwork",
        "n6fk4nfna4.skadnetwork",
        "p78axxw29g.skadnetwork",
        "s39g8k73mm.skadnetwork",
        "wzmmz9fp6w.skadnetwork",
        "ydx93a7ass.skadnetwork",
        "zq492l623r.skadnetwork",
        "24t9a8vw3c.skadnetwork",
        "32z4fx6l9h.skadnetwork",
        "523jb4fst2.skadnetwork",
        "54nzkqm89y.skadnetwork",
        "578prtvx9j.skadnetwork",
        "5l3tpt7t6e.skadnetwork",
        "6xzpu9s2p8.skadnetwork",
        "79pbpufp6p.skadnetwork",
        "9b89h5y424.skadnetwork",
        "cj5566h2ga.skadnetwork",
        "feyaarzu9v.skadnetwork",
        "ggvn48r87g.skadnetwork",
        "glqzh8vgby.skadnetwork",
        "gta9lk7p23.skadnetwork",
        "k674qkevps.skadnetwork",
        "ludvb6z3bs.skadnetwork",
        "n9x2a789qt.skadnetwork",
        "pwa73g5rt2.skadnetwork",
        "xy9t38ct57.skadnetwork",
        "zmvfpc5aq8.skadnetwork",
        "v9wttpbfk9.skadnetwork",
        "n38lu8286q.skadnetwork",
        "97r2b46745.skadnetwork",
        "f73kdq92p3.skadnetwork",
        "w9q455wk68.skadnetwork",
        "238da6jt44.skadnetwork",
        "22mmun2rn5.skadnetwork",
        "252b5q8x7y.skadnetwork",
        "44n7hlldy6.skadnetwork",
        "488r3q3dtq.skadnetwork",
        "52fl2v3hgk.skadnetwork",
        "5tjdwbrq8w.skadnetwork",
        "737z793b9f.skadnetwork",
        "9yg77x724h.skadnetwork",
        "dzg6xy7pwj.skadnetwork",
        "ecpz2srf59.skadnetwork",
        "gvmwg8q7h5.skadnetwork",
        "hdw39hrw9y.skadnetwork",
        "lr83yxwka7.skadnetwork",
        "mls7yz5dvl.skadnetwork",
        "n66cz3y3bx.skadnetwork",
        "nzq8sh4pbs.skadnetwork",
        "pu4na253f3.skadnetwork",
        "v79kvwwj4g.skadnetwork",
        "y45688jllp.skadnetwork",
        "yrqqpx2mcb.skadnetwork",
        "z4gj7hsk7h.skadnetwork",
        "4dzt52r2t5.skadnetwork",
        "mp6xlyr22a.skadnetwork",
        "x44k69ngh6.skadnetwork",
        "7953jerfzd.skadnetwork",
    };

    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPListValues(pathToXcode);
            SetAdvertiserTrackingEnabled(pathToXcode);
            FixediOS13NotificationBug(pathToXcode);
        }
    }

    static void AddPListValues(string pathToXcode)
    {
        // Get Plist from Xcode project 
        string plistPath = pathToXcode + "/Info.plist";

        // Read in Plist 
        PlistDocument plistObj = new PlistDocument();
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // set values from the root obj
        PlistElementDict plistRoot = plistObj.root;

        // Set value in plist
        plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);
        plistRoot.SetString("NSCalendarsUsageDescription", "$(PRODUCT_NAME) user your calendar.");
        plistRoot.SetString("NSLocationAlwaysUsageDescription", "$(PRODUCT_NAME) user your localtion.");
        plistRoot.SetString("NSLocationWhenInUseUsageDescription", "$(PRODUCT_NAME) user your localtion.");
        plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);


        PlistElementArray array = GetSKAdNetworkItemsArray(plistObj);
        if (array != null)
        {
            foreach (string id in skAdNetworkIds)
            {
                if (!ContainsSKAdNetworkIdentifier(array, id))
                {
                    PlistElementDict added = array.AddDict();
                    added.SetString(KEY_SK_ADNETWORK_ID, id);
                }
            }
        }

        //save
        File.WriteAllText(plistPath, plistObj.WriteToString());

        Debug.Log("AddPListValues");
    }

    private static void SetAdvertiserTrackingEnabled(string pathToXcode)
    {
        var destPath = Path.Combine(pathToXcode, "Classes/UnityAppController.mm");
        string unityAppController = File.ReadAllText(destPath);

        // add set tracking for FAN
        if (!unityAppController.Contains("Profiler_InitProfiler();\n    [FBAdSettings setAdvertiserTrackingEnabled: YES];\n"))
        {
            unityAppController = unityAppController.Replace("#include <sys/sysctl.h>", "#include <sys/sysctl.h>\n#include <FBAudienceNetwork/FBAdSettings.h>");
            unityAppController = unityAppController.Replace("Profiler_InitProfiler();", "Profiler_InitProfiler();\n    [FBAdSettings setAdvertiserTrackingEnabled: YES];\n");
            File.WriteAllText(destPath, unityAppController);
        }


        Debug.Log("SetAdvertiserTrackingEnabled");
    }

    private static void FixediOS13NotificationBug(string pathToXcode)
    {
        string managerPath = pathToXcode + "/Libraries/com.unity.mobile.notifications/Runtime/iOS/Plugins/UnityNotificationManager.m";
        var text = File.ReadAllLines(managerPath).ToList();
        for (int i = 0; i < text.Count; i++)
        {
            if (text[i] == @"- (void)updateScheduledNotificationList")
            {
                text.RemoveRange(i, 7);
                text.Insert(i, @"- (void)updateScheduledNotificationList
                {
                    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
                    if (@available(iOS 15, *))
                    {
                        dispatch_async(dispatch_get_main_queue(), ^{
                            [center getPendingNotificationRequestsWithCompletionHandler:^(NSArray < UNNotificationRequest *> *_Nonnull requests) {
                                self.cachedPendingNotificationRequests = requests;
                            }];
                        });
                    }
                    else
                    {
                        [center getPendingNotificationRequestsWithCompletionHandler:^(NSArray < UNNotificationRequest *> *_Nonnull requests) {
                            self.cachedPendingNotificationRequests = requests;
                        }];
                    }

                }");
                break;
            }
        }

        File.WriteAllLines(managerPath, text.ToArray());

        Debug.Log("FixediOS13NotificationBug");
    }


#if UNITY_IOS
    static PlistElementArray GetSKAdNetworkItemsArray(PlistDocument document)
    {
        PlistElementArray array;
        if (document.root.values.ContainsKey(KEY_SK_ADNETWORK_ITEMS))
        {
            try
            {
                PlistElement element;
                document.root.values.TryGetValue(KEY_SK_ADNETWORK_ITEMS, out element);
                array = element.AsArray();
            }
#pragma warning disable 0168
            catch (Exception e)
#pragma warning restore 0168
            {
                // The element is not an array type.
                array = null;
            }
        }
        else
        {
            array = document.root.CreateArray(KEY_SK_ADNETWORK_ITEMS);
        }
        return array;
    }

    static bool ContainsSKAdNetworkIdentifier(PlistElementArray skAdNetworkItemsArray, string id)
    {
        foreach (PlistElement elem in skAdNetworkItemsArray.values)
        {
            try
            {
                PlistElementDict elemInDict = elem.AsDict();
                PlistElement value;
                bool identifierExists = elemInDict.values.TryGetValue(KEY_SK_ADNETWORK_ID, out value);

                if (identifierExists && value.AsString().Equals(id))
                {
                    return true;
                }
            }
#pragma warning disable 0168
            catch (Exception e)
#pragma warning restore 0168
            {
                // Do nothing
            }
        }

        return false;
    }
#endif
}
#endif