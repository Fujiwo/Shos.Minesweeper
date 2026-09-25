# Testing (Testable, TDD, Test Case Design)

Read this when writing, adding, or reviewing tests, when designing a structure that is easy to test, or when proceeding with implementation through TDD. The details of maximizing feedback and of the test side of contracts are here (contracts and guard clauses themselves are in quality-gates.md).

## Testable and Maximizing Feedback

### Testable [1.2.7]
- Definition: Being structured so that you can tell the code is written correctly. What a Testable design aims for is **maximizing feedback**. The earlier and more frequently you get feedback, the more efficiently you can develop high-quality software.
- When: When writing a new class or method. When you try to write a test and the setup feels heavy.
- Do: Inject dependencies (repositories, external services, the clock, the file system) through the constructor or similar. Extract decisions and calculations into pure parts that do not receive dependencies. Isolate external I/O in a thin layer.
- Check: Can you write tests without a DB, network, or the current time, just by plugging in test doubles?
- Exceptions/cautions: Reading generated code line by line to the end is not realistic in terms of volume. Only a structure that can be verified quickly, repeatedly, and automatically turns generation speed into quality. Writing large amounts of non-Testable code is the same as piling up debt that cannot be verified. Creating a substitution point is not a YAGNI violation (judgment rule 3 in SKILL.md).

```csharp
// before: depends directly on the DB and the current time, and cannot be verified on its own
public void ProcessOrder(int orderId) {
    using var connection = new SqlConnection("...");
    var now = DateTime.Now;   // validation, calculation, and sending are also concentrated in the same method
}
// after: repository, email, and clock are injected via the constructor, and decisions are extracted into a pure part
public OrderProcessingResult ProcessOrder(int orderId) {
    var order = repository.FindById(orderId);
    return ValidateAndProcess(order, processedAt: clock.Now);   // sending the email on success is omitted
}
```

### Kinds of Feedback [1.2.7]

Use them in order, fastest first. Where only slow feedback is available, move toward a form that yields fast feedback.

| Kind | What it detects | Agent's action |
|---|---|---|
| Compile time | Misuse of types | Use dedicated types for values that represent concepts (below) |
| Unit tests | Divergence from the specification | Replace dependencies with test doubles and write in AAA |
| Integration tests | Defects in the whole workflow with the parts connected | Verify the whole workflow with the parts connected. Because they are slower than unit tests, limit them to checking the joints and the workflow |
| Continuous feedback | Divergence at each change | Run build, static analysis, and tests on every change (quality-gates.md) |

### Get Compile-Time Feedback with Dedicated Types [1.2.7]
- Definition: Detect errors at compile time through type safety. This is the fastest feedback.
- When: When you are about to make a concept such as an ID, amount, or quantity a parameter or attribute as a plain int or string.
- Do: Use a dedicated type such as `OrderId` instead of int for parameters, so that mixing them up becomes a compile error.
- Check: Does compilation fail if you swap the order of the arguments?
- Exceptions/cautions: If you find the same smell in existing code, treat it as "Primitive Obsession" (refactoring.md).

```csharp
// before: compiles even if the arguments are mixed up
public Order(int id, int customerId) { ... }
new Order(customerId, orderId);   // you cannot notice until you run it
// after: mixing them up becomes a compile error
public Order(OrderId id, CustomerId customerId) { ... }
```

### Shape of a Test: AAA and Test Doubles [1.2.7, 6.3.2]
- Lay out a test in three stages: Arrange, Act, and Assert. If the subject is small it may fit in two lines, but keep the same order.
- For dependencies to be substituted, use test doubles that are simple implementations for testing. Examples: a Fake repository that returns fixed data, a Spy email sender that records the destination, a clock provider that returns a fixed time.
- Isolating external I/O (DB, API, file system) makes tests fast and stable.

## Test-First

### Tests Are the First Client [6.1.1]
- Definition: Test-first is a mechanism for settling the What (what you want to do) before the How (how to achieve it). If naming is the technique of expressing the What with names, test-first is the technique of expressing the What with code. Test code is the **first client** of the class you are about to build.
- When: When creating a new class, method, or API.
- Do: Before the implementation, write the code on the side that uses it. Attach assertions to that "example of the using side" and keep it as an asset as is.
- Check: At the point of writing the test, is the call natural? An API that is hard to use reveals its awkwardness here. If it does, fix the name and shape before implementing (SON in naming.md).
- Exceptions/cautions: In test-first, you do not assume a testable shape in advance and design for it; rather, writing the test first leads the design into a testable shape. The "Testable" structure above (injecting dependencies, extracting pure parts) is the same shape seen from the side of the result, and the two do not contradict each other.

### Tests Are an Executable Design Document [6.1.2]
- Definition: Test code is an "executable design document". Nobody notices when a written design document diverges from the implementation, but a test fails the moment the implementation diverges from the specification. The divergence between the specification and the current state: that is a bug.
- When: When you want to settle the specification. When you cannot know the whole specification from the start.
- Do: Think "if it passes the tests at that point, it matches the specification at that point", and settle the specification step by step while adding test cases one at a time. Do not try to solve everything at once.
- Check: Can you understand the specification just by reading the list of test names?
- Exceptions/cautions: Even if you write preconditions and postconditions in comments, they stay silent while diverging from the implementation. A test turns red the moment it diverges [6.5.1].

### Red, Green, Refactor and Switching Hats [6.1.2]

| Beat | What to do | Hat and concern |
|---|---|---|
| Red | Write a failing test and confirm that it fails. The failure makes "the divergence between the specification and the current state" visible | The client's hat. Forget the internals and think only about what way of calling it would make it easy to use |
| Green | Write the minimal implementation that passes the test. Do not care about cleanliness; get to Green by the shortest path | The implementer's hat |
| Refactor | Improve the internal structure using the state where all tests pass as a safety net. Do not change the tests | Still the implementer, but the concern shifts from "working" to "internal quality" |

- One cycle is a few minutes. Grow the code incrementally, never straying far from the "all Green" state.
- Consciously go back and forth, each cycle, between the viewpoint that decides the specification and the viewpoint that builds the code.

## How to Proceed with TDD (Worked Example) [6.2]

Situation: Build from scratch, test-first, a type `Date` that answers whether a year, month, and day are valid as a date (writing it with the Japanese identifiers `日付` and `日付として正しい` makes the intent stand out even more).

1. **Translate the specification into pairs of inputs and expected values.** "Handle leap years correctly" leaves room for interpretation, but "2100/02/29 is invalid" leaves none. The table is the 15 cases in "Designing Test Cases" below.
2. **Turn them into code one at a time.** Do not write all 15 at once.
3. **Always see Red.** Failing to compile because the type does not exist yet is also Red, and it is the fastest feedback. If a test is Green from the start before you have implemented anything, that test verifies nothing. Seeing Red once verifies that the test itself is working.
4. **Get to Green by the shortest path with Fake It (a provisional implementation).** Return a constant, as in `IsValid => true`. You can get back to Green in seconds, and it establishes that the way the test is written, the execution environment, and the wiring are correct. This lie will be exposed by the next test, so it is not left in place.
5. **Generalize by triangulation.** Adding "month 13 is invalid" means it can no longer be a constant, and it becomes `Month <= 12`. With one point you can cheat with a constant, but with two points you cannot. Each time you write one test, widen the implementation only minimally. An implementation that jumps ahead because "it will be needed later anyway" has no test verifying it.
6. **Postpone the hard parts.** For leap years, the most complex part, first proceed with the provisional implementation "February goes up to the 28th". The provisional implementation survives only because there is not yet a test that exposes it. The three cases 2008, 2100, and 2000 drive the condition, one step at a time, to "a multiple of 4, except multiples of 100, but including multiples of 400".
7. **Keep tests that pass from the start too.** "February 30 in a leap year is invalid" does not turn Red when added, but it has value as a sentinel that keeps protecting the boundary (the "see Red" in step 3 is a rule for tests before the implementation satisfies them).
8. **In Refactor, run the tests after every step.** Split a 20-line block into properties named for each meaningful unit (`IsYearValid`, `LastDayOfMonth`, `IsLeapYear`, and so on), confirming Green each time.

- Reason: Because the tests settle the What first, the implementation is easily organized into a form that speaks the What. Because the validation logic was written as pure expressions without side effects and given no external dependencies, the tests ran nimbly without any mock setup.

## Designing Test Cases [6.3]

### Representative Values, Boundary Values, Edge Cases [6.3.1]

| Category | Approach | Date examples (input → expected) |
|---|---|---|
| Representative values | One ordinary value at the center of the specification | 2007/02/14 → valid |
| Boundary values | One case on each side of each "border" in the specification. Mistakes between `<` and `<=` are revealed only exactly on both sides of the boundary | year 0, month 0, month 13, day 0, day 32 → invalid / January 31 → valid / April 31 → invalid / April 30 → valid |
| Edge cases | Special values where the specification's exception rules overlap. For each level of exception rule, at least one value where that rule applies | common year 2007/02/28 → valid, 2007/02/29 → invalid / multiple of 4 2008/02/29 → valid, 2008/02/30 → invalid / multiple of 100 2100/02/29 → invalid / multiple of 400 2000/02/29 → valid |

- The idea of boundary values appears nested even within edge cases (February 29 and 30 in a leap year).
- Designing the test cases becomes, as is, designing how the implementation grows (the three cases for the three levels of exception rules grew the condition one step at a time).
- You do not need to have all cases ready at the start. If you notice a gap along the way, add one row to the table at that point. The list of test cases is a specification that you grow.

### Let Test Names and Assertions Tell the Specification [6.3.2]
- Make a test name **a sentence of the specification that includes the input situation and the expected result** (e.g. `April31IsInvalid`, "February 29 in a year that is a multiple of 400 is valid"). With `Test1` or `MonthTest`, when it fails you cannot tell which part of the specification was broken.
- Verify only one concern in one test. Do not mix leap-year verification into "month 13 is invalid". A test that mixes concerns cannot tell which part of the specification was broken when it fails.
- The test name tells the specification, and the assertion settles the expected value. Only when both are in place does it become a specification document that stays resident and keeps being executed.

## Testing and Refactoring [6.4]

### Safety Net [6.4.1]
- Definition: Because there are tests, you can refactor. Because you refactor, the code is kept clean. Because the code is clean, tests are easy to write too. The starting point of this virtuous cycle is the safety net called tests. The true purpose of TDD is not to find bugs but **not to drive yourself into a situation so full of bugs that you cannot touch it**.
- When: When you are about to change the structure. When you encounter untested "code that works but that nobody can touch" (a typical source of comprehension debt).
- Do: Before a structural change, confirm that tests covering the contract are in place. Return to Green every few minutes, and detect and fix divergence while it is small.
- Check: Are the relevant tests Green after every step, and all tests Green before reporting completion?
- Exceptions/cautions: The later a divergence is found, the more expensive the fix (magnitude of failure = error × dwell time; refactoring.md). Not heavily reworking untested code follows "Restraint in the Scope of Change" in SKILL.md.

### Three-Way Critique [6.4.2]
- Definition: Implementation is a critique of the design, and tests are a critique of the implementation. Feedback is an accurate critique of what you have made, and the earlier and the more of it, the better.
- Test → implementation: reveals shortcomings in the implementation. Implementation → design: reveals the awkwardness of an API or the inappropriateness of a name. Test → design: guarantees safety when you change the design.
- Do: Set up the fastest critic (the tests) first. If during implementation you notice something off about an API or a name, treat it as feedback on the design and fix it.

## Conditions of Good Test Code [6.5]

### Test Against the Contract [6.5.1]
- Definition: Test against the contract, not against implementation details. This is the condition for tests that withstand refactoring. A test is an executable description of the contract. A good test suite is designed not only for what it verifies but also for **what it does not verify**.
- When: When deciding what to test. When you feel like directly testing private or internal helper methods.
- Do: Verify only public behavior (promises to the outside). Verify internal helper methods indirectly through public behavior.
- Check: Even if you rewrite the internals from a mass of ifs to a switch, can you get by without changing a single line of the tests?
- Exceptions/cautions: Tests tied to the internal structure must be rewritten every time the structure changes, and rather than being a safety net, they become a shackle on refactoring. In the date example, the internal `IsLeapYear` did not exist until the refactoring stage.

### Coverage and Metrics Hacking [1.2.7, 6.5.2]
- Definition: Coverage measures "whether the tests passed through the code", not "whether the tests verified correctness". Even if you delete all the assertions, coverage stays at 100%. The moment a number becomes the goal, only the number is achieved (metrics hacking).
- When: When asked to "increase coverage". When generating large numbers of tests.
- Do: Do not write tests without assertions. Use coverage as a means of finding places the tests do not reach, and review low-coverage areas using the categories of representative values, boundary values, and edge cases.
- Check: Does each test have an assertion that settles the expected value? Does it fail if you deliberately break the implementation?
- Exceptions/cautions: Coverage is not meaningless. Do not confuse the means with the goal. Even when asked for a coverage target, report what you verified along with the number.

### Agree with the User on What to Verify [6.5.2]
- Definition: AI is good at generating test code, but the work of making the specification table, that is, **the work of deciding what should be verified, stays with humans**. The ability to decide the content of verification is the core of the technique.
- When: When designing test cases. When the specification is given only in prose.
- Do: Show the user, as a table of inputs and expected values, where the boundaries are, how many levels of exception rules there are, and what is promised as the contract. For rows where interpretations diverge (whether a boundary is inclusive or exclusive, how exception rules are handled), confirm before turning them into tests.
- Check: Does every row of the table correspond to the specification the user intended?
- Exceptions/cautions: Mass-producing tests while leaving this ambiguous becomes metrics hacking with a plausible shape. For small changes where interpretations do not diverge, do not ask for confirmation; just include the table in the report (Principle of Action 3 in SKILL.md).

## Adding Tests to Existing Code After the Fact [6.4.1, 6.5]

- When: Before refactoring untested code (to set up a safety net). When asked to add tests to an existing class.
- Do:
  1. For the public behavior (the contract), make a table of representative values, boundary values, and edge cases, and write tests that pin down the current behavior.
  2. Because the implementation already exists, the tests are Green from the start. Instead of seeing Red, deliberately break the implementation to confirm that the tests fail, and revert the breaking change.
  3. For rows where the current behavior does not seem to be the specification (looks like a bug), handle them differently depending on the purpose of adding the tests.
     - When setting it up as a safety net for refactoring: since the purpose is to preserve behavior, pin down that row as it currently is too, and report it as a "suspected bug". Whether to fix it is decided in a procedure separate from the refactoring.
     - When adding tests to verify the specification: do not turn it into a test with the expected value matched to the current state. Report that row to the user and handle it after confirming the specification.
  4. If a substitution point for dependencies is needed in order to write tests, that is a structural change in an untested state. Confirm with the user before creating the substitution point.
- Check: Do the added tests pin down each row of the contract, and do they fail when the implementation is broken?

## When There Is No Test Infrastructure

Details of "Alternatives When Something Cannot Be Done" in SKILL.md.

1. **Do not introduce a test framework on your own.** Introducing one is a change outside the scope of the request, adding a mechanism nobody asked for. Confirm with the user whether to introduce one, together with candidates (the language's standard options, how to incorporate it into the existing build procedure). However, using the language's standard test mechanism (one that requires no new dependencies or configuration) does not count as introducing one. In that case too, report the tests you added and how to run them.
2. **Until you have confirmation, proceed with the smallest verification means available in the existing environment.**
   - Types: Use dedicated types so that mixing values up becomes a compile error.
   - Assertions and guard clauses: Check preconditions at run time (quality-gates.md).
   - Confirmation by execution: Actually run the inputs from the test case table above and compare them with the expected values. Do not leave temporary verification code in the repository.
3. **Report which verification you did.** Show which rows of the table you confirmed by which means, and which rows remain unconfirmed.
4. Even for a request that does not want tests, keep a structure that is easy to test (injecting dependencies, extracting pure parts).

## Test Self-Check

- [ ] Are the tests set against only public behavior (the contract)?
- [ ] Does each test have an assertion, and does it fail when the implementation is broken?
- [ ] Is each test name a sentence of the specification that includes the input situation and the expected result?
- [ ] Is it in AAA order, with one concern per test?
- [ ] Are representative values, boundary values (both sides), and edge cases (at least one per level of exception rule) all in place?
- [ ] Did you see each new test's Red once (for sentinel tests that pass from the start or tests added after the fact, did you confirm that they fail when the implementation is deliberately broken, and revert the breaking change)?
- [ ] Can the clock, I/O, and external systems be substituted?
