# Learning log – SwissCommute

I build this project in pair-programming style with an AI assistant: I write the code,
the assistant explains and reviews. After every step I answer interview-style questions.
This log records my answers, an honest assessment and what I still need to work on.

Not being perfect – getting better.

## 2026-10-31 – Step 0: Working mode & Git workflow

### Questions & my answers
**Q: Why use a feature branch even for a small change when working alone?**
My answer: Separates tasks cleanly; mistakes can be fixed on the branch without affecting master ("encapsulated development").
Assessment: ✅ Core idea correct.
To add next time:
- The PR is a quality checkpoint (later: CI runs build + tests on every PR).
- master stays deployable (we will deploy from master to Azure).
- A branch can simply be thrown away.

### Learned by doing
- Before committing, check `git status`; stage specific files (`git add <file>`) instead of everything.
  (connection.json almost ended up in the docs commit.)
- Conventional Commits: lowercase description, and it must describe the *whole* change.
- `git commit --amend` creates a *new* commit (new hash), so a push needs `--force-with-lease`.
  OK on your own unmerged branch, never on master or someone else's branch.
- Files should end with a newline, otherwise the last line shows up in every future diff.
- Squash and merge → one clean commit per feature on master.

### Open / to practise
- Delete the branch after merging (forgotten once).

### Target knowledge level (Git)
Explain the feature-branch + PR workflow and why it matters, and do amend / force-with-lease safely.
Status: 🟢 mostly there

## 2026-10-31 – Step 1: Exploring the transport API

### Questions & my answers
**Q: Two files in one commit – does that contradict "one concept per commit"?**
My answer: No. A concept can span several files and still be one coherent change.
Assessment: ✅ Correct.
To add next time:
- The term is "atomic commit": one logical change, complete, nothing more.
- Rule of thumb: "Would I revert these changes together?" If yes, one commit.

**Q: Why is `feature/transport-client` a good branch name?**
(Note: the navigator gave the name away before I could answer, so I explained it afterwards.)
My answer: The prefix names the category of change; the rest says precisely what is built.
Assessment: ✅ Correct.
To add next time:
- Prefixes match Conventional Commit types; tools group branches by prefix and can attach rules to them.
- Name the result ("transport-client"), not the activity ("api-exploration").

**Q: What does the API return, and how do delay and times look?**
My answers: /locations returns a list of stations (id, name, coordinate, ...). `delay` is on `from`/`to`, 0 probably means on time.
Times look like `2026-10-31T13:08:00+0100`. `departure` is the plan, `prognosis.departure` the real-time forecast.
Assessment: ✅ All correct, and I stayed careful ("probably") where one sample was not enough.
To add next time:
- Station ids are strings, not numbers (important for DTOs).
- `delay` is in minutes; 0 = on time, null = no real-time data. Our logic must distinguish these.
- `+0100` is a UTC offset, not a time zone. The time zone is Europe/Zurich (+0100 in winter, +0200 in summer).
- `prognosis` also contains platform changes (possible future feature).

**Q: How does the API's time format differ from ISO 8601?**
My answer: ISO uses `+01:00`, the API sends `+0100` – the colon is missing.
Assessment: ✅ Correct.
To add next time: ISO 8601 has a basic format (no separators) and an extended format (with separators). The API mixes both, so strict parsers may reject it. We will verify this with a test.

**Q: What does a `+` mean in a URL query string?**
My answer: It separates the date from the summer/winter time information.
Assessment: 🟡 That describes the timestamp, not the URL.
Correct answer: In a query string, `+` means a space. The server silently receives `13:08:00 0100`. A literal plus must be encoded as `%2B`.
Lesson: Encode every value that goes into a URL, not only the ones with umlauts.

### Discovered myself
- Station names are ambiguous (Pfäffikon SZ / ZH), and the API picks a match by fuzzy search, even for a typo ("Pfïffikon").
  → Store the official station id (e.g. 8503209), not the name. Search by name, work with ids.
- /locations is a text search that returns ranked candidates; ids do not match there. /connections accepts names or ids.

### Learned by doing
- URL encoding: characters become UTF-8 bytes written as `%XX` (ä = `%C3%A4`, space = `%20`).
- `.http` files: variables (`@name`, `{{name}}`), `###` separators with request names, `Accept` header.
- First push of a new branch: `git push -u origin <branch>`.
- With squash merge, fix-up commits are fine; the final message can be edited when merging.
- Conventional Commit type is `feat`, the branch prefix is `feature/`.

### Open / to practise
- After a fix, verify the *result* (the response), not just the code. The direction was wrong twice.
- Commit messages in imperative mood ("add", "correct"), not past tense ("corrected").

### Target knowledge level
- HTTP & URL encoding: explain percent-encoding, reserved characters (`&`, `=`, `+`) and why every value must be encoded. Status: 🟡
- Dates & times: explain UTC offset vs time zone, ISO 8601, and why `DateTimeOffset` matters. Status: 🟡
- API exploration: explain why you explore an API before writing DTOs, and why we store ids. Status: 🟢

## 2026-10-31 – Step 2: DTOs for the transport API

### Questions & my answers
**Q: What do we build in this step, and why do DTOs live in Infrastructure?**
My answer: C# types that hold the data from the external API. If the API changes, only Infrastructure changes, not the Core logic – this limits the impact of an API change.
Assessment: ✅ Correct.
To add next time: Term "anti-corruption layer"; in English the impact of a change is called its "blast radius".

**Q: Why `record`?**
My answer: To recognise which files contain DTOs; value equality.
Assessment: 🟡 Value equality is right; recognising DTOs is not the reason (folder and name do that).
Correct reasons: immutability (with `init`), value equality, little code.

**Q: Which types for id, delay and times?**
My answers: `id` as `string?`, `delay` as `int?` (null = no data, 0 = on time), `DateTimeOffset?` for times.
Assessment: ✅ Types correct; 🟡 reasoning for `string`.
To add next time:
- `id` is a string because the JSON sends it in quotes; ids are identifiers, not numbers (like postcodes).
- Rule of thumb: quotes in JSON → `string`, no quotes → number.

**Q: Attribute per field (A) or JsonSerializerDefaults.Web (B)?**
My answer: A – nothing happens invisibly, so someone new to the code finds their way.
Assessment: ✅ Good reasoning. Bonus: C# properties can be renamed without breaking the mapping.

**Q: Why `LocationsResponse` and not `LocationsDto`?** (my own question)
Answer I got: `…Response` = the envelope a whole endpoint returns; `…Dto` = data objects inside it.
`ConnectionDto` vs `ConnectionsDto` would differ by a single "s".

### Learned by doing
- Map JSON systematically, from the outside in: every `{ }` → a record, every `[ ]` → `IReadOnlyList<T>`.
- DTOs mirror only the JSON. `Route` is our own domain concept and does not belong there.
- YAGNI: only map what we need. The top-level `stations` is an object, not a list. Mapping it as a list would have crashed.
- `?` on the list (`IReadOnlyList<string>?`), not on the items, when only the list can be missing.
- Never return `null` for collections → default to an empty list (`= []`).
- Record body `{ … }` with fields without a modifier = **private fields**; System.Text.Json ignores them → every value stays null, silently.
- `[property: …]` is only needed for positional records; on normal properties it is redundant.
- `{ get; set; }` makes a record mutable; `{ get; init; }` only allows setting during creation.
- Positional vs nominal records: nominal with `init` is more readable when there are many attributes.
- File-scoped namespaces; `ImplicitUsings` makes `using System;` etc. unnecessary.

### Open / to practise
- **A green build does not mean "all good" – read the warnings.** (CS0657 / CS0649 pointed to the private-field bug.)
- *Departure* vs *destination*: mixed up twice. A checkpoint has arrival/departure; the destination is where a connection ends.
- Plural of abbreviations without an apostrophe: "dtos", not "dto's".

### Target knowledge level
- Records & immutability: explain record vs class, `init` vs `set`, positional vs nominal. Status: 🟡
- JSON mapping: map a JSON structure to C# types and justify nullability. Status: 🟢
- Compiler warnings: read and understand them, know `TreatWarningsAsErrors`. Status: 🟡

## 2026-11-01 – Step 3: Typed client with SearchStationsAsync + first tests

### Warm-up (repetition of open points)
**Q: Serialize vs deserialize – which one do we do with the API response?**
My answer: Serialize = object → JSON, deserialize = JSON → object. We deserialize.
Assessment: ✅ Correct. (Works for any object and any format, not only DTOs and JSON.)

**Q: What is wrong with `{ get; set; }` on a record, and how do you fix it?**
My answer: A public setter lets anyone change the DTO; use `init` so the value can only be set once.
Assessment: ✅ Correct. Status for records & immutability: 🟡 → 🟢

### Questions & my answers
**Q (my own): Why should DTOs be immutable?**
Answer I got: A DTO is a snapshot of external data. Immutability lets the compiler prevent accidental changes
(e.g. someone turning a `null` delay into `0`), makes objects thread-safe and keeps value equality/hash codes reliable.
Changes via `with` create a copy.

**Q (my own): What does `sealed` mean?**
Answer I got: No class can inherit from it. "Design for inheritance or prohibit it." Makes intent explicit,
prevents unexpected overrides, small JIT optimisations. Replaceability comes from interfaces, not inheritance.

**Q: Why not `new HttpClient()` everywhere, and what does IHttpClientFactory do better?**
My answer: The factory manages connections, pools them and refreshes them for DNS changes. New clients use up ports;
a static client never notices DNS changes.
Assessment: ✅ Correct.
To add next time:
- Ports run out even if every HttpClient is disposed correctly: closed connections stay in TIME_WAIT.
- The factory pools the HttpMessageHandlers (the real connections), not the HttpClient instances.
- Typed client: our class receives an `HttpClient` in the constructor, not the factory itself.

**Q: How do you test an HTTP client without the network?**
My answer: A fake handler in the HttpClient returns recorded real responses; the client cannot tell the difference.
Assessment: ✅ Correct.
To add next time: The class is `HttpMessageHandler`. The fake also records the request (`LastRequest`), so we can verify the URL the client built.

**Q: Tell me about a bug your tests found.**
My answer: The BaseAddress `…/v1` without a trailing slash made the request go to `/locations` instead of `/v1/locations`.
Assessment: 🟡 Good story, cause slightly off.
To add next time:
- The Uri keeps `…/v1`; the problem is resolving the relative URL against it (RFC 3986): the last segment is treated like a file and replaced.
- Rule: BaseAddress ends with `/`, relative URL starts without `/`.
- Tell it as: what happened → cause → fix → prevention (validate configuration at startup, "fail fast").

### Learned by doing
- `GetFromJsonAsync<T>` is an extension method from `System.Net.Http.Json`: GET + status check + deserialize.
- Deserialize into the envelope (`LocationsResponse`) but return only the list – callers should not know the API's packaging.
- `response?.Stations ?? []`: null-conditional + null-coalescing; never return null for collections.
- `ArgumentException.ThrowIfNullOrWhiteSpace` (guard clause); `Uri.EscapeDataString` for every URL value.
- An `async` method returns `Task`/`Task<T>`; with `Task<T>` you return the value, the compiler builds the task.
- Primary constructors are short, but their parameters are not `readonly` (trade-off vs readonly fields).
- `Path` is a static class; `Path.Combine` returns a string. Use `AppContext.BaseDirectory` to find test data.
- Test data: `<None Update="TestData\**" CopyToOutputDirectory="PreserveNewest" />` copies every file automatically.
- Git detects renames (`TestDate => TestData`) and keeps the file history.
- xUnit: `[Fact]`, `async Task` tests, `Assert.Equal(expected, actual)` – expected first.
- Arrange–Act–Assert: object creation belongs to Arrange; Act ideally one line.
- DRY vs DAMP in tests: extract technical setup into helpers, keep what is checked inside the test.
- Tuples + deconstruction: `var (client, _) = await CreateClientAsync(...)`; `_` is a discard.
- Null-forgiving operator `!` is fine in tests, use sparingly in production code.
- Recorded raw responses also cover escapes (`\u00fc`, `\/`).

### Open / to practise
- Read warnings before saying "it builds" (CS8618, CS1998 showed up again).
- Run the tests before reporting – a test that never ran proves nothing.
- Names must not lie (`testDataFile` contained JSON content).
- Commit messages: "SearchStation**s**Async" – check names in messages.

### Target knowledge level
- HttpClient & IHttpClientFactory: socket exhaustion, DNS, handler pooling, typed clients. Status: 🟢
- Testing HTTP clients: stub HttpMessageHandler, recorded responses, verifying requests. Status: 🟢
- URL resolution: BaseAddress trailing slash and relative URLs. Status: 🟢
- Async/await basics: Task vs Task<T>, CS1998, passing CancellationToken. Status: 🟡
- Test design: AAA, DRY vs DAMP, helpers, expected-first. Status: 🟡
