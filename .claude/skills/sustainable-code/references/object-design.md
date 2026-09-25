# Object Design: Responsibility, Boundaries, Collaboration, Dependency

Read this when deciding how to divide classes and modules, where to place responsibilities, which operations to expose, and which direction dependencies point, or when extracting code whose responsibilities are mixed. The key point is not to copy the real world into the program, but to place responsibilities and dependencies by concern and confine the impact of change.

The example subject is a Breakout game made up of a ball, a paddle, and a collection of bricks (the ball bounces and removes bricks, and the paddle moves with key input. There are stages, kinds of bricks, and a score).

## Four Questions to Ask of a Design [5.intro]

If you cannot put "the sense that something has been organized" into words, you cannot design. Once you have decided on a division, answer the following.

1. Why did you divide into this unit (which concern does it correspond to)?
2. A process that touches the state of two objects: whose job do you make it?
3. On what basis did you decide the names and scope of the exposed operations?
4. When you want to add kinds or configurations, what changes, and what can stay unchanged?

## Enumerate the Concerns First; Classes Are the Result [5.1]

- Definition: A model is something made simple by removing what lies outside the concern. The very choice of what to include and what to discard in light of the purpose is design. The list of classes does not come first; organizing the concerns comes first, and the classes are decided as its result. Listing nouns from the real world and designing a model that fits the purpose are different acts.
- When: When thinking about the class structure of a new feature. When you are about to turn the nouns in a requirement statement directly into classes.
- Do: First enumerate the concerns needed for the purpose (e.g. progression/movement/control/state). Create a unit corresponding to each concern. Also re-capture the places and responsibilities from the side of the acts ("is asked to make a deposit", "verifies identity"). For things that are not essential to the purpose, such as effects and display, consider placing them outside the model.
- Check: Can you state in one line which concern each class corresponds to? Are there no classes that correspond to no concern, and no attributes that are not used for the purpose?
- Exceptions/cautions: The right answer for what to include is determined by the purpose of the software and by where it is likely to change from now on.

"Class" is used at three levels, so do not confuse them: conceptual level = a set of responsibilities / design level = a set of operations it can respond to / implementation level = code and data (the language's syntax). Make judgments about responsibility, collaboration, and dependency mainly at the conceptual and design levels, and do not make them depend on the syntax of a specific language. Judge inheritance, too, at the design level as a kind-of (is a kind of ...) relationship, and do not confuse it with the syntax of derived classes at the implementation level [5.1]. Being able to carry the same name through all three levels (not having to rephrase the vocabulary) is a strength of OO.

## Strengths and Weaknesses of OO [5.1]

OO is not a cure-all either. Compensate for its weaknesses with other paradigms (paradigms.md).

| | Content |
|---|---|
| Strength: flexible abstraction | With interfaces and abstract types, it is easy to define a common contract that does not depend on implementation details |
| Strength: affinity with GUIs | UI components that hold state and their event handling can be represented intuitively as a single object |
| Strength: separation of concerns through encapsulation | By unifying data and behavior and hiding the internals, it is easy to confine the impact of specification changes within the object |
| Strength: polymorphism | New kinds of objects can be added or swapped in without changing the caller |
| Weakness: difficulty of runtime tracing and debugging | Behavior switches at runtime through state and dynamic dispatch, making it hard to grasp the actual behavior just by reading |
| Weakness: weakness with cross-cutting concerns | It is difficult to handle processing that cuts across multiple objects, such as logging, authentication, and authorization, by dividing responsibilities alone |
| Weakness: difficulty of concurrency due to shared mutable state | Because internal state changes, mutual exclusion and debugging tend to become difficult |

## Assigning Responsibilities [5.2]

- Definition: The central task of design is deciding "whose job shall we make this?" There is no right answer about where a responsibility lives written somewhere in the real world. The designer decides.
- When: When one process touches the state of two or more objects.
- Do: Divide the process by "whose state does it change," and assign each part to the owner of that state (e.g. removing a brick is the responsibility of the collection of bricks; bouncing is the responsibility of the ball). Each object changes only its own state and does not touch the other's state.
- Check: Are there no places that directly rewrite another object's state?

## The "What Is It, in a Word?" Test [5.2]

- Definition: After assigning a responsibility, ask "What is this class's job, in a word?" If you cannot say it in a word, multiple responsibilities may be mixed. A good assignment is one in which that word is itself the name of the class.
- When: When you create a class, module, or method; when you name it; when you review it.
- Do: Write out the job in a word. If it contains an enumeration such as "does X and Y," divide it by responsibility.
- Check: Do the word and the name match? If they do not, fix either the name or the way of dividing (for details on naming, see naming.md).

## The Smaller the Scope of a Responsibility, the Better [5.2]

| Scope of the responsibility | Where to place it |
|---|---|
| One method's worth | Extract into a variable or a private method |
| Concerns the whole class | A method or property of the class |
| Goes beyond that class's area of responsibility | Move it out to another class |
| General-purpose, usable anywhere | A general-purpose class or function |

- A collection of small units divided by responsibility is simpler than one huge function. However, do not divide blindly (the three viewpoints on ways of dividing are in modeling.md).
- Scope grows. Move the responsibility's location while discerning where it belongs: expression → function → method of the owner of that data.

## Give It One Reason to Change [5.2]

- Definition: The Single Responsibility Principle is the principle that "a class should have only one reason to change," and its true value shows when the specification changes. The actual form of a violation is having many reasons to change but only one place to fix.
- When: When one function or class is growing large. When every specification change ends up fixing the same place.
- Do: Enumerate as reasons to change the parts of the specification that can change independently (e.g. physics, control, configuration, rules), and map "this change → this place to fix" one line each. Redistribute responsibilities so that one reason corresponds to one object.
- Check: Is the place to fix for each reason to change a separate object, one each?
- Exceptions/cautions: Do not misread SRP as "a rule to make classes small." Chopping things finely and mechanically does not make a good design. What to ask is "What is the reason this part will change? Is that reason one?"

## Names and Service Boundaries [5.3]

- Definition: Giving a name is declaring "we will call the concept within this range this," and drawing the boundary between what it is and what it is not. Decide the boundary as seen from the outside, and do not express the implementation in the name. If the implementation leaks into the name, the name starts lying as soon as you change the implementation.
- When: When deciding the names and exposure scope of classes and public methods.
- Do:
  - First write the code on the side that uses the service, and work backward from it to the operations each object should expose and their names. The client's concerns become, as they are, the list and names of the exposed operations.
  - Do not put internal data structures or algorithms (that it is an array, that it scans) into the name (`BrickSet`, not `BrickArray`).
  - Do not put circumstances specific to the client into the name (`collideWith`, not `collideWithGameBall`). The name of a service represents only what the service itself provides.
- Check: Does the client code avoid stepping into the other party's internal circumstances (state such as flags and coordinates)? Can the client and the name stay unchanged even if you change the internal implementation?
- When names and exposure scope are decided from the client's viewpoint, a contract is established between objects. Both collaboration and dependency design stand on this contract (for how to write contracts, see quality-gates.md).

```csharp
// before: the client steps into the other party's implementation details
if (paddle.RightPressed) paddle.X += 7;
// after: call only the client's concern as a public service
paddle.Step();
```

## Collaboration: See Association, Aggregation, and Delegation as Division of Roles [5.4]

| Relationship | Meaning | Judgment |
|---|---|---|
| Association | There is a semantic connection, and they exchange messages | Depending on how strong the relationship is, choose whether to hold it or pass it as an argument on the spot |
| Aggregation | The whole has parts (has-a) | Is there propagation of operations from the whole to the parts (reset of the whole → reset of the parts)? |
| Delegation | Hand a job you received to the party suited to it | Follow Expert and "where the role lives" below |

## Let the Holder of the Information Decide (Expert) [5.4]

- Definition: Assign a responsibility to the one that holds the information needed to fulfill it. Put information and responsibility in the same place.
- When: When you are extracting the data needed for some decision from another object and computing it.
- Do: Make the decision itself a method of the object that holds the data (e.g. the decision of whether it is hit is taken on by the brick itself, which knows its own position and size).
- Check: Are the places that extract another object's internal values for a decision gone?

## Delegation Does Not Move Where the Role Lives [5.4]

- Definition: Whether to delegate and "whose role it is" are separate questions. Delegating does not move where the role lives. An object's role is what service it provides to the outside, and it is determined independently of how it is realized internally (whether it does it itself or leaves it to someone). Example: even if Game delegates reacting to key input to Paddle, Game continues to bear the role of "accepting the player's input" toward the outside.
- When: When you are stacking delegations. When it is hard to answer "who is ultimately responsible for this process?"
- Do: For each object, be able to state separately its "interface to the outside (the services it exposes)" and its "internal means of realization (delegates)." When the place of a responsibility changes (e.g. when a "does not disappear unless hit twice" specification arrives, move the decision of whether it disappears to the individual elements), rearrange the form of collaboration.
- Check: Can you name the responsible object without tracing the chain of delegation?

## Replace Inheritance for Reuse with Composition [5.4]

- Definition: Instead of creating a base class with common processing and reusing it through inheritance, have the side that needs it hold an instance of a class that has the common processing (the composition-over-inheritance principle). The implementation technique of forwarding processing to an object in a composition relationship is delegation.
- When: When you are about to create a base class because of common processing, such as "both of them move."
- Do: Make the common processing an independent object, and have each class hold it and leave the job to it.
- Check: Can you grasp the whole picture of each class without looking at a base class?
- Exceptions/cautions: The harms of inheritance are that responsibilities are scattered between the base and derived classes so the whole picture becomes invisible, that the deeper the hierarchy, the higher the coupling, so changes to the base unintentionally break the derived classes (the fragile base class problem), and that the scope of a change's impact becomes hard to predict. On the other hand, polymorphic inheritance for swapping behavior as kinds (keeping the contract via is-a) is a different matter. The dividing line is the single point: "Are you building a hierarchy solely from the motive of wanting to reuse common processing?"

Delegation alone is sometimes not enough for cross-cutting concerns (logging, authentication, authorization, transaction management). Make AOP, decorators, and dependency injection (DI) options.

## Dependency: One Change → One Fix [5.5]

- Definition: The basis of maintainability is "one change → one fix." High cohesion = the code related to one reason to change is gathered in one place (the fix needs only one place). Low coupling = an internal change to one object does not ripple out to others (the fix stays contained). High cohesion means having one clear purpose and name and having consistent contents; loose coupling means having a simple interface and talking as little as possible with unrelated things (for the original meaning of loose coupling, see the "Represent Independent Things as Independent" section of modeling.md).
- When: When creating or evaluating dependencies.
- Do: Make each object depend only on the other party's public services (contract). Do not create dependencies between matters that are, as specifications, decided independently of each other (e.g. the laws of physics and the arrangement of bricks).
- Check: Even if you change one object's internal implementation (e.g. from an array to a different data structure), do the other objects not change by even one line?

## Think in the Order: Change-Request Scenario → What Breaks → Principle [5.5]

- Definition: It is not that principles come first and decide the design. The question "if this change comes, what breaks?" comes first, and principles provide the vocabulary and criteria for answering that question.
- When: When evaluating a design. When unsure whether to introduce an extension mechanism.
- Do: Bring in a concrete change request, identify what breaks, and then choose the remedy.

| Type of change request | What breaks | Remedy (principle) |
|---|---|---|
| Add configurations (adding stages) | A class with the configuration logic hard-coded | Supply the configuration from outside as data (OCP) |
| Add kinds (hard bricks) | A switch that branches on kind grows | Move the "answer for yourself" responsibility to each kind (polymorphism, OCP, LSP) |
| Change the input method (mouse, touch) | A higher-level class that knows the device details | Make the detail side conform to an abstraction defined by the higher level, and inject it from outside (DIP, DI). Do not force dependence on operations the client does not use (ISP) |

- Check: For each change request, have you reached the state where "you only add new classes or data, and existing classes do not change"?
- Exceptions/cautions: Introduce remedies only when the outlook for change is concrete (for the judgment rule, see "Simplicity vs. extensibility" in SKILL.md). When you turn branching into type substitution with polymorphism, which implementation is called is determined only at runtime, and you can no longer follow it just by reading.

## SOLID Principles [5.5]

| Principle | Definition |
|---|---|
| Single Responsibility Principle (SRP) | Each should have only one responsibility |
| Open-Closed Principle (OCP) | Should be open for extension and closed for modification |
| Liskov Substitution Principle (LSP) | Objects of a derived class should be substitutable for objects of the base class |
| Interface Segregation Principle (ISP) | Divide interfaces so that implementers of an interface have no methods they do not use |
| Dependency Inversion Principle (DIP) | Higher-level modules must not depend on lower-level modules. Details should depend on abstractions |

## The Law of Demeter [5.5]

A method may call only itself, what was passed as an argument, what it created, and what it directly holds (don't talk to strangers). The moment you traverse an internal structure, the traversing side gets caught up in changes to that internal structure. Complete an inquiry in one step, decision included.

```csharp
if (ball.Position.Y > screen.Height) GameOver();   // before: traverses the internal structure
if (ball.HasFallenBelow(screen.Height)) GameOver(); // after: inquires, decision included
```

## Use Patterns as Typical Forms of Collaboration [5.5]

- Definition: Design patterns and GRASP are not something to memorize as a catalog. Use them as typical forms: "for this problem, this placement of responsibilities and form of collaboration works."
- When: When you feel like using a pattern.
- Do: Use them in this order: state the problem first (e.g. there are multiple ways of bouncing and you want to switch between them), decide the placement of responsibilities (each calculation method into an independent object), and the pattern (Strategy) emerges as the result. Use pattern names in reports as vocabulary for talking about the design.
- Check: Even if you remove the pattern's name, can you explain the structure from the problem?

Of GRASP, the ones used for judgment:
- Expert: Assign a responsibility to the one that holds the necessary information
- Creator: Assign the creation of an object to the one that owns or uses it
- Low Coupling / High Cohesion: The evaluation axes of "one change → one fix"
- Polymorphism: Express branching by kind through type substitution

## Principles Are Auxiliary Lines [5.5]

- Definition: SOLID is an auxiliary line. High cohesion/loose coupling, SOLID, the Law of Demeter, design patterns, and GRASP are all vocabulary and criteria for estimating "if this change comes, what breaks," not rules to memorize and apply mechanically. Principles and patterns alike describe, from different angles, the same single thing: placing responsibilities and dependencies by changing concern and confining the impact of change.
- When: When you are about to add structure on the grounds of a principle's name.
- Do: Do not use a principle's name alone as the reason; show concretely which change breaks what, and then apply it.
- Check: Can you explain it not by "which principle you followed" but by "which change is now contained"?

## Explain the Design with a Minimal Diagram [5.6]

- Definition: Design does not end when it is decided; it functions only once it can be explained. The value of a diagram lies not in being a formal document but in being a framework for thinking and a tool for dialogue. Drawing in too much actually makes it harder to convey.
- When: When you have changed the responsibilities and dependencies of multiple classes. When presenting design options to the user.
- Do: If needed, show only aggregation, the direction of dependencies, and the extent of inheritance, using a Mermaid class diagram or the like. Narrow down the elements by asking yourself, "What is the simplest diagram that best conveys the intent?"
- Check: Can you read from the diagram whether there are no reverse dependencies and no responsibilities that have no place anywhere (e.g. the score)?

## Do Not Treat OO as Absolute [5.6]

- Definition: OO is not optimal for every problem. Write the structure in OO and the data transformations inside it in the functional style (for how to combine them, see the "Basic Forms of Combination" section of paradigms.md).
- When: When you are about to write an aggregation (e.g. the total points of the removed elements) as a collaboration of objects. The transformation from the model's state into a sequence of drawing commands, too, can sometimes be clearer when written as a transformation.
- Do: Write aggregations and transformations as data transformations (paradigms.md). What to use where: OO for structuring responsibilities and state, functional for data transformation and concurrency, data-oriented design for areas where performance dominates.
- Check: Would that process not be more straightforward written as a transformation than as a collaboration of objects?
- The judgments of choosing concerns, observing reasons to change, deciding boundaries, and arranging the direction of dependencies can be used as they are for designing the boundaries of functions, modules, and services as well.

## Vocabulary for Self-Inspection and Reporting [5.7]

Class structures can be generated instantly, but the soundness of where responsibilities live, the direction of dependencies, and names and service boundaries is not guaranteed merely by generating them. Inspect the structures you generated or changed with the following three questions. If you cannot answer, revise the structure or indicate the uncertainty in your report.

- [ ] Does this class have one reason to change (can you enumerate the reasons to change and show them mapped to the places to fix)?
- [ ] Does this dependency point toward an abstraction (does it depend only on the other party's public services, without traversing internal structure)?
- [ ] Does this name represent the client's concern (are the implementation or circumstances specific to the client not leaking)?

The boundaries of responsibility and the direction of dependencies are directly linked to ease of testing. An object whose dependencies are limited to public services can be verified on its own (testing.md).
