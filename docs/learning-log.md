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
