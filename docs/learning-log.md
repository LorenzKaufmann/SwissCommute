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

## 2026-11-08 – Step 4: GetConnectionsAsync, timestamp converter and time zones

### Warm-up (repetition of open points)
**Q: Why must every value in a URL be encoded?**
First answer: Otherwise stations are not found; umlauts become "random values".
Assessment: 🟡 → after two rounds 🟢
Final answer: Characters like `&`, `=`, `+`, `#` have special meaning. Without encoding, input changes the
structure of the request: normal users get wrong results, attackers can inject parameters. Use `Uri.EscapeDataString` for every value.

**Q: Time zone vs UTC offset, and why DateTimeOffset?**
First answer: Mixed up time zone and offset; DateTimeOffset because it can read "+0100".
Assessment: 🟡 → after several rounds 🟢
Final answer: An offset is only the difference to UTC at a moment; it says nothing about a region (+01:00 fits Zurich, Paris, Lagos).
A time zone (Europe/Zurich) is a region with rules for which offset applies when (DST). DateTimeOffset stores the offset, so the
instant is unambiguous on every server. `DateTime` with `Local` uses the server's zone – in the cloud usually UTC –
so a 06:42 notification would only be sent at 07:42 Swiss time (2 hours late in summer), and only in production.
Note: DateTimeOffset stores an offset, not a time zone. For recurring times ("every day at 07:12") we need TimeZoneInfo.

**Q: Your build is green – is your code fine?**
Assessment: 🟡 → 🟢
Final answer: Green build = no compiler errors. Warnings showed my DTOs had fields instead of properties (all values would be null).
Green tests only prove what they check: my first client test was green with a wrong BaseAddress; only a test on the request URL found it.

### Thinking aloud before coding (new working rule)
My first attempt for GetConnectionsAsync: input (station ids, time, token), output (list, empty if none),
edge cases (encoding, server errors), approach (URL, deserialize, return).
To add next time:
- CancellationToken is for aborting a running operation (timeout, client disconnects), not for invalid input.
- `DateTimeOffset` is a value type and can never be null – no null check needed.
- Server errors are not the client's job to show to users: the client throws, the caller decides (separation of concerns).
- The URL has no offset: the API assumes Swiss local time → convert before formatting.

### Questions & my answers
**Q: An external API returns timestamps your deserializer does not understand. What do you do?**
My answer: Create a class deriving from `JsonConverter<DateTimeOffset>` and adapt `Read`.
Assessment: 🟡 Right solution, but the path to it is what counts.
To add next time: Tell it as diagnosis (test + JsonException path) → alternatives (string property, timestamp, options) →
decision → safeguard (test with recorded response). `Write` is abstract and must be implemented.

**Q: What is TDD, and did it help you?**
My answer: Write the test first, empty method, test is red, then implement until green.
Assessment: 🟡 Mechanics right.
To add next time: Red → Green → **Refactor**. Concrete benefit: the test exposed the `+0100` problem before the code ever ran against the real API.
Starting red proves the test can fail at all.

**Q: How do you know your tests would actually find bugs?**
My answer: I break the code on purpose; without the time zone conversion all three theory cases failed.
Assessment: ✅ Correct, with a real example.
To add next time: The term is mutation testing (tool: Stryker.NET). Choose inputs where wrong code gives a different result (UTC, not Swiss time).

### Learned by doing
- TDD cycle: `throw new NotImplementedException()` → red → implement → green.
- `JsonConverter<T>`: override `Read` (and the abstract `Write`); register via `[JsonConverter(typeof(...))]`.
  A converter for `DateTimeOffset` also handles `DateTimeOffset?`.
- `DateTimeOffset.TryParse` is more tolerant than System.Text.Json; use `CultureInfo.InvariantCulture` for parsing too.
  Rule: data between machines → invariant culture; display for humans → user culture.
- `TimeZoneInfo.ConvertTime(dateTimeOffset, zone)` always works – the overloads with a source zone exist for `DateTime`, which has no offset.
- Daylight saving time depends on the date, not on the offset you pass in (my "summer" case first used a November date).
- Format `HH` (24h) vs `hh` (12h); `TimeSpan` does not support `HH`.
- xUnit `[Theory]` + `[InlineData]`; attribute arguments must be constants → pass strings and parse.
- Expected values in tests should be literal; computing them with production code (`Uri.EscapeDataString`) mirrors the implementation.
- Order asserts so the first failure shows the cause (count before `First()`).
- `Assert.Equal` on DateTimeOffset compares the instant, not the offset → assert `.Offset` separately if it matters.
- `IReadOnlyList` (ordered, indexer) vs `IReadOnlyCollection` (count + enumeration).
- DTO properties stay nullable (external data can be missing); our own method parameters can be non-nullable (we decide).
- A guard clause that can never fail is misleading dead code.
- "Make invalid states unrepresentable": let the type guarantee what a check would enforce.
- `git merge master` into a feature branch vs `git rebase master` (rewrites history, needs force push).
- The API returns the next connections from the requested time (07:12 → first train 07:19).

### Open / to practise
- Send the exact error message (code + text) instead of describing it; read it first.
- If the fix seems to be "convert to DateTime", stop and rethink.
- Clean up `using`s before committing (still forgotten sometimes).
- Comments in code in English.
- Interview answers: tell the story (diagnosis → options → decision → safeguard) and name what you learned.

### Target knowledge level
- Dates & times: offset vs zone, DateTimeOffset vs DateTime, conversion, DST. Status: 🟢
- URL encoding & parameter injection. Status: 🟢
- Custom JSON converters. Status: 🟢
- TDD & test quality (mutation testing, test data choice, literal expectations). Status: 🟡
- Thinking aloud before coding (questions, edge cases, approach). Status: 🟡 – keep practising