# Bug Investigation Report

This report details the findings from an investigation into the `daskAL` School Management System codebase.

## 1. Silent Failure in Student Editing
**Severity:** Critical
**Location:** `SchoolManagementSystem.Web/Components/Pages/EditStudent.razor`

**Description:**
The `EditStudent` component has a flawed implementation for selecting a student's class.
1.  **Unreliable Pre-selection:** The component attempts to pre-select the class in the dropdown by matching the class **Name** (stored in `StudentViewModel`) against the list of available classes. This is fragile; if the name doesn't match exactly (or if multiple classes share a name), the dropdown defaults to "Select Class" (empty value).
2.  **Silent Submission Failure:** The `HandleValidSubmit` method attempts to parse the `selectedClassId` string to an integer.
    ```csharp
    if(int.TryParse(selectedClassId, out int classId))
    {
         await StudentService.UpdateStudentAsync(student, classId);
         NavigationManager.NavigateTo("students");
    }
    ```
    If the user does not select a class (or if the pre-selection failed), `selectedClassId` is empty. `int.TryParse` returns `false`, causing the method to exit immediately without calling the service or navigating. The user clicks "Save", and nothing happens, with no error message displayed.

**Recommendation:**
-   Update `StudentViewModel` to include `ClassId` (int).
-   Bind the `InputSelect` directly to this `ClassId`.
-   Add `[Required]` validation to the `ClassId`.
-   Ensure the form submission handles validation errors and provides feedback.

## 2. Silent Failure in Grade Creation
**Severity:** Critical
**Location:** `SchoolManagementSystem.Web/Components/Pages/Grades.razor`

**Description:**
The `Add Grade` form fails silently because of a mismatch between the ViewModel validation and the form binding.
-   `GradeViewModel` marks `SubjectName` as `[Required]`.
-   The form in `Grades.razor` binds to `SubjectId` but **does not set** `SubjectName`.
-   When the user clicks "Add", the `DataAnnotationsValidator` detects that `SubjectName` is null/empty and blocks the submission.
-   Since there is no `<ValidationMessage>` for `SubjectName` and no `<ValidationSummary>`, the user sees no error. The `OnValidSubmit` handler (`HandleAddGrade`) is never called.

**Recommendation:**
-   Remove `[Required]` from `SubjectName` in `GradeViewModel` (it is for display only), or set it to a default value.
-   Validate that `SubjectId` is selected instead.

## 3. Silent Failure in Student Creation Service
**Severity:** Major
**Location:** `SchoolManagementSystem.Web/Services/StudentService.cs`

**Description:**
The `AddStudentAsync` method overload that accepts a `StudentViewModel` performs a risky parse operation:
```csharp
public async Task AddStudentAsync(StudentViewModel model)
{
    if (int.TryParse(model.Class, out int classId))
    {
        await AddStudentAsync(model, classId);
    }
}
```
If `model.Class` (which is a string) cannot be parsed as an integer (e.g., if it contains a Name), the method silently returns. The student is not added, and no error is raised.

**Recommendation:**
-   Throw an exception if parsing fails.
-   Refactor to pass `int ClassId` explicitly.

## 4. Broken Teacher Account & Messaging System
**Severity:** Major
**Location:** `AuthService.cs`, `TeacherService.cs`, `Messages.razor`

**Description:**
1.  **Cannot Create Teacher Login:** `AuthService.RegisterAsync` hardcodes the role to "Student" for all new users. `TeacherService.AddTeacherAsync` adds a record to the `Teachers` table but does not create a User account or link the `UserId`. Thus, created teachers cannot log in.
2.  **Broken "Send Message":** The "Send Message" button in the Teacher list links to `messages?composeTo={TeacherId}`.
    -   `Messages.razor` does not map `TeacherId` to an Email/User.
    -   The recipient dropdown contains hardcoded dummy data (`admin@school.com`, etc.) and does not list actual teachers from the database.
    -   It is effectively impossible to send a message to a specific real teacher.

**Recommendation:**
-   Implement proper User creation with Role selection (Admin only).
-   Link `Teacher` entities to `User` entities (via `UserId`).
-   Update `Messages.razor` to fetch real users/teachers for the recipient list.

## 5. Incomplete Data Import
**Severity:** Moderate
**Location:** `SchoolManagementSystem.Web/Services/DataImportService.cs`

**Description:**
The import service creates Students and Teachers but **skips creating Subjects**.
-   Imported grades are created with `SubjectName` but `SubjectId` is null.
-   This leaves the system in a state where grades exist but the corresponding Subjects do not appear in management lists or dropdowns.

**Recommendation:**
-   Implement the logic to import Subjects from the legacy data and link them to Teachers.
