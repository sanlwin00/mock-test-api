using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using MongoDB.Bson;

namespace MockTestApi.Services
{
    public class MockTestService : IMockTestService
    {
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestService(IMockTestRepository mockTestRepository)
        {
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MockTest> CreateAsync(MockTest mockTest)
        {
            await _mockTestRepository.CreateAsync(mockTest);
            return mockTest;
        }

        public async Task<bool> UpdateProgressAsync(string id, UpdateMockTestDto updateMockTestDto)
        {
            var mockTest = await _mockTestRepository.GetByIdAsync(id);
            if (mockTest == null)
            {
                Console.WriteLine($"MockTest not found for id: {id}");
                return false;
            }

            var question = mockTest.Questions.FirstOrDefault(q => q.QuestionId == updateMockTestDto.QuestionId);
            if (question == null)
            {
                Console.WriteLine($"Question not found for questionId: {updateMockTestDto.QuestionId}");
                return false;
            }

            Console.WriteLine($"Updating progress - ID: {id}, QuestionId: {updateMockTestDto.QuestionId}, SelectedOption: {updateMockTestDto.SelectedOption}, UserAnswer: {updateMockTestDto.UserAnswer}, ReviewLater: {updateMockTestDto.ReviewLater}");

            return await _mockTestRepository.UpdateProgressAsync(id, updateMockTestDto.QuestionId, updateMockTestDto.UserAnswer, updateMockTestDto.SelectedOption, updateMockTestDto.ReviewLater);
        }

        public async Task<bool> CompleteTestAsync(string id, CompleteMockTestDto completeMockTestDto)
        {
            var mockTest = await _mockTestRepository.GetByIdAsync(id);
            if (mockTest == null) return false;

            return await _mockTestRepository.CompleteTestAsync(id, completeMockTestDto.Results);
        }


        public Task<IEnumerable<MockTest>> GetAllMockTestsAsync() =>
            _mockTestRepository.GetAllAsync();

        public Task<MockTest> GetMockTestByIdAsync(string id) =>
            _mockTestRepository.GetByIdAsync(id);

        public Task<IEnumerable<MockTest>> GetMockTestsByUserIdAsync(string userId) =>
            _mockTestRepository.GetByUserIdAsync(userId);
    }

}
