MembershipReboot.Postgres
-------------------------

A persistence layer for MembershipReboot that uses PostgreSQL to store the user accounts and groups.
Requires PostgreSQL 9.4 or later, as it uses the jsonb type to store the complete account.

This is an opinionated implementation, be free to use it if it fits you. Or if you feel you can improve things, send a PR :).

Work needed around more advanced features: linked accounts and certificates in particular, their performance is probably not very good (see code to understand why).
