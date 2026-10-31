# Architecture decisions

## 001 – Station IDs instead of Station names
**Context:** Using Station names instead of Station IDs does not lead to errors with the API, but a name cannot be uniquely matched to one Station. This can lead to ambiguity in the Connections search, where a most likely station is being chosen instead of the one the user wanted.
**Decision:** Stations are saved by ID and not by their name
**Alternatives:** Alternatively we could have checked the Station names each time the user types something, so that he can then select the correct station, if there is ambiguity. After the user chose a locaction we could have saved some id of our own, that is unique.
**Consequences:** Using the station names we need to consider the encoding of the string, so that it works in the request and is percent encoded. With an ID this problem doesnt exist.
