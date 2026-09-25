---
name: sustainable-code
description: If user's messages are in Japanese and "sustainable-code-jp" is available, use it; otherwise,  use "sustainable-code". Decision criteria for writing "Sustainable Code" that both humans and AI can keep understanding and changing. Use, in any programming language, for work that writes, changes, or evaluates source code (design, implementation, naming, testing (TDD), refactoring, code review/self-review, bug fixes, performance improvement; including design consultations before coding; SQL and shell scripts count as code). Applies the Seven Articles, Name and Conquer, Service-Oriented Naming (SON), Testable design, Think Simple (YAGNI, design by subtraction), code smells and techniques, and contracts and guard clauses. Use it even for small fixes (apply it lightly). Do not use it for work that does not touch code (fixing typos in documents, git-only operations, environment setup, questions about library usage, only explaining or reading code, only changing config values, only creating or editing CI/build config files).
---

# Sustainable Code

Bracketed section numbers such as [1.2] in headings and text are source citations indicating section numbers in the original text; they do not need to be consulted.

## Premise

- Beautiful code is code that is easy to extend and maintain. The center of gravity of development is reading, and the code an agent writes is "someone else's code" from the start. [1.1, 0.1]
- AI output without constraints drifts toward average designs, adds complexity, and piles up comprehension debt (code that works but that no one can explain) at generation speed. [0.2, 0.4, 8.6]
- The supreme value is Think Simple, supported by three moves: "divide boundaries and name them / express intent as a What / put it in a verifiable form." This axis of judgment, the details of the premise, and the glossary are in [foundations.md](references/foundations.md). [8.1, 10.5]

## Principles of Action

1. **Give priority to the reader.** Write your own code as someone else's code from the start. [0.1]
2. **Leave no comprehension debt.** Do not ship code when you cannot explain why you wrote it that way. Attach an explanation of the What and the Why to every change. [0.2, 9.6, 10.6]
3. **Put into words first what to build and what not to build.** Before writing, restate the What in your own words. Ask for confirmation only when ambiguity would greatly affect the result. Otherwise, proceed with your assumptions stated explicitly, and show what is not known within the report. Do not settle the problem definition on your own. [0.3, 3.4, 10.2, 10.6]
4. **Do not drift into average output.** Impose the Seven Articles and Think Simple on yourself as constraints. [1.5, 4.5]
5. **Responsibility stays with the user.** Present the material for judgment, the trade-offs, and the verification results. Do not treat your own output as the specification. [0.5, 9.6]
6. **Be aware of addition and subtract.** Remove abstractions, patterns, and provisions for the future that were not requested. [8.6]
7. **Solve the problem while it is simple.** Do not add constraints or obstacles that are not in the problem statement. [8.2]
8. **Work in small steps and always keep it Green.** Run the build, static analysis, and the relevant tests at every step, and run all tests before the completion report. Do not move on to the next step in a broken state. [7.1, 9.4]
9. **Speak in vocabulary, not appeals to mindset.** Make findings, reports, and self-inspection concrete, using the names of smells and techniques (not "make it cleaner" but "three responsibilities are mixed, so extract them"). [7.9]

The steps before starting and the forms of questions are in [collaboration.md](references/collaboration.md); the form of the completion report is in "Scaling Application to the Size of the Work" below.

## Scaling Application to the Size of the Work

For classification, give the following criterion priority over the examples in parentheses: if neither a design decision nor an interpretation of the specification is needed, it is a small change; if either is needed, it is work involving judgment. Example: merely adding one discount rate in the same form as the existing discounts is a small change. If you need to decide the rounding method or what it applies to, it is work involving judgment even if the number of lines is small.

- **Small change** (a fix of a few lines, a fix for a bug whose cause and remedy are both obvious, an addition that merely traces existing code): apply only the "Self-Check" at the end, and do not read the references. However, for a bug fix when test infrastructure exists, add one test that reproduces the defect, confirm it is Red, then fix it and make it Green (when the expected value cannot be determined from the specification, read "Bug Fixes" in quality-gates.md).
- **Work involving judgment** (adding a feature that decides new behavior, design decisions, investigating the cause of a bug, refactoring, review, test design, naming consultations, performance improvement): read the "Must read" in the table below, and read the "Read if needed" only when the condition in parentheses applies. When section names are specified, read only those sections.

| Work | Must read | Read if needed |
|---|---|---|
| Implementing a new feature | "Before Starting" in [collaboration.md](references/collaboration.md), "Test-First" and "Designing Test Cases" in [testing.md](references/testing.md) | [modeling.md](references/modeling.md) and [object-design.md](references/object-design.md) (when creating a new class or module, or moving responsibilities between existing units), [paradigms.md](references/paradigms.md) (when writing branches by kind or condition, or transformations and aggregations of collections), [simplicity.md](references/simplicity.md) (when newly introducing interfaces, base classes, generics, design patterns, configuration items, or extension points), "Contracts and Guard Clauses" in [quality-gates.md](references/quality-gates.md) (when writing argument validation or preconditions) |
| Bug fix | "Bug Fixes" and "Guard Clauses" in [quality-gates.md](references/quality-gates.md) | "Adding Tests to Existing Code After the Fact" in [testing.md](references/testing.md) (when there is no foothold for placing a reproduction test), [refactoring.md](references/refactoring.md) (when the cause lies in the structure) |
| Design consultation (including consultation before writing code) | [modeling.md](references/modeling.md), [object-design.md](references/object-design.md) | [paradigms.md](references/paradigms.md) (when deciding whether to write it imperatively, declaratively, OO, or functionally), [simplicity.md](references/simplicity.md) (when newly introducing interfaces, base classes, generics, design patterns, configuration items, or extension points) |
| Naming | [naming.md](references/naming.md) | [object-design.md](references/object-design.md) (when a mixing of responsibilities is found) |
| Adding or designing tests | [testing.md](references/testing.md) | — |
| Refactoring | [refactoring.md](references/refactoring.md) | [testing.md](references/testing.md) (when there is no safety net), [naming.md](references/naming.md) (when renaming is the main move), [object-design.md](references/object-design.md) (when changing where responsibilities live, such as splitting a class or moving a responsibility) |
| Review (when requested by the user) | "Use the Seven Articles as Review Criteria" in [quality-gates.md](references/quality-gates.md), "Code Smells" and "Smell → Technique Table" in [refactoring.md](references/refactoring.md) | [testing.md](references/testing.md) (when it includes tests), [simplicity.md](references/simplicity.md) (when the code under review newly introduces interfaces, base classes, generics, design patterns, configuration items, or extension points), [foundations.md](references/foundations.md) (when you cannot state, with grounds, whether an item of the Self-Check passes or fails) |
| Reviewing your own changes (an implicit self-review, not requested) | — (use the "Self-Check" at the end) | the checklist in "Design by Subtraction" in [simplicity.md](references/simplicity.md) (when you have added abstractions, patterns, or provisions) |
| Performance improvement | "Technical Debt and Performance" in [quality-gates.md](references/quality-gates.md) | [testing.md](references/testing.md) (when the target has no tests pinning down its behavior) |
| Ambiguous request (what to change and how cannot be identified; e.g., "fix it up nicely") | "Before Starting" in [collaboration.md](references/collaboration.md) | — |
| Learning support (when the user explicitly states a learning goal) | the learning-support mode in [collaboration.md](references/collaboration.md) | the reference suited to the subject |
| Questions and progress reports | "Four Elements of Questions and Progress Reports" in [collaboration.md](references/collaboration.md) | — |

- Read mentions of other references inside a reference only when that topic is needed for the judgment at hand. Do not read them in a chain.
- For work that changed code, write the completion report with the following items (for a small change, only the applicable items are enough; examples and supplements are in collaboration.md).
  - **What**: what you changed. Write preparatory refactoring and functional changes separately
  - **Why**: why you wrote it that way. The design you chose, and the trade-offs against the options you discarded
  - **What was not built**: abstractions and provisions you did not include because they were not requested. The assumptions you made
  - **Verification results**: the builds, static analysis, and tests you actually ran, and their results. Also write failed attempts and what you could not verify
  - **Out-of-scope observations**: smells found outside the scope of the change, by the names of the smells
  - **Points needing the user's judgment**: interpretation of the specification, trade-offs between readability and performance, and so on
- Write the results of a requested review with each finding in the format of refactoring.md (`<location>: <smell name> (<evidence of the symptom>) → <technique>`), adding the name of the relevant Article of the Seven Articles and a proposed improvement.

## Order of Precedence

1. Explicit instructions from the user
2. Instructions for agents, such as CLAUDE.md
3. The repository's coding conventions and linter settings
4. The conventions of the existing codebase (Article 6, "Consistent Rules")
5. This Skill

If a higher-ranked rule conflicts with this Skill, follow the higher-ranked one. When you judge that such a rule is increasing complexity, only point it out with your reasons; do not break it on your own. Do not bypass a form or a convention without trying it (giving up at "Shu" (following the form) is not "Ri" (leaving the form) but "escape"). [9.2, 10.1]

## Restraint in the Scope of Change

- Do not refactor outside the requested scope. Limit yourself to reporting the smells you notice, by the names of the smells.
- Do **preparatory refactoring** to make the requested change straightforward only when it directly concerns the target of the change. Do not change behavior, confirm Green, keep its steps separate from the functional change, and show it separately in the report as well. [7.6]
- Do not substantially rework code that has no tests. First add tests that pin down the current behavior, or check with the user. [6.4, 7.1]
- Commit or create branches only when the user has permitted or requested it. Otherwise, keep diffs small, report what you changed at every step, and keep things in a state you can roll back. [7.1]
- Do not carry out the points for which this Skill asks for the user's confirmation (e.g., the interpretation of the specification is split, an optimization that lowers readability, changing a public API or public names, introducing a test framework or a tool) until you have that confirmation. While waiting, proceed only with the parts that can be rolled back and that no one would object to, and list the points awaiting confirmation under "Points needing the user's judgment" in the completion report.

## Alternatives When Something Cannot Be Done

- **Tests are not wanted**: do not write tests. Keep the structure testable, proceed with types, assertions, guard clauses, and confirmation by running the code, and report which verifications you did.
- **There is no test infrastructure**: check with the user before introducing a test framework. Until you have checked, proceed with types, assertions, guard clauses, and confirmation by running the code, and report which verifications you did (details in testing.md).
- **There is no measurement tool**: measure what you can, and if even that is not possible, propose a way to measure. Do not do optimizations that lower readability based on guesswork alone (details in quality-gates.md). [9.5]

## Judgment Rules for When Principles Conflict

1. **Simplicity vs. extensibility**: abstract only when the prospect of change is concrete. Concrete means you can point to grounds (it is in the requirements or the user's explanation, a second implementation has actually arrived [8.3], the same branch has already appeared in a second place [1.2, 7.2]). "It might be supported in the future" alone is not enough. "An abstraction introduced after the need becomes visible fits its shape; an abstraction introduced before the need becomes visible is a guess." [8.3]
2. **The economics of abstraction vs. YAGNI**: the effort of writing is no reason to avoid abstraction (the cost of writing has become nearly zero, but the cost of reading has not come down). Actively introduce abstractions that lower the cost of reading (Extract Method, naming a concept, giving a type to a data clump). Do not introduce abstractions that only increase what must be read (an interface hierarchy with only one implementation). [9.6, 7.4, 8.3]
3. **Testability vs. YAGNI**: making time, I/O, and external systems replaceable is not a YAGNI violation. Test code is the first client, and that client already needs the replacement. [1.2.7, 6.1.1]
4. **Reducing comments vs. accountability**: express the What and the How with names and code, and leave only the Why in comments. For any decision that sacrifices readability, always leave the Why. [7.5, 9.5]
5. **Readability vs. performance**: first write code that works correctly → measure → optimize only the bottleneck, locally. [9.5]
6. **Eliminating duplication vs. wrong commonization**: determine "is it the same intent?" before gathering it into one place. Do not merge code that merely happens to look similar. [1.2, 7.2]
7. **Declarative vs. imperative**: imperative if the procedure is the spec, declarative if the result is the spec. [4.2]
8. **Simple vs. easy**: do not avoid declarative expressions on the grounds of unfamiliarity. Consider whether the team can read it and whether it is consistent with existing conventions, and introduce styles not already present gradually (do not keep avoiding them on the grounds of unfamiliarity). [8.3, 4.5]
9. **Inheritance vs. composition**: do not use inheritance whose only purpose is reusing common processing. Inheritance for polymorphism (is-a and honoring the contract) is allowed. [5.4.3, 5.5.2]
10. **Numerical criteria**: 9 statements is the guideline of Article 5, based on the chunks of information a person can handle at once. Nesting of 2 levels (control structures such as if and for nested up to two deep; however, extract the inner part of a double loop into a named method [8.4.2]), 20-line methods, and 3 arguments are forms imposed on beginners at the first stage (provisional safety fences). The numbers vary with the target and the situation, and a stricter value (nesting up to one level) may be better in some cases. First try it according to the form; exceed it only when you can explain in your own words that exceeding it is more readable in light of its intent (containing complexity), and when you do exceed it, write one line in the report giving the reason. [1.2, 7.6, 9.2, 10.1]
11. **Differences between languages**: apply the principles within the range of means available in the target language and the existing code. Do not force in mechanisms the language does not have.

## Self-Check (for every change, before completion)

- [ ] Express Intent: is the What written, with few descriptions other than the intent (noise)?
- [ ] Single responsibility: can you state each unit's job "in a word"? Is that job not leaking elsewhere?
- [ ] Precise names: do names express the job neither more nor less, with the same name for the same thing and different names for different things?
- [ ] Once And Only Once: is the same intent not duplicated?
- [ ] Precise methods: are they consistent in descriptions at the same level of abstraction, with a natural granularity?
- [ ] Consistent Rules: does it match the conventions of the existing code?
- [ ] Testable: is it in a form whose correctness can be confirmed? Are the tests and build Green?
- [ ] Subtraction: have you not added abstractions, patterns, or provisions for the future that were not requested?
- [ ] Subtraction: have you not complicated the problem by adding constraints that are not in the problem statement?
- [ ] Explanation: can you explain the What and the Why of this change in your own words?
