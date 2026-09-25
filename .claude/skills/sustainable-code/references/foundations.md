# Foundations: Beautiful Code and the Seven Articles

Read this when implementing, when evaluating code quality, or when you want to judge each item of the self-check in SKILL.md in depth. This file summarizes the definition of beautiful code, the original criteria and the judgment for each of the Seven Articles, technical debt and comprehension debt, the premise and the axis of judgment of these criteria, and the glossary.

## What Is Beautiful Code [1.1, 0.4]

- Definition: **Beautiful code is code that is easy to extend and maintain.** It does not mean visual neatness or aligned indentation. **The work of programming is always a change to the existing code.** Working is a given premise; what should be compared is "dirty and working" versus "clean and working". A program can only be called a "working program" once it has been able to keep working until the end of its life.
- Sustainable Code is code that both humans and AI can keep understanding and keep changing. Names that convey intent, narrow scopes, and clear boundaries of responsibility lower human cognitive load and, at the same time, create an environment in which AI is less likely to misread the context.
- Dirty code becomes debt, inflating development costs like interest. Beautiful code is an asset that makes future changes easy.
- When: When you are tempted to think "it just has to work" or "I'll clean it up later". When you are about to choose an option based on how little code it takes or how fast it is to write.
- Do: Write so that the following three are satisfied: easy to change / testable / easy to understand. Evaluate the quality of code not by the amount written at the moment of writing, but by the cost over its entire lifecycle.
- Check: Can the next person to change this code (including yourself six months from now or another agent) identify without hesitation where to fix it?
- Exceptions/cautions: When the scale doubles, the number of relationships between elements increases by more than that, so complexity grows by more than double. If beauty is not maintained, complexity grows in a vicious circle and development becomes unsustainable. The same thing happens in development using AI.

### Qualities That Beautiful Code Improves [1.1.2, 1.3.1]

- What beautiful code directly improves are three things: **understandability** (intent is clear), **changeability** (easy to fix and extend), and **verifiability** (easy to test). Maintainability is the general term for the quality obtained by combining these three.
- These are internal qualities visible to developers, and they support the external qualities visible to end users over the long term. Efficiency can be in a trade-off with readability (for how to handle this, see judgment rule 5 in SKILL.md).

## The Seven Articles for Beautiful Code [1.2]

The Seven Articles are not individual techniques but a single integrated set of mutually related guidelines. They have the greatest effect when applied in an integrated way. The one-line version is in the self-check of SKILL.md. Here, the original criteria of each article and how to make judgments are written.

### Article 1: Express Intent [1.2.1]
- Definition: Intent is expressed / Intent is easy to understand / There is little description other than intent / What (what to do) is described rather than How (how to do it) / If possible, Why (why to do it) is also described.
- When: When writing procedures such as loops, indexes, temporary variables, and string building.
- Do: Write code not as instructions to a computer but as a means of communication between humans, the way a person talks to a person. Remove noise other than intent, such as loop variables and indexes, and write the What.
- Check: If you read the code aloud as it is, does it become a sentence of "what it does"?
- Exceptions/cautions: `return a + b;` and `var sum = a + b; return sum;` are, strictly speaking, descriptions with different intents (the latter is "let a + b be the sum. Return the sum"). This is not about which is better. Choose the one that fits the intent you want to express.

```csharp
// before: How (the procedure) is mixed in as noise
var result = "";
for (var index = 0; index < names.Length; index++) {
    if (index > 0) result += ", ";
    result += names[index];
}
return result;
// after: Only the What (what it does) remains
return string.Join(", ", names);
```

### Article 2: Single Responsibility Principle [1.2.2]
- Definition: A unit of the program describes one and only one job / That job is described completely within that program unit. Satisfying these two is called **high cohesion**. It is important not only that the responsibility is single, but also that the responsibility leaks into other places as little as possible. The Single Responsibility Principle (SRP) is defined as "a class should have only one reason to change".
- When: When things that change for different reasons, such as calculation, persistence, and display format, sit side by side in one class or method.
- Do: Divide units by reason to change (e.g. if salary calculation, data saving, and report generation coexist in an employee class, divide them into `BonusCalculator`, `EmployeeRepository`, and `EmployeeReportGenerator`).
- Check: When you enumerate the reasons to change for that unit, is there only one? Is the code describing that job not scattered across other units?
- Exceptions/cautions: How to assign responsibilities, and not misreading SRP as a "rule to make things small", are covered in object-design.md.

### Article 3: Precise Naming [1.2.3]
- Definition: A name expresses one and only one job in a word, necessarily and sufficiently / The same thing gets the same name, different things get different names. Decide names from the client's viewpoint.
- Do: For the full criteria and techniques (SON, naming anti-patterns, the procedure for step-by-step improvement), follow naming.md.

### Article 4: Once And Only Once [1.2.4]
- Definition: Things with the same intent are not written in duplicate / What is it and what is not it can be distinguished. If code with the same intent is not duplicated, it becomes simple, and you avoid repeating the same work in implementation, extension, and maintenance.
- When: When you are about to write similar code, or when you find it.
- Do: Distinguish the type of duplication. Duplication of implementation (the same code is in multiple places) / duplication of knowledge (the same business rule is implemented in multiple places) / duplication of structure (similar structures are repeated). Once you can determine that they have the same intent, gather them into one place and have the others call it (e.g. the calculation of the total amount calls the calculation of the subtotal).
- Check: When you change that rule, is there only one place to fix?
- Exceptions/cautions: When you find duplication, gather it only after determining whether it is duplication of "the same intent". Forcibly lumping together code of different intents that merely happens to look similar makes it harder to change instead.

### Article 5: Precisely Written Methods [1.2.5]
- Definition: The inside of a method consists of a collection of descriptions at the same level of abstraction / The inside of a method is described at a natural granularity (like spoken language) / A moderate amount (not too much description).
- When: When low-level details such as opening and closing connections or building SQL are mixed in between high-level steps. When a method has become long.
- Do: Extract low-level details into methods with names that express intent. Make the higher-level method a sequence of steps at the granularity at which humans naturally think.
- Check: When you read the higher-level method aloud, does it become an explanation of steps in spoken language? Does it fit within about nine statements (the chunks of information a person can handle at once in short-term memory number about nine at most, and "I'll tell you three things" gets across better than "I'll tell you twenty important things")?
- Exceptions/cautions: For how to treat the nine statements, follow judgment rule 10 in SKILL.md.

```csharp
// before: Levels of abstraction are mixed
void ProcessOrder(Order order) {
    ValidateOrder(order);
    using (var connection = new SqlConnection(connectionString)) {
        connection.Open();
        new SqlCommand("UPDATE Orders SET Status = ...", connection).ExecuteNonQuery();
    }
    SendConfirmationEmail(order);
}
// after: Only steps at the same level of abstraction line up (details go into UpdateOrderStatus)
void ProcessOrder(Order order) {
    ValidateOrder(order);
    UpdateOrderStatus(order, OrderStatus.Processed);
    SendConfirmationEmail(order);
}
```

### Article 6: Consistent Rules [1.2.6]
- Definition: The whole follows the same rules. Consistent rules lower learning costs, raise predictability (in similar situations, similar solutions can be expected), and improve maintainability.
- Elements to make consistent: naming rules (class names, method names, variable names) / coding style (indentation, brace placement) / design patterns (error handling, logging) / architecture patterns (layer structure, dependencies).
- Do: Before writing, investigate the existing code's style with respect to the four elements above, and match it.
- Check: When the added code is placed next to the existing code, do differences in style (capitalization rules, how errors are returned, etc.) not stand out?
- Exceptions/cautions: What is at issue is not an individual's local preferences but the consistency of the entire codebase. Continuing to keep the chosen style throughout the whole is more important than which style is chosen.

### Article 7: Testable [1.2.7]
- Definition: It is made possible to know that the description is correct. What Testable design aims for is **maximizing feedback**.
- Key point: Avoid direct dependencies on the outside and use a structure in which dependencies can be passed in / get compile-time feedback with dedicated types / do not make coverage the goal (metrics hacking).
- Do: For testable structure, kinds of feedback, and how to write tests, follow testing.md.

## Extract a Concept and Name It: A Minimal Example [0.6]

A concept embedded in a conditional expression must be decoded by the reader every time. If you extract the concept and give it a name, neither yourself six months from now, nor another developer, nor AI will misread the intent.

```csharp
// before: Works correctly, but "what a VIP customer is" is buried in the conditional expression
if (customer.PurchaseHistory.Count > 10 &&
    customer.LastPurchase > DateTime.Now.AddMonths(-3) &&
    customer.TotalSpent > 10000) { /* VIP processing */ }
// after: Extract the concept and give it a name
bool IsVipCustomer(Customer customer) =>
    HasFrequentPurchases(customer) &&
    IsRecentlyActive(customer)     &&
    HasHighLifetimeValue(customer);
```

- The before also works correctly. The problem is not the behavior, but that the intent cannot be read.

## Technical Debt and Comprehension Debt [0.2, 1.3]

### Technical Debt Exists in the Form of Concrete Code [1.3.2, 1.3.3]
- Definition: Technical debt is the future cost arising from choosing a short-term solution (a design loan that you are made to repay later with interest). It is not an abstract slogan; **it exists as concrete forms of code**, such as names whose intent cannot be read (`ProcessData`) and if-else branches that multiply every time a rule is added. **That is exactly why it can be repaid with the Seven Articles.**
- Interest: Takes time to understand / high risk when changing / difficult to test / becomes a breeding ground for bugs.
- When: When you find code that adds a branch every time a rule is added, or names whose intent cannot be read.
- Do: Name the debt as a smell, and decide how to repay it by which of the Seven Articles it violates (e.g. replace a chain of per-character if-else with a lookup table of conversion rules and a named method that converts one character). If it is out of scope, limit yourself to reporting it (the "Restraint in the Scope of Change" section of SKILL.md).
- Check: After repayment, does adding a rule take only adding one line to the lookup table?

### Debt Reversal: Comprehension Debt [0.2, 1.3.2]
- Definition: Comprehension debt is a state in which the code is working, but no one can explain "why it is this way" or "where to fix it".
- Debt reversal:

| | What is ahead | What lags behind | How it is repaid |
|---|---|---|---|
| Traditional technical debt | Correct understanding in the head | Organization of the code | Through refactoring, make the code catch up with the understanding |
| Comprehension debt | Code that works perfectly and passes its tests | Understanding of why that code was written that way | The understanding cannot catch up with the code |

- Even though the form of the debt changes, its true nature remains the lack of understandability, changeability, and verifiability.
- Comprehension debt is usually inconspicuous. It looks tidy, the tests pass, and the diffs look small. So it piles up unnoticed and surfaces all at once in situations such as specification changes, bug fixes, and incident response.
- Comprehension debt is not a problem for humans alone. The larger and more complex a codebase becomes, the harder it becomes for AI itself to grasp the whole picture. If changes are piled up without considering the increase in complexity, the result is a collection of code that is not sufficiently understood by either humans or AI and is difficult to maintain and extend.
- **The illusion of productivity**: When results are displayed immediately and diffs grow quickly, it feels as if work is progressing. In reality, requirement confirmation, revising the design, review, testing, and rework remain, and even if only the speed of writing goes up, the project as a whole does not get faster at the same ratio. You can borrow speed in advance, but you cannot borrow understanding in advance.
- When: When you have generated a large amount of code in a short time. When you are about to judge the work complete because the tests passed.
- Do: Do not measure progress only by the amount of diff or by tests passing. Look for places in your change where you cannot say "why I wrote it this way" or "where I would fix it to change it next", and fix the structure or names until you can.
- Check: Can you explain the What and Why of the change in your own words without looking at the code (the form of the report is in collaboration.md)?
- Exceptions/cautions: The three debts (technical debt, comprehension debt, intent debt) and the overall countermeasures for them are in quality-gates.md.

## Worked Example: Evaluation by the Seven Articles [1.5]

Situation: Evaluate a method like the following. `UserManager.DoStuff(string data)` performs, within the nesting of a single method, checks for null and empty strings, splitting by commas and checking the number of elements, console output, saving to a database with a hard-coded connection string, and sending email via SMTP.

| Article | Problem |
|---|---|
| Express Intent | The name `DoStuff` does not tell you what it does |
| Single Responsibility | Multiple responsibilities, validation, saving, and email sending, are mixed |
| Precise Naming | The method name and variable names are inappropriate |
| Once And Only Once | The structure is prone to duplication in the future |
| Precisely Written Methods | Levels of abstraction are mixed, and it is too long |
| Consistent Rules | The coding style is not consistent |
| Testable | Testing is difficult because it depends directly on the database and SMTP |

- Judgment: Make it a service with a name that expresses what it does (`RegisterUser`), move the parsing of input into a dedicated type, and separate saving and email sending into injected dependencies.
- Reason: The problems of each article are resolved by changes in the same direction: a name that expresses intent, separation of responsibilities, and injection of dependencies. Because the Seven Articles are a single integrated set of guidelines, do not fix them one by one separately; look at them together and judge.
- Basis for self-inspection: Mixed responsibilities, vague names like `object` and `data`, and direct dependencies on external systems are characteristics that tend to appear when AI generates code without being given constraints. AI output tends to approach the statistical average of its training data, and unless constraints are made explicit, it tends to become code that appears to work but carries structural concerns. Suspect your own output from these three points first.
- Evaluation by the Seven Articles can be used as is, both for code written by humans and as review criteria for code generated by AI (for application to reviews, see quality-gates.md).

## Premise: Why Follow These Criteria

- **Beautiful code is code that is easy to extend and maintain.** "It works" is a given; what we compare is "dirty and works" against "clean and works." The work of programming is always a change to the current code. [1.1]
- The center of gravity of development is reading. The code an agent writes is "someone else's code" from the start. [0.1]
- **Comprehension debt**: a state in which the code works, yet no one can explain "why it is like this" or "where to fix it." It piles up at AI generation speed and also hinders the AI's own understanding. [0.2]
- AI output without constraints drifts toward average designs and has a habit of adding complexity, "this and that too." Code that AI generates is not necessarily sustainable. [0.4, 8.6]
- Quality and speed are not a trade-off; they move together. Code that is easy to understand is fast to change, and because changes are fast, verification and improvement are fast too. [0.4]

## The Axis of Judgment [8.1, 8.2.3, 8.4.2, 10.5]

```
Supreme value: Think Simple
  Do not exceed the complexity the reader can handle / Do not put yourself in a complex situation
  ├ Move 1: Divide boundaries and name them … Name and Conquer, SON
  ├ Move 2: Express intent as a What …… the Seven Articles, accountability, S/N ratio
  └ Move 3: Put it in a verifiable form ……… Testable, contracts
```

The four weapons (the Seven Articles for Beautiful Source Code / Name and Conquer / SON / Testable design) are one system supporting these three moves. [0.6]

## Glossary

| Term | Definition (details) |
|---|---|
| The Seven Articles | Express Intent / Single Responsibility Principle / Precise Naming / Once And Only Once / Precisely Written Methods / Consistent Rules / Testable. A unified set of guidelines that are interrelated (this file) |
| Accountability | The source code itself conveying, just by being read, its intent and the scope of its responsibility (what it does and what it does not do). Write the What rather than the How (naming.md) |
| S/N ratio | Intent is the signal; descriptions other than the intent are noise. Simple code is code with a high S/N ratio (modeling.md) |
| Separation of concerns | The basic principle of modeling. Cut out concerns and give each a name (modeling.md) |
| Why→What→How | Think in the order: why build it → what to build → how to build it. The purpose drives the means (modeling.md) |
| To understand = to be able to separate | Being able to state the boundary between "what this is" and "what this is not." What cannot be separated is not yet understood (modeling.md) |
| Name and Conquer | The technique of finding what deserves attention, naming it, separating the concept from everything else, and thereby dominating the problem (naming.md) |
| SON | Service-Oriented Naming. Name = interface = service. Decide names from the viewpoint of the side that uses them (naming.md) |
| Vocabulary | Types, methods, and variables are the vocabulary for describing a program. What you do not know the name of, you cannot see (naming.md) |
| Responsibility and reason to change | Deciding "which object's job we will say it is." One change → one fix (object-design.md) |
| Auxiliary line | SOLID, the Law of Demeter, and patterns are not rules to memorize and apply, but auxiliary lines for judgment when thinking about how a change propagates (object-design.md) |
| Testable | Being structured so that you can tell the code is written correctly. The purpose is maximizing feedback (testing.md) |
| Test against the contract | Tests verify published behavior (the contract) and do not test against implementation details (testing.md) |
| Code smell | A name given to a sign of code degradation. Name the smell and choose the corresponding technique (refactoring.md) |
| Magnitude of failure | Error × dwell time of the error. The moment you notice a smell is the moment it is cheapest to fix (refactoring.md) |
| Kaizen | Not doing good things, but continuing to try what you think is good (refactoring.md) |
| Think Simple | The attitude of not ending up having to solve complex problems. The Giraffe-in-the-Refrigerator Principle and the Saibara Principle (simplicity.md) |
| YAGNI / design by subtraction | Build it when it becomes necessary. Decide what not to build and what to remove (simplicity.md) |
| Contracts and guard clauses | Engrave preconditions, postconditions, and invariants into the code. A contract is accountability in executable form (quality-gates.md) |
| The three debts | Technical debt (the code), comprehension debt (in the head), intent debt (the record of design decisions) (quality-gates.md) |
