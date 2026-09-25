# Think Simple (Simplicity)

Read this when starting a design, when reviewing generated code, and when you feel "this is getting complex." It gathers the principle of Think Simple, the highest value, and the vocabulary for identifying and cutting complexity.

## Think Simple: Do Not Solve Complex Problems [8.1]

- Definition: How should you solve a complex problem? Keep yourself from ending up having to solve a complex problem. Much of the complexity in software development lies not in the problem itself but on the side of how it is solved.
- The value at the core of the Seven Articles is simplicity. Naming, responsibility, testing, and refactoring are all means of committing to this value.
- **Simplicity is not knowledge but practice.** Merely knowing "build it simply" means nothing. Every time you tackle a task, actually act in the direction of increasing simplicity (commitment to the value).
- There are two ways to fight complexity: the engineering approach (division, modeling, planning) and the agile approach (solve it while it is still simple, do not make it complex, correct course with feedback). The two are not mutually exclusive; only with both together can you fight complexity.

## Two Principles of Simplicity [8.2]

### The Giraffe-in-the-Refrigerator Principle [8.2.1]
- Definition: Solve the problem while it is simple. Do not make it complex. A problem is at its simplest when it is first given.
- When: When you have read the requirements and start thinking about how to solve them. When you start thinking "but what if...".
- Do: Do not add constraints or obstacles on your own that are not in the problem statement (request, specification, tests). Start from the simplest solution that satisfies the written requirements as they are.
- Check: For each constraint you are handling, can you point to where in the request, specification, or existing code it is grounded? A constraint you cannot point to is one you added yourself.
- Exceptions/cautions: If a constraint with an unclear basis greatly affects the result, do not add it; instead, state it explicitly as an assumption or confirm with the user (Principle of Action 3 in SKILL.md).

### Developer B's Thinking: You Are the One Making the Problem Complex [8.2.2]
- Definition: A way of thinking that keeps adding problems that have not occurred, and even when shown a solution, brings up yet another new problem, steering the problem toward being unsolvable. The person themselves does not notice it.
- In code, it appears as continually adding provisions for situations that have not occurred: "requirements like this might come in the future," "to be prepared for every possible input." Code written that way is Developer B's thinking given form as it is.
- Do: Before adding a provision, distinguish whether it is "a current problem" or "a problem that does not yet exist." If the latter, do not add it.
- Exceptions/cautions: Evaluating risks in advance is itself important. Evaluating is fine. What is wrong is adding problems until the problem cannot be solved. Report risks you notice, and decide whether to add countermeasures to the code in light of the requirements.

### The Saibara Principle [8.2.3]
- Definition: Do not bring yourself into a situation where you must deal with complex problems. There is no magic that efficiently fixes, after the fact, code that has accumulated so many bugs it cannot be touched.
- Repeat the daily practices for "not bringing yourself there" (four pillars) in small increments from the early stages of development:

| Pillar | The agent's practice |
|---|---|
| Simple coding | Perform a subtraction check at every step and do not bring in complexity |
| Tests that always run | Proceed test-first and run the tests at every step (testing.md) |
| Refactoring | Simplify places that have become complex, on the spot. The moment you notice is the cheapest (refactoring.md) |
| Continuous feedback | Crush deviations early with a self-review at every step (quality-gates.md) |

- Refactoring is the technique of **bringing back to simplicity** what has become complex; Think Simple is the attitude of **not making** it complex in the first place. Continuing to use the technique of bringing back to simplicity every day becomes the practical form of the attitude of not falling into complexity. The two are a pair; neither holds up without the other.

## Identifying the True Nature of Complexity [8.3]

### Contain Essential Complexity; Eliminate Accidental Complexity [8.3.2]
- Definition: Essential complexity is complexity that the problem domain itself has (e.g., calculation rules of a tax system). Accidental complexity is complexity that has nothing to do with the problem, brought in through implementation convenience or a poor way of solving (e.g., configuration loading or DB connections mixed into business logic, abstraction layers that exist only for the future). Contain essential complexity; eliminate accidental complexity.
- When: When you feel the code is complex. When designing complex processing.
- Do: Confine essential complexity inside the boundary of the type responsible for it, so that from the outside it looks like a simple interface (object-design.md). Accidental complexity is not part of the problem but baggage you brought in, so cut it.
- Check: If you removed that complexity, would any of the requirements become impossible to satisfy? If they can still be satisfied, it is accidental.
- Exceptions/cautions: **The trap of "it's essential, so it can't be helped."** If "essentially complex" means "the complexity is unavoidable," it is a tautology and explains nothing. Before deciding it is essential and giving up, examine whether it truly cannot be avoided. Under the label "essential," a considerable proportion of accidental complexity that could have been avoided is hidden.

### YAGNI: Complexity for the Future Is Waste Now [8.3.3]
- Definition: You Aren't Gonna Need It. Build it when it becomes necessary. Among accidental complexity, what slips in wearing the most well-meaning face is "provisions for the future."
- When: When you are about to add an interface, abstract base class, generics, extension point, or configuration item.
- Do: Write in the minimal form that solves today's problem. Abstract when a second implementation is actually needed (or when it is explicitly stated in the requirements).
- Check: What part of today's problem does that abstraction solve? If what it solves is "a requirement that might come someday," cut it.
- Exceptions/cautions: **An abstraction introduced after the need becomes visible fits its shape; an abstraction introduced before the need becomes visible is a guess.** Extension points added in anticipation often do not fit in shape when the actual requirement arrives. And all the while until then, they keep making every reader pay the cost of wondering "what is this layer for?" For drawing the line with abstractions that lower the cost of reading, follow judgment rules 1–3 in SKILL.md.

```csharp
// before: only CSV is used, yet it anticipates "future data sources"
interface IDataSource<T> { IEnumerable<T> Read(); }
abstract class DataSourceBase<T> : IDataSource<T> { /* Read → ReadCore */ }
class CsvDataSource(string path) : DataSourceBase<string[]> { /* ReadCore */ }

// after: the minimal form needed now
static IEnumerable<string[]> ReadCsvLines(string path)
    => File.ReadLines(path).Select(line => line.Split(','));
```

### Simple Is Not the Same as Easy [8.3.4]
- Definition: Expressing something simply, or understanding a simple expression, is not necessarily easy. Abstract descriptions are often both simple and hard to understand. The criterion of simplicity is neither ease of writing nor being beginner-friendly, but that **the intent is expressed directly, with nothing extra**.
- When: When torn between a declarative and a procedural way of writing. When you feel like avoiding an expression "because it is hard to understand."
- Do: Do not confuse a matter of familiarity (whether it is easy) with a matter of structure (whether it is simple). Example: one line expressing the same intent declaratively is simpler than ten lines of loop, but it does not look easy to a reader who is unfamiliar with it. That it does not look easy is a matter of familiarity, not of structure.
- Exceptions/cautions: For how to treat the reader's familiarity and the existing style, follow judgment rule 8 in SKILL.md. Conventions that forbid particular expressions without this distinction do not improve maintainability (quality-gates.md).

## Simplicity Shows in the Expression of Intent [8.4]

### Simple = High S/N Ratio [8.4.1]
- Definition: Simple code is code with a high S/N ratio (for the definitions of S/N ratio, intent, and noise, see modeling.md). Accountability (putting What before How), declarative style, Extract Method, and comments only for Why are all techniques that reduce noise and make intent stand out, and simplicity is the name of the state they ultimately reach.

### Do Not Exceed the Complexity the Reader Can Handle [8.4.2]
- Definition: The indicator of beautiful source code is being simple: not exceeding the limit of complexity that the person reading that source code can handle. People cannot think through complex things perfectly down to the details all at once. That is why subroutines exist. The baseline is drawn not by the machine's convenience but by the limits of human cognition.
- When: When writing code with nested loops, intricate index calculations, or many states to track at once.
- Do:
  - For a double loop, name the inner processing and extract it, making each method a single loop ("find a specified string among multiple names" is a combination of single-loop problems). For the nesting guideline and the conditions for exceeding it, follow judgment rule 10 in SKILL.md.
  - Handle a multidimensional array of arrays as a one-dimensional array with accessors, contain complexity such as coordinate transformation in one place, and do not make the caller think about it.
- Check: When reading each method, are there few things you need to keep in mind at the same time?
- Exceptions/cautions: Code that exceeds the limit cannot be read even if it is correct. Code that cannot be read cannot be verified, and becomes comprehension debt.

```csharp
// Hold a two-dimensional board as a one-dimensional array, and contain the coordinate transformation in one place: the indexer
class GameBoard {
    const int Size = 8;
    readonly int[] cells = new int[Size * Size];
    public int this[int row, int column] {
        get => cells[row * Size + column];
        set => cells[row * Size + column] = value;
    }
}   // The caller just writes board[row, column], without having to think about the inside being one-dimensional
```

## Commit to Simplicity [8.5]

### Rephrasing Table for Self-Inspection [8.5.1]

When you are unsure about a design, when writing review comments, and when making your own work plan, check which side, left or right, your thinking is on. The left side is dangerous precisely because it is earnest and even diligent.

| Non-simple way of thinking | Thinking simply |
|---|---|
| How shall I solve this complex problem? | How can I make the problem simpler? |
| How should I confront complex problems in the future? | How can I avoid making the problem complex from now on? |
| I wonder what I ought to do? | What shall I do? |
| Let's enumerate every conceivable solution and examine them all thoroughly | Let's actually try the solutions we have now, little by little, and adjust the next move based on feedback from the results |
| Let's enumerate everything that could be an obstacle and devise countermeasures for all of them | Instead of only listing reasons why we can't or won't, let's just start, and get feedback from the results |

### The Two Principles in Coding Practice [8.5.2]

| Principle | Practical form | Means |
|---|---|---|
| The Saibara Principle | Bring yourself toward facing not a large problem whose specification and verification method are both unclear, but simple problems that have a clear specification and are verified automatically ("is it the last day of February?", "is it a leap year?") | Naming, the What viewpoint, dividing responsibilities (naming.md, object-design.md) |
| The Giraffe-in-the-Refrigerator Principle | Instead of trying to consider everything and solve it all at once, reach the solution by actually solving the problem at hand one at a time | Test-first (testing.md) |
| (Bringing back to simplicity) | Bring what has become complex back to simplicity | Refactoring (refactoring.md) |

Example: Do not write date validation in one go as nested conditional branches that account even for leap years. First write "if it is not correct as a date, throw an exception," and then settle it one level at a time: "being correct as a date means the year is correct, the month is correct, and the day is correct."

## Design by Subtraction [8.6]

- Definition: What not to build. What to cut. Where to say "that is not needed." Precisely because addition has become AI's forte, the work becomes subtraction.
- Background: Generation costs have plummeted, and we have entered an era in which complexity is cheaply mass-produced. Meanwhile, understanding has not gotten faster, and the bill for cheaply mass-produced complexity piles up as comprehension debt. Moreover, AI output has a habit of packing in probabilistically plausible "this and that" considerations, and of introducing generic hierarchies and design patterns "for future extension" without being asked. It commits YAGNI violations nonchalantly, with a well-meaning face.
- When: Right after generating code, before reporting the diff. Always view your own output as a target for subtraction.
- Do: Cut abstractions, patterns, and provisions that were not asked for from your own output. Explain the reason for cutting not with appeals to mindset ("make it simpler") but with the following vocabulary.
- Check: Is there anything that matches any row of the checklist below?

| What to check | Vocabulary | Example of cutting / reporting |
|---|---|---|
| Interface hierarchies with only one implementation; unused extension points, arguments, and configuration items | YAGNI violation | "This hierarchy is a YAGNI violation, so I cut it until a second implementation appears" |
| Configuration loading, DB connections, logging, etc. mixed into business logic | Accidental complexity | "Configuration loading is accidental complexity, so I separated it from the business logic" |
| Temporary variables, indices, and procedural descriptions that do not exist in the intent | Noise (low S/N ratio) | "This loop has a lot of noise, so I rewrote it in a declarative form where the intent is visible" |
| Provisions for inputs and situations not in the problem statement (defenses against situations that have not occurred) | Giraffe in the Refrigerator / Developer B's thinking | Report what was not built: "I did not include handling for XX, which is not in the requirements. I will add it if needed" |
| Design patterns and generics that were not asked for | YAGNI violation / accidental complexity | Introduce them once the need becomes visible. Add one line to the report about the candidates you held off on |

- Exceptions/cautions: Substitution points for testing and abstractions that lower the cost of reading (Extract Method, naming a concept, giving a type to a clump of data) are not targets for cutting (judgment rules 2 and 3 in SKILL.md). Also do not cut boundaries that contain essential complexity. Guard clauses and type-based validation of preconditions at public boundaries are an explicit statement of the contract (accountability in executable form), not targets for cutting (quality-gates.md). What you cut are branches that go out of their way to handle inputs and situations not in the requirements. The vocabulary for declining complexity is the last bastion protecting simplicity, the core of the Seven Articles.
