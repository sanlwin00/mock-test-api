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
            if (mockTest == null) return false;

            var question = mockTest.Questions.FirstOrDefault(q => q.QuestionId == updateMockTestDto.QuestionId);
            if (question == null) return false;

            return await _mockTestRepository.UpdateProgressAsync(id, updateMockTestDto.QuestionId, updateMockTestDto.UserAnswer, updateMockTestDto.SelectedOption);
        }

        public async Task<bool> CompleteTestAsync(string id, MockTestResults completeMockTestDto)
        {
            var mockTest = await _mockTestRepository.GetByIdAsync(id);
            if (mockTest == null) return false;

            return await _mockTestRepository.CompleteTestAsync(id, completeMockTestDto);
        }


        public Task<IEnumerable<MockTest>> GetAllMockTestsAsync() => 
            _mockTestRepository.GetAllAsync();

        public Task<MockTest> GetMockTestByIdAsync(string id) => 
            _mockTestRepository.GetByIdAsync(id);
    }

}
