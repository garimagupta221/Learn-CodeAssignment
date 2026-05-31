# Clean Coding Principles & Checklist

* Use clear, intention-revealing names for variables, methods, classes, and objects.
* Follow standard naming rules: classes as nouns or noun phrases, and methods as verbs or verb phrases.
* Use consistent terminology by sticking to one word for each abstraction across the code.
* Keep names short but meaningful, avoiding unnecessary or extra words.
* Ensure every method or function has a single responsibility and performs only one task.
* Keep methods small and maintain a single level of abstraction.
* Write code in a top-down manner, showing high-level logic before implementation details.
* Limit method parameters to a maximum of two.
* Ensure methods either perform an action or return data, but not both.
* Remove duplicate or dead code by extracting common logic into reusable methods.
* Avoid unnecessary comments and prefer readable, self-explanatory code or refactoring.
* Replace magic numbers and hard-coded values with named constants or configuration values (if a hard-coded value is absolutely required, proper comments must be available).
* Follow SOLID principles:
  * SRP (one responsibility),
  * OCP (extend without modifying existing code),
  * LSP (subclasses should be replaceable),
  * ISP (no unused method dependencies),
  * DIP (depend on abstractions, not concrete implementations).
* Ensure business logic is correct, clear, and easy to understand.
* Properly handle edge cases and validate inputs where necessary.
* Implement proper, meaningful exception handling and avoid silent errors.
* Avoid unnecessary database or external API calls to improve performance.
* Keep code clean, readable, and well-formatted, following project and .NET standards.
* Ensure commit messages are clear and meaningful.
* Ensure no unnecessary files are included in the Pull Request (PR).