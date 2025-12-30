using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;
using System.IO;

/// <summary>
/// Stability AI - SDXL 1.0 전용 이미지 생성기 (안정성 최우선)
/// </summary>
public class ImageGenerator : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string stabilityApiKey = "YOUR_STABILITY_AI_KEY_Here";
    [SerializeField] private int numberOfImages = 1;
    [SerializeField] private float timeout = 60f;

    // SDXL 1.0 API Endpoint (가장 안정적)
    private const string API_URL = "https://api.stability.ai/v1/generation/stable-diffusion-xl-1024-v1-0/text-to-image";

    // 프롬프트 스타일 (한국 어린이 애니메이션 스타일 - 핑*, 뽀*로 느낌)
    private const string PROMPT_SUFFIX = ", cute korean anime style, chibi style, pastel colors, sparkly eyes, clean lines, high quality, vibrant, manhwa style for kids, soft lighting, 3d render";

    private Texture2D[] generatedImages;
    private Action<Texture2D[]> onComplete;

    public void GenerateImages(string keywords, Action<Texture2D[]> callback)
    {
        // 프롬프트 강화
        string enhancedPrompt = $"A cute illustration of {keywords}{PROMPT_SUFFIX}";

        Debug.Log($"[ImageGenerator] Requesting SDXL... Prompt: {enhancedPrompt}");
        onComplete = callback;

        StartCoroutine(GenerateRoutine(enhancedPrompt));
    }

    private IEnumerator GenerateRoutine(string prompt)
    {
        // SDXL 1.0 요청 바디
        string body = $@"{{
            ""text_prompts"": [
                {{
                    ""text"": ""{prompt}"",
                    ""weight"": 1
                }}
            ],
            ""cfg_scale"": 7,
            ""height"": 1024,
            ""width"": 1024,
            ""samples"": {numberOfImages},
            ""steps"": 30
        }}";

        using (UnityWebRequest req = new UnityWebRequest(API_URL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", "Bearer " + stabilityApiKey);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            req.timeout = (int)timeout;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                ProcessResponse(req.downloadHandler.text, prompt);
            }
            else
            {
                Debug.LogError($"[Stability Error] {req.error}\nResponse: {req.downloadHandler.text}");
                FinishWithPlaceholder();
            }
        }
    }

    private void ProcessResponse(string json, string prompt)
    {
        try
        {
            generatedImages = new Texture2D[numberOfImages];
            int count = 0;

            string key = "\"base64\"";
            int currentIndex = 0;

            while (count < numberOfImages)
            {
                int keyIndex = json.IndexOf(key, currentIndex);
                if (keyIndex == -1) break;

                int colonIndex = json.IndexOf(":", keyIndex + key.Length);
                if (colonIndex == -1) break;

                int valueQuoteStart = json.IndexOf("\"", colonIndex);
                if (valueQuoteStart == -1) break;

                int dataStart = valueQuoteStart + 1;
                int dataEnd = json.IndexOf("\"", dataStart);
                if (dataEnd == -1) break;

                string b64 = json.Substring(dataStart, dataEnd - dataStart);

                byte[] imageBytes = Convert.FromBase64String(b64);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(imageBytes))
                {
                    generatedImages[count] = tex;
                    SaveImageToDisk(imageBytes, prompt, count);
                    count++;
                }

                currentIndex = dataEnd;
            }

            Debug.Log($"[ImageGenerator] Success! Loaded {count} images.");

            for (; count < numberOfImages; count++) generatedImages[count] = CreatePlaceholderImage(count);
            onComplete?.Invoke(generatedImages);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageGenerator] Parse Error: {e.Message}");
            FinishWithPlaceholder();
        }
    }

    private void SaveImageToDisk(byte[] bytes, string prompt, int index)
    {
        string folderPath = Path.Combine(Application.dataPath, "GeneratedImages");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string sanitisedPrompt = prompt;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitisedPrompt = sanitisedPrompt.Replace(c, '_');
        }

        if (sanitisedPrompt.Length > 50) sanitisedPrompt = sanitisedPrompt.Substring(0, 50);

        string dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{sanitisedPrompt}_{dateStr}_{index}.png";

        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"[ImageGenerator] Saved image to: {fullPath}");
    }

    private void FinishWithPlaceholder()
    {
        generatedImages = new Texture2D[numberOfImages];
        for (int i = 0; i < numberOfImages; i++) generatedImages[i] = CreatePlaceholderImage(i);
        onComplete?.Invoke(generatedImages);
    }

    private Texture2D CreatePlaceholderImage(int index)
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size);
        Color baseColor = Color.yellow;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, baseColor);
        tex.Apply();
        return tex;
    }
}
