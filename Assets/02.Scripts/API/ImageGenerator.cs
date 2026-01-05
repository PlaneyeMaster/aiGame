using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

/// <summary>
/// Stability AI - SDXL 1.0 이미지 생성기 (안전 필터 적용)
/// </summary>
public class ImageGenerator : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string stabilityApiKey = "YOUR_STABILITY_AI_KEY_Here";
    [SerializeField] private int numberOfImages = 1;
    [SerializeField] private float timeout = 60f;

    // SDXL 1.0 API Endpoint
    private const string API_URL = "https://api.stability.ai/v1/generation/stable-diffusion-xl-1024-v1-0/text-to-image";

    // 어린이 친화적 스타일
    private const string PROMPT_STYLE = ", cute korean anime style, chibi style, pastel colors, sparkly eyes, clean lines, high quality, vibrant, manhwa style for kids, soft lighting, 3d render, child-friendly, wholesome, bright and cheerful";

    private Texture2D[] generatedImages;
    private Action<Texture2D[]> onComplete;

    /// <summary>
    /// 문장 기반 이미지 생성 (StepWordSelector 연동)
    /// </summary>
    public void GenerateFromSentence(Action<Texture2D[]> callback)
    {
        if (StepWordSelector.Instance == null || !StepWordSelector.Instance.IsSelectionComplete)
        {
            Debug.LogError("[ImageGenerator] Sentence is not complete!");
            callback?.Invoke(null);
            return;
        }

        string sentence = StepWordSelector.Instance.GetPromptSentence();
        GenerateImages(sentence, callback);
    }

    /// <summary>
    /// 키워드로 이미지 생성
    /// </summary>
    public void GenerateImages(string keywords, Action<Texture2D[]> callback)
    {
        // 콘텐츠 필터 적용
        string safePrompt = keywords;
        string negativePrompt = "";
        
        if (ContentFilter.Instance != null)
        {
            safePrompt = ContentFilter.Instance.SanitizePrompt(keywords);
            negativePrompt = ContentFilter.Instance.GetNegativePrompt();
        }
        
        // 프롬프트 구성
        string enhancedPrompt = $"A cute illustration of {safePrompt}{PROMPT_STYLE}";

        Debug.Log($"[ImageGenerator] Prompt: {enhancedPrompt}");
        Debug.Log($"[ImageGenerator] Negative: {negativePrompt}");
        
        onComplete = callback;
        StartCoroutine(GenerateRoutine(enhancedPrompt, negativePrompt));
    }

    private IEnumerator GenerateRoutine(string prompt, string negativePrompt)
    {
        // SDXL 1.0 요청 바디 (Negative Prompt 포함)
        string body;
        
        if (!string.IsNullOrEmpty(negativePrompt))
        {
            body = $@"{{
                ""text_prompts"": [
                    {{
                        ""text"": ""{EscapeJson(prompt)}"",
                        ""weight"": 1
                    }},
                    {{
                        ""text"": ""{EscapeJson(negativePrompt)}"",
                        ""weight"": -1
                    }}
                ],
                ""cfg_scale"": 7,
                ""height"": 1024,
                ""width"": 1024,
                ""samples"": {numberOfImages},
                ""steps"": 30
            }}";
        }
        else
        {
            body = $@"{{
                ""text_prompts"": [
                    {{
                        ""text"": ""{EscapeJson(prompt)}"",
                        ""weight"": 1
                    }}
                ],
                ""cfg_scale"": 7,
                ""height"": 1024,
                ""width"": 1024,
                ""samples"": {numberOfImages},
                ""steps"": 30
            }}";
        }

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
                ProcessResponse(req.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Stability Error] {req.error}\nResponse: {req.downloadHandler.text}");
                FinishWithPlaceholder();
            }
        }
    }

    private string EscapeJson(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    private void ProcessResponse(string json)
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
