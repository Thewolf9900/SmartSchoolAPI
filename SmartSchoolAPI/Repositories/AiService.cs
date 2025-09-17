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

        // Using different models for different tasks is a good practice
        private const string GenerationModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";
        //private const string ChatModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key=";
        private const string ChatModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";

        public AiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["AiService:GeminiApiKey"]
                      ?? throw new InvalidOperationException("مفتاح Gemini API غير موجود في ملف الإعدادات appsettings.json.");
        }

        #region Question Generation Logic

        public async Task<IEnumerable<CreateQuestionDto>> GenerateQuestionsAsync(GenerateQuestionsFromTextDto generationParams, string language)
        {
            var client = _httpClientFactory.CreateClient();
            var fullApiUrl = $"{GenerationModelUrl}{_apiKey}";

            string prompt = BuildQuestionPrompt(generationParams, language);

            var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(fullApiUrl, content);

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
            var fullApiUrl = $"{ChatModelUrl}{_apiKey}";

            var chatHistoryForApi = BuildChatHistoryForApi(messageHistory);

            var requestBody = new
            {
                contents = chatHistoryForApi,
                generationConfig = new { maxOutputTokens = 1024 }
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(fullApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error from Gemini API: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var generatedText = ExtractGeneratedText(jsonResponse);

            return string.IsNullOrWhiteSpace(generatedText) ? "عذرًا، لم أتمكن من معالجة طلبك حاليًا." : generatedText;
        }

        private object[] BuildChatHistoryForApi(IEnumerable<ChatMessage> messageHistory)
        {
            // Convert our ChatMessage history to the format Gemini expects for multi-turn chat
            var history = messageHistory.Select(msg => new {
                role = msg.Sender.ToLower() == "user" ? "user" : "model",
                parts = new[] { new { text = msg.Content } }
            }).ToList();

            // Optionally add a system instruction for persona (though it's often better to put it as the first 'user' message)
            // Example:
            // var systemInstruction = new { role = "system", parts = new[] { new { text = "You are a helpful assistant..." } } };
            // history.Insert(0, systemInstruction);

            return history.ToArray();
        }

        #endregion

        #region Shared Helper Methods

        private string ExtractGeneratedText(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var text = doc.RootElement
                              .GetProperty("candidates")[0]
                              .GetProperty("content")
                              .GetProperty("parts")[0]
                              .GetProperty("text")
                              .GetString();
                return text?.Trim().Replace("```json", "").Replace("```", "") ?? "";
            }
            catch (Exception)
            {
                return jsonResponse;
            }
        }

        #endregion
    }
}