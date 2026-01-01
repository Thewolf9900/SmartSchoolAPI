using SmartSchoolAPI.DTOs.QuestionBank;
using SmartSchoolAPI.Interfaces;
using System.Text;
using System.Text.Json;
using SmartSchoolAPI.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartSchoolAPI.Entities;
using System.Linq;

namespace SmartSchoolAPI.Services
{
    public class AiService : IAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        // Using Google Gemini API
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        
        public AiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["AiService:GeminiApiKey"] 
                      ?? configuration["AiService:AimlApiKey"] // Fallback attempt
                      ?? throw new InvalidOperationException("مفتاح Google Gemini API غير موجود. يرجى إضافته باسم GeminiApiKey.");
        }

        #region Question Generation Logic

        public async Task<IEnumerable<CreateQuestionDto>> GenerateQuestionsAsync(GenerateQuestionsFromTextDto generationParams, string language)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);

            string prompt = BuildQuestionPrompt(generationParams, language);
            
            // Google Gemini format
            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.95,
                    topK = 40,
                    maxOutputTokens = 2048,
                    responseMimeType = "text/plain"
                }
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
            
            // API Key is passed in URL query parameter for Gemini
            var urlWithKey = $"{BaseUrl}?key={_apiKey}";
            
            var response = await client.PostAsync(urlWithKey, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error from Gemini API: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var generatedText = ExtractGeneratedText(jsonResponse);

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                return Enumerable.Empty<CreateQuestionDto>();
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var generatedQuestions = JsonSerializer.Deserialize<List<CreateQuestionDto>>(generatedText, options);
                return generatedQuestions ?? new List<CreateQuestionDto>();
            }
            catch (JsonException jsonEx)
            {
                throw new JsonException($"Failed to deserialize the JSON response from AI. Response text: {generatedText}", jsonEx);
            }
        }

        private string BuildQuestionPrompt(GenerateQuestionsFromTextDto generationParams, string language)
        {
            string languageDescription = language.ToLower() == "arabic" ? "in Arabic" : "in English";

            return $@"
You are an expert AI assistant creating educational materials.
Your task is to generate questions based on the provided context.
**VERY IMPORTANT: The 'text' for the question and all of its 'options' MUST be generated strictly in the following language: {language}**

Follow these rules precisely:
1.  Generate exactly {generationParams.NumberOfQuestions} questions of type '{generationParams.QuestionType}'.
2.  The difficulty level for all questions must be '{generationParams.Difficulty}'.
3.  Each multiple-choice question must have 4 options. Each true-false question must have 2 options (True/False).
4.  Exactly one option per question must be correct.
5.  Your response MUST be ONLY a valid JSON array of objects. Do not include any text before or after the JSON array, and do not use markdown like ```json.
6.  Each object in the array must have this exact structure with English keys: {{ ""text"": ""The question text {languageDescription}"", ""questionType"": {(int)generationParams.QuestionType}, ""difficultyLevel"": {(int)generationParams.Difficulty}, ""options"": [ {{ ""text"": ""Option A {languageDescription}"", ""isCorrect"": false }}, {{ ""text"": ""Option B {languageDescription}"", ""isCorrect"": true }} ] }}

Context to use for generating the questions:
---
{generationParams.ContextText}
---
";
        }

        #endregion

        #region Chatbot Logic

        public async Task<string> GetChatResponseAsync(IEnumerable<ChatMessage> messageHistory)
        {
            var client = _httpClientFactory.CreateClient();
            // Auth is via URL param

            // Construct Gemini format messages
            var contents = new List<object>();
            foreach (var msg in messageHistory)
            {
                // Gemini uses 'model' instead of 'assistant'
                string role = msg.Sender.ToLower() == "user" ? "user" : "model";
                contents.Add(new 
                { 
                    role = role, 
                    parts = new[] { new { text = msg.Content } }
                });
            }

            var requestBody = new
            {
                contents = contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.95,
                    topK = 40,
                    maxOutputTokens = 2048,
                    responseMimeType = "text/plain"
                }
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var urlWithKey = $"{BaseUrl}?key={_apiKey}";
            var response = await client.PostAsync(urlWithKey, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error from Gemini API: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var generatedText = ExtractGeneratedText(jsonResponse);

            return string.IsNullOrWhiteSpace(generatedText) ? "عذرًا، لم أتمكن من معالجة طلبك حاليًا." : generatedText;
        }

        private string BuildChatPromptGeneric(IEnumerable<ChatMessage> messageHistory)
        {
            var sb = new StringBuilder();
            foreach (var msg in messageHistory)
            {
                if (msg.Sender.ToLower() == "user")
                {
                    sb.Append($"[INST] {msg.Content} [/INST]");
                }
                else
                {
                    sb.Append($" {msg.Content} </s>");
                }
            }
            return sb.ToString();
        }

        #endregion

        #region Shared Helper Methods

        private string ExtractGeneratedText(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var content = candidates[0].GetProperty("content");
                    if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();
                        return text?.Trim().Replace("```json", "").Replace("```", "") ?? "";
                    }
                }
                return jsonResponse;
            }
            catch (Exception)
            {
                return jsonResponse;
            }
        }

        #endregion
    }
}