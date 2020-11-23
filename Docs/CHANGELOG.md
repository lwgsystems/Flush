# Changelog

## v0.4

Refactored AutomaticLogoutProvider into an independent service.

Added moderator function.

Added minimum, maximum and mode vote display.

Added randomly assigned avatars for players.

General UX Improvements.

## v0.3

Added ASP.NET Core Identity 3 support. Flush configures ASP.NET Core Identity 3 with an encrypted sqlite backing store.

Added JWT Bearer based authentication.

Added support for session reconnection.

Moved to an in-memory SignalR user/group model. This is faster than the existing sqlite backing store, and identity now tracks much of what we were originally tracking.

### v0.3.1

Added XML documentation to all classes.

Fixed an issue involving scoped use of AuthenticationProvider in AutomaticLogoutProvider.

### v0.3.2

Add bootstrapping scripts.

Update Dockerfile to support bootstrapping scripts and new tooling.

### v0.3.3

Reinstate room autofill function.

### v0.3.4

Fixed an issue that could cause capitalisation to result in unique rooms.

Fixed an issue that could cause usernames to be claimed globally, rather than per room.

## v0.2

Improved disconnection handling.

Improved keep-alive/ttl.

## v0.1

Initial version.
