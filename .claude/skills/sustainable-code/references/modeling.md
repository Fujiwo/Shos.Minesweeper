# Modeling: Intent and Noise, Ways of Dividing, Correspondence with the Domain

Read this when capturing requirements before starting, when deciding how to divide classes and functions, when mapping domain concepts onto code, and when evaluating whether code is good or bad.

## A Model = Something Made Simple by Removing What Lies Outside the Concern [3.1.1]

- Definition: A model is something that has a purpose, extracts only the parts that serve that purpose, and is made simple by removing what lies outside the concern (a map draws only roads, stations, and landmarks). Conveying everything in detail is different from conveying things appropriately.
- When: When deciding a class's attributes or arguments, or the items a process handles.
- Do: Do not give it attributes that are unneeded in light of the system's purpose and the user's viewpoint (do not give the employee class of an employee directory a "favorite food").
- Check: For each attribute and each item, can you answer "Is this needed for the purpose?"
- Exceptions/cautions: With a request like "create an employee class", the agent tends to line up a full set of plausible real-world attributes (date of birth, address, phone number, emergency contact...). Whether something is needed can be decided only by whoever knows the system's purpose. After generating, check your own output against the purpose and delete attributes that have no basis. When you do not know the purpose and cannot judge, state your assumptions explicitly or ask.

## Intent and Noise [3.1.3]

- Definition: What source code should express is intent. Anything written other than the intent is noise to the model. **Goodness or badness does not reside in the code itself. It resides in how well the intent and the code correspond.** Therefore, when evaluating code, ask about the intent first.
- When: Before writing code, and before starting a review or self-review.
- Do: First, state concisely "What is the intent of this code?" Next, compare it with the code and see how much is written that is not included in the intent.
- Check: Can you state the intent in a word? (If you can say "confirm the order", there is a model. If only a procedure comes out, like "first read from the DB, then run a loop...", either there is no model or the code is not written according to the model.) If you cannot state the intent, there is no model, before any question of review.
- Exceptions/cautions: The same goes for names. A name directly reflects the intent, and a name removed from the intent becomes noise (for naming, see naming.md).

```csharp
// Intent: "Output every employee in the employee list to the screen"
for (int index = 0; index < employees.Count; index++)   // int, index, <, Count, ++, [index] are
    Output(employees[index]);                           // not in the intent = noise
employees.ForEach(employee => employee.Output());      // less noise
```

However, if the intent is "set the integer index to 0 and, while it is less than Count, output the index-th element while incrementing it", then the for loop is better. `l.ForEach(x => x.Func(s))` is also good if the intent is "Func every x in l with s".

## S/N Ratio and Declarative Description [3.1.4]

- Definition: The ratio of intent to noise is called the S/N ratio (signal-to-noise ratio). The intent is the signal; everything other than the intent is noise. Declarative style does not write "how to do it", so it has little noise. Because the writer writes only the intent, the reader needs to read only the intent.
- When: When procedural description such as loops, temporary variables, and index manipulation fills the space around the intent.
- Do: If the intent is a declaration of a result, write it declaratively and hide the procedure (for "do something 10 times", consider a form that can be written like `10.Times(DoSomething)`). Do this within the range of declarative means that the language and existing libraries provide; do not create new extension methods or helpers just for this purpose, unless they are already part of the existing conventions.
- Check: Does each remaining piece of description correspond to a word in the intent?
- Exceptions/cautions: Declarative style is not always superior. Declarative description fit only because the intent happened to be a declaration; if the procedure itself is the intent, imperative style is more faithful to the intent. What matters is that the nature of the intent and the form of the description match. For how to choose, see paradigms.md.

### Two Conditions of a Pure Model [3.1.4]

- Definition: A model being pure means it satisfies two conditions: (1) nothing other than the intent is written, and (2) the intent is written out completely. If you pursue only the elimination of noise, you see only (1).
- When: After trimming noise, and after writing processing that is meant to be general-purpose.
- Do: See whether part of the intent has been replaced by a fixed value or a specific type. If the intent is "filter some collection by some condition" but the element type is fixed to `int` and the condition to "multiple of 5", it is not as intended. Make it a form that receives the type and the condition and does only the filtering.
- Check: Does every element of the intent's sentence ("some collection", "some condition", "filter") appear in the code?
- Exceptions/cautions: If the intended model can be written out completely in the language, no comments are needed. Write whatever cannot be written out as a necessary evil. What and How are easy to write in a language, but Why often cannot be written out fully (for how to handle comments, see refactoring.md). Adding "by some condition" is because it is included in the intent; it does not mean you may add generalization that is not in the intent (simplicity.md).

## Ways of Dividing [3.2.1]

- Definition: The basis of resolving complexity is dividing. 100 lines × 100 is simpler than a 10000-line program. However, if the way of dividing is bad, the 100 parts become entangled and more complex than the original 10000 lines. How to divide so that it is simplest is where skill shows.
- When: When deciding the boundaries of functions, classes, modules, and layers. When a single method mixes concerns of different levels of abstraction and granularity, such as judging business rules, splitting strings, and saving to a database.
- Do: Divide so that each divided part handles a single problem, one problem is not scattered across multiple parts, and the boundaries are sharp. Make the boundaries sharp with interfaces (the result is low coupling and high cohesion). Choose the unit of division (function, class, aspect, layer, MVC, component, fixed part and variable part) by the kind of concern you want to separate.
- Check: Compare ways of dividing from the following three viewpoints: **understandability / complexity when a change occurs / ease of testing**.
- Exceptions/cautions: Procedural style can divide only by units of processing. A single paradigm alone struggles to handle various kinds of separation (paradigms.md).

### To Understand = To Be Able to Separate [3.2.1]

- Definition: "To understand" means "to be able to separate". It means being able to say "this and this are different problems", and knowing "the boundary between what is this and what is not". What you cannot separate, you do not yet understand.
- When: When you are stuck on a design. When you cannot find a way of dividing.
- Do: Suspect that you cannot find a way of dividing perhaps because you do not yet understand the subject, and before proceeding with implementation, go back to understanding the subject (requirements, domain, existing code).
- Check: Can you enumerate the problems that make up the subject and state the boundary of each? For how to use this in self-verifying task understanding, see collaboration.md.

## Viewpoints and Models [3.2.2]

- Definition: Even for the same subject, when the viewpoint changes, the model changes (differences in paradigm, differences in height, differences in angle, AsIs looking at the problem and ToBe looking at the solution). However, analysis, design, and implementation are not separate models. **There is one model; only the viewpoint differs.**
- When: When moving from the vocabulary of analysis to design and implementation. When a single loop houses counting, judging, and output together.
- Do: Change the viewpoint and compare how the concerns divide (one loop → make enumeration, processing, and output independent parts, and give each a name). Keep the vocabulary used in analysis in design and implementation too.
- Check: Do the names from the analysis model remain in the design model and in the code?
- Exceptions/cautions: Beautiful source code is not unique. But there are criteria. Understandability means not diverging from the model the reader understands.

### Represent Independent Things as Independent [3.2.2]

- Definition: Loose coupling = representing independent phenomena as independent. The x component and y component of a vector are mathematically independent, so it is natural for them to be loosely coupled in a program too. This is not making things loosely coupled "as a programming technique". It is simply that coupling things that are inherently independent moves away from the model in people's heads and makes things harder to understand.
- When: When you are unsure whether to cut a coupling, or conversely, when you are about to add a mechanism "for the sake of loose coupling".
- Do: Decide whether to couple by whether the subjects themselves are independent.
- Check: Does that coupling (or separation) match the reader's model? For judging the direction of dependencies, see object-design.md.

## Mapping the Domain onto Code [3.3.1]

### BCE and Frequency of Change

| Role | Content | Frequency of change |
|---|---|---|
| Boundary | The boundary of the system. Points of contact with screens and external systems | High |
| Control | Business logic concerned with control | Medium |
| Entity | The substance of the system (persistent data) and the behavior related to it. Has behavior independent of the environment | Low |

- Definition: If you separate things with different frequencies of change, you can localize changes (screen layouts change frequently, but the business rule "you cannot withdraw more than the balance" rarely changes).
- Do: Give an Entity only its own behavior, knowing neither the screen nor the DB (a deposit account knows only how to increase and decrease its balance correctly). Do not put display concerns and data concerns in the same class.
- Check: Is a Boundary class holding onto a collection of Entities or business rules?

### IOP (Inside-Out Principle)

- Definition: Design from the inside out. Design the model (Entity and Control) first, and design the user interface later. If you start from the screen, the screen's convenience seeps into the model.
- Do: For a new feature, start writing from the domain's types and behavior, and connect the UI and input/output last.

### Do Not Copy the Real World As Is

- Definition: If you take "map the real world directly onto objects" literally, you will fail. Rather than copying the real world as is, abstract it into IT concepts according to the system's purpose.
- When: When creating classes from a description of the business or an existing manual workflow.
- Do: Do not copy the paper-based practice (customer ledger, bank clerk, calculator, cash book); keep only the concepts needed from the viewpoint of the system's concern (correctly managing account balances): customer, deposit account, transaction history. The bank clerk and the calculator are outside the concern, and the calculator's computation is just addition to a computer.
- Check: Is each class a concept needed in light of the system's purpose?

**Layer division**: If BCE is a vertical division, layer division is a horizontal division by level of abstraction (application layer / framework layer / foundation library layer). Dividing by level of abstraction changes reusability.

## Conditions of a Good Model [3.3.2]

| Condition | Judgment |
|---|---|
| Once And Only Once | No duplication. A change for one reason is handled by a fix in one place |
| Simple | The degree of complexity does not exceed the range people can understand. It is a naturally expressed model |
| Ease To Understand | It is easy to understand. Names are precise |
| Ease To Change | It is easy to change and easy to extend |
| Ease To Test | It is easy to verify. It is separated so as to be easy to verify |

- A good model is a highly maintainable model. Functionality, usability, and performance are outside the concern in this context and are handled by models from other viewpoints.
- Check whether it is "a naturally expressed model" by drawing it as a diagram. Are the diagram and the code equivalent models? A model that is hard to explain by drawing a diagram is also hard to explain in code. A rigorous diagram is not needed; a sketch is enough. Choosing which diagram to draw is itself a separation of concerns that decides "what do I want to talk about right now".
- Inspect it with "What is this class's job, in a word?" (for details, see object-design.md).

## Why→What→How [3.4.1]

- Definition: Think in the order: why you build it (Why) → what you build (What) → how you build it (How). The purpose drives the means. "Just slap some name on it and build it without deciding what to build" is the opposite.
- When: When starting to build a feature, and when writing a single method.
- Do:
  - Do not start thinking from How, as in "first run a loop...". State what you are after as a What (the average is the total divided by the count), and further decompose that What into Whats (the total is everything added together). Subroutines emerge naturally in the course of decomposition (for a code example, see Average/Sum in refactoring.md).
  - Think "So, in the end, what is it that needs to be done here?" When you decompose by What, you naturally stop creating nested loops.
  - Precisely because generating How is fast and cheap, decide the What before writing, and step back up one level to ask whether that What is right in light of the Why. It is not uncommon for a different means to fit the purpose better.
- Check: Does each method in the code you wrote correspond to one step of the What decomposition? Can you say what this feature is needed for? Verify that it meets the requirements with integration tests, and that it matches the design with unit tests.
- Exceptions/cautions: The feature-level What and Why belong to the user's problem definition. The agent restates and presents them, and does not finalize them on its own (collaboration.md). The decomposition of What inside a method (average → total ÷ count) is done by the agent itself.

## Worked Example: Revising a Model [3.4.2]

Requirement: "Clicking two points with the mouse adds one line segment."

- **Situation**: Read straightforwardly, "the main window holds a collection of line segments". This works.
- **Judgment**: Even though it works, stop and ask "Is it really like this?" What the user is drawing is not the contents of the window but a drawing, and the window is merely what renders it. A line segment is one kind of shape on the drawing. → Extract "drawing", so that the drawing holds the line segments and the window renders the drawing.
- **Reason**: The original model had the Boundary (the window) holding onto the Entity (the collection of line segments). The display concern and the data concern were separated.
- **Names**: Revision proceeds by adding names. The addition of the names drawing and shape means concepts were extracted. Revision, in many cases, is finding names that were missing.
- **What not to build**: If at that point there are only line segments, do not put the "shape" abstraction into the design. Introduce it when the kinds of shapes increase (simplicity.md).
- **Result**: The vocabulary of analysis (drawing, shape, line segment, render) becomes the vocabulary of the code as is, and `drawing.Draw` becomes a single line that writes almost directly the intent "render all the line segments it holds (after the kinds of shapes increase, all the shapes)".

Takeaways: The first analysis model is a hypothesis, and just because it works does not mean it is correct. Revision proceeds by adding names. Carry the vocabulary through from analysis to implementation.
