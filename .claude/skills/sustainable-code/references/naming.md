# Naming: Accountability, Name and Conquer, SON

Read this when naming types, methods, variables, or modules; when revisiting names; when consulted about naming; or when reviewing the names in someone else's code or your own.

## Naming Is the Core of Modeling [2.2]

- Naming is not the selection of identifiers; it is design, and it is the central act of modeling. The act of programming is **design + implementation + testing**, and source code is a model that humans can understand, machines can execute, and that can be verified.
- What humans find easy to understand is what is "closest to the model in their heads." Giving a name to each step makes the description consistent at roughly the same level of abstraction (a cooking procedure with named steps such as "cut the ingredients," "stir-fry," and "simmer" is easier to understand than one long sentence).
- Naming is the most important means of keeping consistency among the analysis model, the design model, and the implementation model.
- When concerns are separated, it is names that support the boundaries (Parser, Validator, Repository, NotificationService). If the names are sloppy, the boundaries between concerns blur. For separation of concerns itself, see modeling.md.

## Accountability [2.1]

- Definition: Write source code so that it fulfills its own accountability. Write "what you want to do (What)" rather than "how it is processed (How)." Make it so that simply reading the code conveys that the intent is clear (what it is trying to do) and that the scope of responsibility is clear (what it does and what it does not do).
- When: When writing code, when deciding a name, when you read something and cannot immediately say "what it is trying to do."
- Do: Extract judgments and calculations and give them names, so that the caller can be read in terms of What alone. Do not line up bare conditional expressions or procedures.
- Check: Can you read a few lines of the caller and state the intent and what that code does not do? Does the shape itself convey how to use it (affordance)?
- Exceptions/cautions: The problem of vague names concealing intent and responsibility appears in AI-generated code as well. Inspect your own output too, checking whether its names fulfill their accountability.

```csharp
// before: How is lined up, and the intent cannot be read
static void ChkFunc2(int y, int m, int d) {
    if (y < 1) Console.WriteLine(txt);
    else if (m < 1 || m > 12) Console.WriteLine(txt);
    else if (m == 2) { if (y % 4 == 0 && ...) ... }   // branching continues below
}
// after: readable in the vocabulary of What
bool IsValid => IsValidYear && IsValidMonth && IsValidDay;
bool IsValidDay => Day >= 1 && Day <= LastDayOfMonth;
int  LastDayOfMonth => Month switch { 2 => LastDayOfFebruary, 4 or 6 or 9 or 11 => 30, _ => 31 };
if (!date.IsValid) ui.ShowError("The date is not valid.");
```

Note: Writing with Japanese identifiers such as `日付として正しい` and `うるう年か` makes how clearly the intent comes across stand out, but in practice identifiers are basically in English.

## Criteria for Article 3, "Precise Naming" [1.2.3]

- The name expresses its (sole) job (in a word, necessary and sufficient)
- The same thing is expressed with the same name, and different things with different names
- Do not use a known name with a different meaning
- Do not use words of the problem domain with their meaning changed

By giving a name, you fix a concept, clarify its boundary, limit its responsibility, and express intent.

## Name and Conquer [2.3]

- Definition: There are two basic strategies for dealing with complexity: Divide and Conquer, which splits a large problem into small ones, and Name and Conquer, which finds what deserves attention and gives it a name. Giving a name is fixing a concept, which means deciding the boundary between the concept and everything else, that is, "what it is / what it is not." It is the act of creating order out of chaos.
- When: When there is a class or method whose role is vague (the likes of `SystemManager.DoWork(object)`). When there is something in the code or the requirements that "deserves attention" but has not yet been given a name.
- Do: Find what deserves attention, give it a name, and extract it. With the name you gave, say "the concept in this scope is called X," and decide what goes into X and what does not. Once the name settles, tacit knowledge becomes explicit knowledge and becomes understandable to third parties (both humans and AI).
- Check: Can you say, just by hearing the name, what is included in the concept and what is not?
- Exceptions/cautions: Extracted concepts may form a hierarchy (order management → order processing → order validation, price calculation, inventory check → customer validation, product validation, payment validation).

### Three Guidelines for the Level of Abstraction [2.3.3]

If the level of abstraction is too high, the concept becomes vague (`DataProcessor.Process(object)`); if it is too low, it is bound to implementation details (`SqlServerCustomerTableSelector.SelectFromCustomerTable(sql)`). An appropriate example is `CustomerRepository.FindById` / `Save`.

| Guideline | Judgment |
|---|---|
| Domain concept | Does it correspond to a natural concept of the business domain? |
| Intent of the operation | Does it express what you want to do, rather than a technical means? |
| Implementation independence | Is it free of dependence on a particular technology or implementation method? |

### Limiting Responsibility by Name [2.3.4]

- Definition: An appropriate name makes clear both what the component does and what it does not do. Giving a name limits the scope of responsibility and yields high cohesion (a state in which the contents of a single unit are aligned toward the same purpose).
- When: When adding a method or field to an existing class.
- Do: Judge whether what you are about to add falls within the scope of the name, and if it does not, place it in a unit with a different name (do not put `SaveOrder` or `SendEmail` into `PriceCalculator`).
- Check: Is any work outside the scope of the class name mixed in?

### Use the Same Vocabulary Throughout the Problem and the Solution [2.3.6, 2.7.2]

- Definition: Name and Conquer is not a technique that ends with giving names. Describing the problem in the extracted vocabulary and building the solution in that same vocabulary form one continuous whole. If you retranslate the vocabulary every time the phase changes, the meaning drifts with each translation.
- When: When deriving classes and methods from requirements.
- Do: Use the words that appear in the sentences of the requirements (customer, shopping cart, order, inventory, shipment) directly as the code's vocabulary, and express them with natural verbs (`cart.AddItem`, `checkout.Pay`, `inventory.Reserve`). Give priority to the domain terms used in the user's explanations and in existing code; do not rephrase them in words of your own.
- Check: Do the words of the requirements correspond to the type names and method names in the code? Are there any names that cannot be explained without an intervening translation?

## Curating the Vocabulary [2.4, 0.5]

- Definition: **Subroutines exist not "to group similar processing" but "to give names."** Types, methods, and variables are the vocabulary for describing a program. The important act in design is the work of creating this vocabulary.
- When: When you are unsure whether to extract a method. When descriptions of different granularity are mixed in one method. When the same set of arguments appears again and again.
- Do:
  - Extract based on "what vocabulary you want to describe it in." Going out of your way to write `if (name.Length > 0)` as `if (IsValid(name))` is for when you want to write that logic at that level of abstraction.
  - Set the level of abstraction to the granularity at which a person would describe it "most concisely" in natural language ("go to the nearest airport" is written not as lock the door, wait at the traffic light, but as go outside, walk to the nearest station, take the train bound for the airport station, walk to the airport).
  - Give a type name to a cluster of values that appear together, and give that type responsibilities (a pair of x, y → `Vector2D`. Make distance a responsibility of `Vector2D`; three points → `Triangle`; perimeter → `Triangle.Perimeter`).
- Check: Is the method body consistent in vocabulary at the same level of abstraction, so that "what it is doing" can be read quickly?
- Exceptions/cautions: For the procedure and cautions of Extract Method, see refactoring.md.

**The Joshua Tree effect**: The moment you learn a name, you become able to recognize what you could not see until then. What you do not know the name of, you cannot see; what you cannot put into words, you cannot convey. When finding problems in code and explaining them to the user, use the names of principles, smells, and techniques. Having that vocabulary is itself the power to see through problems.

## Service-Oriented Naming (SON) [2.5]

- Definition: Name = interface = service. The using module is the client and the used module is the service provider; a name in a program is the interface that the service provider offers to the client, and it is the name of the service. What matters is not "how the provider implements it" but "how the client wants to use it." The purpose (solving the customer's problem) drives the means (development).
- When: When creating a new class, method, or API. When implementation convenience shows in a name.
- Do:
  1. Write the caller's code first. Make the name that reads naturally there the service side's name.
  2. Do not expose implementation details (SQL, DB product, data structures) in names. Let the client operate in terms of business concepts.
  3. Name each method as the service the client wants (`EmailService.NotifyCustomer`, `AlertAdministrator`. Hide SMTP and templates).
  4. Use the natural vocabulary of the domain (`Deposit` / `Withdraw` / `Transfer` for a bank, `AddItem` / `Checkout` for e-commerce).
- Check: Does the client's code avoid having to write implementation details (such as SQL strings)?

```csharp
// before: implementer's viewpoint. The client writes SQL and DB details are exposed
var table = dao.ExecuteSqlQuery("SELECT * FROM Customers WHERE Id = " + customerId);
// after: client's viewpoint. It can operate in business concepts, and the implementation is hidden
var customer = repository.FindById(customerId);
```

For the viewpoint of service boundaries and contracts, see object-design.md; for the procedure of restating the user's request in vocabulary from the client's viewpoint, see collaboration.md.

## Naming Anti-Pattern Detection Table [2.6]

Use this as a list of name smells, also for self-review of your own output. These tend to appear in AI-generated code too.

| Anti-pattern | Detection signs | Problem | Countermeasure |
|---|---|---|---|
| Appending numbers | `CustomerService1/2`, `list1`, `dict1` | The difference is unclear. The boundary of responsibility is unclear, and you cannot judge which to use | Make the difference the name (`CustomerRegistrationService` / `CustomerNotificationService`) |
| Abbreviating | `CustMgr`, `ProcOrd`, `custs`, `calcTot` | Hard to read and requires guessing. Interpretations diverge | Write in complete words (`ProcessOrder`, `customers`) |
| Meaningless names | `Thing`, `Data`, `DoStuff`, `Items`, `object` everywhere. `tmpWork`, `flg`, `i2` (they do not express the contents or what the flag is for) [2.2.1] | You cannot tell what it does, and the responsibility cannot be identified. Hard to maintain | Use names that express responsibility and concrete types (`OrderValidationResult`, `List<ValidationError>`) |
| Including the type name | `CustomerList`, `OrderDictionary`, `ProductInterface`, `OrderClass` | Implementation details leak into the name. The name also changes when the implementation changes. The level of abstraction is inappropriate | Express the concept (`Customers`) or the behavior (`IOrderLookup`) |
| No consistency | A mix of `Get`/`Retrieve`/`Fetch`, `Save`/`Store`/`Persist` | High learning cost and unpredictable. Causes confusion | Use the same verb for the same operation (standardize on Find/Save). Consistency forbids mixing verbs with the same meaning (Get/Retrieve/Fetch); it is compatible with the distinction in meaning between Find (may not be found) and Get (always returns) |
| Literal translation | Using the equivalent from a Japanese-English dictionary as is ("大きさ" (size, magnitude) → always `size`) | The ranges of words in Japanese and English are almost always different, and what you want to express falls outside the range of the translated word | Think down to the nuance, and choose a word whose range of meaning includes what you want to express |

## Improving Names Step by Step [2.7.1]

1. **Analyze the current state**: Read the names one by one and say what each represents.
2. **Identify the problems**: Classify them using the detection table above and the following viewpoints. Meaningless (`DataProcessor`, `DoWork`) / too general (`Process`, `Save`, `Send`) / lacking type information (everything is `object`) / numbered (`list1`) / unclear responsibility (what data does it process?).
3. **Stage 1: Make names meaningful**: Use concrete names and types (`OrderProcessor`, `List<Order> pendingOrders`, `ProcessOrder(Order)`, `ValidateOrder`, `SendConfirmation`).
4. **Stage 2: Separate responsibilities**: Split the responsibilities that became visible by naming into units with different names (`IOrderValidator`, `IOrderRepository`, `ICustomerNotificationService`. The whole is `OrderProcessingWorkflow.ProcessNewOrder`).

When fixing names in existing code, treat it as a change that does not alter behavior, and run the tests at each stage. Separate stages that change a signature or return value (making an argument type concrete, returning a result type, and so on) into their own step as functional changes, and show them separately in the report as well.

When, during a naming consultation, it turns out that a single name cannot express the responsibility (responsibilities are mixed), propose the Stage 2 split together with the name candidates. Carry out the split only when asked to.

**Format for findings and reports**: Do not write "use a better name." Show "which name is inappropriate from which viewpoint (responsibility, level of abstraction, type)," paired with an improvement proposal.

```text
1. DataProcessor → express a concrete domain responsibility (e.g., OrderProcessor) [responsibility]
2. Process       → express the concrete processing (e.g., ProcessOrder) [level of abstraction]
3. List<object>  → use a type-safe concrete type (e.g., List<Order>) [type]
4. DoWork        → express a concrete action (e.g., ValidateOrder) [responsibility]
```

## Naming Patterns [2.7.3, 2.7.5]

### Natural English Expressions

- **Judge by consistency with the standard library**: Match predicate-like methods to the standard library of that language. In .NET, follow `Rectangle.IntersectsWith` and use `IntersectsWith`, `CollidesWith`, `ConnectsTo`. `Intersect` and `Connect` are words that can also be used as transitive verbs, and they are not grammatically wrong. The criterion is not grammatical correctness but consistency with the naming conventions of the standard library.
- **Use SVO (transitive verb + object) word order**: `ProcessOrder`, `ValidateInput`, `CreateOrder`. Do not copy the Japanese "noun + verb" word order (`OrderProcess`, `InputValidate` are ×).
- **Add the third-person singular s**: `ContainsPoint`, `IntersectsWith` (`ContainPoint` is ×).
- **Metaphors**: Metaphors such as Factory, Observer, Strategy, Pipeline, and Cache aid understanding, but make sure they do not impair the meaning in the domain. Use a pattern name only when the intent of that pattern matches the implementation.

### Variable and Field Names

| Pattern | Good example | Bad example | Reason |
|---|---|---|---|
| Express intent | `customerCount` | `count` | Clear about what is being counted |
| Avoid type information | `customers` | `customerList` | Expresses the concept, not an implementation detail |
| Avoid abbreviations | `calculatedTotal` | `calcTot` | Clarify intent with complete words |
| Avoid negative forms | `isValid` | `isNotInvalid` | Positive expressions are easier to understand |
| Use the context | `name` (inside Customer) | `customerName` (inside Customer) | Redundant qualifiers are unnecessary within a class |

### Method Names

| Pattern | Good example | Bad example | Reason |
|---|---|---|---|
| Express the action | `CalculateTotal()` | `GetTotal()` | Clear that a calculation is performed |
| Make side effects explicit | `UpdateAndSave()` | `Update()` | Does not hide that there are multiple actions |
| Suggest the return value | `FindCustomer()` | `GetCustomer()` | Suggests the possibility of not being found (null) |
| Use question forms | `IsEmpty()` | `CheckEmpty()` | Naturally expresses a Boolean return value |
| Consistent verbs | `Create/Update/Delete` | `Create/Modify/Remove` | Keeps operations consistent |

`UpdateAndSave()` is an example of showing multiple actions in the name instead of hiding them; it does not exempt you from the Single Responsibility Principle. First consider whether it can be split into `Update()` and `Save()`, and only when there is still a reason to combine them into one, show that honestly in the name.

### Class Names

| Pattern | Good example | Bad example | Reason |
|---|---|---|---|
| Express the responsibility | `CustomerValidator` | `CustomerManager` | The concrete responsibility is clear. Manager tends to make the responsibility vague |
| Suffix indicating the role | `OrderRepository` | `OrderData` | Indicates the data-access role |
| Domain terms | `ShoppingCart` | `ItemContainer` | Uses the terms of the business domain |
| Avoid implementation details | `CustomerRepository` | `CustomerDatabase` | Expresses the role, not the implementation technique |

- Keep the vocabulary of role suffixes consistent: presentation layer `~Controller`/`~ViewModel`, business layer `~Service`/`~Engine`, data layer `~Repository`/`~DataAccess`, creation `~Builder`/`~Factory`/`~Generator`, transformation and processing `~Processor`/`~Transformer`/`~Handler`, validation and judgment `~Validator`/`~Checker`, results and settings `~Result`/`~Configuration`/`~Settings`. If the existing code has its own style of suffixes, give it priority [2.7.5].

## Naming Consistency Checklist [2.7.4]

Use this both for self-review of your own code and for reviewing other people's code.

- **Expression of intent**: Can you tell what it does just by looking at the name? / Are Why and What expressed? / Are the details of How hidden?
- **Clarity of responsibility**: Does it express a single, clear responsibility? / Is its boundary clear? / Is anything outside the responsibility included?
- **Domain fit**: Does it match the terms used by domain experts? / Does it properly express business concepts? / Does it express business value rather than technical details?
- **Consistency**: Is the same name used for the same concept and different names for different concepts? / Are naming rules unified across the whole project?
- **Discoverability**: Can other developers find it easily? / Is it a name that can be guessed? / Does it avoid abbreviations and cryptic expressions?

## Minimal Naming Convention Template [2.7.4]

When the repository has no naming conventions and new ones need to be decided, first infer the following from the style of the existing code (propose to the user only what cannot be inferred). Make the conventions take a form that "has a basis for judgment / can be enforced mechanically (formatters, linters) / has few exceptions."

| Item | What to decide |
|---|---|
| Case | The scope of `PascalCase` / `camelCase` / `snake_case`. First follow that language's standard conventions (e.g., in C#, classes, methods, and constants are PascalCase and variables are camelCase; in Java/JavaScript, methods and variables are camelCase; in Python, methods and variables are snake_case; constants in Java/JavaScript/Python are UPPER_SNAKE) [2.7.5] |
| Boolean | Rules for prefixes such as `is` / `has` / `can` |
| Collection | Rules for plural and singular forms |
| Units and constraints | Criteria for including `ms` / `count` / `id` and the like in names |
| Asynchrony and side effects | Do not betray the caller's expectations, through `Async` and the choice of verbs |

## Improve Names Continuously [2.8]

- Naming does not end once decided. Improve names as your understanding deepens.
- If, partway through implementation, you notice that a name and the actual job have diverged, fix the name at that point (for how to proceed with Rename, see refactoring.md). Finding missing names during a review is a sign that the model has deepened (modeling.md).
- A vague name allows the reader too broad an interpretation and makes divergences between intent and boundaries harder to see. A good name is a shared context that conveys intent and the boundaries of responsibility to both humans and AI, and it is a breakwater against increasing comprehension debt.
