# Refactoring (Smells, Techniques, Kaizen)

Read this when fixing the structure of existing code, when reviewing/self-reviewing code and listing points for improvement, and when tidying up the place for a feature before adding it. Details on the magnitude of failure, Kaizen, and speaking in the names of smells and techniques are here.

## Summary of the Process

1. Check the safety net. When there are no tests, distinguish by whether test infrastructure exists (the "Restraint in the Scope of Change" section of SKILL.md, testing.md).
   - The infrastructure exists, but the target code has no tests (any of a test project, a test framework dependency, or existing tests is present): first add tests that pin down the current behavior, then proceed (the "Adding Tests to Existing Code After the Fact" section of testing.md).
   - The infrastructure itself does not exist: ask the user whether to introduce a framework (the "When There Is No Test Infrastructure" section of testing.md). Until you get confirmation, limit yourself to small single steps that can be done with the automated refactorings of the IDE or language tools (Rename, Extract Method, etc.), and do not substantially rebuild the structure. In environments where automated refactoring cannot be used, do not change the structure by hand; only report the smells and techniques.
2. Name the smell. Fiddling with code at random is not refactoring.
3. Choose one technique that corresponds to the smell and apply just one step.
4. Run the related tests and check that they are Green. If Red, undo the step you just made. Run all tests before reporting completion.
5. Record what you changed in the names of smells and techniques. For smells that are out of scope, only report them.

## What Refactoring Is [7.1]

### Preserve Behavior, Iterate on Design [7.1.1]
- Definition: Improving the internal structure of software so that it becomes easier to understand and modify, while preserving its externally observable behavior. It is the iteration of design.
- When: When you notice a smell. When the place is not in order before putting in a requested change.
- Do: Do not change the relationship between input and output (the contract with the clients) at all; change only the internal structure. Cut steps small enough that you can get back from a Green state within a few minutes, and always keep it buildable and launchable.
- Check: Whether the related tests are Green after each step, and all tests are Green before reporting completion. Whether there is no change to the contract (the published behavior).
- Exceptions/cautions: Do not drop the two phrases of the definition. "Preserve behavior": refactoring is neither adding features nor fixing bugs. If you mix them, you cannot isolate the cause of a change in behavior. "Iteration of design": design is not a process that is done only once at the start and then finished. Touching the code is touching the design.

### The Two-Layer Safety Net [7.1.1]
- Definition: The safety net is a two-layer structure of verification by tests and the ability to go back through version control. Tests verify "whether the behavior is broken," and version control provides the means to return to the previous state after it is found to be broken.
- When: When starting a refactoring. When a step looks like it will become large.
- Do:
  - Verification: Run the related tests after each step, and all tests before reporting completion.
  - Going back: Whether to commit or branch follows the "Restraint in the Scope of Change" section of SKILL.md. If permitted, commit frequently at Green points, and do large-scale refactorings on a dedicated branch (if it does not work out, you can discard the whole branch).
- Check: Whether you can reliably return to the previous Green even if you get stuck on the next step. Refactoring without a safety net is nothing but tightrope walking.

### Magnitude of Failure = Error × Dwell Time [7.1.2]
- Definition: **Magnitude of failure = error × dwell time of the error**. The damage grows not only with the severity of the error but in proportion to how long the error remained in the code. People always make mistakes. So the game is decided not by "not making mistakes" but by "how short you can make the time you are wrong." The same holds for the degradation of design.
- When: When you notice a smell or a defect. When you think, "Don't touch it while it works; fix it all together later."
- Do: The moment you notice a smell is the moment it is cheapest to fix. If it is within the scope of the request, fix it with one step on the spot. If it is out of scope, report it on the spot by the smell's name (the report becomes the cheapest step to shorten the dwell time).
- Check: Whether you are minimizing the time until a deviation is detected (whether you are catching it with fast feedback, in the order type error → test → review; testing.md).
- Exceptions/cautions: Dealing with an unmanageable situation is not about trying hard after you have fallen into it; it is about **not driving yourself into such a situation in the first place**. Do not make things complex in the first place, always review (including showing the user intermediate results to confirm the direction), always test, and refactor—repeat these in small increments from the early stages of development (the idea of not making things complex is in simplicity.md).

## Code Smells [7.2]

### The Value of Smells Having Names [7.2.1]
- Definition: The list of code smells is a **vocabulary** that makes the degradation of code visible. The value lies not in memorizing individual items but in the fact that they have names. If "somehow hard to read" can be named as "this is a Long Method," it can be shared, and a remedy can be chosen (the value of vocabulary is in naming.md).
- Do: When listing points for improvement, first identify the smell by the names in the table below, then choose a technique. For discomfort that does not fit any name in the table, write the symptoms concretely.

| Smell | In a word |
|---|---|
| Duplicated Code | The same processing is written in multiple places |
| Long Method | Multiple concerns are crammed into one method |
| Comments | Comments are used to cover up what the code fails to tell |
| Switch Statements | Branches by kind proliferate in multiple places |
| Large Class | Too many responsibilities are crammed into one class |
| Feature Envy | It is more interested in the data of other classes than its own class |
| Long Parameter List | There are many parameters, and the caller's view is poor |
| Divergent Change | One class is changed often for multiple different reasons |
| Shotgun Surgery | One change requires modifications spanning multiple classes |
| Data Clumps | A chunk of data that always appears together has not become a cohesive concept |
| Primitive Obsession | Primitive types such as int or string keep being used where a concept should be represented (dedicated types are in testing.md) |
| Parallel Inheritance Hierarchies | Every time you add a subclass to one hierarchy, the same subclass is needed in another hierarchy |
| Lazy Class | A class with little reason to exist that does hardly any work |
| Speculative Generality | Generalization and abstraction with no prospect of being used are built in advance |
| Temporary Field | An object always has a field that is used only in specific situations |
| Message Chains | Requests are sent by traversing objects like `a.b.c.d` (the Law of Demeter is in object-design.md) |
| Middle Man | A class that only delegates and does hardly anything itself |
| Inappropriate Intimacy | Classes that depend on each other by reaching into each other's internals |
| Alternative Classes with Different Interfaces | Multiple classes that do similar processing do not have consistent method names or signatures |
| Incomplete Library Class | Your code is pushed around by the circumstances of the library in use |
| Data Class | A class that only holds data and has no behavior (responsibility) |
| Refused Bequest | A subclass that uses only part of the inherited features and refuses/ignores the rest |

### Duplication Is Not an Accident / Suspect the Design Behind the Symptom [7.2.2]
- Definition: Source code is not duplicated by accident. Duplication is a sign indicating the **possibility that multiple parts hold the same responsibility**. A smell is a symptom that appears on the surface; the target of improvement is the structure behind it.
- When: When you find a smell, not limited to duplication.
- Do: Before erasing the surface, ask, "What is the concern behind this symptom? Whose responsibility should it be?" Examples: Duplication → one concern spans multiple places. Long Method → multiple responsibilities coexist. Long Parameter List → a clump of data that should be grouped does not have a name as a concept.
- Check: After fixing, whether the location of that responsibility has been settled in one place.
- Exceptions/cautions: Before unifying, judge whether it is "the same intent" (judgment rule 6 in SKILL.md). Once And Only Once is not a rule for saving lines; it is a principle for settling the location of a responsibility in one place.

## Techniques [7.3–7.7]

### Rename [7.3]
- Definition: **Rename = changing the model (revisiting the design) = refactoring**. When you change a name, what changes is not only the string but the outline of the concept the name points to. It is the most basic refactoring and the one that should be done most frequently.
- When: When you find a name that falls into a naming anti-pattern (Appending numbers, Abbreviating, Meaningless names, Including the type name, No consistency, Literal translation; naming.md). That is the starting point.
- Do: Under the protection of tests, rename with the bulk rename of the IDE or language tools. Example: `data2` → `validatedOrders` adds the concept "a collection of validated orders" to the vocabulary.
- Check: Whether the new name tells the responsibility neither more nor less. Whether all references have changed and it is Green.
- Exceptions/cautions: As long as names are left as they are, the ambiguity of the model remains no matter how much you rework the structure. Renaming a public API is a change of contract, so confirm the scope of the request and the user's consent.

### Extract Method Is Building Vocabulary [7.4]
- Definition: Extract Method is not the work of making code shorter but the **work of building vocabulary**. Methods exist not to group similar processing for reuse but to give it a name and abstract it (subroutines exist to be given names; naming.md). Splitting methods is, before being a matter of performance or line count, a matter of what vocabulary you want to describe things in.
- When: When a concept is buried in a half-formed state such as a Long Method, deep nesting, or a temporary variable.
- Do: Ask, "What, in short, is this chunk doing?" and extract each meaningful unit and give it a name. Make the granularity the one at which a person would describe it most concisely in natural language. Solve from the side of the concept, not the procedure ("first loop over...").
- Check: Whether translating the upper-level method after extraction into natural language yields the very definition of that concept.
- Exceptions/cautions: Mechanically splitting a 20-line method into four equal parts is meaningless. Merely grouping similar processing turns code into spaghetti.

```csharp
// before: "Add a, b, and c, and call this sum. Divide this sum by 3."
static double Average(double a, double b, double c) {
    var sum = a + b + c;
    return sum / 3.0;
}
// after: "The Sum of a, b, and c divided by 3" = the very definition of the average
static double Sum(double a, double b, double c) => a + b + c;
static double Average(double a, double b, double c) => Sum(a, b, c) / 3.0;
```

### Comments Only for Why [7.5]
- Definition: Make comments only of what cannot be written in code. Express what (What) with class names and method names, and how (How) with the code itself. What remains—only why (Why) it was done that way—is worth writing in a comment. If a comment explains What or How, that is a symptom that it could not be fully expressed in names and code, that is, a smell.
- When: When a comment traces the behavior of the next line or the meaning of a conditional expression.
- Do: Turn the content of that comment into a name, and take over the accountability with Extract Method and naming. Keep Whys such as "why wait 5 milliseconds."
- Check: Whether, even with the comment removed, the caller reads just like a sentence of the specification.
- Exceptions/cautions: Unnecessary comments stay silent even when they drift from the implementation, lower the S/N ratio, and bury the Why that really should be read (the S/N ratio is in modeling.md). Rename, Extract Method, and cleaning up comments are three aspects of the same step.

```csharp
// before: the comment traces the What
// If it is a leap year (divisible by 4 and not divisible by 100, or divisible by 400)
if (y % 4 == 0 && y % 100 != 0 || y % 400 == 0) {
    UI.ShowYear(y);   // Show that year on the UI
}
// after: naming takes over the accountability
static bool IsDivisibleBy(this int dividend, int divisor) => dividend % divisor == 0;
static bool IsLeapYear(this int year)
    => year.IsDivisibleBy(4) && !year.IsDivisibleBy(100) || year.IsDivisibleBy(400);
if (year.IsLeapYear()) UI.ShowYear(year);
```

### Simplifying Control Structures [7.6]
- Definition: Each time nesting gets one level deeper, the reader reads on while keeping the context of that branch stacked in their head. There is a limit to the context a person can hold at once, and code beyond that limit becomes unreadable. For deep nesting, Extract Method; for proliferating branches, polymorphism.
- When: When nesting is deep. When branches by the same kind appear in multiple methods.
- Do: Extract the chunk inside the nesting by asking "what, in short, is it doing." Move branches by kind to a form in which each kind answers "what do I do" (polymorphism, the Strategy pattern).
- Check: Whether, when adding one kind, the place to fix is only one (one new class).
- Exceptions/cautions: **switch itself is not evil.** The problem is a structure in which the same branch proliferates in multiple places and forces modification of all of them every time a kind is added (Shotgun Surgery). The means of resolving it are OCP and polymorphism (object-design.md). If the branch is in one place and the kinds are stable, forgo replacing it with polymorphism, from the standpoint of subtraction (simplicity.md). Handling of the guidelines of 2 levels of nesting, 20 lines, and 3 parameters follows judgment rule 10 in SKILL.md.

The subject of the example is the Controller of a simple CAD that switches, by mouse operation, between adding line segments or rectangles and selecting (details in "Preparatory Refactoring" below).

```csharp
// before: branches on the same CommandKind proliferate in MouseDown and MouseUp
void MouseUp(Point p) {
    switch (kind) { case Line: ...; case Rectangle: ...; case Select: ...; }
}
// after: let the command itself answer the behavior for each kind
abstract class Command { public virtual void OnDragEnd(Model m, Point p) { } }
class AddLineCommand : Command { public override void OnDragEnd(Model m, Point p) => m.Add(...); }
void MouseUp(Point p) => command.OnDragEnd(model, p);
```

### Preparatory Refactoring [7.6.3]
- Definition: Refactoring before adding a feature. Not only fixing smells after noticing them, but tidying up the place **before** a change you know is coming next.
- When: When putting the requested change into the current structure as is would grow a smell (such as proliferating branches).
- Do: Fix the structure first without changing behavior, check Green, and then add the feature. How to divide scope and reporting follows the "Restraint in the Scope of Change" section of SKILL.md.
- Check: Whether the feature addition itself was done with a small, local diff.
- Exceptions/cautions: A "change you know about" is a change that is in the request or requirements. Provisions for changes that might come become Speculative Generality (simplicity.md).
- Worked example (command switching in a simple CAD): Adding rectangle addition and selection to a Controller that only adds line segments makes branches by the same kind proliferate in two places, `MouseDown` and `MouseUp`, and a start point used only by some commands squats as a field of the entire Controller (Temporary Field) → before continuing to add features, split responsibilities by command kind and banish the switch from the Controller → the start point moves inside the commands that use it → extract the common skeleton "create a shape from two points" one level further and add it to the vocabulary → when adding ellipses came later, only one class needed to be written.

### Extracting Responsibilities [7.7]
- Definition: Divide clearly by responsibility and give each a name. A unit that can be expressed by a name is the unit of extraction. Draw the boundary so that inside and outside can be distinguished "from the outside" and "simply."
- When: When you find a Large Class or Feature Envy.
- Do: Decide the scope of the extracted responsibility and place it in the smallest possible scope (the four levels of scope and "What is it, in a word?" are in object-design.md).
- Check: Whether it passes the following two inspections.
  - Explanation inspection: If, when explaining the responsibility, an enumeration creeps in such as "it does things like X and Y," the boundary is not sharp.
  - Dependency-while-writing inspection: If writing the internal logic of a function cannot be done without looking at the internal logic of other functions, the coupling is too high. The normal state is being able to write it by looking only at the contract (tests set against the contract verify this after extraction as well; testing.md).
- Exceptions/cautions: Separation of concerns is a good means of keeping things clean, but it is **not the goal**. The cleanliness at the moment you finish dividing starts to crumble when the next change comes. Keep refactoring after dividing, too.

## Kaizen [7.8]

### Keep Trying What You Think Is Good [7.8]
- Definition: Kaizen is not "doing what is good" but "**continuing to try what you think is good**." Often you cannot know what is good until you try. So try what you think is good, learn from the results, and change your actions. Red→Green→Refactor is this loop of plan, do, check, and improve run at high speed at the level of code.
- When: Always. Quality is something maintained as an activity, not by a one-time design.
- Do: Pass your output every time through the **net of feedback tools**: the compiler (deviations in syntax and types), static analysis (deviations from conventions and known dangerous patterns), and unit tests (deviations from the specification). Identify where a deviation was detected using the vocabulary of smells and fix it. Treat the user's reviews as immediate feedback as well.
- Check: Whether you can show in your report that the named smells decreased between before and after the change (the angle of improvement matters more than the level of quality itself).
- Exceptions/cautions: Do not seek perfection. Good enough is fine. An attitude of trying to reach a perfect design in one go is the exact opposite of "the iteration of design." Even what you think you already know, re-examine it without looking at it with the answer already in hand. Individual techniques are means; the purpose is to keep the code in a state that is easy to change (maximizing feedback is in testing.md).

## Speak in the Names of Smells and Techniques, Not Appeals to Mindset [7.9]

- Definition: "Be more careful" and "Be quality-conscious" teach nothing. Explain in the names of smells and techniques: "This method has three responsibilities mixed together, so first give this chunk a name and extract it." Being able to put reasons into words is skill. The names of smells and techniques become, as they are, the vocabulary of review and improvement.
- When: When self-reviewing your own output. When pointing out issues in a review. When reporting the results of a refactoring.
- Do: Inspect your own output on the premise that it can blithely contain mixed responsibilities, ambiguous names, comments that trace the What, and deep nesting. Write findings and reports in the following format.
  - `<location>: <smell name> (<evidence of the symptom>) → <technique> → <result and verification>`
  - Example: "`ProcessData`: Long Method (validation and saving responsibilities mixed) → split into `Validate` and `Save` by Extract Method → 12 tests Green"
  - Example: "switch in `Controller`: Switch Statements (two places grow with each added kind) → proposed resolving with polymorphism (not done because out of scope)"
- Check: Whether no sentence remains in reports or findings that only says "cleaned up," "tidied," or "improved." Whether each item has the smell, the technique, the reason (why do it), and the actual diff or test results.
- Exceptions/cautions: When it does not fit the list of smells, do not force a name onto it; write the symptoms and reasons concretely.

## Smell → Technique Table [7.2–7.7]

The smells the source book treats all the way to remedies are Duplicated Code, Long Method, Comments, Switch Statements, Large Class, Feature Envy, and Temporary Field (outside the 22 items, inappropriate names and deep nesting). The other rows are applications of principles from other chapters (Chapter 5, etc.).

| Smell/symptom | Technique |
|---|---|
| Inappropriate names (naming anti-patterns) | Rename |
| Long Method | Extract Method by meaningful units |
| Comments (explaining What/How) | Replace with Rename and Extract Method; keep only the Why |
| Duplicated Code | Identify the underlying responsibility and settle it in one place with Rename, Extract Method, and extracting responsibilities |
| Deep nesting | Extract Method |
| Switch Statements (proliferating branches) | Replacing types via polymorphism (the Strategy pattern) |
| Temporary Field | Move the field inside the side that needs it |
| Large Class / Feature Envy | Extracting responsibilities (assigning responsibilities, high cohesion and loose coupling; object-design.md) |
| Long Parameter List / Data Clumps | Give the Data Clump a name as a type (naming.md) |
| Primitive Obsession | Introduce a dedicated type (testing.md) |
| Message Chains | Delegate to the recipient of the request, in line with the Law of Demeter (object-design.md) |

For the following smells, no fixed technique is given (only name them). Judge whether to fix them and how by the principles on the right.

| Smell | Principle used for judgment |
|---|---|
| Divergent Change | Give it one reason to change (object-design.md) |
| Shotgun Surgery | One change → one fix. If it is proliferating branches, the Switch Statements row above (object-design.md) |
| Speculative Generality | YAGNI, design by subtraction (simplicity.md) |
| Lazy Class / Middle Man | The "What is it, in a word?" test, and delegation does not move where the role lives (object-design.md). If you cannot name the role, report it as a target for subtraction |
| Data Class | Let the holder of the information decide (Expert; object-design.md) |
| Parallel Inheritance Hierarchies / Refused Bequest | Replace inheritance for reuse with composition, LSP (object-design.md) |
| Inappropriate Intimacy | The Law of Demeter; depend only on the other party's public services (object-design.md) |
| Alternative Classes with Different Interfaces | Article 6, "Consistent Rules," and "No consistency" in naming (naming.md) |
| Incomplete Library Class | Service-Oriented Naming (SON; with names from the client's viewpoint, do not leak the library's circumstances into your own code; naming.md) |
