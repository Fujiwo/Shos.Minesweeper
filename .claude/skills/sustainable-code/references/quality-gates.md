# Quality Gates (Review, Conventions, Contracts, Verification, Performance, Debt)

Read this during implementation and before completion, when doing a self-review or code review, when writing argument validation or preconditions, when asked to improve performance, and when reporting a change.

## Two Axes of Quality Control [9.intro]

- Definition: The pillars of quality control are detecting problems early and keeping the code in a state that is easy to understand and easy to change. The goal is to maintain this sustainable state.
- Early detection works because the magnitude of failure is proportional to the dwell time of an error (the formula is in refactoring.md). Self-review, contracts, and builds and tests are the meshes of a net that catches errors; the finer the mesh, the shorter the dwell time.
- Detecting an error is meaningless if the code is hard to fix. That is why conventions and debt management keep the code easy to understand and easy to change.

## Self-Review [9.1]

### Self-Review at Every Step [9.1.1]
- Definition: Review is not a one-time gate performed all at once after implementation is finished; it is continuous feedback. Design and implementation should always be under review, and this is one of the four pillars of the Saibara Principle (simplicity.md).
- When: Every time you finish a meaningful step (adding one method, renaming, extracting, etc.). Before the completion report.
- Do: For each small diff, review your own change with the Seven Articles questions below. Do not save the review for the end.
- Check: Before moving on to the next step, can you explain the previous diff in the terms of the Seven Articles?

### Use the Seven Articles as Review Criteria [9.1.3]
- Definition: The Seven Articles function both as personal guidelines and, as they are, as shared review criteria. Pointing things out in the terms of the Seven Articles, rather than with a vague impression such as "it bothered me when I read it", makes the findings concrete and easier to act on.
- Do: First, state the intent of the review target (PR, diff, code) in a word. If you cannot, confirm it with the user or the PR description (good and bad reside in how the intent and the code correspond; modeling.md). Then, whether in a self-review or in a review of the user's code, examine it with the following questions. Attach the name of the relevant article to each finding and pair it with an improvement proposal. Do not write appeals to mindset.

| Article | Review question |
|---|---|
| Express Intent | Does the What come before the How? Is description other than the intent (noise) burying the intent? |
| Single Responsibility | Is this responsibility closed into one? How many reasons to change are there? |
| Precise Naming | Does this name represent the boundary neither too much nor too little? Do the same things have the same name? |
| Once And Only Once | Is a description of the same intent present in two or more places? |
| Precisely Written Methods | Is the level of abstraction consistent within one method? |
| Consistent Rules | Does it match the style and conventions of the existing code? |
| Testable | Is it in a form where correctness can be checked? Are the preconditions made explicit? |

- When you can point something out by the name of a smell, use the names of smells and techniques (refactoring.md).

### Pre-Commit Check (Pre-Completion Check)

Run these in this order at every step and before the completion report. Commit only when the user has permitted or requested it (Restraint in the Scope of Change in SKILL.md).

1. Self-review: the Seven Articles questions above and the Self-Check in SKILL.md (including subtraction)
2. Formatter and linter: if the repository has configuration for them, run them and follow the results
3. Build: confirm that it passes
4. Test: at every step run the related tests, and before the completion report run all tests, and confirm Green

## Coding Conventions [9.2, 10.1]

### Use Conventions Only to Contain Complexity [9.2.1]
- Definition: Coding conventions have only one purpose: to contain complexity. What is hard in programming is that things become complex, and that is the greatest enemy of maintainability. Use conventions not as a tool for unifying stylistic preferences but as a tool for not increasing complexity.
- When: When proposing a convention, when unsure whether to follow a convention, when pointing out a convention violation.
- Do: If the repository has an existing formatter, linter, or conventions, run them and follow the results (for the order of precedence, see SKILL.md). When you propose convention items yourself, limit them to items that contain complexity.
- Check: Can you say in a word which complexity that convention item contains?
- Exceptions/cautions: For how to treat numeric thresholds, follow judgment rule 10 in SKILL.md; for the principle of not bypassing the form without trying it, follow the Order of Precedence in SKILL.md.

### Prohibitions When Proposing Conventions [9.2.3]

When conventions become ends in themselves, they do harm instead. Do not propose conventions or practices of the following forms.

| What not to propose | Reason |
|---|---|
| Fixing the structure and having people write only the contents (all classes auto-generated, an application required to create a class, all classes managed in a spreadsheet) | Following the convention itself has become the goal, losing sight of the purpose of containing complexity |
| Practices that leave huge methods alone | They do not contain complexity |
| Conventions tailored to the least skilled person | They hold back the people producing much of the code and do not achieve overall optimization |
| Prohibiting mechanisms that avoid complexity, such as declarative collection operations | The cause of reduced maintainability is not resolved. It confuses simple with easy (simplicity.md) |
| Trivial style rules such as the left/right order of comparisons or forcing braces around single statements | They stray from the main point of containing complexity. The main point is concrete items such as "keep nesting shallow" and "reduce branching" |

- Conventions should be scaffolding for mastering mechanisms that avoid complexity.
- Run conventions through the cycle: decide (minimal items and exception conditions) → automate (formatter, static analysis) → observe → update (review items that have become dead letters and keep the conventions thin). [9.2.4]

## Contracts and Guard Clauses [9.3]

### A Contract Is Accountability in Executable Form [9.3.1]
- Definition: Before being a technique for robustness, contract programming is accountability in executable form. It is the act of declaring, in the code itself, "what this method expects and what it guarantees". When names and visibility are decided from the client's viewpoint, a contract is established between objects (object-design.md). What verifies that contract is tests (testing.md).
- The three-part set [9.3.2]:

| Condition | Content |
|---|---|
| Precondition | A condition the caller must satisfy. If it is broken, the method bears no responsibility for executing its processing |
| Postcondition | The result or state guaranteed to the caller after execution |
| Invariant | A condition that must always hold throughout the lifetime of the object |

- Do: Establish invariants in the constructor, and keep them from breaking after methods that change state. After important state changes, express the postcondition with an assertion [9.3.2].

### Guard Clauses [9.3.2, 9.3.3]
- Definition: A way of writing in which preconditions are explicitly checked at the start of a method, and processing is aborted immediately if they are not satisfied.
- When: When a public method or constructor has assumptions about its arguments or state. When a bug fix reveals that the cause is a broken precondition.
- Do: Verify preconditions with guard clauses at the entry, and make the main processing a flat, single path with no error cases. For verification, prefer the language's standard validation APIs, assertions, and the type system (non-nullable types, dedicated types, etc.); do not build your own validation mechanism.
- Check: Are there no error-case branches left in the main processing? Can the preconditions be read from the code?
- Three benefits of guard clauses:
  1. They eliminate error cases at the entry and keep the main processing simple. They implement, at the code level, "do not put yourself in a situation where you have to handle complex problems"
  2. Early detection of bugs. They detect contract violations on the spot, shrinking the dwell time of errors to nearly zero
  3. They are easy to check mechanically as convention items and review criteria, such as "place argument validation at the start with guard clauses" and "are the preconditions made explicit?"
- Exceptions/cautions:
  - A guard clause detects a broken precondition; it is not a fix for the caller that is breaking it (for how to proceed with bug fixes, see "Bug Fixes" below).
  - Examples of standard language validation APIs: C#'s `ArgumentNullException.ThrowIfNull`, Java's `Objects.requireNonNull`, Python's `raise ValueError`.
  - If the repository's conventions prohibit early returns, follow the conventions, and if you judge that complexity increases, only point it out (the Order of Precedence in SKILL.md). If the conventions prohibit only return, guard clauses that throw exceptions may be used. If they prohibit early exits in general, or you cannot tell, gather the validation result into a variable and use a single return at the end (the second example below), and report how you interpreted it. If the existing contract is in a form that cannot return an error via the return value (void, etc.), confirm with the user without changing the type. Do not silently discard errors.

```csharp
public void Withdraw(decimal amount) {
    // Guard clause: verify preconditions at the entry and abort if not satisfied
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
    if (amount > balance) throw new InvalidOperationException("Insufficient balance");
    var before = balance;
    balance -= amount;                          // The main processing is a flat, single path
    Debug.Assert(balance == before - amount);   // Postcondition
}

// When conventions prohibit early exits (including via exceptions): gather the validation result and use a single return at the end
public Result Register(string? name, int age) {
    var error = name is null or ""      ? "Name is empty"
              : age is < 0 or > 150     ? "Age is out of range"
              : null;
    Result result;
    if (error is null) result = Result.Ok(repository.Add(name!, age)); // The main processing is in one place
    else               result = Result.Fail(error);
    return result;
}
```

### Bug Fixes [6.1, 7.1, 9.3]
- Definition: A bug is a gap between the specification and the current state. Make the gap visible, then fix it.
- When: When asked to fix a defect.
- Do:
  1. If there is test infrastructure, first write a test that reproduces the defect and confirm Red (if there is no infrastructure, see the Alternatives When Something Cannot Be Done in SKILL.md).
     - When the expected value is not determined by the specification, do not decide the expected value by guessing. Put the reproduction test on hold with the reason stated explicitly (e.g. xUnit's `Skip`), report the reproduction steps and what you observed, and confirm the specification. Once the expected value is decided, write it in, remove the hold, confirm Red, and then fix.
     - Do not assert the error currently occurring (e.g. NullReferenceException) as the expected value. It would be Green from the start and would lock the bug in as the specification.
  2. Identify the cause (a caller breaking a precondition, a wrong boundary, etc.), fix it with the smallest change, and make it Green. Do not end with a change that merely suppresses the symptom (catching and ignoring an exception, silently replacing null with a default value).
  3. If the cause is a broken precondition, make the precondition explicit at the entry with a guard clause or a type. However, that is detection of recurrence, and is done separately from fixing the cause.
  4. Do not fix the surrounding smells; report them (Restraint in the Scope of Change in SKILL.md).
- Check: Did the reproduction test go from Red to Green? Are all existing tests Green?
- Exceptions/cautions: Do not mix bug fixes and refactoring. If structural improvement is needed, separate it from the fix as a distinct step (refactoring.md).

## Always Keep It Buildable and Runnable [9.4]

### Build → Static Analysis → Test on Every Change [9.4.1, 9.4.2]
- Definition: Compilers, static code analysis, and unit testing tools are all feedback tools. By connecting them into a single flow and running them constantly, keep the code always buildable and always testable.
- When: At every step.
- Do: On every change, run build → static analysis → test locally. Keep the program in a launchable state as much as possible, and add features incrementally. Do not proceed in a way where it cannot be launched until it is close to complete.
- Check: Do the build and tests pass right now? Are you not moving on to the next step in a broken state?
- Exceptions/cautions: Use the tools available in the repository (type checkers, analyzers, test runners). Mastering good development tools is also part of quality control. Introducing a new tool is an out-of-scope change, so confirm with the user.

## Technical Debt and Performance [9.5]

### Keep the Dwell Time of Debt Short [9.5.1]
- Definition: The formula for the magnitude of failure applies to technical debt as is. Debt whose repayment is postponed swells like interest. Managing technical debt is the activity of keeping the dwell time of debt short.
- Do: Do not leave debt alone. Repay debt found within the change target on the spot (for the conditions for preparatory refactoring, see SKILL.md). Do not fix debt noticed outside the scope; report it with the name of the smell and its location. When reporting out-of-scope debt, add a word on why it will swell if left alone (frequency of change, extent of ripple effects), so that the priority of repayment can be judged [9.5.1].

### Three Principles for Performance Decisions [9.5.2]
- Definition: Performance is a subject of judgment to be measured and agreed on, not guessed. Its priority is relatively low, but that does not mean it can be ignored.
  1. Write code that works correctly first
  2. Measure before optimizing
  3. Choose optimizations that do not sacrifice readability
- When: When asked "it's slow" or "make it faster". When you feel like changing how something is written for performance reasons.
- Do: While keeping readability and maintainability high, identify the bottleneck by measurement and optimize only that spot locally. Once it is resolved, measure again and look for the next bottleneck. 20% of the code accounts for 80% of the execution time (80:20), so focus optimization on the bottleneck.
- Check: Can you show measurements from before and after the optimization? Is the optimized spot the one identified by measurement?
- Exceptions/cautions: Worrying too much about execution speed makes callers depend on the internal implementation of subroutines and breaks the wall of abstraction. Before optimizing, check whether there are tests pinning down the behavior (if not, see "Adding Tests to Existing Code After the Fact" in testing.md). Even without a profiler, do the measurement you can, such as timing a section (do not leave temporary measurement code behind). In an environment where even that is impossible, propose a measurement method (the path to measure, the data volume, the means). Do not make optimizations that reduce readability based on guesses alone. When you have no choice but to do so, state explicitly that it is a guess and confirm with the user.

### Record Trade-offs and Get the User's Agreement [9.5.3]
- Definition: When you deliberately choose an optimization that sacrifices readability, leave the reason for the judgment in a comment and get agreement. Performance trade-offs are a concrete example of Why comments.
- Do: Present an optimization that reduces readability as a proposal (measurements and trade-offs), and implement it only after getting the user's agreement. Even when explicitly asked to improve performance, show each spot that reduces readability individually in the report (location, reason, measurements). Include the measurements in the Why comment.

```csharp
// Why: With the declarative chain, this path performed three traversals, and measurement showed
// it took three times the allowed time on production-equivalent data. Readability decreases, but
// we change it to a single loop to speed up only this path.
var total = 0m;
foreach (var item in items)
    if (item.IsActive) total += item.Price * item.Quantity;
```

## The Three Debts and AI Output [9.6]

### The Three Debts [9.6.1]
- Definition: In an era when AI generates code at high speed, debt accumulates not only in the code itself but also in people's heads.

| Debt | Where it accumulates | Content | The agent's countermeasure |
|---|---|---|---|
| Technical debt | The code itself, the structure of the system | Code that works but is inconsistent and hard to maintain is produced in large quantities at AI speed | Self-review at every step (the Seven Articles, subtraction) |
| Comprehension debt | The heads of developers and teams | If you keep adopting generated code without sufficiently understanding it because "it worked, so it's OK", it piles up even faster at the team level (definition in foundations.md) | Explain the What and Why of the change, and keep diffs to a size the reader can understand |
| Intent debt | Externalized knowledge (records of specifications and design decisions) | The background of "why it was written with this design" is cut away, so the code can be read but decisions about changing it cannot be made | Leave the Why in comments, and the design decisions (the option chosen, the options passed over, the reasons) in the report |

### Do Not Confuse AI Output with the Specification [9.6.2]
- Definition: The speed of writing code is no longer the bottleneck; the process of understanding and verifying the large volume of generated code has become the new bottleneck. AI output is incorporated into the system only once a human has verified it.
- Do: Do not treat your own output as the specification (Principle of Action 5 in SKILL.md). For how to hand over diffs, follow "Preventing Wholesale Delegation" in collaboration.md.
- Check: Is the diff a size that can be read at once? Did you show what you verified (the tests, build, and static analysis you ran)?

- For how to treat the economics of abstraction (even if the effort of writing disappears, the cost of reading does not go down), follow judgment rule 2 in SKILL.md. [9.6.4]
