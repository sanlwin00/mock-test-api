# MockTestApi Unit Tests

This test project contains comprehensive unit tests for the MockTestApi backend services using xUnit, Moq, and FluentAssertions.

## Test Structure

### Services Tested

1. **UserService** (`Services/UserServiceTests.cs`)
   - User registration (happy path and error cases)
   - User retrieval by ID
   - User updates
   - Password hashing and validation
   - Email service integration

2. **MockTestService** (`Services/MockTestServiceTests.cs`)
   - Mock test creation
   - Progress tracking and updates
   - Test completion
   - Test retrieval operations

3. **AuthenticationService** (`Services/AuthenticationServiceTests.cs`)
   - JWT token generation and validation
   - Password verification
   - User authentication with credentials
   - Access code authentication
   - Admin role handling

4. **PaymentService** (`Services/PaymentServiceTests.cs`)
   - Stripe session creation (business logic validation)
   - Payment validation and authorization
   - Data structure validation

### Test Categories

#### Happy Flow Tests
- All primary service methods with valid inputs
- Successful operations and expected responses
- Integration between services and repositories

#### Invalid Flow Tests (`Services/*InvalidFlowTests.cs`)
- Null and invalid parameter handling
- Repository failures and error propagation
- Authentication failures
- Authorization violations

#### Edge Case Tests (`Services/*EdgeCaseTests.cs`)
- Boundary conditions (very long strings, extreme values)
- Special characters and encoding
- Null collections and empty data
- Concurrent operations and race conditions

## Test Helpers

### MongoTestHelper (`Helpers/MongoTestHelper.cs`)
- In-memory MongoDB setup for integration tests
- Database seeding and cleanup utilities
- Support for both in-memory and real MongoDB instances

### TestDataGenerator (`Helpers/TestDataGenerator.cs`)
- Factory methods for creating test data
- Realistic mock objects for all domain models
- Consistent test data across test suites

## Test Configuration

### Dependencies
- **xUnit**: Test framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Fluent assertion library for readable tests
- **MongoDB.Driver**: Database testing support
- **Mongo2Go**: In-memory MongoDB for isolated testing
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support

### Test Coverage Areas

#### Happy Flow Coverage
✅ User registration and authentication
✅ Mock test creation and management
✅ Progress tracking and completion
✅ Payment processing validation
✅ JWT token generation and validation

#### Error Handling Coverage
✅ Null parameter validation
✅ Database connection failures
✅ Authentication and authorization errors
✅ Service integration failures
✅ Invalid data format handling

#### Edge Case Coverage
✅ Boundary value testing
✅ Special character handling
✅ Large data processing
✅ Concurrent operation safety
✅ Performance edge cases

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --logger:"console;verbosity=detailed"

# Run specific test class
dotnet test --filter "UserServiceTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Build and run tests
dotnet build && dotnet test
```

## Test Results Summary

- **Total Test Classes**: 8
- **Total Test Methods**: 50+
- **Coverage Areas**: Services layer, authentication, payments, data validation
- **Test Types**: Unit tests with mocked dependencies
- **Assertion Style**: Fluent assertions for readability

## Notes

- Tests use mocked dependencies to ensure isolation
- Authentication service tests include JWT validation
- Payment service tests focus on business logic (Stripe integration would require additional setup)
- All tests follow AAA pattern (Arrange, Act, Assert)
- Comprehensive error handling and edge case coverage
- Tests are designed to be fast and reliable for CI/CD pipelines