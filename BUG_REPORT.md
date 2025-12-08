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
-   Update `StudentViewModel` (or create a specific `EditStudentViewModel`) to include `ClassId` (int) instead of just the class name.
-   Bind the `InputSelect` directly to this `ClassId`.
-   Add `[Required]` validation to the `ClassId`.
-   Ensure the form submission handles validation errors and provides feedback.

## 2. Silent Failure in Student Creation Service
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
If `model.Class` (which is a string) contains a value that is not a valid integer (e.g., if the ViewModel was populated with a class Name instead of ID, or if it is empty), the method silently returns. The student is not created, and no exception is thrown to alert the caller.

**Recommendation:**
-   Throw an `ArgumentException` or a custom exception if parsing fails.
-   Ideally, refactor the method to accept `int classId` directly from the controller/component, or ensure the ViewModel guarantees a valid ID.

## 3. Broken "Send Message" Feature for Teachers
**Severity:** Major
**Location:** `SchoolManagementSystem.Web/Components/Pages/TeachersList.razor` and `Messages.razor`

**Description:**
The "Send Message" button in the Teacher list redirects to `messages?composeTo={TeacherId}`. However, the `Messages` component does not correctly handle this parameter:
1.  It accepts the parameter but only switches the tab to "Compose".
2.  It does not pre-select the recipient.
3.  **Fundamental Mismatch:** The messaging system identifies recipients by **Email**, but the link provides an **Integer ID**. There is currently no logic in the frontend to resolve a Teacher ID to an Email Address.
4.  **Hardcoded Recipients:** The recipient dropdown in `Messages.razor` is populated with hardcoded dummy data, making it impossible to select actual teachers dynamically.

**Recommendation:**
-   Update `SchoolService` or `TeacherService` to allow looking up a Teacher's email (or User ID) by their Teacher ID.
-   Update `Messages.razor` to resolve the `ComposeTo` ID to an email and pre-select it.
-   Implement a real user/recipient search or fetch logic instead of hardcoded values.

## 4. Potential Data Integrity Issue in Teacher Deletion
**Severity:** Moderate
**Location:** `SchoolManagementSystem.Web/Services/TeacherService.cs`

**Description:**
`DeleteTeacherAsync` removes a teacher entity without explicitly handling related `Subject` entities. While the database likely sets the `TeacherId` foreign key to NULL (orphaning the subjects), this behavior is implicit.

**Recommendation:**
-   Verify the desired business logic: Should subjects be unassigned, or should deletion be blocked if subjects are assigned?
-   Implement a check or warning in the UI before deletion if the teacher has active subjects.
