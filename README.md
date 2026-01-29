# Course Management System

## Overview
This project implements a Course Management System with a generic repository pattern, unit of work, and a service layer. It includes a comprehensive NUnit test suite and a Console Application for interaction.

## Business Rules (BR)
The system enforces the following business rules:

### Department
- **BR01**: Department name must be unique.
- **BR02**: Department name cannot be empty or shorter than 3 characters.
- **BR03**: Cannot delete a department if it has students.
- **BR04**: Cannot delete a department if it has courses.

### Student
- **BR05**: StudentCode must be unique.
- **BR06**: Student must belong to exactly one department.
- **BR07**: Student full name cannot be empty.
- **BR08**: Student full name cannot be shorter than 3 characters.
- **BR09**: Student email must be unique.
- **BR10**: Cannot delete a student if they have enrollments.

### Course
- **BR11**: CourseCode must be unique.
- **BR12**: Course must belong to exactly one department.
- **BR13**: Credits must be between 1 and 6.
- **BR14**: Cannot delete a course if it has enrollments.
- **BR15**: Cannot update a course if it is inactive (Credits = 0).

### Enrollment & Grades
- **BR16**: Student cannot enroll in the same course twice.
- **BR17**: Student cannot enroll in more than 5 courses.
- **BR18**: Enrollment date cannot be in the past.
- **BR19**: Student can only enroll in courses from their own department.
- **BR20**: Student and Course must exist to enroll.
- **BR21**: Cannot assign grade if student is not enrolled.
- **BR22**: Grade must be between 0 and 10.
- **BR23**: Cannot update a grade if it is finalized (`IsFinalized = true`).
- **BR24**: If enrollment fails at any step, no data is persisted (Transaction rollback).
- **BR25**: Service returns a Failure Result object instead of throwing exceptions.

## Testing
The `Service1.Test` project uses **NUnit** and **EF Core InMemory Database** to verify all business rules.
Each test case runs in an isolated context.

### Running Tests
To run the automated tests, execute the following command in the solution root:
```powershell
dotnet test d:\Netcore\Cousre\Service1.Test\Service1.Test.csproj
```

## Console Application
The Console Application (`PRN222.CourseManagement.ConsoleApp`) demonstrates the functionality using the Service Layer and Dependency Injection.

### Running the App
To run the console application:
```powershell
dotnet run --project d:\Netcore\Cousre\CourseManagement\CourseManagement.csproj
```
