using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly string DEFAULT_LOCALE = "en";
        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync(string? locale = null)
        {            
            IEnumerable<Question> questions = await _questionRepository.GetAllAsync();

            // Do not load additional locae if it's the default locale
            if (string.IsNullOrEmpty(locale) || locale.ToLower().Equals(DEFAULT_LOCALE))
                locale = null;

            // Map questions to QuestionDto and handle localization
            return questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text[DEFAULT_LOCALE], 
                TextLocal = GetLocalizedText(q.Text, locale), // Localized text or empty if not found
                Type = q.Type,
                Options = q.Options.Select(o => new OptionDto
                {
                    Text = o.Text[DEFAULT_LOCALE],
                    TextLocal = GetLocalizedText(o.Text, locale),
                    IsCorrect = o.IsCorrect,
                    Image = o.Image
                }).ToList(),
                CorrectAnswer = q.CorrectAnswer,
                Tags = q.Tags,
                Explanation = q.Explanation[DEFAULT_LOCALE],
                ExplanationLocal = GetLocalizedText(q.Explanation, locale),
                Reference = q.Reference,
                Image = q.Image,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            });
        }

        // Helper method to get localized text from the dictionary based on the locale
        private string GetLocalizedText(Dictionary<string, string> localizedText, string? locale)
        {
            if (locale == null) return string.Empty;

            if (localizedText != null && localizedText.ContainsKey(locale))
            {
                return localizedText[locale];
            }

            // Fallback to English if the localized text for the selected locale is missing
            return localizedText.ContainsKey(DEFAULT_LOCALE) ? localizedText[DEFAULT_LOCALE] : string.Empty;
        }
    }
}
