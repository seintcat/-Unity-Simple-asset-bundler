using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    public delegate void LoadDelegate(string bundleInfo);

    private LoadDelegate loadedEvent;
    private bool loaded;
    private string bundleInfo;
    private IEnumerator load;
    private Dictionary<string, AssetBundleCreateRequest> loadedBundles;


    public bool Load(string bundleName, LoadDelegate _loadedEvent)
    {
        if(!loaded) 
            return false;

        loadedEvent = _loadedEvent;
        load = Loading(bundleName);
        StartCoroutine(load);

        return true;
    }

    private IEnumerator Loading(string bundleName)
    {
        string path = Application.streamingAssetsPath + "/" + bundleName;
        loaded = false;
        bundleInfo = path;

        AssetBundleCreateRequest bundleAllFilesRequest = AssetBundle.LoadFromFileAsync(path);
        while (!bundleAllFilesRequest.isDone)
            yield return new WaitForSeconds(0.1f);

        AssetBundleRequest oneFileRequest = bundleAllFilesRequest.assetBundle.LoadAssetAsync("info");
        while (!oneFileRequest.isDone)
            yield return new WaitForSeconds(0.1f);

        bundleInfo = oneFileRequest.asset.ToString();
        loadedBundles.Add(bundleName, bundleAllFilesRequest);
        Loaded();
    }

    private void Loaded()
    {
        loaded = true;
        loadedEvent.Invoke(bundleInfo);
    }

    public Object[] GetFile<T>(string bundleName, string info)
    {
        return loadedBundles[bundleName].assetBundle.LoadAssetWithSubAssets<Object>(info);
    }

    public void Unload(string bundleName)
    {
        AssetBundleCreateRequest bundle = loadedBundles[bundleName];
        loadedBundles.Remove(bundleName);
        bundle.assetBundle.Unload(true);
    }

    public void UnloadAll()
    {
        while(loadedBundles.Count > 0)
        {
            Unload(loadedBundles.Keys.ToList()[0]);
        }
    }
}