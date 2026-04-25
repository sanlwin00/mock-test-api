using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly ITestRepository _testRepository;
        private readonly string DEFAULT_LOCALE = "en";

        public QuestionService(IQuestionRepository questionRepository, ITestRepository testRepository)
        {
            _questionRepository = questionRepository;
            _testRepository = testRepository;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync(string? locale = null)
        {
            var questions = await _questionRepository.GetAllAsync();
            return MapToDto(questions, locale);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByTestIdAsync(string testId, string? locale = null)
        {
            var test = await _testRepository.GetByIdAsync(testId);
            if (test == null) return Enumerable.Empty<QuestionDto>();

            var questionIds = test.Questions.Select(q => q.QuestionId);
            var questions = await _questionRepository.GetByIdsAsync(questionIds);

            // Preserve the test's question order
            var orderedIds = test.Questions.OrderBy(q => q.Sequence).Select(q => q.QuestionId).ToList();
            questions = questions.OrderBy(q => orderedIds.IndexOf(q.Id));

            return MapToDto(questions, locale);
        }

        private IEnumerable<QuestionDto> MapToDto(IEnumerable<Question> questions, string? locale)
        {
            if (string.IsNullOrEmpty(locale) || locale.Equals(DEFAULT_LOCALE, StringComparison.OrdinalIgnoreCase))
                locale = null;

            return questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text[DEFAULT_LOCALE],
                TextLocal = GetLocalizedText(q.Text, locale),
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
