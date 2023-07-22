## Commits

### Template
```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### What is What

__type:__ Represents the type of change introduced by the commit. It is typically one of the following values:

- __feat:__ A new feature.

- __fix:__ A bug fix.

- __chore:__ Routine tasks, maintenance, or tooling changes.

- __docs:__ Documentation-related changes.

- __style:__ Code style and formatting changes (e.g., whitespace, indentation).

- __refactor:__ Code refactoring, no functional changes.

- __test:__ Adding or updating tests.

- __perf:__ Performance-related changes.

- __revert:__ Reverting a previous commit.

__[optional scope]:__ An optional field to specify the scope of the change (e.g., a specific module or component).

__description:__ A concise summary of the changes made in the commit. Use the imperative mood, such as "Fix bug" instead of "Fixed bug" or "Fixes bug."

__[optional body]:__ A more detailed description of the changes, if necessary. This is helpful when the commit requires additional context or explanation.

__[optional footer(s)]:__ Additional information, such as referencing issue numbers, breaking changes, or related commits.

### Example
```
feat(auth): Add user authentication functionality

- Implemented user login and registration.
- Added JWT token-based authentication.

Closes #123
```
