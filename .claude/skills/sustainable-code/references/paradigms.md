# Choosing Among Paradigms

Read this when deciding whether to write in imperative, declarative, object-oriented (OO), or functional style. Use it when choosing a style at design time, and when reviewing whether the style of your own generated code fits the problem.

## A Paradigm Is a Way of Dividing [4.1]

- Definition: The difference between paradigms is a difference in the "way of dividing" in divide and conquer (splitting a problem into pieces small enough to solve). Imperative divides into steps of a procedure, OO into objects that have responsibilities, functional into composable functions, and declarative into "what to achieve" and "how to execute it". Choosing a paradigm is a modeling judgment: "From which viewpoint should this problem be divided to yield the simplest model?"
- When: When you start writing a new piece of processing. When you are unsure whether to change the style of existing code.
- Do: Do not cling to a single paradigm; choose and combine according to the nature of the problem. Multiple paradigms may coexist in one system.
- Check: Can you explain, in the chosen style, "when a change or extension comes, where and how will it have to be touched"?
- Exceptions/cautions: All of them work correctly. The difference is how things look when a change comes; do not choose by preference or fashion.

## OO Does Not Separate the Concerns Flowing Inside a Method [4.1]

- Definition: Dividing into classes and encapsulation work well at the unit of a "concept", but they do not separate the processing concerns that flow inside a single method (filtering, sorting, transformation, display). Concerns that cut across multiple concepts can be separated by combining in the functional style.
- When: When filtering, sorting, formatting, output, and the like are mixed within one loop.
- Do: Assign each concern to one stage of a pipeline (filtering / sorting / transformation / processing of each element), and compose them in series.
- Check: When you change the sorting rule, do you only need to touch the sorting stage; when you change the display format, only the transformation stage?

```csharp
// before: sorting, filtering, and display mixed in one loop
staffs.Sort((a, b) => a.Number.CompareTo(b.Number));
foreach (var s in staffs)
    if (s.Name.Contains(key) || s.Number.ToString() == key) Print(s);
// after: one concern per stage
var lines = staffs.Where(s => s.Name.Contains(key) || s.Number.ToString() == key)
                  .OrderBy(s => s.Number)
                  .Select(s => $"{s.Name}({s.Number})");
foreach (var line in lines) Print(line);   // per-element processing (do not add a ForEach extension just for this)
```

## Imperative If the Procedure Is the Spec, Declarative If the Result Is the Spec [4.2]

- Definition: Imperative writes the "how" as a procedure and changes of state; declarative writes "what" you want to achieve and delegates the details of execution to the language processor. If the procedure is the spec, imperative; if the result is the spec, declarative. When in doubt, ask yourself: "Is what I want to convey to the reader of this code the How or the What?"
- When: When choosing how to write something. When hand-written loops, indices, and temporary variables are lined up.
- Do: Use imperative only where the execution procedure itself is the concern (hardware control where write order and timing are the spec, performance-tuned implementations where the procedure itself is the body of the algorithm, workflows where the procedure and control on failure are the spec). Write data filtering, transformation, and aggregation, and the definition of business rules, declaratively. Make the combination of imperative for the outer workflow control and declarative for the inner data processing the basic form.
- Check: Has the amount of information the reader must follow decreased, rather than the number of lines (e.g., are you making the reader reconstruct in their head that a double loop is a "sort")?
- Exceptions/cautions: Declarative is not always better. Declarative can make the execution process hard to see and hard to debug. Do not forcibly rewrite processing whose procedure is the spec into declarative form.

Why intent gets buried in imperative code (reversed when made declarative):

| Problem with imperative | Improvement with declarative |
|---|---|
| Much noise (loop variables, indices, and temporary variables cover the essence) | Implementation details are hidden inside declarative operations |
| Intent is buried in implementation details | "What you want to do" reads directly |
| Errors are likely because the writer bears the management of boundaries and indices | The language and library guarantee boundary checks and the correctness of sorting |

## Declare Rules as a Lookup Table (Data) [4.2]

- Definition: Declare business rules not as a chain of if statements but as data in the form of a lookup table. Adding a rule becomes adding one row to the table, without touching existing control flow.
- When: When branches that change the calculation per kind or per condition are lined up.
- Do: Write the "condition → calculation" pairs as a lookup table or a list of pattern matches. Make the code take nearly the same shape as the list of business rules.
- Check: Can you see the whole list of rules at a glance by reading just one place in the code?

```csharp
static readonly Dictionary<CustomerType, Func<Order, decimal>> DiscountRules = new()
{
    [CustomerType.Premium] = order => order.Total * 0.15m,
    [CustomerType.Regular] = order => order.Total > 1000 ? order.Total * 0.05m : 0,
    [CustomerType.New]     = order => order.Items.Count > 5 ? order.Total * 0.1m : 0
};
```

## OO: Keep State and the Rules That Protect It in the Same Place [4.3]

- Definition: The core of OO's way of dividing is to unify state (data) and the responsibilities that handle it (behavior) and enclose them in an object. Having state and the rules that protect the state in the same place is encapsulation.
- When: When representing a target that has state and whose state transitions have rules (business concepts, UI components, the progress state of a workflow, etc.).
- Signal that OO fits: When the business language is spoken as "subject + verb" (e.g., "the customer confirms the order"), and the concepts and behaviors appear directly as types and operations [4.3]. However, do not mechanically turn nouns into classes (object-design.md).
- Do: Do not expose state; make all changes go through operations. Put the rules (e.g., you cannot withdraw more than the balance) inside those operations, and create no bypass routes.
- Check: Is the scope of investigating a "the value is wrong" bug narrowed to the few operations that change the state (with publicly exposed mutable state, the scope of investigation is the entire codebase)?

## OO: Encapsulation Confines Change to the Inside; Polymorphism Confines It to Adding or Removing Kinds [4.3]

- Definition: Encapsulation confines change to the inside of one object, and polymorphism, through a common contract, confines change with respect to adding or removing kinds of objects. It expresses in types the boundary between "the common flow" and "the part that varies (the difference)".
- When: When there is a concrete prospect that the kinds of targets receiving the same treatment will increase.
- Do: Make the using side see only the common contract, and confine the differences to each implementation. For assigning responsibilities, the direction of dependencies, and the details of SOLID, follow object-design.md.
- Check: When you add one kind, does not a single line of the using side's code change?
- Exceptions/cautions: For the weaknesses of OO (processing that crosses boundaries, the difficulty of testing and parallel processing due to mutable state), see the "Strengths and Weaknesses of OO" section of object-design.md.

## Functional: Pure Functions Reduce What You Must Think About [4.4]

- Definition: A pure function is a function that (1) always returns the same output for the same input and (2) does not change external state. Its benefit is that "there is less to think about". Tests can be written just by listing input-output pairs, and you need not suspect call order or hidden state.
- When: When writing calculations, transformations, or decisions (tax calculation, discount decisions, fee computation, etc.).
- Do: Make calculations return results from their arguments alone, and do not bring in global state or I/O from outside the signature.
- Check: Can you verify the correctness of the function without examining anything outside it?

## Functional: Manage Side Effects Rather Than Eliminate Them [4.4]

- Definition: Side effects cannot be eliminated completely (saving and displaying are side effects, and a program without side effects is useless). The practical guideline is not eliminating side effects but managing them.
- When: When calculation and I/O or persistence are mixed in one function.
- Do: Separate the parts that can be kept pure (calculation, transformation, decision) from the parts that need side effects (I/O, persistence), and make the pure part as large as possible.
- Check: Is most of the program a testable, predictable region?

## Functional: Immutability and the Habit of Writing Expressions [4.4]

- Definition: Immutable data returns a new value instead of being changed. Since a value, once created, does not change, bugs of the "someone rewrote it without my noticing" kind cannot occur in principle. A statement is a description that "executes something", and an expression is a description that "produces a value"; code written in expressions contains no assignments to intermediate variables (small state changes).
- When: When defining types that pass values around. When assembling a result through repeated assignments.
- Do: If the language has immutable data, pattern matching, or expression-form branching, write processing that can be written as expressions as expressions.
- Check: Has the cost of tracking state changes gone down, and has the state you must assume in parallel processing and tests decreased?

## Declarative/Functional Is the Standard for Collection Transformations [4.4]

- Definition: A pipeline such as filter → group → aggregate → sort is a serial composition of pure functions, where each stage merely takes input and returns output without rewriting the original data. Writing collection transformation processing in declarative/functional style is almost the standard practice.
- When: When writing filtering, transformation, or aggregation of collections.
- Do: Using the target language's declarative collection operations, write so that the description of the spec ("filter by period, group by category, aggregate, sort by sales") becomes the structure of the code as is.
- Check: Are separate concerns (filtering, grouping, aggregation) not entangled within one loop?

## Do Not Overapply the Functional Style [4.4]

- Definition: Trying to write everything in functional style makes code harder to read instead. Functional is also a tool, not a panacea.
- When: When you are trying to represent a problem whose essence is state transitions (workflows, UI) with immutable data. When you are stacking higher-order functions and compositions in many layers.
- Do: If the code for passing state around swells, go back to OO or imperative. Hold back on highly abstract compositions, matching the conventions of the existing code and the team's proficiency.
- Check: Did making it functional really reduce the amount of information the reader must follow?

## Basic Forms of Combination [4.4, 4.5]

- Definition: Build the structure with OO, write the logic functionally, and write the workflow control imperatively. Express domain concepts and responsibility boundaries with classes and interfaces, and write the calculations, transformations, and decisions inside them with pure functions and declarative collection operations.
- When: When concepts with state, data transformations, and procedures are mixed within one feature.
- Do: Determine the nature of the problem for each requirement, and choose a style for each nature (see the worked example below).
- Check: Is the style of each part a straightforward expression of the nature of that part's problem?

## Worked Example: Online Ordering System [4.5]

The first step in choosing a paradigm is not writing code but determining "what nature of problem" each requirement is.

| Requirement | Nature of the problem | Style chosen |
|---|---|---|
| Searching and filtering the product catalog | Transformation from input to output. Holds no state | Declarative/functional |
| Shopping cart | State as "a collection of products and quantities" + rules for adding, removing, and totaling | OO (structure). The total calculation inside is a transformation, so functional |
| Order processing (validation → inventory reservation → payment → confirmation) | The procedure and control on failure (if payment fails, release inventory and abort) are the spec | Imperative |

## Worked Example: Three Options for Discount Calculation [4.5]

Rule: Premium members get 10% off, totals over 1,000 get 5% off, otherwise no discount. All three options return the same result.

| Option | Suitable conditions | Limits |
|---|---|---|
| Imperative (chain of ifs) | The rules number 2-3 and are stable. The most direct, and anyone can read it | Every time a rule is added the ifs grow, and bugs from overlapping conditions or ordering creep in easily |
| Declarative/functional (pure function with pattern matching) | While the rules can be organized as "pairs of condition and formula". The list can be seen at a glance, and testing is easy. The most concise | Once you want to add or remove rules at runtime, or each rule needs complex accompanying processing, it no longer fits in an expression |
| OO (a type per rule + a sequence of rules) | Rules are frequently added or removed; you want tests and configuration to be independent per rule | The largest machinery |

- Threshold for choosing: the number of rules, frequency of change, and the need to be variable at runtime. The deciding factor is not the current shape of the code but the prospect of change ("How will the rules grow from here?" "Who will touch them, and how often?"). Choosing a paradigm is not a matter of preference but a design judgment about change.
- Until you can point to grounds for the prospect (it is in the requirements or the user's explanation; the rules have actually been increasing), do not build OO machinery in advance (judgment rule 1 in SKILL.md).

## Four Questions for Judgment [4.5]

1. What is the essence of this problem: modeling state and responsibilities (→ OO), transformation from input to output (→ functional/declarative), or procedure and control (→ imperative)?
2. Where will change come: if the kinds of concepts increase, OO's boundaries absorb the change; if the transformation rules increase, functional composition absorbs it
3. Can it be read: however appropriate, it cannot be maintained if no one can read and write it. Introduce styles not present in the existing code step by step
4. Can consistency be maintained: use the same style for the same kind of problem in the same codebase. Disorderly mixing is not the right tool for the right job; it is simply confusion

A constraint on yourself: if you write without deciding on a paradigm, the output drifts toward an average style (usually direct imperative code). That is not necessarily bad, but it is not the result of a design judgment. Before writing, decide the paradigm and structure, e.g., "write this transformation as a side-effect-free pipeline" or "the discount rules are expected to grow, so split them into a type per rule", and state the reason in a word in the report.

## Comparison Table [4.6]

| Paradigm | Where it applies | Advantages | Cautions |
|---|---|---|---|
| Imperative | Processing whose procedure is the spec / hardware control / performance-tuned algorithms | Direct / execution cost is easy to predict | Intent is buried in implementation details / the burden of state management falls on the reader |
| Declarative | Filtering, transformation, and aggregation of data / definition of rules and configuration | Clear intent / concise / optimization can be left to the language processor | The execution process can be hard to see and hard to debug |
| OO | Domain models / state management / structuring large systems | Direct expression of concepts / localization of change / boundaries of division of labor can be defined with types | Risk of excessive abstraction / poor at separating cross-cutting concerns |
| Functional | Data transformation pipelines / parallel processing / calculation logic | Predictable and easy to test / parallel processing is safe | Learning cost / unsuited to problems whose essence is state |

## Paradigm Selection Checklist [4.6]

This checklist is not for mechanically determining the "correct paradigm", but for putting you in a position to explain the reasons for your choice. Be able to answer "Why is this a declarative collection operation?" and "Why did you extract this into a class?"

Nature of the problem:
- [ ] Have you put into words whether the essence of this processing is "state and responsibilities", "transformation", or "procedure"?
- [ ] Have you avoided forcibly rewriting processing whose procedure itself is the spec into declarative form?
- [ ] Have you avoided writing filtering, transformation, and aggregation of collections with hand-written loops?

Provisions for change:
- [ ] Do the changes likely to come (adding concepts or adding rules) correspond to the "place that absorbs change" in the chosen paradigm?
- [ ] Is processing that has no need to hold state not written as a class that holds state?
- [ ] Is calculation logic that can be kept pure separated from side effects such as I/O?

Context:
- [ ] Can readers of this codebase read and write the chosen style?
- [ ] Is the style consistent for problems of the same kind in the same codebase?
- [ ] Did you decide the paradigm and structure yourself before writing, and can you explain the reason?
