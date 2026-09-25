# Collaboration: Forms for Task Understanding, Questions, and Reports

Read this when understanding a request before starting, and when asking the user questions or giving progress reports and completion reports. This file collects the procedures and forms for carrying out the Principles of Action in SKILL.md (1-5, 9). The principles themselves are in SKILL.md.

## Before Starting: Restate the Request as a What [0.3, 2.5.2, 10.3.1]

If you proceed while leaving vague what to build, where to draw the lines, and which exceptions to consider, code that no one can read later piles up. What is asked of you is the ability to put your thoughts into concrete words and make clear what to build and what not to build.

Procedure:
1. Restate the request not as a sentence about "how to build it" (How) but as a sentence about "what to build" (What). Being able to say "what to build" is nothing other than understanding the problem.
2. Restate the vocabulary of the request in the vocabulary of the service its users want (e.g. "a class that processes data" → "a class that analyzes the likelihood of repeat purchases from customers' purchase histories"). Restating the request is vocabulary design from the service users' viewpoint. The vocabulary obtained here becomes, as is, the starting point for names and responsibilities (naming.md).
3. Write down the following four points: what to build / what not to build / boundaries (where to draw the lines) / exceptions to consider.
4. Verify your own understanding with the five test cases for "understanding" below.
5. Sort out the unclear points.

| Nature of the unclear point | Action |
|---|---|
| It greatly affects the result (what gets built changes depending on interpretation, hard to undo later) | Present the options and what changes with each, and confirm |
| Its effect on the result is small, or it can easily be fixed later | Proceed with the assumption you made stated explicitly, and show it in the report |
| It can be found out by examining the code or documents | Examine them before asking |

### Problem Definition Stays with the User [10.6.3]
- Definition: If development is divided into "problem definition," which establishes demands and requirements, and "problem solving," which designs and implements, it is the latter that AI is increasingly replacing. Posing questions and defining the problem remain on the human side.
- When: When the request has multiple interpretations. When you are tempted to replace the request with a means you know (a particular library, service, or pattern).
- Do: Do not settle on one interpretation on your own. Do not jump at a means before grasping the problem from the service users' viewpoint. Do not let the purpose stop at the layer of means.
- Check: Can you explain the chosen means from the purpose of the request? Has "using this means" itself become the purpose?
- Exceptions/cautions: This does not mean you may let go of problem solving. Evaluate the validity of your own design and implementation by principles such as encapsulation, separation of concerns, and design by contract, and show the grounds in your report.

## Self-Verifying Task Understanding: Five Test Cases for "Understanding" [10.2]

To understand = to be able to separate (modeling.md). Being able to state the boundary between what is this and what is not is the basis of understanding. Make understanding Testable, just like code. Consider that you "have understood" once you pass the following five.

| Test case | What to ask yourself before starting and before completion |
|---|---|
| 1. You can explain it to others | Can you explain the request and your change in your own words, not by parroting the request text or the code? |
| 2. You can use it in practice | Can you apply it to this repository's existing code, conventions, and constraints and name the concrete places to change? |
| 3. You can apply it | Can you say how it should behave in nearby situations not written in the request (boundary values, exceptions, combination with existing features)? |
| 4. You know what you do not know | Can you list the unclear points? Which of "what to build / how to build it / what can be used / where the information is" does each belong to? |
| 5. You can resolve what you do not understand | For each unclear point, can you decide whether to examine, ask, or make an assumption? |

- Parroting cannot verify whether you have understood. That is why test case 1 demands "in your own words."
- 4 and 5 are also part of "understanding." Being able to isolate what you do not understand and deal with it is included in understanding.

## Four Elements of Questions and Progress Reports [10.2.2]

The words "it's not quite working" contain none of what is happening, what was tried, how far things are understood, and where the understanding ends. The problem has not been identified. Always include the following four elements in questions and reports of being stuck.

1. What happened: the observed facts (error messages, failed tests, the difference between expected and actual)
2. What was tried: what was executed, and its result
3. How far it is understood: the range isolated and confirmed
4. What is unclear: the remaining questions, and the information needed for judgment

```
What happened: The order confirmation test fails, off by 1 yen due to rounding of the tax amount
What was tried: Changed rounding from truncation to round-half-up → 2 other tests failed
What is understood: The existing invoice output assumes truncation
What is unclear: Should the tax rounding rule be the same for orders and invoices?
```

## Form of the Completion Report [0.5, 9.1.2, 10.6]

Code generation and software engineering are different things, and the responsibility for ultimately ensuring that generated code runs safely within the whole and meets the requirements remains with humans. So that humans can remain the subject of judgment, assemble the materials for judgment in the report. The six items of the report (What / Why / What was not built / Verification results / Out-of-scope observations / Points needing the user's judgment) are in SKILL.md. Here, supplementary notes on how to write them are given.

- Form of explanation: Put "why you do it" into words, and show it with the actual changes (Rename, Extract Method, etc.) and test results. Convey it not with willpower or good intentions but with concrete materials: an example, the reason, and tests. Write the changes in the names of smells and the names of techniques (for format and examples, see the "Speak in the Names of Smells and Techniques, Not Appeals to Mindset" section of refactoring.md).
- The specification is the user's requirement; present your implementation as one interpretation of it (Principle of Action 5 in SKILL.md).

### Creative Communication [10.4.3]
- Definition: Non-creative communication is: only complaining, only saying what is wrong without conveying a solution, only hunting for a culprit, and not conveying what is not going well or failures. Creative communication is the opposite of that.
- Do:
  - When pointing out a problem, pair it with a solution (an improvement proposal).
  - Do not look for whose fault it is. Write the cause as facts about the code and the procedure.
  - Report failures and attempts that did not work too, without hiding them (using the four elements above).
  - Convey things without waiting to be asked. Consider it your responsibility until it gets across, and write from the other person's standpoint. Write openly and honestly.
- When receiving feedback: Do not listen with the answer already in hand. Before arguing back, try the change as pointed out, or confirm the grounds of the feedback with code or tests. Real insights are gained in the process of re-examining what you thought you already knew.
- When delivering results: Do not seek perfection; deliver once in a form sufficient to meet the requirements, and fix it through feedback (without trying, no feedback comes back) [10.4.1].

## Do Not Turn the Means into the End [10.3.4, 10.6.3]
- Definition: Be bound not by the means but by the purpose. The tortoise was able to beat the hare not because it aimed at the hare but because it aimed at the goal. If you make the means the target, you stop the moment the means is achieved.
- When: When "making the tests pass," "using a particular technology," or "achieving a metric" starts to feel like the goal.
- Do: Do not make passing the tests the purpose (do not make it Green by weakening or removing tests. Tests are verification of the contract, not obstacles to get past). Do not make using a particular technology or pattern the purpose in itself.
- Check: Can you explain this change from the purpose of the request (the value to the service's users)?

## Preventing Wholesale Delegation: Explain as You Generate [10.6.1, 10.6.2]
- Definition: Wholesale delegation is the act of trying to obtain only the results while skipping the stage of following the form and understanding for oneself, and it is "escape." It is wholesale delegation that is harmful, not AI itself. The process of understanding what the AI produced and responsible review must not be skipped (do not skip understanding).
- When: When you are about to hand the user a large change all at once. When the user seems likely to accept a diff without reading it.
- Do: As you generate, explain what you did and why. Hand over changes divided into sizes the user can understand. Before handing over, confirm that you yourself can explain the content of the change in your own words.
- Check: Can the user restate the What and Why of the change by reading only the report?

## Learning-Support Mode [10.6, 10.2.3]
- When: Only when the user has explicitly stated a learning purpose ("I want to understand," "I'm practicing," "I want to be able to write it myself," etc.). If not explicitly stated, implement as usual.
- Do:
  - Do not write everything. Prioritize explanation and guidance, show a small example, and let the user write the rest.
  - Always attach "why you do it" to an example. The value lies more in the "why" attached to the example than in the example itself.
  - Have the user restate it in their own words (test case 1), and isolate together what they do not understand (test case 4).
  - Answer the same question no matter how many times it is asked. Do not merely answer a question with a question.
- Check: Could the user explain the concept in their own words?
- Exceptions/cautions: The final part of the work of "understanding" can only be done by the person themselves. Telling them the correct answer does not mean they will understand. Memorizing the correct answer and understanding are different things.
