using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;

public class ExishFoilerScript : MonoBehaviour {

    const string url = "https://ktane.timwi.de/json/raw";
    public KMBombInfo Bomb;
    public KMGameInfo Game;
    private bool got;
    private List<string> exishMods = new List<string>();
    private KMBombModule[] mods;
    private KMBombModule[] exishModsOnBomb;

    void Start()
    { 
        Debug.Log("[eXish Foiler] Loaded eXish Foiler.");
        if (Application.isEditor)
            StartCoroutine(Begin());
        else Game.OnStateChange += st => { if (st == KMGameInfo.State.Gameplay) StartCoroutine(LoadComponents()); };
    }
    IEnumerator LoadComponents()
    {
        yield return new WaitUntil(() => Bomb != null && Bomb.IsBombPresent() && Bomb.GetModuleNames().Count() > 0);
        yield return Begin();
    }

    private IEnumerator Begin()
    {
        Debug.Log("[eXish Foiler] Begin Collection.");
        mods = FindObjectsOfType<KMBombModule>();
        yield return GetJSON();
        if (!got)
            yield break;
        Debug.Log("[eXish Foiler] JSON successfully gotten.");
        exishModsOnBomb = mods.Where(m => exishMods.Contains(m.ModuleType)).ToArray();
        foreach (KMBombModule mod in exishModsOnBomb)
            ProcessMod(mod);
    }
    class KtaneData {
        public List<Dictionary<string, object>> KtaneModules { get; set; }
    }
    public IEnumerator GetJSON()
    {
        WWW rq = new WWW(url);
        yield return rq;
        if (rq.error != null)
            yield break;
        got = true;
        string raw = rq.text;
        KtaneData data = JsonConvert.DeserializeObject<KtaneData>(raw);
        foreach (var module in data.KtaneModules)
        {
            string modId = (string)module["ModuleID"];
            string[] contributors = ((string)module["Author"]).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (contributors.Contains("eXish"))
            {
                Debug.LogFormat("[eXish Foiler] Found eXish mod {0}.", modId);
                exishMods.Add(modId);
            }
        }
    }
    public void ProcessMod(KMBombModule mod)
    {
        var sls = mod.GetComponentsInChildren<KMStatusLightParent>();
        if (sls.Count() != 1)
            return;
        var sl = sls.Single();
        Debug.LogFormat("[eXish Foiler] Processing module {0} ({1}).", mod.ModuleDisplayName, mod.ModuleType);
        Vector3 pos = sl.transform.localPosition;
        if (pos.y == 0.2f)
        {
            sl.transform.localPosition = new Vector3(pos.x, 0.01986f, pos.z);
        }
    }
}
