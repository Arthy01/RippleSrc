using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Ripple
{
    public class PexelsImage : MonoBehaviour
    {
        private static int MAX_GENERATIONS_PER_RUN = -1;
        private static int generations = 0;

        private static Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();

        [SerializeField] private RawImage _rawImage;

        private const string API_KEY = "DbH0mNCLAeHqbYPWF7VQZNzMwLSjSsu9GEY0wrOy4ZEUGDYOBkL1Sj8K";
        private const string PEXELS_URL = "https://api.pexels.com/v1/search?query=";


        public void GenerateImage(string searchTerm, int cacheIndex)
        {
            if (cache.ContainsKey(cacheIndex))
            {
                _rawImage.texture = cache[cacheIndex];
                return;
            }

            if (generations >= MAX_GENERATIONS_PER_RUN && MAX_GENERATIONS_PER_RUN >= 0)
                return;

            StartCoroutine(GetPexelsImages(searchTerm, cacheIndex));
            generations++;
        }

        private IEnumerator GetPexelsImages(string searchTerm, int cacheIndex)
        {
            using (UnityWebRequest request = new UnityWebRequest(PEXELS_URL + searchTerm, "GET"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", API_KEY);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    PexelsResponse response = JsonUtility.FromJson<PexelsResponse>(jsonResponse);

                    // Lade das erste Bild aus der Antwort
                    if (response != null && response.photos.Length > 0)
                    {
                        int randomIndex = Random.Range(0, response.photos.Length);  // Zufälligen Index generieren
                        string imageUrl = response.photos[randomIndex].src.medium;
                        StartCoroutine(LoadImage(imageUrl, cacheIndex));
                    }
                }
            }
        }

        private IEnumerator LoadImage(string url, int cacheIndex)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                _rawImage.texture = texture;
                cache[cacheIndex] = texture;
            }
        }

        [System.Serializable]
        private class PexelsResponse
        {
            public Photo[] photos;
        }

        [System.Serializable]
        private class Photo
        {
            public Src src;
        }

        [System.Serializable]
        private class Src
        {
            public string medium;
        }
    }
}


