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